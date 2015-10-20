using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using HolidayShowLibUniversal.Controllers;

namespace HolidayShowEndpointUniversalApp.Controllers
{
    public class AudioInstanceController : IAudioInstanceController
    {
        private IAudioRequestController _currentRequest;

        private MediaElement _mediaElement;

        public event EventHandler<IAudioRequestController> Complete;

        public void PlayMediaUri(IAudioRequestController c, Uri uri)
        {
            _currentRequest = c;
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                _mediaElement.Source = uri;
                                _mediaElement.Play();
                            });
            
        }

        public void SetMediaElement(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.MediaEnded += _mediaElement_MediaEnded;
            _mediaElement.MediaFailed += _mediaElement_MediaFailed;
            _mediaElement.MediaOpened += _mediaElement_MediaOpened;
        }

        public void StopPlayback()
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                        () =>
                                        {
                                            _mediaElement.Stop();
                                            InvokeOnComplete(_currentRequest);
                                        });
           
        }

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

        protected virtual void InvokeOnComplete(IAudioRequestController e)
        {
            Complete?.Invoke(this, e);
            _currentRequest = null;
        }
    }
}
