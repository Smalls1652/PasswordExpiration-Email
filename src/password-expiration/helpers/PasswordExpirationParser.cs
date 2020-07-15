using System;
using System.Collections.Generic;
using PasswordExpiration.Classes;
using PasswordExpiration.Helpers.ActiveDirectory.Models;
using PasswordExpiration.Classes.Errors;

namespace PasswordExpiration
{
    namespace Helpers
    {
        public class UserParser
        {
            public ParsedUserData ParseData(UserAccount userAccount, int maxAlive, int daysRemaining)
            {
                DateTime currentDateTime = DateTime.Now;

                DateTime passwordLastSet;
                DateTime passwordExpirationDate;
                TimeSpan passwordLifeSpan;
                TimeSpan passwordExpiresIn;

                passwordLastSet = userAccount.PasswordLastSet.Value;
                try
                {
                    passwordExpirationDate = passwordLastSet.AddDays(maxAlive);
                }
                catch (InvalidOperationException)
                {
                    passwordExpirationDate = new DateTime(1970, 1, 1);
                    passwordLastSet = new DateTime(1970, 1, 1);
                }

                passwordLifeSpan = new TimeSpan(currentDateTime.Ticks - passwordLastSet.Ticks);
                passwordExpiresIn = new TimeSpan(passwordExpirationDate.Ticks - currentDateTime.Ticks);

                Boolean isExpiringSoon;
                switch (passwordExpiresIn.Days <= daysRemaining)
                {
                    case false:
                        isExpiringSoon = false;
                        break;

                    default:
                        isExpiringSoon = true;
                        break;
                }

                Boolean isExpired;
                switch (passwordExpiresIn.Ticks <= 0)
                {
                    case true:
                        isExpired = true;
                        break;

                    default:
                        isExpired = false;
                        break;
                }

                ParsedUserData parsedUserData = new ParsedUserData
                {
                    UserName = userAccount.UserName,
                    UserPrincipalName = userAccount.UserPrincipalName,
                    DisplayName = $"{userAccount.GivenName} {userAccount.SurName}",
                    LastLogonDate = userAccount.LastLogonDate.Value,
                    PasswordLastSet = passwordLastSet,
                    PasswordExpiration = passwordExpirationDate,
                    PasswordLife = passwordLifeSpan,
                    PasswordExpiresIn = passwordExpiresIn,
                    Expired = isExpired,
                    ExpiringSoon = isExpiringSoon
                };


                return parsedUserData;
            }
        }
    }
}