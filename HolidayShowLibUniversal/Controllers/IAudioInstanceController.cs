using System;

namespace HolidayShowLibUniversal.Controllers
{
    public interface IAudioInstanceController
    {
        event EventHandler<IAudioRequestController> Complete;

        void PlayMediaUri(IAudioRequestController request, Uri uri);

#if !CORE
        void SetMediaElement(Windows.UI.Xaml.Controls.MediaElement mediaElement);
#endif

        void StopPlayback();
    }
}
