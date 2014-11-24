using System;
using System.Collections.Generic;
using System.Text;

namespace HolidayShowLib
{
    public class ProtocolHelper
    {
        public const byte SOH = 0x02;

        public const byte EOH = 0x03;

        public static byte[] Wrap(ProtocolMessage message)
        {
            var sb = new StringBuilder();
            sb.Append("EVENT");
            sb.Append(":");
            sb.Append((int)message.MessageEvent);
            sb.Append(";");

            foreach (var k in message.MessageParts)
            {
                sb.Append(k.Key);
                sb.Append(":");
                sb.Append(k.Value);
                sb.Append(";");
            }

            var b = Encoding.ASCII.GetBytes(sb.ToString());

            var buffer = new byte[b.Length + 2];
            buffer[0] = SOH;
            buffer[buffer.Length - 1] = EOH;
            Buffer.BlockCopy(b, 0, buffer, 1, b.Length);

            return buffer;
        }

        public static ProtocolMessage UnWrap(byte[] rawMessage)
        {
            if (rawMessage.Length <= 2) return null;

            var subMessage = new byte[rawMessage.Length - 2];

            Array.Copy(rawMessage, 1, subMessage, 0, subMessage.Length);

            var str = Encoding.ASCII.GetString(subMessage);
            var s = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length == 0) return null;

            var t = MessageTypeIdEnum.Unknown;

            var dic = new Dictionary<string, string>();
            foreach (var pair in s)
            {
                var p = pair.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length != 2) continue;

                if (p[0] == "EVENT")
                {
                    if (p[1].Trim().Length == 0) return null;

                    var parsed = Enum.TryParse(p[1].Trim(), out t);
                    if (!parsed)
                        return null;
                }
                else
                {
                    var key = p[0];
                    var value = p[1];

                    if (dic.ContainsKey(key)) continue;

                    dic.Add(key, value);
                }
            }

            var message = new ProtocolMessage(t, dic);
            return message;
        }
    }
}
