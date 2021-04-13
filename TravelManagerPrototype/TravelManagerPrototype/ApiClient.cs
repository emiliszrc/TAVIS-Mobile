using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using Xamarin.Essentials;
using Xamarin.Forms.Internals;

namespace TravelManagerPrototype
{
    public class ApiClient
    {
        public bool PostLocation(Xamarin.Essentials.Location location)
        {
            var client = new RestClient($"https://tavisapi.azurewebsites.net/api/Organisation");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");

            var response = client.Execute(request);

            return response.IsSuccessful;
        }

        public List<Trip> GetTripsByClientId(string clientId)
        {
            var client = new RestClient($"https://tavisapi.azurewebsites.net/api/Trips/ByClientId/{clientId}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var distance = JsonConvert.DeserializeObject<List<Trip>>(response.Content);
                return distance;
            }

            throw new Exception("Failed to fetch trips by client id");
        }

        public Client GetClient(string email)
        {
            var client = new RestClient($"https://tavisapi.azurewebsites.net/api/Clients/ByEmail/{email}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var distance = JsonConvert.DeserializeObject<Client>(response.Content);
                return distance;
            }

            throw new Exception("Failed to fetch trips by client id");
        }

        public Client PostNewPassword(PasswordRequest passwordRequest)
        {
            var client = new RestClient($"https://tavisapi.azurewebsites.net/api/Clients/{passwordRequest.Id}/SetPassword");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(passwordRequest);

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var distance = JsonConvert.DeserializeObject<Client>(response.Content);
                return distance;
            }

            throw new Exception("Failed to fetch trips by client id");
        }

        public List<Checkin> GetCheckins(string clientId, string tripId)
        {
            var client = new RestClient($"https://tavisapi.azurewebsites.net/api/Clients/{clientId}/GetCheckins/{tripId}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var distance = JsonConvert.DeserializeObject<List<Checkin>>(response.Content);
                return distance.ToList();
            }

            throw new Exception("Failed to fetch trips by client id");
        }

        public void PostCheckins(List<CheckinPost> checkinRequestItems, string clientId, string tripId)
        {
            var checkinRequests = new CheckinPosts {Items = checkinRequestItems};
            var client = new RestClient($"https://tavisapi.azurewebsites.net/api/Clients/{clientId}/PostCheckins/{tripId}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddJsonBody(checkinRequests);
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //var distance = JsonConvert.DeserializeObject<List<CheckinRequest>>(response.Content);
                return;
            }

            throw new Exception("Failed to fetch trips by client id");
        }
    }

    public class CheckinPosts
    {
        public List<CheckinPost> Items;
    }

    public class Checkin
    {
        public string Id { get; set; }
        public Visit Visit { get; set; }
        public Client Client { get; set; }
        public string Feedback { get; set; }
    }

    public class PasswordRequest
    {
        public string Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class Client
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string DefaultPassword { get; set; }
        public string Password { get; set; }

        public string Email { get; set; }
        public bool Notified { get; set; } = false;

        public List<Participation> Participations { get; set; }
    }

    public class Participation
    {
        public string Id { get; set; }

        public Client Client { get; set; }

        public Trip Trip { get; set; }
    }


    public class Trip
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public List<Visit> Visits { get; set; }
    }

    public class Visit
    {
        public string Id { get; set; }
        public string VisitationIndex { get; set; }
        public DateTime Arrival { get; set; }
        public DateTime Departure { get; set; }
        public Location Location { get; set; }
    }

    public class Location
    {
        public string Id { get; set; }

        public string LocationId { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Address { get; set; }

        public string Longtitude { get; set; }

        public string Latitude { get; set; }
    }

    public class ScheduleViewModel
    {
        public List<ScheduleDayModel> Days { get; set; }

        public static ScheduleViewModel From(Trip trip)
        {
            var groupByDays = trip.Visits.GroupBy(v => v.Arrival);

            var viewModel = new ScheduleViewModel
            {
                Days = groupByDays.Select(g => new ScheduleDayModel
                {
                    Day = g.Key,
                    Visits = trip.Visits.Where(v => v.Arrival == g.Key).ToList()
                }).ToList()
            };

            return viewModel;
        }
    }

    public class ScheduleDayModel
    {
        public DateTime Day { get; set; }
        public List<Visit> Visits { get; set; }
    }
}
