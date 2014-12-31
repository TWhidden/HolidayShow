#include "stdafx.h"
#include <cstring>
#include "Client.h"
#include <iostream>
#include <TcpSocket.h>
#include <ISocketHandler.h>
#include "ProtocolHelper.h"
#include "GpioPins.h"
#include "uvxx.hpp"

#ifdef _MSC_VER
#pragma warning(disable:4786)
#endif

// the constant TCP_BUFSIZE_READ is the maximum size of the standard input
// buffer of TcpSocket
#define RSIZE TCP_BUFSIZE_READ
#include "LibGpio.h"
#include "DelayExecutionContainer.h"

using namespace std;
using namespace HolidayShowLib;

namespace HolidayShowEndpoint
{
	
	// Sends a HolidayShow ProtocolMessage object over the wire
	void Client::SendProtocolMessage(HolidayShowLib::ProtocolMessage& message)
	{
		// get the bytes associated to this message
		auto initBytes = _protocolHelper.Wrap(message);

		// change data over to support SendBuf Api call.
		const char* c = reinterpret_cast<char const*>(initBytes.data());

		// Get the size of the buffer
		auto size = initBytes.size();

		// Send it to the TcpClient base class for processing
		SendBuf(c, size, 0);
	}

	void Client::AllOff()
	{
		for (BroadcomPinNumber &p : _pins)
		{
			_libGpio->OutputValue(p, false);
		}
	}

	void Client::RemoveContainer(BroadcomPinNumber pin)
	{

		_delayedContainers.erase(pin);
	}

	Client::Client(ISocketHandler& socketHandler, string address, int port, int deviceId, PinMap &pins) : TcpSocket(socketHandler)
	{
		_libGpio = make_shared<LibGpio>();

		// Set Socket Options
		socketHandler.Add(this);
		SetDeleteByHandler();
		SetConnectTimeout(1);
		SetConnectionRetry(2000);
		SetReconnect(true);

		// set private vars
		_address = address;
		_port = port;
		_deviceId = deviceId;
		_pins = pins;

		// Create search buffer pattern and add to ByteParserBase as a registered parser
		ByteBufferPattern start = { static_cast<uint8_t>(ProtocolHelper::SOH) };
		ByteBufferPattern end = { static_cast<uint8_t>(ProtocolHelper::EOH) };
		auto parser1 = make_shared<ParserProtocolContainer>(start, end, 1);
		ParserAdd(parser1);



	/*	uvxx::net::stream_socket s;
		s.connect_async("10.64.128.75", 5556).then([](uvxx::pplx::task<void> t)
		{
			
		});*/
		
	}

	Client::~Client()
	{
		
	}

	void Client::ProcessPacket(ByteBuffer& byteBuffer, ParserContainer& parser)
	{
		string str(std::begin(byteBuffer), std::end(byteBuffer));

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

					auto adjustedFor0PinId = (pinId - 1);

					if (adjustedFor0PinId < 0) return;

					if (adjustedFor0PinId > _pins.size()) return;

					auto pin = _pins[adjustedFor0PinId];

					cout << "Pin " << static_cast<int>(pin) << "  On" << endl;

					_libGpio->OutputValue(pin, on == 1);

					if (durration > 0)
					{
						// Destory and remove if map item already exists
						RemoveContainer(pin);
				
						auto a = [=]()
						{
							cout << "Pin " << static_cast<int>(pin) << " Off" << endl;
							_libGpio->OutputValue(pin, false);
						};

						auto dec = make_shared<DelayExecutionContainer>(milliseconds(durration), a);

						_delayedContainers[pin] = dec;
						
					}
				}

				if (messageParts.count(AUDIOFILE))
				{
					auto audioFile = messageParts[AUDIOFILE];

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

    #undef max

    void convert(const std::chrono::milliseconds &ms, timeval &tv)
    {
        chrono::microseconds usec = chrono::duration_cast<chrono::microseconds>(ms);
        if (usec <= chrono::microseconds(0))
            tv.tv_sec = tv.tv_usec = 0;
        else
        {
            tv.tv_sec = usec.count() / 1000000;
            tv.tv_usec = usec.count() % 1000000;
        }
    }

	timeval  Client::ProcessTimers()
	{
        std::chrono::milliseconds next_timer = std::chrono::duration_cast<milliseconds>(std::chrono::microseconds::max());

		for (auto& it = _delayedContainers.cbegin(); it != _delayedContainers.cend();)
		{
			auto response = it->second->ExecuteIfReady();
			if (response)
			{
				_delayedContainers.erase(it++);
			}
            else
            {
                auto next_time = it->second->next_ready();

                if (next_time < next_timer)
                {
                    next_timer = next_time;
                }
            }
		}
        timeval tv;
        convert(next_timer, tv);

        return tv;
	}
};