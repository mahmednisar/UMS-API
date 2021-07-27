using System.ComponentModel.DataAnnotations;

namespace UMS.Dto.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string CompName { get; set; }
        [Required]
        public string UserCode { get; set; }
    }
}