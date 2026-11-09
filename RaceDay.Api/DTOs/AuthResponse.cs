namespace RaceDay.Api.DTOs
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
