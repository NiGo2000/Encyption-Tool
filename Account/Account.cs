using Microsoft.VisualBasic;
using Encrypter.AccStructures;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Encrypter.Account
{
    public static class Accounts
    {
        public static string WebPathName = "webPathName.txt";
        public static string EmailName = "email.txt";
        public static string UsernameName = "usrName.txt";
        public static string PasswordName = "pssWrd.txt";
        public static string DirectoryPath = @"C:\EncryptTemp";
        public static string SelectedPath = @"C:\EncryptTemp\";


        /// <summary>
        /// Gives an account error if SelectedPath does not exist
        /// </summary>
        public static List<ModelAccount> AccountError
        {
            get
            {
                return new List<ModelAccount>()
                {
                    new ModelAccount()
                    {
                        WebsitePath = "Error loading accounts from the ",
                        Email = "Master Account Directory."
                    }
                };
            }
        }

        public static class AccountLoader
        {
            /// <summary>
            /// Make sure the account path does exists
            /// </summary>
            /// <returns></returns>
            public static List<ModelAccount> LoadFiles()
            {
                if (!Directory.Exists(DirectoryPath)) 
                {
                    if (!Directory.Exists(DirectoryPath)) 
                    {
                        if (Directory.Exists(SelectedPath))
                        {
                            return LoadFiles(SelectedPath);
                        }
                        else return AccountError;
                    }
                    else return LoadFiles(DirectoryPath);
                }
                else return LoadFiles(DirectoryPath);
            }

            /// <summary>
            /// loads the accounts and reads them into List<string>.
            /// </summary>
            /// <param name="directoryLocation"></param>
            /// <returns></returns>
            public static List<ModelAccount> LoadFiles(string directoryLocation)
            {
                bool dirExists = Directory.Exists(directoryLocation);
                int dirCount = Directory.GetFiles(directoryLocation).Length;
                
                if (!dirExists)
                {
                    MessageBox.Show(
                        $"{directoryLocation} has not yet been created." +
                        $"Path doesn't exist.");

                    return AccountError;
                }

                if (dirCount < 4)
                {
                    List<string> empty = new();
                    int ListRecoveryCount = 0;
                    void InternalRecreateList(int size)
                    {
                        empty = new List<string>();
                        for (int i = 0; i < size; i++)
                        {
                            empty.Add("[Restored because of lost file]");
                        }
                        ListRecoveryCount++;
                    }

                    int tempLineCount;
                    if (!File.Exists(Path.Combine(directoryLocation, WebPathName))) File.WriteAllLines(Path.Combine(directoryLocation, WebPathName), empty);
                    else { tempLineCount = File.ReadLines(Path.Combine(directoryLocation, WebPathName)).Count(); InternalRecreateList(tempLineCount); }

                    if (!File.Exists(Path.Combine(directoryLocation, EmailName))) File.WriteAllLines(Path.Combine(directoryLocation, EmailName), empty);
                    else { tempLineCount = File.ReadLines(Path.Combine(directoryLocation, EmailName)).Count(); InternalRecreateList(tempLineCount); }

                    if (!File.Exists(Path.Combine(directoryLocation, UsernameName))) File.WriteAllLines(Path.Combine(directoryLocation, UsernameName), empty);
                    else { tempLineCount = File.ReadLines(Path.Combine(directoryLocation, UsernameName)).Count(); InternalRecreateList(tempLineCount); }

                    if (!File.Exists(Path.Combine(directoryLocation, PasswordName))) File.WriteAllLines(Path.Combine(directoryLocation, PasswordName), empty);
                    else { tempLineCount = File.ReadLines(Path.Combine(directoryLocation, PasswordName)).Count(); InternalRecreateList(tempLineCount); }
                }

                int maxCount = 0;

                List<string> webpath = File.ReadAllLines(Path.Combine(directoryLocation, WebPathName)).ToList();
                List<string> emailss = File.ReadAllLines(Path.Combine(directoryLocation, EmailName)).ToList();
                List<string> usernam = File.ReadAllLines(Path.Combine(directoryLocation, UsernameName)).ToList();
                List<string> passwrd = File.ReadAllLines(Path.Combine(directoryLocation, PasswordName)).ToList();

                int accCount = webpath.Count;
                if (accCount < 1)
                {
                    if (emailss.Count > accCount) maxCount = emailss.Count;
                    accCount = emailss.Count;
                    if (usernam.Count > accCount) maxCount = usernam.Count;
                    accCount = usernam.Count;
                    if (passwrd.Count > accCount) maxCount = passwrd.Count;
                    _ = passwrd.Count;

                    //when logged into password manager, the interaction inputbox is called
                    string userAccCount = Interaction.InputBox(
                        $"The files are lost. A maximum of {maxCount} would be possible " +
                        $"accounts were detected in other files. How many accounts were there?", "Files have been lost.", maxCount.ToString());

                    if (int.TryParse(userAccCount, out int userAccOut))
                    {
                        accCount = userAccOut;
                    }
                    else
                    {
                        accCount = maxCount;
                    }
                }

                void VerifyListIsLarge(List<string> list, int large)
                {
                    if (list.Count < large)
                    {
                        int largeDifference = large - list.Count;
                        for (int i = 0; i < largeDifference; i++)
                        {
                            list.Add("[Lost File]");
                        }
                    }
                    else
                    {
                        int largeDifference = list.Count - large;
                        for (int i = 0; i < largeDifference; i++)
                        {
                            list.RemoveAt(list.Count - 1);
                        }
                    }
                }

                VerifyListIsLarge(webpath, accCount);
                VerifyListIsLarge(emailss, accCount);
                VerifyListIsLarge(usernam, accCount);
                VerifyListIsLarge(passwrd, accCount);

                List<ModelAccount> accounts = new();


                for (int i = 0; i < accCount; i++)
                {
                    ModelAccount modelAccount = new()
                    {
                        WebsitePath = webpath[i],
                        Email = emailss[i],
                        Username = usernam[i],
                        Password = passwrd[i]
                    };
                    accounts.Add(modelAccount);
                }
                return accounts;
            }
        }

        public static class AccountSaver
        {
            public static void SaveFiles(List<ModelAccount> accounts)
            {
                if (!Directory.Exists(DirectoryPath))
                {

                    if (Directory.Exists(SelectedPath))
                    {
                        Properties.Settings.Default.MainAccountPath = SelectedPath;
                        Properties.Settings.Default.Save();
                        SaveFiles(accounts, DirectoryPath);
                    }
                    else SaveFiles(accounts, DirectoryPath);

                }
                else
                    SaveFiles(accounts, DirectoryPath);
            }

            /// <summary>
            /// Saves the new files
            /// </summary>
            /// <param name="accounts"></param>
            /// <param name="directoryPath"></param>
            public static void SaveFiles(List<ModelAccount> accounts, string directoryPath)
            {
                List<string> NEWwebname = new();
                List<string> NEWemailss = new();
                List<string> NEWusernam = new();
                List<string> NEWpasswrd = new();

                for (int i = 0; i < accounts.Count; i++)
                {
                    NEWwebname.Add(accounts[i].WebsitePath);
                    NEWemailss.Add(accounts[i].Email);
                    NEWusernam.Add(accounts[i].Username);
                    NEWpasswrd.Add(accounts[i].Password);
                }

                File.WriteAllLines(Path.Combine(directoryPath, WebPathName), NEWwebname);
                File.WriteAllLines(Path.Combine(directoryPath, EmailName), NEWemailss);
                File.WriteAllLines(Path.Combine(directoryPath, UsernameName), NEWusernam);
                File.WriteAllLines(Path.Combine(directoryPath, PasswordName), NEWpasswrd);
            }
        }
    }
}
