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
                    // Make sure it doesnt exist first
                    if (!File.Exists(_fileDownloadContainer.DestinationPath))
                    {
                        // Ensure the entire path is available, because files might be in sub directories for
                        // the storage
                        var pathOnly = Path.GetDirectoryName(_fileDownloadContainer.DestinationPath);
                        if (!Directory.Exists(pathOnly))
                        {
                            Directory.CreateDirectory(pathOnly);
                        }

                        File.WriteAllBytes(_fileDownloadContainer.DestinationPath,
                            Convert.FromBase64String(message.MessageParts[ProtocolMessage.FILEBYTES]));
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
            var dic = new Dictionary<string, string>
                {
                    {ProtocolMessage.FILEDOWNLOAD, _fileDownloadContainer.FileName},
                };

            var message = new ProtocolMessage(MessageTypeIdEnum.RequestFile, dic);
            if (!BeginSend(message))
            {
                Disconnect(false);
                _tcs.SetResult(false);
            }
        }

        protected override void ErrorDetected(Exception ex)
        {
            Disconnect(false);
        }

        protected override void ResetReceived()
        {
            Disconnect(false);
        }

        protected override void EventControlReceived(ProtocolMessage message)
        {

            
        }

        public Task<bool> FileFinsihed()
        {
            Disconnect(false);
            return _tcs.Task;
        }
    }
}
