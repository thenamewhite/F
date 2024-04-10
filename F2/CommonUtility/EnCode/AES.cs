using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace F
{
    public static class AES
    {
        //默认密钥向量   
        private readonly static byte[] mAESKey1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        public static byte[] Encode(string str, string key)
        {
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(str);
            byte[] keyBytes = new byte[32];
            Encoding.UTF8.GetBytes(key).CopyTo(keyBytes, 0);
            des.Key = keyBytes;
            des.IV = mAESKey1;
            using (var ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="i">密文字节数组</param>  
        /// <param name="key">密钥</param>  
        /// <returns>返回解密后的字符串</returns>  
        public static byte[] Decode(byte[] i, string key)
        {
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] keyBytes = new byte[32];
            Encoding.UTF8.GetBytes(key).CopyTo(keyBytes, 0);
            des.Key = keyBytes;
            des.IV = mAESKey1;
            byte[] decryptBytes = new byte[i.Length];
            using (MemoryStream ms = new MemoryStream(i))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                    return decryptBytes;
                }
            }
        }
    }
}
