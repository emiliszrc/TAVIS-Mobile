using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF;

namespace TravelManagerPrototype
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
            XF.Material.Forms.Material.Init(this);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
