namespace TransnetAEX.Api.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Role { get; set; } = "";
        public string Token { get; set; } = "";
        public DateTime TokenExpiry { get; set; }
        public string ProfileImageUrl { get; set; } = "";
    }
}
