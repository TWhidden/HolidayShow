using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HolidayShow.Data;

namespace HolidayShowEditor.Services
{
    public class DbDataContext : IDbDataContext
    {
        public DbDataContext()
        {
            Context = new EfHolidayContext();
        }

        public EfHolidayContext Context { get; private set; }
    }
}
