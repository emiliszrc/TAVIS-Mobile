using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.UI.Dialogs;

namespace TravelManagerPrototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PasswordUpdate : ContentPage
    {
        private Client Client { get; set; }

        public PasswordUpdate(Client client)
        {
            InitializeComponent();
            Client = client;
        }

        public bool ValidatePassword(string password)
        {
            var strongRegex = new Regex("^(?=.*[a-z])(?=.*[0-9])(?=.{8,})");

            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            return strongRegex.IsMatch(password);
        }

        private async void TryChange(object sender, EventArgs e)
        {
            var apiclient = new ApiClient();

            if (!ValidatePassword(PasswordEntry.Text))
            {
                await MaterialDialog.Instance.AlertAsync(message: "Password has to contain a letter, number and be at least 8 characters long");
                return;
            }

            if (!PasswordEntry.Text.Equals(PasswordEntryRepetition.Text))
            {
                await MaterialDialog.Instance.AlertAsync(message: "Passwords are not the same");
                return;
            }

            var request = new PasswordRequest
            {
                Id = Client.Id,
                NewPassword = PasswordEntry.Text,
                OldPassword = string.Empty
            };

            var client = apiclient.PostNewPassword(request);
            await MaterialDialog.Instance.AlertAsync(message: client.Password);
            await Navigation.PushAsync(new Main(client));
        }
    }
}