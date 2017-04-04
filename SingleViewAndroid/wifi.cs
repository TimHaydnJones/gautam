using System;
using Android.App;
using Android.OS;
using Android.Widget;


namespace SingleViewAndroid
{
    [Activity(Label = "Setup wifi")]
    public class wifi : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.wifi);
            Button button1 = FindViewById<Button>(Resource.Id.button1);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            var ssid = FindViewById<TextView>(Resource.Id.editText1);
            var psk = FindViewById<TextView>(Resource.Id.editText2);

            button1.Click += delegate
            //Save and return to main menu
            {
                bool ok;

                string details;
                details = "country=GB" + System.Environment.NewLine;
                details = details + "ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev" + System.Environment.NewLine;
                details = details + "update_config=1" + System.Environment.NewLine;
                details = details + System.Environment.NewLine + "network={";
                details = details + System.Environment.NewLine + "    ssid=\"" + ssid.Text + "\"";
                details = details + System.Environment.NewLine + "    psk=\"" + psk.Text + "\"";
                details = details + System.Environment.NewLine + "}";

                var sftp2 = new Renci.SshNet.SftpClient("192.168.42.123", 22, "pi", "gautampwd");

                using (sftp2)
                {
                    try
                    {
                        sftp2.Connect();
                        ok = true;
                    }
                    catch (Exception)
                    {
                        ok = false;
                    }
                    if (ok)
                    {
                        sftp2.WriteAllText("/etc/wpa_supplicant/wpa_supplicant.conf", details, System.Text.Encoding.GetEncoding("ASCII"));
                        sftp2.Disconnect();
                        RunOnUiThread(() => Toast.MakeText(this, "WIFI details saved.", ToastLength.Long).Show());
                    }
                    else
                    {
                        RunOnUiThread(() => Toast.MakeText(this, "ERROR: Check USB Tethering is active.", ToastLength.Long).Show());
                    }
                }

                ssid.Text = "";
                psk.Text = "";
            };

            button2.Click += delegate
            {
                Finish();
            };
        }
    }
}