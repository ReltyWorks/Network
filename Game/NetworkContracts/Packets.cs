/*
* 이 코드는 자동생성된 코드이므로 편집하지 마시오.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.NetworkContracts
{
    public sealed class S_ConnectionSuccess : IPacket
    {
        public PacketId PacketId => PacketId.S_ConnectionSuccess;

        public int AssignedClientId { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(AssignedClientId);
        }

        public void Deserialize(BinaryReader reader)
        {
            AssignedClientId = reader.ReadInt32();
        }
    }

    public sealed class S_ConnectionFailure : IPacket
    {
        public PacketId PacketId => PacketId.S_ConnectionFailure;

        public string Reason { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Reason);
        }

        public void Deserialize(BinaryReader reader)
        {
            Reason = reader.ReadString();
        }
    }

    public sealed class S_ChatSend : IPacket
    {
        public PacketId PacketId => PacketId.S_ChatSend;

        public int SenderId { get; set; }

        public string Text { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(SenderId);
            writer.Write(Text);
        }

        public void Deserialize(BinaryReader reader)
        {
            SenderId = reader.ReadInt32();
            Text = reader.ReadString();
        }
    }

    public sealed class C_ChatSend : IPacket
    {
        public PacketId PacketId => PacketId.C_ChatSend;

        public string Text { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Text);
        }

        public void Deserialize(BinaryReader reader)
        {
            Text = reader.ReadString();
        }
    }

}
