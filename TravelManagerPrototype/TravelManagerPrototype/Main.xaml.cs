using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
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

            UpdateMainPageDetails();
        }

        private void UpdateMainPageDetails()
        {
            var currentTime = DateTime.UtcNow;
            Visit previousVisit = null;
            Visit nextVisit = null;
            Visit nextAccomodation = null;
            TimeSpan alreadyTravelingFor = TimeSpan.Zero;
            TimeSpan leftUntilNextStop = TimeSpan.Zero;

            foreach (var tripVisit in trip.Visits)
            {
                TimeSpan timeBetween = TimeSpan.Zero;
                TimeSpan timeTraveling = TimeSpan.Zero;

                if (trip.Visits.IndexOf(tripVisit) > 0)
                {
                    var previous = trip.Visits[trip.Visits.IndexOf(tripVisit) - 1];
                    var next = trip.Visits[trip.Visits.IndexOf(tripVisit)];

                    timeTraveling = DateTime.UtcNow - previous.Departure;
                    timeBetween = next.Arrival - previous.Departure;
                }

                var stringed = tripVisit.Departure.ToString();
                var stringedA = tripVisit.Arrival.ToString();
                var current = currentTime.ToString();
                if (tripVisit.Arrival >= currentTime && trip.Visits[trip.Visits.IndexOf(tripVisit) - 1].Departure <= currentTime)
                {
                    nextVisit = tripVisit;
                    previousVisit = trip.Visits[trip.Visits.IndexOf(nextVisit) - 1];
                    alreadyTravelingFor = timeTraveling;

                    if (timeBetween < TimeSpan.FromHours(3))
                    {
                        leftUntilNextStop = timeBetween - timeTraveling;
                    }
                    else
                    {
                        var stopsAt = new List<DateTime>();

                        var timeBtwn = timeBetween;

                        while (true)
                        {

                            if (timeBtwn > TimeSpan.FromHours(3))
                            {
                                timeBtwn = timeBtwn - TimeSpan.FromHours(3);
                                if (stopsAt.Any())
                                {
                                    stopsAt.Add(stopsAt.LastOrDefault() + TimeSpan.FromHours(3));
                                }
                                else
                                {
                                    stopsAt.Add(previousVisit.Departure + TimeSpan.FromHours(3));
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        foreach (var dateTime in stopsAt)
                        {
                            if (stopsAt.IndexOf(dateTime) > 0)
                            {
                                var prevStop = stopsAt[stopsAt.IndexOf(dateTime) - 1];

                                if (currentTime > prevStop && currentTime < dateTime)
                                {
                                    leftUntilNextStop = dateTime - currentTime;
                                    break;
                                }
                            }
                        }
                    }

                    break;
                }
            }

            foreach (var tripVisit in trip.Visits)
            {
                if (tripVisit.Arrival > currentTime && tripVisit.Location.Type == "lodging")
                {
                    nextAccomodation = tripVisit;
                    break;
                }
            }

            if (nextVisit != null)
            {
                upcomingVisitAddress.Text = nextVisit.Location.Address;
                upcomingVisitTitle.Text = nextVisit.Location.Title;
                upcomingVisitTimeLeft.Text = (nextVisit.Arrival - currentTime).Humanize();
            }
            else
            {
                upcomingVisitLabel.Text = "No visits left";
                upcomingVisitAddress.Text = "-";
                upcomingVisitTitle.Text = "-";
                upcomingVisitTimeLeft.Text = "-";
            }

            if (nextAccomodation != null)
            {
                upcomingHotelAddress.Text = nextAccomodation.Location.Address;
                upcomingHotelTitle.Text = nextAccomodation.Location.Title;
                upcomingHotelTimeLeft.Text = (nextAccomodation.Arrival - currentTime).Humanize();
            }
            else
            {
                upcomingHotelLabel.Text = "No hotels left";
                upcomingHotelAddress.Text = "-";
                upcomingHotelTitle.Text = "-";
                upcomingHotelTimeLeft.Text = "-";
            }

            if (leftUntilNextStop != TimeSpan.Zero)
            {
                upcomingStopTiming.Text = leftUntilNextStop.Humanize();
            }
            else
            {
                upcomingStopTiming.Text = "-";
            }

            if (previousVisit != null)
            {
                feedbackLabel.Text = $"You just visited {previousVisit.Location.Title}, let us know what you think!";
            }
            else
            {
                feedbackLabel.Text = "";
            }
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