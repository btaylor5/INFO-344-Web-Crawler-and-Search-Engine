using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlingLibrary
{
    public class TableCommunication
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("urls");


        public static void InitializeCommunication()
        {
            table.CreateIfNotExists();
        }


        public static void VisitedUrl(CrawledURL url)
        {
            TableOperation insertOperation = TableOperation.Insert(url);
            try
            {
                Debug.WriteLine("\n\n\t" + url.Title + " -> " + url.URL + "\n\n");
                table.Execute(insertOperation);

            }
            catch (StorageException se)
            {
                Debug.WriteLine("Error!!!!: " + se.Message + "   " + se.Data + "\n");
                Debug.WriteLine("Caught Table Insert Error: " + url.PartitionKey + " " + url.RowKey + " " + url.Date + " " + url.URL);
            }
        }

        public static bool CrawledYet(string url)
        {
            TableQuery<CrawledURL> visitedUrl = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("URL", QueryComparisons.Equal, url)
                    );

            var results = table.ExecuteQuery(visitedUrl).ToList();
            bool result = results.Count > 0;
            if (result) {
                Debug.WriteLine("Table Collision, Crawled Yet returning false");
            }
            return result;
        }

        public static List<string> LastTenVisitedUrls()
        {
            List<string> answer = new List<string>();

            TableQuery<CrawledURL> rangeQuery = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, CrawledURL.ChronoCompareString())
                    ).Take(10);

            try
            {
                foreach (CrawledURL entity in table.ExecuteQuery(rangeQuery))
                {
                    string combo = "[" + entity.URL + "] -> [" + entity.Title + "] containing a body of [" + entity.Body + "]";
                    answer.Add(combo);
                    Debug.WriteLine(combo);
                }
            }
            catch (StorageException se) // Ask For Forgiveness
            {
                if (se.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableNotFound))
                {
                    table.CreateIfNotExists();
                    answer = LastTenVisitedUrls();
                }
                else
                {
                    throw;
                }
            }

            return answer;
        }

        public static bool RemoveURLHistory()
        {
            table.DeleteIfExists();

            while (!CreatedTableAfterDelete()) { }
            return true;
        }

        private static bool CreatedTableAfterDelete()
        {
            try
            {
                table.CreateIfNotExists();
                return true;
            }
            catch (StorageException e)
            {
                if ((e.RequestInformation.HttpStatusCode == 409) && (e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted)))
                {
                    Thread.Sleep(1000);// The table is currently being deleted. Try again until it works.
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
