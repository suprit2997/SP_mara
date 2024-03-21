using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Linq.Expressions;

namespace BusinessCentralConnector
{
    /// <summary>
    /// Represents a client for interacting with the Business Central API.
    /// </summary>
    public class BusinessCentralApiClient : IDisposable
    {
        private readonly string _tokenEndpoint;
        private readonly string _resource;
        private readonly HttpClient _httpClient;
        private string? _accessToken;
        private string _scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessCentralApiClient"/> class.
        /// </summary>
        /// <param name="tokenEndpoint">The token endpoint URL.</param>
        /// <param name="resource">The resource URL.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        public BusinessCentralApiClient(string tokenEndpoint, string scope, string clientId, string clientSecret)
        {
            _tokenEndpoint = tokenEndpoint;
            _scope = scope;
            _httpClient = new HttpClient();
            // call the Initialize method
            Initialize(clientId, clientSecret).Wait();

        }

        // Refactor the Conconstructor and create an initialize method to call the GetAccessToken method
        private async Task Initialize(string clientId, string clientSecret)
        {
            var token = await GetAccessToken(clientId, clientSecret);
            _accessToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Gets the access token for authentication.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <returns>The access token.</returns>
        private async Task<string?> GetAccessToken(string clientId, string clientSecret)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("scope", _scope)
            });

            var response = await _httpClient.PostAsync(_tokenEndpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

            return tokenResponse?.AccessToken;
        }

        /// <summary>
        /// Retrieves data from the specified URL and returns it as a DataTable.
        /// </summary>
        /// <param name="url">The URL to retrieve data from.</param>
        /// <param name="columns">A dictionary that maps column names to their data types.</param>
        /// <returns>A DataTable containing the retrieved data.</returns>
        public async Task<DataTable> GetDataAsync(string? url, Dictionary<string, Type> columns)
        {
            var dataTable = new DataTable();
            foreach (var column in columns)
            {
                dataTable.Columns.Add(column.Key, column.Value);
            }
            // Retrieve data from the specified URL and add it to the DataTable.
            while (!string.IsNullOrEmpty(url))
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                // Rest of the code...
                var jObject = JObject.Parse(content);

                // Extract the data from the response and add it to the DataTable.
                // check that jObject is not null
                foreach (var item in jObject["value"].Children())
                {
                    var row = dataTable.NewRow();
                    foreach (var column in columns)
                    {
                        var value = item[column.Key]?.ToString();
                        row[column.Key] = string.IsNullOrEmpty(value) ? DBNull.Value : Convert.ChangeType(value, column.Value);
                    }

                    dataTable.Rows.Add(row);
                }

                url = jObject["@odata.nextLink"]?.ToString();
            }

            return dataTable;
        }

        /// <summary>
        /// Get JSON data from the specified URL and returns it as a string.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> GetDataAsync(string? url, KeyValuePair<string, string> customHeader, string filterExpression)
        {
            if (customHeader.Key != null && customHeader.Value != null)
            {
                if (_httpClient.DefaultRequestHeaders.Contains(customHeader.Key))
                    _httpClient.DefaultRequestHeaders.Remove(customHeader.Key);
                _httpClient.DefaultRequestHeaders.Add(customHeader.Key, customHeader.Value);
            }

            var rawOutput = new StringBuilder();
            // Retrieve data from the specified URL and add it to the DataTable.
            while (!string.IsNullOrEmpty(url))
            {
                string content = string.Empty;
                try
                {
                    Console.WriteLine("filter = " + filterExpression);
                    var requestUrl = $"{url}?{filterExpression}";
                    Console.WriteLine("Getting Data From = " + requestUrl);
                    Console.WriteLine("Getting Data From = " + url);
                    var response = await _httpClient.GetAsync(requestUrl);

                    //var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    content = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    // Add a stringbuilder object to capture log-information
                    throw;
                }

                // Append content to rawOutput if needed
                rawOutput.Append(content);

                var jObject = JObject.Parse(content);
                //Console.WriteLine(jObject.ToString());
                url = jObject["@odata.nextLink"]?.ToString();
            }

            return rawOutput.ToString();
        }

        
        /// <summary>
        /// Disposes the underlying HttpClient instance.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string? AccessToken { get; set; }
        }
    }
}
