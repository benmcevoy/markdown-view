using System.Text;

namespace MdView.Templates
{
    public class DefaultTemplate()
    {
        // TODO: use keyvalue pairs for content fragments or something to strengthen this and improve extensionabilty
        public string Render(string title, string aside, string main, string breadcrumb, string name)
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
            template = template.Replace("{{name}}", name);
            template = template.Replace("{{markdown}}", main);

            // TODO: embed scripts too
            template = template.Replace("{{scripts}}",
@"<script type='module'>
    import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';
    mermaid.initialize({{ startOnLoad: true }});
</script>
<!-- KaTeX -->
<link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/katex/dist/katex.min.css'>
<script src='https://cdn.jsdelivr.net/npm/katex/dist/katex.min.js'></script>
<script src='https://cdn.jsdelivr.net/npm/katex/dist/contrib/auto-render.min.js'></script>
<script>
document.addEventListener('DOMContentLoaded', function() {
renderMathInElement(document.body, {
    // customised options
    // • auto-render specific keys, e.g.:
    delimiters: [
        {left: '$$', right: '$$', display: true},
        {left: '$', right: '$', display: false},
        {left: '\\(', right: '\\)', display: false},
        {left: '\\[', right: '\\]', display: true}
    ],
    // • rendering keys, e.g.:
    throwOnError : false
});
});
</script>");


            return template.ToString();
        }
    }
}