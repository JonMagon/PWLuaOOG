using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;

namespace PWLuaOOG
{
    public class Crypt
    {
        public void GetHash(string login, string password, byte[] key, string tablename)
        {
            Program.lua.NewTable(tablename);
            byte[] hash = new HMACMD5(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(login.ToLower() + password))).ComputeHash(key);

            for (int i = 1; i <= hash.Length; i++)
            {
                ((LuaInterface.LuaTable)Program.lua[tablename])[i] = hash[i - 1];
            }
        }
    }
}
