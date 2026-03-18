namespace MdView.Routing
{
    public class RouteInfo
    {
        public string RequestPath { get; set; } = "";
        public string Path { get; set; } = "";
        public RouteType RouteType { get; set; } = RouteType.Unsupported;
    }
}
