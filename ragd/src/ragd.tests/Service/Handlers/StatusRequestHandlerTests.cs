using ragd.Http;
using ragd.Service.Handlers;

namespace ragd.Tests.Service.Handlers;

public class StatusRequestHandlerTests
{
    [Fact]
    public void StatusRequestHandler_CanHandle_only_GET_status()
    {
        // arrange
        var sut = new StatusRequestHandler(new());
        var getStatusRequest = new Request
        {
            Method = ragd.Http.HttpMethod.GET,
            Path = "status"
        };
        var otherRequest = new Request
        {
            Method = ragd.Http.HttpMethod.UNSUPPORTED,
            Path = "state"
        };

        // act
        var actual = sut.CanHandle(getStatusRequest);

        // assert
        Assert.True(actual);

        // act
        var actual1 = sut.CanHandle(otherRequest);

        // assert
        Assert.False(actual1);
    }

    [Fact]
    public void StatusRequestHandler_Handle_returns_status()
    {
        // arrange
        var config = new Config();
        var sut = new StatusRequestHandler(config);
        var getStatusRequest = new Request
        {
            Method = ragd.Http.HttpMethod.GET,
            Path = "status"
        };

        // act
        var actual = sut.Handle(getStatusRequest);
        var body = actual.BodyAs<Dictionary<string, string>>();

        // assert
        Assert.Equal(LifeCycleStates.RUNNING, actual.Status);
        Assert.Equal(config.ModelPath, body["model"]);
    }
}