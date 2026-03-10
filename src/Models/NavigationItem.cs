
namespace MdView.Models
{
    /// <summary>
    /// Model representing a markdown file or folder in the navigation structure.
    /// </summary>
    public class NavigationItem
    {
        /// <summary>
        /// The display name of the file or folder.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The full file system path to the file.
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        /// Child items for folders, or null for leaf files.
        /// </summary>
        public List<NavigationItem> Children { get; set; } = [];

        /// <summary>
        /// The markdown content of the file (without frontmatter).
        /// </summary>
        public string Content { get; set; } = "";

        /// <summary>
        /// The relative path from the root directory.
        /// </summary>
        public string RelativePath { get; set; } = "";

        /// <summary>
        /// Frontmatter metadata extracted from YAML frontmatter (if present).
        /// </summary>
        public Dictionary<string, string> Frontmatter { get; set; } = [];
    }
}