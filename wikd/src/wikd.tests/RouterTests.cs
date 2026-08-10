using wikd.Routing;

namespace wikd.Tests
{
    public class RouterTests
    {
        private const string RootPath = "../../../../../../sample";
        private readonly FileSystemRouter _fileSystemService;
        private readonly Router _router;

        public RouterTests()
        {
            _fileSystemService = new(RootPath, [".md"]);
            _router = new(new Http.Parser(), _fileSystemService);
        }

        private static Stream AsGET(string url)
        {
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes($"GET {url} HTTP/1.1"));
        }

        [Fact]
        public void Map_Root_ReturnsIsFolder()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/"));

            // assert
            Assert.True(route is FolderRoute); // Root path points to sample directory (a folder)
            Assert.Equal(RootPath, route.Path);
        }

        // Map_IndexMd_ReturnsIndexMdAbsolutePath
        [Fact]
        public void Map_IndexMd_ReturnsIndexMdAbsolutePath()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/index.md"));

            // assert
            Assert.Equal($"{RootPath}/index.md", route.Path);
            Assert.False(route is FolderRoute);
        }

        // Map_FileFd_ReturnsFileMd
        [Fact]
        public void Map_FileMd_ReturnsFileMd()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/page1.md"));

            // assert
            Assert.Equal($"{RootPath}/page1.md", route.Path);
            Assert.True(route is FileRoute);
        }

        // Map_FileMd_IsNotFolder
        [Fact]
        public void Map_FileMd_IsNotFolder()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/page1.md"));

            // assert
            Assert.False(route is FolderRoute);
        }

        // Map_PathTraversal_Throws
        [Fact]
        public void Map_PathTraversal_Returns401()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/../../../etc/passwd"));

            // assert
            Assert.True(route.RouteType() == "special");
            Assert.True(route.Name == "401");
        }

        // Map_QueryString_Throws
        [Fact]
        public void Map_QueryString_CleanUri()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/index.md?foo=bar"));

            // assert
            Assert.True(route.RouteType() == "file");
            Assert.True(route.Uri == "/index.md");
        }

        // Map_UriFragment_Throws
        [Fact]
        public void Map_UriFragment_CleanUri()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/index.md#section"));

            // assert
            Assert.True(route.RouteType() == "file");
            Assert.True(route.Uri == "/index.md");
        }

        // Map_UriEndcoded_Throws
        [Fact]
        public void Map_UriEndcoded_Returns404()
        {
            // arrange
            // act
            var route = _router.Map(AsGET("/index%20page.md"));

            // assert
            Assert.True(route.RouteType() == "special");
            Assert.True(route.Name == "404");
        }
    }
}
