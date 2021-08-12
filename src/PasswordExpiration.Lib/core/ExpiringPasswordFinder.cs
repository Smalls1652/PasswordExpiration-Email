using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordExpiration.Lib
{
    using Core;
    using Core.Graph;
    using Models.Core;
    using Models.Graph.Users;

    /// <summary>
    /// Hosts methods for finding users with passwords expiring soon.
    /// </summary>
    public static class ExpiringPasswordFinder
    {
        /// <summary>
        /// Get users with passwords expiring soon based off a search criteria.
        /// </summary>
        /// <param name="userTools"><See cref="UserTools" /></param>
        /// <param name="domainName">The domain name of the users.</param>
        /// <param name="ouPath">The path to the on-premise AD OU.</param>
        /// <param name="lastNameStartsWith">The first letter to search by. Can be null.</param>
        /// <param name="maxPasswordAge">The amount of days a password can be valid.</param>
        /// <param name="closeToExpirationTimespan">The amount of days to consider a user's password to be close to expiration.</param>
        /// <returns></returns>
        public static List<UserPasswordExpirationDetails> GetUsersWithExpiringPasswords(
            UserTools userTools,
            string domainName,
            string ouPath,
            string lastNameStartsWith,
            TimeSpan maxPasswordAge,
            TimeSpan closeToExpirationTimespan)
        {
            List<User> foundUsers = userTools.GetUsers(domainName, ouPath, lastNameStartsWith);

            List<UserPasswordExpirationDetails> userPasswordExpirationDetails = new List<UserPasswordExpirationDetails>();
            foreach (User userItem in foundUsers)
            {
                userPasswordExpirationDetails.Add(
                    new UserPasswordExpirationDetails(
                        userItem,
                        maxPasswordAge,
                        closeToExpirationTimespan
                    )
                );
            }
            
            userPasswordExpirationDetails = userPasswordExpirationDetails.FindAll(FilterUsersWithExpiringPasswords);

            return userPasswordExpirationDetails;
        }

        /*
        public static Task<List<UserPasswordExpirationDetails>> GetUsersWithExpiringPasswordsAsync(
            UserTools userTools,
            string domainName,
            string lastNameStartsWith,
            TimeSpan maxPasswordAge,
            TimeSpan closeToExpirationTimespan)
        {
            

            return userPasswordExpirationDetails;
        }
        */

        /// <summary>
        /// Predicate for filtering out <See cref="UserPasswordExpirationDetails">UserPasswordExpirationDetails</see> objects that are not expiring soon or have already expired.
        /// </summary>
        /// <param name="user"><See cref="UserPasswordExpirationDetails" /></param>
        /// <returns></returns>
        private static bool FilterUsersWithExpiringPasswords(UserPasswordExpirationDetails user)
        {
            if ((user.PasswordIsExpired != true) && (user.PasswordIsExpiringSoon == true))
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