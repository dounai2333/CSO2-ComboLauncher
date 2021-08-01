using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CSO2_ComboLauncher
{
    // 马卡大大遗物
    public class IniParser
    {
        private string _path;
        private struct SectionPair
        {
            public string Section;
            public string Key;
        }

        private readonly Dictionary<SectionPair, string> keyPair = new Dictionary<SectionPair, string>();

        public IniParser(string path)
        {
            _path = path;
            if (File.Exists(path))
            {
                TextReader reader = new StreamReader(path);
                string strLine = null;
                string currentRoot = null;
                string[] keyPair = null;
                try
                {
                    for (strLine = reader.ReadLine(); strLine != null; strLine = reader.ReadLine())
                    {
                        strLine = strLine.Trim();
                        if (strLine.StartsWith("//"))
                        {
                            continue;
                        }
                        if (strLine != "")
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                if (strLine.Contains("//"))
                                {
                                    strLine = strLine.Substring(0, strLine.IndexOf("//") - 1);
                                }

                                keyPair = strLine.Split(new char[] { '=' }, 2);
                                SectionPair pair;
                                string value = null;

                                if (currentRoot == null)
                                    currentRoot = "ROOT";

                                pair.Section = currentRoot;
                                pair.Key = keyPair[0];

                                if (keyPair.Length > 1)
                                    value = keyPair[1];

                                this.keyPair.Add(pair, value);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    reader.Close();
                }
            }
            else
            {
                //throw new FileNotFoundException("Unable to locate " + path);
            }
        }

        public string GetSetting(string sectionName, string settingName)
        {
            try
            {
                SectionPair sectionPair;
                sectionPair.Section = sectionName;
                sectionPair.Key = settingName;

                return keyPair[sectionPair];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public void AddSetting(string sectionName, string settingName, string settingValue)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;

            if (keyPair.ContainsKey(sectionPair))
                keyPair.Remove(sectionPair);

            keyPair.Add(sectionPair, settingValue);
        }

        public void AddSetting(string sectionName, string settingName)
        {
            AddSetting(sectionName, settingName, null);
        }

        public void DeleteSetting(string sectionName, string settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;

            if (keyPair.ContainsKey(sectionPair))
                keyPair.Remove(sectionPair);
        }

        public async Task SaveSettings(string filepath)
        {
            List<string> sections = new List<string>();
            string tmpValue = "";
            string strToSave = "";

            foreach (SectionPair sectionPair in keyPair.Keys)
            {
                if (!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }

            foreach (string section in sections)
            {
                strToSave += ("[" + section + "]\r\n");

                foreach (SectionPair sectionPair in keyPair.Keys)
                {
                    if (sectionPair.Section == section)
                    {
                        tmpValue = keyPair[sectionPair];

                        if (tmpValue != null)
                            tmpValue = "=" + tmpValue;

                        strToSave += (sectionPair.Key + tmpValue + "\r\n");
                    }
                }

                strToSave += "\r\n";
            }

            try
            {
                TextWriter tw = new StreamWriter(filepath);
                await tw.WriteAsync(strToSave);
                tw.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async void SaveSettings()
        {
            await SaveSettings(_path);
        }
    }
}