using System;
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
        private IAudioRequestController _currentRequest;
#if !CORE
        private MediaElement _mediaElement;
#endif

        public event EventHandler<IAudioRequestController> Complete;

        public async void PlayMediaUri(IAudioRequestController c, Uri uri)
        {
#if !CORE
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

#if !CORE
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
            _currentRequest = null;
        }
    }
}
