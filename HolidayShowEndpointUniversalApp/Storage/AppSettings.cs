using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HolidayShowLibUniversal;

namespace HolidayShowEndpointUniversalApp.Storage
{
    [DataContract]
    public class AppSettings
    {
        private const string SETTING_NAME = "HolidayShowSettings.xml";

        private static readonly string SettingsFilePath;

        static AppSettings()
        {
            var applicationData = Windows.Storage.ApplicationData.Current;
            var localFolder = applicationData.LocalFolder;
            SettingsFilePath = Path.Combine(localFolder.Path, SETTING_NAME);
            // Validate the file exists, and if it doesnt, add it
            if (!File.Exists(SettingsFilePath))
            {
                var s = new AppSettings();
                s.ServerAddress = "127.0.0.0";
                s.ServerPort = 5555;
                s.Save();

            }

        }

        public AppSettings()
        {
            DeviceId = 4;
            ServerAddress = "10.64.128.75";
            ServerPort = 5556;
        }

        [DataMember]
        public string ServerAddress { get; set; }

        [DataMember]
        public ushort ServerPort { get; set; }

        [DataMember]
        public int DeviceId { get; set; }


        public static AppSettings Load()
        {
            var data = File.ReadAllText(SettingsFilePath);
            return SerializationHelper.Deserialize<AppSettings>(data);
        }

        public void Save()
        {
            var data = SerializationHelper.Serialize(this);
            File.WriteAllText(SettingsFilePath, data);
        }
    }
}
