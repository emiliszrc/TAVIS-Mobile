using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace TravelManagerPrototype
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
                
            await Navigation.PushAsync(new Login());

            //var apiclient = new ApiClient();

            //var trip = apiclient.GetTripsByClientId("2191f20f-aa85-4dda-82ef-e979ea5815fc").FirstOrDefault();

            //ScheduleList.ItemsSource = trip.Visits;
        }

        private string SendLocation(Xamarin.Essentials.Location location)
        {
            var apiclient = new ApiClient();

            var trips = apiclient.GetTripsByClientId("2191f20f-aa85-4dda-82ef-e979ea5815fc");

            return apiclient.PostLocation(location) ? "ok" : "not ok";
        }

        private async Task<Xamarin.Essentials.Location> HandleButtonClick()
        {
            var handler = new LocationHandler();
            var location = await handler.GetLocation();

            return location;
        }

        private async void SendNavigation(object sender, ItemTappedEventArgs e)
        {
            var handler = new LocationHandler();
            var myListView = (ListView)sender;
            var myItem = (Visit)myListView.SelectedItem;

            await handler.NavigateTo(myItem.Location.Latitude, myItem.Location.Longtitude);
        }
    }
}
