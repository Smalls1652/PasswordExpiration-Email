using System;

namespace PasswordExpiration
{
    namespace Classes
    {
        public class ParsedUserData
        {
            public string SamAccountName { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public DateTime PasswordLastSet { get; set; }
            public DateTime PasswordExpiration { get; set; }
            public TimeSpan PasswordLife { get; set; }
            public TimeSpan PasswordExpiresIn { get; set; }
            public Boolean Expired { get; set; }
            public Boolean ExpiringSoon { get; set; }
        }
    }
}
