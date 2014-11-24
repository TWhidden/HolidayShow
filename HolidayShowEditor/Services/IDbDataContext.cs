using HolidayShow.Data;

namespace HolidayShowEditor.Services
{
    public interface IDbDataContext
    {
        HolidayShowDataContext Context { get; }
    }
}