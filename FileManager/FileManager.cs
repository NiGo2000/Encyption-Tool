using Microsoft.Win32;
using System;
using System.IO;

namespace Encrypter
{
    class FileManager
    {
        /// <summary>
        /// stores the files for the different encryption methods and for the different settings
        /// </summary>
        /// <param name="caseFile"></param>
        /// <param name="Path"></param>
        /// <param name="result"></param>
        /// <param name="cbDeleteAESFile"></param>
        /// <param name="cbHideAESFile"></param>
        public static void SaveFile(int caseFile, string Path, byte[] result, bool cbDeleteAESFile, bool cbHideAESFile)
        {
            String fileExt = System.IO.Path.GetExtension(Path);
            SaveFileDialog sd = new SaveFileDialog();

            switch (caseFile)
            {
                case 1:
                    sd.Filter = "Files (*" + fileExt + ") | *" + fileExt;
                    if (sd.ShowDialog() == true)
                    {
                        File.WriteAllBytes(sd.FileName, result);
                    }
                    File.Move(Path + ".aes", sd.FileName + ".aes");
                    if (cbHideAESFile)
                    {
                        File.SetAttributes(sd.FileName + ".aes", FileAttributes.Hidden);
                    }
                    break;
                case 2:
                    if (cbDeleteAESFile)
                    {
                        File.Delete(System.IO.Path.GetFullPath(Path + ".aes"));
                    }
                    break;
                case 3:
                    
                    File.WriteAllBytes(Path, result);
                    break;
                default:
                    sd.Filter = "Files (*" + fileExt + ") | *" + fileExt;
                    if (sd.ShowDialog() == true)
                    {
                        File.WriteAllBytes(sd.FileName, result);
                    }
                    break;
            }
        }
    }
}
