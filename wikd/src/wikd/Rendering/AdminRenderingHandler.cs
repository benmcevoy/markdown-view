using wikd.Routing;

namespace wikd.Rendering;

public class AdminRenderingHandler : IRenderingHandler
{
    public string[] SupportedFileExtensions => [];

    public bool CanHandle(Route input) =>
            input is SpecialRoute f && input.Path == Router.AdminRoute;

    public Task<string> HandleAsync(Route input) => Task.FromResult("TODO: admin, reindex, list supported file extension, list scripts");
}