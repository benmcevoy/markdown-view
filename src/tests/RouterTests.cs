using System;
using System.IO;
using Xunit;

namespace MdView.Tests
{
    public class RouterTests
    {
        private readonly Router _router = new("/home/agent/hello-world/sample");

        [Fact]
        public void Map_Root_ReturnsIsFolder()
        {
            // arrange
            // act
            var route = _router.Map("/");

            // assert
            Assert.Equal("/", route.RequestPath);
            Assert.True(route.IsFolder); // Root path points to sample directory (a folder)
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
            Assert.Equal("/index.md", route.RequestPath);
            Assert.Equal("/home/agent/hello-world/sample/index.md", route.Path);
            Assert.False(route.IsFolder);
        }

        // Map_StaticAssetCss_ReturnsCss
        [Fact]
        public void Map_StaticAssetCss_ReturnsCss()
        {
            // arrange
            // act
            var route = _router.Map("/css/style.css");

            // assert
            Assert.Equal("/css/style.css", route.RequestPath);
            Assert.Equal("wwwroot/css/style.css", route.Path);
            Assert.True(route.IsStaticAsset);
        }

        // Map_StaticAssetJs_ReturnsJs
        [Fact]
        public void Map_StaticAssetJs_ReturnsJs()
        {
            // arrange
            // act
            var route = _router.Map("/js/toggle.js");

            // assert
            Assert.Equal("/js/toggle.js", route.RequestPath);
            Assert.Equal("wwwroot/js/toggle.js", route.Path);
            Assert.True(route.IsStaticAsset);
        }

        // Map_StaticAssetJpg_IsNotSupported
        [Fact]
        public void Map_StaticAssetJpg_IsNotSupported()
        {
            // arrange
            // act
            var route = _router.Map("/images/test.jpg");

            // assert
            Assert.Equal("/images/test.jpg", route.RequestPath);
            Assert.Equal("/home/agent/hello-world/sample/images/test.jpg", route.Path);
            Assert.False(route.IsStaticAsset); // jpg is NOT a static asset (only css/js are)
        }

        // Map_FileFd_ReturnsFileMd
        [Fact]
        public void Map_FileMd_ReturnsFileMd()
        {
            // arrange
            // act
            var route = _router.Map("/page1.md");

            // assert
            Assert.Equal("/page1.md", route.RequestPath);
            Assert.Equal("/home/agent/hello-world/sample/page1.md", route.Path);
            Assert.False(route.IsFolder);
            Assert.False(route.IsStaticAsset);
        }

        // Map_StaticAsset_IsStaticAsset
        [Fact]
        public void Map_StaticAsset_IsStaticAsset()
        {
            // arrange
            // act
            var route = _router.Map("/css/style.css");

            // assert
            Assert.True(route.IsStaticAsset);
        }

        // Map_FileMd_IsNotStaticAsset
        [Fact]
        public void Map_FileMd_IsNotStaticAsset()
        {
            // arrange
            // act
            var route = _router.Map("/page1.md");

            // assert
            Assert.False(route.IsStaticAsset);
        }

        // Map_FileMd_IsNotFolder
        [Fact]
        public void Map_FileMd_IsNotFolder()
        {
            // arrange
            // act
            var route = _router.Map("/page1.md");

            // assert
            Assert.False(route.IsFolder);
        }

        // Map_Folder_IsNotStaticAsset
        [Fact]
        public void Map_Folder_IsNotStaticAsset()
        {
            // arrange
            // act
            var route = _router.Map("/topic/");

            // assert
            Assert.False(route.IsStaticAsset);
            Assert.True(route.IsFolder);
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
