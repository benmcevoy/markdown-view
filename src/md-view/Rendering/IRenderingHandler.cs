using MdView.Routing;

namespace MdView.Rendering
{
    public interface IRenderingHandler : IHandler<Route, string>
    {
        string[] SupportedFileExtensions { get; }

        bool IHandler<Route, string>.CanHandle(Route input)=>
            input is FileRoute f && SupportedFileExtensions.Contains(f.Extension);
    }
}