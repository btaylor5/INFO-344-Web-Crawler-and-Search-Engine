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
using System.Xml.XPath;

namespace CrawlingLibrary
{
    public class WebCrawler
    {

        //private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

        public List<string> allowedDomainBases { get; set; }
        private HashSet<string> disallowedList;
        private HashSet<string> touched;
        private List<string> toVisit;


        /// <summary>
        /// Constructs a WebCrawler to be used for parseing XML Sitemaps and crawling urls
        /// </summary>
        public WebCrawler()
        {
            allowedDomainBases = new List<string>();
            allowedDomainBases.Add("cnn.com/");
            allowedDomainBases.Add("bleacherreport.com/");
            disallowedList = TableCommunication.DisallowList();
            toVisit = new List<string>();
            touched = new HashSet<string>();
        }
        

        /// <summary>
        /// Adds a domain base meaning what domains can the links be from.
        /// </summary>
        /// <param name="domainBase"></param>
        public void AddDomainBase(string domainBase)
        {
            allowedDomainBases.Add(domainBase);
        }


        /// <summary>
        /// loads up robots.txt and collects sitemaps and disallowed urls
        /// </summary>
        /// <param name="url"></param>
        /// <param name="domainBase"></param>
        public void PrepareCrawlOfSite(string url, string domainBase)
        {
            AddDomainBase(domainBase);
            Debug.WriteLine("Crawling Robot.txt");
            Dictionary<string, URLStatus.Status> robotResults = ParseRobotTxtIfFound(url, domainBase);

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
                    CrawlSiteMap(entry.Key);
                }
            }
            if (!robotResults.ContainsValue(URLStatus.Status.Sitemap))
            {
                QueueCommunication.AddURL(url.Replace("/robots.txt", ""));
            }
            Debug.WriteLine("Done Crawling Robot.txt");
        }

        /// <summary>
        /// Crawls through an XML sitemap link
        /// </summary>
        /// <param name="xmlLink"></param>
        private void CrawlSiteMap(string xmlLink)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(xmlLink);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);
            XmlNodeList siteMapList = xmlDoc.DocumentElement.GetElementsByTagName("sitemap");
            XmlNodeList urlList = xmlDoc.DocumentElement.GetElementsByTagName("url");
            if (siteMapList.Count > 0)
            {
                foreach (XmlNode node in siteMapList)
                {
                    var loc = node["loc"];
                    var date = node["lastmod"];

                    if (date != null && loc != null)
                    {
                        DateTime published = Convert.ToDateTime(date.InnerText);
                        DateTime old = DateTime.Now.AddMonths(-2);
                        if(published > old ) {
                            string sitemap = loc.InnerText;
                            if (sitemap.EndsWith(".xml"))
                            {
                                CrawlSiteMap(sitemap);
                            }
                            else
                            {
                                QueueCommunication.AddURL(sitemap);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Too Old! Cut Off at: " + old + " Compared With:" + published);
                        }
                    } else if (loc != null){
                        string sitemap = loc.InnerText;
                        touched.Add(sitemap);
                        QueueCommunication.AddURL(sitemap);
                    }
                }
            } else if (urlList.Count > 0) {
                foreach (XmlNode node in urlList)
                {
                    var loc = node["loc"];
                    var date = node["lastmod"];

                    if (date != null && loc != null)
                    {
                        DateTime published = Convert.ToDateTime(date.InnerText);
                        DateTime old = DateTime.Now.AddMonths(-2);
                        if (published > old)
                        {
                            string sitemap = loc.InnerText;
                            touched.Add(sitemap);
                            QueueCommunication.AddURL(sitemap);
                        }
                        else
                        {
                            Debug.WriteLine("Too Old! Cut Off at: " + old + " Compared With:" + published);
                        }
                    }
                    else if (loc != null && date == null)
                    {
                        var news = node["news:news"];
                        if (news != null)
                        {
                            news = news["news:publication_date"];
                        }
                        if (news != null)
                        {
                            DateTime published = Convert.ToDateTime(news.InnerText);
                            DateTime old = DateTime.Now.AddMonths(-2);
                            if (published > old)
                            {
                                string sitemap = loc.InnerText;
                                touched.Add(sitemap);
                                QueueCommunication.AddURL(sitemap);
                            }
                            else
                            {
                                Debug.WriteLine("Too Old! Cut Off at: " + old + " Compared With:" + published);
                            }
                        } else
                        {
                            string url = loc.InnerText;
                            touched.Add(url);
                            QueueCommunication.AddURL(url);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// returns whether a time is within the past two months
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsOld(DateTime time)
        {
            return (time <= DateTime.Now.AddMonths(-2));
        }


        /// <summary>
        /// Crawls the URL and adds the links found to be processed later. Will pull out important body information and published date
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
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
                   
                    //looks for three variations in possible dates
                    var lastmod = doc.DocumentNode.SelectSingleNode("//meta[@content and @name='lastmod']");
                    var ogPubdate = doc.DocumentNode.SelectSingleNode("//meta[@content and @name='og:pubdate']");
                    var pubdate = doc.DocumentNode.SelectSingleNode("//meta[@content and @name='pubdate']");
                    string time = "NotIncluded";
                    if (ogPubdate != null)
                    {
                        time = ogPubdate.GetAttributeValue("content", "");;
                    }
                    else if (pubdate != null)
                    {
                        time = pubdate.GetAttributeValue("content", "");;
                    }
                    else if (lastmod != null)
                    {
                        time = lastmod.GetAttributeValue("content", "");;
                    }
                    else
                    {
                        Debug.WriteLine("No Meta Tag Match for name = (lastmod || og:pubdate || pubdate)");
                    }

                    //loops through every link adding to queueu
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        string path = link.GetAttributeValue("href", null);

                        path = FixFilePath(url, path);

                        if (sameDomain(path) && !Disallowed(path) && !touched.Contains(path))
                        {
                            Debug.WriteLine("New Link, Touching and Adding to Queue: " + path);
                            touched.Add(path);
                            QueueCommunication.AddURL(path);
                        }
                        else if (touched.Contains(path))
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
                    string body;
                    // If it's an article, not a homepage, it will pickout the article text
                    StringBuilder total = new StringBuilder();
                    doc.DocumentNode.Descendants().Where(n =>
                        n.Attributes.Contains("class") && n.Attributes["class"].Value.Split(' ').Any(b => b.Equals("zn-body__paragraph"))
                    ).ToList()
                    .ForEach(x => total.Append(x.InnerText + " "));

                    if (total.Length > 0)
                    {
                        body = total.ToString();
                    }
                    else //otherwise it will grab whatever it has access to
                    {
                        doc.DocumentNode.SelectSingleNode("/html/body").Descendants()
                       .Where(
                       x => x.Name == "nav" || x.Name == "header" || x.Name == "footer" || x.Name == "script" || x.Name == "style" || x.Name == "#comment"
                       ).ToList()
                       .ForEach(x => x.Remove());
                        body = doc.DocumentNode.SelectSingleNode("/html/body").InnerText;
                    }
                    string compressedBody = Regex.Replace(body, @"\s+", " ").Trim();
                    return new CrawledURL(TableCommunication.SanitizeForTable(title), url, time, compressedBody);
                }
                else
                {
                    return null;
                }
            }
            catch (WebException ex)
            {   //track web exception
                TableCommunication.InsertError(ex.Status.ToString(), ex.Message, url);
                return null;
            }
        }

        /// <summary>
        /// Parses the robots.txt to collect sitempas and disallowed urls
        /// </summary>
        /// <param name="url"></param>
        /// <param name="domainBase"></param>
        /// <returns></returns>
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
                           
                            if (!bleacherReportHardCode || (bleacherReportHardCode && sitemapPath.Contains("nba.xml")) && !urlBank.ContainsKey(sitemapPath))
                            {
                                urlBank.Add(sitemapPath, URLStatus.Status.Sitemap);
                            }
                        }
                        else if (line.StartsWith("Disallow:"))
                        {
                            string result = urlRoot + cutBetweenStrings(line, "Disallow:", "#");
                            TableCommunication.AddToDisallow(result, domainBase);
                            if (!urlBank.ContainsKey(result))
                            {
                                urlBank.Add(result, URLStatus.Status.Disallow);
                            }
                        }
                        else if (line.StartsWith("Allow:"))
                        {
                            string result = url + cutBetweenStrings(line, "Allow:", "#");
                            if (urlBank.ContainsKey(result))
                            {
                                urlBank.Add(result, URLStatus.Status.Allow);
                            }
                            Debug.WriteLine("Allow: " + result);
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

        /// <summary>
        /// returns whether a link is disallowed
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns whether a url has been indexed yet
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsIndexed(string url)
        {
            return TableCommunication.IndexedYet(url);
        }


        /// <summary>
        /// returns whether ot not a given link is from the same domain
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool sameDomain(string url)
        {
            if (url.EndsWith(".com") || url.EndsWith(".net") || url.EndsWith(".org"))
            {
                url = url + "/";
            }
            foreach (string domain in allowedDomainBases)
            {
                if (url.Contains(domain)) 
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// will fix URLs when they are relative
        /// </summary>
        /// <param name="currentURL"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public string FixFilePath(string currentURL, string path)
        {
           if (path.StartsWith("//") || path.StartsWith("www."))
           {
               return "http:" + path;
           }
           else if (path.StartsWith("/"))
           {
               if (currentURL.Contains(".com")) {
                   return cutBetweenStrings(currentURL, "", ".com") + ".com" + path;
               }
               else if (currentURL.Contains(".org"))
               {
                   return cutBetweenStrings(currentURL, "", ".org") + ".org" + path;
               }
               else
               {
                   return cutBetweenStrings(currentURL, "", ".net") + ".net" + path;
               }
           }
           {
               Debug.WriteLine("FilePath is Perfect: " + path);
               return path;
           }
        }
    }
}
