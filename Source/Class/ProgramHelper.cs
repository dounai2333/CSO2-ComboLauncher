using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSO2_ComboLauncher
{
    static class ProgramHelper
    {
        public static Process Start(ProcessStartInfo startinfo)
        {
            Process process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = startinfo
            };
            process.Start();
            return process;
        }

        public static Process Start(string path, string arguments = "", bool withoutwindow = false)
        {
            ProcessStartInfo startinfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                CreateNoWindow = withoutwindow,
                WindowStyle = withoutwindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            return Start(startinfo);
        }

        public static async Task<int> StartAndGetExitCode(string path, string arguments = "", bool withoutwindow = false)
        {
            Process process = Start(path, arguments, withoutwindow);
            await Task.Run(() => process.WaitForExit());

            return process.ExitCode;
        }

        /// <summary>
        /// run cmd command, cmd will exit after executed the command.
        /// </summary>
        /// <param name="command">also support 'bat' file.</param>
        /// <param name="pausebeforeexit">show 'Press any key to continue' before exiting. will be forced to 'false' if 'withoutwindow' is true.</param>
        /// <param name="withoutwindow">if enabled, 'pausebeforeexit' will be force to 'false'.</param>
        public static Process CmdCommand(string command, bool pausebeforeexit = false, bool withoutwindow = false)
        {
            return Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "cmd.exe"), $"/C \"{command} {(!withoutwindow ? (pausebeforeexit ? "&& pause" : "") : "")}\"", withoutwindow);
        }
    }
}