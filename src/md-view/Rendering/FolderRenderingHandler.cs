using System.Text;

namespace MdView.Rendering
{
    public class FolderRenderingHandler() : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [];

        public bool CanHandle(FileSystemInfo route) => route is FolderInfo;

        public string Handle(FileSystemInfo route)
        {
            var sb = new StringBuilder();
            var folder = route as FolderInfo;

            if (folder!.Children.Count == 0) return "Nothing here...";

            foreach (var f in folder.OrderedChildren())
            {
                sb.AppendLine($"<div class='{f.FileSystemInfoType()}'><a href='{f.Uri}'> {f.Name}</a></div>");
            }

            return sb.ToString();
        }
    }
}