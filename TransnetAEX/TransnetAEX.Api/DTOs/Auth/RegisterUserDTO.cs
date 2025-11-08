namespace TransnetAEX.Api.DTOs.Auth
{
    public class RegisterUserDTO
    {
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string SAP_Number { get; set; } = "";
        public string ContactNumber { get; set; } = "";
        public string Description { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Role { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
