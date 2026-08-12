namespace ragd.Tests.Service.Clean.Text;

using ragd.Service.Clean.Text;

public class CondenseWhitespaceCleanerTests
{
    [Fact]
    public void CondenseWhiteSpaceCleaner_Clean()
    {
        // arrange 
        var sut = new CondenseWhiteSpaceCleaner();
        var expected = @"This is a line tab with spaces
and
some blank lines.";

        // act 
        var actual = sut.Clean(@"This is a line     tab    with spaces

        and



        some blank lines.
        
        ");

        // assert 
        Assert.Equal(expected, actual);
    }
  }
