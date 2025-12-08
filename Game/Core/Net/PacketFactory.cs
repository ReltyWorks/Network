using System;
using System.IO;
using System.Collections.Generic;

namespace Core.Net
{
    public static class PacketFactory
    {
        private static Dictionary<PacketId, Func<IPacket>> s_constructors = new Dictionary<PacketId, Func<IPacket>>
        {
            { PacketId.S_ChatSend, () => new S_ChatSend() },
            { PacketId.C_ChatSend, () => new C_ChatSend() }
        };

        public static byte[] ToByte(IPacket packet)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)packet.PacketId);
                packet.Serialize(writer);

                return stream.ToArray();
            }
        }

        public static IPacket FromBytes(byte[] bytes)
        {
            // Packet Id 도 못읽는 짧은 데이터는 잘못된 데이터
            if (bytes.Length == sizeof(PacketId))
                return null;

            PacketId packetId = (PacketId)BitConverter.ToUInt16(bytes);

            // 생성할 수 있는 패킷인지?
            if (s_constructors.TryGetValue(packetId, out Func<IPacket> constructor) == false)
                return null;

            IPacket packet = constructor.Invoke();

            using (MemoryStream stream = new MemoryStream(bytes, sizeof(PacketId), bytes.Length - sizeof(PacketId)))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                packet.Deserialize(reader);

                return packet;
            }
        }
    }
}
