using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkProgrammingP47.Services
{
    /// <summary>
    /// Key Derivation Function by RFC 2898
    /// </summary>
    internal class KdfService
    {
        public static String Dk(String password, String salt)
        {
            int c = 100000;
            String t = Hash(password + salt);
            for (int i = 0; i < c; i++)
            {
                t = Hash(t);
            }
            return t;
        }

        private static String Hash(String input)
        {
            return System.Convert.ToHexString(
                System.Security.Cryptography.MD5.HashData(
                    System.Text.Encoding.UTF8.GetBytes(input) 
            ));
        }
    }
}
