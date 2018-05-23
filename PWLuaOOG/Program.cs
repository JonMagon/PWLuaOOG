using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaInterface;
using System.IO;
using NetSockets;
using Crypt;

namespace PWLuaOOG
{
    class Program
    {
        public static NetClient Client;
        public static Lua lua = new Lua();
        static Dictionary<FunctionType, LuaFunction> Functions = new Dictionary<FunctionType, LuaFunction>();

        public static bool Encode = false;

        public static RC4 RC4_Client;
        public static RC4 RC4_Server;

        private static MppcUnpacker Compressor = new MppcUnpacker();

        enum FunctionType
        {
            Main,
            Connected,
            Disconnected,
            Received,
            ReceivedSubPacket
        }

        private static bool debug_mode = false;

        static void Main(string[] args)
        {
            Encode = false;
            Console.Title = "PWLuaOOG 0.3b [by JonMagon]";

            Console.WriteLine("PWLuaOOG 0.3b [by JonMagon]");
            Console.WriteLine("");

            lua["Console"] = new LuaConsole();
            lua["ReceivedPacket"] = new ReceivedPacket();
            lua["SendPacket"] = new SendPacket();
            lua["Protocol"] = new Protocol();
            lua["Crypt"] = new Crypt();
            lua["Math"] = new Math();

            // Очень криво работают потоки
            //Threads threads = new Threads();
            //lua.RegisterFunction("Thread", threads, threads.GetType().GetMethod("Thread"));

            SendPacket.Data = new List<byte>();

            Client = new NetClient();
            Client.OnConnected += client_OnConnected;
            Client.OnDisconnected += Client_OnDisconnected;
            Client.OnReceived += client_OnReceived;

            try
            {
                lua.DoFile(@".\scripts\main.lua");

                if (lua["debug_mode"] != null)
                    debug_mode = true;

                SendPacket.debug_mode = debug_mode;

                Functions.Add(FunctionType.Main, (LuaFunction)lua["Main"]);
                Functions.Add(FunctionType.Connected, (LuaFunction)lua["Connected"]);
                Functions.Add(FunctionType.Disconnected, (LuaFunction)lua["Disconnected"]);
                Functions.Add(FunctionType.Received, (LuaFunction)lua["Received"]);
                Functions.Add(FunctionType.ReceivedSubPacket, (LuaFunction)lua["ReceivedSubPacket"]);

                if (Functions[FunctionType.Main] == null)
                    throw new Exception("Ошибка: не найдена функция инициализации Main");

                if (Functions[FunctionType.Connected] == null)
                    Console.WriteLine("Предупреждение: не найдена функция Connected()");

                if (Functions[FunctionType.Disconnected] == null)
                    Console.WriteLine("Предупреждение: не найдена функция Disconnected()");

                if (Functions[FunctionType.Received] == null)
                    Console.WriteLine("Предупреждение: не найдена функция Received(opcode, length)");

                if (Functions[FunctionType.ReceivedSubPacket] == null)
                    Console.WriteLine("Предупреждение: не найдена функция ReceivedSubPacket(opcode, length)");

                Functions[FunctionType.Main].Call();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return;
            }
        }

        static void Client_OnDisconnected(object sender, NetDisconnectedEventArgs e)
        {
            try
            {
                if (Functions[FunctionType.Disconnected] != null)
                    Functions[FunctionType.Disconnected].Call();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void client_OnConnected(object sender, NetConnectedEventArgs e)
        {
            try
            {
                if (Functions[FunctionType.Connected] != null)
                    Functions[FunctionType.Connected].Call();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void client_OnReceived(object sender, NetReceivedEventArgs<byte[]> e)
        {
            try
            {
                while (true)
                {
                    if (ReceivedPacket.Data == null)
                        break;

                    System.Threading.Thread.Sleep(1);
                }
                BinaryReader DataPacket = new BinaryReader(new MemoryStream(Encode ? Compressor.Unpack(RC4_Server.Decode(e.Data, e.Data.Length)) : e.Data));

                ReceivedPacket.Data = DataPacket;

                ReceivedPacket.Opcode = Protocol.ReadCUInt32(ref ReceivedPacket.Data);
                ReceivedPacket.Length = Protocol.ReadCUInt32(ref ReceivedPacket.Data);

                if (debug_mode)
                    Console.WriteLine("[S -> C]: 0x{0} Length: {1}", ReceivedPacket.Opcode.ToString("X2"), ReceivedPacket.Length);

                if (ReceivedPacket.Opcode == 0x02)
                    new System.Threading.Thread(delegate()
                    {
                        while (true)
                        {
                            System.Threading.Thread.Sleep(12000);
                            if (!Program.Client.IsConnected || !Encode)
                                break;

                            Client.Send(RC4_Client.Encode(new byte[] { 0x5A, 0x01, 0x5A }, 3));
                        }
                    }).Start();

                if (ReceivedPacket.Opcode == 0x00)
                {
                    DataPacket.BaseStream.Position = ReceivedPacket.Data.BaseStream.Position;
                    while (DataPacket.PeekChar() != -1)
                    {
                        byte header = DataPacket.ReadByte();

                        uint sub_size = Protocol.ReadCUInt32(ref DataPacket);

                        ReceivedPacket.Data = new BinaryReader(new MemoryStream(DataPacket.ReadBytes((int)sub_size)));

                        if (sub_size < 3)
                            continue;

                        ReceivedPacket.Length = Protocol.ReadCUInt32(ref ReceivedPacket.Data);
                        ReceivedPacket.Opcode = ReceivedPacket.Data.ReadUInt16();

                        if (debug_mode)
                            Console.WriteLine("[S -> C] [GS]: 0x{0} Length: {1}", ReceivedPacket.Opcode.ToString("X2"), ReceivedPacket.Length);

                        ReceivedPacket.isSubPacket = true;
                        SendPacket.Data = new List<byte>();

                        if (Functions[FunctionType.ReceivedSubPacket] != null)
                            Functions[FunctionType.ReceivedSubPacket].Call(new object[] { ReceivedPacket.Opcode, ReceivedPacket.Length });
                    }
                }
                else
                {
                    if (Functions[FunctionType.Received] != null)
                        Functions[FunctionType.Received].Call(new object[] { ReceivedPacket.Opcode, ReceivedPacket.Length });
                }
                ReceivedPacket.Opcode = 0;
                ReceivedPacket.Length = 0;
                ReceivedPacket.isSubPacket = false;
                ReceivedPacket.Data = null;
                SendPacket.Data = new List<byte>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}