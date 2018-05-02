using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HolidayShowLib
{
    public abstract class ByteParserBase 
    {
        /// <summary>
        ///  Used to syncronize the buffer list.
        /// </summary>
        private readonly object _byteListLock = new object();
        /// <summary>
        /// Buffer size to start the memory stream.  This is only to start, and the stream will add to it as it needs.
        /// </summary>
        private const int STARTING_BUFFER_SIZE = 2048;

        /// <summary>
        /// Stream to hold the data as its received from the medium.
        /// </summary>
        private readonly MemoryStream _messageBuffer = new MemoryStream(STARTING_BUFFER_SIZE);

        /// <summary>
        /// Contains a list of possible parsing protocols - Handy if more than one protocols comes across the line.
        /// </summary>
        protected readonly List<ParserProtocolContainer> Parsers = new List<ParserProtocolContainer>();

        private readonly List<BytePositions> ParserResults = new List<BytePositions>();

        private bool _isDisposed;


        public void BytesReceived(byte[] byteBuffer)
        {
            if (ParserResults.Count != Parsers.Count)
            {
                //Clear for safety
                ParserResults.Clear();

                // Initilize the parsers
                foreach (var parser in Parsers)
                {
                    ParserResults.Add(new BytePositions(parser));
                }
            }

            // It may be considered that here we should not block, and return to the caller asap, and add data to the list async, but in order
            lock (_byteListLock)
            {
                // Write the data to the memory stream
                _messageBuffer.Write(byteBuffer, 0, byteBuffer.Length);
#if DEBUG
                buffersReceived++;
#endif
            }

            // Initiate a buffer search
            SearchBuffer();
        }

        [DebuggerDisplay("Start={Start};End={End};EndOnly={EndParserOnly}")]
        private class BytePositions
        {
            public BytePositions(ParserProtocolContainer parser)
            {
                this.Parser = parser;

                EndParserOnly = parser.StartingBytes.Length == 0 && parser.EndingBytes.Length > 0;
                Start = EndParserOnly ? 0 : -1;  // If its an end-only, we want the starting position to be the first element of the stream all the time.
                End = -1;
            }

            public ParserProtocolContainer Parser { get; private set; }
            public int Start { get; set; }
            public int End { get; set; }

            public bool EndParserOnly { get; private set; }

            public void Reset()
            {
                Start = EndParserOnly ? 0 : -1;  // If its an end-only, we want the starting position to be the first element of the stream all the time.
                End = -1;
            }
        }

#if DEBUG

        private long buffersReceived;
        private long bufferSearches;
        private long buffersReturned;

#endif

        private void SearchBuffer()
        {
  
            int searchAgain = 0;
            var stopWatch = Stopwatch.StartNew();

        top:

#if DEBUG
            bufferSearches++;
#endif

            bool searchBufferAgain = false;
            lock (_byteListLock)
            {
                try
                {

                    for (var i = 0; i < ParserResults.Count; i++)
                    {
                        // Get the parser results, and reset the values between each pass
                        var p = ParserResults[i];
                        var parser = p.Parser;
                        p.Reset();

                        // There may be no initial byte[], and we are only splitting on a sequence of end bytes.  
                        // This feature is new in DataEvents 5.14.0501.0 and greater.  
                        if (!p.EndParserOnly)
                        {
                            p.Start = FindPosition(parser.StartingBytes);

                            if (p.Start == -1)
                            {
                                continue;
                            }
                        }

                        // Offset the end find by the last start found, and the starting bytes length
                        var startingPositition = p.Start + parser.StartingBytes.Length;

                        // Since a start was found, find the next position, after the start with the ending sequence
                        p.End = FindPosition(startingPositition, parser.EndingBytes);
                    }

                    // First look for a full packet, at the lowest index start index found
                    var lowestParser = ParserResults.Where(x => x.End != -1).OrderBy(x => x.Start).FirstOrDefault();

                    // If no full packet is found, find the lowest parser where the start of the packet was found
                    if (lowestParser == null)
                    {
                        lowestParser = ParserResults.Where(x => x.Start > -1).OrderBy(x => x.Start).FirstOrDefault();
                    }

                    // If the lowest parser is still null, lets see if there is a EndOnly parser
                    if (lowestParser == null)
                    {
                        lowestParser = ParserResults.Where(x => x.EndParserOnly && x.End > 0).OrderBy(x => x.Start).FirstOrDefault();
                    }

                    if (
                        lowestParser == null ||
                        (!lowestParser.EndParserOnly && lowestParser.Start != 0 && lowestParser.End == -1)  // Look for any start that is found, without and end... and is not an EndOnly Parser
                        )
                    {
                        var newLength = 0;
                        var newStart = 0;

                        if (lowestParser != null && lowestParser.Start != -1)
                        {
                            newStart = lowestParser.Start;
                            newLength = (int)_messageBuffer.Length - newStart;
                        }

                        // Move the data left
#if WINDOWS_UWP
                        ArraySegment<byte> data;
                        if (_messageBuffer.TryGetBuffer(out data))
                        {
                            Buffer.BlockCopy(data.Array, newStart, data.Array, 0, newLength);
                        }
                        
#else
                        Buffer.BlockCopy(_messageBuffer.GetBuffer(), newStart, _messageBuffer.GetBuffer(), 0, newLength);
#endif

                        _messageBuffer.SetLength(newLength);
                        _messageBuffer.Position = newLength;

                        // nothing left to do, exit function
#if DEBUG && !WINDOWS_UWP
                        // goto end for debug parse output.
                        goto end;
#else
                        return;
#endif
                    }

                    if (lowestParser.Start != -1 && lowestParser.End != -1)
                    {
                        // The lowest index has a start / end buffer that 
                        var parser = lowestParser.Parser;

                        // The final byte index with the sequence value added in.
                        var realEnd = lowestParser.End + parser.EndingBytes.Length;

                        // Sets the length of the expected data - the entire packet
                        var messagelength = realEnd - lowestParser.Start;

                        // Selects the data that we are expecting - starting bytes to end of ending sequence bytes
                        //var bytesRead = _messageBuffer.Select(x => x).Skip(byteStart).Take(messagelength).ToArray();
                        var bytesRead = new byte[messagelength];
#if WINDOWS_UWP

                        ArraySegment<byte> bytes;
                        if (_messageBuffer.TryGetBuffer(out bytes))
                        {
                            Buffer.BlockCopy(bytes.Array, lowestParser.Start, bytesRead, 0, messagelength);
                        }

#else

                        Buffer.BlockCopy(_messageBuffer.GetBuffer(), lowestParser.Start, bytesRead, 0, messagelength);
#endif

                        if (bytesRead != null)
                        {
                            // Sends off for processing
                            ProcessPacket(bytesRead, parser);
#if DEBUG
                            buffersReturned++;
#endif
                        }

                        // truncates the messageBuffer
                        var length = (int)_messageBuffer.Length - realEnd;
                        if (length != 0)
                        {
#if WINDOWS_UWP
                            ArraySegment<byte> data;
                            if (_messageBuffer.TryGetBuffer(out data))
                            {
                                Buffer.BlockCopy(data.Array, realEnd, data.Array, 0, length);
                            }
                            // Reset the stream to the new end
                            
#else
                            Buffer.BlockCopy(_messageBuffer.GetBuffer(), realEnd, _messageBuffer.GetBuffer(), 0, length);
#endif
                        }

                        _messageBuffer.SetLength(length);
                        _messageBuffer.Position = length;

                        // If there is more bytes to look for, search again (2 would mean a min of two bytes + a data byte at a min)
                        if (length > 3)
                            searchBufferAgain = true;
                    }
                }
                catch
                {
                    _messageBuffer.Position = 0;
                }
            }

            // outside the lock to prevent a deadlock.
            if (searchBufferAgain)
            {
                searchAgain++;
                //_loggerService.Debug("SearchAgain!");
                goto top; // do this until the entire buffer is processed
            }


#if DEBUG && !WINDOWS_UWP
            end:
            Console.WriteLine("{0}; ReSearch {1}; Received: {2}; Searches: {3}; Returned: {4}", stopWatch.Elapsed, searchAgain, buffersReceived, bufferSearches, buffersReturned);
#endif
        }

        public abstract void ProcessPacket(byte[] bytes, ParserProtocolContainer parser);

        private static bool NextBytesMatch(int pos, byte[] searchBytes, Byte[] byteSequence)
        {
            for (int i = 0; i < byteSequence.Length; i++)
            {
                if (searchBytes[pos + i] != byteSequence[i])
                {
                    return false;
                }
            }

            return true;
        }


        private int FindPosition(int startingPos, byte[] byteSequence)
        {
            if ((byteSequence.Length + startingPos) > _messageBuffer.Length)
                return -1;

#if WINDOWS_UWP
            ArraySegment<byte> data;
            if (!_messageBuffer.TryGetBuffer(out data))
            {
                return -1;
            }
            var memoryBuffer = data.Array;
#else
            var memoryBuffer = _messageBuffer.GetBuffer();
#endif

            var membufferlen = _messageBuffer.Length;

            for (int i = startingPos; i < membufferlen; i++)
            {
                if (NextBytesMatch(i, memoryBuffer, byteSequence))
                    return i;
            }

            return -1;
        }


        private int FindPosition(byte[] byteSequence)
        {
            return FindPosition(0, byteSequence);
        }

    }
}
