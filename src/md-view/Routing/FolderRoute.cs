namespace MdView.Routing
{
    public class FolderRoute : Route
    {
        public List<Route> Children { get; set; } = [];

        public IEnumerable<Route> OrderedChildren()
        {
            var folders = new List<Route>();
            var files = new List<Route>();
            // folders then files
            foreach (var child in Children.OrderBy(x => x.Name))
            {
                if (child is FolderRoute f)
                {
                    folders.Add(child);
                    continue;
                }

                files.Add(child);
            }

            return folders.Concat(files);
        }
    }
}