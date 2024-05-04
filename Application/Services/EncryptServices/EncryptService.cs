using Application.Extensions;
using Application.SiteSetting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;

namespace Application.Services.EncryptServices
{
    internal class EncryptService : IScopedDependency, IEncryptService
    {
        private readonly ICryptographySetting _setting;

        public EncryptService(ICryptographySetting setting)
        {
            _setting = setting;
        }

        public IServiceResponse<string> Decrypt(string encryptedText)
        {
            var Errors = new Hashtable();
            if (string.IsNullOrEmpty(encryptedText))
            {
                Errors.Add("encryptedText", "empty");
                return new ServiceRespnse<string>().Failed(System.Net.HttpStatusCode.NotFound, Errors);
            }

            byte[] iv = new byte[16];
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_setting.Key);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return new ServiceRespnse<string>().OK(streamReader.ReadToEnd());
                        }
                    }
                }
            }
        }


        public IServiceResponse<string> Encrypt(string text)
        {
            var Errors = new Hashtable();
            if (string.IsNullOrEmpty(text))
            {
                Errors.Add("text", "empty");
                return new ServiceRespnse<string>().Failed(System.Net.HttpStatusCode.NotFound, Errors);
            }

            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_setting.Key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(text);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }
            return new ServiceRespnse<string>().OK(Convert.ToBase64String(array));

        }

    }

}
