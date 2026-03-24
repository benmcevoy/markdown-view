namespace MdView.Templates
{
    public class DefaultTemplate()
    {
        // TODO: use keyvalue pairs for content fragments
        public string Render(string title, string aside, string main, string breadcrumb)
        {
            var template = Assets.default_html;

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

            return template;
        }
    }
}