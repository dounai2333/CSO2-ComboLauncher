using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CSO2_ComboLauncher
{
    static class QQMail
    {
        public static string Filename { get; private set; }

        /// <summary>
        /// get download link from QQmail, if success global variable '<see cref="Filename"/>' will be set with the same filename from current link, else it's '<see cref="string.Empty"/>'.
        /// </summary>
        /// <returns>download link or error message.</returns>
        public static async Task<string> GetLink(string code, string key)
        {
            Filename = string.Empty;

            using (Web Web = new Web())
            {
                string page = await Web.Client.DownloadStringTaskAsync($"https://mail.qq.com/cgi-bin/ftnExs_download?t=exs_ftn_download&code={code}&k={key}");
                Regex regex = new Regex("http[^\"]+");

                foreach (Match address in regex.Matches(page))
                {
                    if (address.Value.Contains("ftn.qq.com"))
                    {
                        Match filenameMatch = new Regex("(?<=fname=).+?(?=&)").Match(address.Value);
                        if (filenameMatch.Success)
                        {
                            Filename = FixFilename(filenameMatch.Value);
                        }
                        else
                        {
                            filenameMatch = new Regex("(?<=\"ft_d_filename\">).+?(?=</div>)").Match(page);
                            if (filenameMatch.Success)
                                Filename = filenameMatch.Value;
                        }

                        return address.Value;
                    }
                }

                Match messageMatch = new Regex("(?<=\"ft_d_error infobar error\" ><p>).+?(?=</p>)|(?<=\"ft_d_error infobar error\"><p>).+?(?=</p>)").Match(page);
                if (messageMatch.Success)
                    return messageMatch.Value.Replace("<br />", "\n").Replace("<br>", "\n");

                return "address is not found.";
            }
        }

        /// <param name="path">if a folder is given, file name will be from QQmail server.</param>
        public static async Task<bool> DownloadFile(string path, int threads, string code, string sha1, string key)
        {
            string address = await GetLink(code, key);
            if (!address.StartsWith("http"))
                return false;

            return await Downloader.FileFromHttp(address, (Path.GetFileName(path) == string.Empty) ? path + Filename : path, threads, "sha1", sha1);
        }

        public static async Task<string> DownloadString(string code, string key)
        {
            string address = await GetLink(code, key);
            if (!address.StartsWith("http"))
                return null;

            return await Downloader.StringFromHttp(address);
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