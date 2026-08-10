using ragd.Service.Handlers;

namespace ragd.Tests.Service.Handlers;

public class HelpRequestHandlerTest
{
    [Fact]
    public void HelpRequestHandler_CanHandle_GET_Help()
    {
        // arrange
        var sut = new HelpRequestHandler();
        var request = new Http.Request
        {
            Method = Http.HttpMethod.GET,
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
        var request = new Http.Request
        {
            Method = Http.HttpMethod.GET,
            Path = "/help"
        };

        // act
        var actual = sut.CanHandle(request);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public void HelpRequestHandler_Handle_returns_help_as_string()
    {
        // arrange
        var sut = new HelpRequestHandler();

        // act
        var actual = sut.Handle(new());

        // assert
        Assert.True(actual.Body is string);
        Assert.False(string.IsNullOrWhiteSpace(actual.Body as string));
    }
}