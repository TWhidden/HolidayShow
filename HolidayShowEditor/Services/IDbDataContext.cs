using HolidayShow.Data;

namespace HolidayShowEditor.Services
{
    public interface IDbDataContext
    {
        EfHolidayContext Context { get; }
    }
}