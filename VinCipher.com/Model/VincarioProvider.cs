using Infrastructure.APIs.Abstracts;

namespace VinCipher.Model
{
    public class VincarioProvider : BaseApiProvider
    {

        private readonly ConfigurationManager _configManager;
        public VincarioProvider (ConfigurationManager manager)
        {
            _configManager = manager;
            ApiKeyToken = _configManager["Vin:vindecoder:apikey"];
            ApiUrlPrefix = _configManager["Vin:vindecoder:apiPrefix"];
            ApiSecretKey = _configManager["Vin:vindecoder:secretkey"];

        }

    }
}
