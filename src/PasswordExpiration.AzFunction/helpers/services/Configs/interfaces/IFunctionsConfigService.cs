using System;
using System.Collections.Generic;

namespace PasswordExpiration.AzFunction.Helpers.Services
{
    using AzFunction.Models.Configs;
    public interface IFunctionsConfigService
    {
        List<EmailTemplateItem> EmailTemplates { get; set; }

        List<UserSearchConfigItem> UserSearchConfigs { get; set; }

        List<string> AutomatedSearchConfigs { get; set; }

        string GetEmailTemplateHtmlFileFullPath(EmailTemplateItem template);

        List<string> GetEmailTemplateAttachmentFileFullPath(EmailTemplateItem template);

        EmailTemplateItem GetEmailTemplate(string templateId);

        UserSearchConfigItem GetUserSearchConfig(string configId);
    }
}