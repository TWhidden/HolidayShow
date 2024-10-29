using System;
using System.Collections.Generic;
using System.Text;

namespace HolidayShowLib
{
    public static class ProtocolHelper
    {
        public const byte SOH = 0x02; // Start of Header
        public const byte EOH = 0x03; // End of Header
        public const string Event = "EVENT";

        public static byte[] Wrap(ProtocolMessage message)
        {
            var sb = new StringBuilder();
            sb.Append($"{Event}:{(int)message.MessageEvent};");

            foreach (var k in message.MessageParts)
            {
                sb.Append($"{k.Key}:{k.Value};");
            }

            var b = Encoding.ASCII.GetBytes(sb.ToString());

            var buffer = new byte[b.Length + 2];
            buffer[0] = SOH;
            buffer[buffer.Length - 1] = EOH;
            Buffer.BlockCopy(b, 0, buffer, 1, b.Length);

            return buffer;
        }

        // Existing UnWrap method for byte[]
        public static ProtocolMessage UnWrap(byte[] rawMessage)
        {
            return UnWrap(rawMessage, rawMessage.Length);
        }

        // New UnWrap method accepting buffer and count
        public static ProtocolMessage UnWrap(byte[] buffer, int count)
        {
            if (count <= 2)
                return null;

            if (buffer[0] != SOH || buffer[count - 1] != EOH)
                return null;

            var subMessage = new byte[count - 2];
            Buffer.BlockCopy(buffer, 1, subMessage, 0, count - 2);

            var str = Encoding.ASCII.GetString(subMessage);
            var segments = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return null;

            MessageTypeIdEnum messageType = MessageTypeIdEnum.Unknown;
            var dic = new Dictionary<string, string>();

            foreach (var segment in segments)
            {
                var parts = segment.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                if (parts[0] == Event)
                {
                    if (string.IsNullOrWhiteSpace(parts[1]))
                        return null;

                    if (!Enum.TryParse<MessageTypeIdEnum>(parts[1].Trim(), out messageType))
                        return null;
                }
                else
                {
                    var key = parts[0];
                    var value = parts[1];

                    if (!dic.ContainsKey(key))
                        dic.Add(key, value);
                }
            }

            return new ProtocolMessage(messageType, dic);
        }
    }
}
