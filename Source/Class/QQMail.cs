using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CSO2_ComboLauncher
{
    static class QQMail
    {
        public static string Filename { get; private set; }

        public static string Mail5k { get; private set; }

        /// <summary>
        /// get download link from QQmail, if success global variable '<see cref="Filename"/>' will be set with the same filename from current link, else it's '<see cref="string.Empty"/>'.
        /// </summary>
        /// <returns>download link or "address is not found."</returns>
        public static async Task<string> GetLink(string code, string key)
        {
            Filename = string.Empty;
            Mail5k = string.Empty;

            try
            {
                HttpWebRequest httpWebRequest = WebRequest.Create($"https://wx.mail.qq.com/ftn/download?func=4&code={code}&key={key}") as HttpWebRequest;
                httpWebRequest.Proxy = null;
                httpWebRequest.Method = WebRequestMethods.Http.Head;
                httpWebRequest.Timeout = 10000;
                httpWebRequest.AllowAutoRedirect = false;

                httpWebRequest.Headers[HttpRequestHeader.AcceptLanguage] = "zh-CN,zh;q=0.9";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.6045.200 Safari/537.36";

                using (HttpWebResponse httpWebResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
                {
                    // we only need header info, not the response stream.
                    httpWebResponse.GetResponseStream().Dispose();

                    if (httpWebResponse.StatusCode == HttpStatusCode.Redirect)
                    {
                        string address = httpWebResponse.Headers.Get("Location");
                        if (!string.IsNullOrEmpty(address))
                        {
                            Match mail5kmatch = new Regex("mail5k=[^;]+").Match(httpWebResponse.Headers.Get("Set-Cookie"));
                            if (mail5kmatch.Success)
                                Mail5k = mail5kmatch.Value;

                            Match filenameMatch = new Regex("(?<=fname=).+?(?=&)").Match(address);
                            if (filenameMatch.Success)
                                Filename = FixFilename(filenameMatch.Value);

                            return address;
                        }
                    }
                }
            }
            catch /*(WebException webEx)*/
            {
                //HttpWebResponse httpWebResponse = webEx.Response as HttpWebResponse;
            }

            return "address is not found.";
        }

        /// <param name="path">if a folder is given, file name will be from QQmail server.</param>
        public static async Task<bool> DownloadFile(string path, int threads, string code, string key, string hashchecktype = "", string hash = "")
        {
            string address = await GetLink(code, key);
            if (!address.StartsWith("http"))
                return false;

            return await Downloader.FileFromHttp(address, (Path.GetFileName(path) == string.Empty) ? path + Filename : path, threads, hashchecktype, hash);
        }

        public static async Task<bool> DownloadFile(string path, int threads, string[] code, string[] key, string hashchecktype = "", string hash = "")
        {
            if (code.Count() != key.Count())
                return false;

            int[] indexes = new int[key.Count()];
            for (int i = 0; i < indexes.Length; i++)
                indexes[i] = i;

            Misc.ShuffleArray(ref indexes);

            for (int i = 0; i < indexes.Count(); i++)
                if (await DownloadFile(path, threads, code[indexes[i]], key[indexes[i]], hashchecktype, hash))
                    return true;

            return false;
        }

        public static async Task<string> DownloadString(string code, string key)
        {
            string address = await GetLink(code, key);
            if (!address.StartsWith("http"))
                return null;

            return await Downloader.StringFromHttp(address);
        }

        public static async Task<string> DownloadString(string[] code, string[] key)
        {
            if (code.Count() != key.Count())
                return null;

            int[] indexes = new int[key.Count()];
            for (int i = 0; i < indexes.Length; i++)
                indexes[i] = i;

            Misc.ShuffleArray(ref indexes);

            for (int i = 0; i < indexes.Count(); i++)
            {
                string result = await DownloadString(code[indexes[i]], key[indexes[i]]);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static string FixFilename(string filename)
        {
            filename = filename.Replace("+", " ");
            Match match = new Regex("(%[a-zA-Z0-9]{2})+").Match(filename);
            if (match.Success)
            {
                string result = Misc.HexToString(match.Value.Replace("%", ""));
                filename = filename.Replace(match.Value, result);
            }

            return filename;
        }
    }
}