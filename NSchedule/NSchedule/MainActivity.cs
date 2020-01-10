﻿using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using NSchedule.Helpers;

namespace NSchedule
{
    [Activity(Label = "Welcome to NSchedule", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Activity
    {
        Authenticator auth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // If already logged in, pass through to the schedule activity.
            // TODO
            auth = new Authenticator();

            // Setting layout and hiding actionbar.
            SetContentView(Resource.Layout.MainActivity);

            // Setting event for the login button.
            var loginbtn = FindViewById<Button>(Resource.Id.loginbutton);
            loginbtn.Click += (sender, e) =>
            {
                Loginbtn_Click();
            };

            var disclaimer = FindViewById<TextView>(Resource.Id.disclaimer);
            disclaimer.Click += Disclaimer_Click;
        }

        // nothing to see here, goto nothing_to_see;
        private int clicks = 0;
        private void Disclaimer_Click(object sender, EventArgs e)
        {
            clicks++;

            if(clicks > 10)
            {
                Toast.MakeText(Application.Context, "Onii-chan, yamero!", ToastLength.Long).Show();
            }
        }
        // label nothing_to_see;

        private async void Loginbtn_Click()
        {
            // Getting email and password from their respective fields.
            var email = FindViewById<EditText>(Resource.Id.emailfield).Text;
            var password = FindViewById<EditText>(Resource.Id.passwordfield).Text;

            // Tell the user we are trying to sign in now.
            Toast.MakeText(Application.Context, $"Trying to sign in to {email}...", ToastLength.Short).Show();

            // TODO Authenticate
            RestHelper r = new RestHelper();

            var success = await r.authenticate(email, password);

            r.TryGetTheDocentenHaha();
        }

        // Auto generated by xamarin. Seems important. Guess we'll keep it for now.
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

