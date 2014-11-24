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
            Context = new HolidayShowDataContext();
        }

        public HolidayShowDataContext Context { get; private set; }
    }
}
