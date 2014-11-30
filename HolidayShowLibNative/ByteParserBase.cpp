
#ifdef _WIN32
#include "stdafx.h"
#endif

#include <cstring>
#include <iostream>
#include <stdio.h>
#include "ByteParserBase.h"
#include <algorithm>

using namespace std;

namespace HolidayShowLib
{
	
	

	bool ByteParserBase::NextBytesMatch(int pos, ByteBuffer& searchBytes, ByteBuffer& byteSequence)
	{
		for (int i = 0; i < byteSequence.size(); i++)
		{
			if (searchBytes[pos + i] != byteSequence[i])
			{
				return false;
			}
		}

		return true;
	}

	int ByteParserBase::FindPosition(int startingPos, ByteBuffer& byteSequence)
	{

		if ((byteSequence.size() + startingPos) > _messageBuffer.size())
			return -1;

		for (int i = startingPos; i < _messageBuffer.size(); i++)
		{
			if (NextBytesMatch(i, _messageBuffer, byteSequence))
				return i;
		}

		return -1;

	}

	ByteParserBase::ByteParserBase()
	{
	}


	ByteParserBase::~ByteParserBase()
	{
	}

	void ByteParserBase::BytesReceived(ByteBuffer& byteBuffer)
	{
		if (_parser.size() != _parserResults.size())
		{
			//Clear for safety
			_parserResults.clear();



			// Initilize the parsers
			for (auto p = 0; p < _parser.size(); p++)
			{
				auto parser = _parser[p];
				_parserResults.emplace_back(std::make_shared<BytePositions>(parser));
			}
		}

		// It may be considered that here we should not block, and return to the caller asap, and add data to the list async, but in order
		{
			std::lock_guard<std::mutex> lock(_byteListLock);

			_messageBuffer.insert(std::end(_messageBuffer), std::begin(byteBuffer), std::end(byteBuffer));
		}

		// Initiate a buffer search
		SearchBuffer();
	}

	void ByteParserBase::SearchBuffer()
	{

		bool searchBufferAgain = false;
		
		{
			std::lock_guard<std::mutex> lock(_byteListLock);

			try
			{
				for (auto i = 0; i < _parserResults.size(); i++)
				{
					// Get the parser results, and reset the values between each pass
					auto p = _parserResults[i];
					auto parser = p->ParserGet();
					p->Reset();

					// There may be no initial byte[], and we are only splitting on a sequence of end bytes.  
					// This feature is new in DataEvents 5.14.0501.0 and greater.  
					if (!p->EndParserOnlyGet())
					{
						p->StartSet(FindPosition(0, parser->StartingBytesGet()));

						if (p->StartGet() == -1)
						{
							continue;
						}
					}

					// Offset the end find by the last start found, and the starting bytes length
					auto startingPositition = p->StartGet() + parser->StartingBytesGet().size();

					// Since a start was found, find the next position, after the start with the ending sequence
					p->EndSet(FindPosition(startingPositition, parser->EndingBytesGet()));
				}

                // Presort the list vector 
				std::sort(std::begin(_parserResults), std::end(_parserResults), [](const BytePositionPtr& a, const BytePositionPtr& b)
				{
					return a->StartGet() < b->StartGet();
				});

             
				auto lowestParser = std::find_if(std::begin(_parserResults), std::end(_parserResults), [](const BytePositionPtr& p)
				{
					return p->EndGet() != -1;
				});  

                if (lowestParser == _parserResults.end())
                {
					lowestParser = std::find_if(std::begin(_parserResults), std::end(_parserResults), [](const BytePositionPtr& p)
					{
						return p->StartGet() > -1;
					});
				}

				if (lowestParser == _parserResults.end())
				{
					lowestParser = std::find_if(std::begin(_parserResults), std::end(_parserResults), [](const BytePositionPtr& p)
					{
						return p->EndParserOnlyGet() && p->EndGet() > 0;
					});
				}


				if (lowestParser  == _parserResults.end() ||
					(!lowestParser[0]->EndParserOnlyGet() && lowestParser[0]->StartGet() != 0 && lowestParser[0]->EndGet() == -1)  // Look for any start that is found, without and end... and is not an EndOnly Parser
					)
				{
					auto newLength = 0;
					auto newStart = 0;

					if (lowestParser != std::end(_parserResults) && lowestParser[0]->StartGet() != -1)
					{
						newStart = lowestParser[0]->StartGet();
						newLength = (int)_messageBuffer.size() - newStart;
					}

					memmove(_messageBuffer.data(), _messageBuffer.data() + newStart, newLength);
					// Move the data left
					//Buffer.BlockCopy(_messageBuffer.GetBuffer(), newStart, _messageBuffer.GetBuffer(), 0, newLength);
					_messageBuffer.resize(newLength);

					return;
				}

				if (lowestParser[0]->StartGet() != -1 && lowestParser[0]->EndGet() != -1)
				{
					// The lowest index has a start / end buffer that 
					auto parser = lowestParser[0]->ParserGet();

					// The final byte index with the sequence value added in.
					auto realEnd = lowestParser[0]->EndGet() + parser->EndingBytesGet().size();

                    // starting byte location
                    auto realStart = lowestParser[0]->StartGet();

					// Sets the length of the expected data - the entire packet
					auto messagelength = realEnd - realStart;

					// Selects the data that we are expecting - starting bytes to end of ending sequence bytes
					//var bytesRead = _messageBuffer.Select(x => x).Skip(byteStart).Take(messagelength).ToArray();
					//auto bytesRead = new byte[messagelength];
					ByteBuffer bytesRead;

					bytesRead.insert(std::begin(bytesRead), std::begin(_messageBuffer) + realStart, std::begin(_messageBuffer) + realStart + messagelength);

					//Buffer.BlockCopy(_messageBuffer.GetBuffer(), lowestParser.Start, bytesRead, 0, messagelength);

					// Sends off for processing
					ProcessPacket(bytesRead, parser);

					// truncates the messageBuffer
					auto length = (int)_messageBuffer.size() - realEnd;
					if (length != 0)
					{
						memmove(_messageBuffer.data(), _messageBuffer.data() + realEnd, length);

						// Reset the stream to the new end
						//Buffer.BlockCopy(_messageBuffer.GetBuffer(), realEnd, _messageBuffer.GetBuffer(), 0, length);
					}

					_messageBuffer.resize(length);

					// If there is more bytes to look for, search again (2 would mean a min of two bytes + a data byte at a min)
					if (length > 3)
						searchBufferAgain = true;
				}
			}
			catch (const std::exception& ex)
			{
				
			}
			
		}


	}


};