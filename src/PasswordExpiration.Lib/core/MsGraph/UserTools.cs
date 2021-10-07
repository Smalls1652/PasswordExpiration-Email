using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

using SmallsOnline.MsGraphClient.Models;

namespace PasswordExpiration.Lib.Core.Graph
{
    using PasswordExpiration.Lib.Models.Graph.Users;

    /// <summary>
    /// Hosts methods for interacting with Microsoft Graph user API endpoints.
    /// </summary>
    public class UserTools
    {
        public UserTools(IGraphClient graphClient)
        {
            GraphClient = graphClient;
        }

        public IGraphClient GraphClient { get; set; }

        private string[] userProperties = new string[] {
            "id",
            "accountEnabled",
            "userPrincipalName",
            "displayName",
            "givenName",
            "surname",
            "lastPasswordChangeDateTime",
            "onPremisesDistinguishedName"
        };

        /// <summary>
        /// Get a user object.
        /// </summary>
        /// <param name="userPrincipalName">The user principal name of the user.</param>
        /// <returns></returns>
        public User GetUser(string userPrincipalName)
        {
            string apiResponse = GraphClient.SendApiCall($"users/{userPrincipalName}?$select={string.Join(",", userProperties)}", null, HttpMethod.Get);

            User userItem = JsonConverter.ConvertFromJson<User>(apiResponse);

            return userItem;
        }

        /// <summary>
        /// Get a set of users based off their domain name and what their last name starts with.
        /// </summary>
        /// <param name="domainName">The domain name of the users.</param>
        /// <param name="ouPath">The OU path the users are in.</param>
        /// <param name="lastNameStartsWith">The first letter to search by. Can be null.</param>
        /// <returns></returns>
        public List<User> GetUsers(string domainName, string ouPath, string lastNameStartsWith)
        {
            List<User> allUsers = new List<User>();

            Regex nextLinkRegex = new Regex("^https:\\/\\/graph.microsoft.com\\/(?'version'v1\\.0|beta)\\/(?'endpoint'.+?)$");

            List<string> filters = new List<string>(
                new string[] {
                    $"(endsWith(userPrincipalName, '@{domainName}'))",
                    "and (accountEnabled eq true)",
                }
            );

            if (!string.IsNullOrEmpty(lastNameStartsWith))
            {
                filters.Add($"and (startsWith(surname, '{lastNameStartsWith}'))");
            }

            bool noNextLink = false;
            //string filterString = $"endsWith(userPrincipalName, '@{domainName}') and (accountEnabled eq true)";
            string filterString = string.Join(" ", filters);
            string apiEndpoint = $"users?$select={string.Join(",", userProperties)}&$filter={filterString}&$count=true";

            while (!noNextLink)
            {
                //Console.WriteLine($"--- API Endpoint: {apiEndpoint} ---");
                string apiResponse = GraphClient.SendApiCall(apiEndpoint, null, HttpMethod.Get);

                UserCollection userCollection = JsonConverter.ConvertFromJson<UserCollection>(apiResponse);

                foreach (User user in userCollection.Value)
                {
                    allUsers.Add(user);
                }

                switch (string.IsNullOrEmpty(userCollection.OdataNextLink))
                {
                    case false:
                        Match nextLinkRegexMatch = nextLinkRegex.Match(userCollection.OdataNextLink);
                        apiEndpoint = nextLinkRegexMatch.Groups["endpoint"].Value;
                        break;

                    default:
                        noNextLink = true;
                        //Console.WriteLine("--- No next link found. Finishing. ---");
                        break;
                }
            }

            if (!string.IsNullOrEmpty(ouPath))
            {
                allUsers = allUsers.FindAll(
                    (user) =>
                    {
                        if (string.IsNullOrEmpty(user.OnPremisesDistinguishedName) == false && user.OnPremisesDistinguishedName.EndsWith(ouPath))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                );
            }

            return allUsers;
        }
    }
}