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

namespace DashboardWeb
{
    /// <summary>
    /// Summary description for Admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
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


        //[WebMethod]
        //public string AddUrl()
        //{

        //    while (!CreatedTableAfterDelete()) { Debug.WriteLine("Creating Table..."); }

        //    CrawledURL url = new CrawledURL("Test", "http://BryantTaylor.info", "This is a test, yo");
        //    TableOperation insertOperation = TableOperation.Insert(url);
        //    Debug.WriteLine("\n\n\t" + url.Title + " -> " + url.URL + "\n\n");
        //    table.Execute(insertOperation);

        //    return "Added URL: " + url.Title + " -> " + url.URL;

        //}

        [WebMethod]
        public List<string> VisitedUrls()
        {
            List<string> answer = new List<string>();

            TableQuery<CrawledURL> rangeQuery = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, DateTimeOffset.Now.AddHours(-2).Date)
                );

            try
            {
                foreach (CrawledURL entity in table.ExecuteQuery(rangeQuery))
                {
                    answer.Add(entity.Title + " at " + entity.URL);
                    Debug.WriteLine(entity.Title + " with " + entity.URL);
                }
            }
            catch (StorageException se) // Ask For Forgiveness
            {
                table.CreateIfNotExists();
                answer = VisitedUrls();
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
