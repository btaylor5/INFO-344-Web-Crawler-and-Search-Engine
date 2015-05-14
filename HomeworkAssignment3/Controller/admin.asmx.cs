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

namespace Controller
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class admin : System.Web.Services.WebService
    {


        [WebMethod]
        public string StartCrawling()
        {
            QueueCommunication.AddURL("http://www.cnn.com");
            return "Started Crawling CNN";
        }


        [WebMethod]
        public List<string> LastTenVisitedUrls()
        {
            List<string> answer = TableCommunication.LastTenVisitedUrls();
            return answer;
        }

        [WebMethod]
        public string clearQueue()
        {
            QueueCommunication.DeleteQueue();
            return "Deleted Queue";
        }

        [WebMethod]
        public string clearTable()
        {
            TableCommunication.RemoveURLHistory();
            return "Deleting Crawl Indices";
        }
    }
}
