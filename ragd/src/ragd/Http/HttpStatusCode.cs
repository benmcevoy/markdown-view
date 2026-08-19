namespace ragd.Http;

public record HttpStatusCode 
{
    public static HttpStatusCode OK = new("OK", 200);
    public static HttpStatusCode ServerError = new("Internal Server Error", 500);
    public static HttpStatusCode ClientError = new("Bad Request", 400);
    public static HttpStatusCode Malformed= new("Malformed", 999);

    private HttpStatusCode(string status, int code)
    {
        Status = status;
        Code = code;
    }

    public string Status { get; }
    public int Code { get; }
    public override string ToString() => $"{Code} {Status}";
}
