using System.Collections.Generic;
using System.Text.RegularExpressions;

using Chsword;

namespace CSO2_ComboLauncher
{
    static class Json
    {
        public static dynamic Parse(string jsonstring)
        {
            // JDynamic doesn't support \" in json string.
            jsonstring = jsonstring.Replace("\\\"", "'").Replace("\\n", "\n");

            MatchCollection mc = new Regex(@"\\u[a-zA-Z0-9]{4}").Matches(jsonstring);
            foreach (Match match in mc)
                jsonstring = jsonstring.Replace(match.Value, Misc.UnicodeToString(match.Value));

            return new JDynamic(jsonstring);
        }

        public static string ReadValue(dynamic parsedjson, string key)
        {
            return parsedjson[key].ToString();
        }

        public static List<string> ReadArray(dynamic array)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < array.Length; i++)
                result.Add(array[i].ToString());
            return result;
        }
    }
}