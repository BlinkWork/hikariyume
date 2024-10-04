namespace Webclient.Models
{
    public class UserChangeInformation
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; } 
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
