using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace PasswordExpiration.AzFunction.Helpers.Services
{
    using Lib.Models.Core;
    using Models.Configs;
    public interface ICosmosDbService
    {
        List<UserSearchConfigItem> GetUserSearchConfigs(ILogger logger);
    }
}