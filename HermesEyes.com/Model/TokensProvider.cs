namespace HermesEyes.com.Model
{
    public class TokensProvider
    {
        private readonly ConfigurationManager _configurationManager;
        public TokensProvider (ConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            Tokens = _configurationManager["Tokens"].Split(',').ToList();
        }
        public List<string> Tokens { get; set; }
        public bool IsValid (string token)
        {
            if (token == null)
            {
                return false;
            }

            return Tokens.Contains(token);
        }
    }
}
