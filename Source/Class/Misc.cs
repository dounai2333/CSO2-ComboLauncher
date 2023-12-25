﻿using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Cryptography;

namespace CSO2_ComboLauncher
{
    static class Misc
    {
        public static List<string> IPSorter(List<string> iparray)
        {
            return iparray.Select(Version.Parse).OrderBy(arg => arg).Select(arg => arg.ToString()).ToList();
        }

        public static string PathLengthSaver(string path, int maxlength)
        {
            path = path.Replace("\\", "/");
            string file = Path.GetFileName(path);

            if (path.Length <= maxlength)
                return path;
            if (file.Length > maxlength)
                return file.Remove(maxlength - 3) + "...";
            if (file.Length + 3 >= maxlength)
                return "...".Substring(0, maxlength - file.Length) + file;

            string result = path.Remove(maxlength - file.Length);
            result = result.Remove(result.Length - 3);
            result += "..." + file;

            return result;
        }

        public static int GetPercentage(long smallnum, long largenum)
        {
            return (int)(smallnum * 100 / largenum);
        }

        public static string ChangeTextEncoding(string text, string from, string to)
        {
            return Encoding.GetEncoding(to).GetString(Encoding.GetEncoding(from).GetBytes(text));
        }

        public static async void Sleep(int ms, bool async = false)
        {
            if (async)
                await Sleep(ms);
            else
                Thread.Sleep(ms);
        }

        public static async Task Sleep(int ms)
        {
            await Task.Delay(ms);
        }

        public static async Task<int> TCPing(string target, int port = 80)
        {
            int time = -1;
            await Task.Run(() =>
            {
                try
                {
                    using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        sock.SendTimeout = 2000;
                        sock.ReceiveTimeout = 2000;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        sock.Connect(target, port);
                        stopwatch.Stop();
                        time = (int)stopwatch.Elapsed.TotalMilliseconds;
                    }
                }
                catch { }
            });
            return time;
        }

        public static bool IsTCPPortAvailable(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                if (tcpi.LocalEndPoint.Port == port)
                    return false;
            IPEndPoint[] ipEndPointArray = ipGlobalProperties.GetActiveTcpListeners();
            foreach (IPEndPoint ipendpoint in ipEndPointArray)
                if (ipendpoint.Port == port)
                    return false;

            return true;
        }

        public static string ConvertByteTo(long bytes, string to, bool withtypereturn)
        {
            if (bytes <= 0)
                return "0" + (withtypereturn ? "B" : "");

            string[] type = { "B", "KB", "MB", "GB", "TB", "BEST" };
            long[] large = { 1, 1024, 1048576, 1073741824, 1099511627776 };

            double converted = 0;
            int index = Array.IndexOf(type, to.ToUpper());
            if (index == 5)
            {
                for (int i = 0; i < large.Count(); i++)
                {
                    if (bytes < large[i])
                    {
                        converted = (double)bytes / large[i - 1];
                        index = i - 1;
                        break;
                    }
                }
            }
            else
            {
                converted = (double)bytes / large[index];
            }

            return converted.ToString("#.##") + (withtypereturn ? type[index] : "");
        }

