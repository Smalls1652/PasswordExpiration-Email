using Microsoft.Azure.Functions.Worker;

using SmallsOnline.MsGraphClient.Models;

namespace PasswordExpiration.AzFunction.Helpers.Services
{
    public interface IGraphClientService : IGraphClient
    {
        void Connect();

        void TestAuthToken();
    }
}