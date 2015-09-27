using System;
using System.Threading.Tasks;

namespace HolidayShowLibUniversal.Controllers
{
    public interface IAudioRequestController 
    {
        string FileName { get; set; }

        Task<Uri> FileReady();

        event EventHandler OnStop;

        void Stop();
    }
}
