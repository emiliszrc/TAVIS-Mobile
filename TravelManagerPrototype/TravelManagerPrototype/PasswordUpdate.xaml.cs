using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private async void TryChange(object sender, EventArgs e)
        {
            var apiclient = new ApiClient();
            if (!OldPassword.Text.Equals(Client.DefaultPassword))
            {
                await MaterialDialog.Instance.AlertAsync(message: "Old password does not match");
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
                OldPassword = OldPassword.Text
            };

            var client = apiclient.PostNewPassword(request);
            await MaterialDialog.Instance.AlertAsync(message: client.Password);
            await Navigation.PushAsync(new Main(client));
        }
    }
}