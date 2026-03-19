namespace MdView.Templates
{
    public class DefaultTemplate()
    {
        public string Render(string title, string aside, string main)
        {
            var template = MainTemplate;

            template = template.Replace("{{title}}", title);
            template = template.Replace("{{aside}}", aside);
            template = template.Replace("{{markdown}}", main);

            return template;
        }

        private const string MainTemplate = @$"<!DOCTYPE html>
<html lang='en'>
<head>
    <link rel='icon' href='data:image/x-icon;base64,AAABAAEAEBAQAAAAAAAoAQAAFgAAACgAAAAQAAAAIAAAAAEABAAAAAAAgAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAZlX/ADOA/wAzVf8AfwB/ADOq/wD//wAA/wD/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAZmYAAGZmAAAAEQAAEQAAAAA1AAAzAAAAABFmZiMAAAAAAHd3AAAAAAAAZmYAAAAAZgB3dwBmAAB3AGZmAHcAAAB3d3d3AAAAAAB3dwAAAAAAd3d3dwAAAHcAd3cAdwAAdwB3dwB3AAAAd0REdwAAAABEAABEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA' type='image/x-icon' />
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{{{{title}}}}</title>
    <style>{css.Main.Css}</style>
    <style>{css.Markdown.Css}</style>
</head>
<body>
    <div class='container'>
        <div class='sidebar'>
            <h3>Navigation</h3>
            {{{{aside}}}}
        </div>
        <div class='main-content'>
            <div class='markdown-body'>
                {{{{markdown}}}}
            </div>
        </div>
    </div>
    <script type='module'>
  import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';
  mermaid.initialize({{ startOnLoad: true }});
</script>
</body>
</html>";
    }
}