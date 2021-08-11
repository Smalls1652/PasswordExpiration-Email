using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordExpiration.Lib
{
    using Core;
    using Core.Graph;
    using Models.Core;
    using Models.Graph.Users;

    public static class ExpiringPasswordFinder
    {
        public static List<UserPasswordExpirationDetails> GetUsersWithExpiringPasswords(
            UserTools userTools,
            string domainName,
            string lastNameStartsWith,
            TimeSpan maxPasswordAge,
            TimeSpan closeToExpirationTimespan)
        {
            List<User> foundUsers = userTools.GetUsers(domainName, null, lastNameStartsWith);

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