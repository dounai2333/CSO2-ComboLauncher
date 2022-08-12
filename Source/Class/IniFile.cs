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
                    string currentRoot = "";
                    string[] keyPair = { };

                    for (string strLine = reader.ReadLine(); strLine != null; strLine = reader.ReadLine())
                    {
                        strLine = strLine.Trim();
                        if (strLine.StartsWith("//"))
                            continue;

                        if (!string.IsNullOrWhiteSpace(strLine))
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                if (strLine.Contains("//"))
                                    strLine = strLine.Substring(0, strLine.IndexOf("//") - 1);

                                keyPair = strLine.Split(new char[] { '=' }, 2);
                                SectionPair pair;

                                if (currentRoot == null)
                                    currentRoot = "ROOT";

                                pair.Section = currentRoot;
                                pair.Key = keyPair[0];

                                this.keyPair.Add(pair, keyPair.Length > 1 ? keyPair[1] : null);
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
                        string tmpValue = keyPair[sectionPair];

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