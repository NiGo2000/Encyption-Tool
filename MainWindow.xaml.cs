using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace Encrypter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] abc;
        byte[,] table;

        PasswordEncrypter pwEncyrpter = new PasswordEncrypter();
        FileManager fileManager = new FileManager();

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens tab where you can select filesand shows the path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
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
        private async void bStart_Click(object sender, RoutedEventArgs e)
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
                        pwEncyrpter.FileASCIIEncrypt(fileContent, tbPassword.Password, keys, result, abc, table);

                        //Save result to new file with the same extention
                        fileManager.SaveFile(0, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
                    }
                    else
                    {
                        await pwEncyrpter.FileAESEncrypt(tbPath.Text, tbPassword.Password);

                        //Save result to new file with the same extention
                        fileManager.SaveFile(1, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
                    }
                }
                
                //Decrypt 
                if ((bool)rbDecrypt.IsChecked)
                {
                    if ((bool)rbASCII.IsChecked)
                    {
                        pwEncyrpter.FileASCIIDecrypt(fileContent, tbPassword.Password, keys, result, abc, table);

                        //Save result to new file with the same extention
                        fileManager.SaveFile(0, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
                    }
                    else
                    {
                        await pwEncyrpter.FileAESDecrypt(tbPath.Text + ".aes", tbPath.Text, tbPassword.Password);

                        //Save result to new file with the same extention
                        fileManager.SaveFile(2, tbPath.Text, result, (bool)cbDeleteAESFile.IsChecked, (bool)cbHideAESFile.IsChecked);
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
        private void bDelete_Click(object sender, RoutedEventArgs e)
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
        private void bHide_Click(object sender, RoutedEventArgs e)
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
        private void bVisible_Click(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// drop file -> returns the file path to the tbPath
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Drop(object sender, DragEventArgs e)
        {
           if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] folderPath = (string[])(e.Data.GetData("FileName", false));
                tbPath.Text = folderPath[0];
            }
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

        private void bPasswordManager_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
