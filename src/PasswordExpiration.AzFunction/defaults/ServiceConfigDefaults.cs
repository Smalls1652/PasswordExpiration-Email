using System.Collections.Generic;

namespace PasswordExpiration.AzFunction
{
    using Models.Configs;
    public class ServiceConfigDefaults
    {
        public List<EmailTemplateItem> EmailTemplates
        {
            get => _emailTemplates;
        }
        
        public List<UserSearchConfigItem> SearchConfigs
        {
            get => _searchConfigs;
        }

        private readonly List<EmailTemplateItem> _emailTemplates = new()
        {
            new()
            {
                TemplateId = "8ab99c2e-e142-4060-b1d8-8a845f3d8ed5",
                TemplateName = "Default",
                TemplateHtmlFileName = "email.html",
                IsCustom = false
            }
        };

        private readonly List<UserSearchConfigItem> _searchConfigs = new()
        {
            new()
            {
                Id = "65ef0352-9545-42f8-b70f-6dd21fa8df6b",
                Name = "Default",
                MaxPasswordAge = 90,
                EmailIntervalsEnabled = false,
                EmailIntervalDays = null,
                EmailTemplateId = "8ab99c2e-e142-4060-b1d8-8a845f3d8ed5"
            }
        };
    }
}