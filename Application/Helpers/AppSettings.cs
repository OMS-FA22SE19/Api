namespace Application.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public bool ValidateIssuer { get; set; }
        public string ValidIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public string ValidAudience { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public int ExpireHours { get; set; }
    }
}
