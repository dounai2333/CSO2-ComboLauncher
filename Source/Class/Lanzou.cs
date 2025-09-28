using System.IO;
using System.Net;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;

namespace CSO2_ComboLauncher
{
    static class Lanzou
    {
        public static string Filename { get; private set; }

        public static string Referer { get; set; }

        /// <summary>
        /// get download link from Lanzou, if success global variable '<see cref="Filename"/>' will be set with the same filename from current link, else it's '<see cref="string.Empty"/>'.
        /// </summary>
        /// <param name="code">Password protected file is not supported.</param>
        /// <returns>download link or "address is not found."</returns>
        public static async Task<string> GetLink(string subdomain, string code)
        {
            Filename = string.Empty;
            Referer = string.Empty;

            string secondUrl = null;

            using (Web Web = new Web())
            {
                string downloadPage = await Web.Client.DownloadStringTaskAsync($"https://{subdomain}.lanzoul.com/{code}");

                // Anti-crawler
                if (downloadPage.Contains("acw_sc__v2"))
                {
                    BypassAntiCrawler(downloadPage, Web.Client.ResponseHeaders[HttpResponseHeader.SetCookie]);
                    return await GetLink(subdomain, code);
                }

                Regex regex = new Regex("(?<=href=\").+?(?=\")");
                foreach (Match href in regex.Matches(downloadPage))
                {
                    if (href.Value.StartsWith("/tp"))
                    {
                        secondUrl = $"https://{subdomain}.lanzoul.com{href.Value}";
                        break;
                    }
                }
            }

            if (secondUrl != null)
            {
                // We have to initialize again or it will bugged out.
                using (Web Web = new Web())
                {
                    string secondPage = await Web.Client.DownloadStringTaskAsync(secondUrl);

                    string url = null;
                    string url2 = null;

                    Regex regex = new Regex("'http[^;]+");
                    foreach (Match address in regex.Matches(secondPage))
                        if (address.Value.Contains("/file/"))
                            url = address.Value.Trim('\'');

                    Match match = new Regex(@"'\?[^;]+").Match(secondPage);
                    if (match.Success)
                        url2 = match.Value.Trim('\'');

                    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(url2))
                    {
                        Filename = new Regex("(?<=<title>).+?(?=</title>)").Match(secondPage).Value;

                        try
                        {
                            HttpWebRequest httpWebRequest = WebRequest.Create(url + url2) as HttpWebRequest;
                            httpWebRequest.Proxy = null;
                            httpWebRequest.Method = WebRequestMethods.Http.Head;
                            httpWebRequest.Timeout = 10000;
                            httpWebRequest.AllowAutoRedirect = false;

                            // Lanzou return code 302 when no User-Agent is given.
                            httpWebRequest.Headers[HttpRequestHeader.AcceptLanguage] = Web.Client.Headers[HttpRequestHeader.AcceptLanguage];
                            httpWebRequest.Headers[HttpRequestHeader.Cookie] = "down_ip=1; " + QQMail.Cookie;

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

        private static void BypassAntiCrawler(string page, string setCookies)
        {
            string arg1 = new Regex("(?<=arg1=').+?(?=')").Match(page).Value;
            if (!string.IsNullOrEmpty(arg1))
            {
                string mask = "3000176000856006061501533003690027800375";
                int[] posList = new int[] { 15, 35, 29, 24, 33, 16, 1, 38, 10, 9, 19, 31, 40, 27, 22, 23, 25, 13, 6, 11, 39, 18, 20, 8, 14, 21, 32, 26, 2, 30, 7, 4, 17, 5, 3, 28, 34, 37, 12, 36 };

                char[] outPutList = new char[posList.Length];
                for (int i = 0; i < arg1.Length; i++)
                {
                    char this_i = arg1[i];
                    for (int j = 0; j < posList.Length; j++)
                    {
                        if (posList[j] == i + 1)
                        {
                            outPutList[j] = this_i;
                            break;
                        }
                    }
                }

                string arg2 = new string(outPutList);

                StringBuilder arg3 = new StringBuilder();
                for (int i = 0; i < arg2.Length && i < mask.Length; i += 2)
                {
                    int strChar = int.Parse(arg2.Substring(i, 2), NumberStyles.HexNumber);
                    int maskChar = int.Parse(mask.Substring(i, 2), NumberStyles.HexNumber);
                    int xorResult = strChar ^ maskChar;
                    string xorChar = xorResult.ToString("x");
                    if (xorChar.Length == 1)
                        xorChar = "0" + xorChar;

                    arg3.Append(xorChar);
                }

                string acw_tc = new Regex("acw_tc=[^;]+").Match(setCookies).Value;
                string cdn_sec_tc = new Regex("cdn_sec_tc=[^;]+").Match(setCookies).Value;
                string acw_sc__v2 = "acw_sc__v2=" + arg3.ToString();

                QQMail.Cookie = string.Join("; ", new string[] { acw_tc, cdn_sec_tc, acw_sc__v2 }.Where(c => !string.IsNullOrEmpty(c)));
            }
        }
    }
}