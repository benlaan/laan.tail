using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Laan.Tools.Tail.Win
{
    [Serializable]
    [XmlRoot("Settings")]
    public class UserSettings : IUserSettings
    {
        private static string ConfigFileName = "config.xml";

        public UserSettings()
        {
            Appearance = new ConfigAppearance();
            Tail = new ConfigTail();
            Application = new ConfigApplication();
            Highlighters = new List<Highlighter>();
        }

        public static string ConfigFile
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LaanTail");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return Path.Combine(path, ConfigFileName);
            }
        }

        private void Compile()
        {
            foreach (var highlighter in Highlighters)
                highlighter.Compile();
        }

        public void Save()
        {
            XmlSerializer s = new XmlSerializer(typeof(UserSettings));
            using (FileStream stream = new FileStream(ConfigFile, FileMode.Create))
            {
                s.Serialize(stream, this);
            }
        }

        public static IUserSettings Load()
        {
            if (!File.Exists(ConfigFile))
            {
                var config = new UserSettings();
                config.Save();
                return config;
            }

            XmlSerializer s = new XmlSerializer(typeof(UserSettings));
            using (var stream = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var config = (UserSettings)s.Deserialize(stream);
                config.Compile();
                return config;
            }
        }

        [XmlIgnore]
        public IShell Shell { get; set; }
        
        [XmlIgnore]
        public bool WordWrap
        {
            get { return Application.WordWrap; }
            set
            {
                Application.WordWrap = value;
                Shell.ApplyConfig();
            }
        }

        #region IConfig Members

        public List<Highlighter> Highlighters { get; set; }
        public ConfigTail Tail { get; set; }
        public ConfigAppearance Appearance { get; set; }
        public ConfigApplication Application { get; set; }

        public ScrollBarVisibility ScrollbarVisibilty 
        { 
            get { return WordWrap ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto; }
        }

        public TextWrapping TextWrapping 
        { 
            get { return WordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap; }
        }

        #endregion
    }
}
