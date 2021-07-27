using System.ComponentModel.DataAnnotations;

namespace UMS.Dto.Auth
{
    public class AuthenticateRequest
    {
        [Required] 
        public string CompName { get; set; }
        [Required] 
        public string UserCode { get; set; }
        [Required]
        public string Password { get; set; }
        public bool LogIn { get; set; }
    }
}