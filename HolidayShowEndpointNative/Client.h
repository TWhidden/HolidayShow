#pragma once
#include "ByteParserBase.h"
#include <TcpSocket.h>
#include <ISocketHandler.h>
#include "GpioPins.h"
#include "LibGpio.h"

using namespace std;
using namespace SocketsGee;

namespace HolidayShowEndpoint
{
	class DelayExecutionContainer;
	typedef vector<BroadcomPinNumber> PinMap;
	typedef map<BroadcomPinNumber, shared_ptr<DelayExecutionContainer>> DelayExecutionMap;




	class Client : public HolidayShowLib::ByteParserBase, public TcpSocket
	{
	private:
		HolidayShowLib::ProtocolHelper _protocolHelper;
		
		shared_ptr<LibGpio> _libGpio;
		
		PinMap _pins;

		string _address = "";
		int _port = 5555;
		int _deviceId = -1;

		void SendProtocolMessage(HolidayShowLib::ProtocolMessage& message);

		void AllOff();

		DelayExecutionMap _delayedContainers;

		void RemoveContainer(BroadcomPinNumber pin);

	public:
		Client(ISocketHandler&, string address, int port, int deviceId, PinMap& pins);

		virtual ~Client();

		virtual void ProcessPacket(HolidayShowLib::ByteBuffer& byteBuffer, std::shared_ptr<HolidayShowLib::ParserProtocolContainer>& parser) override;

		void OnRawData(const char *buf, size_t len) override;
		
		void OnConnect() override;

		void Start();

		void ProcessTimers();
	};

};
