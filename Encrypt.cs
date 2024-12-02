using System;
using System.Security.Cryptography;
using System.Text;

namespace BookLendingSystem
{
    public class Encrypt
    {
        public static string MD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    int val = b & 0xFF;
                    if (val < 16)
                    {
                        sb.Append("0");
                    }
                    sb.Append(val.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
