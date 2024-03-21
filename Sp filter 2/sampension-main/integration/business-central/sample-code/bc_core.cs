using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        private string _accessToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessCentralApiClient"/> class.
        /// </summary>
        /// <param name="tokenEndpoint">The token endpoint URL.</param>
        /// <param name="resource">The resource URL.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        public BusinessCentralApiClient(string tokenEndpoint, string resource, string clientId, string clientSecret)
        {
            _tokenEndpoint = tokenEndpoint;
            _resource = resource;
            _httpClient = new HttpClient();

            var token = GetAccessToken(clientId, clientSecret).GetAwaiter().GetResult();
            _accessToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        /// <summary>
        /// Gets the access token for authentication.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <returns>The access token.</returns>
        private async Task<string> GetAccessToken(string clientId, string clientSecret)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("resource", _resource)
            });

            var response = await _httpClient.PostAsync(_tokenEndpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

            return tokenResponse?.AccessToken;
        }
7/
        /// <summary>
        /// Retrieves data from the specified URL and returns it as a DataTable.
        /// </summary>
        /// <param name="url">The URL to retrieve data from.</param>
        /// <param name="columns">A dictionary that maps column names to their data types.</param>
        /// <returns>A DataTable containing the retrieved data.</returns>
        public async Task<DataTable> GetDataAsync(string url, Dictionary<string, Type> columns)
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
                var jObject = JObject.Parse(content);

                // Extract the data from the response and add it to the DataTable.
                var responseData = jObject["value"].Children();
                foreach (var item in responseData)
                {
                    var row = dataTable.NewRow();
// Convert the data to the appropriate type and add it to the DataTable.
                    foreach (var column in columns)c
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
        /// Disposes the underlying HttpClient instance.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }
    }
}
