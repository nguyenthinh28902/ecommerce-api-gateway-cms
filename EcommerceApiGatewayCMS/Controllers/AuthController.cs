using EcommerceApiGatewayCMS.Models.ViewModels.Accounts;
using EcommerceApiGatewayCMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EcommerceApiGatewayCMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) { 
            _authService = authService;
        }

        [HttpPost("dang-nhap")]
        public async Task<IActionResult> SignIn([FromForm] SignInViewModel signInViewModel)
        {
            var result = await _authService.AuthenticateInternal(signInViewModel);
            await _authService.SignIn(result);

            return Redirect(signInViewModel.ReturnUrl);
        }
    }
}
