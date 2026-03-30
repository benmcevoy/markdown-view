using FileInfo = MdView.FileSystem.FileInfo;
using FileSystemInfo = MdView.FileSystem.FileSystemInfo;

namespace MdView.Rendering
{
    public class FaviconFileRendererHandler : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [".ico"];

        public bool CanHandle(FileSystemInfo route) => route is FileInfo && route.Name == "favicon.ico";

        public string Handle(FileSystemInfo input)
        {
            return @"<link rel='icon'
        href='data:image/x-icon;base64,AAABAAEAEBAQAAAAAAAoAQAAFgAAACgAAAAQAAAAIAAAAAEABAAAAAAAgAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAZlX/ADOA/wAzVf8AfwB/ADOq/wD//wAA/wD/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAZmYAAGZmAAAAEQAAEQAAAAA1AAAzAAAAABFmZiMAAAAAAHd3AAAAAAAAZmYAAAAAZgB3dwBmAAB3AGZmAHcAAAB3d3d3AAAAAAB3dwAAAAAAd3d3dwAAAHcAd3cAdwAAdwB3dwB3AAAAd0REdwAAAABEAABEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA'
        type='image/x-icon' />";
        }
    }
}