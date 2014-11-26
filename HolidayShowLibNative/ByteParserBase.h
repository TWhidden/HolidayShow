#pragma once
#include "ProtocolHelper.h"
#include "ParserProtocolContainer.h"
#include <mutex>
#include <utility>

namespace HolidayShowLib
{
	class ParserProtocolContainer;

	class ByteParserBase
	{
		/// Used to syncronize the buffer list.
		std::mutex _byteListLock;

		/// Vector Initial size
		const int STARTING_BUFFER_SIZE = 2048;

		/// Stream to hold the data as its received from the medium.
		std::vector<uint8_t> _messageBuffer;

		/// Contains a list of possible parsing protocols - Handy if more than one protocols comes across the line.
		std::vector<std::shared_ptr<ParserProtocolContainer>> _parser;

		class BytePositions
		{
			std::shared_ptr<ParserProtocolContainer> _parser;

			int _start = -1;
			int _end = 1;
			bool _endParserOnly = false;
			
		public:

			BytePositions(std::shared_ptr<ParserProtocolContainer>& parser) 
			{
				_parser = parser;
				_endParserOnly = parser->StartingBytesGet().size() == 0 && parser->EndingBytesGet().size() > 0;
				_start = _endParserOnly ? 0 : -1;
				_end = -1;
			}
			~BytePositions() = default;

			std::shared_ptr<ParserProtocolContainer> ParserGet()
			{
				return _parser;
			}

			int StartGet()
			{
				return _start;
			}

			void StartSet(int start)
			{
				_start = start;
			}

			int EndGet()
			{
				return _end;
			}

			void EndSet(int end)
			{
				_end = end;
			}

			bool EndParserOnlyGet()
			{
				return _endParserOnly;
			}

			void Reset()
			{
				_start = _endParserOnly ? 0 : -1;
				_end = -1;
			}
		};

		typedef std::shared_ptr<BytePositions> BytePositionPtr;

		std::vector<std::shared_ptr<BytePositions>> _parserResults;

		void SearchBuffer();

		bool NextBytesMatch(int pos, ByteBuffer& searchBytes, ByteBuffer& byteSequence);

		int FindPosition(int startingPos, ByteBuffer& byteSequence);

	public:
		ByteParserBase();
		virtual ~ByteParserBase();

		void ParserAdd(std::shared_ptr<ParserProtocolContainer>& parser)
		{
			_parser.push_back(std::move(parser));
		}

		void BytesReceived(ByteBuffer& byteBuffer);

		virtual void ProcessPacket(ByteBuffer& byteBuffer, std::shared_ptr<ParserProtocolContainer>& parser) = 0;

	};



};

