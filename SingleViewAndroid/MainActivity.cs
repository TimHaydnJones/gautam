using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using Android;
using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SingleViewAndroid
{
    [Activity(Label = "B9Eye Client", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        //these are global variables
        string deviceid, devicealias, lastknownip, proxy, servername, serverport, authtoken;
        int numreps;
        string directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            Button button1 = FindViewById<Button>(Resource.Id.button1);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            button1.Click += delegate
            {
                StartActivity(typeof(Setup));
            };

            button2.Click += delegate
            {
                StartActivity(typeof(Report));
            };

            button3.Click += delegate
            {
                Finish();
            };
        }
    }
}

