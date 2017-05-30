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

namespace ListOfDeal {
    public interface IWLConnector {
        WLTask ChangeTitleOfTask(string wlId, string newName, int revision);
        WLTask ChangeScheduledTime(string wlId, string dueTime, int revision);
        WLTask ChangeStarredOfTask(string wlId, bool isMajor, int revision);
        List<WLList> GetAllLists();
        List<WLTask> GetTasksForList(int listId);
        WLTask GetTask(string taskId);
        List<WLNote> GetNodesForTask(string taskId);
        WLTask CreateTask(string title, int listId, DateTime? dueDate, bool isMajor);
        WLTask ChangeListOfTask(string wlId, int listId, int revision);
        WLTask UpdateTask(WLTask task);
        WLTask CompleteTask(string wlId);
        WLNote CreateNote(string taskId, string content);
        WLNote UpdateNoteContent(string noteId, int revision, string content);
        void DeleteNote(string noteId, int revision);
        string GetBackup();
    }
    public class WLConnector : IWLConnector {
        public WLConnector() {
            GetSettings();
            //  var v = GetAllLists();
        }
        string accessToken;
        string clientId;

        public void Test() {
            var lst = GetTasksForList(WLProcessor.MyListId);
        }

        void GetSettings() {
            var st = ListOfDeal.Properties.Resources.settings.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            clientId = st[0];
            accessToken = st[2];
        }

        private string GetHttpRequestResponse(string url, string requestType, string json = "", bool isBackUp=false) {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add(string.Format("X-Access-Token: {0}", accessToken));
            httpWebRequest.Headers.Add(string.Format("X-Client-ID: {0}", clientId));
            if (isBackUp) {
                httpWebRequest.Headers.Add(string.Format("x-client-device-id: {0}", "custom id"));
                httpWebRequest.Headers.Add(string.Format("x-client-current-time: {0}", "custom time"));
                httpWebRequest.Headers.Add(string.Format("x-client-instance-id: {0}", "custom id"));
            }
            httpWebRequest.Method = requestType;
            if (json != "") {
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
                MainViewModel.SaveChanges();
                string st = e.Message + Environment.NewLine;
                st = st + url + Environment.NewLine;
                st = st + json + Environment.NewLine;
                st = st + e.StackTrace + Environment.NewLine;
                MessageBox.Show("Error " + st);
                StreamWriter sw = new StreamWriter("exception.txt");
                sw.Write(st);
                sw.Close();
                return null;
            }
        }
    
