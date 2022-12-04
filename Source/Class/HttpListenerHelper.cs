using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;

namespace CSO2_ComboLauncher
{
    class HttpListenerHelper : IDisposable
    {
        public HttpListener Listener { get; private set; }

        private Thread ListenerThread { get; set; }

        private List<string> RequestUrlList { get; set; }

        private List<string> RequestUrlList_Response { get; set; }

        private List<bool> RequestUrlList_IsFile { get; set; }

        private List<string> RequestUrlList_Overall { get; set; }

        private List<string> RequestUrlList_Overall_Folder { get; set; }

        public string ServerAddress { get; private set; }

        public HttpListenerHelper(int port)
        {
            if (!Misc.IsProcessHasAdminAccess())
                throw new Exception("HttpListener cannot be used without administrator access.");

            Listener = new HttpListener
            {
                IgnoreWriteExceptions = true
            };

            ListenerThread = null;

            RequestUrlList = new List<string>();
            RequestUrlList_Response = new List<string>();
            RequestUrlList_IsFile = new List<bool>();
            RequestUrlList_Overall = new List<string>();
            RequestUrlList_Overall_Folder = new List<string>();

            CheckPort(port);
        }

        public void Start()
        {
            if (IsListening())
                return;

            ListenerThread = new Thread(() =>
            {
                while (true)
                {
                    bool needcontinue = false;

                    HttpListenerContext context = Listener.GetContext();
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");

                    string requesturl = context.Request.Url.LocalPath;
                    if (string.IsNullOrEmpty(Path.GetFileName(requesturl)))
                        requesturl += "index.html";

                    for (int i = 0; i < RequestUrlList_Overall.Count(); i++)
                    {
                        if (requesturl.StartsWith(RequestUrlList_Overall[i]))
                        {
                            string localpath = RequestUrlList_Overall_Folder[i] + requesturl.Replace(RequestUrlList_Overall[i], "");
                            if (File.Exists(localpath))
                            {
                                byte[] data = File.ReadAllBytes(localpath);

                                context.Response.StatusCode = 200;
                                context.Response.StatusDescription = "Success (200)";
                                context.Response.ContentType = MediaTypeNames.Application.Octet;

                                context.Response.ContentLength64 = data.Length;
                                context.Response.OutputStream.Write(data, 0, data.Length);
                                context.Response.Close();
                                needcontinue = true;
                                break;
                            }
                        }
                    }
                    if (needcontinue)
                        continue;

                    for (int i = 0; i < RequestUrlList.Count(); i++)
                    {
                        if (requesturl == RequestUrlList[i])
                        {
                            if (!string.IsNullOrEmpty(RequestUrlList_Response[i]))
                            {
                                if (RequestUrlList_IsFile[i])
                                {
                                    if (File.Exists(RequestUrlList_Response[i]))
                                    {
                                        byte[] data = File.ReadAllBytes(RequestUrlList_Response[i]);

                                        context.Response.StatusCode = 200;
                                        context.Response.StatusDescription = "Success (200)";
                                        context.Response.ContentType = MediaTypeNames.Application.Octet;

                                        context.Response.ContentLength64 = data.Length;
                                        context.Response.OutputStream.Write(data, 0, data.Length);
                                        context.Response.Close();
                                        needcontinue = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    context.Response.StatusCode = 200;
                                    context.Response.StatusDescription = "Success (200)";
                                    context.Response.ContentType = MediaTypeNames.Text.Plain;

                                    using (StreamWriter sw = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
                                        sw.Write(RequestUrlList_Response[i]);
                                    context.Response.Close();
                                    needcontinue = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (needcontinue)
                        continue;

                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "Not Found (404)";
                    context.Response.ContentType = MediaTypeNames.Text.Plain;

                    using (StreamWriter sw = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
                        sw.Write("Not Found (404)");
                    context.Response.Close();
                }
            });
            Listener.Start();
            ListenerThread.Start();
        }

        public void Pause()
        {
            if (IsListening())
            {
                Listener.Stop();
            }
        }

        public void AddResponse(string url, string response, bool isfile)
        {
            url = UrlFix(url);

            RequestUrlList.Add(url);
            RequestUrlList_Response.Add(response);
            RequestUrlList_IsFile.Add(isfile);
        }

        public void RemoveResponse(string url)
        {
            url = UrlFix(url);

            int index = RequestUrlList.IndexOf(url);
            if (index != -1)
            {
                RequestUrlList.Remove(url);
                RequestUrlList_Response.Remove(RequestUrlList_Response[index]);
                RequestUrlList_IsFile.Remove(RequestUrlList_IsFile[index]);
            }
        }

        public void RefreshResponse(string url, string response, bool isfile)
        {
            url = UrlFix(url);

            int index = RequestUrlList.IndexOf(url);
            if (index != -1)
            {
                RequestUrlList_Response[index] = response;
                RequestUrlList_IsFile[index] = isfile;
            }
        }

        public void AddOverallResponse(string folder)
        {
            if (string.IsNullOrEmpty(Path.GetFileName(folder)))
                folder = Path.GetFullPath(folder).Remove(Path.GetFullPath(folder).Length - 1);
            string url = UrlFix(Path.GetFileName(folder));

            folder = folder.Replace("\\", "/");
            if (!Directory.Exists(folder))
                return;

            RequestUrlList_Overall.Add(url);
            RequestUrlList_Overall_Folder.Add(folder);
        }

        public void RemoveOverallResponse(string folder)
        {
            if (string.IsNullOrEmpty(Path.GetFileName(folder)))
                folder = Path.GetFullPath(folder).Remove(Path.GetFullPath(folder).Length - 1);
            string url = UrlFix(Path.GetFileName(folder));

            int index = RequestUrlList_Overall.IndexOf(url);
            if (index != -1)
            {
                RequestUrlList_Overall.Remove(url);
                RequestUrlList_Overall_Folder.Remove(RequestUrlList_Overall_Folder[index]);
            }
        }

        public bool IsListening()
        {
            return Listener != null && Listener.IsListening && IsThreadRunning();
        }

        public bool IsThreadRunning()
        {
            return ListenerThread != null && ListenerThread.IsAlive;
        }

        private void CheckPort(int port)
        {
            if (!Misc.IsTCPPortAvailable(port))
            {
                port++;
                CheckPort(port);
            }
            else
            {
                Listener.Prefixes.Add($"http://+:{port}/");
                ServerAddress = $"http://127.0.0.1:{port}/";
            }
        }

        private string UrlFix(string url)
        {
            url = url.Replace("\\", "/");
            if (url[0].ToString() != "/")
                url = "/" + url;
            return url;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Listener?.Abort();
                ListenerThread?.Abort();
            }
        }
    }
}