using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PWLuaOOG
{
    public class ReceivedPacket
    {
        public static uint Opcode;
        public static uint Length;
        public static bool isSubPacket;
        public static BinaryReader Data;

        public uint Position
        {
            get
            {
                return (uint)Data.BaseStream.Position;
            }
            set
            {
                try
                {
                    Data.BaseStream.Position = (long)value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void Seek(int offset, int dirseek = 2)
        {
            if (dirseek == 1)
                Data.BaseStream.Seek((long)offset, System.IO.SeekOrigin.Begin);
            else if (dirseek == 2)
                Data.BaseStream.Seek((long)offset, System.IO.SeekOrigin.Current);
            else if (dirseek == 3)
                Data.BaseStream.Seek((long)offset, System.IO.SeekOrigin.End);
        }

        public uint StreamLength
        {
            get
            {
                return (uint)Data.BaseStream.Length;
            }
        }

        public byte[] ReadBytes(int length)
        {
            try
            {
                return Data.ReadBytes(length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public byte ReadByte()
        {
            try
            {
                return Data.ReadByte();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public uint ReadDword()
        {
            try
            {
                return !isSubPacket ? Convertation.ReverseBytes((uint)Data.ReadInt32()) : (uint)Data.ReadInt32();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public uint ReadWord()
        {
            try
            {
                return !isSubPacket ? Convertation.ReverseBytes((uint)Data.ReadInt16()) : (uint)Data.ReadInt16();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public string ReadString()
        {
            try
            {
                return Encoding.ASCII.GetString(Data.ReadBytes((int)Protocol.ReadCUInt32(ref Data)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public string ReadUString()
        {
            try
            {
                return Encoding.Unicode.GetString(Data.ReadBytes((int)Protocol.ReadCUInt32(ref Data)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public string ReadUStringZ(uint length)
        {
            try
            {
                string text = Encoding.Unicode.GetString(Data.ReadBytes((int)length));
                return text.Contains("\0") == false ? text : text.Substring(0, text.IndexOf("\0"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public uint ReadCUInt()
        {
            try
            {
                return Protocol.ReadCUInt32(ref Data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

    }
}