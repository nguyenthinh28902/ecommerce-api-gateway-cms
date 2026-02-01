namespace Ecommerce.ApiGateway.Cms.Service.Interfaces
{
    public interface ITokenClientService
    {
        public Task<string> GetSystemTokenAsync();
    }
}
