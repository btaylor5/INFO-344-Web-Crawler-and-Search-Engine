using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CrawlingLibrary
{
    public class WebCrawler
    {

        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

        private List<string> allowedDomainBases;
        private List<string> disallowedList;
        private List<string> siteMaps;
        private List<string> toVisit;

        //private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        //private static CloudQueue queue = queueClient.GetQueueReference("todo");

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("urls");

        public WebCrawler(string[] domainBase)
        {
            allowedDomainBases = new List<string>();
            foreach (string domain in domainBase)
            {
                allowedDomainBases.Add(domain);
            }
            disallowedList = new List<string>();
            siteMaps = new List<string>();
            toVisit = new List<string>();
        }

        public void PrepareCrawlOfSite(string url)
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
                    disallowedList.Add(entry.Key);
                }
                else if (entry.Value == URLStatus.Status.Sitemap)
                {
                    AddSiteMapToQueue(entry.Key);
                }
            }
        }

        private void AddSiteMapToQueue(string url)
        {
            // Todo: Use Table For Storing dissallowed
            // Todo: Recurse Through SiteMaps that lead to sitemaps, be aware that they wil throw 404
            // Todo: Check SiteMaps for the past tqo months

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
                    if (reader.Name.Equals("loc"))
                    {
                        string schemaUrl = reader.ReadElementString();
                        if (schemaUrl.EndsWith(".xml"))
                        {
                            reader.Read();
                            if (reader.Name.Equals("lastmod"))
                            {
                                DateTime time = reader.ReadElementContentAsDateTime();
                                if (time > DateTime.Now.AddMonths(-2))
                                {
                                    Debug.WriteLine("\nFound SiteMap: " + schemaUrl + "\n");
                                    AddSiteMapToQueue(schemaUrl);
                                }
                                Debug.WriteLine("Too Old");
                            }
                        }
                        else
                        {
                            QueueCommunication.AddURL(schemaUrl);
                        }
                    }
                }
            }

        }

        public CrawledURL CrawlURL(string url)
        {
             
            HtmlWeb Webget = new HtmlWeb();
            HtmlDocument doc = Webget.Load(url);


            if (sameDomain(url))
            {
                string title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;

                Debug.WriteLine("\n\n\t" + title + "\n\n");

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    string path = link.GetAttributeValue("href", null);

                    Debug.WriteLine("On Page: " + link.GetAttributeValue("href", null));
                    if (path.StartsWith("/"))
                    {
                        path = FixFilePath(url, path);
                        Debug.WriteLine("Fix Path:" + path);
                    }
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
                        if (line.StartsWith("User-Agent:"))
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
                        else if (line.StartsWith("Disallow:"))
                        {
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

        private void AddToTable(CrawledURL crawledUrl)
        {
            TableOperation insertOperation = TableOperation.Insert(crawledUrl);
            table.Execute(insertOperation);
        }

        public bool Disallowed(string url)
        {
            foreach (string directory in disallowedList)
            {
                if (url.Contains(directory))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Visited(string url)
        {
            return TableCommunication.CrawledYet(url);
        }

        public bool sameDomain(string url)
        {
            foreach (string domain in allowedDomainBases)
            {
                if (url.Contains(domain)) 
                {
                    return true;
                }
            }
            return false;
        }

        public string FixFilePath(string currentURL, string path)
        {
           Debug.WriteLine("Attempt to Fix: " + path);
           if ((path.StartsWith("//") || path.StartsWith("http://")) && sameDomain(path))
           {
               return path;
           }
           else if (path.StartsWith("/"))
           {
               return cutBetweenStrings(currentURL, "", ".com") + ".com" + path;
           }
           return path;
        }

        public string RemoveURLHistory()
        {
            table.DeleteIfExists();

            while (!CreatedTableAfterDelete()) { }
            return "History Was Removed";
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
