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

    var stage = root.querySelector('.kai-voice__stage');
    var stateLabel = document.getElementById('kai-voice-state');
    var help = document.getElementById('kai-voice-help');
    var status = document.getElementById('kai-voice-status');
    var microphone = document.getElementById('kai-voice-microphone');
    var stop = document.getElementById('kai-voice-stop');
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
        if (window.speechSynthesis) { window.speechSynthesis.cancel(); }

        recognition = new Recognition();
        recognition.continuous = false;
        recognition.interimResults = true;
        recognition.lang = language;
        recognition.maxAlternatives = 1;

        var transcript = '';
        recognition.onstart = function () {
            isListening = true;
            setState('listening', 'Listening…', 'Speak naturally. Your words will appear in the question box for review.');
            setStatus('Microphone is active. Click again to stop listening.');
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
            input.value = (transcript + interim).trim();
            updatingRecognizedInput = true;
            input.dispatchEvent(new Event('input', { bubbles: true }));
            updatingRecognizedInput = false;
            if (inputMode) { inputMode.value = 'voice'; }
        };
        recognition.onerror = function (event) {
            var messages = {
                'not-allowed': 'Microphone permission was denied. Allow access in your browser settings or continue with text.',
                'service-not-allowed': 'Speech recognition is not allowed by this browser or device.',
                'no-speech': 'No speech was detected. Try again when you are ready.',
                'audio-capture': 'No working microphone was found.',
                'network': 'The browser speech service is unavailable right now.'
            };
            setState('error', 'Voice input needs attention', messages[event.error] || 'Voice input could not be completed.');
            setStatus(messages[event.error] || 'Speech recognition failed.', true);
        };
        recognition.onend = function () {
            isListening = false;
            microphone.innerHTML = '<span aria-hidden="true">●</span> Start listening';
            microphone.setAttribute('aria-label', 'Start voice input');
            if (stage.dataset.voiceState === 'listening') {
                setState('idle', transcript.trim() ? 'Question ready to review' : 'Ready when you are', transcript.trim() ? 'Review or edit the recognized question, then press Ask.' : 'Use the microphone to dictate a question.');
                setStatus(transcript.trim() ? 'Your recognized question is ready in the text box.' : '');
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
                setState('idle', 'Ready when you are', 'Use the microphone to dictate another question.');
            }
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

    microphone.addEventListener('click', beginRecognition);
    input.addEventListener('input', function () {
        if (!updatingRecognizedInput && inputMode) { inputMode.value = 'text'; }
    });
    stop.addEventListener('click', function () {
        cancelSpeech(true);
    });
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
