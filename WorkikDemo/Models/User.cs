namespace WorkikDemo.Models
{
    public class User
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!; // In real apps, store hashed passwords
        public string Role { get; set; } = "User";
    }
}