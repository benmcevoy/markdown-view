using MdView.Templates;

namespace MdView.Rendering
{
    public class Renderer(DefaultTemplate template, IRenderingHandler[] handlers)
    {
        private readonly IRenderingHandler[] _handlers = handlers;

        public ContentInfo Render(FileSystemInfo route)
        {
            var main = "nothing to display";

            foreach (var r in _handlers)
            {
                if (r.CanHandle(route))
                {
                    main = r.Handle(route);
                    break;
                }
            }

            return new ContentInfo(template.Render(route.Name, "TODO: build nav structure", main));
        }
    }
}



