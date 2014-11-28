#pragma once
#include "ByteParserBase.h"
#include <TcpSocket.h>
#include <ISocketHandler.h>

using namespace std;

namespace HolidayShowEndpoint
{

	class Client : public HolidayShowLib::ByteParserBase, public TcpSocket
	{
	private:
		HolidayShowLib::ProtocolHelper _protocolHelper;

		string _address;
		int _port;

	public:
		Client(ISocketHandler&, string address, int port);

		virtual ~Client();

		virtual void ProcessPacket(HolidayShowLib::ByteBuffer& byteBuffer, std::shared_ptr<HolidayShowLib::ParserProtocolContainer>& parser) override;

		void BytesAdd(HolidayShowLib::ByteBuffer& newBytes);

		void OnRawData(const char *buf, size_t len) override;
		
		void OnConnect() override;

		void Start();
	};

};
