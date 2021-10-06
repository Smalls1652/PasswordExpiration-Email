using System;
using System.Text.Json.Serialization;

namespace PasswordExpiration.Lib.Models.Core
{
    using PasswordExpiration.Lib.Models.Graph.Users;

    /// <summary>
    /// Hosts information about a user account's password expiration.
    /// </summary>
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

        [JsonPropertyName("id")]
        public string Id
        {
            get => User.Id;
        }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("passwordLastSetDate")]
        public Nullable<DateTimeOffset> PasswordLastSetDate
        {
            get => passwordLastSetDate;
        }
        private Nullable<DateTimeOffset> passwordLastSetDate;

        [JsonPropertyName("passwordLifespan")]
        public Nullable<TimeSpan> PasswordLifespan
        {
            get => passwordLifespan;
        }
        private Nullable<TimeSpan> passwordLifespan;

        [JsonPropertyName("passwordExpirationDate")]
        public Nullable<DateTimeOffset> PasswordExpirationDate
        {
            get => passwordExpirationDate;
        }
        private Nullable<DateTimeOffset> passwordExpirationDate;

        [JsonPropertyName("passwordExpiresIn")]
        public Nullable<TimeSpan> PasswordExpiresIn
        {
            get => passwordExpiresIn;
        }
        private Nullable<TimeSpan> passwordExpiresIn;

        [JsonPropertyName("passwordIsExpired")]
        public bool PasswordIsExpired
        {
            get => passwordIsExpired;
        }
        private bool passwordIsExpired;

        [JsonPropertyName("passwordIsExpiringSoon")]
        public bool PasswordIsExpiringSoon
        {
            get => passwordIsExpiringSoon;
        }
        private bool passwordIsExpiringSoon;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxDays">The amount of days a password can be valid.</param>
        /// <param name="daysRemaining">The amount of days to consider a user's password to be close to expiration.</param>

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