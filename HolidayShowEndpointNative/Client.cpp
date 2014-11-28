#include "stdafx.h"
#include "Client.h"
#include <iostream>
#include <TcpSocket.h>
#include <ISocketHandler.h>
#include "ProtocolHelper.h"

#ifdef _MSC_VER
#pragma warning(disable:4786)
#endif

// the constant TCP_BUFSIZE_READ is the maximum size of the standard input
// buffer of TcpSocket
#define RSIZE TCP_BUFSIZE_READ

using namespace std;
using namespace HolidayShowLib;

namespace HolidayShowEndpoint
{
	Client::Client(ISocketHandler& socketHandler, string address, int port) : TcpSocket(socketHandler)
	{
		SetDeleteByHandler();

		_address = address;
		_port = port;

		ByteBufferPattern start = { static_cast<uint8_t>(ProtocolHelper::SOH) };
		ByteBufferPattern end = { static_cast<uint8_t>(ProtocolHelper::EOH) };
		auto parser1 = std::make_shared<ParserProtocolContainer>(start, end, 1);
		ParserAdd(parser1);
		socketHandler.Add(this);
		SetReconnect(true);
		
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

	void Client::OnConnect()
	{
		Socket::OnConnect();

		MessageParts parts;
		parts["ID"] = "1";
		parts["PINSAVAIL"] = "10";
		ProtocolMessage p(MessageTypeIdEnum::DeviceId, parts);

		auto initBytes = _protocolHelper.Wrap(p);

		const char* c = reinterpret_cast<char const*>(initBytes.data());

		auto size = initBytes.size();

		SendBuf(c, size, 0);
	}

	void Client::Start()
	{
		auto open = Open(_address, _port);


	}
};