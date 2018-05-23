using System;
using System.Collections.Generic;
using System.Text;

namespace Crypt
{
    public class EncDec
    {
        RC4 enc;
        RC4 dec;
        MPPC mppc;
        public void CreateEnc(byte[] key)
        {
            enc = new RC4();
            enc.Shuffle(key);
            mppc = new MPPC();
        }
        public void CreateDec(byte[] key)
        {
            dec = new RC4();
            dec.Shuffle(key);
        }
        public byte[] Decode(byte[] allbuf, int len)
        {
            byte[] buf = new byte[len];
            Array.Copy(allbuf, buf, len);
            if (dec == null)
                return buf;
            byte[] ret = new byte[buf.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = dec.Encode(buf[i]);
            return ret;
        }
        public byte[] Encode(byte[] allbuf)
        {
            if (enc == null)
                return allbuf;

            byte[] buf = allbuf;
            buf = mppc.Pack(buf);
            for (int i = 0; i < buf.Length; i++)
                buf[i] = enc.Encode(buf[i]);

            return buf;
        }
    }
}
