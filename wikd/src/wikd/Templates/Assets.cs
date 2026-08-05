using System.Reflection;

namespace wikd.Templates;

public class Assets
{
  static Assets()
  {
    const string prefix = "wikd.Templates.";

    main_css = GetResource($"{prefix}css.main.css");
    markdown_css = GetResource($"{prefix}css.markdown.css");
    default_html = GetResource($"{prefix}html.default.html");
  }
  public static readonly string main_css;
  public static readonly string markdown_css;
  public static readonly string default_html;

  private static string GetResource(string resourceName)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resource = assembly.GetManifestResourceStream(resourceName);
    using var reader = new StreamReader(resource!);

    return reader.ReadToEnd();
  }
}

