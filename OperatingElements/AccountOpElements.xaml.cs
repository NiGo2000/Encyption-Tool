using System.Windows.Controls;
using System.Windows.Input;

namespace Encrypter.OperatingElements
{
    /// <summary>
    /// Interaction logic for AccountListItem.xaml
    /// </summary>
    public partial class AccountOpElements : UserControl
    {
        public AccOperatingElementsModel Account
        {
            get => this.DataContext as AccOperatingElementsModel;
            set => this.DataContext = value;
        }

        public AccountOpElements()
        {
            InitializeComponent();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Account.ShowContentsPanel();
        }
    }
}
