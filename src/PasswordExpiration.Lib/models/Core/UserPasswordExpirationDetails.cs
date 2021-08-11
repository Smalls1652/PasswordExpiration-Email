using System;

namespace PasswordExpiration.Lib.Models.Core
{
    using PasswordExpiration.Lib.Models.Graph.Users;
    public class UserPasswordExpirationDetails
    {
        public UserPasswordExpirationDetails(User user, TimeSpan maxPasswordLife, TimeSpan expiringSoonTimespan)
        {
            User = user;

            if (null != user.LastPasswordChangeDateTime)
            {
                ParsePasswordDetails(maxPasswordLife, expiringSoonTimespan);
            }
        }

        public User User { get; set; }

        public Nullable<DateTimeOffset> PasswordLastSetDate
        {
            get => passwordLastSetDate;
        }
        private Nullable<DateTimeOffset> passwordLastSetDate;

        public Nullable<TimeSpan> PasswordLifespan
        {
            get => passwordLifespan;
        }
        private Nullable<TimeSpan> passwordLifespan;

        public Nullable<DateTimeOffset> PasswordExpirationDate
        {
            get => passwordExpirationDate;
        }
        private Nullable<DateTimeOffset> passwordExpirationDate;

        public Nullable<TimeSpan> PasswordExpiresIn
        {
            get => passwordExpiresIn;
        }
        private Nullable<TimeSpan> passwordExpiresIn;

        public bool PasswordIsExpired
        {
            get => passwordIsExpired;
        }
        private bool passwordIsExpired;

        public bool PasswordIsExpiringSoon
        {
            get => passwordIsExpiringSoon;
        }
        private bool passwordIsExpiringSoon;

        private void ParsePasswordDetails(TimeSpan maxDays, TimeSpan daysRemaining)
        {
            DateTimeOffset currentDateTime = DateTimeOffset.Now;

            this.passwordLastSetDate = this.User.LastPasswordChangeDateTime;
            this.passwordLifespan = (currentDateTime - this.passwordLastSetDate);

            this.passwordExpirationDate = this.passwordLastSetDate.Value.AddDays(maxDays.Days);
            this.passwordExpiresIn = (this.passwordExpirationDate - currentDateTime);

            if (currentDateTime >= this.passwordExpirationDate)
            {
                this.passwordIsExpired = true;
            }

            if (this.passwordExpiresIn.Value.Days <= daysRemaining.Days)
            {
                this.passwordIsExpiringSoon = true;
            }
        }
    }
}