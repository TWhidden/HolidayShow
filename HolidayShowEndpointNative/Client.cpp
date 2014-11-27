#include "stdafx.h"
#include "Client.h"
#include <iostream>
#include <TcpSocket.h>
#include <ISocketHandler.h>

#ifdef _MSC_VER
#pragma warning(disable:4786)
#endif

// the constant TCP_BUFSIZE_READ is the maximum size of the standard input
// buffer of TcpSocket
#define RSIZE TCP_BUFSIZE_READ

using namespace std;

namespace HolidayShowEndpoint
{
	Client::Client(ISocketHandler& h) : TcpSocket(h)
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


	void Client::OnRawData(const char* buf, size_t len)
	{
		// Byte Buffer to pass in
		HolidayShowLib::ByteBuffer bb;
		bb.resize(len);
		memcpy(bb.data(), buf, len);

		// 
		HolidayShowLib::ByteParserBase::BytesReceived(bb);
	}
};