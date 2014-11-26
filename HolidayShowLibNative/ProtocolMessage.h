#pragma once
#include "MessageTypeId.h"
#include <string>
#include <map>


namespace HolidayShowLib
{
	typedef std::map<std::string, std::string> MessageParts;

	class ProtocolMessage
	{

		MessageTypeIdEnum _messageEvent;
		MessageParts _messageParts;

		const char* PINID = "PINID";
		const char* DEVID = "ID";
		const char* DURATION = "DURATION";
		const char* PINON = "PINON";
		const char* PINSAVAIL = "PINSAVAIL";
		const char* AUDIOFILE = "AUDIOFILE";


	public:
		ProtocolMessage(MessageTypeIdEnum e);
		ProtocolMessage(MessageTypeIdEnum e, MessageParts& parts);
		~ProtocolMessage();


		MessageParts& MessagePartsGet();

		MessageTypeIdEnum& MessageEventGet();
	};

};