using System.Text.RegularExpressions;
using MdView.Models;

namespace MdView
{
    /// <summary>
    /// Scanner class that recursively scans directories for markdown files,
    /// builds a nested navigation structure, and parses YAML frontmatter.
    /// </summary>
    public class Scanner(string rootPath)
    {
        public readonly string _rootPath = rootPath;
        public readonly List<NavigationItem> NavigationTree;

        /// <summary>
        /// Parses YAML frontmatter from a markdown file and extracts metadata.
        /// </summary>
        /// <param name="content">The full markdown file content</param>
        /// <param name="frontmatter">The parsed frontmatter dictionary</param>
        /// <param name="remainingContent">The markdown content after the frontmatter</param>
        /// <returns>True if frontmatter was found and parsed, false otherwise</returns>
        private static bool TryParseFrontmatter(
            string content,
            out Dictionary<string, string> frontmatter,
            out string remainingContent)
        {
            frontmatter = new Dictionary<string, string>();
            remainingContent = content;

            // Check if file starts with frontmatter (---)
            if (!content.Trim().StartsWith("---", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Find the closing ---
            var closeIndex = content.IndexOf("---", content.IndexOf("---") + 3);
            if (closeIndex < 0)
            {
                // No closing ---, treat entire content as frontmatter (or no frontmatter)
                remainingContent = "";
                return true;
            }

            var frontmatterSection = content.Substring(3, closeIndex - 3);
            remainingContent = content.Substring(closeIndex + 3);

            // Parse YAML frontmatter (simple key-value pairs)
            var lines = frontmatterSection.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                // Skip empty lines and comments
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                // Parse key: value pairs
                var colonIndex = trimmedLine.IndexOf(':');
                if (colonIndex >= 0)
                {
                    var key = trimmedLine.Substring(0, colonIndex).Trim();
                    var value = trimmedLine.Substring(colonIndex + 1).Trim();
                    // Remove quotes if present
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    frontmatter[key] = value;
                }
            }

            return true;
        }

        public List<NavigationItem> ScanDirectory()
        {
            return ScanDirectory(_rootPath);
        }

        private List<NavigationItem> ScanDirectory(string path)
        {
            var items = new List<NavigationItem>();
            var files = Directory.GetFiles(path);
            var directories = Directory.GetDirectories(path);

            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".md")
                {
                    var fileName = Path.GetFileName(file);
                    var relativePath = $"./{Path.GetRelativePath(_rootPath, file)}";

                    // Load file content
                    var fullContent = File.ReadAllText(file);

                    // Parse frontmatter if present
                    var frontmatter = new Dictionary<string, string>();
                    var markdownContent = fullContent;
                    TryParseFrontmatter(fullContent, out frontmatter, out markdownContent);

                    // Strip the frontmatter from the displayed content
                    var content = markdownContent.TrimStart('\n', '\r').Trim();

                    items.Add(new NavigationItem
                    {
                        Name = fileName,
                        FilePath = file,
                        Content = content,
                        RelativePath = relativePath,
                        Frontmatter = frontmatter
                    });
                }
            }

            foreach (var directory in directories)
            {
                var directoryName = Path.GetFileName(directory);
                var directoryPath = directory;
                var subItems = ScanDirectory(directoryPath);

                items.Add(new NavigationItem
                {
                    Name = directoryName,
                    FilePath = directoryPath,
                    Children = subItems
                });
            }

            return items;
        }
    }
}