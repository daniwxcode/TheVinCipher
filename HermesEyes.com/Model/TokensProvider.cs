using Microsoft.Extensions.Configuration;

namespace HermesEyes.com.Model
{
    public enum AllowedFunction
    {
        Decode,
        Evaluate
    }

    public class TokenInfo
    {
        public string Token { get; set; }
        public bool IsFunctionAllowed(AllowedFunction function)
        {
            return AllowedFunctions.Contains(function);
        }
        public List<AllowedFunction> AllowedFunctions { get; set; } = new();
    }

    public class TokensProvider
    {
        private readonly ConfigurationManager _configurationManager;
        public TokensProvider (ConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            Tokens = Tokens = _configurationManager.GetSection("Tokens").Get<List<TokenInfo>>() ?? new List<TokenInfo>();
        }
        private List<TokenInfo> Tokens { get; set; } = new();

        public bool IsValid(string token, out TokenInfo? tokenInfo)
        {
            tokenInfo = null;

            if (string.IsNullOrEmpty(token))
                return false;

            tokenInfo = Tokens.FirstOrDefault(t => t.Token == token);
            return tokenInfo != null;
        }


    }
}
