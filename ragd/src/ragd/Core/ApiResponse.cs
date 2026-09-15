namespace ragd.Core;

public interface IApiResponse<out T>
{
    string Message { get; }
    string Status { get; }
    T? Data { get; }
    bool HasData() => Data is not null;
}

public class ApiResponse<T> : IApiResponse<T>
{
    public string Message { get; set; } = "";
    public string Status { get; set; } = "";
    public T? Data { get; set; } = default;
}

public class ApiResponse : ApiResponse<object>
{
    internal static ApiResponse Stopped() =>
        new()
        {
            Status = LifeCycleStates.STOPPED,
            Message = "RAG daemon is not running."
        };
}
