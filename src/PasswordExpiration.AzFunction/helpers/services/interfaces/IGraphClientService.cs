using Microsoft.Azure.Functions.Worker;

namespace PasswordExpiration.AzFunction.Helpers.Services
{
    using Lib.Models.Core;

    public interface IGraphClientService : IGraphClient
    {
        void Connect();
    }
}