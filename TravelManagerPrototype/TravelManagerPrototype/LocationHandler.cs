using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Location = Xamarin.Essentials.Location;

namespace TravelManagerPrototype
{
    public class LocationHandler
    {
        public async Task<Xamarin.Essentials.Location> GetLocation()
        {
            var location = await GetCurrentLocation();

            return location ?? await GetLastKnownLocation();
        }

        private async Task<Xamarin.Essentials.Location> GetLastKnownLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    Console.WriteLine(
                        $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    return location;
                }

                return new Xamarin.Essentials.Location();
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                return new Xamarin.Essentials.Location();
            }
            catch (FeatureNotEnabledException fneEx)
            {
                return new Xamarin.Essentials.Location();
            }
            catch (PermissionException pEx)
            {
                return new Xamarin.Essentials.Location();
            }
            catch (Exception ex)
            {
                return new Xamarin.Essentials.Location();
            }
        }

        private async Task<Xamarin.Essentials.Location> GetCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken cancelToken = source.Token;
                var location = await Geolocation.GetLocationAsync(request, cancelToken);

                if (location != null)
                {
                    Console.WriteLine(
                        $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    return location;
                }

                return new Xamarin.Essentials.Location();
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                return new Xamarin.Essentials.Location();
            }
            catch (FeatureNotEnabledException fneEx)
            {
                return new Xamarin.Essentials.Location();
            }
            catch (PermissionException pEx)
            {
                return new Xamarin.Essentials.Location();
            }
            catch (Exception ex)
            {
                return new Xamarin.Essentials.Location();
            }
        }

        //protected override void OnDisappearing()
        //{
        //    if (cts != null && !cts.IsCancellationRequested)
        //        cts.Cancel();
        //    base.OnDisappearing();
        //}

        public async Task NavigateTo(string lat, string lon)
        {
            try
            {
                var latitude = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);
                var longtitude = double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture);
                
                var location = new Xamarin.Essentials.Location(latitude, longtitude);
                var options = new MapLaunchOptions {NavigationMode = NavigationMode.Walking};

                DependencyService.Get<IMessage>().ShortAlert("Opening navigation");
                await Map.OpenAsync(location, options);
            }
            catch (Exception e)
            {
                DependencyService.Get<IMessage>().LongAlert(e.Message);
            }
        }
    }

}
