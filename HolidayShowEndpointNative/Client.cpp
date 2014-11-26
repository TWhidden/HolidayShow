#include "stdafx.h"
#include "Client.h"
#include <iostream>
using namespace std;

namespace HolidayShowEndpoint
{
	Client::Client()
	{
		HolidayShowLib::ByteBufferPattern start = { static_cast<uint8_t>(HolidayShowLib::ProtocolHelper::SOH) };
		HolidayShowLib::ByteBufferPattern end = { static_cast<uint8_t>(HolidayShowLib::ProtocolHelper::EOH) };
		auto parser1 = std::make_shared<HolidayShowLib::ParserProtocolContainer>(start, end, 1);
		ParserAdd(parser1);
	}

	Client::~Client()
	{
	}

	void Client::ProcessPacket(HolidayShowLib::ByteBuffer& byteBuffer, std::shared_ptr<HolidayShowLib::ParserProtocolContainer>& parser)
	{
        std::string str(std::begin(byteBuffer), std::end(byteBuffer));

        cout << "\"" << str << "\"";


	}

	void Client::BytesAdd(HolidayShowLib::ByteBuffer& newBytes)
	{
		HolidayShowLib::ByteParserBase::BytesReceived(newBytes);
	}
};