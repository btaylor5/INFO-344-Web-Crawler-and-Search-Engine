using System;
using System.Collections.Generic;
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
using System.Configuration;
using AnswerLibrary;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);


        private static CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        private static CloudQueue queue = queueClient.GetQueueReference("calculator");

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable table = tableClient.GetTableReference("calcresults");


        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

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

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                queue.CreateIfNotExists();
                queue.FetchAttributes();
                int? cachedMessageCount = queue.ApproximateMessageCount;
                Trace.TraceInformation("Count: " + cachedMessageCount.ToString());
                if (cachedMessageCount > 0)
                {
                    CloudQueueMessage param = queue.GetMessage();
                    var paramAsArray = param.AsString.Split(',');
                    int sum = 0;
                    foreach (string number in paramAsArray)
                    {
                        sum += Convert.ToInt32(number);
                    }

                    table.CreateIfNotExists();
                    Answer answer = new Answer(param.ToString(), sum);
                    TableOperation insertOperation = TableOperation.Insert(answer);
                    Debug.WriteLine("\n\n\t" + answer.sum + "\n\n");
                    table.Execute(insertOperation);
                    queue.DeleteMessage(param);
                }
                else
                {
                    Trace.TraceInformation("Queue was empty");
                }
                await Task.Delay(10000);
            }
        }
    }
}
