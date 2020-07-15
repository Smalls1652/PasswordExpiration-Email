using System;

namespace PasswordExpiration
{
    namespace Classes
    {
        public class ParsedUserData
        {
            public ParsedUserData() { }

            public ParsedUserData(string _UserName, string _UserPrincipalName, string _DisplayName, Nullable<DateTime> _LastLogonDate, Nullable<DateTime> _PasswordLastSet, Nullable<DateTime> _PasswordExpiration, Nullable<TimeSpan> _PasswordLife, Nullable<TimeSpan> _PasswordExpiresIn, Boolean _Expired, Boolean _ExpiringSoon)
            {
                UserName = _UserName;
                UserPrincipalName = _UserPrincipalName;
                DisplayName = _DisplayName;
                LastLogonDate = _LastLogonDate;
                PasswordLastSet = _PasswordLastSet;
                PasswordExpiration = _PasswordExpiration;
                PasswordLife = _PasswordLife;
                PasswordExpiresIn = _PasswordExpiresIn;
                Expired = _Expired;
                ExpiringSoon = _ExpiringSoon;
            }

            public string UserName { get; set; }
            public string UserPrincipalName { get; set; }
            public string DisplayName { get; set; }
            public Nullable<DateTime> LastLogonDate { get; set; }
            public Nullable<DateTime> PasswordLastSet { get; set; }
            public Nullable<DateTime> PasswordExpiration { get; set; }
            public Nullable<TimeSpan> PasswordLife { get; set; }
            public Nullable<TimeSpan> PasswordExpiresIn { get; set; }
            public Boolean Expired { get; set; }
            public Boolean ExpiringSoon { get; set; }
        }
    }
}
