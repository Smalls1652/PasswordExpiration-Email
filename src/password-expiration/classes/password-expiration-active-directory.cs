using System;

namespace PasswordExpiration
{
    namespace Helpers
    {
        namespace ActiveDirectory
        {
            namespace Models
            {
                public class UserAccount
                {
                    public string UserName { get; set; }
                    public string UserPrincipalName { get; set; }
                    public string GivenName { get; set; }
                    public string SurName { get; set; }
                    public Nullable<DateTime> LastLogonDate { get; set; }
                    public Nullable<DateTime> PasswordLastSet { get; set; }
                }
            }
        }
    }
}