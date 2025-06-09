using System.Threading.Tasks;

namespace api.Interfaces.Services
{
    public interface IPaypalService
    {
        Task<string> GetAccessTokenAsync();
    }
}
