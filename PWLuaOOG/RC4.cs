using System;
using System.Linq;

namespace PWLuaOOG
{
    public class RC4
    {
        private byte[] S = new byte[0x100];
        private int x = 0;
        private int y = 0;

        public RC4(byte[] key)
        {
            this.init(key);
        }

        public byte[] Decode(byte[] dataB, int size)
        {
            return this.Encode(dataB, size);
        }

        public byte[] Encode(byte[] dataB, int size)
        {
            byte[] buffer = dataB.Take<byte>(size).ToArray<byte>();
            byte[] buffer2 = new byte[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer2[i] = (byte)(buffer[i] ^ this.keyItem());
            }
            return buffer2;
        }

        private void init(byte[] key)
        {
            int num2;
            int length = key.Length;
            for (num2 = 0; num2 < 0x100; num2++)
            {
                this.S[num2] = (byte)num2;
            }
            int num3 = 0;
            for (num2 = 0; num2 < 0x100; num2++)
            {
                num3 = ((num3 + this.S[num2]) + key[num2 % length]) % 0x100;
                this.S.Swap<byte>(num2, num3);
            }
        }

        private byte keyItem()
        {
            this.x = (this.x + 1) % 0x100;
            this.y = (this.y + this.S[this.x]) % 0x100;
            this.S.Swap<byte>(this.x, this.y);
            return this.S[(this.S[this.x] + this.S[this.y]) % 0x100];
        }
    }
}

