#pragma once
#include "ByteParserBase.h"

namespace HolidayShowEndpoint
{

	class Client : public HolidayShowLib::ByteParserBase
	{

	public:
		Client();

		virtual ~Client();

		virtual void ProcessPacket(HolidayShowLib::ByteBuffer& byteBuffer, std::shared_ptr<HolidayShowLib::ParserProtocolContainer>& parser);

		void BytesAdd(HolidayShowLib::ByteBuffer& newBytes);
	};

};
