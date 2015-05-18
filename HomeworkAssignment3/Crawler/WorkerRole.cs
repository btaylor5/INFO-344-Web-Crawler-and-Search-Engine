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
        private string[] domainBases = new string[] { "cnn.com", "bleacherreport.com/nba" };
        WebCrawler crawler;

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
            QueueCommunication.InitializeCommunication();
            TableCommunication.InitializeCommunication();

            crawler = new WebCrawler(domainBases);

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

            int performanceLoop = 20;
            string lastCommand = "STOP";
            TableCommunication.InsertSystemStatus("IDLE", "Just Booted Up System");
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                if (performanceLoop >= 0)
                {
                    performanceLoop--;
                } else {
                    PerformanceMonitor.GetCPU();
                    PerformanceMonitor.GetMemory();
                    performanceLoop = 20;
                }

                if (QueueCommunication.PeekCommand() != null)
                {
                    CloudQueueMessage command = QueueCommunication.GetCommand();
                    lastCommand = command.AsString;
                    QueueCommunication.DeleteCommand(command);
                }
                if (lastCommand.Equals("LOAD"))
                {
                    TableCommunication.InsertSystemStatus("LOADING", "Started Load");
                    crawler.PrepareCrawlOfSite("http://www.cnn.com/robots.txt", "cnn.com");
                    TableCommunication.InsertSystemStatus("LOADING", "Finished Loading");
                    crawler.PrepareCrawlOfSite("http://www.bleacherreport.txt/robots.txt", "bleacherreport.com/nba");
                    lastCommand = "CRAWL";
                }
                if ((QueueCommunication.PeekURL() != null) && lastCommand.Equals("CRAWL"))
                {
                    TableCommunication.InsertSystemStatus("CRAWLING", "System is currently crawling");
                    CloudQueueMessage urlMessage = QueueCommunication.GetMessage();
                    string url = urlMessage.AsString;

                    bool isIndexed = crawler.IsIndexed(url);
                    bool disallowed = crawler.Disallowed(url);

                    if (!disallowed && !isIndexed)
                    {
                        CrawledURL info = crawler.CrawlURL(url);
                        if (info != null)
                        {
                            TableCommunication.IndexUrl(info);
                        }
                    }
                    if (disallowed)
                    {
                        Debug.WriteLine("Disallowed!!! -> " + url);
                    }
                    if (isIndexed)
                    {
                        Debug.WriteLine("Link is Already Indexed! " + url);
                        TableCommunication.InsertError("AlreadyIndexedURL", "This page has already been indexed", url);
                    }
                    QueueCommunication.DeleteMessage(urlMessage);
                }
                else if(lastCommand.Equals("CRAWL"))
                {
                    TableCommunication.InsertSystemStatus("IDLE", "Nothing Left To Process");
                } else if(lastCommand.Equals("STOP"))
                {
                    TableCommunication.InsertSystemStatus("IDLE", "System is currently IDLE");
                }
                await Task.Delay(1000);
            }
        }

    }
}