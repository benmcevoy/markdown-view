using wikd.Routing;

namespace wikd.Rendering;

public class AdminRenderingHandler : IRenderingHandler
{
    public string[] SupportedFileExtensions => [];

    bool IHandler<Route, string>.CanHandle(Route input) =>
            input is SpecialRoute f && input.Path == Router.AdminRoute;

    public string Handle(Route input) => "TODO: admin, reindex, list supported file extension, list scripts";
}