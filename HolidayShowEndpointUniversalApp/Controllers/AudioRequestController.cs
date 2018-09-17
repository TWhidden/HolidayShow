using System;
using System.IO;
using System.Threading.Tasks;
using HolidayShowEndpointUniversalApp.Containers;
using HolidayShowLibUniversal.Controllers;
using HolidayShowLibUniversal.Services;

namespace HolidayShowEndpointUniversalApp.Controllers
{
    public class AudioRequestController : IAudioRequestController
    {
        private readonly IResolverService _resolverService;
        private const string StoragePathFolder = "AudioFiles";

        private readonly string _rootStoragePath;

        public AudioRequestController(IResolverService resolverService)
        {
            _resolverService = resolverService;
#if CORE
            _rootStoragePath = HolidayShowClient.Core.Program.StoragePath;
#else
            var applicationData = Windows.Storage.ApplicationData.Current;
            var localFolder = applicationData.LocalFolder.Path;
            _rootStoragePath = Path.Combine(localFolder, StoragePathFolder);
#endif

            // Validate the path exists. If it doesnt, create it
            if (!Directory.Exists(_rootStoragePath))
            {
                Directory.CreateDirectory(_rootStoragePath);
            }
        }

        public void Stop()
        {
            InvokeOnStop();
        }

        /// <summary>
        /// The Filename requested for playback. This shouldnt be the path. Just the filename.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The Path of where to download the file if its not available on the system.
        /// </summary>
        /// <summary>
        /// Verify that the file exists, and pull the file from the server if its not available
        /// Then returns the path of the file. If the file can not be found or aquired, it will return an empty string.
        /// </summary>
        /// <returns></returns>
        public async Task<Uri> FileReady()
        {
            // Find out if the file exists.
            var audioPath = Path.Combine(_rootStoragePath, FileName);
            if (File.Exists(audioPath)) return new Uri(audioPath);

            var fd = new FileDownloadContainer(FileName, audioPath);

            // Download the file from the serfer
            var fileDownloader = _resolverService.Resolve<FileDownloadClient>(fd);
            if (!await fileDownloader.FileFinsihed())
            {
                return null;
            }
            return new Uri(audioPath);
        }

        public event EventHandler OnStop;

        protected virtual void InvokeOnStop()
        {
            OnStop?.Invoke(this, EventArgs.Empty);
        }
    }
}
