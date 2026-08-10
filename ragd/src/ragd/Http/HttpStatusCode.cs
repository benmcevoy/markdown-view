namespace ragd.Http;

public class HttpStatusCode 
{
    public static HttpStatusCode OK = new("OK", 200);
    public static HttpStatusCode ServerError = new("Internal Sedrver Error", 500);
    public static HttpStatusCode ClientError = new("Bad Request", 400);

    private HttpStatusCode(string status, int code)
    {
        Status = status;
        Code = code;
    }

    public string Status { get; }
    public int Code { get; }
    public override string ToString() => $"{Status} {Code}";

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not HttpStatusCode otherValue) return false;

        return Code.Equals(otherValue.Code);
    }

    public override int GetHashCode() => Code.GetHashCode();
}
