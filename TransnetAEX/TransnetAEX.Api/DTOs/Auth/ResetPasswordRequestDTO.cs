namespace TransnetAEX.Api.DTOs.Auth
{
    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; } = "";
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
