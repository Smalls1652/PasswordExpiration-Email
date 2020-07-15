using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

using PasswordExpiration.Helpers.ActiveDirectory.Models;

namespace PasswordExpiration
{
    namespace Helpers
    {
        namespace ActiveDirectory
        {
            public class AccountSearcher
            {
                public AccountSearcher() { }

                public AccountSearcher(string dnsName)
                {
                    ConnectToDomainController(dnsName);
                }

                public AccountSearcher(string dnsName, string ouPath)
                {
                    ConnectToDomainController(dnsName);
                    ChangeSearchRoot(ouPath);
                }

                public AccountSearcher(string domainName, bool isDomain)
                {
                    ConnectToDomain(domainName);
                }

                public AccountSearcher(string domainName, string ouPath, bool isDomain)
                {
                    ConnectToDomain(domainName);
                    ChangeSearchRoot(ouPath);
                }

                public DomainController domainController;
                public DirectorySearcher directorySearcher;
                private string _defaultFilter = "(&(objectCategory=person)(objectClass=user)(!(UserAccountControl:1.2.840.113556.1.4.803:=2))(!(UserAccountControl:1.2.840.113556.1.4.803:=65536)))";
                private int _defaultPageSize = 500000;
                private String[] _propertiesToLoad = new String[] {
                    "givenname",
                    "sn",
                    "userprincipalname",
                    "samaccountname",
                    "pwdlastset",
                    "lastlogontimestamp"
                };

                public void ConnectToDomain(string domainName)
                {
                    DirectoryContext directoryContext = new DirectoryContext(DirectoryContextType.Domain, domainName);

                    Domain domain = Domain.GetDomain(directoryContext);

                    domainController = domain.FindDomainController();

                    directorySearcher = domainController.GetDirectorySearcher();
                    SetDefaultDirectorySearcherOptions();
                }

                public void ConnectToDomainController(string dnsName)
                {
                    DirectoryContext directoryContext = new DirectoryContext(DirectoryContextType.DirectoryServer, dnsName);

                    domainController = DomainController.GetDomainController(directoryContext);

                    directorySearcher = domainController.GetDirectorySearcher();
                    SetDefaultDirectorySearcherOptions();
                }

                private void SetDefaultDirectorySearcherOptions()
                {
                    directorySearcher.Filter = _defaultFilter;
                    directorySearcher.PropertiesToLoad.AddRange(_propertiesToLoad);
                    directorySearcher.PageSize = _defaultPageSize;
                }

                public void ChangeSearchRoot(string ouPath)
                {
                    DirectoryEntry newSearchRoot = new DirectoryEntry($"LDAP://{domainController.Name}/{ouPath}");

                    directorySearcher.SearchRoot = newSearchRoot;
                }

                public List<UserAccount> GetUsers()
                {
                    List<UserAccount> foundUsers = new List<UserAccount>();

                    SearchResultCollection results = directorySearcher.FindAll();

                    foreach (SearchResult item in results)
                    {
                        UserAccount usrObj = new UserAccount();

                        foreach (string propertyName in item.Properties.PropertyNames)
                        {
                            switch (propertyName)
                            {
                                case "samaccountname":
                                    usrObj.UserName = item.Properties[propertyName][0].ToString();
                                    break;

                                case "userprincipalname":
                                    usrObj.UserPrincipalName = item.Properties[propertyName][0].ToString();
                                    break;

                                case "givenname":
                                    usrObj.GivenName = item.Properties[propertyName][0].ToString();
                                    break;

                                case "sn":
                                    usrObj.SurName = item.Properties[propertyName][0].ToString();
                                    break;

                                case "pwdlastset":
                                    usrObj.PasswordLastSet = DateTime.FromFileTime(Convert.ToInt64(item.Properties[propertyName][0].ToString()));
                                    break;

                                case "lastlogontimestamp":
                                    usrObj.LastLogonDate = DateTime.FromFileTime(Convert.ToInt64(item.Properties[propertyName][0].ToString()));
                                    break;

                                default:
                                    break;
                            }
                        }

                        foundUsers.Add(usrObj);
                    }

                    return foundUsers;
                }
            }
        }
    }
}