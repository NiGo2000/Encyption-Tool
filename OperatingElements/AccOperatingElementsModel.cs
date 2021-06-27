using Encrypter.AccStructures;
using Encrypter.Utilities;
using System;
using System.Windows;
using System.Windows.Input;

namespace Encrypter.OperatingElements
{
    public class AccOperatingElementsModel : ModelBase
    {
        private ModelAccount account;

        public ModelAccount Account
        {
            get => account;
            set => RaisePropertyChanged(ref account, value);
        }

        public ICommand SetClipboardCommand { get; }

        public Action<AccOperatingElementsModel> AutoShowContentCallback { get; set; }

        public AccOperatingElementsModel()
        {
            SetClipboardCommand = new CommandParam<int>(SetClipboard);
        }

        public void SetClipboard(int accountInfoUid)
        {
            switch (accountInfoUid)
            {
                case 1: Clipboard.SetText(Account.Username); break;
                case 2: Clipboard.SetText(Account.Password); break;
                case 3: Clipboard.SetText(Account.Email); break;
                case 4: Clipboard.SetText(Account.WebsitePath); break;
            }
        }

        public void ShowContentsPanel()
        {
            AutoShowContentCallback?.Invoke(this);
        }
    }
}
