using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PasswordExpiration.AzFunction.Helpers.Services
{
    using AzFunction.Models.Configs;
    using Lib.Core;

    public class FunctionsConfigService : IFunctionsConfigService
    {
        public FunctionsConfigService(ILogger<FunctionsConfigService> _logger)
        {
            logger = _logger;

            EmailTemplates = new();
            UserSearchConfigs = new();
            AutomatedSearchConfigs = new();

            InitServiceSettings();
        }

        public List<EmailTemplateItem> EmailTemplates { get; set; }

        public List<UserSearchConfigItem> UserSearchConfigs { get; set; }

        public List<string> AutomatedSearchConfigs { get; set; }

        private readonly ILogger<FunctionsConfigService> logger;

        private readonly string _defaultEmailFilesPath = Path.Join(Environment.CurrentDirectory, "defaults\\email-templates\\");

        private readonly string _customEmailFilesPath = Path.Join(Environment.CurrentDirectory, "_email-files\\");

        private void InitServiceSettings()
        {
            logger.LogInformation("Initalizing service config.");

            string svcDir = Environment.CurrentDirectory;

            logger.LogInformation("Loading the default configs...");
            string defaultEmailTemplatesPath = Path.Join(svcDir, "defaults\\email-templates\\email-templates.json");
            string defaultSearchConfigsPath = Path.Join(svcDir, "defaults\\search-configs\\user-search-configs.json");

            EmailTemplates.AddRange(ReadConfigFile<EmailTemplateCollection>(defaultEmailTemplatesPath).Templates);
            UserSearchConfigs.AddRange(ReadConfigFile<UserSearchConfigCollection>(defaultSearchConfigsPath).SearchConfigs);

            string userEmailTemplatesPath = Path.Join(svcDir, "_service-config\\email-templates.json");
            try
            {
                EmailTemplates.AddRange(ReadConfigFile<EmailTemplateCollection>(userEmailTemplatesPath).Templates);
            }
            catch (FileNotFoundException)
            {
                logger.LogWarning("No custom email templates config file was found.");
            }

            string userSearchConfigsPath = Path.Join(svcDir, "_service-config\\user-search-configs.json");
            try
            {
                UserSearchConfigs.AddRange(ReadConfigFile<UserSearchConfigCollection>(userSearchConfigsPath).SearchConfigs);
            }
            catch (FileNotFoundException)
            {
                logger.LogWarning("No custom user search configs file was found.");
            }

            string automatedSearchesConfigPath = Path.Join(svcDir, "_service-config\\automated-searches.json");
            try
            {
                AutomatedSearchConfigs.AddRange(ReadConfigFile<AutomatedSearchConfigCollection>(automatedSearchesConfigPath).AutomatedSearchConfigs);
            }
            catch (FileNotFoundException)
            {
                logger.LogWarning("No custom automated search config file was found.");
            }

            List<string> emailTemplatesLoaded = GetEmailTemplateNames();
            logger.LogInformation($"Email Template configs loaded:\n\t- {string.Join("\n\t- ", emailTemplatesLoaded)}");

            List<string> searchConfigsLoaded = GetUserSearchConfigNames();
            logger.LogInformation($"User Search configs loaded:\n\t- {string.Join("\n\t- ", searchConfigsLoaded)}");
        }

        public EmailTemplateItem GetEmailTemplate(string templateId)
        {
            EmailTemplateItem foundItem;

            foundItem = EmailTemplates.Find(
                (EmailTemplateItem item) => item.TemplateId == templateId
            );

            return foundItem;
        }

        public UserSearchConfigItem GetUserSearchConfig(string configId)
        {
            UserSearchConfigItem foundItem;

            foundItem = UserSearchConfigs.Find(
                (UserSearchConfigItem item) => item.Id == configId
            );

            return foundItem;
        }

        public string GetEmailTemplateHtmlFileFullPath(EmailTemplateItem template)
        {
            string filePath;
            if (template.IsCustom)
            {
                filePath = Path.Join(_customEmailFilesPath, template.TemplateHtmlFileName);
            }
            else
            {
                filePath = Path.Join(_defaultEmailFilesPath, template.TemplateHtmlFileName);
            }

            return filePath;
        }

        public List<string> GetEmailTemplateAttachmentFileFullPath(EmailTemplateItem template)
        {
            List<string> filePaths = new();
            foreach (string attachmentItem in template.IncludedAttachments)
            {
                if (template.IsCustom)
                {
                    filePaths.Add(Path.Join(_customEmailFilesPath, attachmentItem));
                }
                else
                {
                    filePaths.Add(Path.Join(_defaultEmailFilesPath, attachmentItem));
                }
            }

            return filePaths;
        }

        private static T ReadConfigFile<T>(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            if (fileInfo.Exists == false)
            {
                throw new FileNotFoundException(
                    $"'{filePath}' was not found."
                );
            }
            else
            {
                StreamReader configFileReader = new(filePath);
                string configFileContents = configFileReader.ReadToEnd();
                configFileReader.Dispose();

                return JsonConverter.ConvertFromJson<T>(configFileContents);
            }
        }

        private List<string> GetEmailTemplateNames()
        {
            List<string> templateNames = new();
            foreach(EmailTemplateItem item in EmailTemplates)
                templateNames.Add($"{item.TemplateName} ({item.TemplateId})");

            return templateNames;
        }

        private List<string> GetUserSearchConfigNames()
        {
            List<string> configNames = new();
            foreach(UserSearchConfigItem item in UserSearchConfigs)
                configNames.Add($"{item.Name} ({item.Id})");

            return configNames;
        }
    }
}