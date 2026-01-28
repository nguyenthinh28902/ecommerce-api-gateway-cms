namespace EcommerceIdentityServerCMS.Models.Settings
{
    public class ServiceAuthOptions
    {
        // Tên định danh của Service (ví dụ: personnel.api)
        public string ServiceName { get; set; } = string.Empty;

        // ClientId đã đăng ký trong IdentityServer (Database)
        public string ClientId { get; set; } = string.Empty;

        // Secret tương ứng để Gateway xác thực với IdentityServer
        public string ClientSecret { get; set; } = string.Empty;

        // Các quyền mà Service này yêu cầu (ví dụ: "openid profile personnel.read")
        public string Scope { get; set; } = string.Empty;

        public string GrantType {  get; set; } = string.Empty;
        public string RedirectUri {  get; set; } = string.Empty;
    }
}
