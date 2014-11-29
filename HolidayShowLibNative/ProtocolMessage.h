#pragma once
#include "MessageTypeId.h"
#include <string>
#include <map>


namespace HolidayShowLib
{
	typedef std::map<std::string, std::string> MessageParts;

	static const std::string PINID = "PINID";
	static const std::string DEVID = "ID";
	static const std::string DURATION = "DURATION";
	static const std::string PINON = "PINON";
	static const std::string PINSAVAIL = "PINSAVAIL";
	static const std::string AUDIOFILE = "AUDIOFILE";

	class ProtocolMessage
	{

		MessageTypeIdEnum _messageEvent;
		MessageParts _messageParts;

	public:

		ProtocolMessage(MessageTypeIdEnum e);
		ProtocolMessage(MessageTypeIdEnum e, MessageParts& parts);
		~ProtocolMessage();


		MessageParts& MessagePartsGet();

		MessageTypeIdEnum& MessageEventGet();
	};

};