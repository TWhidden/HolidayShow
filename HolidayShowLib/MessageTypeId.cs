using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolidayShowLib
{
    public enum MessageTypeIdEnum : int
    {
        Unknown = 0,
        DeviceId = 1,
        KeepAlive = 2,
        EventControl = 3,
        Reset = 4
    }
}
