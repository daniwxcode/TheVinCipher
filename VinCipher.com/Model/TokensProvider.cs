using Microsoft.Extensions.Configuration;

namespace VinCipher.Model
{
    public enum AllowedFunction
    {
        Decode,
        Evaluate
    }

    public class TokenInfo
    {
        public string Token { get; set; }
        public List<AllowedFunction> AllowedFunctions { get; set; } = new();
        public DateTime? DateLimite { get; set; }
        public bool IsFunctionAllowed(AllowedFunction function)
        {
            bool isAllowed = AllowedFunctions.Any(_=>_==function);
            return isAllowed;
        }
        public bool IsExpired()
        {
            if (DateLimite is null)
                return false;

            return DateTime.UtcNow > DateLimite.Value;
        }

    }

    public class TokensProvider
    {
        private readonly object _lock = new();
        private readonly List<TokenInfo> _tokens;

        public TokensProvider(ConfigurationManager configurationManager)
        {
            _tokens = configurationManager.GetSection("Tokens").Get<List<TokenInfo>>() ?? [];
        }

        public bool IsValid(string token, out TokenInfo? tokenInfo)
        {
            tokenInfo = null;

            if (string.IsNullOrEmpty(token))
                return false;

            lock (_lock)
            {
                tokenInfo = _tokens.FirstOrDefault(t => t.Token == token);
            }

            return tokenInfo is not null && !tokenInfo.IsExpired();
        }

        /// <summary>
        /// Registers a playground-generated token in the in-memory token list
        /// so that VinDecoderController can validate it.
        /// </summary>
        public void AddPlaygroundToken(string key, DateTime expiresAtUtc)
        {
            lock (_lock)
            {
                _tokens.Add(new TokenInfo
                {
                    Token = key,
                    AllowedFunctions = [AllowedFunction.Decode],
                    DateLimite = expiresAtUtc
                });
            }
        }
    }
}
