using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Toqe.Downloader.Business.Utils;
using Toqe.Downloader.Business.Observer;
using Toqe.Downloader.Business.Download;
using Toqe.Downloader.Business.DownloadBuilder;
using Toqe.Downloader.Business.Contract;
using Toqe.Downloader.Business.Contract.Enums;
using Toqe.Downloader.Business.Contract.Events;

namespace CSO2_ComboLauncher
{
    class MultiThreadDownloader : IDisposable
    {
        private SimpleWebRequestBuilder RequestBuilder { get; set; }

        private DownloadChecker DlChecker { get; set; }

        private SimpleDownloadBuilder HttpDlBuilder { get; set; }

        private ResumingDownloadBuilder ResumingDlBuilder { get; set; }

        private List<DownloadRange> AlreadyDownloadedRanges { get; set; }

        private MultiPartDownload Download { get; set; }

        private DownloadSpeedMonitor SpeedMonitor { get; set; }

        private DownloadProgressMonitor ProgressMonitor { get; set; }

        private DownloadToFileSaver DlSaver { get; set; }

        private int Buffer { get; set; }

        private Thread Counter { get; set; }

        private long logbyte { get; set; }

        private bool Downloading { get; set; }

        private string Link { get; set; }

        private int Threads { get; set; }

        private WebHeaderCollection Headers { get; set; }
        
        public MultiThreadDownloader()
        {
            using (Web Web = new Web())
                Headers = Web.Client.Headers;

            RequestBuilder = new SimpleWebRequestBuilder();
            DlChecker = new DownloadChecker();
            HttpDlBuilder = new SimpleDownloadBuilder(RequestBuilder, DlChecker, Headers);
            ResumingDlBuilder = new ResumingDownloadBuilder(10000, 1000, 9999, HttpDlBuilder, Headers);
            AlreadyDownloadedRanges = null;
            SpeedMonitor = null /*new DownloadSpeedMonitor(256)*/;
            ProgressMonitor = new DownloadProgressMonitor();
            Buffer = 1024;

            Downloading = false;
            logbyte = 0;

            StartCounter();
        }

        public async Task<bool> DownloadFile(string link, string path, int threads, string hashchecktype = "", string hash = "")
        {
            if (CSO2_ComboLauncher.Download.Instance.IsVisible)
                return false;

            Link = link;
            Threads = threads;
            hash = hash.ToUpper();

            if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!Misc.IsFileAvailable(path))
                return false;
            else
                File.WriteAllText(path, string.Empty);

            Download = new MultiPartDownload(new Uri(link), Buffer, threads, ResumingDlBuilder, RequestBuilder, DlChecker, AlreadyDownloadedRanges, Headers);
            Download.DownloadCompleted += MultiPartDownload_OnCompleted;
            //SpeedMonitor.Attach(Download);
            ProgressMonitor.Attach(Download);

            DlSaver = new DownloadToFileSaver(new FileInfo(path));
            DlSaver.Attach(Download);

            Downloading = true;
            CSO2_ComboLauncher.Download.Instance.dwldfile.Text = Misc.PathLengthSaver(Path.GetFileName(path), 35);
            CSO2_ComboLauncher.Download.Instance.Show();

            CSO2_ComboLauncher.Download.OnRetry += async () =>
            {
                Pause();
                await Misc.Sleep(2000);
                Resume();
            };

            Download.Start();

            while (Downloading || (Download != null && Download.State != DownloadState.Finished))
                await Misc.Sleep(100);

            while (!Misc.IsFileAvailable(path))
                await Misc.Sleep(100);

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
            if (Download == null)
                return;

            CSO2_ComboLauncher.Download.ResetStatus();

            Downloading = false;
            logbyte = 0;
            Download.Stop();
            Download.DetachAllHandlers();
            Download.Dispose();
            Download = null;
        }

        public void Pause()
        {
            if (Download == null || Downloading == false)
                return;

            Downloading = false;
            Download.Stop();

            long alreadyDownloadedSizeInBytes = ProgressMonitor.GetCurrentProgressInBytes(Download);
            long totalDownloadSizeInBytes = ProgressMonitor.GetTotalFilesizeInBytes(Download);
            int currentProgressInPercent = (int)(ProgressMonitor.GetCurrentProgressPercentage(Download) * 100);
            CSO2_ComboLauncher.Download.MainOutput(alreadyDownloadedSizeInBytes, totalDownloadSizeInBytes, "Paused", currentProgressInPercent);
            logbyte = alreadyDownloadedSizeInBytes;
        }

        public void Resume()
        {
            if (Download == null || Downloading != false)
                return;

            Downloading = true;

            AlreadyDownloadedRanges = Download.AlreadyDownloadedRanges;
            Download.DetachAllHandlers();
            Download.Dispose();

            Download = new MultiPartDownload(new Uri(Link), Buffer, Threads, ResumingDlBuilder, RequestBuilder, DlChecker, AlreadyDownloadedRanges, Headers);
            Download.DownloadCompleted += MultiPartDownload_OnCompleted;
            //SpeedMonitor.Attach(Download);
            ProgressMonitor.Attach(Download);
            DlSaver.Attach(Download);

            Download.Start();
        }

        private void StartCounter()
        {
            Counter = new Thread(() =>
            {
                while (true)
                {
                    Misc.Sleep(1000, false);
                    if (!Downloading || !CSO2_ComboLauncher.Download.Instance.IsVisible)
                        continue;

                    long alreadyDownloadedSizeInBytes = ProgressMonitor.GetCurrentProgressInBytes(Download);
                    long totalDownloadSizeInBytes = ProgressMonitor.GetTotalFilesizeInBytes(Download);
                    int currentProgressInPercent = (int)(ProgressMonitor.GetCurrentProgressPercentage(Download) * 100);

                    CSO2_ComboLauncher.Download.MainOutput(alreadyDownloadedSizeInBytes, totalDownloadSizeInBytes, (alreadyDownloadedSizeInBytes - logbyte).ToString(), currentProgressInPercent);
                    logbyte = alreadyDownloadedSizeInBytes;
                }
            });
            Counter.Start();
        }

        void MultiPartDownload_OnCompleted(DownloadEventArgs args)
        {
            Downloading = false;
            logbyte = 0;
            CSO2_ComboLauncher.Download.ResetStatus();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Download?.DetachAllHandlers();
                Download?.Dispose();

                SpeedMonitor?.Dispose();
                ProgressMonitor?.Dispose();
                DlSaver?.Dispose();

                Counter.Abort();
                Counter = null;
            }
        }
    }
}