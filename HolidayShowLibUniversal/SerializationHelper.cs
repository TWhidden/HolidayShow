using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace HolidayShowLibUniversal
{
    public static class SerializationHelper
    {
        public static string Serialize<T>(T data)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(memoryStream, data);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var reader = new StreamReader(memoryStream);
                var content = reader.ReadToEnd();
                return "<?xml version=\"1.0\" encoding=\"UTF-16\" ?>" + System.Environment.NewLine + content;
            }
        }

        public static T Deserialize<T>(string xml)
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {
                var serializer = new DataContractSerializer(typeof(T));
                var theObject = (T)serializer.ReadObject(stream);
                return theObject;
            }
        }

    }
}
