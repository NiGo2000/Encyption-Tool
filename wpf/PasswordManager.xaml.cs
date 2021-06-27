using Encrypter.MainModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using Encrypter.FileEncrypter;
using System.Security;
using System.Net;

namespace Encrypter.wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PasswordManager : Window
    {
        private readonly string[] DirectoryPaths = { @"C:\EncryptTemp\", @"C:\EncryptTemp", "webPathName.txt", "email.txt", "usrName.txt", "pssWrd.txt" };
        private SecureString masterPassword;
        private static bool IsOpen;
        public MainModel ViewModel { get; set; }

        public PasswordManager()
        {
            InitializeComponent();
            IsOpen = false;
            ViewModel = new MainModel();
            this.DataContext = ViewModel;
            HideContentPanel();
        }

        public void AnimateContentPanelWidth(double oldWidth, double newWidth)
        {
            DoubleAnimation da = new(oldWidth, newWidth, TimeSpan.FromSeconds(0.2))
            {
                AccelerationRatio = 0,
                DecelerationRatio = 1,
            };
            AccountPanel.BeginAnimation(WidthProperty, da);
        }

        public void HideContentPanel()
        {
            AnimateContentPanelWidth(450, 0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.SaveAccounts();
            ViewModel.CloseAllWindows();
            SaveSettings();

            string[] DirectoryFiles;
            List<string> buffer = new();

            for (int i = 2; i <= 5; i++)
            {
                buffer.Add(DirectoryPaths[0] + DirectoryPaths[i]);
            }
            DirectoryFiles = buffer.ToArray();
            buffer.Clear();

            masterPassword = MainWindow.GetSecureString(masterPassword);


            for (int i = 0; i < DirectoryFiles.Length; i++)
                PasswordEncrypter.DirectoryAESEncrypt(DirectoryFiles[i], new NetworkCredential("", masterPassword).Password);

            IsOpen = true;

            bool isMainWindowOpen = MainWindow.GetWindowIsOpen();
            App.IsWindowOpen(isMainWindowOpen);
        }

       
        public void SaveSettings()
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }
            Properties.Settings.Default.Save();
        }

        public static bool GetWindowIsOpen()
        {
            return IsOpen;
        }
    }
}