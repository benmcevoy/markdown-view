namespace MdView.Tests
{
    public class RouterTests
    {
        private readonly FileSystemInfoService _fileSystemService;
        private readonly Router _router;

        public RouterTests()
        {
            _fileSystemService = new("/home/agent/hello-world/sample", [".md"]);
            _router = new(_fileSystemService.Build());
        }

        [Fact]
        public void Map_Root_ReturnsIsFolder()
        {
            // arrange
            // act
            var route = _router.Map("/");

            // assert
            Assert.True(route is FolderInfo); // Root path points to sample directory (a folder)
            Assert.Equal("/home/agent/hello-world/sample", route.Path);
        }

        // Map_IndexMd_ReturnsIndexMdAbsolutePath
        [Fact]
        public void Map_IndexMd_ReturnsIndexMdAbsolutePath()
        {
            // arrange
            // act
            var route = _router.Map("/index.md");

            // assert
            Assert.Equal("/home/agent/hello-world/sample/index.md", route.Path);
            Assert.False(route is FolderInfo);
        }

        // Map_FileFd_ReturnsFileMd
        [Fact]
        public void Map_FileMd_ReturnsFileMd()
        {
            // arrange
            // act
            var route = _router.Map("/page1.md");

            // assert
            Assert.Equal("/home/agent/hello-world/sample/page1.md", route.Path);
            Assert.True(route is FileInfo);
        }

        // Map_FileMd_IsNotFolder
        [Fact]
        public void Map_FileMd_IsNotFolder()
        {
            // arrange
            // act
            var route = _router.Map("/page1.md");

            // assert
            Assert.False(route is FolderInfo);
        }

        // Map_PathTraversal_Throws
        [Fact]
        public void Map_PathTraversal_Throws()
        {
            // arrange
            // act
            var ex = Assert.Throws<NotSupportedException>(() => _router.Map("/../../../etc/passwd"));

            // assert
            Assert.Equal("forbidden", ex.Message);
        }

        // Map_QueryString_Throws
        [Fact]
        public void Map_QueryString_Throws()
        {
            // arrange
            // act
            var ex = Assert.Throws<NotSupportedException>(() => _router.Map("/index.md?foo=bar"));

            // assert
            Assert.Equal("forbidden", ex.Message);
        }

        // Map_UriFragment_Throws
        [Fact]
        public void Map_UriFragment_Throws()
        {
            // arrange
            // act
            var ex = Assert.Throws<NotSupportedException>(() => _router.Map("/index.md#section"));

            // assert
            Assert.Equal("forbidden", ex.Message);
        }

        // Map_UriEndcoded_Throws
        [Fact]
        public void Map_UriEndcoded_Throws()
        {
            // arrange
            // act
            var ex = Assert.Throws<NotSupportedException>(() => _router.Map("/index%20page.md"));

            // assert
            Assert.Equal("forbidden", ex.Message);
        }
    }
}
