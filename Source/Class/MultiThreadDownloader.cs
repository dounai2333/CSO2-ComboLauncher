using System;
using System.IO;
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

        private Thread Counter { get; set; }

        private bool Downloading { get; set; }
        
        public MultiThreadDownloader()
        {
            RequestBuilder = new SimpleWebRequestBuilder();
            DlChecker = new DownloadChecker();
            HttpDlBuilder = new SimpleDownloadBuilder(RequestBuilder, DlChecker);
            ResumingDlBuilder = new ResumingDownloadBuilder(10000, 100, 9999, HttpDlBuilder);
            AlreadyDownloadedRanges = null;
            SpeedMonitor = new DownloadSpeedMonitor(maxSampleCount: 512);
            ProgressMonitor = new DownloadProgressMonitor();

            Downloading = false;

            StartCounter();
        }

        public async Task<bool> DownloadFile(string link, string path, int threads, string hashchecktype = "", string hash = "")
        {
            if (CSO2_ComboLauncher.Download.Instance.IsVisible)
                return false;

            hash = hash.ToUpper();

            if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!Misc.IsFileAvailable(path))
                return false;
            else
                File.WriteAllText(path, string.Empty);

            Uri url = new Uri(link);
            FileInfo file = new FileInfo(path);
            Download = new MultiPartDownload(url, 8192, threads, ResumingDlBuilder, RequestBuilder, DlChecker, AlreadyDownloadedRanges);
            Download.DownloadCompleted += MultiPartDownload_OnCompleted;
            SpeedMonitor.Attach(Download);
            ProgressMonitor.Attach(Download);

            DlSaver = new DownloadToFileSaver(file);
            DlSaver.Attach(Download);

            Downloading = true;
            CSO2_ComboLauncher.Download.Instance.dwldfile.Text = Misc.PathLengthSaver(Path.GetFileName(path), 35);
            CSO2_ComboLauncher.Download.Instance.Show();

            Download.Start();

            while (Downloading || Download.State != DownloadState.Finished)
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

        private void StartCounter()
        {
            Counter = new Thread(() =>
            {
                while (true)
                {
                    Misc.Sleep(1000, false);
                    if (!Downloading || !CSO2_ComboLauncher.Download.Instance.IsVisible)
                        continue;

                    var alreadyDownloadedSizeInBytes = ProgressMonitor.GetCurrentProgressInBytes(Download);
                    var totalDownloadSizeInBytes = ProgressMonitor.GetTotalFilesizeInBytes(Download);
                    var currentSpeedInBytesPerSecond = SpeedMonitor.GetCurrentBytesPerSecond();
                    int currentProgressInPercent = (int)(ProgressMonitor.GetCurrentProgressPercentage(Download) * 100);

                    CSO2_ComboLauncher.Download.MainOutput(alreadyDownloadedSizeInBytes, totalDownloadSizeInBytes, currentSpeedInBytesPerSecond, currentProgressInPercent);
                }
            });
            Counter.Start();
        }

        void MultiPartDownload_OnCompleted(DownloadEventArgs args)
        {
            // this is an important thing to do after a download isn't used anymore, otherwise you will run into a memory leak.
            args.Download.DetachAllHandlers();
            
            Downloading = false;
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