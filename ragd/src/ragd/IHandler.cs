namespace ragd
{
    public interface IHandler<TIn, TOut>
    {
        bool CanHandle(TIn context);
        TOut Handle(TIn context);
    }
}