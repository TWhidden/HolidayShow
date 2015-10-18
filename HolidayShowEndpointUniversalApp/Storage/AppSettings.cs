using System.IO;
using System.Runtime.Serialization;
using HolidayShowLibUniversal.Helpers;

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
#if !DEBUG
                var s = new AppSettings
                {
                    ServerAddress = "127.0.0.0",
                    ServerPort = 5555
                };
                s.Save();
#else
                var s = new AppSettings
                {
                    ServerAddress = "10.64.128.245",
                    ServerPort = 5555,
                    DeviceId = 5
                };
                s.Save();
#endif

            }

        }

        public AppSettings()
        {
          
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
