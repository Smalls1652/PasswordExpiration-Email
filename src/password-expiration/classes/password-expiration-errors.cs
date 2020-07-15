using System;
using PasswordExpiration.Helpers.ActiveDirectory.Models;

namespace PasswordExpiration
{
    namespace Classes
    {
        namespace Errors
        {
            [Serializable]
            public class PasswordExpirationParseException : Exception
            {
                public UserAccount UserAccount { get; set; }

                public PasswordExpirationParseException(string message) : base(message) { }
                public PasswordExpirationParseException(string message, Exception inner) : base(message, inner) { }
                public PasswordExpirationParseException(string message, UserAccount userAccount, Exception inner) : this(message, inner)
                {
                    UserAccount = userAccount;

                }
            }
        }
    }
}