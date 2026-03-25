using System.Text;

namespace MdView.Rendering
{
    public class Navigation(FileSystemInfoService fileSystemService)
    {
        private readonly FolderInfo _root = fileSystemService.FileSystem;

        public string Render(FileSystemInfo current)
        {
            return @$"
<nav>
    {Render(_root, current)}
</nav>";
        }

        private static string Render(FolderInfo root, FileSystemInfo current)
        {
            var sb = new StringBuilder("<ul>");

            foreach (var x in root.OrderedChildren())
            {
                sb.AppendLine(@$"<li class='{FileSystemTypeClass(x)}'>
                                    <a class='{CurrentClass(x, current)}' href='{x.Uri}'>{x.Name}</a>");

                if (x is FolderInfo f && f.Children.Count > 0)
                {
                    sb.AppendLine(Render(f, current));
                }

                sb.AppendLine("</li>");
            }

            sb.Append("</ul>");

            return sb.ToString();
        }

        private static string CurrentClass(FileSystemInfo candidate, FileSystemInfo current) =>
             candidate.Path.Equals(current.Path, StringComparison.Ordinal) ? "current" : "";

        private static string FileSystemTypeClass(FileSystemInfo candidate) =>
            (candidate is FolderInfo) ? "folder" : "file";
    }
}