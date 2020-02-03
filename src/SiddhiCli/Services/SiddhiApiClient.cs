using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SiddhiCli.Configuration;

namespace SiddhiCli.Services
{
    public class SiddhiApiClient : ISiddhiApiClient
    {
        private readonly SiddhiAppsApiSettings _apiSettings;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<SiddhiApiClient> _logger;

        public SiddhiApiClient(IOptions<SiddhiAppsApiSettings> apiSettings, IHttpClientFactory clientFactory, ILogger<SiddhiApiClient> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _apiSettings = apiSettings.Value;
        }

        public string ExecQuery(string appName, string tableName)
        {
            var payload = $"{{'appName':'{appName}','query': 'from {tableName} select *'}}";
            using var webClient = _clientFactory.CreateClient();
            using var request = CreateRequest("POST", $"{_apiSettings.ApiQueryHost}/stores/query", payload);
            try
            {
                using var response = webClient.SendAsync(request).GetAwaiter().GetResult();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return json;
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Error while requesting URL: {request.RequestUri}");
                throw;
            }
        }

        public string GetActiveApps()
        {
            using var webClient = _clientFactory.CreateClient();
            using var request = CreateRequest("GET", $"{_apiSettings.ApiAppsHost}/siddhi-apps", string.Empty);
            try
            {
                using var response = webClient.SendAsync(request).GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode) return "There was an error.";

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return json;
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Error while requesting URL: {request.RequestUri}");
                throw;
            }
        }

        public string Deploy(string file)
        {
            var payload = File.ReadAllText(file);
            using var webClient = _clientFactory.CreateClient();
            using var request = CreateRequest("PUT", $"{_apiSettings.ApiAppsHost}/siddhi-apps", payload, MediaTypeNames.Text.Plain);
            try
            {
                using var response = webClient.SendAsync(request).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode) return "Deployed.";

                var textResult = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var json = JObject.Parse(textResult);

                return $"ERROR: {json["message"]}";
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Error while requesting URL: {request.RequestUri}");
                throw;
            }
        }

        public string Delete(string appName)
        {
            using var webClient = _clientFactory.CreateClient();
            using var request = CreateRequest("DELETE", $"{_apiSettings.ApiAppsHost}/siddhi-apps/{appName}", string.Empty);
            try
            {
                using var response = webClient.SendAsync(request).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode) return "Deleted.";

                var textResult = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var json = JObject.Parse(textResult);

                return $"ERROR: {json["message"]}";
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Error while requesting URL: {request.RequestUri}");
                throw;
            }
        }

        private HttpRequestMessage CreateRequest(string method, string endpoint, string payload, string mediaType = MediaTypeNames.Application.Json)
        {
            var httpMethod = new HttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, new Uri(endpoint))
            {
                Content = new StringContent(payload, Encoding.UTF8, mediaType),
                Version = HttpVersion.Version11
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _apiSettings.Authorization);
            return request;
        }
    }
}