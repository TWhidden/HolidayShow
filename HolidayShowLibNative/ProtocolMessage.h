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

	public:

		std::string PINID = "PINID";
		std::string DEVID = "ID";
		std::string DURATION = "DURATION";
		std::string PINON = "PINON";
		std::string PINSAVAIL = "PINSAVAIL";
		std::string AUDIOFILE = "AUDIOFILE";

		ProtocolMessage(MessageTypeIdEnum e);
		ProtocolMessage(MessageTypeIdEnum e, MessageParts& parts);
		~ProtocolMessage();


		MessageParts& MessagePartsGet();

		MessageTypeIdEnum& MessageEventGet();
	};

};