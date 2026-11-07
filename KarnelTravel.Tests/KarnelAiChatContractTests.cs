using System.Text.Json;
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
}