        public static bool IsProcessHasAdminAccess()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <returns>true if net adapter is exist, false if not. reset will not work without administrator access.</returns>
        public static async Task<bool> ResetNetAdapter(string productorservicename)
        {
            bool exist = false;
            await Task.Run(() =>
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL"))
                {
                    if (searcher != null)
                    {
                        using (ManagementObjectCollection moc = searcher.Get())
                        {
                            if (moc != null && moc.Count > 0)
                            {
                                foreach (ManagementObject mo in moc)
                                {
                                    if (!exist && (mo.GetPropertyValue("ProductName")?.ToString() == productorservicename || mo.GetPropertyValue("ServiceName")?.ToString() == productorservicename))
                                    {
                                        if (IsProcessHasAdminAccess())
                                        {
                                            if (mo.GetPropertyValue("NetConnectionStatus")?.ToString() == "2")
                                            {
                                                try
                                                {
                                                    mo.InvokeMethod("Disable", null);
                                                    mo.InvokeMethod("Enable", null);
                                                }
                                                catch
                                                {
                                                    // safer way if first is failed due to "Object reference not set to an instance of an object".
                                                    ProgramHelper.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "netsh.exe"), $"interface set interface \"{mo.GetPropertyValue("NetConnectionID")}\" disable", true).WaitForExit();
                                                    ProgramHelper.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "netsh.exe"), $"interface set interface \"{mo.GetPropertyValue("NetConnectionID")}\" enable", true).WaitForExit();
                                                }
                                            }
                                            else if (mo.GetPropertyValue("NetConnectionStatus")?.ToString() == "0")
                                            {
                                                try
                                                {
                                                    mo.InvokeMethod("Enable", null);
                                                }
                                                catch
                                                {
                                                    ProgramHelper.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "netsh.exe"), $"interface set interface \"{mo.GetPropertyValue("NetConnectionID")}\" enable", true).WaitForExit();
                                                }
                                            }
                                        }
                                        exist = true;
                                    }
                                    mo?.Dispose();
                                }
                            }
                        }
                    }
                }
            });
            return exist;
        }

        public static bool IsFileAvailable(string file, FileAccess fa = FileAccess.ReadWrite)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, fa))
                    return true;
            }
            catch (Exception ex)
            {
                string code = DecimalToHex(ex.HResult);
                // 0x80070020 (ERROR_SHARING_VIOLATION): "The process cannot access the file because it is being used by another process."
                // 0x80070021 (ERROR_LOCK_VIOLATION): "The process cannot access the file because another process has locked a portion of the file."
                return code != "0x80070020" && code != "0x80070021";
            }
        }

        /// <param name="srcText">only unicode text accepted, else exception is thrown.</param>
        public static string UnicodeToString(string srcText)
        {
            string dst = "";
            string src = srcText;
            int len = srcText.Length / 6;

            for (int i = 0; i < len; i++)
            {
                string str = src.Substring(0, 6).Substring(2);
                src = src.Substring(6);
                byte[] bytes = new byte[2];
                bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }
            return dst;
        }

        public static string StringToHex(string input, string encode = "UTF-8")
        {
            string result = "";
            byte[] bytes = Encoding.GetEncoding(encode).GetBytes(input);

            foreach (byte text in bytes)
                result += text.ToString("X2");

            return result;
        }

        public static string HexToString(string input, string encode = "UTF-8")
        {
            byte[] bytes = new byte[input.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);

            return Encoding.GetEncoding(encode).GetString(bytes);
        }

        public static string DecimalToHex(int input, bool withhexmark = true)
        {
            return (withhexmark ? "0x" : "") + input.ToString("X").ToUpper();
        }

        public static int HexToDecimal(string input)
        {
            return Convert.ToInt32(input, 16);
        }

        public static string[] SplitString(string input, bool unixstyle = false)
        {
            return unixstyle ? input.Split(char.Parse("\n")) : input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        /// <param name="type">CRC is not supported.</param>
        public static async Task<string> GetHash(string file, string type, bool uppertext = true)
        {
            if (!IsFileAvailable(file, FileAccess.Read))
                return "";

            byte[] fb = { };
            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    fb = ((HashAlgorithm)CryptoConfig.CreateFromName(type)).ComputeHash(fs);
            });

            string hash = BitConverter.ToString(fb).Replace("-", "");
            return uppertext ? hash.ToUpper() : hash.ToLower();
        }

        public static string Encrypt(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            string str = "";
            string[] line = SplitString(content);
            for (int i = 0; i < line.Count(); i++)
            {
                if (i == line.Count() - 1)
                    str += Convert.ToBase64String(Encoding.UTF8.GetBytes(line[i]));
                else
                    str += Convert.ToBase64String(Encoding.UTF8.GetBytes(line[i])) + Environment.NewLine;
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        public static string Decrypt(string content)
        {
            if (string.IsNullOrEmpty(content) || !IsBase64String(content))
                return "";

            string str = "";
            string temp = Encoding.UTF8.GetString(Convert.FromBase64String(content));
            string[] line = SplitString(temp);
            for (int i = 0; i < line.Count(); i++)
            {
                if (i == line.Count() - 1)
                    str += Encoding.UTF8.GetString(Convert.FromBase64String(line[i]));
                else
                    str += Encoding.UTF8.GetString(Convert.FromBase64String(line[i])) + Environment.NewLine;
            }
            return str;
        }

        public static bool IsBase64String(string content)
        {
            content = content.Trim();
            return (content.Length % 4 == 0) && new Regex(@"^[a-zA-Z0-9\+/]*={0,3}$").IsMatch(content);
        }

        public static bool EncryptFile(string file, string newfile)
        {
            if (!File.Exists(file))
                return false;

            string encrypted = Encrypt(File.ReadAllText(file, Encoding.UTF8));
            File.WriteAllText(newfile, encrypted, Encoding.UTF8);
            return true;
        }

        public static bool DecryptFile(string file, string newfile)
        {
            if (!File.Exists(file))
                return false;

            string text = File.ReadAllText(file, Encoding.UTF8);
            if (IsBase64String(text))
            {
                string decrypted = Decrypt(text);
                File.WriteAllText(newfile, decrypted, Encoding.UTF8);
                return true;
            }

            return false;
        }
    }
}