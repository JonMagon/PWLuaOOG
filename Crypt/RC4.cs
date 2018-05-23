using System;
using System.Collections.Generic;
using System.Text;
using dword = System.Int32;

namespace Crypt
{
    public class RC4
    {
        public RC4()
        {
            for (dword i = 0; i < 256; i++)
                m_Table[i] = Convert.ToByte(i);
            m_Shift1 = 0;
            m_Shift2 = 0;
        }

        public void Shuffle(byte[] Key)
        {
            byte Shift = 0;
            for (dword i = 0; i < 256; i++)
            {
                byte A = Key[i % 16];
                Shift += (byte)(A + m_Table[i]);

                byte B = m_Table[i];
                m_Table[i] = m_Table[Shift];
                m_Table[Shift] = B;
            }
        }

        public byte Encode(byte InPacket)
        {
            m_Shift1++;
            byte A = m_Table[m_Shift1];
            m_Shift2 += A;
            byte B = m_Table[m_Shift2];
            m_Table[m_Shift2] = A;
            m_Table[m_Shift1] = B;
            byte C = (byte)(A + B);
            byte D = m_Table[C];
            return (byte)(InPacket ^ D);
        }

        private byte m_Shift1;
        private byte m_Shift2;
        private byte[] m_Table = new byte[256];
    }
}
//*/