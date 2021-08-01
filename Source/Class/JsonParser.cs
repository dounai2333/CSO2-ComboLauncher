using System.Collections.Generic;

using Chsword;

namespace CSO2_ComboLauncher
{
    static class Json
    {
        public static dynamic Parse(string jsonstring)
        {
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
