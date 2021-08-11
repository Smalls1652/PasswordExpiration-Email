using System;

namespace PasswordExpiration.AzFunction.Helpers
{
    /// <summary>
    /// Hosts methods to get Azure Function app settings.
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// Get the value of a setting configured in the Azure Function app settings.
        /// </summary>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>A string value of the setting.</returns>
        public static string GetSetting(string settingName)
        {
            string settingValue;
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