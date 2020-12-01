using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;

namespace NSchedule.Droid
{
    [Activity(Label = "NSchedule", Icon = "@drawable/cal", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(
        new string[] { Android.Content.Intent.ActionView },
        Categories = new string[] {Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable},
        DataScheme = "nsc")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        App app;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Window.SetSoftInputMode(SoftInput.AdjustResize);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            this.app = new App();

            if (Intent?.Data != null)
            {
                var code = Intent.Data.GetQueryParameter("code");
                int.TryParse(Intent.Data.GetQueryParameter("day"), out int day);
                int.TryParse(Intent.Data.GetQueryParameter("month"), out int month);
                int.TryParse(Intent.Data.GetQueryParameter("year"), out int year);

                if (!string.IsNullOrEmpty(code) && day > 0 && month > 0 && year > 0)
                {
                    this.app.SendToSchedule(code, month, day, year);
                }
            }

            LoadApplication(app);
            AndroidBug5497WorkaroundForXamarinAndroid.assistActivity(this, this.Window.WindowManager);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}