namespace wikd.Tests;

public class SearchServiceTests
{
    //[Fact]
    public void SearchService_IsSearchAvailable()
    {
        // arrange
        // TODO: ragd should be running first.  this is not a unit test.
        var scope = EnvironmentVariableTarget.Process;
        var path = Environment.GetEnvironmentVariable("PATH", scope);
        Environment.SetEnvironmentVariable("PATH", "../../../../../../ragd/src/ragd/bin/Debug/net10.0:" + path, scope);

        // act
        var sut = new SearchService("");

        // assert
        Assert.True(sut.IsSearchAvailable());

        var result = sut.Search("how to autocapitalize text?");
    }
}