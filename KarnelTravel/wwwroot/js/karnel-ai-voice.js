(function () {
    'use strict';

    var root = document.getElementById('kai-voice');
    if (!root) { return; }

    var Recognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    var recognition = null;
    var isListening = false;
    var isSpeaking = false;
    var selectedVoice = null;
    var voices = [];
    var language = root.dataset.defaultLanguage || 'en-US';
    var speechQueue = [];
    var speechIndex = 0;
    var speechRun = 0;
    var latestRequestId = null;
    var voiceSessionActive = false;
    var voiceTurnPending = false;
    var recognitionRun = 0;
    var voiceConversationKey = sessionStorage.getItem('karnel-ai-conversation') || crypto.randomUUID();
    sessionStorage.setItem('karnel-ai-conversation', voiceConversationKey);

    var stage = root.querySelector('.kai-voice__stage');
    var stateLabel = document.getElementById('kai-voice-state');
    var help = document.getElementById('kai-voice-help');
    var status = document.getElementById('kai-voice-status');
    var microphone = document.getElementById('kai-voice-microphone');
    var stop = document.getElementById('kai-voice-stop');
    var end = document.getElementById('kai-voice-end');
    var pause = document.getElementById('kai-voice-pause');
    var resume = document.getElementById('kai-voice-resume');
    var enabled = document.getElementById('kai-voice-enabled');
    var languageSelect = document.getElementById('kai-voice-language');
    var voiceSelect = document.getElementById('kai-voice-voice');
    var input = document.getElementById('chat-message');
    var inputMode = document.getElementById('kai-input-mode');
    var form = document.getElementById('chat-form');
    var updatingRecognizedInput = false;

    function setState(next, message, detail) {
        stage.dataset.voiceState = next;
        stateLabel.textContent = message;
        if (detail) { help.textContent = detail; }
        root.classList.toggle('is-error', next === 'error');
        if (next !== 'speaking') {
            stop.hidden = true;
            pause.hidden = true;
            resume.hidden = true;
        }
        if (end) { end.hidden = !voiceSessionActive; }
    }

    function setStatus(message, isError) {
        status.textContent = message || '';
        status.classList.toggle('is-error', Boolean(isError));
    }

    function supportsRecognition() {
        return Boolean(Recognition);
    }

    function stopRecognition() {
        if (!recognition || !isListening) { return; }
        recognitionRun += 1;
        recognition.stop();
    }

    function beginRecognition() {
        if (!supportsRecognition()) {
            setState('error', 'Voice input is not available', 'Use the text box below, or try a supported browser with microphone access.');
            setStatus('This browser does not provide SpeechRecognition.', true);
            return;
        }
        if (isListening) {
            stopRecognition();
            return;
        }
        voiceSessionActive = true;
        voiceTurnPending = false;
        if (window.speechSynthesis) { window.speechSynthesis.cancel(); }

        recognition = new Recognition();
        recognition.continuous = false;
        recognition.interimResults = true;
        recognition.lang = language;
        recognition.maxAlternatives = 1;

        var transcript = '';
        var finalSubmitted = false;
        var currentRun = ++recognitionRun;
        recognition.onstart = function () {
            isListening = true;
            setState('listening', 'Listening…', 'Speak naturally. I will process each completed turn automatically.');
            setStatus('Microphone is active.');
            microphone.innerHTML = '<span aria-hidden="true">■</span> Stop listening';
            microphone.setAttribute('aria-label', 'Stop voice input');
        };
        recognition.onresult = function (event) {
            var interim = '';
            transcript = '';
            for (var index = event.resultIndex; index < event.results.length; index += 1) {
                var phrase = event.results[index][0].transcript;
                if (event.results[index].isFinal) { transcript += phrase; } else { interim += phrase; }
            }
            if (voiceSessionActive && interim) { setStatus('Listening: ' + interim.trim()); }
            if (voiceSessionActive && transcript.trim() && !finalSubmitted) {
                finalSubmitted = true;
                voiceTurnPending = true;
                stopRecognition();
                submitVoiceTurn(transcript.trim());
            }
        };
        recognition.onerror = function (event) {
            var messages = {
                'not-allowed': 'Microphone permission was denied. Allow access in your browser settings or continue with text.',
                'service-not-allowed': 'Speech recognition is not allowed by this browser or device.',
                'no-speech': 'No speech was detected. Try again when you are ready.',
                'audio-capture': 'No working microphone was found.',
                'network': 'The browser speech service is unavailable right now.'
            };
            if (event.error === 'no-speech' && voiceSessionActive) {
                setState('listening', 'Listening…', 'No speech detected. Speak when ready.');
            } else {
                voiceSessionActive = false;
                setState('error', 'Voice input needs attention', messages[event.error] || 'Voice input could not be completed.');
            }
            setStatus(messages[event.error] || 'Speech recognition failed.', true);
        };
        recognition.onend = function () {
            isListening = false;
            microphone.innerHTML = '<span aria-hidden="true">●</span> Start listening';
            microphone.setAttribute('aria-label', 'Start voice input');
            if (currentRun !== recognitionRun) { return; }
            if (voiceSessionActive && !voiceTurnPending && stage.dataset.voiceState === 'listening') {
                setTimeout(function () { if (voiceSessionActive && !isListening && !voiceTurnPending) { beginRecognition(); } }, 250);
            }
        };

        try {
            recognition.start();
        } catch (error) {
            isListening = false;
            microphone.disabled = false;
            setState('error', 'Could not start the microphone', 'If the microphone is already active, stop it and try again.');
            setStatus(error.name === 'NotAllowedError'
                ? 'Microphone permission was denied. Allow microphone access and try again.'
                : 'Voice input could not be started. Try again.', true);
        }

    async function submitVoiceTurn(message) {
        var requestId = crypto.randomUUID();
        latestRequestId = requestId;
        setState('processing', 'Processing…', 'Karnel AI is preparing a spoken answer.');
        setStatus('Understanding: ' + message);
        try {
            var token = document.querySelector('input[name="__RequestVerificationToken"]');
            var response = await fetch('/KarnelAI/Chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token ? token.value : '' },
                body: JSON.stringify({ message: message, conversationKey: voiceConversationKey, inputMode: 'voice', requestId: requestId })
            });
            var data = await response.json();
            if (!response.ok || !data.success || data.requestId !== requestId) { throw new Error(data.error || 'The assistant could not respond.'); }
            voiceTurnPending = false;
            if (typeof window.karnelAiSpeak === 'function') { window.karnelAiSpeak(data.reply); }
            else { speak(data.reply); }
        } catch (error) {
            voiceTurnPending = false;
            voiceSessionActive = false;
            setState('error', 'Voice conversation failed', 'You can try again or use Text Chat.');
            setStatus(error.message || 'The assistant could not respond.', true);
        }
    }
    }

    function refreshVoices() {
        if (!window.speechSynthesis || !voiceSelect) { return; }
        voices = window.speechSynthesis.getVoices();
        voiceSelect.innerHTML = '';
        var defaultOption = document.createElement('option');
        defaultOption.value = '';
        defaultOption.textContent = 'System default voice';
        voiceSelect.appendChild(defaultOption);
        voices.forEach(function (voice, index) {
            var option = document.createElement('option');
            option.value = String(index);
            option.textContent = voice.name + (voice.lang ? ' · ' + voice.lang : '');
            voiceSelect.appendChild(option);
        });
        voiceSelect.hidden = voices.length === 0;
    }

    function splitSpeech(text) {
        var normalized = text.replace(/\s+/g, ' ').trim();
        if (normalized.length <= 220) { return normalized ? [normalized] : []; }

        var chunks = [];
        var remaining = normalized;
        while (remaining.length > 220) {
            var boundary = remaining.lastIndexOf(' ', 220);
            if (boundary < 80) { boundary = 220; }
            chunks.push(remaining.slice(0, boundary).trim());
            remaining = remaining.slice(boundary).trim();
        }
        if (remaining) { chunks.push(remaining); }
        return chunks;
    }

    function resetSpeechControls() {
        isSpeaking = false;
        stop.hidden = true;
        pause.hidden = true;
        resume.hidden = true;
    }

    function cancelSpeech(restoreIdle) {
        speechRun += 1;
        speechQueue = [];
        speechIndex = 0;
        if (window.speechSynthesis) { window.speechSynthesis.cancel(); }
        resetSpeechControls();
        if (restoreIdle) {
            setState('idle', 'Ready when you are', 'Use the microphone to dictate another question.');
        }
    }

    function speakNext(runId) {
        if (runId !== speechRun || speechIndex >= speechQueue.length) {
            if (runId === speechRun) {
                resetSpeechControls();
                if (voiceSessionActive) {
                    setState('listening', 'Listening…', 'Speak your next question.');
                    beginRecognition();
                } else {
                    setState('idle', 'Ready when you are', 'Start a voice conversation when you are ready.');
                }
            }

    window.karnelAiSpeak = speak;
            return;
        }

        var utterance = new SpeechSynthesisUtterance(speechQueue[speechIndex]);
        utterance.lang = language;
        if (selectedVoice) { utterance.voice = selectedVoice; }
        utterance.rate = 0.96;
        utterance.pitch = 1;
        utterance.onstart = function () {
            if (runId !== speechRun) { return; }
            isSpeaking = true;
            setState('speaking', 'Karnel AI is speaking', 'You can pause or stop the spoken response at any time.');
            stop.hidden = false;
            pause.hidden = false;
            pause.textContent = 'Pause';
        };
        utterance.onpause = function () {
            if (runId !== speechRun) { return; }
            pause.hidden = true;
            resume.hidden = false;
        };
        utterance.onresume = function () {
            if (runId !== speechRun) { return; }
            pause.hidden = false;
            resume.hidden = true;
        };
        utterance.onerror = function () {
            if (runId !== speechRun) { return; }
            cancelSpeech(false);
            setState('error', 'Speech output was unavailable', 'The written response is still available in the conversation.');
            setStatus('Your browser could not play the spoken response.', true);
        };
        utterance.onend = function () {
            if (runId !== speechRun) { return; }
            speechIndex += 1;
            speakNext(runId);
        };
        window.speechSynthesis.speak(utterance);
    }

    function speak(text) {
        if (!text) { return; }
        if (!window.speechSynthesis || !window.SpeechSynthesisUtterance) {
            setState('error', 'Voice replies are unavailable', 'The written response is still available in the conversation.');
            setStatus('This browser does not provide SpeechSynthesis.', true);
            return;
        }
        cancelSpeech(false);
        speechQueue = splitSpeech(text);
        speechIndex = 0;
        var runId = speechRun;
        speakNext(runId);
    }

    enabled.checked = localStorage.getItem('karnel-ai-voice-enabled') !== 'false';
    languageSelect.value = localStorage.getItem('karnel-ai-voice-language') || language;
    language = languageSelect.value;
    refreshVoices();
    if (window.speechSynthesis) { window.speechSynthesis.addEventListener('voiceschanged', refreshVoices); }

    if (!supportsRecognition()) {
        microphone.disabled = true;
        setState('error', 'Voice input is not available', 'Text chat remains available. Try a supported browser to use the microphone.');
        setStatus('SpeechRecognition is not supported in this browser.', true);
    }
    if (!window.speechSynthesis || !window.SpeechSynthesisUtterance) {
        enabled.disabled = true;
        setStatus('Spoken replies are not supported in this browser. Text chat remains available.', true);
    }

    microphone.addEventListener('click', function () {
        if (voiceSessionActive) {
            voiceSessionActive = false;
            voiceTurnPending = false;
            stopRecognition();
            cancelSpeech(false);
            setState('idle', 'Voice conversation ended', 'Start a new voice conversation when you are ready.');
            return;
        }
        beginRecognition();
    });
    input.addEventListener('input', function () {
        if (!updatingRecognizedInput && inputMode) { inputMode.value = 'text'; }
    });
    stop.addEventListener('click', function () {
        cancelSpeech(false);
        if (voiceSessionActive) { beginRecognition(); }
    });
    if (end) { end.addEventListener('click', function () {
        voiceSessionActive = false;
        voiceTurnPending = false;
        stopRecognition();
        cancelSpeech(false);
        setState('idle', 'Voice conversation ended', 'Start a new voice conversation when you are ready.');
        setStatus('');
    }); }
    pause.addEventListener('click', function () { if (window.speechSynthesis) { window.speechSynthesis.pause(); } });
    resume.addEventListener('click', function () { if (window.speechSynthesis) { window.speechSynthesis.resume(); } });
    enabled.addEventListener('change', function () {
        localStorage.setItem('karnel-ai-voice-enabled', String(enabled.checked));
        if (!enabled.checked && window.speechSynthesis) {
            cancelSpeech(false);
            setState('idle', 'Current voice reply stopped', 'Voice-originated responses remain automatic when they arrive.');
        }
    });
    languageSelect.addEventListener('change', function () {
        language = languageSelect.value;
        localStorage.setItem('karnel-ai-voice-language', language);
    });
    voiceSelect.addEventListener('change', function () {
        selectedVoice = voices[Number(voiceSelect.value)] || null;
    });
    form.addEventListener('submit', stopRecognition, true);
    document.addEventListener('karnel-ai:question', function (event) {
        latestRequestId = event.detail && event.detail.requestId ? event.detail.requestId : null;
        cancelSpeech(false);
        setState('thinking', 'Karnel AI is thinking', 'The existing travel intelligence is checking published Karnel records.');
    });
    document.addEventListener('karnel-ai:response', function (event) {
        if (!event.detail || event.detail.requestId !== latestRequestId) { return; }
        if (event.detail.inputMode === 'voice') { speak(event.detail.text); }
    });
    document.addEventListener('karnel-ai:error', function (event) {
        setState('error', 'Karnel AI needs another try', event.detail.message || 'The written assistant response could not be completed.');
    });
})();
