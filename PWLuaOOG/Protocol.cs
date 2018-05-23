using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace PWLuaOOG
{
    class Protocol
    {
        public bool Connect(string link, int port)
        {
            return Program.Client.TryConnect(link, port);
        }

        public void Disconnect()
        {
            Program.Client.Disconnect();
        }

        public void InitRC4(byte[] CMKey, LuaInterface.LuaTable SMKey, LuaInterface.LuaTable Hash, string login)
        {
            byte[] SMKeyBytes = new byte[SMKey.Keys.Count];
            byte[] HashBytes = new byte[Hash.Keys.Count];
            for (int i = 1; i <= SMKey.Keys.Count; i++)
                SMKeyBytes[i - 1] = Convert.ToByte(SMKey[i]);

            for (int i = 1; i <= Hash.Keys.Count; i++)
                HashBytes[i - 1] = Convert.ToByte(Hash[i]);

            Program.RC4_Client = new RC4(GetKey(CMKey, HashBytes, login));
            Program.RC4_Server = new RC4(GetKey(SMKeyBytes, HashBytes, login));

            Program.Encode = true;
        }

        private byte[] GetKey(byte[] enchash_key, byte[] hash, string login)
        {
            byte[] buffer = new byte[enchash_key.Length + hash.Length];
            Array.Copy(hash, 0, buffer, 0, hash.Length);
            Array.Copy(enchash_key, 0, buffer, hash.Length, enchash_key.Length);
            return new HMACMD5(Encoding.ASCII.GetBytes(login)).ComputeHash(buffer);
        }

        public static uint ReadCUInt32(ref BinaryReader data)
        {
            byte code = data.ReadByte();
            switch (code & 0xE0)
            {
                case 0xE0:
                    return Convertation.ReverseBytes(data.ReadUInt32());
                case 0xC0:
                    byte[] bt = data.ReadBytes(3);
                    Array.Reverse(bt);
                    return BitConverter.ToUInt32(new byte[] { bt[2], bt[1], bt[0], code }, 0) & 0x1FFFFFFF;
                case 0x80:
                case 0xA0:
                    return (uint)(BitConverter.ToUInt16(new byte[] { data.ReadByte(), code }, 0) & 0x3FFF);
            }
            return (uint)code;
        }
    }
}
