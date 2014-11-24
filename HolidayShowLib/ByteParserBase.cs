using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolidayShowLib
{
    public abstract class ByteParserBase
    {
        /// <summary>
        ///  Used to syncronize the buffer list.
        /// </summary>
        private object _byteListLock = new object();
        /// <summary>
        /// Byte message buffer container.
        /// </summary>
        private List<byte> _messageBuffer = new List<byte>();

        /// <summary>
        /// Contains a list of possible parsing protocols - Handy if more than one protocols comes across the line.
        /// </summary>
        protected readonly List<ParserProtocolContainer> Parsers = new List<ParserProtocolContainer>();


        public void BytesReceived(byte[] byteBuffer)
        {
            // It may be considered that here we should not block, and return to the caller asap, and add data to the list async, but in order
            lock (_byteListLock)
            {
                _messageBuffer.AddRange(byteBuffer);
            }

            SearchBuffer();
        }



        private void SearchBuffer()
        {
            bool searchBufferAgain = false;
            lock (_byteListLock)
            {

                try
                {
                    // Try and find the lowest parser values

                    var lowestIndex = -1;
                    var byteStart = -1;
                    var byteEnd = -1;

                    for (var i = 0; i < Parsers.Count; i++)
                    {
                        // Select the parser on this itteration
                        var parser = Parsers[i];

                        // Find the first position of the byte sequence
                        var start = FindPosition(_messageBuffer, parser.StartingBytes);
                        if (start == -1)
                            continue;
                        // Since a start was found, find the next position, after the start with the ending sequence
                        var end = FindPosition(start, _messageBuffer, parser.EndingBytes);
                        if (end == -1)
                            continue;

                        // Since both a start and stop was found, check to see if anything has been set yet
                        if (byteStart == -1)
                        {
                            byteStart = start;
                            byteEnd = end;
                            lowestIndex = i;
                        }
                        else
                        {
                            // Since this has been set in a previous itteration, see if this starting point
                            // is less then the previously set starting point. If it is, update the index.
                            if (start < byteStart)
                            {
                                byteStart = start;
                                byteEnd = end;
                                lowestIndex = i;
                            }
                        }

                    }

                    if (lowestIndex != -1)
                    {
                        // The lowest index has a start / end buffer that 
                        var parser = Parsers[lowestIndex];

                        // The final byte index with the sequence value added in.
                        var realEnd = byteEnd + parser.EndingBytes.Length;

                        // Sets the length of the expected data - the entire packet
                        var messagelength = realEnd - byteStart;

                        // Selects the data that we are expecting - starting bytes to end of ending sequence bytes
                        var bytesRead = _messageBuffer.Select(x => x).Skip(byteStart).Take(messagelength).ToArray();

                        // Sends off for processing
                        ProcessPacket(bytesRead, parser);

                        // truncates the messageBuffer
                        _messageBuffer = _messageBuffer.Select(x => x).Skip(realEnd).Take(_messageBuffer.Count - realEnd).ToList();

                        // If there is more bytes to look for, search again (2 would mean a min of two bytes + a data byte at a min)
                        if (_messageBuffer.Count > 3)
                            searchBufferAgain = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error in Byte parser: {0}", ex.Message), ex);
                    _messageBuffer.Clear();
                }
            }

            // outside the lock to prevent a deadlock.
            if (searchBufferAgain)
            {
                //_loggerService.Debug("SearchAgain!");
                SearchBuffer(); // do this until the entire buffer is processed
            }
        }

        public abstract void ProcessPacket(byte[] bytes, ParserProtocolContainer parser);

        protected static int FindPosition(int startingPos, List<byte> stream, byte[] byteSequence)
        {
            if (byteSequence.Length > stream.Count)
                return -1;

            var buffer = new byte[byteSequence.Length];

            using (var bufStream = new BufferedStream(new MemoryStream(stream.Skip(startingPos).ToArray()), byteSequence.Length))
            {
                int i;
                while ((i = bufStream.Read(buffer, 0, byteSequence.Length)) == byteSequence.Length)
                {
                    if (byteSequence.SequenceEqual(buffer))
                        return (int)(bufStream.Position - byteSequence.Length) + startingPos;
                    else
                        bufStream.Position -= byteSequence.Length - PadLeftSequence(buffer, byteSequence);
                }
            }

            return -1;
        }

        protected static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            int i = 1;
            while (i < bytes.Length)
            {
                int n = bytes.Length - i;
                byte[] aux1 = new byte[n];
                byte[] aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                    return i;
                i++;
            }
            return i;
        }

        protected static int FindPosition(List<byte> stream, byte[] byteSequence)
        {
            return FindPosition(0, stream, byteSequence);
        }
    }
}
