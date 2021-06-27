using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Encrypter.FileEncrypter
{
    /// <summary>
    /// responsible to encrypt the files
    /// </summary>
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
        public static void FileASCIIEncrypt(byte[] fileContent, string password, byte[] keys, byte[] result, byte[] abc, byte[,] table)
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
        public static void FileASCIIDecrypt(byte[] fileContent, string password, byte[] keys, byte[] result, byte[] abc, byte[,] table)
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
            using (RNGCryptoServiceProvider CryptoServiceProvider = new())
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
        public static async Task FileAESEncrypt(string tbPath, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            byte[] salt = await Task.Run(GenerateSalt);
            byte[] passwords = await Task.Run(() => Encoding.UTF8.GetBytes(password));
            RijndaelManaged AES = new(); //Create a new instance of the RijndaelManaged (Rijndael algorithm)

            //define the RijndaelManaged object in two tasks
            var taskOne = Task.Run(() => {
                AES.KeySize = 256;//aes 256 bit encryption 
                AES.BlockSize = 128;//aes 128 bit encryption 
                AES.Padding = PaddingMode.PKCS7;
                return Task.CompletedTask;
            });

            var taskTwo = Task.Run(async() =>
            {
                var key = new Rfc2898DeriveBytes(passwords, salt, 50000);
                AES.Key = await Task.Run(() => key.GetBytes(AES.KeySize / 8));
                AES.IV = await Task.Run(() => key.GetBytes(AES.BlockSize / 8));
                AES.Mode = CipherMode.CFB;
                return Task.CompletedTask;
            }
            );
            //create streams used for encryption
            using (FileStream fileCrypt = new(tbPath + ".aes", FileMode.Create))
            {
                await Task.Run(()=>fileCrypt.Write(salt, 0, salt.Length));
                taskOne.Wait();
                taskTwo.Wait();
                using (CryptoStream cryptoStream = new(fileCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (FileStream fileRead = new(tbPath, FileMode.Open))
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
        public static async Task FileAESDecrypt(string tbPathAES, string tbPath, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            byte[] passwords = await Task.Run(()=>Encoding.UTF8.GetBytes(password));
            byte[] salt = new byte[32];

            using (FileStream fileCrypt = new(tbPathAES, FileMode.Open))
            {
                var taskOne = Task.Run(()=>fileCrypt.Read(salt, 0, salt.Length));
                
                RijndaelManaged AES = new(); //Create a new instance of the RijndaelManaged (Rijndael algorithm)

                var taskTwo= Task.Run(async()=>
                {
                    //define the RijndaelManaged object
                    AES.KeySize = 256;//aes 256 bit encryption 
                    AES.BlockSize = 128;//aes 128 bit encryption 
                    var key = new Rfc2898DeriveBytes(passwords, salt, 50000);
                    AES.Key = await Task.Run(() => key.GetBytes(AES.KeySize / 8));
                    AES.IV = await Task.Run(() => key.GetBytes(AES.BlockSize / 8));
                    AES.Padding = PaddingMode.PKCS7;
                    AES.Mode = CipherMode.CFB;
                });

                taskOne.Wait();
                taskTwo.Wait();

                //create streams used for encryption
                using (CryptoStream cryptoStream = new(fileCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (FileStream fileOut = new(tbPath, FileMode.Create))
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

        /// <summary>
        ///  encrypts multiple files in succession
        /// </summary>
        /// <param name="bytesEncrypted"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        private static byte[] AESEncrypt(byte[] bytesEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            byte[] salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            //create streams used for encryption
            using (MemoryStream memoryStream = new())
            {
                using(RijndaelManaged AES = new()) //Create a new instance of the RijndaelManaged (Rijndael algorithm)
                {
                    //define the RijndaelManaged object
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using(var cryptoStream = new CryptoStream(memoryStream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytesEncrypted, 0, bytesEncrypted.Length);
                        cryptoStream.Close();
                    }
                    encryptedBytes = memoryStream.ToArray();
                }
            }
            return encryptedBytes;
        }

        public static void DirectoryAESEncrypt(string file, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            byte[] bytesEncrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            //Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] Encrypted = AESEncrypt(bytesEncrypted, passwordBytes);

            string fileEncrypted = file;

            File.WriteAllBytes(fileEncrypted, Encrypted);

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }

        /// <summary>
        /// decrypt multiple files in succession
        /// </summary>
        /// <param name="bytesDecrypt"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        private static byte[] AESDecrypt (byte[] bytesDecrypt, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            byte[] salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            //create streams used for encryption
            using (MemoryStream memoryStream = new())
            {
                using (RijndaelManaged AES = new()) //Create a new instance of the RijndaelManaged (Rijndael algorithm)
                {
                    //define the RijndaelManaged object
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cryproStream = new CryptoStream(memoryStream, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryproStream.Write(bytesDecrypt, 0, bytesDecrypt.Length);
                        cryproStream.Close();
                    }
                    decryptedBytes = memoryStream.ToArray();
                }
            }
            return decryptedBytes;
        }

        public static void DirectoryAESDecrypt(string fileEncrypted, string password)
        {
            GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);

            byte[] bytesDecrypt = File.ReadAllBytes(fileEncrypted);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            //Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] Decrypt = AESDecrypt(bytesDecrypt, passwordBytes);

            string file = fileEncrypted;

            File.WriteAllBytes(file, Decrypt);

            ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            gch.Free();
        }
    }
}
