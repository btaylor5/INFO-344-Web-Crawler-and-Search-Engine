using CrawlingLibrary;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Services;

namespace Controller
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class admin : System.Web.Services.WebService
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);


        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue queue = queueClient.GetQueueReference("todo");

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("urls");


        [WebMethod]
        public void AddMessage(string input)
        {
            queue.CreateIfNotExists();
            CloudQueueMessage message = new CloudQueueMessage(input);
            queue.AddMessage(message);
        }


        [WebMethod]
        public string AddUrl()
        {

            while (!CreatedTableAfterDelete()) { Debug.WriteLine("Creating Table..."); }

            CrawledURL url = new CrawledURL("Test", "http://BryantTaylor.info", "This is a test, yo");
            TableOperation insertOperation = TableOperation.Insert(url);
            Debug.WriteLine("\n\n\t" + url.Title + " -> " + url.URL + "\n\n");
            table.Execute(insertOperation);

            return "Added URL: " + url.Title + " -> " + url.URL;

        }

        [WebMethod]
        public List<string> LastTenVisitedUrls()
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
                if (se.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableNotFound)) {
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

        [WebMethod]
        public string RemoveURLHistory()
        {
            table.DeleteIfExists();

            while (!CreatedTableAfterDelete()) { }
            return "History Was Removed";

        }

        [WebMethod]
        public string StartCrawling()
        {
            queue.CreateIfNotExists();

            string url = "http://BryantTaylor.info";
            CloudQueueMessage message = new CloudQueueMessage(url);
            queue.AddMessage(message);

            return "Started Crawling";
        }

        private bool CreatedTableAfterDelete()
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
