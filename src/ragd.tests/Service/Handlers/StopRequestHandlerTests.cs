using ragd.Http;
using ragd.Service.Handlers;

namespace ragd.Tests.Service.Handlers;

public class StopRequestHandlerTests
{
    [Fact]
    public void StopRequestHandler_CanHandle_only_POST_stop()
    {
        // arrange
        var sut = new StopRequestHandler();
        var postStopRequest = new Http.Request
        {
            Method = "POST",
            Path = "stop"
        };
        var otherRequest = new Request
        {
            Method = "GET",
            Path = "Stahp!"
        };

        // act
        var actual = sut.CanHandle(postStopRequest);

        // assert
        Assert.True(actual);

        // act
        var actual1 = sut.CanHandle(otherRequest);

        // assert
        Assert.False(actual1);
    }

    [Fact]
    public void StopRequestHandler_Handle_returns_status_stopped()
    {
        // arrange
        var sut = new StopRequestHandler();
        var postStopRequest = new Http.Request
        {
            Method = "POST",
            Path = "stop"
        };

        // act
        var actual = sut.Handle(postStopRequest);

        // assert
        Assert.Equal(LifeCycleStates.STOPPED, actual.Status);
    }

    [Fact]
    public void StopRequestHandler_IsStopRequest()
    {
        // arrange
        var postStopRequest = new Http.Request
        {
            Method = "POST",
            Path = "stop"
        };
        var otherRequest = new Request
        {
            Method = "GET",
            Path = "Stop!"
        };

        // act
        // assert
        Assert.True(StopRequestHandler.IsStopRequest(postStopRequest));
        Assert.False(StopRequestHandler.IsStopRequest(otherRequest));
    }
}