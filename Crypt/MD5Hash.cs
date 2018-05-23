using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Crypt
{
    public class MD5Hash
    {
        byte[] Hash = new byte[16];
        byte[] Login = new byte[16];
        public byte[] GetHash(string Login, byte[] passhash, byte[] key)
        {
            this.Login = Encoding.ASCII.GetBytes(Login);
            byte[] hash = new HMACMD5(passhash).ComputeHash(key);
            Hash = hash;
            return hash;
        }
        public byte[] GetKey(byte[] key)
        {
            byte[] nhash = new byte[key.Length + Hash.Length];
            for (int i = 0; i < Hash.Length; i++) nhash[i] = Hash[i];
            for (int i = 0; i < key.Length; i++) nhash[i + Hash.Length] = key[i];

            byte[] hash = new HMACMD5(Login).ComputeHash(nhash);
            return hash;
        }
    }
}
