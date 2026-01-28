namespace EcommerceApiGatewayCMS.Models.DTOs
{
    public class SignInResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int WorkplaceId { get; set; }
    }
}
