using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.UI.Dialogs;

namespace TravelManagerPrototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void TryLogin(object sender, EventArgs e)
        {

            var apiclient = new ApiClient();

            var response = apiclient.GetClient(UsernameEntry.Text);

            if (!response.DefaultPassword.Equals(string.Empty))
            {
                if (PasswordEntry.Text.Equals(response.DefaultPassword))
                {
                    await MaterialDialog.Instance.AlertAsync(message: "Successfully logged in. Please create a password");
                    await Navigation.PushAsync(new PasswordUpdate(response));
                    return;
                }

                await MaterialDialog.Instance.AlertAsync(message: "Password is not correct");
                return;
            }

            var hashedPassword = ComputeSha256Hash(PasswordEntry.Text);

            if (hashedPassword.Equals(response.Password))
            {
                await MaterialDialog.Instance.AlertAsync(message: "Successfully logged in.");
                await Navigation.PushAsync(new Main(response));
            }

            await MaterialDialog.Instance.AlertAsync(message: "Password is not correct");
            return;
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}