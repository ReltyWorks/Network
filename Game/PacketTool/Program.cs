namespace PacketTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("패킷 자동 생성 시작");

            if (args.Length != 2)
            {
                Console.WriteLine("패킷 자동 생성에 필요한 인자가 충분하지 않습니다. 패킷 자동 생성을 종료합니다.");
                return;
            }

            string defPath = args[0];
            string outDir = args[1];

            if (!File.Exists(defPath))
                throw new FileNotFoundException($"패킷 메시지가 정의된 파일을 찾을 수 없습니다. {args[0]}");

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            TypeBuilderForPackets.Build(defPath, outDir);
        }
    }
}
