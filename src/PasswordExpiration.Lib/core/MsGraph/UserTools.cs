using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PasswordExpiration.Lib.Core.Graph
{
    using PasswordExpiration.Lib.Models.Core;
    using PasswordExpiration.Lib.Models.Graph.Users;
    public class UserTools
    {
        public UserTools(GraphClient graphClient)
        {
            GraphClient = graphClient;
        }

        public GraphClient GraphClient { get; set; }

        private string[] userProperties = new string[] {
            "accountEnabled",
            "userPrincipalName",
            "displayName",
            "givenName",
            "surname",
            "lastPasswordChangeDateTime",
            "onPremisesDistinguishedName"
        };

        private string ouSearchPath;

        public User GetUser(string userPrincipalName)
        {
            string apiResponse = GraphClient.SendApiCall($"users/{userPrincipalName}?$select={string.Join(",", userProperties)}", null, HttpMethod.Get);

            User userItem = JsonConverter.ConvertFromJson<User>(apiResponse);

            return userItem;
        }

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
                ouSearchPath = ouPath;
                allUsers = allUsers.FindAll(FilterByOuPath);
                ouSearchPath = null;
            }

            return allUsers;
        }

        private bool FilterByOuPath(User user)
        {
            if (string.IsNullOrEmpty(user.OnPremisesDistinguishedName) == false && user.OnPremisesDistinguishedName.EndsWith(ouSearchPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}