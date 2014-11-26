#pragma once

namespace HolidayShowLib
{
	enum class MessageTypeIdEnum : int
	{
		Unknown = 0,
		DeviceId = 1,
		KeepAlive = 2,
		EventControl = 3,
		Reset = 4
	};
};
