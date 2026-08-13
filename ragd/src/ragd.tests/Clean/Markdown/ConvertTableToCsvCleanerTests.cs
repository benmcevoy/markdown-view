namespace ragd.Tests.Clean.Text;

using ragd.Clean.Markdown;

public class ConvertTableToCsvCleanerTests
{
    //[Fact("TODO")]
    public void ConvertTableToCsvCleaner_Convert()
    {
        // arrange 
        var sut = new ConvertTableToCsvCleaner();
        var expected = @"This is some text
        
col1,col2,col3
r1c1,r1c2,r1c3
r2c1,r2c2,r2c3


b_col1,b_col1
b_r1c1,b_r1c2
b_r2c1,b_r2c2
        
        ";

        // act 
        var actual = sut.Clean(@"This is some text
        
col1|col2|col3
----|----|----
r1c1|r1c2|r1c3
r2c1|r2c2|r2c3


|b_col1|b_col1|
|:----|:----:|
|b_r1c1|**b_r1c2**|
|b_r2c1|b_r2c2|


oh and other styles


+-----------+-----------+
| Header 1  | Header 2  |
+===========+===========+
| Cell 1    | Cell 2    |
+-----------+-----------+
| Cell 3    | Cell 4    |
+-----------+-----------+



+-----------+-------------------+
| Name      | Description       |
+===========+===================+
| Markdig   | A fast, powerful  |
|           | Markdown parser.  |
+-----------+-------------------+
| cmark     | The C reference   |
|           | implementation.   |
+-----------+-------------------+

+-------+-------+
| A     | B     |
+=======+=======+
| Cell spanning |
+-------+-------+

|no header| here|
|no header| here|
|no header| here|


        ");

        // assert 
        Assert.Equal(expected, actual);
    }
  }
