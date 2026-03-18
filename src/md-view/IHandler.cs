namespace MdView
{
    public interface IHandler<TIn, TOut>
    {
        bool CanHandle(TIn input);
        TOut Handle(TIn input);
    }
}