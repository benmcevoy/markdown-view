using ragd.Chunk;
namespace ragd.Tests.Chunk;

public class ContextSizeAdaptiveDocumentChunkerTests
{
    [Fact]
    public void ContextSizeAdaptiveDocumentChunker_plaintext_chunks_to_paragraphs()
    {
        // arrange
        // a context size of 200 equates to 544 characters, so all these paragraphs wshould fit fine
        var sut = new ContextSizeAdaptiveDocumentChunker(new FakeDocumentChunker(ChunkType.PlainText, 
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In lacus diam, facilisis ac condimentum a, egestas sed lorem. Sed et nisl libero. 
Donec vitae est a metus consequat aliquet nec a metus. Quisque viverra ut lorem vitae sollicitudin. 
            
Pellentesque rutrum finibus turpis, tristique hendrerit elit porta vel; Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos! Fusce at accumsan purus. Mauris in purus ac nulla fermentum lobortis   ? 
Curabitur cursus et nisi vitae euismod. Proin urna orci, luctus eu mollis eget, semper vitae velit. 
Donec pharetra nulla quis orci lacinia pharetra. Pellentesque ornare nisi eu odio suscipit, ac mattis augue egestas. Duis sed purus metus.

Donec vitae orci at sapien lacinia tempor quis at orci!? Cras sem elit, lacinia ac semper sit amet, pretium sagittis dolor. 
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nulla sit amet efficitur justo. 

Fusce sit amet euismod mi, ac pellentesque lacus. Suspendisse potenti. Quisque a dolor faucibus, condimentum felis vitae, pretium diam. 
Ut et feugiat mi, non bibendum nunc. Curabitur id porttitor urna. In nunc felis, ultricies quis condimentum vitae, laoreet vel ante. 
Donec ut feugiat augue. Suspendisse iaculis eros libero, ac efficitur leo porttitor eu. So there"), 200);

        // act
        var actual = sut.Handle(null!).ToList();

        // assert
        var expectedNumberOfChunks = 4;
        Assert.Equal(544, sut.CharacterLimit);
        Assert.Equal(expectedNumberOfChunks, actual.Count);
    }

    [Fact]
    public void ContextSizeAdaptiveDocumentChunker_plaintext_chunks_to_fit_contextsize()
    {
        // arrange
        // a context size of 200 equates to 272 characters, so some paragraphs will be broken to sentances
        var sut = new ContextSizeAdaptiveDocumentChunker(new FakeDocumentChunker(ChunkType.PlainText, 
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In lacus diam, facilisis ac condimentum a, egestas sed lorem. Sed et nisl libero. 
Donec vitae est a metus consequat aliquet nec a metus. Quisque viverra ut lorem vitae sollicitudin. 
            
Pellentesque rutrum finibus turpis, tristique hendrerit elit porta vel; Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos! Fusce at accumsan purus. Mauris in purus ac nulla fermentum lobortis   ? 
Curabitur cursus et nisi vitae euismod. Proin urna orci, luctus eu mollis eget, semper vitae velit. 
Donec pharetra nulla quis orci lacinia pharetra. Pellentesque ornare nisi eu odio suscipit, ac mattis augue egestas. Duis sed purus metus.

Donec vitae orci at sapien lacinia tempor quis at orci!? Cras sem elit, lacinia ac semper sit amet, pretium sagittis dolor. 
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nulla sit amet efficitur justo. 

Fusce sit amet euismod mi, ac pellentesque lacus. Suspendisse potenti. Quisque a dolor faucibus, condimentum felis vitae, pretium diam. 
Ut et feugiat mi, non bibendum nunc. Curabitur id porttitor urna. In nunc felis, ultricies quis condimentum vitae, laoreet vel ante. 
Donec ut feugiat augue. Suspendisse iaculis eros libero, ac efficitur leo porttitor eu. So there"), 100);

        // act
        var actual = sut.Handle(null!).ToList();

        // assert
        var expectedNumberOfChunks = 6;
        Assert.Equal(272, sut.CharacterLimit);
        Assert.Equal(expectedNumberOfChunks, actual.Count);
        // count the characters in each paragraph to figure this out.
        Assert.Equal(
            @"Curabitur cursus et nisi vitae euismod. Proin urna orci, luctus eu mollis eget, semper vitae velit. Donec pharetra nulla quis orci lacinia pharetra. Pellentesque ornare nisi eu odio suscipit, ac mattis augue egestas. Duis sed purus metus.", 
            actual[2].Content);
    }

    [Fact]
    public void ContextSizeAdaptiveDocumentChunker_plaintext_inner_chunks_are_independant()
    {
        // arrange
        // a context size of 200 equates to 544 characters, (200 * 0.8 * 3.4)
        // both of these paragraphs could be combined to fit in a single chunk
        // but to preserve the original semantic boundary
        // they should be seperate
        var sut = new ContextSizeAdaptiveDocumentChunker(new FakeDocumentChunker(ChunkType.PlainText, 
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In lacus diam, facilisis ac condimentum a, egestas sed lorem. Sed et nisl libero. 
Donec vitae est a metus consequat aliquet nec a metus. Quisque viverra ut lorem vitae sollicitudin.",

@"Pellentesque rutrum finibus turpis, tristique hendrerit elit porta vel; 
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos! Fusce at accumsan purus. 
Mauris in purus ac nulla fermentum lobortis?"
), 200);

        // act
        var actual = sut.Handle(null!).ToList();

        // assert
        var expectedNumberOfChunks = 2;
        Assert.Equal(544, sut.CharacterLimit);
        Assert.Equal(expectedNumberOfChunks, actual.Count);
    }

    [Fact]
    public void ContextSizeAdaptiveDocumentChunker_markdown_chunks_to_paragraphs()
    {
        // arrange
        // a context size of 150 equates to 408 characters, so all the second paragraphs should break down to sentances
        var sut = new ContextSizeAdaptiveDocumentChunker(new FakeDocumentChunker(ChunkType.Markdown, 
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In lacus diam, facilisis ac condimentum a, egestas sed lorem. Sed et nisl libero. 
Donec vitae est a metus consequat aliquet nec a metus. Quisque viverra ut lorem vitae sollicitudin. 

## a heading is not a paragraph

Pellentesque rutrum finibus turpis, tristique hendrerit elit porta vel; Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos! Fusce at accumsan purus. Mauris in purus ac nulla fermentum lobortis   ? 
Curabitur cursus et nisi vitae euismod. Proin urna orci, luctus eu mollis eget, semper vitae velit. 
Donec pharetra nulla quis orci lacinia pharetra. Pellentesque ornare nisi eu odio suscipit, ac mattis augue egestas. Duis sed purus metus.

### this heading is part of the following paragraph

Donec vitae orci at sapien lacinia tempor quis at orci!? Cras sem elit, lacinia ac semper sit amet, pretium sagittis dolor. 
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nulla sit amet efficitur justo."), 150);

        // act
        var actual = sut.Handle(null!).ToList();

        // assert
        var expectedNumberOfChunks = 4;
        Assert.Equal(408, sut.CharacterLimit);
        Assert.Equal(expectedNumberOfChunks, actual.Count);
        // second paragrah is split to keep the heading as part of the content. 
        Assert.Equal(
@"## a heading is not a paragraph

Pellentesque rutrum finibus turpis, tristique hendrerit elit porta vel; Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos! Fusce at accumsan purus. Mauris in purus ac nulla fermentum lobortis   ? Curabitur cursus et nisi vitae euismod. Proin urna orci, luctus eu mollis eget, semper vitae velit.", 
            actual[1].Content);
        //  after 408 char limit the remainder of the paragraph appears as next chunk
        Assert.Equal(
"Donec pharetra nulla quis orci lacinia pharetra. Pellentesque ornare nisi eu odio suscipit, ac mattis augue egestas. Duis sed purus metus.",
            actual[2].Content);
    }
}
