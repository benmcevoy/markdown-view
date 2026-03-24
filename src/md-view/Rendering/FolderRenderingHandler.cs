using System.Text;

namespace MdView.Rendering
{
    public class FolderRenderingHandler() : IRenderingHandler
    {
        public bool CanHandle(FileSystemInfo route) =>
             route is FolderInfo f && !f.Children.Any(x => x.Name == "index.md");

        public string Handle(FileSystemInfo route)
        {
            var sb = new StringBuilder();
            var folder = route as FolderInfo;

            if (folder!.Children.Count == 0) return "";

            foreach (var f in folder.OrderedChildren())
            {
                var title = f is FolderInfo ? $"/{f.Name}" : $"{f.Name}";
                
                sb.AppendLine($"<div><a href='{f.Uri}'>{title}</a></div>");
            }

            return sb.ToString();
        }
    }
}