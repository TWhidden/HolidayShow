using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HolidayShowLibUniversal.Controllers
{
    public interface IAudioInstanceController
    {
        event EventHandler<IAudioRequestController> Complete;

        void PlayMediaUri(IAudioRequestController request, Uri uri);

        void SetMediaElement(MediaElement mediaElement);

        void StopPlayback();
    }
}
