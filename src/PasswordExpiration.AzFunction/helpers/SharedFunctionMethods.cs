using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace PasswordExpiration.AzFunction.Helpers
{
    using AzFunction.Models.Configs;
    using AzFunction.Helpers.Services;

    using Lib;
    using Lib.Core;
    using Lib.Core.Graph;
    using Lib.Models.Core;

    public static class SharedFunctionMethods
    {
        /// <summary>
        /// Get the user search config item. If configId is null, then the default search config is used.
        /// </summary>
        /// <param name="functionsConfigSvc">The <see cfef="FunctionsConfigService">FunctionsConfigService</see> passed in from the executing function.</param>
        /// <param name="configId">The unique identifier for the user search config.</param>
        /// <returns></returns>
        public static UserSearchConfigItem GetUserSearchConfigItem(
            IFunctionsConfigService functionsConfigSvc,
            string configId
        )
        {
            UserSearchConfigItem userSearchConfig = string.IsNullOrEmpty(configId) switch
            {
                true => functionsConfigSvc.GetUserSearchConfig("65ef0352-9545-42f8-b70f-6dd21fa8df6b"),
                _ => functionsConfigSvc.GetUserSearchConfig(configId),
            };

            return userSearchConfig;
        }

        /// <summary>
        /// Convert a 'ConcurrentBag' object to a 'List' object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputConcurrentBag">The ConcurrentBag to convert.</param>
        /// <returns></returns>
        public static List<T> ConvertConcurrentBagToList<T>(
            ConcurrentBag<T> inputConcurrentBag
        )
        {
            return new List<T>(inputConcurrentBag);
        }
        
        /// <summary>
        /// Get the password expiration details about all of the users matching the search criteria.
        /// </summary>
        /// <param name="userTools">The <see cref="UserTools">UserTools</see> object to get user details from the Microsoft Graph API.</param>
        /// <param name="searchConfigItem">The <see cref="UserSearchConfigItem">UserSearchConfigItem</see> to use for searching for user.</param>
        /// <param name="logger">The <see cref="ILogger">ILogger</see> running in the function's context.</param>
        /// <returns></returns>
        public async static Task<List<UserPasswordExpirationDetails>> GetUserDetailsByLastNames(
            UserTools userTools,
            UserSearchConfigItem searchConfigItem,
            ILogger logger
        )
        {
            /*
                The following section is to prep for running the tasks by:
                    - Creating a task throttler (using SemaphoreSlim) to limit the amount of concurrent threads used to 10.
                    - Creating a 'ConcurrentBag' for the tasks to store each user data in.
                    - Creating a 'List' for storing the created tasks in.
            */
            SemaphoreSlim taskThrottler = new(10);
            ConcurrentBag<UserPasswordExpirationDetails> usersWithExpiringPasswordsBag = new();
            List<Task> taskList = new();

            // Run a for loop to increment from 65 to 90 (The UTF32 decimal codes for the letters A-Z) to create the tasks to get users.
            for (int i = 65; i <= 90; i++)
            {
                // If we reach the max number of concurrent tasks, then wait to create the rest of the tasks.
                await taskThrottler.WaitAsync();

                // Convert the current loop integer (i) value into the corresponding UTF32 character.
                string lastNameStartsWith = Char.ConvertFromUtf32(i);
                logger.LogInformation($"Creating task for getting users, under the '{searchConfigItem.DomainName}' domain and their last name starts with '{lastNameStartsWith}', with expiring passwords.");

                // Create the task to get the users.
                taskList.Add(
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                // Get all of the users matching the search criteria.
                                List<UserPasswordExpirationDetails> userPasswordExpirationDetails = ExpiringPasswordFinder.GetUsersWithExpiringPasswords(
                                    userTools,
                                    searchConfigItem.DomainName,
                                    searchConfigItem.OuPath,
                                    lastNameStartsWith,
                                    TimeSpan.FromDays(searchConfigItem.MaxPasswordAge),
                                    TimeSpan.FromDays(10)
                                );

                                // Add each found user into the 'ConcurrentBag' of all the users.
                                foreach (UserPasswordExpirationDetails user in userPasswordExpirationDetails)
                                {
                                    usersWithExpiringPasswordsBag.Add(user);
                                }
                            }
                            finally
                            {
                                // When the task is finished, release the thread for the next task to utilize.
                                taskThrottler.Release();
                            }
                        }
                    )
                );
            }

            // Wait for the remaining tasks to finish.
            logger.LogInformation("Waiting for all remaining tasks to complete.");
            Task.WaitAll(taskList.ToArray());

            // Dispose of all of the tasks that were created.
            logger.LogInformation("Cleaning up resources.");
            foreach (Task task in taskList)
            {
                task.Dispose();
            }

            // Convert the 'ConcurrentBag' to a 'List'. Then clear all of the values out of the 'ConcurrentBag'.
            List<UserPasswordExpirationDetails> userList = ConvertConcurrentBagToList<UserPasswordExpirationDetails>(usersWithExpiringPasswordsBag);
            usersWithExpiringPasswordsBag.Clear();

            taskList.Clear();
            taskThrottler.Dispose();

            // Sort all of the items alphabetically in the 'List' by each user's last name.
            userList.Sort((item1, item2) => string.Compare(item1.User.Surname, item2.User.Surname));

            return userList;
        }

        /// <summary>
        /// Send the password expiration email to the user.
        /// </summary>
        /// <param name="mailTools"><see cref="MailTools" /></param>
        /// <param name="mailFromUPN">The user principal name of the user to send the email from.</param>
        /// <param name="userItem">The <see cref="UserPasswordExpirationDetails">password expiration details</see> for the user.</param>
        /// <param name="emailTemplateFilePath">The file path to the HTML file for the email template to use.</param>
        /// <param name="emailTemplateFileAttachments">The file paths to the attachments to add to the email.</param>
        public static void SendEmailToUser(
            MailTools mailTools,
            string mailFromUPN,
            UserPasswordExpirationDetails userItem,
            string emailTemplateFilePath,
            string[] emailTemplateFileAttachments
        )
        {
            // Generate the email message body with the user's details.
                string emailBody = MailBodyGenerator.CreateMailGenerator(
                    userItem,
                    emailTemplateFilePath,
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
                );

                // Send the email to the user.
                mailTools.SendMessageWithAttachment(
                    mailFromUPN,
                    userItem.User,
                    $"Alert: Password Expiration Notice ({userItem.PasswordExpiresIn.Value.Days} days)",
                    emailBody,
                    emailTemplateFileAttachments
                );
        }
    }
}