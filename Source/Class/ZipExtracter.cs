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

        /// <param name="stream">Stream will be disposed.</param>
        public static async Task<bool> Extract(Stream stream, string unzippath, bool allowoverwrite = true)
        {
            using (ZipArchive ZipArchive = new ZipArchive(stream))
            {
                return await Extract(ZipArchive, unzippath, allowoverwrite);
            }
        }

        /// <param name="archive">Archive will be disposed.</param>
        /// <returns>'true' if extract is done, 'false' if error or something.</returns>
        public static async Task<bool> Extract(ZipArchive archive, string unzippath, bool allowoverwrite = true)
        {
            if (archive == null)
                return false;

            ZipWorker.Instance.Show();

            int totalfiles = archive.Entries.Count();
            for (int i = 0; i < totalfiles; i++)
            {
                ZipArchiveEntry target = archive.Entries[i];
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
                    archive.Dispose();
                    return false;
                }
            }

            ZipWorker.ResetStatus();
            archive.Dispose();
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