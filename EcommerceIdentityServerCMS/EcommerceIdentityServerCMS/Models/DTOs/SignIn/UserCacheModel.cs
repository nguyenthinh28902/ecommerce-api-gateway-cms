namespace EcommerceIdentityServerCMS.Models.DTOs.SignIn
{
    public class UserCacheModel
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public int? WorkplaceId { get; set; }

    }
}
