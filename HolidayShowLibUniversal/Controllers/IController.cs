using System.Threading.Tasks;

namespace HolidayShowLibUniversal.Controllers
{
    public interface IController
    {
        Task Run();

        Task Stop();
    }
}
