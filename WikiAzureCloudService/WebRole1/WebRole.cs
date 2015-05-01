using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;
using System.Diagnostics;
using System.IO;
using WikiLibrary;

namespace WebRole1
{
    /// <summary>
    /// This WebRole is in charge of setting up the web service on deploy. 
    /// Once deployed, the service will initialize itself so it will be ready for requests.
    /// </summary>
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            // These processes will take some time. 
            CallService.HttpRequest("/DownloadBlob");
            CallService.HttpRequest("/BuildTrie");

            return base.OnStart();
        }

    }
}
