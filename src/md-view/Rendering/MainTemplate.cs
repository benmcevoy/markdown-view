namespace MdView.Rendering
{
    public class MainTemplate(string wwwroot)
    {
        private readonly string _mainTemplate = File.ReadAllText(Path.Combine(wwwroot, "html/main.html"));

        public string Render(string title, string aside, string main)
        {
            var template = _mainTemplate;

            template = template.Replace("{{title}}", title);
            template = template.Replace("{{aside}}", aside);
            template = template.Replace("{{markdown}}", main);

            return template;
        }
    }
}