using wikd.Routing;

namespace wikd.Rendering
{
    public interface IRenderingHandler : IAsyncHandler<Route, string>
    {
        string[] SupportedFileExtensions { get; } 
        bool IAsyncHandler<Route, string>.CanHandle(Route input) =>
            input is FileRoute f && SupportedFileExtensions.Contains(f.Extension);
    }
}