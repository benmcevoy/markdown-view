using System.Text;

namespace wikd.Templates
{
    public class DefaultTemplate()
    {
        // TODO: use keyvalue pairs for content fragments or something to strengthen this and improve extensionabilty
        public string Render(string title, string aside, string main, string breadcrumb, string name)
        {
            var template = new StringBuilder(Assets.default_html);

            template = template.Replace("{{styles}}",
                @$"<style>
                    {Assets.main_css}
                    {Assets.markdown_css}
                </style>");

            template = template.Replace("{{title}}", title);
            template = template.Replace("{{breadcrumb}}", breadcrumb);
            template = template.Replace("{{aside}}", aside);
            template = template.Replace("{{name}}", name);
            template = template.Replace("{{markdown}}", main);

            template = template.Replace("{{scripts}}",
                @$"
                    {Assets.script_html}
                ");

            return template.ToString();
        }
    }
}