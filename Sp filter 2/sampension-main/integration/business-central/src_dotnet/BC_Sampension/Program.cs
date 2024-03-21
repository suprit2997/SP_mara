// See https://aka.ms/new-console-template for more information
using BusinessCentralConnector;
using System;
using System.Collections.Generic;
using System.Data;

public class Program {
    static void Main(string[] args) {
        Console.WriteLine("SAMPENSION Business Central API Test");
        Console.WriteLine("Connecting to Business Central...");

        // initiate a new BusinessCentralSampension object
        var bc = new BusinessCentralSampension();
        // get the field names
        List<string> fields = bc.GetFields();

        // defined from and to dates
        var fromDate = new DateTime(2023, 1, 1);
        var toDate = new DateTime(2024, 12, 31);

        // Call the GetBusinessCentralData to retrieve data from Business Central for summarizedGLAccount's (I assume for now that is what we need)
        DataTable dt = bc.GetBusinessCentralData(fromDate, toDate);

        Console.WriteLine("Data retrieved from Business Central:");
        Console.WriteLine(dt.Rows.Count + " rows retrieved");

        // Print out each row in the DataTable
        foreach (DataRow row in dt.Rows)
        {
            Console.WriteLine(row["glAccountNo"] + " " + row["dimSetId"] + " " + row["glAccountName"] + " " + row["incomeBalance"] + " " + row["postingDate"] + " " + row["amount"] + " " + row["dimension1"] + " " + row["dimension1Value"] + " " + row["dimension2"] + " " + row["dimension2Value"] + " " + row["dimension3"] + " " + row["dimension3Value"] + " " + row["dimension4"] + " " + row["dimension4Value"] + " " + row["dimension5"] + " " + row["dimension5Value"] + " " + row["dimension6"] + " " + row["dimension6Value"] + " " + row["dimension7"] + " " + row["dimension7Value"] + " " + row["dimension8"] + " " + row["dimension8Value"]);
        }
    }
}
