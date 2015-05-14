using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using CrawlingLibrary;
using System.Xml.Linq;

namespace CrawlerWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);


        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue queue = queueClient.GetQueueReference("todo");

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("urls");

        public override void Run()
        {
            Trace.TraceInformation("CrawlerWorker is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            // Retrieve storage account from connection string


            // Create the queue client
            queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue
            queue = queueClient.GetQueueReference("test");

            // Get the next message

            //Process the message in less than 30 seconds, and then delete the message

            bool result = base.OnStart();

            Trace.TraceInformation("CrawlerWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("CrawlerWorker is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("CrawlerWorker has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");

                int? cachedMessageCount = queue.ApproximateMessageCount;
                Trace.TraceInformation("Count: " + cachedMessageCount.ToString());
                if (cachedMessageCount > 0)
                {
                    CloudQueueMessage url = queue.GetMessage();
                    Debug.WriteLine("\n\n\t" + url.AsString + "\n\n\t");
                    CrawledURL info = CrawlURL(url.AsString);
                    queue.DeleteMessage(url);
                }
                else
                {
                    Trace.TraceInformation("Queue was empty");
                }
                await Task.Delay(1000);
            }
        }

        private CrawledURL CrawlURL(string url)
        {
            if (/* Make Sure Subdomain and Domain are correct*/ true) {
                string html = GetHTML(url);
                XElement xmlTree = XElement.Parse(html);

                //var title = xmlTree.Descendants()
                  //  .Where()

                return new CrawledURL();
            }
            else
            {
                return null;
            }
        }

        private string GetHTML(string url) {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        private void StoreURL(string Title, string URL, string Body)
        {
            CrawledURL url = new CrawledURL(Title, URL, Body);
            TableOperation insertOperation = TableOperation.Insert(url);
            Debug.WriteLine("\n\n\t" + url.Title + " -> " + url.URL + "\n\n");
            table.Execute(insertOperation);
        }

    }
}
