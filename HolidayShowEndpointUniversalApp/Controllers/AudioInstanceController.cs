using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
#if !CORE
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
#endif
using HolidayShowLibUniversal.Controllers;

namespace HolidayShowEndpointUniversalApp.Controllers
{
    public class AudioInstanceController : IAudioInstanceController
    {

        private static int _playbackCounter = 0;
        private int playerCount = -1;
        
#if CORE
        private Process _externalPlayerProcess;
#else
        private IAudioRequestController _currentRequest;
        private MediaElement _mediaElement;
#endif

        public event EventHandler Complete;
        
        public async void PlayMediaUri(IAudioRequestController c, Uri uri)
        {
            playerCount = Interlocked.Increment(ref _playbackCounter);

#if CORE
            Console.WriteLine($"[{playerCount}] Audio File Play: {uri.AbsolutePath}");

            _externalPlayerProcess = new Process()
            {
                // play would not support m4a files
                //StartInfo = new ProcessStartInfo("play", uri.AbsolutePath),
                StartInfo = new ProcessStartInfo("mplayer", $"-novideo -ao alsa -really-quiet -softvol -softvol-max 300 -af volume=10 {uri.AbsolutePath}"),
            };

            _externalPlayerProcess.Exited += _externalPlayerProcess_Exited;
            try
            {
                var result = _externalPlayerProcess.Start();

                Console.WriteLine($"[{playerCount}] Start Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed starting process to play audio. Error: {ex.Message}");
            }

#else
            _currentRequest = c;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    _mediaElement.Source = uri;
                    _mediaElement.Play();
                });
#endif

        }

        private void _externalPlayerProcess_Exited(object sender, EventArgs e)
        {
            Console.WriteLine($"[{playerCount}] Audio Process Exited!");
            if (sender is Process p)
            {
                p.Exited -= _externalPlayerProcess_Exited;
                p.Dispose();
            }

            InvokeOnComplete();

#if CORE
            _externalPlayerProcess = null;
#endif
        }
#if !CORE
        public void SetMediaElement(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.MediaEnded += _mediaElement_MediaEnded;
            _mediaElement.MediaFailed += _mediaElement_MediaFailed;
            _mediaElement.MediaOpened += _mediaElement_MediaOpened;
        }
#endif

        public async void StopPlayback()
        {
            Console.WriteLine($"[{playerCount}] StopPlayback();");
#if CORE
            // x3 idea from https://stackoverflow.com/a/283357/1004187
            if (_externalPlayerProcess != null)
            {
                try
                {
                    if (!_externalPlayerProcess.HasExited)
                    {
                        Console.WriteLine($"[{playerCount}] Process not exited. Attempting close input;");
                        try
                        {
                            _externalPlayerProcess?.StandardInput.WriteLine("\x3");
                        }
                        catch
                        {
                        }

                        Console.WriteLine($"[{playerCount}] Attempting Close()");
                        try
                        {
                            _externalPlayerProcess?.StandardInput.Close();
                        }
                        catch
                        {
                        }

                        Console.WriteLine($"[{playerCount}] Attempting Kill()");
                        try
                        {
                            _externalPlayerProcess?.Kill();
                        }
                        catch
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with checking HasExited: {ex.Message}");
                }

                _externalPlayerProcess?.Close();
                _externalPlayerProcess?.Dispose();
                InvokeOnComplete();
            }
#else
            Console.WriteLine($"StopPlayback() called for {_currentRequest.FileName}");
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    _mediaElement.Stop();
                    InvokeOnComplete();
                });
#endif

        }

#if !CORE
        private void _mediaElement_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            
        }

        private void _mediaElement_MediaFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
        {
            InvokeOnComplete();
        }

        private void _mediaElement_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            InvokeOnComplete();
        }
#endif

        protected virtual void InvokeOnComplete()
        {
            Console.WriteLine($"[{playerCount}] InvokeOnComplete() called.");
            Complete?.Invoke(this, new EventArgs());

#if !CORE
            _currentRequest = null;
#endif
        }
    }
}
