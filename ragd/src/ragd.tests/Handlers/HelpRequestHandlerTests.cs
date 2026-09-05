using ragd.Handlers;
using http;

namespace ragd.Tests.Handlers;

public class HelpRequestHandlerTest
{
    [Fact]
    public void HelpRequestHandler_CanHandle_GET_Help()
    {
        // arrange
        var sut = new HelpRequestHandler();
        var request = new Request
        {
            Method = http.HttpMethod.GET,
            Path = "Help"
        };

        // act
        var actual = sut.CanHandle(request);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public void HelpRequestHandler_CannotHandle_GET_SlashHelp()
    {
        // arrange
        var sut = new HelpRequestHandler();
        var request = new Request
        {
            Method = http.HttpMethod.GET,
            Path = "/help"
        };

        // act
        var actual = sut.CanHandle(request);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public void HelpRequestHandler_Handle_returns_help_as_stream()
    {
        // arrange
        var sut = new HelpRequestHandler();

        // act
        var actual = sut.Handle(new());

        // assert
        Assert.NotNull(actual.Body);
        Assert.True(actual.Body.Length > 0);
    }
}