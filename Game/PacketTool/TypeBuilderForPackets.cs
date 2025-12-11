using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketTool
{
    internal static class TypeBuilderForPackets
    {
        internal static void Build(string defPath, string outDir)
        {
            // 1. 정의해야하는 모든 패킷 모델 쿼리
            IEnumerable<PacketDef> packetDefs = File.ReadAllLines(defPath) // 전체 라인 읽음
                                                    .Select(l => l.Trim()) // 각 라인 앞뒤 공백 없앰
                                                    .Where(l => !l.StartsWith('#') && l.Length > 0) // 라인이 주석이거나 공백인것 제외
                                                    .Select(Parse);

            // 2. PacketId Enum 정의
            string enumText = BuildPacketIdEnum(packetDefs);
            File.WriteAllText(Path.Combine(outDir, "PacketId.cs"), enumText, Encoding.UTF8);

            // 3. Packet 클래스들 정의
            string classesText = BuildPacketClasses(packetDefs);
            File.WriteAllText(Path.Combine(outDir, "Packets.cs"), classesText, Encoding.UTF8);

            // 4. PacketFactory 클래스 정의
            string factoryClassText = BuildPacketFactoryClass(packetDefs);
            File.WriteAllText(Path.Combine(outDir, "PacketFactory.cs"), factoryClassText, Encoding.UTF8);
        }

        internal static PacketDef Parse(string line)
        {
            string[] splits = line.Split(new[] { ' ', '\t' }, 3, StringSplitOptions.RemoveEmptyEntries);
            string hexId = splits[0];
            string name = splits[1];
            List<FieldDef> fields = splits[2].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                             .Select(s =>
                                             {
                                                 string[] pair = s.Trim().Split(' ');
                                                 return new FieldDef(pair[1], pair[0]);
                                             })
                                             .ToList();

            return new PacketDef(name, hexId, fields);
        }

        static string BuildPacketIdEnum(IEnumerable<PacketDef> packetDefs)
        {
            StringBuilder sb = new StringBuilder("""
                /*
                * 이 코드는 자동생성된 코드이므로 편집하지 마시오.
                */
                using System;
                using System.Collections.Generic;
                using System.IO;
                using System.Linq;
                
                namespace Game.NetworkContracts
                {
                    public enum PacketId : ushort
                    {
                """);
            sb.AppendLine();

            foreach (var packetDef in packetDefs)
            {
                sb.AppendLine($"        {packetDef.Name} = {packetDef.HexId},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        static string BuildPacketClasses(IEnumerable<PacketDef> packetDefs)
        {
            StringBuilder sb = new StringBuilder("""
                /*
                * 이 코드는 자동생성된 코드이므로 편집하지 마시오.
                */
                using System;
                using System.Collections.Generic;
                using System.IO;
                using System.Linq;

                namespace Game.NetworkContracts
                {
                """);
            sb.AppendLine();

            foreach (var packetDef in packetDefs)
            {

                sb.AppendLine($"    public sealed class {packetDef.Name} : IPacket");
                sb.AppendLine("    {");
                sb.AppendLine($"        public PacketId PacketId => PacketId.{packetDef.Name};");
                sb.AppendLine();

                foreach (var fieldDef in packetDef.Fields)
                {
                    sb.AppendLine($"        public {fieldDef.CsType} {fieldDef.Name} {{ get; set; }}");
                    sb.AppendLine();
                }

                sb.AppendLine("        public void Serialize(BinaryWriter writer)");
                sb.AppendLine("        {");

                foreach (var fieldDef in packetDef.Fields)
                {
                    sb.AppendLine($"            writer.Write({fieldDef.Name});");
                }

                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        public void Deserialize(BinaryReader reader)");
                sb.AppendLine("        {");

                foreach (var fieldDef in packetDef.Fields)
                {
                    sb.AppendLine($"            {fieldDef.Name} = reader.Read{TypeLookup.TypeByName[fieldDef.CsType].Name}();");
                }

                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        static string BuildPacketFactoryClass(IEnumerable<PacketDef> packetDefs)
        {
            StringBuilder sb = new StringBuilder("""
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
                """);
            sb.AppendLine();

            foreach (var packetDef in packetDefs) 
            {
                sb.AppendLine($"            {{PacketId.{packetDef.Name},() => new {packetDef.Name}()}},");
            }

            sb.AppendLine("        };");

            sb.AppendLine("""
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
                """);
            return sb.ToString();
        }

        // record : 데이터 모델 정의용 클래스. 
        // 주로 DTO (Data Transfer Object) 정의시에 사용
        internal record FieldDef(string Name, string CsType);
        internal record PacketDef(string Name, string HexId, List<FieldDef> Fields);
    }
}
