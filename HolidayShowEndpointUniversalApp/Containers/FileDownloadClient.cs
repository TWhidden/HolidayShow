using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HolidayShowEndpointUniversalApp.Services;
using HolidayShowLib;

namespace HolidayShowEndpointUniversalApp.Containers
{
    public class FileDownloadClient : ProtocolClient
    {
        private readonly FileDownloadContainer _fileDownloadContainer;
        
        readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public FileDownloadClient(IServerDetails endPoint, FileDownloadContainer fileDownloadContainer) : base(endPoint)
        {
            _fileDownloadContainer = fileDownloadContainer;
        }

        public override void FileRequestReceived(ProtocolMessage message)
        {
            try
            {
                // Get the guts of the message here
                if (message.MessageParts.ContainsKey(ProtocolMessage.FILEBYTES))
                {
                    // Make sure it doesn't exist first
                    if (!File.Exists(_fileDownloadContainer.DestinationPath))
                    {
                        // Ensure the entire path is available, because files might be in sub directories for
                        // the storage
                        var pathOnly = Path.GetDirectoryName(_fileDownloadContainer.DestinationPath);
                        if (!Directory.Exists(pathOnly))
                        {
                            Console.WriteLine($"Creating '{pathOnly}'");
                            Directory.CreateDirectory(pathOnly);
                            Console.WriteLine($"Created '{pathOnly}'");
                        }

                        var fileBytes = Convert.FromBase64String(message.MessageParts[ProtocolMessage.FILEBYTES]);

                        Console.WriteLine($"Writing {fileBytes.Length:N} bytes to file {_fileDownloadContainer.DestinationPath}");
                        File.WriteAllBytes(_fileDownloadContainer.DestinationPath, fileBytes);
                    }
                    _tcs.SetResult(true);
                }
                else
                {
                    _tcs.SetResult(false);
                }
            }
            catch
            {
                _tcs.SetResult(false);
            }
            finally
            {
                Disconnect(false);
            }
        }

        protected override void NewConnectionEstablished()
        {
            Console.WriteLine($"Requesting Missing File '{_fileDownloadContainer.FileName}'");

            var dic = new Dictionary<string, string>
                {
                    {ProtocolMessage.FILEDOWNLOAD, _fileDownloadContainer.FileName},
                };

            var message = new ProtocolMessage(MessageTypeIdEnum.RequestFile, dic);
#if CORE
            BeginSend(message);
#else
            if (!BeginSend(message))
            {
                Disconnect(false);
                _tcs.SetResult(false);
            }
#endif
        }

        protected override void ErrorDetected(Exception ex)
        {
            Console.WriteLine($"Error detected in FileDownloadClient: {ex.Message}");
            Disconnect(false);
            _tcs.SetResult(false);
        }

        protected override void ResetReceived()
        {
            Console.WriteLine("Reset Received in FileDownloadClient");
            //Disconnect(false);
            //_tcs.SetResult(false);
        }

        protected override void EventControlReceived(ProtocolMessage message)
        {

            
        }

        public Task<bool> FileFinished()
        {
            Disconnect(false);
            return _tcs.Task;
        }
    }
}
