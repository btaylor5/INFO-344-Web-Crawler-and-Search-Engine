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


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Start()
        {
            QueueCommunication.AddCommand("CRAWL");
            return new JavaScriptSerializer().Serialize("[Command] [" + DateTime.Now.ToString() + "] Start Crawling");
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]

        public string Load()
        {
            QueueCommunication.AddCommand("LOAD");
            return new JavaScriptSerializer().Serialize("[Command] [" + DateTime.Now.ToString() + "] Load Crawler");
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Stop()
        {
            QueueCommunication.AddCommand("STOP");
            return new JavaScriptSerializer().Serialize("[Command] [" + DateTime.Now.ToString() + "] Stop Crawling");
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> LastTenVisitedUrls()
        {
            List<string> answer = TableCommunication.LastTenVisitedUrls();
            return answer;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string clearQueue()
        {
            QueueCommunication.DeleteQueue();
            return "Deleted Queue";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ClearIndex()
        {
            TableCommunication.RemoveURLHistory();
            return "Deleting Indices of Crawled URL";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public bool GetResults(string url)
        {
            List<string> bank = new List<string>();
            List<TouchedURL> answer = TableCommunication.GetList(url);
            foreach (TouchedURL one in answer)
            {
                bank.Add(url);
            }
            int size = bank.Count;
            return size > 0;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetMemory()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.GetCounter(1, "Memory"));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetCPU()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.GetCounter(1, "CPU"));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string LeftToProcess()
        {
            return new JavaScriptSerializer().Serialize(QueueCommunication.URLCount());
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetErrorMessages()
        {
            return new JavaScriptSerializer().Serialize(TableCommunication.GetErrorMessages(1));
        }

    }
}
