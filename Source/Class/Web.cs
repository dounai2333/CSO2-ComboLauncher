using System;
using System.IO;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CSO2_ComboLauncher
{
    class Web : IDisposable
    {
        public class TimeoutWebClient : WebClient
        {
            private int Timeoutms { get; set; }

            public TimeoutWebClient(int timeoutms)
            {
                Timeoutms = timeoutms;
            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = Timeoutms;
                return w;
            }
        }

        public WebClient Client { get; private set; }

        public TimeoutWebClient TimeoutClient { get; private set; }

        public bool msgpassivemode { get; set; }

        private Thread counter { get; set; }

        private bool shouldoutput { get; set; }

        private long logbyte { get; set; }

        private int ProgressPercentage { get; set; }

        private long TotalBytesToReceive { get; set; }

        private long BytesReceived { get; set; }

        public Web(bool passiveoutput = false, int clienttimeout = 5000)
        {
            Client = new WebClient
            {
                Proxy = null,
                Encoding = Encoding.GetEncoding("GBK")
            };
            Client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.6045.200 Safari/537.36";
            Client.Headers[HttpRequestHeader.AcceptLanguage] = "zh-CN,zh;q=0.9";
            Client.Headers[HttpRequestHeader.Cookie] = QQMail.Mail5k; // temp workaround
            Client.DownloadProgressChanged += Downloader_AsyncProgressChanged;
            Client.DownloadFileCompleted += Downloader_AsyncCompleted;

            TimeoutClient = new TimeoutWebClient(clienttimeout)
            {
                Proxy = Client.Proxy,
                Encoding = Client.Encoding,
            };
            TimeoutClient.Headers[HttpRequestHeader.UserAgent] = Client.Headers[HttpRequestHeader.UserAgent];
            TimeoutClient.Headers[HttpRequestHeader.AcceptLanguage] = Client.Headers[HttpRequestHeader.AcceptLanguage];

            msgpassivemode = passiveoutput;
            shouldoutput = false;
            logbyte = 0;

            ProgressPercentage = 0;
            TotalBytesToReceive = 0;
            BytesReceived = 0;

            StartCounter();
        }

        public async Task<bool> DownloadFile(string link, string path, string hashchecktype = "", string hash = "")
        {
            if (Download.Instance.IsVisible)
                return false;

            hash = hash.ToUpper();

            if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            Download.Instance.dwldfile.Text = Misc.PathLengthSaver(Path.GetFileName(path), 35);
            Download.Instance.Show();

            try
            {
                await Client.DownloadFileTaskAsync(link, path);
            }
            catch
            {
                Download.ResetStatus();
                File.Delete(path);
                return false;
            }

            if (File.Exists(path))
            {
                if (hashchecktype != "" && await Misc.GetHash(path, hashchecktype) != hash)
                {
                    File.Delete(path);
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public void StopDownloading()
        {
            Client.CancelAsync();
        }

        private void StartCounter()
        {
            counter = new Thread(() =>
            {
                while (true)
                {
                    Misc.Sleep(1000, false);
                    if (BytesReceived == 0)
                        continue;

                    if (msgpassivemode)
                    {
                        shouldoutput = true;
                    }
                    else
                    {
                        Download.MainOutput(BytesReceived, TotalBytesToReceive, (BytesReceived - logbyte).ToString(), ProgressPercentage);
                        logbyte = BytesReceived;
                    }
                }
            });
            counter.Start();
        }

        private void Downloader_AsyncCompleted(object sender, AsyncCompletedEventArgs e)
        {
            logbyte = 0;
            ProgressPercentage = 0;
            TotalBytesToReceive = 0;
            BytesReceived = 0;

            if (!e.Cancelled)
                Download.ResetStatus();
        }

        private void Downloader_AsyncProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressPercentage = e.ProgressPercentage;
            TotalBytesToReceive = e.TotalBytesToReceive;
            BytesReceived = e.BytesReceived;

            if (msgpassivemode)
            {
                if (shouldoutput)
                {
                    Download.MainOutput(BytesReceived, TotalBytesToReceive, (BytesReceived - logbyte).ToString(), ProgressPercentage);
                    logbyte = BytesReceived;
                    shouldoutput = false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client?.Dispose();
                TimeoutClient?.Dispose();

                counter.Abort();
                counter = null;
            }
        }
    }
}