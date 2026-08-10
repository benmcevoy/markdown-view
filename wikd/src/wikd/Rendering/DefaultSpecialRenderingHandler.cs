using wikd.Routing;

namespace wikd.Rendering;

public class DefaultSpecialRenderingHandler : IRenderingHandler
{
    public string[] SupportedFileExtensions => [];

    bool IHandler<Route, string>.CanHandle(Route input) =>
            input is SpecialRoute f;

    public string Handle(Route input) => @"<div style='white-space: pre;font-family: monospace;line-height:1.17'>
 █████ █████     █████    █████ █████ 
░░███ ░░███    ███░░░███ ░░███ ░░███  
 ░███  ░███ █ ███   ░░███ ░███  ░███ █
 ░███████████░███    ░███ ░███████████
 ░░░░░░░███░█░███    ░███ ░░░░░░░███░█
       ░███░ ░░███   ███        ░███░ 
       █████  ░░░█████░         █████ 
      ░░░░░     ░░░░░░         ░░░░░  
    </div>";
}