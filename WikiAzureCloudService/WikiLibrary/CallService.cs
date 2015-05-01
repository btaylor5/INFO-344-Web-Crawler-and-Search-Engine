using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace WikiLibrary
{
    /// <summary>
    /// The CallService Class is used whenever you need to touch the webservice.
    /// Check TraceInformation when debugging.
    /// 
    /// IMPORTANT: configure the constant BASE_URL to the address of your webservice
    /// 
    /// </summary>
    public class CallService
    {
        private const string BASE_URL = "http://wikisuggest.cloudapp.net/WikiSuggest.asmx";
        //private const string BASE_URL = "http://localhost:4109/WikiSuggest.asmx";


        /// <summary>
        /// Initiates the web service path that is passed in. Will automatiically prepend the BASE_URL
        /// </summary>
        /// <param name="url">the relative path to call a web service method</param>
        public static void HttpRequest(string url)
        {
            url = BASE_URL + url;
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                using (WebResponse response = myRequest.GetResponse())
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Trace.TraceInformation(url + " -> " + httpResponse.StatusCode.ToString());
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    try
                    {
                        Trace.TraceInformation("Error code: {0}", httpResponse.StatusCode);

                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            Trace.TraceInformation(text);
                        }
                    }
                    catch (NullReferenceException n)
                    {
                        Trace.TraceInformation(n.Message);
                    }

                }
            }
        }
    }
}