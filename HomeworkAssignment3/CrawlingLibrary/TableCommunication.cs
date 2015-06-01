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
        private static CloudTable indexed = tableClient.GetTableReference("indexed2");
        private static CloudTable touched = tableClient.GetTableReference("touched2");
        private static CloudTable disallow = tableClient.GetTableReference("disallowed2");
        private static CloudTable performace = tableClient.GetTableReference("performance2");
        private static CloudTable errors = tableClient.GetTableReference("errors2");
        private static CloudTable system = tableClient.GetTableReference("system2");
        private static CloudTable crawltotals = tableClient.GetTableReference("crawltotals2");
        private static CloudTable lastTen = tableClient.GetTableReference("lastten2");
        private static string[] DoNotIndex = new string[] { "is", "at", "which", "and", "on", "the", "a", "in", "cnn", "com" };
        private static char[] Remove = new char[] { ' ', '\'', '`', '\"', ',', ':', '-', '?', '!', '_', '.', '~', '/', '\\', '^', '(', ')', '#' };
        //private static char[] Replace = new char[] { '\'', '.'}

        /// <summary>
        /// This Should be run before making any table requests to ensure that the necessary tables are built for using the CrawlerClass
        /// </summary>
        public static void InitializeCommunication()
        {
            indexed.CreateIfNotExists();
            touched.CreateIfNotExists();
            disallow.CreateIfNotExists();
            performace.CreateIfNotExists();
            errors.CreateIfNotExists();
            system.CreateIfNotExists();
            crawltotals.CreateIfNotExists();
            lastTen.CreateIfNotExists();
        }

        /// <summary>
        /// Determines whether a link has been visited yet. Should be used only in multiple workerrole environment
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        //public static bool IsTouchedLink(string url)
        //{
        //   TableQuery<TouchedURL> touchedLink = new TableQuery<TouchedURL>()
        //        .Where(
        //            TableQuery.GenerateFilterCondition("url", QueryComparisons.Equal, url)
        //            );

        //    var results = touched.ExecuteQuery(touchedLink).ToList();
        //    bool result = results.Count > 0;
        //    return result;
        //}

        /// <summary>
        /// Returns a List of Touched URLS. Can Be used in development to store links already visited inbetween boots.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        //public static List<TouchedURL> GetList(string url)
        //{
        //    TableQuery<TouchedURL> touchedLink = new TableQuery<TouchedURL>()
        //        .Where(
        //            TableQuery.GenerateFilterCondition("url", QueryComparisons.Equal, url)
        //            );

        //    var results = touched.ExecuteQuery(touchedLink).ToList();
        //    return results;
        //}

        /// <summary>
        /// This Stores the Disallowed links in the cloud so that at each system boot it will know where it can't go. 
        /// Good for preventing blacklisting during development
        /// </summary>
        /// <param name="path">The URL</param>
        /// <param name="source"><The Domain Root to associate the disallow with/param>
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

        /// <summary>
        /// Returns all entries in the Disallow List
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> DisallowList()
        {
            TableQuery<DisallowedURL> query = new TableQuery<DisallowedURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "")
                    );

            var results = disallow.ExecuteQuery(query).Select(x => x.path).ToList();
            HashSet<string> response = new HashSet<string>();
            foreach (string url in results)
            {
                response.Add(url);
            }

            return response;
        }


        /// <summary>
        /// Returns a string that can be used in the Partition and Rowkey of an Azure Table
        /// </summary>
        /// <param name="input">the string you want sanitized</param>
        /// <returns></returns>
        public static string SanitizeForTable(string input)
        {
            char[] forbidden = { '?', '\\', '/', '#' };
            var answer = new string(input
                .Where(x => !forbidden.Contains(x))
                .ToArray());
            return answer;
        }

        //public static void TouchLink(string url)
        //{
        //    TouchedURL site = new TouchedURL(url);
        //    TableOperation operation = TableOperation.Insert(site);
        //    try
        //    {
        //        touched.Execute(operation);
                
        //    }
        //    catch (StorageException se)
        //    {
        //        Debug.WriteLine("\nError Touching Link!!!!: ");
        //        Debug.WriteLine("Caught Table Insert Error: " + se.Message + "\n");
        //    }

        //}

        /// <summary>
        /// Stores the URL and its information into the table for access later
        /// </summary>
        /// <param name="url"></param>
        public static void IndexUrl(CrawledURL urlEntity)
        {

            urlEntity.PartitionKey = SanitizeForTable(urlEntity.PartitionKey);
            while (64000 <= urlEntity.Body.Length * sizeof(Char))
            {
                int overflow = (urlEntity.Body.Length * sizeof(Char));
                if (64000 - overflow < 0) {
                    overflow = overflow - 64000;
                }
                try
                {
                    urlEntity.Body = urlEntity.Body.Substring(0, overflow - 1);
                }
                catch
                {
                    urlEntity.Body = urlEntity.Body.Substring(0, 1000);
                }
            }

            //string[] DoNotIndex = new string[] {"is", "at", "which", "on", "the", "a", "in" };
            //char[] Remove = new char[] {' ', '\'', '`', '\"', ',', ':', '-', '?', '!', '_', '.', '~', '/', '\\', '^', '(', ')', '#'};
           
            var listOfWords = urlEntity.Title.ToLower().Split(Remove, StringSplitOptions.RemoveEmptyEntries).Where(x => !DoNotIndex.Contains(x.ToLower()));
            foreach (string word in listOfWords)
            {
                Debug.WriteLine("In the Foreach: " + word);
                CrawledURL reverseIndex = new CrawledURL(word, urlEntity.URL, urlEntity.Title, urlEntity.Date, urlEntity.Body);
                TableOperation insertOperation = TableOperation.Insert(reverseIndex);
                try
                {
                    Debug.WriteLine("[Indexed] " + urlEntity.Title + " -> " + urlEntity.URL + "\n");
                    indexed.Execute(insertOperation);
                }
                catch (StorageException se)
                {
                    Debug.WriteLine("Error!!!!: " + se.Message + "   " + se.Data + "\n");
                    InsertError(SanitizeForTable(se.Message), SanitizeForTable(se.Data.ToString()), urlEntity.URL);
                    Debug.WriteLine("Caught Table Insert Error: " + urlEntity.PartitionKey + " " + urlEntity.RowKey + " " + urlEntity.Date + " " + urlEntity.URL + " " + urlEntity.Body);
                }
            }
            TableCommunication.IncrementIndexed();
        }


        public static List<CrawledURL> Search(string query, int MexResults)
        {

            List<CrawledURL> unsorted = new List<CrawledURL>();
            var keywords = query.Split(Remove, StringSplitOptions.RemoveEmptyEntries).Where(x => !DoNotIndex.Contains(x.ToLower()));

            foreach (string word in keywords) {
                unsorted.AddRange(KeywordSearch(word));
            }

            var sorted = unsorted
                .GroupBy(x => x.URL)
                .OrderByDescending(x => x.Count())
                .ThenByDescending(x => x.ElementAt(0).Date)
                .Select(x => x.ElementAt(0))
                .ToList();

            //var injectBolding = unsorted
            //    .GroupBy(x => x.URL)
            //    .OrderByDescending(x => x.Count())
            //    .ThenByDescending(x => x.ElementAt(0).Date)
            //    .Select(x => x.ElementAt(0))
            //    .Where(x => x.Body
            //        .Split(new char[] {' ', ',', '.', '!', '?'})
            //        .Where(y => keywords.Contains(y))
            //    .ToList();


            return sorted;


            //return sorted;
        }

        public static List<CrawledURL> KeywordSearch(string keyword)
        {

            keyword = keyword.ToLower();

            TableQuery<CrawledURL> query = new TableQuery<CrawledURL>()
               .Where(
                   TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SanitizeForTable(keyword))
                   );

            var results = indexed.ExecuteQuery(query).ToList();
            return results;
        }

        /// <summary>
        /// Will return whether or not a link has been indexed yet.
        /// </summary>
        /// <param name="url">the url path to check</param>
        /// <returns></returns>
        public static bool IndexedYet(string url)
        {
            TableQuery<CrawledURL> visitedUrl = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SanitizeForTable(url))
                    );

            var results = indexed.ExecuteQuery(visitedUrl).ToList();
            bool result = results.Count > 0;
            return result;
        }

        /// <summary>
        /// Returns the last ten links 
        /// </summary>
        /// <returns></returns>
        public static List<string> LastTenVisitedUrls()
        {
            List<string> answer = new List<string>();

            TableQuery<CrawledURL> rangeQuery = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                    );

            try
            {
                answer = indexed.ExecuteQuery(rangeQuery).OrderBy(x => x.RowKey).Select(x => x.URL).Take(10).ToList();
            }
            catch (StorageException se) // Ask For Forgiveness
            {
                if (se.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableNotFound))
                {
                    while (!CreatedTableAfterDelete(indexed)) { }
                    answer = LastTenVisitedUrls();
                }
                else
                {
                    throw;
                }
            }


            return answer;
        }


        /// <summary>
        /// Deletes the indexed urls and prepares for indexing further urls
        /// </summary>
        /// <returns></returns>
        public static bool RemoveURLHistory()
        {
            indexed.DeleteIfExists();

            while (!CreatedTableAfterDelete(indexed)) { }

            ResetCrawlCounters();
            return true;
        }

        public static bool ResetCrawlCounters()
        {
            crawltotals.DeleteIfExists();
            while (!CreatedTableAfterDelete(crawltotals)) { }
            return true;

        }


        /// <summary>
        /// Creates the table and returns false if that table is currently being deleted
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private static bool CreatedTableAfterDelete(CloudTable table)
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
                    Thread.Sleep(5000);// The table is currently being deleted. Try again until it works.
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns the Performance Counters
        /// </summary>
        /// <param name="resultLimit">How many you want returned. Useful if you want to create a chart</param>
        /// <param name="counterType">Pass in either CPU or Memory</param>
        /// <returns>either MB of memory left or %CPU utilization</returns>
        public static List<string> GetCounter(int resultLimit, string counterType)
        {
            TableQuery<PerformanceEntity> touchedLink = new TableQuery<PerformanceEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, counterType),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                )).Take(resultLimit);

            var results = performace.ExecuteQuery(touchedLink).OrderByDescending(x => x.RowKey).Select(x => x.value).ToList();

            return results;
        }

        /// <summary>
        /// Inserts the Performance counter into the table for later use
        /// </summary>
        /// <param name="counterType"></param>
        /// <param name="value"></param>
        public static void InsertCounter(string counterType, string value)
        {
            PerformanceEntity perf = new PerformanceEntity(counterType, value);
            TableOperation operation = TableOperation.Insert(perf);
            performace.Execute(operation);
        }


        /// <summary>
        /// Inserts an error into the error table
        /// </summary>
        /// <param name="status">status string, could be HTTP Error Codes or custom</param>
        /// <param name="message">Cusotom message to help user</param>
        /// <param name="url">The url that lifted the error</param>
        public static void InsertError(string status, string message, string url)
        {
            ErrorEntity entity = new ErrorEntity(SanitizeForTable(status), SanitizeForTable(message), SanitizeForTable(url));
            TableOperation operation = TableOperation.Insert(entity);
            errors.Execute(operation);
            TableCommunication.IncrementErrors();
        }

        /// <summary>
        /// Returns the last error messages
        /// </summary>
        /// <param name="maxResults">number of results you want returned</param>
        /// <returns></returns>
        public static List<string> GetErrorMessages(int maxResults)
        {
            TableQuery<ErrorEntity> lastErrors = new TableQuery<ErrorEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                ).Take(maxResults);

            try
            {
                var results = errors.ExecuteQuery(lastErrors).Select(x => x.ErrorString()).ToList();
                return results;
            }
            catch (StorageException se)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Returns the count of Indexed URL's
        /// </summary>
        /// <returns></returns>
        public static int IndexCountQuery()
        {
            TableQuery<CrawledURL> allEntries = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "")
                    );
            try
            {
                var total = indexed.ExecuteQuery(allEntries).ToList().Count;
                return total;
            }
            catch (StorageException e)
            {
                Debug.WriteLine("Storage Exception");
                return 0;
            }
        }

        /// <summary>
        /// Inserts the System Status into a table so you can recall the history of using the crawler (when IDLE, STOPPED, LOADED)
        /// </summary>
        /// <param name="status"></param>
        /// <param name="message"></param>
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


        /// <summary>
        /// Returns the last system messages
        /// </summary>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public static List<string> LastSystemStatus(int maxResults)
        {
            TableQuery<SystemStatus> lastChange = new TableQuery<SystemStatus>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, ChronHelper.ChronoCompareString())
                ).Take(maxResults);

            Thread.Sleep(50);

            var results = system.ExecuteQuery(lastChange).Select(x => x.StatusString()).ToList();

            return results;
        }

        /// <summary>
        /// returns how many errors there have been
        /// </summary>
        /// <returns></returns>
        public static int ErrorCountQuery()
        {
            TableQuery<ErrorEntity> allEntries = new TableQuery<ErrorEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "")
                    );
            try
            {
                var total = errors.ExecuteQuery(allEntries).ToList().Count;
                return total;

            }
            catch(StorageException se)
            {
                InitializeCommunication();
                return 0;
            }
        }


        /// <summary>
        /// Returns the information for an indexed url
        /// </summary>
        /// <param name="url">url you are searching for</param>
        /// <returns></returns>
        public static List<string> SearchForIndex(string url)
        {
            TableQuery<CrawledURL> lastChange = new TableQuery<CrawledURL>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, SanitizeForTable(url))
                ).Take(1);

            var results = indexed.ExecuteQuery(lastChange).Select(x => "[Title]: " + x.Title + " [Last-Modified: " + x.Date + "] with a Body of [" + x.Body + "]").ToList();
            return results;
        }

        /// <summary>
        /// Removes the Error History from the table
        /// </summary>
        public static void RemoveErrorHistory()
        {
            errors.DeleteIfExists();

            while (!CreatedTableAfterDelete(errors)) { }
        }
        /// <summary>
        /// removes what links have been touched
        /// </summary>
        public static void RemoveTouchHistory()
        {
            touched.DeleteIfExists();

            while (!CreatedTableAfterDelete(touched)) { }
        }

        public static void IncrementIndexed()
        {
            IncrementCounter(1, 0);
        }

        public static void IncrementErrors()
        {
            IncrementCounter(0, 1);
        }

        private static void IncrementCounter(int addToIndexed, int addToErrors)
        {
            TotalEntity counter = lastCounters();
            counter.totalIndexed = counter.totalIndexed + addToIndexed;
            counter.totalErrors = counter.totalErrors + addToErrors;
            counter.totalCrawled = counter.totalCrawled + addToIndexed + addToErrors;

            TableOperation op = TableOperation.InsertOrReplace(counter);
            crawltotals.Execute(op);
        }

        public static int GetErrorCount()
        {
            return lastCounters().totalErrors;
        }

        public static int GetTotalIndexed()
        {
            return lastCounters().totalIndexed;

        }

        public static int GetTotalCrawled()
        {
            return lastCounters().totalCrawled;

        }


        public static TotalEntity lastCounters()
        {
            TableQuery<TotalEntity> lastCounters = new TableQuery<TotalEntity>()
                .Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Total")
            ).Take(1);

            var lastStatus = crawltotals.ExecuteQuery(lastCounters).ToList();
            TotalEntity counter;
            if (lastStatus.Count > 0)
            {
                counter = lastStatus[0];
            }
            else
            {
                counter = new TotalEntity(0, 0);
            }
            return counter;
        }

        public static int[] ArrayOfCounters()
        {
            TotalEntity counter = lastCounters();
            return new int[3] { counter.totalIndexed, counter.totalErrors, counter.totalCrawled };
        }

        public static LastTen GetLastTen(string type)
        {
            TableQuery<LastTen> getlastten = new TableQuery<LastTen>()
               .Where(
               TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, type)
                ).Take(1);

            var lastinfo = lastTen.ExecuteQuery(getlastten).ToList();

            LastTen last;
            if (lastinfo.Count > 0)
            {
                last = lastinfo[0];
            }
            else
            {
                last = new LastTen(type, "");
            }
            return last;
        }

        public static void UpdateLastTen(string type, string info)
        {
            LastTen last = GetLastTen(type);
            last.info = info;
            TableOperation op = TableOperation.InsertOrReplace(last);
            lastTen.Execute(op);
        }

    }
}
