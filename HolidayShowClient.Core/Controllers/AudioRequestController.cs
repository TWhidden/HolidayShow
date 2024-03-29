﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HolidayShowClient.Core.Containers;
using HolidayShowLibShared.Core.Services;
using HolidayShowLibUniversal.Controllers;

namespace HolidayShowClient.Core.Controllers
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
            _rootStoragePath = Path.Combine(HolidayShowClient.Core.Program.StoragePath, StoragePathFolder);
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
        /// The Filename requested for playback. This shouldn't be the path. Just the filename.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The Path of where to download the file if its not available on the system.
        /// </summary>
        /// <summary>
        /// Verify that the file exists, and pull the file from the server if its not available
        /// Then returns the path of the file. If the file can not be found or acquired, it will return an empty string.
        /// </summary>
        /// <returns></returns>
        public async Task<Uri> FileReady()
        {
            // Find out if the file exists.
            var audioPath = Path.Combine(_rootStoragePath, Regex.Replace(FileName, "[^a-zA-Z0-9.\\-]", "_"));
            if (File.Exists(audioPath))
            {
                Console.WriteLine($"Audio File Exists: {audioPath}");
                return new Uri(audioPath);
            }

            Console.WriteLine($"Audio File DOES NOT Exist: {audioPath}");

            var fd = new FileDownloadContainer(FileName, audioPath);

            // Download the file from the server
            var downloadClient = _resolverService.Resolve<FileDownloadClient>(fd);
            if (!await downloadClient.FileFinished())
            {
                Console.WriteLine($"FileDownloadClient returned false for downloading. Cant download");
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
