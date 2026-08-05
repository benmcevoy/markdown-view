using System.Reflection;

namespace ragd.Service.Handlers;

public class Help
{
  static Help() => Api = GetResource("ragd.Service.Handlers.help.json");

  public static readonly string Api;

  private static string GetResource(string resourceName)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resource = assembly.GetManifestResourceStream(resourceName);
    using var reader = new StreamReader(resource!);

    return reader.ReadToEnd();
  }
}
