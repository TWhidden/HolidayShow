using System.Collections.Generic;

namespace HolidayShowLib
{
    public class ProtocolMessage
    {
        public const string PINID = "PINID";
        public const string DEVID = "ID";
        public const string DURATION = "DURATION";
        public const string PINON = "PINON";
        public const string PINSAVAIL = "PINSAVAIL";
        public const string PINNAMES = "PINNAMES";
        public const string AUDIOFILE = "AUDIOFILE";
        public const string FILEDOWNLOAD = "FILEDOWNLOAD";
        public const string FILEBYTES = "FILEBYTES";


        public ProtocolMessage(MessageTypeIdEnum e, Dictionary<string, string> parts)
        {
            MessageEvent = e;
            MessageParts = parts;
        }

        public ProtocolMessage(MessageTypeIdEnum e)
        {
            MessageEvent = e;
            MessageParts = new Dictionary<string, string>();
        }

        public Dictionary<string, string> MessageParts { get; private set; }

        public MessageTypeIdEnum MessageEvent { get; private set; }
    }
}
