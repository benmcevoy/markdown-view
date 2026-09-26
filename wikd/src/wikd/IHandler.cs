namespace wikd
{
    public interface IAsyncHandler<TIn, TOut>
    {
        bool CanHandle(TIn input);
        Task<TOut> HandleAsync(TIn input);
    }
}