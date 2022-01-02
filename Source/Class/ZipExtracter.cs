using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace CSO2_ComboLauncher
{
    static class Zip
    {
        public static async Task<bool> Extract(string file, string unzippath, bool allowoverwrite = true)
        {
            return await Extract(FileToArchive(file), unzippath, allowoverwrite);
        }

        /// <returns>true if extract is done, false if error or something. the source archive will be disposed.</returns>
        public static async Task<bool> Extract(ZipArchive file, string unzippath, bool allowoverwrite = true)
        {
            if (file == null)
                return false;

            ZipWorker.Instance.Show();

            int totalfiles = file.Entries.Count();
            for (int i = 0; i < totalfiles; i++)
            {
                ZipArchiveEntry target = file.Entries[i];
                if (string.IsNullOrEmpty(target.Name))
                    continue;

                ZipWorker.Instance.zipwkfile.Text = Misc.PathLengthSaver(target.FullName, 35);
                ZipWorker.MainOutput(i, totalfiles);

                string targetpath = unzippath + "\\" + target.FullName;
                if (!string.IsNullOrEmpty(Path.GetDirectoryName(targetpath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(targetpath));
                try
                {
                    await Task.Run(() => target.ExtractToFile(targetpath, allowoverwrite));
                }
                catch
                {
                    ZipWorker.ResetStatus();
                    file.Dispose();
                    file = null;

                    return false;
                }
            }

            ZipWorker.ResetStatus();
            file.Dispose();
            file = null;

            return true;
        }

        public static ZipArchive FileToArchive(string file)
        {
            try
            {
                if (File.Exists(file))
                    return ZipFile.OpenRead(file);
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
    }
}