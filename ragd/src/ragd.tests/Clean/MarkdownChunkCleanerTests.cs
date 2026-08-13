namespace ragd.Tests.Clean;

using ragd.Clean;

public class MarkdownChunkCleanerTests
{
    [Fact]
    public void MarkdownCleaner_condense_white_space()
    {
        // arrange 
        var sut = new MarkdownChunkCleaner();
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

    [Fact]
    public void MarkdownCleaner_condense_blank_lines()
    {
        // arrange 
        var sut = new MarkdownChunkCleaner();
        var expected = @"some
bl
ank
ank
lines";
        // act 
        var actual = sut.Clean(@" 
        



some
      bl
  ank
  
ank


lines



");
        // assert 
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MarkdownChunkCleaner_Removes_lines_and_variants()
    {
        // arrange
        var sut = new MarkdownChunkCleaner();

        // act
        var actual = sut.Clean(@"

3 hyphens with blank line above and below

---

3 asterix with blank line above and below

***

3 underscores with blank line above and below
___


2 hyphens are legit

--

hyphens with spaces with blank line above and below

- - -

many hyphens with blank line above and below

------------ -

many space separated asterix with blank line above and below

* * * * * * * 

3 characters with blank line above and below

@@@
       
        ");

        var expected = @"3 hyphens with blank line above and below
3 asterix with blank line above and below
3 underscores with blank line above and below
2 hyphens are legit
--
hyphens with spaces with blank line above and below
many hyphens with blank line above and below
many space separated asterix with blank line above and below
3 characters with blank line above and below
@@@";

        // assert
        Assert.Equal(expected, actual);
    }

    //[Fact]
    // public void MarkdownCleaner_remove_urls_from_links()
    // {
    //     // arrange 
    //     // act 
    //     // assert 
    //     Assert.Fail("TODO");
    // }

    // //[Fact]
    // public void MarkdownCleaner_remove_frontmatter()
    // {
    //     // arrange 
    //     // act 
    //     // assert 
    //     Assert.Fail("TODO");
    // }

    // //[Fact]
    // public void MarkdownCleaner_remove_formatting_bold_italic_inlinecode()
    // {
    //     // arrange 
    //     // act 
    //     // assert 
    //     Assert.Fail("TODO");
    // }

    // //[Fact]
    // public void MarkdownCleaner_convert_tables_to_CSV()
    // {
    //     // arrange 
    //     // act 
    //     // assert 
    //     Assert.Fail("TODO");
    // }
}
