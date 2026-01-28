namespace EcommerceIdentityServerCMS.Models.DTOs.SignIn
{
    public class SignInResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public int WorkplaceId { get; set; }
        public List<string> Scopes { get; set; }
    }
}
