#pragma once
#include "ByteParserBase.h"
#include <TcpSocket.h>
#include <ISocketHandler.h>

namespace HolidayShowEndpoint
{

	class Client : public HolidayShowLib::ByteParserBase, public TcpSocket
	{

	public:
		Client(ISocketHandler&);

		virtual ~Client();

		virtual void ProcessPacket(HolidayShowLib::ByteBuffer& byteBuffer, std::shared_ptr<HolidayShowLib::ParserProtocolContainer>& parser) override;

		void BytesAdd(HolidayShowLib::ByteBuffer& newBytes);

		void OnRawData(const char *buf, size_t len) override;
	};

};
