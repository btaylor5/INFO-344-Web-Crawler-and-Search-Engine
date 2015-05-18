using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlingLibrary
{
    public class TableCommunication
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

        private static CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static CloudTable indexed = tableClient.GetTableReference("indexed");
        private static CloudTable touched = tableClient.GetTableReference("touched");
        private static CloudTable disallow = tableClient.GetTableReference("disallowed");
        private static CloudTable performace = tableClient.GetTableReference("performance");
        private static CloudTable errors = tableClient.GetTableReference("errors");
        private static CloudTable system = tableClient.GetTableReference("system");
        //private static int indexCounter = IndexCountQuery();
        //private static int errorCounter = ErrorCountQuery();


        public static void InitializeCommunication()
        {
            indexed.CreateIfNotExists();
            touched.CreateIfNotExists();
            disallow.CreateIfNotExists();
            performace.CreateIfNotExists();
            errors.CreateIfNotExists();
            system.CreateIfNotExists();;
        }

        public static bool IsTouchedLink(string url)
        {
           TableQuery<TouchedURL> touchedLink = new TableQuery<TouchedURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("url", QueryComparisons.Equal, url)
                    );

            var results = touched.ExecuteQuery(touchedLink).ToList();
            bool result = results.Count > 0;
            return result;
        }

        public static List<TouchedURL> GetList(string url)
        {
            TableQuery<TouchedURL> touchedLink = new TableQuery<TouchedURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("url", QueryComparisons.Equal, url)
                    );

            var results = touched.ExecuteQuery(touchedLink).ToList();
            return results;
        }

        public static void AddToDisallow(string path, string source)
        {
            source = SanitizeForTable(source);

            DisallowedURL url = new DisallowedURL(path, source);
            TableOperation operation = TableOperation.Insert(url);
            try
            {
                Debug.WriteLine("[Add To Disallow]: " + url.path);
                disallow.Execute(operation);
            }
            catch (StorageException se)
            {
                Debug.WriteLine("Couldn't Disallow!!: ");
                Debug.WriteLine("Error: " + se.Message + "\n");
            }
        }

        public static HashSet<string> DisallowList(string source)
        {
            source = SanitizeForTable(source);
            TableQuery<DisallowedURL> query = new TableQuery<DisallowedURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, source)
                    );

            var results = disallow.ExecuteQuery(query).Select(x => x.path).ToList();
            HashSet<string> response = new HashSet<string>();
            foreach (string url in results)
            {
                response.Add(url);
            }

            return response;
        }

        public static string SanitizeForTable(string input)
        {
            char[] forbidden = { '?', '\\', '/', '#' };
            var answer = new string(input
                .Where(x => !forbidden.Contains(x))
                .ToArray());
            return answer;
        }

        public static void TouchLink(string url)
        {
            TouchedURL site = new TouchedURL(url);
            TableOperation operation = TableOperation.Insert(site);
            try
            {
                touched.Execute(operation);
                
            }
            catch (StorageException se)
            {
                Debug.WriteLine("\nError Touching Link!!!!: ");
                Debug.WriteLine("Caught Table Insert Error: " + se.Message + "\n");
            }

        }

        public static void IndexUrl(CrawledURL url)
        {

            url.PartitionKey = SanitizeForTable(url.PartitionKey);

            TableOperation insertOperation = TableOperation.Insert(url);
            try
            {
                Debug.WriteLine("[Indexed] " + url.Title + " -> " + url.URL + "\n");
                indexed.Execute(insertOperation);
            }
            catch (StorageException se)
            {
                Debug.WriteLine("Error!!!!: " + se.Message + "   " + se.Data + "\n");
                Debug.WriteLine("Caught Table Insert Error: " + url.PartitionKey + " " + url.RowKey + " " + url.Date + " " + url.URL);
            }
        }

        public static bool IndexedYet(string url)
        {
            TableQuery<CrawledURL> visitedUrl = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("URL", QueryComparisons.Equal, url)
                    );

            var results = indexed.ExecuteQuery(visitedUrl).ToList();
            bool result = results.Count > 0;
            return result;
        }

        public static List<string> LastTenVisitedUrls()
        {
            List<string> answer = new List<string>();

            TableQuery<CrawledURL> rangeQuery = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                    ).Take(10);

            try
            {
                foreach (CrawledURL entity in indexed.ExecuteQuery(rangeQuery))
                {
                    string combo = "[URL: " + entity.URL + "]";
                    answer.Add(combo);
                    //Debug.WriteLine(combo);
                }
            }
            catch (StorageException se) // Ask For Forgiveness
            {
                if (se.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableNotFound))
                {
                    indexed.CreateIfNotExists();
                    answer = LastTenVisitedUrls();
                }
                else
                {
                    throw;
                }
            }


            return answer;
        }

        public static bool RemoveURLHistory()
        {
            indexed.DeleteIfExists();

            while (!CreatedTableAfterDelete()) { }
            return true;
        }

        private static bool CreatedTableAfterDelete()
        {
            try
            {
                indexed.CreateIfNotExists();
                return true;
            }
            catch (StorageException e)
            {
                if ((e.RequestInformation.HttpStatusCode == 409) && (e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted)))
                {
                    Thread.Sleep(5000);// The table is currently being deleted. Try again until it works.
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }


        public static List<string> GetCounter(int resultLimit, string counterType)
        {
            TableQuery<PerformanceEntity> touchedLink = new TableQuery<PerformanceEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, counterType),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                )).Take(resultLimit);

            var results = performace.ExecuteQuery(touchedLink).Select(x => x.value).ToList();

            return results;
        }

        public static void InsertCounter(string counterType, string value)
        {
            PerformanceEntity perf = new PerformanceEntity(counterType, value);
            TableOperation operation = TableOperation.Insert(perf);
            performace.Execute(operation);
        }

        public static void InsertError(string status, string message, string url)
        {
            ErrorEntity entity = new ErrorEntity(status, message, url);
            TableOperation operation = TableOperation.Insert(entity);
            errors.Execute(operation);
        }

        public static List<string> GetErrorMessages(int maxResults)
        {
            TableQuery<ErrorEntity> lastErrors = new TableQuery<ErrorEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                ).Take(maxResults);

            var results = errors.ExecuteQuery(lastErrors).Select(x => x.ErrorString()).ToList();

            return results;
        }

        public static int IndexCountQuery()
        {
            TableQuery<CrawledURL> allEntries = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTime.Now.AddDays(1))
                    );
            var total = indexed.ExecuteQuery(allEntries).ToList().Count;
            return total;
        }

        public static void InsertSystemStatus(string status, string message)
        {

            TableQuery<SystemStatus> lastChange = new TableQuery<SystemStatus>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                ).Take(1);

            var lastStatus = system.ExecuteQuery(lastChange).Select(x => x).ToList();
            SystemStatus old = null;
            if (lastStatus.Count > 0)
            {
                old = lastStatus[0];
            }
            SystemStatus sys = new SystemStatus(status, message);
            TableOperation operation;
            if (old != null && old.Equals(sys))
            {
                operation = TableOperation.Delete(old);
                system.Execute(operation);
            }
            operation = TableOperation.Insert(sys);
            system.Execute(operation);
        }

        public static List<string> LastSystemStatus(int maxResults)
        {
            TableQuery<SystemStatus> lastChange = new TableQuery<SystemStatus>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                ).Take(maxResults);

            var results = system.ExecuteQuery(lastChange).Select(x => x.StatusString()).ToList();

            return results;
        }


        public static int ErrorCountQuery()
        {
            TableQuery<ErrorEntity> allEntries = new TableQuery<ErrorEntity>()
                .Where(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, DateTime.Now.AddDays(1))
                    );
            var total = errors.ExecuteQuery(allEntries).ToList().Count;
            return total;
        }

        public static List<string> SearchForIndex(string url)
        {
            TableQuery<CrawledURL> lastChange = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("URL", QueryComparisons.Equal, url)
                );

            var results = indexed.ExecuteQuery(lastChange).Select(x => "[Title]: " + x.Title + " [Last-Modified': ").ToList();
            return results;
        }

    }
}
