using System;
using System.IO;

namespace PasswordExpiration.Lib.Core
{
    using PasswordExpiration.Lib.Models.Core;
    public static class MailBodyGenerator
    {
        public static string CreateMailGenerator(UserPasswordExpirationDetails userPwDetailsObj, string htmlTemplatePath)
        {
            string mailBody = null;

            StreamReader htmlTemplateFileReader = new StreamReader(htmlTemplatePath);

            string mailBodyTemplate = htmlTemplateFileReader.ReadToEnd();

            htmlTemplateFileReader.Close();
            htmlTemplateFileReader.Dispose();

            mailBody = mailBodyTemplate
                .Replace("$USERNAME", userPwDetailsObj.User.DisplayName)
                .Replace("$EXPIREINDAYS", userPwDetailsObj.PasswordExpiresIn.Value.Days.ToString("00"))
                .Replace("$EXPIREDATE", userPwDetailsObj.PasswordExpirationDate.Value.ToString("MM/dd/yyyy hh:mm tt zzzz"));

            return mailBody;
        }
    }
}