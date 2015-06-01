using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Controller;

namespace WikiSearchSuggestions
{
    /// <summary>
    /// Summary description for WikiSuggest
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WikiSuggest : System.Web.Services.WebService
    {

        private PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available MBytes");
        private const int MAX_RESULTS = 10;
        private static HybridTrie trie;

        public WikiSuggest()
        {
        }

        [WebMethod]
        public float GetMemory()
        {
            return memProcess.NextValue();
        }

        /// <summary>
        /// Download the file of Wiki Titles
        /// </summary>
        /// <returns>a string with a message that either confirms success or raises awareness to a failure. </returns>
        [WebMethod]
        public string DownloadBlob()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("wiki-references");

            string response = "File Was Not Downloaded.";
            if (container.Exists()) {
                foreach(IListBlobItem item in container.ListBlobs(null, false)) {
                    if (item.GetType() == typeof(CloudBlockBlob)) {
                        CloudBlockBlob blob = (CloudBlockBlob) item;

                        using (Stream outputFile = File.Create(HttpContext.Current.Server.MapPath("~/wiki-reference"))) {
                            blob.DownloadToStream(outputFile);
                            response = "Download Successful.";
                        }

                    }
                }
            }

            return response;
        }

        /// <summary>
        /// BuildTrie must be called before. Calling BuildTrie without first calling DownloadBlob can result in using a previously downloaded file.
        /// </summary>
        /// <returns>a message of how many title were succesfully built into the trie</returns>
        [WebMethod]
        public string BuildTrie()
        {
            if (File.Exists(HttpContext.Current.Server.MapPath("~/wiki-reference")))
            {
                trie = new HybridTrie(HttpContext.Current.Server.MapPath("~/wiki-reference"));
                return "Built Trie w/ " + GetTrieCount() + " titles.";
            }
            else
            {
                Debug.WriteLine("File Not Found");
            }
            return "An Error Occured. Trie Will Still Be Null";
        }

        /// <summary>
        /// Will return a JSON representation of an array of query suggestions.
        /// </summary>
        /// <param name="prefix">pass in a prefix of a word or query</param>
        /// <returns>The first 10 suggestions for possible queries</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSuggestions(string prefix)
        {
            if (prefix.Equals(""))
            {
                return "";
            }

            if (trie == null)
            {
                BuildTrie();
            }

            List<string> suggestions = new List<string>();

            if (trie != null)
            {
                suggestions = trie.GetAllSuggestions(prefix.ToLower(), MAX_RESULTS);
            }
            else
            {
                suggestions.Add("Trie Was Null");
            }

            return new JavaScriptSerializer().Serialize(suggestions);
        }


        [WebMethod]
        public void SearchForPrefix(string prefix)
        {
            HybridTrieNode node = trie.SearchForPrefix(prefix);
        }

        /// <summary>
        /// Returns an int representing how many titles were read into the trie
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int GetTrieCount()
        {
            if (trie != null)
            {
                return trie.TrieCount;
            }
            else
            {
                return 0;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetLastWord()
        {
            if (trie != null)
            {
                return trie.lastWord;
            }
            else
            {
                return "BuildTrieFirst";
            }
        }

    }
}
