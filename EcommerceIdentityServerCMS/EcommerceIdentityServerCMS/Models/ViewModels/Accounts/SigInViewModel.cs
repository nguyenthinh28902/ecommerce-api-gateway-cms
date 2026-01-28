using System.ComponentModel.DataAnnotations;

namespace EcommerceIdentityServerCMS.Models.ViewModels.Accounts
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã người dùng")]
        [Display(Name = "Mã người dùng (ID)")]
        public int UserId { get; set; } // Theo yêu cầu của bạn là kiểu int

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
