using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;
using AnswerLibrary;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace WebRole
{
    /// <summary>
    /// Summary description for Admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
    {

        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);


        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue queue = queueClient.GetQueueReference("calculator");

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("calcresults");

        [WebMethod]
        public string CalculateSumUsingWorkerRole(int a, int b, int c)
        {
            queue.CreateIfNotExists();
            string input = "" + a + "," + b + "," + c;
            CloudQueueMessage message = new CloudQueueMessage(input);
            queue.AddMessage(message);
            return "Calculation Requested for: " + a + " + " + b + " + " + c;
        }

        [WebMethod]
        public List<int> ReadSumFromTableStorage(int numberOfResults)
        {
            List<int> answers = new List<int>();

            table.CreateIfNotExists();
            TableQuery<Answer> rangeQuery = new TableQuery<Answer>()
                .Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, Answer.ChronoCompareString())
                    ).Take(numberOfResults);

            try
            {
                foreach (Answer entity in table.ExecuteQuery(rangeQuery))
                {
                    string combo = "[" + entity.sum + "]";
                    answers.Add(entity.sum);
                    Debug.WriteLine(combo);
                }
            }
            catch (StorageException se) // Ask For Forgiveness
            {
                if (se.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableNotFound))
                {
                    table.CreateIfNotExists();
                }
                else
                {
                    throw;
                }
            }
            return answers;

        }
    }
}
