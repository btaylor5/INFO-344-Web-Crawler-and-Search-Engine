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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CrawlingLibrary
{
    public class WebCrawler
    {

        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

        private List<string> allowedDomainBases;
        private HashSet<string> disallowedList;
        private List<string> siteMaps;
        private List<string> toVisit;
        private int currentDomainBase;


        public WebCrawler(string[] domainBase)
        {
            currentDomainBase = 0;
            allowedDomainBases = new List<string>();
            foreach (string domain in domainBase)
            {
                allowedDomainBases.Add(domain);
            }
            disallowedList = TableCommunication.DisallowList(allowedDomainBases[currentDomainBase]);
            siteMaps = new List<string>();
            toVisit = new List<string>();
        }

        public void PrepareCrawlOfSite(string url, string domainBase)
        {
            Debug.WriteLine("Crawling Robot.txt");
            Dictionary<string, URLStatus.Status> robotResults = ParseRobotTxtIfFound(url, domainBase);
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
            Debug.WriteLine("Done Crawling Robot.txt");
            currentDomainBase++;
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
                    string schemaUrl = "";
                    DateTime time = DateTime.MinValue;
                    while (reader.Read())
                    {
                        //Debug.WriteLine("For URL: " + url + " ->  " + reader.Name);
                        if (reader.Name.Equals("loc"))
                        {
                            schemaUrl = reader.ReadElementString();
                        }
                        if (reader.Name.Equals("lastmod"))
                        {
                            time = reader.ReadElementContentAsDateTime();
                        }
                        else if (reader.Name.Equals("<news:publication_date>"))
                        {
                            time = reader.ReadElementContentAsDateTime();
                        }
                        else if (!schemaUrl.Equals("") && (reader.Name.Equals("sitemap") || reader.Name.Equals("url")))
                        {
                            Debug.WriteLine("No Time Found");
                            time = DateTime.Now;
                        }
                        if (!schemaUrl.Equals("") && !IsOld(time))
                        {
                            if (schemaUrl.EndsWith(".xml"))
                            {
                                AddSiteMapToQueue(schemaUrl);
                            }
                            //else if (!TableCommunication.IsTouchedLink(schemaUrl))
                            //{
                                TableCommunication.TouchLink(schemaUrl);
                                Debug.WriteLine("[With Time]: " + time.ToString());
                                QueueCommunication.AddURL(schemaUrl);
                            //}
                            schemaUrl = "";
                            time = DateTime.MinValue;
                        }
                        else if (!schemaUrl.Equals("") && IsOld(time) && time != DateTime.MinValue)
                        {
                            Debug.WriteLine("[Too Old, But Touching]: " + time);
                            TableCommunication.TouchLink(schemaUrl);
                            schemaUrl = "";
                            time = DateTime.MinValue;
                        }
                    }
                }
            }

        public bool IsOld(DateTime time)
        {
            return (time <= DateTime.Now.AddMonths(-2));
        }

        public CrawledURL CrawlURL(string url)
        {

            try
            {

                HtmlWeb Webget = new HtmlWeb();
                HtmlDocument doc = Webget.Load(url);
                Webget.StatusCode.GetTypeCode();

                if (!Webget.StatusCode.ToString().Equals(System.Net.HttpStatusCode.OK.ToString()))
                {
                    Debug.WriteLine("");
                    TableCommunication.InsertError(Webget.StatusCode.ToString(), "Link Was Bad", url);
                    return null;
                }

                if (sameDomain(url))
                {
                    string title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
                   // 
                   //Debug.WriteLine(compressedBody);
                    string updated = "";
                    string original = "";
                    foreach(HtmlNode date in doc.DocumentNode.SelectNodes("//meta[@content]")) {
                        foreach (var attribute in date.Attributes) {
                            if(attribute.Value.Equals("og:pubdate")) {
                                original = attribute.Value;
                                Debug.WriteLine("Date!: " + original);
                            } else if(attribute.Value.Equals("pubdate")) {
                                updated = attribute.Value;
                                Debug.WriteLine("Date!: " + updated);
                            }
                        }
                    }
                    if (original.Equals("") && !updated.Equals("")) {
                        original = updated;
                    }
                    //Debug.WriteLine("\n\n\t" + title + "\n\n");
                    //HashSet<string> uniqueLinks = new HashSet<string>();
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        string path = link.GetAttributeValue("href", null);

                        path = FixFilePath(url, path);
                        //Debug.WriteLine("Fixed Path:" + path);

                        if (sameDomain(path) && !Disallowed(path) && !TableCommunication.IsTouchedLink(path))
                        {
                            Debug.WriteLine("New Link, Touching and Adding to Queue: " + path);
                            TableCommunication.TouchLink(path);
                            QueueCommunication.AddURL(path);
                        }
                        else if (TableCommunication.IsTouchedLink(path))
                        {
                            Debug.WriteLine("Already Touched: " + path);
                        }
                        else if (Disallowed(path))
                        {
                            Debug.WriteLine("Disallowed: " + path);
                        }
                        else if (!sameDomain(path))
                        {
                            Debug.WriteLine("Not the Same Domain: " + path);
                        }
                        else
                        {
                            Debug.WriteLine("Something Else Went wrong in WorkerRole: " + path);
                        }
                    }
                    doc.DocumentNode.SelectSingleNode("/html/body").Descendants()
                        .Where(
                        x => x.Name == "script" || x.Name == "style" || x.Name == "#comment"
                        ).ToList()
                        .ForEach(x => x.Remove());
                    string body = doc.DocumentNode.SelectSingleNode("/html/body").InnerText;
                    string compressedBody = Regex.Replace(body, @"\s+", " ").Trim();
                    return new CrawledURL(title, url, original, compressedBody);
                }
                else
                {
                    return null;
                }
            }
            catch (WebException ex)
            {
                TableCommunication.InsertError(ex.Status.ToString(), ex.Message, url);
                return null;
            }
        }

        
        public Dictionary<string, URLStatus.Status> ParseRobotTxtIfFound(string url, string domainBase)
        {
            Dictionary<string, URLStatus.Status> urlBank = new Dictionary<string, URLStatus.Status>();
            WebClient client = new WebClient();
            bool bleacherReportHardCode = domainBase.Equals("bleacherreport.com/");
            string urlRoot = url.Replace("/robots.txt", "");
            try
            {
                Stream stream = client.OpenRead(url);
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
                           
                            if (!bleacherReportHardCode || (bleacherReportHardCode && sitemapPath.Contains("nba.xml")) )
                            {
                                urlBank.Add(sitemapPath, URLStatus.Status.Sitemap);
                            }
                        }
                        else if (line.StartsWith("Disallow:"))
                        {
                            string result = urlRoot + cutBetweenStrings(line, "Disallow:", "#");
                            TableCommunication.AddToDisallow(result, domainBase);
                            urlBank.Add(result, URLStatus.Status.Disallow);
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
                        }
                    }
                }
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.Message + "  ->  " + we.Status.ToString());
                TableCommunication.InsertError(we.Response.ToString(), we.Message.ToString(), url);

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

        public bool Disallowed(string url)
        {
            foreach (string disallowed in disallowedList)
            {
                if (url.StartsWith(disallowed))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsIndexed(string url)
        {
            return TableCommunication.IndexedYet(url);
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
           if (path.StartsWith("//") || path.StartsWith("www."))
           {
               return "http:" + path;
           }
           else if (path.StartsWith("/"))
           {
               return cutBetweenStrings(currentURL, "", ".com") + ".com" + path;
           }
           {
               Debug.WriteLine("FilePath is Perfect: " + path);
               return path;
           }
        }
    }
}
