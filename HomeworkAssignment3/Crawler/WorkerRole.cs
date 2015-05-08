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

            HashSet<string> offLimits = new HashSet<string>();
            HashSet<string> siteMaps = new HashSet<string>();
            HashSet<string> visited = new HashSet<string>();
            HashSet<string> toVisit = new HashSet<string>();


            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                
                queue.FetchAttributes();
                int? cachedMessageCount = queue.ApproximateMessageCount;
                Trace.TraceInformation("Total in Queue: " + cachedMessageCount.ToString());
                if (cachedMessageCount > 0)
                {
                    
                    CloudQueueMessage urlMessage = queue.GetMessage();
                    string url = urlMessage.AsString;
                    Debug.WriteLine("\n\n\t" + url + "\n\n\t");

                    bool root = true;
                    if (root)
                    {
                        Dictionary<string, URLStatus.Status> robotResults = ParseRobotTxtIfFound(url);
                        // Have A Dictioray with all of the sitemap paths, and allowable, and disallowed
                        foreach (KeyValuePair<string, URLStatus.Status> entry in robotResults)
                        {
                            if (entry.Value == URLStatus.Status.Allow)
                            {
                                toVisit.Add(entry.Key);
                            }
                            else if (entry.Value == URLStatus.Status.Disallow)
                            {
                                offLimits.Add(entry.Key);
                            }
                            else if (entry.Value == URLStatus.Status.Sitemap)
                            {
                                AddSiteMapToQueue(entry.Key);
                            }
                        }
                        root = false;
                    }


                    offLimits.Add(url);
                    //CrawledURL info = CrawlURL(offLimits, url);
                    queue.DeleteMessage(urlMessage);
                }
                else
                await Task.Delay(1000);
            }
        }

        private void AddSiteMapToQueue(string url)
        {

            WebClient client = new WebClient();
            Stream stream = client.OpenRead(url);

            using (XmlReader reader = XmlReader.Create(stream))
            {
                //<url>
                //  <loc>
                        //THE LINK
                //  </loc>
                //  <ton of shiit />
                //<url>
                while (reader.Read())
                {
                    //Debug.WriteLine("For URL: " + url + " ->  " + reader.Name);
                    if (reader.Name.Equals("loc")) {
                        string schemaUrl = reader.ReadElementString();
                        if (schemaUrl.EndsWith("html") || schemaUrl.EndsWith(".htm"))
                        {
                            Debug.WriteLine("Found" + schemaUrl);
                            //Crawl If I Can
                                // Build A tree. Break Down the URLS into different hierachies where it each folder has flag for allowed or denied
                        }
                    }
                }
            }

        }

        private CrawledURL CrawlURL(HashSet<string> offLimits, string url)
        {

            HtmlWeb Webget = new HtmlWeb();
            HtmlDocument doc = Webget.Load(url);
            string host = "cnn";
            Regex regEx = new Regex(@"" + host + "", RegexOptions.IgnoreCase);


            if (sameDomain(url) && !offLimits.Contains(url))
            {
                string title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
                
                Debug.WriteLine("\n\n\t" + title + "\n\n");

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    string path = link.GetAttributeValue("href", null);

                    Debug.WriteLine("" + link.GetAttributeValue("href", null));
                    // When adding to the queue, make sure it's a whole URL and not a filepath.
                }

                return new CrawledURL(title, url);
            }
            else
            {
                return null;
            }
        }

        private string GetHTML(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        private void StoreURL(CrawledURL url)
        {
            TableOperation insertOperation = TableOperation.Insert(url);
            Debug.WriteLine("\n\n\t" + url.Title + " -> " + url.URL + "\n\n");
            table.Execute(insertOperation);
        }

        private bool sameDomain(string url)
        {
            return true;
        }

        public bool isNewRoot() {
            return true;
        }

        public Dictionary<string, URLStatus.Status> ParseRobotTxtIfFound(string url)
        {
            Dictionary<string, URLStatus.Status> urlBank = new Dictionary<string, URLStatus.Status>();
            WebClient client = new WebClient();
            try
            {
                Stream stream = client.OpenRead(url + "/robots.txt");
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if(line.StartsWith("User-Agent:"))
                        {
                            if (!line.Contains("*"))
                            {
                                Debug.WriteLine("\n\n\n Robot Said NO!!! \n\n\n");
                                return new Dictionary<string, URLStatus.Status>();
                            }
                        }
                        else if (line.StartsWith("Sitemap:"))
                        {
                            string sitemapPath = cutBetweenStrings(line, "Sitemap:", "#");
                            urlBank.Add(sitemapPath, URLStatus.Status.Sitemap);
                            Debug.WriteLine(sitemapPath);
                        }
                        else if (line.StartsWith("Disallow:")) {
                            string result = url + cutBetweenStrings(line, "Disallow:", "#");
                            urlBank.Add(result, URLStatus.Status.Disallow);
                            Debug.WriteLine(result);

                        }
                        else if (line.StartsWith("Allow:"))
                        {
                            string result = url + cutBetweenStrings(line, "Allow:", "#");

                            urlBank.Add(result, URLStatus.Status.Allow);
                            Debug.WriteLine(result);
                        }
                        else if (line.StartsWith("#"))
                        {
                            Debug.WriteLine("Comment:" + line);
                            //Comment
                        }
                    }
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.Message + "  ->  " + we.Status.ToString());
            }
            return urlBank;
        }

        /// <summary>
        /// Returns the substring between the params start and end on the input string. 
        /// Response will be trimmed of spaces. 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private string cutBetweenStrings(string input, string start, string end)
        {

            int startIndex = input.IndexOf(start);

            if (startIndex == -1)
            {
                startIndex = 0;
            }

            //from start to end of start string
            int endOfStart = start.Length + startIndex;

            //from start to start of end string
            int endIndex = input.IndexOf(end);

            //second string not found
            if (endIndex == -1)
            {
                return input.Substring(endOfStart).Trim();
            }
            else
            {
                return input.Substring(endOfStart, endIndex).Trim();
            }
        }

        private void AddToQueue(string messageText)
        {
            CloudQueueMessage message = new CloudQueueMessage(messageText);
            queue.AddMessage(message);
        }

    }
}
