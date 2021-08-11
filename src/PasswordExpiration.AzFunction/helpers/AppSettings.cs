using System;

namespace PasswordExpiration.AzFunction.Helpers
{
    public static class AppSettings
    {
        public static string GetSetting(string settingName)
        {
            string settingValue = null;

            try
            {
                settingValue = Environment.GetEnvironmentVariable(
                    settingName,
                    EnvironmentVariableTarget.Process
                );
            }
            catch (ArgumentNullException e)
            {
                throw new Exception($"The value for setting '{settingName}' is null.", e);
            }

            return settingValue;
        }
    }
}