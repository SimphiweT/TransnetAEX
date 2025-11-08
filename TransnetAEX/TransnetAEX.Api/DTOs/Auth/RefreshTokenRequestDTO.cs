namespace TransnetAEX.Api.DTOs.Auth
{
    public class RefreshTokenRequestDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
