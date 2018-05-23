using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWLuaOOG
{
    class Math
    {
        public void RandomTable(string tablename, int count)
        {
            Program.lua.NewTable(tablename);
            byte[] buffer = new byte[count];
            new Random().NextBytes(buffer);
            for (int i = 1; i <= buffer.Length; i++)
            {
                ((LuaInterface.LuaTable)Program.lua[tablename])[i] = buffer[i - 1];
            }
        }
    }
}
