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
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace Crawler
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private WebCrawler crawler = new WebCrawler(new string[] { "cnn.com" });

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
            QueueCommunication.InitializeCommunication();
            TableCommunication.InitializeCommunication();

            bool root = true;
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {

                int? cachedMessageCount = QueueCommunication.MessageCount();
                if (cachedMessageCount > 0)
                {
                    
                    CloudQueueMessage urlMessage = QueueCommunication.GetMessage();
                    string url = urlMessage.AsString;
                    Debug.WriteLine("\n\n\t" + url + "\n\n\t");

                    
                    if (root)
                    {
                        crawler.PrepareCrawlOfSite(url);
                        root = false;
                    }

                    if (!crawler.Disallowed(url) && !crawler.Visited(url))
                    {
                        CrawledURL info = crawler.CrawlURL(url);
                        TableCommunication.VisitedUrl(info);
                    }

                    QueueCommunication.DeleteMessage(urlMessage);
                }
                await Task.Delay(1000);
            }
        }

    }
}
