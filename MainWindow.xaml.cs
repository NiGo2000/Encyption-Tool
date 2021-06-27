using System;
using System.Text;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using Encrypter.FileEncrypter;
using System.Security;
using Encrypter.wpf;
using System.Net;
using System.Collections.Generic;

namespace Encrypter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] abc;
        byte[,] table;
        string[] DirectoryFiles;

        private readonly string[] DirectoryPaths = { @"C:\EncryptTemp\", @"C:\EncryptTemp", "webPathName.txt", "email.txt", "usrName.txt", "pssWrd.txt" };
        private static SecureString securePassword;
        private static bool IsOpen;

        /*
         * System requirements
         */

        public MainWindow()
        {
            InitializeComponent();
            IsOpen = false;
        }

        /// <summary>
        /// drop file -> returns the file path to the tbPath
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (tabFile.IsSelected)
            {
                if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] folderPath = (string[])(e.Data.GetData("FileName", false));
                    tbPath.Text = folderPath[0];
                }
            }

            if (tabDirectory.IsSelected)
            {
                if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] folderPath = (string[])(e.Data.GetData("FileName", false));
                    DirectoryPath.Text = folderPath[0];
                }
            }

        }

        public static bool GetWindowIsOpen()
        {
            return IsOpen;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsOpen = true;
            App.IsWindowOpen(IsOpen);
        }

        /// <summary>
        /// Drag file to window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /*
         * File Encrypter tab
         */

        /// <summary>
        /// Opens tab where you can select filesand shows the path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog od = new();
            od.Multiselect = false;
            Nullable<bool> result = od.ShowDialog();
            if (result == true)
            {
                tbPath.Text = od.FileName;
            }

        }

        /// <summary>
        /// when the window is loaded, this should be executed and the settings loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rbEncrypt.IsChecked = true;
            //init abc and table
            abc = new byte[256];
            for (int i = 0; i < 256; i++)
                abc[i] = Convert.ToByte(i);

            table = new byte[256, 256];
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 256; j++)
                {
                    table[i, j] = abc[(i + j) % 256];
                }
        }

        /// <summary>
        /// Start button for the encryption methods and decryption methods
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            //Check input values
            if (!File.Exists(tbPath.Text))
            {
                MessageBox.Show("File does not exist");
                return;
            }

            if (String.IsNullOrEmpty(tbPassword.Password))
            {
                MessageBox.Show("Password empty. Please enter your Password");
                return;
            }

            //Get file content and key encrypt/decrypt
            try
            {
                byte[] fileContent = File.ReadAllBytes(tbPath.Text);
                byte[] passwordTmp = Encoding.ASCII.GetBytes(tbPassword.Password);
                byte[] keys = new byte[fileContent.Length];

                for (int i = 0; i < fileContent.Length; i++)
                    keys[i] = passwordTmp[i % passwordTmp.Length];

                //Encrypt
                byte[] result = new byte[fileContent.Length];

                if ((bool)rbEncrypt.IsChecked)
                {
                    if ((bool)rbASCII.IsChecked)
                    {
                        PasswordEncrypter.FileASCIIEncrypt(fileContent, tbPassword.Password, keys, result, abc, table);

                        //Save result to new file with the same extention
                        FileManager.SaveFile(0, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
                    }
                    else
                    {
                        await PasswordEncrypter.FileAESEncrypt(tbPath.Text, tbPassword.Password);

                        //Save result to new file with the same extention
                        FileManager.SaveFile(1, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
                    }
                }
                
                //Decrypt 
                if ((bool)rbDecrypt.IsChecked)
                {
                    if ((bool)rbASCII.IsChecked)
                    {
                        PasswordEncrypter.FileASCIIDecrypt(fileContent, tbPassword.Password, keys, result, abc, table);

                        //Save result to new file with the same extention
                        FileManager.SaveFile(0, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
                    }
                    else
                    {
                        await PasswordEncrypter.FileAESDecrypt(tbPath.Text + ".aes", tbPath.Text, tbPassword.Password);

                        //Save result to new file with the same extention
                        FileManager.SaveFile(2, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
                        MessageBox.Show("File " + tbPath.Text +" is encryptet");
                    }
                }
            }
            catch
            {
                MessageBox.Show("File is in use. Close other program is using this file");
                return;
            }
        }

        /// <summary>
        /// button to delete the files that are in the path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tbPath.Text))
            {
                MessageBox.Show("File does not exist");
            }
            else
            {
                File.Delete(System.IO.Path.GetFullPath(tbPath.Text));
                MessageBox.Show("File has been deleted");
            }
        }

        /// <summary>
        /// Button to hide files that are in the path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tbPath.Text))
            {
                MessageBox.Show("File does not exist");

            }
            else
            {
                File.SetAttributes(tbPath.Text, FileAttributes.Hidden);
                MessageBox.Show("The file is now hidden.");
            }
        }

        /// <summary>
        /// Button to make hidden files visible again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Visible_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tbPath.Text))
            {
                MessageBox.Show("File does not exist");

            }
            else
            {
                FileAttributes attributes = File.GetAttributes(tbPath.Text);
                attributes = RemoveAttribute(attributes, FileAttributes.Hidden);
                File.SetAttributes(tbPath.Text, attributes);
                MessageBox.Show("The file is no longer hidden.");
            }
        }

        /// <summary>
        /// removes the attributes from the file
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="attributesToRemove"></param>
        /// <returns></returns>
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }


        /*
         * Directory Encrypter tab
         */

        private void DirectoryStart_Click(object sender, RoutedEventArgs e)
        {
            //Check input values
            if (!Directory.Exists(DirectoryPath.Text))
            {
                MessageBox.Show("Directory does not exist");
                return;
            }

            if (String.IsNullOrEmpty(DirectoryPassword.Password))
            {
                MessageBox.Show("Password empty. Please enter your Password");
                return;
            }

            //Get Directory content and encrypt/decrypt
            try
            {
                DirectoryFiles = Directory.GetFiles(DirectoryPath.Text, "*", SearchOption.AllDirectories);

                if ((bool)rbDecrypt_Directory.IsChecked)
                {
                    for (int i = 0; i < DirectoryFiles.Length; i++)
                        PasswordEncrypter.DirectoryAESDecrypt(DirectoryFiles[i], DirectoryPassword.Password);
                    MessageBox.Show("Directory is decryoted.");
                }

                if ((bool)rbEncrypt_Directory.IsChecked)
                {
                    MessageBox.Show("hallo");
                    for (int i = 0; i < DirectoryFiles.Length; i++)
                        PasswordEncrypter.DirectoryAESEncrypt(DirectoryFiles[i], DirectoryPassword.Password);
                    MessageBox.Show("Directory is encrypted.");
                }
            }
            catch
            {
                MessageBox.Show("Directory is in use. Close other program is using this Directory");
                return;
            }
        }

        /*
         * Password Manager tab
         */

        private async void LogIn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(PasswordLogIn.Password))
            {
                MessageBox.Show("Password empty. Please enter your Password");
                return;
            }

            if (!Directory.Exists(DirectoryPaths[1]))
            {
                Directory.CreateDirectory(DirectoryPaths[1]);

                for (int i = 2; i <= 5; i++)
                {
                    if (!File.Exists(Path.Combine(DirectoryPaths[0], DirectoryPaths[i])))
                    {
                        await File.Create(Path.Combine(DirectoryPaths[0], DirectoryPaths[i])).DisposeAsync();
                    }
                }

                securePassword = new NetworkCredential("", PasswordLogIn.Password).SecurePassword;

                PasswordManager mw = new();
                mw.Show();
            }
            else
            {
                for (int i = 2; i <= 5; i++)
                {
                    if (!File.Exists(Path.Combine(DirectoryPaths[0], DirectoryPaths[i])))
                    {
                        await File.Create(Path.Combine(DirectoryPaths[0], DirectoryPaths[i])).DisposeAsync();
                    }
                }

                try
                {
                    string[] DirectoryFiles;
                    List<string> buffer = new();
                    
                    for (int i = 2; i <= 5; i++)
                    {
                        buffer.Add(DirectoryPaths[0] + DirectoryPaths[i]);
                    }
                    DirectoryFiles = buffer.ToArray();
                    buffer.Clear();

                    securePassword = new NetworkCredential("", PasswordLogIn.Password).SecurePassword;
                    
                    for (int i = 0; i < DirectoryFiles.Length; i++)
                        PasswordEncrypter.DirectoryAESDecrypt(DirectoryFiles[i], PasswordLogIn.Password);
                    
                    PasswordManager mw = new();
                    mw.Show();
                }
                catch
                {
                    MessageBox.Show("You have entered the wrong password.");
                    return;
                }
            }
        }

        public static SecureString GetSecureString(SecureString secureString)
        {
            secureString = securePassword;

            return secureString;
        }
    }
}
