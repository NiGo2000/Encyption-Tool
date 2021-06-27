using Encrypter.AccStructures;
using Encrypter.OperatingElements;
using Encrypter.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Encrypter.SearchAlg
{
    public enum SearchTypes
    {
        WebsitePath,
        Email,
        Username,
        Password
    }

    public class SearchModel : ModelBase
    {
        private SearchTypes typeFilter;
        private string searchForCharacters;
        private int chosenIndex;

        public SearchTypes TypeFilter
        {
            get => typeFilter;
            set => RaisePropertyChanged(ref typeFilter, value);
        }

        public string SearchForCharacters
        {
            get => searchForCharacters;
            set => RaisePropertyChanged(ref searchForCharacters, value);
        }

        public int ChosenIndex
        {
            get => chosenIndex; set => RaisePropertyChanged(ref chosenIndex, value);
        }

        public List<AccOperatingElementsModel> TempItems { get; set; }
        public ObservableCollection<AccOperatingElementsModel> AccountsList { get; set; }

        public ICommand SearchCommand { get; private set; }

        public Action GetAccountItems { get; set; }

        public SearchModel()
        {
            AccountsList = new ObservableCollection<AccOperatingElementsModel>();
            TempItems = new List<AccOperatingElementsModel>();
            SearchCommand = new Command(Search);
            TypeFilter = SearchTypes.WebsitePath;
        }

        public void AddTempItem(AccOperatingElementsModel account)
        {
            TempItems.Add(account);
        }

        public void AddAccount(AccOperatingElementsModel account)
        {
            AccountsList.Add(account);
        }

        public void RemoveSelectedAccount()
        {
            AccountsList.RemoveAt(ChosenIndex);
        }

        public void ClearAccountsList()
        {
            ChosenIndex = 0;
            AccountsList.Clear();
        }

        public void Search()
        {
            ClearAccountsList();
            GetAccountItems?.Invoke();
            List<AccOperatingElementsModel> items = new();

            if(SearchForCharacters == null)
            {
                MessageBox.Show("Please enter something in the search box!");
            }
            else
            {
                _ = SearchForCharacters.Trim();
                string searchFor = SearchForCharacters.ToLower();
                foreach (AccOperatingElementsModel accountItm in TempItems)
                {
                    if (accountItm?.Account != null && !string.IsNullOrEmpty(searchFor))
                    {
                        ModelAccount account = accountItm.Account;
                        switch (TypeFilter)
                        {
                            case SearchTypes.WebsitePath:
                                if (account.WebsitePath.ToLower().Contains(searchFor))
                                    items.Add(accountItm);
                                break;
                            case SearchTypes.Email:
                                if (account.Email.ToLower().Contains(searchFor))
                                    items.Add(accountItm);
                                break;
                            case SearchTypes.Username:
                                if (account.Username.ToLower().Contains(searchFor))
                                    items.Add(accountItm);
                                break;
                            case SearchTypes.Password:
                                if (account.Password.ToLower().Contains(searchFor))
                                    items.Add(accountItm);
                                break;
                        }
                        AccountsList.Clear();
                        foreach (AccOperatingElementsModel accountItem in items)
                        {
                            AddAccount(accountItem);
                        }
                    }
                }
            }
        }
    }
}
