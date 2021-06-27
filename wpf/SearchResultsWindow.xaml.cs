using Encrypter.OperatingElements;
using Encrypter.SearchAlg;
using System.Windows;
using System.Windows.Input;

namespace Encrypter.wpf
{
    /// <summary>
    /// Interaction logic for SearchResultsWindow.xaml
    /// </summary>
    public partial class SearchResultsWindow : Window
    {
        public SearchModel SearchContext
        {
            get => this.DataContext as SearchModel;
            set => this.DataContext = value;
        }
        public SearchResultsWindow()
        {
            InitializeComponent();
        }

        public void AddRealAccount(AccOperatingElementsModel account)
        {
            SearchContext.AddTempItem(account);
        }

        public void Search()
        {
            SearchContext.Search();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            SearchContext.ClearAccountsList();
            this.Hide();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
            if (e.Key == Key.Enter)
                this.Close();
        }
    }
}
