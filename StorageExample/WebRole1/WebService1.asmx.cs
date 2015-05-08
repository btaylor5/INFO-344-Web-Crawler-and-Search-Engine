using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        private static NBAPlayerStats[] nbaplayers;
        public static CloudTable table;
        public static CloudQueue queue;
        
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        public WebService1()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("nbaplayerstats");
            table.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("myurls");
            queue.CreateIfNotExists();
        }

        [WebMethod]
        public int LoadCSV()
        {
            string filename = System.Web.HttpContext.Current.Server.MapPath(@"/2012-2013.nba.stats.csv");
            List<string> filedata = LoadCSVFile(filename);
            var nbaplayersvar = filedata.Skip(9)
                .Select(x => x.Split(','))
                .Select(x => new NBAPlayerStats(x[1], x[24]))
                //.Take(30)
                .ToArray();

            nbaplayers = nbaplayersvar;

            return nbaplayers.Length;
        }

        private static List<string> LoadCSVFile(string filename) {
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            List<string> answer = new List<string>();

            string line;
            while ((line = file.ReadLine()) != null)
            {
                answer.Add(line);
            }
            file.Close();

            return answer;
        }

        [WebMethod]
        public void Insert()
        {
            
            foreach (NBAPlayerStats player in nbaplayers)
            {
                TableOperation insertOperation = TableOperation.Insert(player);
                Trace.TraceInformation(player.Name);
                table.Execute(insertOperation);
            }

        }
        
        [WebMethod]
        public List<string> ReadPlayers()
        {
            List<string> answer = new List<string>();

            TableQuery<NBAPlayerStats> rangeQuery = new TableQuery<NBAPlayerStats>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "A"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, "C"))
                );

            foreach (NBAPlayerStats entity in table.ExecuteQuery(rangeQuery)) {
                answer.Add(entity.Name + " with " + entity.PPG);
                Debug.WriteLine(entity.Name + " with " + entity.PPG);
            }
            return answer;
        }

        [WebMethod]
        public List<string> SearchByPPG(double min, double max)
        {

            List<string> answer = new List<string>();

            TableQuery<NBAPlayerStats> rangeQuery = new TableQuery<NBAPlayerStats>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDouble("PPG", QueryComparisons.GreaterThan, min),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDouble("PPG", QueryComparisons.LessThan, max))
                );

            foreach (NBAPlayerStats entity in table.ExecuteQuery(rangeQuery))
            {
                answer.Add(entity.Name + " with " + entity.PPG);
                Debug.WriteLine(entity.Name + " with " + entity.PPG);
            }

            return answer;
        }

        [WebMethod]
        public void InsertIntoQueue(string url)
        {
            CloudQueueMessage message = new CloudQueueMessage(url);
            queue.AddMessage(message);
        }

        [WebMethod]
        public void InsertRandomIntoQueue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CloudQueueMessage message = new CloudQueueMessage(Guid.NewGuid().ToString());
                queue.AddMessage(message);
            }

        }

        [WebMethod]
        public void ReadFromQueue()
        {
            CloudQueueMessage message2 = queue.GetMessage(TimeSpan.FromMinutes(5));
            Debug.WriteLine(message2.AsString);
            queue.DeleteMessage(message2);
        }
    }
}
