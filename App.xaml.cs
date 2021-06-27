using Encrypter.wpf;
using System;
using System.Linq;
using System.Windows;

namespace Encrypter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// responsible for closing the program
        /// </summary>
        /// <param name="isMainWindowOpen"></param>
        public static void IsWindowOpen(bool isMainWindowOpen)
        {
            if (isMainWindowOpen)
            {
                if (PasswordManager.GetWindowIsOpen())
                {
                    Application.Current.Shutdown();
                }
            }

            if (PasswordManager.GetWindowIsOpen())
            {
                if (isMainWindowOpen)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
