using System;
using System.IO;
using System.Globalization;

namespace PasswordExpiration.Lib.Core
{
    using PasswordExpiration.Lib.Models.Core;
    public static class MailBodyGenerator
    {
        
        public static string CreateMailGenerator(UserPasswordExpirationDetails userPwDetailsObj, string htmlTemplatePath, TimeZoneInfo timeZone)
        {
            string mailBody = null;

            DateTime currentDateTime = DateTime.Now;

            if (timeZone == null)
            {
                timeZone = TimeZoneInfo.Utc;
            }

            TimeSpan timeZoneOffset = timeZone.GetUtcOffset(currentDateTime);

            StreamReader htmlTemplateFileReader = new StreamReader(htmlTemplatePath);

            string mailBodyTemplate = htmlTemplateFileReader.ReadToEnd();

            htmlTemplateFileReader.Close();
            htmlTemplateFileReader.Dispose();

            mailBody = mailBodyTemplate
                .Replace("$USERNAME", userPwDetailsObj.User.DisplayName)
                .Replace("$EXPIREINDAYS", userPwDetailsObj.PasswordExpiresIn.Value.Days.ToString("00"))
                .Replace("$EXPIREDATE", userPwDetailsObj.PasswordExpirationDate.Value.ToOffset(timeZoneOffset).ToString("MM/dd/yyyy hh:mm tt zzzz"));

            return mailBody;
        }
    }
}