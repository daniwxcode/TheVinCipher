namespace Infrastructure.APIs.Abstracts
{
    public class BaseApiProvider
    {
        public BaseApiProvider ()
        {

        }
        public string ApiKeyToken { get; set; }
        public string ApiUrlPrefix { get; set; }
        public string? ApiSecretKey { get; set; }

    }
}
