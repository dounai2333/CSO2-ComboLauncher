using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CSO2_ComboLauncher
{
    class Ftp : IDisposable
    {
        public FtpWebRequest Request { get; private set; }

        public FtpWebResponse Response { get; private set; }

        private string ServerAddress { get; set; }

        private string Username { get; set; }

        private string Password { get; set; }

        public Ftp(string server, string username = "anonymous", string password = "")
        {
            ServerAddress = server.Replace("ftp://", "");
            Username = username;
            Password = password;

            Request = null;
            Response = null;
        }

        private void Refresh(string path)
        {
            Request = (FtpWebRequest)WebRequest.Create($"ftp://{ServerAddress}/{path}");
            Request.Credentials = new NetworkCredential(Username, Password);

            Request.Proxy = null;
            Request.Timeout = 10000;
            Request.ReadWriteTimeout = 60000;
        }

        public async Task<bool> DownloadFile(string path, string serverpath)
        {
            if (!Misc.IsFileAvailable(path))
                return false;

            bool done = false;
            Stream stream = Stream.Null;
            await Task.Run(() =>
            {
                stream = GetStream(serverpath, WebRequestMethods.Ftp.DownloadFile);
                if (stream != Stream.Null)
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                        stream.CopyTo(fs);
                    done = true;
                }
            });
            stream.Dispose();
            return done;
        }

        public async Task<string> DownloadString(string serverpath)
        {
            string str = "";
            Stream stream = Stream.Null;
            await Task.Run(() =>
            {
                stream = GetStream(serverpath, WebRequestMethods.Ftp.DownloadFile);
                if (stream != Stream.Null)
                {
                    using (StreamReader sr = new StreamReader(stream))
                        str = sr.ReadToEnd();
                }
            });
            stream.Dispose();
            return str;
        }

        public async Task<bool> VerifyConnection()
        {
            FtpStatusCode code = FtpStatusCode.Undefined;
            await Task.Run(() =>
            {
                code = SendCommand(WebRequestMethods.Ftp.PrintWorkingDirectory);
            });
            return code == FtpStatusCode.PathnameCreated;
        }

        public FtpStatusCode SendCommand(string command)
        {
            Refresh(string.Empty);

            Request.Method = command;
            Response = (FtpWebResponse)Request.GetResponse();
            return Response.StatusCode;
        }

        private Stream GetStream(string serverpath, string command)
        {
            Refresh(serverpath);

            Request.Method = command;
            Response = (FtpWebResponse)Request.GetResponse();
            return Response.GetResponseStream();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request = null;
                Response?.Dispose();
            }
        }
    }
}