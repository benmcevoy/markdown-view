namespace ragd.Tests.Service.Clean.Html;

using ragd.Service.Clean.Html;

public class RemoveLinesAndRulesCleanerTests
{
    [Fact]
    public void RemoveHtmlHrVariantsCleaner_Removes_html_hr_tags()
    {
        // arrange
        var sut = new RemoveHtmlHrVariantsCleaner();

        // act
        var actual = sut.Clean(@"self closing html tag
<hr>

self closing with space
<Hr />

self closing no space
<hr/>

and this <hr  > in some text");

        var expected = @"self closing html tag


self closing with space


self closing no space


and this  in some text";

        // assert
        Assert.Equal(expected, actual);
    }
}