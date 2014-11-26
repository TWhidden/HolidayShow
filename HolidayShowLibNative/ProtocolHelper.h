#pragma once
#include <stdint.h>
#include "ProtocolMessage.h"
#include <vector>
#include <string>
#include <sstream>
#include <memory>
#include <algorithm>
#include <iterator>


namespace HolidayShowLib
{
	typedef std::vector<uint8_t> ByteBuffer;

	class ProtocolHelper
	{

	public:

		static const char SOH = 0x02;
		static const char EOH = 0x03;

		ProtocolHelper() = default;
		~ProtocolHelper() = default;

		ByteBuffer Wrap(ProtocolMessage message)
		{
			std::stringstream ss;
			ss << SOH << "EVENT:" << static_cast<int>(message.MessageEventGet()) << ";";

			for (auto& kv : message.MessagePartsGet())
			{
				ss << kv.first << ":" << kv.second << ";";
			}

			ss << EOH;

			auto length = ss.tellp();

			ByteBuffer bb(length);

			ss.read(reinterpret_cast<char*>(bb.data()), length);

			return bb;
		}

		std::unique_ptr<ProtocolMessage> UnWrap(ByteBuffer rawMessage)
		{
			if (rawMessage.size() <= 2) return nullptr;

			std::string str(std::begin(rawMessage)+1, std::end(rawMessage)-1);

			std::string delimiter = ";";
			std::string dataDelimiter = ":";

			auto messageParts = StringSplit(str, delimiter);

			MessageParts map;
			MessageTypeIdEnum eventType = MessageTypeIdEnum::Unknown;

			for (auto& part : messageParts)
			{
				auto parts = StringSplit(part, dataDelimiter);

				if (parts.size() != 2) continue;

				auto k = parts[0];
				auto v = parts[1];

				if (k == "EVENT")
				{
					int i = stoi(v);
					eventType = static_cast<MessageTypeIdEnum>(i);
				} else
				{
					map[k] = v;
				}
			}

			std::unique_ptr<ProtocolMessage> m(new ProtocolMessage(eventType, map));

			return m;
		}
	
	private:

		std::vector<std::string> StringSplit(std::string& str, std::string& delimiter)
		{
			std::vector<std::string> splits;

			auto start = 0U;
			auto end = str.find(delimiter);
			while (end != std::string::npos)
			{
				splits.emplace_back(str.substr(start, end - start));
				start = end + delimiter.length();
				end = str.find(delimiter, start);
			}
			if (start < str.size())
			{
				// add remaining data
				splits.emplace_back(str.substr(start, str.size() - start));
			}

			return splits;
		}

	};

	
}