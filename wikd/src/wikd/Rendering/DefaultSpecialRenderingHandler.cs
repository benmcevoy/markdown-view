using wikd.Routing;

namespace wikd.Rendering;

public class DefaultSpecialRenderingHandler : IRenderingHandler
{
    public string[] SupportedFileExtensions => [];

    public bool CanHandle(Route input) => input is SpecialRoute;

    public Task<string> HandleAsync(Route input) => Task.FromResult(@"<div style='white-space: pre;font-family: monospace;line-height:1.17'>
 █████ █████     █████    █████ █████ 
░░███ ░░███    ███░░░███ ░░███ ░░███  
 ░███  ░███ █ ███   ░░███ ░███  ░███ █
 ░███████████░███    ░███ ░███████████
 ░░░░░░░███░█░███    ░███ ░░░░░░░███░█
       ░███░ ░░███   ███        ░███░ 
       █████  ░░░█████░         █████ 
      ░░░░░     ░░░░░░         ░░░░░  
    </div>");
}