using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeSyncService
{
    public class IniFileReader
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue,
                                                        char[] retVal, int size, string filePath);

        public static string ReadValue(string section, string key, string filePath)
        {
            var buffer = new char[255];
            GetPrivateProfileString(section, key, "", buffer, 255, filePath);
            return new string(buffer).Trim('\0');
        }

        public static string[] ReadAllValues(string section, string filePath)
        {
            var buffer = new char[4096];
            GetPrivateProfileString(section, null, "", buffer, 4096, filePath);
            var result = new string(buffer).Trim('\0').Split('\0');
            return result;
        }
    }
}