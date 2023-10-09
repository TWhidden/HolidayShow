
namespace HolidayShowLib
{
    public class ParserProtocolContainer
    {
        public ParserProtocolContainer(byte[] startingBytes, byte[] endingBytes, int protocolNumber)
        {
            StartingBytes = startingBytes;
            EndingBytes = endingBytes;
            ProtocolNumber = protocolNumber;
        }

        public byte[] StartingBytes { get; private set; }

        public byte[] EndingBytes { get; private set; }

        public int ProtocolNumber { get; private set; }
    }
}
