using System;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography;


namespace Gurus.Utils
{
    public class Cryptography
    {
        static string Passphrase = "#dLj-e7Yx8CkQkYBTh+2=$w^xwK&xC&!^BeKr?a^-vdqWC&Rnc-6N#yBq6BANxmmgZ-XBGBQzv6U-UezkQyVj7*#9wa+b9Mmc++uuu#r!4q!uHgUny%j!cG8B4LWz*@L$hhunrSRrXMr&fE_K9K7^4%EuVz^yhLnGC6Aqf58bGr8p24SKp+eR=&PVax%yrJQNJX+YXw*5C*?sFWNtNuH%r69ZDFCQ!Z+a$=V5R87c@c3LD+!JEUNVwgH6p9=$$_+";

        public static string DecryptString(string Message)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToDecrypt = Convert.FromBase64String(Message);
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return UTF8.GetString(Results);
        }
    }
}