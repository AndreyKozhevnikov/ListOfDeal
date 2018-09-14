using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WlConnectionLibrary {
    public class WLConnector : IWLConnector {
        public WLConnector() {
            GetSettings();
            ShowExceptions = true;
            //  var v = GetAllLists();
        }
        string accessToken;
        string clientId;

        public void Test() {
            // doOAuth();
            //     var ll = GetAllLists();
            //    var lst = GetTasksForList(WLProcessor.MyListId);
        }

        void GetSettings() {
            var st = WLConnectorAssembly.Properties.Resources.settings.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            clientId = st[0];
            accessToken = st[2];
        }

        private string GetHttpRequestResponse(string url, string requestType, string json = "", bool isBackUp = false) {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add(string.Format("X-Access-Token: {0}", accessToken));
            httpWebRequest.Headers.Add(string.Format("X-Client-ID: {0}", clientId));
            if(isBackUp) {
                httpWebRequest.Headers.Add(string.Format("x-client-device-id: {0}", "custom id"));
                httpWebRequest.Headers.Add(string.Format("x-client-current-time: {0}", "custom time"));
                httpWebRequest.Headers.Add(string.Format("x-client-instance-id: {0}", "custom id"));
                httpWebRequest.Timeout= (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
            }
            httpWebRequest.Method = requestType;
            if(json != "") {
                StreamWriter writer = new StreamWriter(httpWebRequest.GetRequestStream());
                writer.Write(json);
                writer.Flush();
                writer.Close();
            }
            try {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var streamReader = new StreamReader(httpResponse.GetResponseStream());
                var responseText = streamReader.ReadToEnd();
                streamReader.Close();
                return responseText;
            }
            catch(Exception e) {
                string st = e.Message + Environment.NewLine;
                st = st + url + Environment.NewLine;
                st = st + json + Environment.NewLine;
                st = st + e.StackTrace + Environment.NewLine;
                StreamWriter sw = new StreamWriter("exception.txt");
                sw.Write(st);
                sw.Close();
                RaiseConnectionErrorEvent(e);
                return null;
            }
        }

        public event EventHandler<UnhandledExceptionEventArgs> ConnectionErrorEvent;
        void RaiseConnectionErrorEvent(Exception e) {
            ConnectionErrorEvent?.Invoke(this, new UnhandledExceptionEventArgs(e, false));
        }
        public bool ShowExceptions { get; set; }
        protected internal string NormalizeString(string title) {
            return title.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", "\\r\\n").Replace("\n\n", "\\r\\n");
        }
        public WLTask CreateTask(string title, int listId, DateTime? dueDate, bool isMajor) {
            title = NormalizeString(title);
            string st = "http://a.wunderlist.com/api/v1/tasks";
            JsonCreator.Add("list_id", listId);
            JsonCreator.Add("title", title);
            if(dueDate != null) {
                JsonCreator.Add("due_date", ConvertToWLDate(dueDate.Value));
            }
            if(isMajor == true) {
                JsonCreator.Add("starred", true);
            }
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st, "POST", json);
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }

        public List<WLList> GetAllLists() {
            string st = "http://a.wunderlist.com/api/v1/lists";
            var responseText = GetHttpRequestResponse(st, "GET");
            var model = JsonConvert.DeserializeObject<List<WLList>>(responseText);
            return model;
        }

        public WLTask GetTask(string taskId) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}", st, taskId);
            var responseText = GetHttpRequestResponse(st2, "GET");
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }

        public List<WLTask> GetTasksForList(int listId) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format("{0}?list_id={1}", st, listId);
            var responseText = GetHttpRequestResponse(st2, "GET");
            var list = JsonConvert.DeserializeObject<List<WLTask>>(responseText);
            return list;
        }

        public List<WLTask> GetCompletedTasksForList(int listId) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format("{0}?list_id={1}&completed=true", st, listId);
            var responseText = GetHttpRequestResponse(st2, "GET");
            var list = JsonConvert.DeserializeObject<List<WLTask>>(responseText);
            return list;
        }

        public WLTask UpdateTask(WLTask task) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}", st, task.id);
            JsonCreator.Add("revision", task.revision);
            JsonCreator.Add("title", "NewTestTitle3" + DateTime.Now.Millisecond);
            //  JsonCreator.Add("completed", true);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st2, "PATCH", json);
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }
        public WLTask CompleteTask(string wlId) {
#if !Release
            return null;
#endif
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}", st, wlId);

            var revision = GetTask(wlId).revision;


            JsonCreator.Add("revision", revision);
            JsonCreator.Add("completed", true);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st2, "PATCH", json);
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }
        public WLTask ChangeTitleOfTask(string wlId, string newTitle, int revision) {
            newTitle = NormalizeString(newTitle);
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}", st, wlId);


            JsonCreator.Add("revision", revision);
            JsonCreator.Add("title", newTitle);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st2, "PATCH", json);
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }
        public WLTask ChangeStarredOfTask(string wlId, bool isMajor, int revision) {

            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}", st, wlId);


            JsonCreator.Add("revision", revision);
            JsonCreator.Add("starred", isMajor);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st2, "PATCH", json);
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }
        public WLTask ChangeListOfTask(string wlId, int listId, int revision) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}", st, wlId);


            JsonCreator.Add("revision", revision);
            JsonCreator.Add("list_id", listId);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st2, "PATCH", json);
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }
        public WLTask ChangeScheduledTime(string wlId, string dueDate, int revision) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}", st, wlId);

            JsonCreator.Add("revision", revision);
            JsonCreator.Add("due_date", dueDate);
            //  JsonCreator.Add("completed", true);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st2, "PATCH", json);
            var wlTask = JsonConvert.DeserializeObject<WLTask>(responseText);
            return wlTask;
        }


        public WLNote CreateNote(string taskId, string content) {
            var normalContent = NormalizeString(content);
            string st = "http://a.wunderlist.com/api/v1/notes";
            JsonCreator.Add("task_id", taskId);
            JsonCreator.Add("content", normalContent);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st, "POST", json);
            var wlNote = JsonConvert.DeserializeObject<WLNote>(responseText);
            return wlNote;
        }

        public void ReadAllJSON() {
            StreamReader sr = new StreamReader(@"c:\test\wunderlist.json");
            string st = sr.ReadToEnd();
            sr.Close();
            var v = JsonConvert.DeserializeObject<RootObject>(st);
        }

        public static string ConvertToWLDate(DateTime dt) {
            return dt.ToString("yyyy-MM-dd");
        }

        public List<WLNote> GetNodesForTask(string taskId) {
            string st = "http://a.wunderlist.com/api/v1/notes";
            string st2 = string.Format("{0}?task_id={1}", st, taskId);
            var responseText = GetHttpRequestResponse(st2, "GET");
            var list = JsonConvert.DeserializeObject<List<WLNote>>(responseText);
            return list;
        }

        public WLNote UpdateNoteContent(string noteId, int revision, string content) {
            var normalConten = NormalizeString(content);
            string st = "http://a.wunderlist.com/api/v1/notes";
            string st2 = string.Format(@"{0}/{1}", st, noteId);
            JsonCreator.Add("revision", revision);
            JsonCreator.Add("content", normalConten);
            string json = JsonCreator.GetString();
            var responseText = GetHttpRequestResponse(st2, "PATCH", json);
            var WLNote = JsonConvert.DeserializeObject<WLNote>(responseText);
            return WLNote;
        }
        public void DeleteNote(string noteId, int revision) {
            string st = "http://a.wunderlist.com/api/v1/notes";
            string st2 = string.Format(@"{0}/{1}?revision={2}", st, noteId, revision);
            var responseText = GetHttpRequestResponse(st2, "DELETE");
        }
        public void DeleteTask(WLTask task) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}?revision={2}", st, task.id, task.revision);
            var responseText = GetHttpRequestResponse(st2, "DELETE");
        }
        public string GetBackup() {
            string st = "https://backup.wunderlist.com/api/v1/export";
            var responseText = GetHttpRequestResponse(st, "GET", isBackUp: true);
            return responseText;
        }


        //  #if needNewToken
        const string authorizationEndpoint = "https://www.wunderlist.com/oauth/authorize";
        private async void doOAuth() {
            // Generates state and PKCE values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Creates a redirect URI using an available port on the loopback address.
            //  string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, "50010");
            output("redirect URI: " + redirectURI);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            output("Listening..");
            http.Start();


            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?client_id={1}&redirect_uri={2}&state={3}",
                authorizationEndpoint,
                clientId,
                  System.Uri.EscapeDataString(redirectURI),
                state,
                code_challenge,
                code_challenge_method);

            // Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings the Console to Focus.
            BringConsoleToFront();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) => {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
            if(context.Request.QueryString.Get("error") != null) {
                output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if(context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null) {
                output("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if(incoming_state != state) {
                output(String.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }
            output("Authorization code: " + code);

            // Starts the code exchange at the Token Endpoint.
            performCodeExchange(code, code_verifier, redirectURI);
        }
        string ClientSecret = "0e3567ca881eacc4a1c3473523ee47958dde7b92e21a342a21c48b100fa4";
        async void performCodeExchange(string code, string code_verifier, string redirectURI) {
            output("Exchanging code for tokens...");
            string tokenRequestURI = "https://www.wunderlist.com/oauth/access_token";
            //request2
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";

            using(var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                string json = String.Format(@"{{""client_id"":""{0}"",""client_secret"":""{1}"",""code"":""{2}""}}", clientId, ClientSecret, code);
                Debug.Print(json);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using(var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                var requiredToken = streamReader.ReadToEnd();
            }

        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public void BringConsoleToFront() {
            SetForegroundWindow(GetConsoleWindow());
        }
        public static int GetRandomUnusedPort() {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
        public static string randomDataBase64url(uint length) {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return base64urlencodeNoPadding(bytes);
        }
        public static string base64urlencodeNoPadding(byte[] buffer) {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
        public void output(string output) {
            Console.WriteLine(output);
        }
        public static byte[] sha256(string inputStirng) {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }
        //  #endif
    }
}



