namespace wikd.Routing
{
    public abstract class Route
    {
        public FolderRoute? Parent { get; set; } = null;
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Uri { get; set; } = "";
        public string RouteType() => this switch
        {
            FolderRoute => "folder",
            FileRoute => "file",
            _ => "special"
        };
    }
}