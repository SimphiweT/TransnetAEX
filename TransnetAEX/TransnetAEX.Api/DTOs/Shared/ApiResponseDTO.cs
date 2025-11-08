namespace TransnetAEX.Api.DTOs.Shared
{
    public class ApiResponseDTO<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public T? Data { get; set; }

        public static ApiResponseDTO<T> SuccessResponse(T? data, string? message = null) =>
            new() { Success = true, Message = message ?? "Operation successful.", Data = data };

        public static ApiResponseDTO<T> FailResponse(string error, string? message = null) =>
            new() { Success = false, Error = error, Message = message };
    }
}
