using System.Net.Http;

namespace SiddhiCli.Services
{
    public class UntrustedHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var httpClientHandler = new HttpClientHandler
            {
                // bypassing SSL certificate verification
                // so request can be done via HTTPS when there is no SSL certificate for the domain
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            return new HttpClient(httpClientHandler);
        }
    }
}