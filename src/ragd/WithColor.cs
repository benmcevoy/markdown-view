namespace ragd;

public class WithColor : IDisposable {
    private readonly ConsoleColor _original;

    public WithColor(ConsoleColor color)
    {
        _original = Console.ForegroundColor;
        Console.ForegroundColor = color;
    }

    public void Dispose() => Console.ForegroundColor = _original;
}