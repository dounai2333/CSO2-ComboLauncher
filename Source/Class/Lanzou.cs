using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CSO2_ComboLauncher
{
    static class Lanzou
    {
        public static string Filename { get; private set; }

        public static string Referer { get; private set; }

        /// <summary>
        /// get download link from Lanzou, if success global variable '<see cref="Filename"/>' will be set with the same filename from current link, else it's '<see cref="string.Empty"/>'.
        /// </summary>
        /// <param name="code">Password protected file is not supported.</param>
        /// <returns>download link or "address is not found."</returns>
        public static async Task<string> GetLink(string subdomain, string code)
        {
            Filename = string.Empty;
            Referer = string.Empty;

            using (Web Web = new Web())
            {
                string page = await Web.Client.DownloadStringTaskAsync($"https://{subdomain}.lanzoul.com/tp/{code}");

                string url = null;
                string url2 = null;

                // This detection still exists.
                //bool killdns = page.IndexOf("killdns") != -1;

                Regex regex = new Regex("'http[^;]+");
                foreach (Match address in regex.Matches(page))
                    if (address.Value.Contains("/file/"))
                        url = address.Value.Replace("'", "");

                Match match = new Regex(@"'\?[^;]+").Match(page);
                if (match.Success)
                    url2 = match.Value.Replace("'", "");

                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(url2))
                {
                    Filename = new Regex("(?<=<title>).+?(?=</title>)").Match(page).Value;

                    try
                    {
                        HttpWebRequest httpWebRequest = WebRequest.Create(url + url2) as HttpWebRequest;
                        httpWebRequest.Proxy = null;
                        httpWebRequest.Method = WebRequestMethods.Http.Head;
                        httpWebRequest.Timeout = 10000;
                        httpWebRequest.AllowAutoRedirect = false;

                        // Lanzou return code 302 when no User-Agent is given.
                        httpWebRequest.Headers[HttpRequestHeader.AcceptLanguage] = Web.Client.Headers[HttpRequestHeader.AcceptLanguage];
                        httpWebRequest.Headers[HttpRequestHeader.Cookie] = "down_ip=1"; // Remove?

                        using (HttpWebResponse httpWebResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
                        {
                            // we only need header info, not the response stream.
                            httpWebResponse.GetResponseStream().Dispose();

                            if (httpWebResponse.StatusCode == HttpStatusCode.Redirect)
                            {
                                Referer = url + url2;

                                string address = httpWebResponse.Headers.Get("Location");
                                if (!string.IsNullOrEmpty(address))
                                    return address;
                            }
                        }
                    }
                    catch // Cannot reach the final download link?
                    {
                        return url + url2;
                    }
                }
            }

            return "address is not found.";
        }

        /// <param name="path">if a folder is given, file name will be from Lanzou server.</param>
        public static async Task<bool> DownloadFile(string path, int threads, string subdomain, string code, string hashchecktype = "", string hash = "")
        {
            string address = await GetLink(subdomain, code);
            if (!address.StartsWith("http"))
                return false;

            return await Downloader.FileFromHttp(address, (Path.GetFileName(path) == string.Empty) ? path + Filename : path, threads, hashchecktype, hash);
        }

        public static async Task<string> DownloadString(string subdomain, string code)
        {
            string address = await GetLink(subdomain, code);
            if (!address.StartsWith("http"))
                return null;

            return await Downloader.StringFromHttp(address);
        }
    }
}