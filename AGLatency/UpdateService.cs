using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using System.Threading;

namespace AGLatency
{

    public class ExtendedWebClient : WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request != null)
                request.Timeout = Timeout;
            return request;
        }

        public ExtendedWebClient()
        {
            Timeout = 60000; // 60 seconds
        }
    }

   
    class UpdateService
    {

        private static string PostWebApiUri = "http://104.42.189.107/GetUpdateService/api/values";
        //string PostWebApiUri = "http://localhost:1609/api/values";
        public delegate void callbackFunc(List<string> ls);
        public delegate void VersionUpdateFunc(string msg);
       
        VersionUpdateFunc vuFunc = null;
        private static string _latestVersion = "";

        
        public UpdateService(  VersionUpdateFunc vu)
        {
          
            vuFunc = vu;
        }
        public string PostIt(string PostContent,string computer)
        {

            try
            {
                WebClient webClient = new WebClient();

                Uri uri = new Uri(PostWebApiUri, UriKind.Absolute);

                webClient.Headers["Content-type"] = "text/plain";
                //webClient.Headers["Host"] = "localhost:1707";
                webClient.Headers["Host"] = computer;
                //webClient.Headers["User-Agent"] = "Fiddler";
                //webClient.Headers["Content-Length"] = "285";
                webClient.Encoding = Encoding.UTF8;
                webClient.UploadStringCompleted += new UploadStringCompletedEventHandler(webClient_UploadStringCompleted);



                //string temp2 = JsonConvert.SerializeObject(PostContent);

                //webClient.UploadString(uri, "POST", PostContent);

                string encodeStr = myEncode(PostContent);
                string result = webClient.UploadString(uri, "POST", encodeStr);
                //Console.Write("result:"+result));
                return result;

            }
            catch (WebException exception)
            {
                string responseText;
                if (exception.Response == null)
                {
                    Logger.LogMessage("WebException without response: " + exception.Message);
                    return "";
                }

                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                    Logger.LogMessage(responseText);
                    Logger.LogException(exception, Thread.CurrentThread);
                }
            }
            catch (Exception ex)
            {

                //  Console.Write(ex.Message);

                Logger.LogException(ex, Thread.CurrentThread);

            }

            return "";
        }

        void webClient_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            try
            {
                string temp = e.ToString();
                //   Console.Write("Done:"+temp);
                //Customer customer = JsonConvert.DeserializeObject<Customer>(e.Result);
            }

            catch (WebException ex)
            {
                string responseText;

                using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                    Logger.LogMessage(responseText);
                    Logger.LogException(ex, Thread.CurrentThread);
                }
            }
            catch (Exception ex)
            {
                // Console.Write(ex.Message);

            }
        }

        public static void DownloadString(Uri address)
        {
            WebClient client = new WebClient();
            string reply = client.DownloadString(address);

            Console.WriteLine(reply);
        }

        private static string myEncode(string src)
        {
            if (String.IsNullOrEmpty(src)) return null;
            byte[] ke = Encoding.UTF8.GetBytes("UpdateService");
            byte[] srcB = Encoding.UTF8.GetBytes(src);

            int k = 0;
            for (int i = 0; i < srcB.Length; i++)
            {
                srcB[i] = (byte)(srcB[i] ^ ke[k]);
                k++;
                if (k == ke.Length) k = 0;

            }

            string result = Convert.ToBase64String(srcB);


            return result;
        }
        private static string myDecode(string b64src)
        {
            if (String.IsNullOrEmpty(b64src)) return null;

            byte[] srcB = Convert.FromBase64String(b64src);

            byte[] ke = Encoding.UTF8.GetBytes("UpdateService");


            int k = 0;
            for (int i = 0; i < srcB.Length; i++)
            {
                srcB[i] = (byte)(srcB[i] ^ ke[k]);
                k++;
                if (k == ke.Length) k = 0;

            }

            string result = Encoding.UTF8.GetString(srcB);


            return result;
        }
        //   output("Runtime Total : " + totalElapsedTime + "  SymbolLoading: " + debugClientSymbolElapsedTime + "  SQLAnalyzing:" + SQLDumpDataElapsedTime);



        public void VersionUpdate()
        {
            try
            {

                string MachineName = System.Environment.MachineName;
                _latestVersion = PostIt("AGLatencyLatestVersion||"+ MachineName, MachineName);
                string version = _latestVersion;

                if (version.Contains("AGLatencyLatestVersion")) // it is valid version string
                {
                    string serverVersion = version.Replace("[\"AGLatencyLatestVersion\",\"", "").Replace("\"]", "");

                    string currentVersion = typeof(Program).Assembly.GetName().Version.ToString();

                    string info = "";
                    if (String.Compare(serverVersion, currentVersion) > 0)
                    {
                        //  string str = "New version of SQLDumpViewer tool is available. Installed version:" + currentVersion
                        //   + ". Latest version:" + serverVersion;
                        info = "New version " + serverVersion + " is available";
                        Logger.LogMessage(info);

                    }
                    else
                    {
                        Logger.LogMessage("Version update doesn't find new version.");
                    }
                    if (this.vuFunc != null) vuFunc(info);


                } //if

            }
            catch (Exception ex)
            {

            }

        }

    }


}
