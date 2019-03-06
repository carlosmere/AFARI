using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Configuration;

namespace VEH.Intranet.Logic
{
    public class Encriptacion
    {
        private byte[] secretKey = {};
        private byte[] iv64 = {};

        public Encriptacion() 
        {
            var encriptacionSecretKey = ConfigurationManager.AppSettings["Encriptacion.SecretKey"].ToString();
            secretKey = System.Text.Encoding.UTF8.GetBytes(encriptacionSecretKey);
        }

        public String Desencriptar(String stringToDecrypt, String iv)
        {
            return Desencriptar(stringToDecrypt,iv,false);
        
        }

        public String Desencriptar(String stringToDecrypt, String iv, Boolean allowNullOrEmpty)
        {
            if (allowNullOrEmpty && String.IsNullOrEmpty(stringToDecrypt))
                return "";

            stringToDecrypt = stringToDecrypt.Replace(" ", "+");
            iv64 = System.Text.Encoding.UTF8.GetBytes(iv);
            TripleDESCryptoServiceProvider cryptoSP = new TripleDESCryptoServiceProvider();
            byte[] inputByteArray = Convert.FromBase64String(stringToDecrypt);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoSP.CreateDecryptor(secretKey, iv64), CryptoStreamMode.Write);
            cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
            cryptoStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        public String Encriptar(String stringToEncrypt, String iv)
        {
            try
            {
                iv64 = System.Text.Encoding.UTF8.GetBytes(iv);
                TripleDESCryptoServiceProvider cryptoSP = new TripleDESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoSP.CreateEncryptor(secretKey, iv64), CryptoStreamMode.Write);
                cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
