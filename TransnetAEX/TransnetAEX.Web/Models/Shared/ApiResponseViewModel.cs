namespace TransnetAEX.Web.Models.Shared
{
    public class ApiResponseViewModel<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public T? Data { get; set; }
    }
}
