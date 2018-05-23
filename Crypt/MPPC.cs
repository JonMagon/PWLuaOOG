using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Crypt
{
    public class MPPC
    {
        public byte[] decHistory = new byte[0xffff];
        public int j;
        public BitArray srcbinary;

        public byte[] Pack(byte[] src)
        {
            int num3;
            BitArray outputbits = new BitArray((src.Length * 9) + 11);
            int from = 0;
            int num2 = 0;
            BitArray array2 = new BitArray(src);
            try
            {
                this.ReOrderBitArray(array2);
            }
            catch
            {
            }
            while (from < array2.Length)
            {
                if (array2[from])
                {
                    outputbits[from + num2] = true;
                    num2++;
                    outputbits[from + num2] = false;
                    this.Copy(array2, from + 1, outputbits, (from + num2) + 1, 7);
                    from += 8;
                }
                else
                {
                    this.Copy(array2, from, outputbits, from + num2, 8);
                    from += 8;
                }
            }
            for (num3 = 0; num3 < 4; num3++)
            {
                outputbits[(from + num2) + num3] = true;
            }
            from += 4;
            for (num3 = 0; num3 < 6; num3++)
            {
                outputbits[(from + num2) + num3] = false;
            }
            from += 6;
            outputbits.Length = (((from + num2) % 8) == 0) ? (from + num2) : ((from + num2) + (8 - ((from + num2) % 8)));
            this.ReOrderBitArray(outputbits);
            return this.BitsToBytesCompressor(outputbits, outputbits.Length);
        }

        private byte[] BitsToBytesCompressor(BitArray target, int lenght)
        {
            int[] numArray = new int[lenght / 8];
            byte[] buffer = new byte[lenght / 8];
            for (int i = 0; i < (lenght / 8); i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (target[(i * 8) + j])
                    {
                        numArray[i] += Convert.ToInt32(Math.Pow(Convert.ToDouble(2), Convert.ToDouble(j)));
                    }
                }
                buffer[i] = BitConverter.GetBytes(numArray[i])[0];
            }
            return buffer;
        }

        private void Copy(BitArray inputbits, int from, BitArray outputbits, int to, int length)
        {
            for (int i = 0; i < length; i++)
            {
                outputbits[to + i] = inputbits[from + i];
            }
        }

        public void ReOrderBitArray(BitArray src)
        {
            for (int i = 0; i < src.Length; i += 8)
            {
                for (int j = 0; j < 4; j++)
                {
                    bool flag = src[i + j];
                    src[i + j] = src[i + (7 - j)];
                    src[i + (7 - j)] = flag;
                }
            }
        }
    }
}
