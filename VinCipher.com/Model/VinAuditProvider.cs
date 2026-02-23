using Infrastructure.APIs.Abstracts;

namespace VinCipher.Model
{
    public class VinAuditProvider : BaseApiProvider
    {

        private readonly ConfigurationManager _configManager;
        public VinAuditProvider (ConfigurationManager manager)
        {
            _configManager = manager;
            ApiKeyToken = _configManager["Vin:VinAudit:apikey"];
            ApiUrlPrefix = _configManager["Vin:VinAudit:apiPrefix"];
            ApiSecretKey = _configManager["Vin:VinAudit:secretkey"];

        }

    }
}
