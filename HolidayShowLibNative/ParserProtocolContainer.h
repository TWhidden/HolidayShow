#pragma once
#include <vector>
#include <stdint.h>
#include <utility>

namespace HolidayShowLib
{
	typedef std::vector<uint8_t> ByteBufferPattern;


	class ParserProtocolContainer
	{
	
		ByteBufferPattern _startingBytes;
		ByteBufferPattern _endingBytes;

		int _protocolNumber;

		public:
			ParserProtocolContainer(ByteBufferPattern& startingBytes, ByteBufferPattern& endingBytes, int protocolNumber)
			{
				_startingBytes = std::move(startingBytes);
				_endingBytes = std::move(endingBytes);
				_protocolNumber = protocolNumber;
			};

			ByteBufferPattern& StartingBytesGet()
			{
				return _startingBytes;
			}

			ByteBufferPattern& EndingBytesGet()
			{
				return _endingBytes;
			}

			int ProtocolNumberGet()
			{
				return _protocolNumber;
			}
	}; 
};