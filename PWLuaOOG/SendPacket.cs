using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PWLuaOOG
{
    class SendPacket
    {
        public static bool debug_mode;
        public static List<byte> Data;

        public void Send(uint Opcode)
        {
            if (debug_mode)
                Console.WriteLine("[C -> S]: 0x{0} Length: {1}", Opcode.ToString("X2"), Data.Count);

            Data.InsertRange(0, WriteCUInt32((uint)Data.Count));
            Data.InsertRange(0, WriteCUInt32(Opcode));

            Program.Client.Send(Program.Encode ? Program.RC4_Client.Encode(Data.ToArray(), Data.Count) : Data.ToArray());
            Data = new List<byte>();
        }

        public uint StreamLength
        {
            get
            {
                return (uint)Data.Count();
            }
        }

        public void WriteCUInt(uint value)
        {
            Data.AddRange(WriteCUInt32(value));
        }

        public void WriteBytes(LuaInterface.LuaTable value)
        {
            for (int i = 1; i <= value.Keys.Count; i++)
                Data.Add(Convert.ToByte(value[i]));
        }

        public void WriteWord(short value, bool swap = false)
        {
            Data.AddRange(swap ? Convertation.ReverseBytes(BitConverter.GetBytes(value)) : BitConverter.GetBytes(value));
        }

        public void WriteByte(byte value)
        {
            Data.Add(value);
        }

        public void WriteDword(int value, bool swap = false)
        {
            Data.AddRange(swap ? Convertation.ReverseBytes(BitConverter.GetBytes(value)) : BitConverter.GetBytes(value));
        }

        public void WriteString(string value, bool writesize = true)
        {
            byte[] bytes = Encoding.GetEncoding(1251).GetBytes(value);

            if (writesize)
                Data.AddRange(WriteCUInt32((uint)bytes.Length));

            Data.AddRange(bytes);
        }

        public void WriteUString(string value, bool writesize = true)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value.Replace("{magic_code}", ""));

            if (writesize)
                Data.AddRange(WriteCUInt32((uint)bytes.Length));

            Data.AddRange(bytes);
        }

        public void WriteUStringZ(string value, uint count)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            Data.AddRange(bytes);

            if (bytes.Count() < count)
                Data.AddRange(new byte[count - bytes.Count()]);
        }

        public void PackContainer(ushort Opcode)
        {
            Data.InsertRange(0, BitConverter.GetBytes(Opcode));
            Data.InsertRange(0, WriteCUInt32((uint)Data.Count));
        }

        private static byte[] WriteCUInt32(uint value)
        {
            if (value <= 0x7F)
            {
                return new byte[] { (byte)value };
            }
            if (value <= 0x3FFF)
            {
                byte[] bt = BitConverter.GetBytes((ushort)(value + 0x8000));
                Array.Reverse(bt);
                return bt;
            }
            if (value <= 0x1FFFFFFF)
            {
                byte[] bt = BitConverter.GetBytes((uint)(value + 0xC0000000));
                Array.Reverse(bt);
                return bt;
            }
            if (value <= 0xFFFFFFFF)
            {
                List<byte> bt = new List<byte>();
                bt.Add(0xE0);
                byte[] arrbt = BitConverter.GetBytes((uint)value);
                Array.Reverse(arrbt);
                bt.AddRange(arrbt);
                return bt.ToArray();
            }
            return new byte[] { (byte)value };
        }
    }
}
