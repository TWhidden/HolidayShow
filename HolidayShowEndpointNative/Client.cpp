#include "stdafx.h"
#include "Client.h"
#include <iostream>
#include <TcpSocket.h>
#include <ISocketHandler.h>
#include "ProtocolHelper.h"
#include "GpioPins.h"

#ifdef _MSC_VER
#pragma warning(disable:4786)
#endif

// the constant TCP_BUFSIZE_READ is the maximum size of the standard input
// buffer of TcpSocket
#define RSIZE TCP_BUFSIZE_READ
#include "LibGpio.h"

using namespace std;
using namespace HolidayShowLib;

namespace HolidayShowEndpoint
{
	void Client::SendProtocolMessage(HolidayShowLib::ProtocolMessage& message)
	{
		auto initBytes = _protocolHelper.Wrap(message);

		const char* c = reinterpret_cast<char const*>(initBytes.data());

		auto size = initBytes.size();

		SendBuf(c, size, 0);
	}

	void Client::AllOff()
	{
		for (BroadcomPinNumber &p : _pins)
		{
			_libGpio.OutputValue(p, false);
		}
	}

	Client::Client(ISocketHandler& socketHandler, string address, int port, int deviceId, PinMap &pins) : TcpSocket(socketHandler)
	{
		SetDeleteByHandler();

		_address = address;
		_port = port;
		_deviceId = deviceId;
		_pins = pins;

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

	void Client::ProcessPacket(HolidayShowLib::ByteBuffer& byteBuffer, ParserContainer& parser)
	{
		std::string str(std::begin(byteBuffer), std::end(byteBuffer));

		try{

			//cout << "\"" << str << "\"";
			auto message = _protocolHelper.UnWrap(byteBuffer);
			if (message == nullptr) return;

			auto messageEvent = message->MessageEventGet();
			auto messageParts = message->MessagePartsGet();

			if (messageEvent == MessageTypeIdEnum::Unknown) return;

			if (messageEvent == MessageTypeIdEnum::KeepAlive)
			{
				ProtocolMessage p(MessageTypeIdEnum::KeepAlive);
				SendProtocolMessage(p);
			}
			else if (messageEvent == MessageTypeIdEnum::Reset)
			{
				AllOff();
			}
			else if (messageEvent == MessageTypeIdEnum::EventControl)
			{
				if (messageParts.count(PINID) &&
					messageParts.count(DURATION) &&
					messageParts.count(PINON))
				{
					int pinId = std::stoi(messageParts[PINID]);

					int durration = std::stoi(messageParts[DURATION]);

					int on = std::stoi(messageParts[PINON]);

					auto adjustedFor0PinId = pinId - 1;

					if (adjustedFor0PinId < 0) return;

					if (adjustedFor0PinId > _pins.size()) return;

					std::cout << "Pin " << adjustedFor0PinId << " start" << endl;

					//_libGpio->OutputValue(_pins[adjustedFor0PinId], on == 1);
				}

				if (messageParts.count(AUDIOFILE))
				{

				}
			}
		}
		catch (exception &e)
		{

		}
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
		parts[DEVID] = std::to_string(_deviceId);
		parts[PINSAVAIL] = std::to_string(_pins.size());
		ProtocolMessage p(MessageTypeIdEnum::DeviceId, parts);

		SendProtocolMessage(p);
	}



	void Client::Start()
	{
		auto open = Open(_address, _port);


	}
};