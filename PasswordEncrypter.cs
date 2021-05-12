using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Encrypter
{
    class PasswordEncrypter
    {
        /// <summary>
        /// Call this function to remove the key from memory after use for security
        /// </summary>
        /// <param name="targetAddress"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr targetAddress, int Length);

        /// <summary>
        /// File is encrypted with ACSII encryption
        /// </summary>
        /// <param name="fileContent"></param>
        /// <param name="password"></param>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        /// <param name="abc"></param>
        /// <param name="table"></param>
        public void FileASCIIEncrypt(byte[] fileContent, string password, byte[] keys, byte[] result, byte[] abc, byte[,] table)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            for (int i = 0; i < fileContent.Length; i++)
            {
                byte value = fileContent[i];
                byte key = keys[i];
                int valueIndex = -1, keyIndex = -1;

                for (int j = 0; j < 256; j++)
                    if (abc[j] == value)
                    {
                        valueIndex = j;
                        break;
                    }
                
                for (int j = 0; j < 256; j++)
                    if (abc[j] == key)
                    {
                        keyIndex = j;
                        break;
                    }
           
                result[i] = table[keyIndex, valueIndex];
            }

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }

        /// <summary>
        /// File is decrypted with ACSII decryption
        /// </summary>
        /// <param name="fileContent"></param>
        /// <param name="password"></param>
        /// <param name="keys"></param>
        /// <param name="result"></param>
        /// <param name="abc"></param>
        /// <param name="table"></param>
        public void FileASCIIDecrypt(byte[] fileContent, string password, byte[] keys, byte[] result, byte[] abc, byte[,] table)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            for (int i = 0; i < fileContent.Length; i++)
             {
                byte value = fileContent[i];
                byte key = keys[i];
                int valueIndex = -1, keyIndex = -1;
  
                for (int j = 0; j < 256; j++)
                    if (abc[j] == key)
                    {
                        keyIndex = j;
                        break;
                    }
                
                for (int j = 0; j < 256; j++)
                    if (table[keyIndex, j] == value)
                    {
                        valueIndex = j;
                        break;
                    }
                

                
                result[i] = abc[valueIndex];
                }

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }

        /// <summary>
        /// Fills an array of bytes with a cryptographically strong sequence of random values.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateSalt()
        {
            byte[] data = new byte[32];
            using (RNGCryptoServiceProvider CryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                CryptoServiceProvider.GetBytes(data); 
            }
            return data;
        }

        /// <summary>
        /// File is encrypted with AES256 encryption
        /// </summary>
        /// <param name="tbPath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task FileAESEncrypt(string tbPath, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            byte[] salt = await Task.Run(GenerateSalt);
            byte[] passwords = await Task.Run(() => Encoding.UTF8.GetBytes(password));
            RijndaelManaged AES = new RijndaelManaged(); //Create a new instance of the RijndaelManaged (Rijndael algorithm)

            //define the RijndaelManaged object in two tasks
            var taskOne = Task.Run(async() =>
            {
                AES.KeySize = 256;//aes 256 bit encryption 
                AES.BlockSize = 128;//aes 128 bit encryption 
                AES.Padding = await Task.Run(() => PaddingMode.PKCS7);
            });

            var taskTwo = Task.Run(async() =>
            {
                var key = new Rfc2898DeriveBytes(passwords, salt, 50000);
                AES.Key = await Task.Run(() => key.GetBytes(AES.KeySize / 8));
                AES.IV = await Task.Run(() => key.GetBytes(AES.BlockSize / 8));
                AES.Mode = await Task.Run(() => CipherMode.CFB);
            }
            );
            //create streams used for encryption
            using (FileStream fileCrypt = new FileStream(tbPath + ".aes", FileMode.Create))
            {
                await Task.Run(()=>fileCrypt.Write(salt, 0, salt.Length));
                taskOne.Wait();
                taskTwo.Wait();
                using (CryptoStream cryptoStream = new CryptoStream(fileCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (FileStream fileRead = new FileStream(tbPath, FileMode.Open))
                    {
                        byte[] buffer = new byte[1048576];
                        int read;
                        while ((read = fileRead.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cryptoStream.Write(buffer, 0, read); //write all data to the stream
                        } 
                       
                    }
                }
            }
            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }

        /// <summary>
        /// File is decrypted with AES256 decryption
        /// </summary>
        /// <param name="tbPathAES"></param>
        /// <param name="tbPath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task FileAESDecrypt(string tbPathAES, string tbPath, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            byte[] passwords = await Task.Run(()=>Encoding.UTF8.GetBytes(password));
            byte[] salt = new byte[32];

            using (FileStream fileCrypt = new FileStream(tbPathAES, FileMode.Open))
            {
                var taskOne = Task.Run(()=>fileCrypt.Read(salt, 0, salt.Length));
                
                RijndaelManaged AES = new RijndaelManaged(); //Create a new instance of the RijndaelManaged (Rijndael algorithm)

                var taskTwo= Task.Run(async()=>
                {
                    ////define the RijndaelManaged object
                    AES.KeySize = 256;//aes 256 bit encryption 
                    AES.BlockSize = 128;//aes 128 bit encryption 
                    var key = new Rfc2898DeriveBytes(passwords, salt, 50000);
                    AES.Key = await Task.Run(() => key.GetBytes(AES.KeySize / 8));
                    AES.IV = await Task.Run(() => key.GetBytes(AES.BlockSize / 8));
                    AES.Padding = await Task.Run(() => PaddingMode.PKCS7);
                    AES.Mode = await Task.Run(() => CipherMode.CFB);
                });

                taskOne.Wait();
                taskTwo.Wait();

                //create streams used for encryption
                using (CryptoStream cryptoStream = new CryptoStream(fileCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (FileStream fileOut = new FileStream(tbPath, FileMode.Create))
                    {
                        int read;
                        byte[] buffer = new byte[1048576];
                        while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileOut.Write(buffer, 0, read); //write all data to the stream
                        }
                    }
                }
            }
            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }
    }
}
