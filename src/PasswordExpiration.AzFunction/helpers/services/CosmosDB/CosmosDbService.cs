using System;
using System.Collections.Generic;
using System.Net;

using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

namespace PasswordExpiration.AzFunction.Helpers.Services
{
    using Lib.Models.Core;
    using Lib.Models.Graph.Users;

    using Helpers;
    using Models.Configs;

    public class CosmosDbService : ICosmosDbService
    {
        public CosmosDbService(ILogger logger)
        {
            InitService(logger);
        }

        private CosmosClient cosmosDbClient;
        private readonly CosmosDbSerializer jsonSerializer = new(
            new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }
        );

        public List<UserSearchConfigItem> GetUserSearchConfigs(ILogger logger)
        {
            logger.LogInformation("Getting search configs.");
            Task<List<UserSearchConfigItem>> searchConfigsTask = Task.Run(
                async () =>
                {
                    List<UserSearchConfigItem> searchConfigItems = new();

                    Container container = cosmosDbClient.GetContainer("password-expiration-svc", "userSearchConfigs");
                    QueryDefinition query = new("Select * from c");

                    FeedIterator<UserSearchConfigItem> containerQueryIterator = container.GetItemQueryIterator<UserSearchConfigItem>(query);
                    while (containerQueryIterator.HasMoreResults)
                    {
                        foreach (UserSearchConfigItem item in await containerQueryIterator.ReadNextAsync())
                        {
                            searchConfigItems.Add(item);
                        }
                    }

                    containerQueryIterator.Dispose();

                    return searchConfigItems;
                }
            );

            searchConfigsTask.Wait();

            return searchConfigsTask.Result;
        }
        private void InitService(ILogger logger)
        {
            cosmosDbClient = new(
                connectionString: AppSettings.GetSetting("CosmosDbConnectionString"),
                clientOptions: new()
                {
                    Serializer = jsonSerializer
                }
            );

            logger.LogInformation("DB service initialized.");
        }
    }
}