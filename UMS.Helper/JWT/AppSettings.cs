namespace UMS.Helper.JWT
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public int AccessTokenExpiration { get; set; }
        public string CompHead { get; set; }
        public string CompName { get; set; }
    }
}