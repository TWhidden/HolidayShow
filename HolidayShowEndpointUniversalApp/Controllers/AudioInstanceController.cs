using System;
using System.Diagnostics;
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
        
#if CORE
        private Process _externalPlayerProcess;
#else
        private IAudioRequestController _currentRequest;
        private MediaElement _mediaElement;
#endif

        public event EventHandler<IAudioRequestController> Complete;
        
        public async void PlayMediaUri(IAudioRequestController c, Uri uri)
        {
#if CORE
            Console.WriteLine($"Audio File Play: {uri.AbsolutePath}");

            _externalPlayerProcess = new Process()
            {
                StartInfo = new ProcessStartInfo("play", uri.AbsolutePath),
                

            };
            var result = _externalPlayerProcess.Start();
            
            Console.WriteLine($"Start Result: {result}");

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
#if CORE
            // x3 idea from https://stackoverflow.com/a/283357/1004187
            _externalPlayerProcess?.StandardInput.WriteLine("\x3");
            _externalPlayerProcess?.StandardInput.Close();
            _externalPlayerProcess?.Kill();
            _externalPlayerProcess?.Close();
            _externalPlayerProcess?.Dispose();

#else
            Console.WriteLine($"StopPlayback() called for {_currentRequest.FileName}");
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    _mediaElement.Stop();
                    InvokeOnComplete(_currentRequest);
                });
#endif

        }

#if !CORE
        private void _mediaElement_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            
        }

        private void _mediaElement_MediaFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
        {
            InvokeOnComplete(_currentRequest);
        }

        private void _mediaElement_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            InvokeOnComplete(_currentRequest);
        }
#endif

        protected virtual void InvokeOnComplete(IAudioRequestController e)
        {
            Complete?.Invoke(this, e);

#if !CORE
            _currentRequest = null;
#endif
        }
    }
}
