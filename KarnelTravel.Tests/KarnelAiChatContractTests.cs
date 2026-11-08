using System.Text.Json;
using System.IO;
using KarnelTravel.ViewModels;

namespace KarnelTravel.Tests;

public sealed class KarnelAiChatContractTests
{
    [Fact]
    public void Chat_request_defaults_to_text_mode()
    {
        var request = new KarnelAiChatRequest();

        Assert.Equal("text", request.InputMode);
    }

    [Theory]
    [InlineData("voice")]
    [InlineData("text")]
    public void Chat_mode_is_preserved_when_serialized(string inputMode)
    {
        var requestJson = JsonSerializer.Serialize(new KarnelAiChatRequest
        {
            Message = "Find a hotel",
            InputMode = inputMode,
            RequestId = "request-123"
        });
        var request = JsonSerializer.Deserialize<KarnelAiChatRequest>(requestJson);

        Assert.Equal(inputMode, request?.InputMode);
        Assert.Equal("request-123", request?.RequestId);

        var responseJson = JsonSerializer.Serialize(new KarnelAiChatResponse
        {
            Success = true,
            Reply = "Here are some hotels.",
            InputMode = inputMode,
            RequestId = "request-123"
        });
        var response = JsonSerializer.Deserialize<KarnelAiChatResponse>(responseJson);

        Assert.Equal(inputMode, response?.InputMode);
        Assert.Equal("request-123", response?.RequestId);
    }

    [Fact]
    public void Voice_script_uses_a_dedicated_turn_without_populating_text_input()
    {
        var path = FindWorkspaceFile("KarnelTravel", "wwwroot", "js", "karnel-ai-voice.js");
        var script = File.ReadAllText(path);

        Assert.Contains("submitVoiceTurn", script);
        Assert.Contains("inputMode: 'voice'", script);
        Assert.Contains("voiceConversationKey", script);
        Assert.DoesNotContain("input.value = (transcript + interim)", script);
        Assert.DoesNotContain("form.submit()", script);
    }

    private static string FindWorkspaceFile(params string[] parts)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(parts).ToArray());
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Workspace file was not found.", Path.Combine(parts));
    }
}
