using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Win32;

namespace Laan.Tools.Tail.Services
{
    public class RegistryReader
    {
        private static string GetDefaultValue(string extension)
        {
            using (var progId = Registry.ClassesRoot.OpenSubKey(extension))
            {
                return progId.GetValue(null).ToString();
            }
        }

        public static string GetRegisteredApplication(string fileName, string extension)
        {
            string progIdValue = GetDefaultValue("." + extension);

            // Get associated application
            string value = GetDefaultValue(progIdValue + "\\shell\\open\\command");
            Regex regex = new Regex(@"\""?%\d\""?");
            return regex.Replace(value, "").Trim();
        }
    }
}
