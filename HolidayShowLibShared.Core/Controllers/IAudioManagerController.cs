using System.Threading.Tasks;

namespace HolidayShowLibUniversal.Controllers
{
    public interface IAudioManagerController
    {
        Task<IAudioRequestController> RequestAndPlay(string fileName);

        void StopAllAudio();
    }
}
