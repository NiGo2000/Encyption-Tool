using Encrypter.AccStructures;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Encrypter.wpf
{
    /// <summary>
    /// Interaction logic for NewAccountWindow.xaml
    /// </summary>
    public partial class NewAccWindow : Window
    {
        public ModelAccount AccountModel = new ModelAccount();
        public Action AddAccountCallback { get; set; }
        public NewAccWindow()
        {
            InitializeComponent();
            DataContext = AccountModel;
        }

        public void ResetAccountContext()
        {
            AccountModel = new ModelAccount();
            this.DataContext = AccountModel;
        }

        private void AddAccountCallbackFunc() { AddAccountCallback?.Invoke(); }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: this.Hide(); break;
                case Key.Enter: AddAccountCallbackFunc(); this.Hide(); break;
            }
        }

        private void PasteTextToBox(object sender, RoutedEventArgs e)
        {
            switch (int.Parse(((Button)e.Source).Uid))
            {
                case 1: a.Text = Clipboard.GetText(); break;
                case 2: b.Text = Clipboard.GetText(); break;
                case 3: c.Text = Clipboard.GetText(); break;
                case 4: d.Text = Clipboard.GetText(); break;
            }
        }

        private void ClearTextClick(object sender, RoutedEventArgs e)
        {
            switch (int.Parse(((Button)e.Source).Uid))
            {
                case 1: a.Text = ""; break;
                case 2: b.Text = ""; break;
                case 3: c.Text = ""; break;
                case 4: d.Text = ""; break;
            }
        }

        private void AddAccountClick(object sender, RoutedEventArgs e)
        {
            AddAccountCallbackFunc();
            this.Hide();
        }
    }
}
