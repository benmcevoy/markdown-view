using System.Text;

namespace MdView.Templates
{
    public class DefaultTemplate()
    {
        // TODO: use keyvalue pairs for content fragments or something to strengthen this and improve extensionabilty
        public string Render(string title, string aside, string main, string breadcrumb)
        {
            var template = new StringBuilder(Assets.default_html, Assets.default_html.Length * 2);

            template = template.Replace("{{styles}}",
        @$"<style>
            {Assets.main_css}
            {Assets.markdown_css}
        </style>");

            template = template.Replace("{{title}}", title);
            template = template.Replace("{{breadcrumb}}", breadcrumb);
            template = template.Replace("{{aside}}", aside);
            template = template.Replace("{{markdown}}", main);

            // TODO: embed scripts too
            template = template.Replace("{{scripts}}",
        @$"<script type='module'>
            import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';
            mermaid.initialize({{ startOnLoad: true }});
        </script>");

            return template.ToString();
        }
    }
}