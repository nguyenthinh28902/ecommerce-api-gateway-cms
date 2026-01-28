using Duende.IdentityServer.Validation;
using EcommerceIdentityServerCMS.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class CustomTokenRequestValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            // Đọc tham số "custom_user_id" mà Gateway gửi sang trong Form-data
            //form["custom_user_id"] = signInResponseDto.Id.ToString();
            //form["custom_email"] = signInResponseDto.Email.ToString();
            //form["custom_role_id"] = signInResponseDto.Role.ToString();
            var userId = context.Result.ValidatedRequest.Raw.Get("custom_user_id");
            var email = context.Result.ValidatedRequest.Raw.Get("custom_email");
            var role = context.Result.ValidatedRequest.Raw.Get("custom_role");
            var workplaceId = context.Result.ValidatedRequest.Raw.Get("custom_WorkplaceId") ?? string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                // Thêm claim "sub" vào Access Token. 
                // Đây là lúc thông tin thô biến thành thông tin bảo mật có chữ ký.
                context.Result.ValidatedRequest.ClientClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId));
                context.Result.ValidatedRequest.ClientClaims.Add(new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty));
                context.Result.ValidatedRequest.ClientClaims.Add(new Claim("role", role ?? string.Empty));
                context.Result.ValidatedRequest.ClientClaims.Add(new Claim("wid", workplaceId));
            }

            return Task.CompletedTask;
        }
    }
}
