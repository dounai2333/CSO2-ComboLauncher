using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CSO2_ComboLauncher
{
    // 马卡大大遗物
    public class IniParser
    {
        private string _path { get; set; }

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
                using (TextReader reader = new StreamReader(path))
                {
                    string strLine = "";
                    string currentRoot = "";
                    string[] keyPair = { };

                    for (strLine = reader.ReadLine(); strLine != null; strLine = reader.ReadLine())
                    {
                        strLine = strLine.Trim();
                        if (strLine.Trim().StartsWith("//"))
                        {
                            continue;
                        }
                        if (!string.IsNullOrWhiteSpace(strLine))
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
            catch
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
                strToSave += "[" + section + "]\r\n";

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

            using (TextWriter tw = new StreamWriter(filepath))
                await tw.WriteAsync(strToSave);
        }

        public async void SaveSettings()
        {
            await SaveSettings(_path);
        }
    }
}