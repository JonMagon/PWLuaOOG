using System;
using System.Collections.Generic;

namespace PWLuaOOG
{
    public class MppcUnpacker
    {
        private int code1;
        private int code2;
        private int code3;
        private int code4;

        private byte packedOffset;

        private readonly List<byte> packedBytes;
        private readonly List<byte> unpackedBytes;
        private List<byte> unpackedChunk;

        public MppcUnpacker()
        {
            code3 = 0; code4 = 0;

            packedBytes = new List<byte>();
            unpackedBytes = new List<byte>();
        }

        public byte[] Unpack(byte packedByte)
        {
            packedBytes.Add(packedByte);

            unpackedChunk = new List<byte>();

            if (unpackedBytes.Count >= 10240)
                unpackedBytes.RemoveRange(0, 2048);

            for (; ; )
            {
                if (code3 == 0)
                {
                    if (HasBits(4))
                    {
                        if (GetPackedBits(1) == 0)
                        {
                            // 0-xxxxxxx
                            code1 = 1;
                            code3 = 1;
                        }
                        else
                        {
                            if (GetPackedBits(1) == 0)
                            {
                                // 10-xxxxxxx
                                code1 = 2;
                                code3 = 1;
                            }
                            else
                            {
                                if (GetPackedBits(1) == 0)
                                {
                                    // 110-xxxxxxxxxxxxx-*
                                    code1 = 3;
                                    code3 = 1;
                                }
                                else
                                {
                                    if (GetPackedBits(1) == 0)
                                    {
                                        // 1110-xxxxxxxx-*
                                        code1 = 4;
                                        code3 = 1;
                                    }
                                    else
                                    {
                                        // 1111-xxxxxx-*
                                        code1 = 5;
                                        code3 = 1;
                                    }
                                }
                            }
                        }
                    }
                    else
                        break;
                }
                else if (code3 == 1)
                {
                    if (code1 == 1)
                    {
                        if (HasBits(7))
                        {
                            var outB = (byte)GetPackedBits(7);
                            unpackedChunk.Add(outB);
                            unpackedBytes.Add(outB);
                            code3 = 0;
                        }
                        else
                            break;
                    }
                    else if (code1 == 2)
                    {
                        if (HasBits(7))
                        {
                            var outB = (byte)(GetPackedBits(7) | 0x80);
                            unpackedChunk.Add(outB);
                            unpackedBytes.Add(outB);
                            code3 = 0;
                        }
                        else
                            break;
                    }
                    else if (code1 == 3)
                    {
                        if (HasBits(13))
                        {
                            code4 = (int)GetPackedBits(13) + 0x140;
                            code3 = 2;
                        }
                        else
                            break;
                    }
                    else if (code1 == 4)
                    {
                        if (HasBits(8))
                        {
                            code4 = (int)GetPackedBits(8) + 0x40;
                            code3 = 2;
                        }
                        else
                            break;
                    }
                    else if (code1 == 5)
                    {
                        if (HasBits(6))
                        {
                            code4 = (int)GetPackedBits(6);
                            code3 = 2;
                        }
                        else
                            break;
                    }
                }
                else if (code3 == 2)
                {
                    if (code4 == 0)
                    {
                        // Guess !!!
                        if (packedOffset != 0)
                        {
                            packedOffset = 0;
                            packedBytes.RemoveAt(0);
                        }
                        code3 = 0;
                        continue;
                    }
                    code2 = 0;
                    code3 = 3;
                }
                else if (code3 == 3)
                {
                    if (HasBits(1))
                    {
                        if (GetPackedBits(1) == 0)
                        {
                            code3 = 4;
                        }
                        else
                        {
                            code2++;
                        }
                    }
                    else
                        break;
                }
                else if (code3 == 4)
                {
                    int copySize;

                    if (code2 == 0)
                    {
                        copySize = 3;
                    }
                    else
                    {
                        var size = code2 + 1;

                        if (HasBits(size))
                        {
                            copySize = (int)GetPackedBits(size) + (1 << size);
                        }
                        else
                            break;
                    }

                    Copy(code4, copySize, ref unpackedChunk);
                    code3 = 0;
                }
            }

            return unpackedChunk.ToArray();
        }

        public byte[] Unpack(byte[] compressedBytes)
        {
            var rtnList = new List<byte>(compressedBytes.Length);

            foreach (var b in compressedBytes)
            {
                rtnList.AddRange(Unpack(b));
            }

            return rtnList.ToArray();
        }

        private void Copy(int shift, int size, ref List<byte> unpackedChunkData)
        {
            for (var i = 0; i < size; i++)
            {
                var pIndex = unpackedBytes.Count - shift;

                if (pIndex < 0)
                    return;

                var b = unpackedBytes[pIndex];
                unpackedBytes.Add(b);
                unpackedChunkData.Add(b);
            }
        }

        private uint GetPackedBits(int bitCount)
        {
            if (bitCount > 16)
                return 0;

            if (!HasBits(bitCount))
                throw new Exception("Unpack bit stream overflow");

            var alBitCount = bitCount + packedOffset;
            var alByteCount = (alBitCount + 7) / 8;

            uint v = 0;

            for (var i = 0; i < alByteCount; i++)
            {
                v |= (uint)(packedBytes[i]) << (24 - i * 8);
            }

            v <<= packedOffset;
            v >>= 32 - bitCount;

            packedOffset += (byte)bitCount;
            var freeBytes = packedOffset / 8;

            if (freeBytes != 0)
                packedBytes.RemoveRange(0, freeBytes);

            packedOffset %= 8;
            return v;
        }

        private bool HasBits(int count)
        {
            return (packedBytes.Count * 8 - packedOffset) >= count;
        }
    }
}