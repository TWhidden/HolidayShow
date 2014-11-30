#ifdef _WIN32
#include "stdafx.h"
#endif
#include "ProtocolMessage.h"
#include <utility>

namespace HolidayShowLib
{
	ProtocolMessage::ProtocolMessage(MessageTypeIdEnum e)
	{
		_messageEvent = e;
	}

	ProtocolMessage::ProtocolMessage(MessageTypeIdEnum e, MessageParts& parts)
	{
		_messageEvent = e;
		_messageParts = std::move(parts);
	}

	ProtocolMessage::~ProtocolMessage()
	{

	}

	MessageParts& ProtocolMessage::MessagePartsGet()
	{
		return _messageParts;
	}

	MessageTypeIdEnum& ProtocolMessage::MessageEventGet()
	{
		return _messageEvent;
	}
};