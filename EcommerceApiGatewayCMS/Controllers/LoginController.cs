using EcommerceApiGatewayCMS.Models.ViewModels.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApiGatewayCMS.Controllers
{
    public class LoginController : Controller
    {

        public LoginController() { }


        [HttpGet("dang-nhap-he-thong")]
        public IActionResult Index(string returnUrl = "/")
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Chuyển hướng về trang chủ hoặc returnUrl thay vì cho đăng nhập tiếp
                return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
            }
            var signInViewModel = new SignInViewModel();
            signInViewModel.ReturnUrl = returnUrl;
            return View(signInViewModel);
        }   

        [HttpGet("dang-nhap-that-bai")]
        public IActionResult Error() {

            string mess = "Mã người dùng hoặc mật khẩu không đúng, vui lòng thử lại.";
            return View("Error", mess);
        }
    }
}
