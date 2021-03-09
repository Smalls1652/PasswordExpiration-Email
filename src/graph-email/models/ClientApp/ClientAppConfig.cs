using System.Security.Cryptography.X509Certificates;

namespace graph_email.Models.ClientApp
{
    public class ClientAppConfig
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string ClientSecret { get; set; }
        public X509Certificate2 ClientCertificate { get; set; }
    }
}