        protected internal string NormalizeString(string title) {
            return title.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n","\\r\\n");
        }
        public WLTask CreateTask(string title, int listId, DateTime? dueDate, bool isMajor) {
            title = NormalizeString(title);
            string st = "http://a.wunderlist.com/api/v1/tasks";
            JsonCreator.Add("list_id", listId);
            JsonCreator.Add("title", title);
            if (dueDate != null) {
                JsonCreator.Add("due_date", ConvertToWLDate(dueDate.Value));
            }
            if (isMajor == true) {
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
        public void DeleteTask(WLTask task) {
            string st = "http://a.wunderlist.com/api/v1/tasks";
            string st2 = string.Format(@"{0}/{1}?revision={2}", st, task.id, task.revision);
            var responseText = GetHttpRequestResponse(st2, "DELETE");


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
        public string GetBackup() {
            string st = "https://backup.wunderlist.com/api/v1/export";
            var responseText = GetHttpRequestResponse(st, "GET",isBackUp:true);
            return responseText;
        }
   

#if needNewToken
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
            if (context.Request.QueryString.Get("error") != null) {
                output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null) {
                output("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state) {
                output(String.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }
            output("Authorization code: " + code);

            // Starts the code exchange at the Token Endpoint.
            performCodeExchange(code, code_verifier, redirectURI);
        }
        string ClientSecret = "333";
        async void performCodeExchange(string code, string code_verifier, string redirectURI) {
            output("Exchanging code for tokens...");

            // builds the  request
            string tokenRequestURI = "https://www.wunderlist.com/oauth/access_token";
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientId,
                code_verifier,
                ClientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream())) {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    Console.WriteLine(responseText);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string access_token = tokenEndpointDecoded["access_token"]; //access toketn
                    GetAllLists();
                }
            }
            catch (WebException ex) {
                if (ex.Status == WebExceptionStatus.ProtocolError) {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null) {
                        output("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                            // reads response body
                            //string responseText = reader.ReadToEndAsync();
                            //output(responseText);
                        }
                    }

                }
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
#endif
    }
    public static class JsonCreator {
        static List<Tuple<string, object>> tList = new List<Tuple<string, object>>();
        public static void Add(string st, object o) {
            tList.Add(new Tuple<string, object>(st, o));
        }

        public static string GetString() {
            var lstTuple = tList.Select(x => GetTupleString(x));
            string st = string.Join(",", lstTuple);
            st = "{" + st + "}";
            tList.Clear();
            return st;
        }
        static string GetTupleString(Tuple<string, object> t) {
            if (t.Item1.Contains("id"))
                return string.Format("\"{0}\":{1}", t.Item1, t.Item2);
            if (t.Item2 is string)
                return string.Format("\"{0}\":\"{1}\"", t.Item1, t.Item2);
            if (t.Item2 is bool)
                return string.Format("\"{0}\":{1}", t.Item1, t.Item2.ToString().ToLower());
            return string.Format("\"{0}\":{1}", t.Item1, t.Item2);
        }

    }

    //public class WLList {

    //}
    [DebuggerDisplay("List. Id-{id} title-{title}")]
    public class WLList {
        public int id { get; set; }
        public string title { get; set; }
        public string owner_type { get; set; }
        public int owner_id { get; set; }
        public string list_type { get; set; }
        public bool @public { get; set; }
        public int revision { get; set; }
        public string created_at { get; set; }
        public string type { get; set; }
        public string created_by_request_id { get; set; }
    }
    [DebuggerDisplay("Task. Id-{id} title-{title}")]
    public class WLTask {
        public string id { get; set; }
        public string created_at { get; set; }
        public int created_by_id { get; set; }
        public string created_by_request_id { get; set; }
        public bool completed { get; set; }
        public string completed_at { get; set; }
        public int completed_by_id { get; set; }
        public bool starred { get; set; }
        public int list_id { get; set; }
        public int revision { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string due_date { get; set; }
        public string recurrence_type { get; set; }
        public int? recurrence_count { get; set; }
        public override string ToString() {
            return string.Format("Task {0}", title);
        }
    }

    public class Reminder {
        public int id { get; set; }
        public string date { get; set; }
        public int task_id { get; set; }
        public int revision { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string created_by_request_id { get; set; }
        public string type { get; set; }
    }

    public class Subtask {
        public int id { get; set; }
        public int task_id { get; set; }
        public bool completed { get; set; }
        public string completed_at { get; set; }
        public string created_at { get; set; }
        public int created_by_id { get; set; }
        public string created_by_request_id { get; set; }
        public int revision { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class WLNote {
        public string id { get; set; }
        public int revision { get; set; }
        public string content { get; set; }
        public string type { get; set; }
        public string task_id { get; set; }
        public string created_by_request_id { get; set; }
    }

    public class TaskPosition {
        public int id { get; set; }
        public int list_id { get; set; }
        public int revision { get; set; }
        public List<int> values { get; set; }
        public string type { get; set; }
    }

    public class SubtaskPosition {
        public int id { get; set; }
        public int task_id { get; set; }
        public int revision { get; set; }
        public List<object> values { get; set; }
        public string type { get; set; }
    }

    public class Data {
        public List<WLList> lists { get; set; }
        public List<WLTask> tasks { get; set; }
        public List<Reminder> reminders { get; set; }
        public List<Subtask> subtasks { get; set; }
        public List<WLNote> notes { get; set; }
        public List<TaskPosition> task_positions { get; set; }
        public List<SubtaskPosition> subtask_positions { get; set; }
    }

    public class RootObject {
        public int user { get; set; }
        public string exported { get; set; }
        public Data data { get; set; }
    }
}



