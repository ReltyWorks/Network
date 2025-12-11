/*
* 이 코드는 자동생성된 코드이므로 편집하지 마시오.
*/
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.NetworkContracts
{
    public static class PacketFactory
    {
        static Dictionary<PacketId, Func<IPacket>> s_constructors = new Dictionary<PacketId, Func<IPacket>>
        {
            {PacketId.S_ConnectionSuccess,() => new S_ConnectionSuccess()},
            {PacketId.S_ConnectionFailure,() => new S_ConnectionFailure()},
            {PacketId.S_ChatSend,() => new S_ChatSend()},
            {PacketId.C_ChatSend,() => new C_ChatSend()},
        };
        public static byte[] ToBytes(IPacket packet)
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
            // Packet Id 도 못읽는 짧은 데이터는 잘못된 데이터다
            if (bytes.Length < sizeof(PacketId))
                return null;

            PacketId packetId = (PacketId)BitConverter.ToUInt16(bytes);

            // 생성할수있는 패킷 ?
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
