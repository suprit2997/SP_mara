using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;

namespace BusinessCentralConnector
{
    internal class BusinessCentralSampension
    {
        // Constants for the Business Central API setup >> should move to a hidden location, eg, datatable in OneStream as parameters
        private const string tenantId = "7cb704f0-946d-416b-9582-4926034e2bb2";
        private const string clientId = "9622551a-2839-4f5a-9c80-3ab87f610cf5";
        private const string clientSecret = "1mP8Q~5EV~uKeD5dHJXHaIGR_2mjThFxO6mOmcwV";
        private const string tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
        private const string scope = "https://api.businesscentral.dynamics.com/.default";
        private const string environment = "Sandbox_SP_Test"; //var environment = "Production";
        private const string baseUrl = $"https://api.businesscentral.dynamics.com/v2.0/{environment}/api/sampension/api/v1.0";
        private Dictionary<string, Type> jsonDef;

        /// <summary>
        /// Returns the fields returned from the datatable
        /// </summary>
        /// <returns></returns>
        public List<string> GetFields()
        {
            // return the key part of the jsonDef
            return new List<string>(jsonDef.Keys);
        }

        public BusinessCentralSampension()
        {
            SetupTableDefinition();
        }

        private void SetupTableDefinition()
        {
            // Create the JSON definition for the API call through the Dictionary object
            jsonDef = new Dictionary<string, Type>
            {
                {"regnskab", typeof(string) },
                { "dimSetId", typeof(int) },
                { "glAccountNo", typeof(string) },
                { "glAccountName", typeof(string) },
                { "incomeBalance", typeof(string) },
                { "postingDate", typeof(string) },
                { "amount", typeof(decimal) },
                { "dimension1", typeof(string) },
                { "dimension1Value", typeof(string) },
                { "dimension2", typeof(string) },
                { "dimension2Value", typeof(string) },
                { "dimension3", typeof(string) },
                { "dimension3Value", typeof(string) },
                { "dimension4", typeof(string) },
                { "dimension4Value", typeof(string) },
                { "dimension5", typeof(string) },
                { "dimension5Value", typeof(string) },
                { "dimension6", typeof(string) },
                { "dimension6Value", typeof(string) },
                { "dimension7", typeof(string) },
                { "dimension7Value", typeof(string) },
                { "dimension8", typeof(string) },
                { "dimension8Value", typeof(string) }
            };
        }

        /// <summary>
        /// Connects to Business Central and retrieves data from the API for the SummarizedGLEntries endpoint.
        /// </summary>
        /// <returns></returns>
        public DataTable GetBusinessCentralData(DateTime fromDate, DateTime toDate, string entityName)
        {
            // Construct the endpoint URL with the OData filter
            var endPoint = $"{baseUrl}/summarizedGLEntries?$expand=dimensionSetEntries";
            var filter = $"&$filter=postingDate gt '{fromDate.ToString("yyyy-MM-dd")}' and postingDate lt '{toDate.ToString("yyyy-MM-dd")}'";
            endPoint += filter;

            DataTable resultTable = new DataTable();
            foreach (var item in jsonDef)
            {
                resultTable.Columns.Add(item.Key, item.Value);
            }

            try
            {
                using (var client = new BusinessCentralApiClient(tokenEndpoint, scope, clientId, clientSecret))
                {
                    // Retrieve data from the specified endpoint URL and add it to the result table
                    var jsonData = client.GetDataAsync(endPoint, new KeyValuePair<string, string>()).Result;
                    var jArray = JArray.Parse(JObject.Parse(jsonData)["value"].ToString());
                    BuildDataTableContent(resultTable, jArray);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new Exception("Error retrieving data from Business Central", ex);
            }

            return resultTable;
        }

        /// <summary>
        /// Build the content of the datatable from the json data
        /// </summary>
        /// <param name="resultTable"></param>
        /// <param name="items"></param>
        private void BuildDataTableContent(DataTable resultTable, JArray items)
        {
            try
            {
                foreach (var item in items)
                {
                    string accountNo = item["glAccountNo"].Value<string>();
                    int dimSetId = item["dimSetId"].Value<int>();
                    string accountName = item["glAccountName"].Value<string>();
                    string incomeBalance = item["incomeBalance"].Value<string>();
                    string postingDate = item["postingDate"].Value<string>();
                    decimal amount = item["amount"].Value<decimal>();

                    // get the nested array of dimensionSetEntries
                    var dimensionSetEntries = item["dimensionSetEntries"].Value<JArray>();

                    var row = resultTable.NewRow();
                    // for each dimensionsetEntry, but only map the dimensionCode like this: "MODPART" > dimension4, "PRODUKT" > dimension2, "PROJEKTER" > dimension3, "SELSKAB" > dimension1
                    foreach (var dimensionSetEntry in dimensionSetEntries)
                    {
                        string dimensionCode = dimensionSetEntry["dimensionCode"].Value<string>();
                        string dimensionValue = dimensionSetEntry["dimensionValue"].Value<string>();
                        switch (dimensionCode)
                        {
                            case "AFDELING":
                                row["dimension1"] = dimensionCode;
                                row["dimension1Value"] = dimensionValue;
                                break;
                            case "PROJEKTER":
                                row["dimension2"] = dimensionCode;
                                row["dimension2Value"] = dimensionValue;
                                break;
                            case "PRODUKT":
                                row["dimension3"] = dimensionCode;
                                row["dimension3Value"] = dimensionValue;
                                break;
                            case "SELSKAB":
                                row["dimension4"] = dimensionCode;
                                row["dimension4Value"] = dimensionValue;
                                break;
                            case "MODPART":
                                row["dimension5"] = dimensionCode;
                                row["dimension5Value"] = dimensionValue;
                                break;
                            default:
                                break;
                        }
                    }
                    // Add a row to the datatable with all the values
                    row["glAccountNo"] = accountNo;
                    row["dimSetId"] = dimSetId;
                    row["glAccountName"] = accountName;
                    row["incomeBalance"] = incomeBalance;
                    row["postingDate"] = postingDate;
                    row["amount"] = amount;
                    resultTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error building data table content", ex);
            }
        }
    }
}
