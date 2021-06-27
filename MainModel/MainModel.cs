using Encrypter.AccStructures;
using Encrypter.OperatingElements;
using Encrypter.Utilities;
using Encrypter.wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using static Encrypter.Account.Accounts;

namespace Encrypter.MainModels
{
    public class MainModel : ModelBase
    {
        private int chosenIndex;
        private bool contentPanelIsOn;
        private AccOperatingElementsModel AccSelected;

        public ObservableCollection<AccOperatingElementsModel> AccountsList { get; set; }

        public int ChosenIndex
        {
            get => chosenIndex;
            set { RaisePropertyChanged(ref chosenIndex, value); }
        }

        public bool ContentPanelIsOpen
        {
            get => contentPanelIsOn;
            set => RaisePropertyChanged(ref contentPanelIsOn, value);
        }

        public AccOperatingElementsModel AccountSelected
        {
            get => AccSelected;
            set => RaisePropertyChanged(ref AccSelected, value);
        }

        public ICommand ShowAddAccountWindowCommand { get; set; }
        public ICommand DeleteAccountCommand { get; set; }
        public ICommand SearchAccountCommand { get; set; }
        public ICommand CopyDetailsCommand { get; set; }

        //Some tools
        public bool AccountsArePresent => AccountsList.Count > 0;
        public bool AccountIsSelected { get => ChosenIndex > -1; }

        public NewAccWindow NewAccWndow { get; set; }
        public SearchResultsWindow SearchWindow { get; set; }
        public Action ShowContentPanelCallback { get; set; }

        public MainModel()
        {
            AccountsList = new ObservableCollection<AccOperatingElementsModel>();
            NewAccWndow = new NewAccWindow();
            SearchWindow = new SearchResultsWindow();
            SearchWindow.SearchContext.GetAccountItems = SetSearchAccountItems;
            SetupCommandBindings();
            NewAccWndow.AddAccountCallback = this.AddAccount;
            LoadAccounts();
        }

        private void SetupCommandBindings()
        {
            ShowAddAccountWindowCommand = new Command(ShowAddAccountWindow);
            DeleteAccountCommand = new Command(DeleteSelectedAccount);
            SearchAccountCommand = new Command(SearchAccount);
            CopyDetailsCommand = new CommandParam<int>(CopyDetailsToClipboard);
        }

        private void SetSearchAccountItems()
        {
            SearchWindow.SearchContext.TempItems.Clear();
            foreach (AccOperatingElementsModel accItem in AccountsList)
            {
                SearchWindow.AddRealAccount(accItem);
            }
        }

        public void SearchAccount()
        {
            ShowSearchWindow();
            SearchWindow.Search();
        }

        /*
         * Save and Load Accounts
         */

        public void SaveAccounts()
        {
            List<ModelAccount> tempAccounts = new();
            foreach (AccOperatingElementsModel item in AccountsList)
            {
                if (item.Account != null)
                    tempAccounts.Add(item.Account);
            }
            AccountSaver.SaveFiles(tempAccounts);
        }

        public void LoadAccounts()
        {
            ClearAccountsList();
            foreach (ModelAccount accounts in AccountLoader.LoadFiles())
            {
                AddAccount(accounts);
            }
        }

        /*
         * Add Accounts, Deleting Accounts
         */

        public void AddAccount() { AddAccount(NewAccWndow.AccountModel); }
        public void AddAccount(ModelAccount accountContent)
        {
            //e
            AccOperatingElementsModel account = CreateAccountItem(accountContent);

            AddAccount(account);
            NewAccWndow.ResetAccountContext();
        }

        public void AddAccount(AccOperatingElementsModel account)
        {
            AccountsList.Add(account);
        }

        public AccOperatingElementsModel CreateAccountItem(ModelAccount accountDetails)
        {
            AccOperatingElementsModel account = new()
            {
                Account = accountDetails
            };
            SetupAccountItemCallbacks(account);
            return account;
        }

        public void SetupAccountItemCallbacks(AccOperatingElementsModel item)
        {
            item.AutoShowContentCallback = ShowAccountContent;
        }

        public void DeleteSelectedAccount()
        {
            if (AccountIsSelected && AccountsArePresent) AccountsList.RemoveAt(ChosenIndex);
        }

        public void ShowAddAccountWindow()
        {
            NewAccWndow.Show();
            NewAccWndow.Focus();
        }

        public void ShowAccountContent(AccOperatingElementsModel account)
        {
            if (account?.Account != null)
            {
                if (!ContentPanelIsOpen) ShowContentPanel();
                AccountSelected = account;
            }
        }

        public void ShowSearchWindow()
        {
            SearchWindow.Show();
            SearchWindow.Focus();
        }

        public void ClearAccountsList()
        {
            ChosenIndex = 0;
            AccountsList.Clear();
        }

        public void CloseAllWindows()
        {
            NewAccWndow.Close();
            SearchWindow.Close();
        }

        public void ShowContentPanel()
        {
            if (!ContentPanelIsOpen)
            {
                ShowContentPanelCallback?.Invoke();
                ContentPanelIsOpen = true;
            }
        }

        public void CopyDetailsToClipboard(int detailsIndex)
        {
            switch (detailsIndex)
            {
                case 0: Clipboard.SetText(AccountSelected.Account.Email); break;
                case 1: Clipboard.SetText(AccountSelected.Account.Username); break;
                case 2: Clipboard.SetText(AccountSelected.Account.Password); break;
                case 3: Clipboard.SetText(AccountSelected.Account.WebsitePath); break;
            }
        }
    }
}
