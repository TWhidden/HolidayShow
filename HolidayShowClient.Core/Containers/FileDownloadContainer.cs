namespace HolidayShowClient.Core.Containers
{
    public class FileDownloadContainer
    {
        public FileDownloadContainer(string fileName, string writePath)
        {
            FileName = fileName;
            DestinationPath = writePath;
        }

        public string FileName { get; }

        public string DestinationPath { get; }
    }
}
