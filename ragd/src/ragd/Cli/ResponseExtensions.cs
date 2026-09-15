using System.Text.Json;
using http;
using ragd.Core;

namespace ragd.Cli;

public static class ResponseExtensions
{
    public static ApiResponse AsApiResponse(this Response response)
        => JsonSerializer.Deserialize<ApiResponse>(response.Body, JsonSerializerOptions.Web)
        ?? new ApiResponse
        {
            Status = "ERROR",
            Message = "Unable to deserialize response.Body."
        };

    public static ApiResponse<T> AsApiResponse<T>(this Response response)
            => JsonSerializer.Deserialize<ApiResponse<T>>(response.Body, JsonSerializerOptions.Web)
            ?? new ApiResponse<T>
            {
                Status = "ERROR",
                Message = "Unable to deserialize response.Body."
            };

}