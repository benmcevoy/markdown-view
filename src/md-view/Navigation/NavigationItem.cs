namespace MdView.Navigation
{
    public class NavigationItem
    {
        public bool IsCurrent { get; set; } = false;
        public string RelativeUrl { get; set; } = "";
        public string Name { get; set; } = "";
        public string Extension { get; set; } = "";
        public List<NavigationItem> Children { get; set; } = [];

        public override string ToString()
        {
            return $"{Name} ({RelativeUrl})";
        }
    }
}