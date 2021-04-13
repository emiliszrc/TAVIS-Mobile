using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.UI.Dialogs;

namespace TravelManagerPrototype
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Main : ContentPage
    {
        private Client Client;
        private Trip trip = new Trip();
        private ObservableCollection<CheckinRequest> checkinRequestItems = new ObservableCollection<CheckinRequest>();

        public Main(Client client)
        {
            InitializeComponent();
            Client = client;
            nameLabel.Text = $"Greetings {client.Name}";

            var apiclient = new ApiClient();
            trip = apiclient.GetTripsByClientId(Client.Id)
                .OrderByDescending(t=> t.Visits
                    .OrderByDescending(v=> v.Arrival)
                        .FirstOrDefault().Arrival)
                .FirstOrDefault();

            ScheduleList.ItemsSource = trip.Visits;

            var checkins = apiclient.GetCheckins(Client.Id, trip.Id);

            foreach (var tripVisit in trip.Visits)
            {
                var checkinItem = CheckinRequest.From(tripVisit);
                if (checkins.Any(c => c.Visit.Id == tripVisit.Id))
                {
                    checkinItem.CheckedIn = true;
                }

                checkinRequestItems.Add(checkinItem);
            }

            CheckinList.ItemsSource = checkinRequestItems;
        }

        private async void SendNavigation(object sender, ItemTappedEventArgs e)
        {
            var handler = new LocationHandler();
            var myListView = (ListView)sender;
            var myItem = (Visit)myListView.SelectedItem;

            await handler.NavigateTo(myItem.Location.Latitude, myItem.Location.Longtitude);
        }

        private void AddParticipation(object sender, ItemTappedEventArgs e)
        {
            var myListView = (ListView)sender;
            var myItem = (CheckinRequest)myListView.SelectedItem;
            var item = checkinRequestItems.FirstOrDefault(i => i.Visit.Id == myItem.Visit.Id);
            item.CheckedIn = !item.CheckedIn;

            CheckinList.ItemsSource = null;
            CheckinList.ItemsSource = checkinRequestItems;

        }

        private async void SaveCheckins(object sender, EventArgs e)
        {
            var apiclient = new ApiClient();
            var checkins = checkinRequestItems.Where(r => r.CheckedIn).Select(r=>CheckinPost.From(r, trip.Id, Client.Id)).ToList();
            apiclient.PostCheckins(checkins, Client.Id, trip.Id);
            await MaterialDialog.Instance.AlertAsync(message: "Updated your check-ins");
        }
    }

    public class CheckinPost
    {
        public string TripId { get; set; }
        public string VisitId { get; set; }
        public string ClientId { get; set; }

        public static CheckinPost From(CheckinRequest r, string tripId, string clientId)
        {
            return new CheckinPost
            {
                TripId = tripId,
                VisitId = r.Visit.Id,
                ClientId = clientId
            };
        }
    }
    public class CheckinRequest
    {
        public Visit Visit { get; set; }
        public Location Location { get; set; }
        public bool CheckedIn { get; set; } = false;

        public static CheckinRequest From(Visit v)
        {
            return new CheckinRequest
            {
                Visit = v,
                CheckedIn = false,
                Location = v.Location
            };
        }
    }
}