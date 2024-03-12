using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BankApp
{
    public partial class MainPage : ContentPage
    {
        ObservableCollection<Account> accounts = new ObservableCollection<Account>();
        Account selectedSender;
        Account selectedReceiver;
        CancellationTokenSource cancellationTokenSource;
        public MainPage()
        {
            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            AccountHolder accountHolder1 = new AccountHolder { LastName = "Sanchez", FirstName = "Rick", CreditRating = 1000, RegistrationDate = DateTime.Now };
            AccountHolder accountHolder2 = new AccountHolder { LastName = "Smith", FirstName = "Morty", CreditRating = 100, RegistrationDate = DateTime.Now };
            Account account1 = new Account(accountHolder1) { Balance = 45678 };
            Account account2 = new Account(accountHolder2) { Balance = 1234 };
            accounts.Add(account1);
            accounts.Add(account2);
            senderPicker.ItemsSource = accounts;
            receiverPicker.ItemsSource = accounts;

            RefreshAccounts();
        }
        private async void sendButton_Clicked(object sender, EventArgs e)
        {
            if (selectedSender == null || selectedReceiver == null)
            {
                await DisplayAlert("Error", "Please select sender and receiver accounts.", "OK");
                return;
            }

            double amount;
            if (!double.TryParse(amountEntry.Text, out amount))
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Run(async () =>
                {
                    MoneyTransfer.Transfer(selectedSender, selectedReceiver, amount, cancellationTokenSource.Token);
                    await Task.Delay(10000);
                });
                RefreshAccounts();
                cancellationTokenSource.Cancel();
                await DisplayAlert("Success", "Money transfer completed.", "OK");
            }
            catch (OperationCanceledException)
            {
                await DisplayAlert("Cancelled", "Money transfer cancelled.", "OK");
                cancellationTokenSource.Cancel();
            }

        }


        void SenderPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedSender = senderPicker.SelectedItem as Account;
        }

        void ReceiverPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedReceiver = receiverPicker.SelectedItem as Account;
        }
        private Label CreateLabel(string text)
        {
            var label = new Label();
            label.FontSize = 20;
            label.Text = text;
            return label;

        }
        private Grid CreateAccount(Account account)
        {
            Grid grid = new Grid()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };
            var owner = CreateLabel($"{account.Owner.FirstName} {account.Owner.LastName}");
            Grid.SetColumn(owner, 0);
            grid.Children.Add(owner);

            var balance = CreateLabel($"Balance: ${account.Balance}");
            Grid.SetColumn(balance, 1);
            grid.Children.Add(balance);

            return grid;
        }
        void RefreshAccounts()
        {
            accountsStack.Children.Clear();
            foreach (var account in accounts)
            {
                accountsStack.Children.Add(CreateAccount(account));
            }
        }
    }
}
