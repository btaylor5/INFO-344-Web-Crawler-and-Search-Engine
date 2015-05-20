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
using System.Web.Script.Serialization;
using System.Web.Script.Services;
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
    [System.Web.Script.Services.ScriptService]
    public class admin : System.Web.Services.WebService
    {

        /// <summary>
        /// The start command. Will begin to process the urls in the queue
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Start()
        {
            QueueCommunication.AddCommand("CRAWL");
            return new JavaScriptSerializer().Serialize("[Command] [" + DateTime.Now.ToString() + "] Start Crawling");
        }

        /// <summary>
        /// Load Command. Will start to load the predefined sites CNN and BleacherReport
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Load()
        {
            QueueCommunication.AddCommand("LOAD");
            return new JavaScriptSerializer().Serialize("[Command] [" + DateTime.Now.ToString() + "] Load Crawler");
        }

        /// <summary>
        /// Will stop crawling after it completes its current crawl
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Stop()
        {
            QueueCommunication.AddCommand("STOP");
            return new JavaScriptSerializer().Serialize("[Command] [" + DateTime.Now.ToString() + "] Stop Crawling");
        }

        /// <summary>
        /// Returns the Last Ten indexed urls
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> LastTenVisitedUrls()
        {
            List<string> answer = TableCommunication.LastTenVisitedUrls();
            return answer;
        }

        /// <summary>
        /// Removes history of indexed and errors
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ClearIndex()
        {
            TableCommunication.RemoveURLHistory();
            TableCommunication.RemoveErrorHistory();
            TableCommunication.RemoveTouchHistory();
            return "Deleting Indices of Crawled URL";
        }


        //[WebMethod]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public bool GetResults(string url)
        //{
        //    List<string> bank = new List<string>();
        //    List<TouchedURL> answer = TableCommunication.GetList(url);
        //    foreach (TouchedURL one in answer)
        //    {
        //        bank.Add(url);
        //    }
        //    int size = bank.Count;
        //    return size > 0;
        //}

        /// <summary>
        /// Loads the request into the cutom crawler
        /// </summary>
        /// <param name="crawl_string">"format is "http://www.path.com/to/robot.txt=path.com/"</param>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void CustomCrawl(string crawl_string)
        {
            Debug.WriteLine("yup" + crawl_string);
            QueueCommunication.AddLoader(crawl_string);
            QueueCommunication.AddCommand("LOAD_CUSTOM_ROOT");
        }


        /// <summary>
        /// returns the available memory left. Will not respond during loading function
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetMemory()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.GetCounter(1, "Memory"));
        }

        /// <summary>
        /// returns the % CPU utilization. Will not respond during loading function
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetCPU()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.GetCounter(1, "CPU"));
        }


        /// <summary>
        /// returns how many urls are left to process
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string LeftToProcess()
        {
            return new JavaScriptSerializer().Serialize(QueueCommunication.URLCount());
        }

        /// <summary>
        /// returns a the last 10 error messages
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetErrorMessages()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.GetErrorMessages(10));
        }

        /// <summary>
        /// Adds a specific URL to be crawled
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddUrlToQueue(string url)
        {
            QueueCommunication.AddURL(url);
            return new JavaScriptSerializer().Serialize("[Command] [" + DateTime.Now.ToString() + "] Added " + url + " to crawling Queue");
        }


        /// <summary>
        /// returns how many urls have been indexed
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string IndexCount()
        {
           return new JavaScriptSerializer().Serialize(TableCommunication.IndexCountQuery());
        }

        /// <summary>
        /// returns how many errors have occured
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ErrorCount()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.ErrorCountQuery());
        }

        /// <summary>
        /// reuturns how the last system status recorded
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SystemStatus()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.LastSystemStatus(1));
        }

        /// <summary>
        /// returns the last 10 system history events
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SystemRunHistory()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.LastSystemStatus(10));
        }

        /// <summary>
        /// returns the search results for a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SearchResults(string url)
        {
            List<string> index = TableCommunication.SearchForIndex(url);
            return new JavaScriptSerializer().Serialize(index);

        }

        /// <summary>
        /// cleared the queue of urls to be processed
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ClearQueue()
        {
            QueueCommunication.ClearQueue();
            return "Clearing Queue";
        }

    }
}
