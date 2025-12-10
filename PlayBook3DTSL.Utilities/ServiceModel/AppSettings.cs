namespace PlayBook3DTSL.Utilities.ServiceModel
{
    public class AppSettings
    {
        public string Key { get; set; } = null!;

        public string JwtIssuer { get; set; } = null!;

        public string JwtAudience { get; set; } = null!;

        public double TokenExpiryTimeInMinutes { get; set; }

    }
}
