﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BusinessCentralConnector
{
    internal class BusinessCentralSampension
    {

        // Constants for the Business Central API setup >> should move to a hidden location, eg, datatable in OneStream as parameters
        private const string tenantId = "7cb704f0-946d-416b-9582-4926034e2bb2";

        private const string clientId = "9622551a-2839-4f5a-9c80-3ab87f610cf5";

        private const string clientSecret = "1mP8Q~5EV~uKeD5dHJXHaIGR_2mjThFxO6mOmcwV";

        private string tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

        private const string scope = "https://api.businesscentral.dynamics.com/.default";

        private const string environment = "Sandbox_SP_Test";
        //var environment = "Production";
        private string baseUrl = $"https://api.businesscentral.dynamics.com/v2.0/{environment}/api/sampension/api/v1.0";

        private Dictionary<string, Type> jsonDef;


        /// <summary>
        /// Returns the fields returned from the datatable
        /// </summary>
        /// <returns></returns>
        public List<string> GetFields()
        {
            // return the key part of the jsonDef
            return jsonDef.Keys.ToList();
        }

        public BusinessCentralSampension()
        {
            SetupTableDefinition();
        }

        private void SetupTableDefinition()
        {
            // Create the JSON definition for the API call through the Dicionary object
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
                { "dimension1Value",typeof(string) },
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
        public DataTable GetBusinessCentralData(DateTime fromDate, DateTime toDate)
        {
            // create a datatable with the jsonDef
            DataTable resultTable = new DataTable();
            foreach (var item in jsonDef)
            {
                resultTable.Columns.Add(item.Key, item.Value);
            }

            var jsonEndpoints = new Dictionary<string, Type>
            {
                { "name", typeof(string) },
                { "value", typeof(string) }
            };

            // create the endpoint for the API call
            var endPoint = baseUrl;
            try
            {
                // create a new instance of the BusinessCentralApiClient
                // fix the using part of the BusinessCentralApiClient
                using (var client = new BusinessCentralApiClient(tokenEndpoint, scope, clientId, clientSecret))
                {
                    // Get the companies that exists within the environment, to get all related data across different regnskaber
                    var companies = GetCompanies(client);

                    KeyValuePair<string, string> companyHeader = new KeyValuePair<string, string>();
                    foreach (var company in companies)
                    {
                        companyHeader = new KeyValuePair<string, string>("Company", company);
                        endPoint = baseUrl + $"/summarizedGLEntries?$expand=dimensionSetEntries&$filter=postingdate gt '{fromDate.ToString("yyyy-MM-dd")}' and postingdate lt '{toDate.ToString("yyyy-MM-dd")}'";
                        //var jsonRaw = JObject.Parse(client.GetDataAsync(endPoint, companyHeader).Result);
                        // convert the value part of the jsonRaw to a JArray
                        var jsonData = client.GetDataAsync(endPoint, companyHeader).Result;
                        // convert the value part of the jsonRaw to a JArray
                        var jArray = JArray.Parse(JObject.Parse(jsonData)["value"].ToString());
                        BuildDataTableContent(resultTable, company, jArray);
                    }
                };
            }
            catch (Exception ex)
            {
                throw;
            }
            return resultTable;
        }

        /// <summary>
        /// Build the content of the datatable from the json data
        /// </summary>
        /// <param name="resultTable"></param>
        /// <param name="regnskab"></param>
        /// <param name="items"></param>
        private void BuildDataTableContent(DataTable resultTable, string regnskab, JArray items)
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
                    // for each dimensionsetEntry, but only map the dimensionCode like this: "MODPART" > dimenaion4, "PRODUKT" > dimension2, "PROJEKTER" > dimension3, "SELSKAB" > dimension1
                    foreach (var dimensionSetEntry in dimensionSetEntries)
                    {
                        string dimensionCode = dimensionSetEntry["dimensionCode"].Value<string>();
                        string dimensionValue = dimensionSetEntry["dimensionValue"].Value<string>();
                        switch (dimensionCode)
                        {
                            case "MODPART":
                                row["dimension4"] = dimensionCode;
                                row["dimension4Value"] = dimensionValue;
                                break;
                            case "PRODUKT":
                                row["dimension2"] = dimensionCode;
                                row["dimension2Value"] = dimensionValue;
                                break;
                            case "PROJEKTER":
                                row["dimension3"] = dimensionCode;
                                row["dimension3Value"] = dimensionValue;
                                break;
                            case "SELSKAB":
                                row["dimension1"] = dimensionCode;
                                row["dimension1Value"] = dimensionValue;
                                break;
                            default:
                                break;
                        }
                    }
                    // Add a row to the datatable with all the values
                    row["regnskab"] = regnskab;
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
                throw;
            }
        }

        /// <summary>
        /// For Sampension they have multiple companies that all report for same afdelinger, so we need to get the data from all companies
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public List<string> GetCompanies(BusinessCentralApiClient client)
        {
            List<string> companies;
            try
            {
                var jsonString = client.GetDataAsync(baseUrl + "/companies", new KeyValuePair<string, string>()).Result;
                var jsonRaw = JObject.Parse(client.GetDataAsync(baseUrl + "/companies", new KeyValuePair<string, string>()).Result);
                var jsonCompanies = JArray.Parse(jsonRaw["value"].ToString());
                // convert the json string to a list of objects
                companies = jsonCompanies.Select(item => item["name"].Value<string>()).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
            return companies;
        }

    }
    public interface IBCLogger
    {
        void Debug(string message);
        void Error(Exception exception, string message);
    }

    public class BCLogger : IBCLogger
    {
        public void Debug(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]:[DEBUG]:{message}");
        }

        public void Error(Exception exception, string message)
        {
            Console.WriteLine(message);
            Console.WriteLine(exception.Message);
        }
    }

    // Create a new BClogger class that uses the OneStream Brapi.Log.LogMessage method
}
