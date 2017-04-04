using System;
using Android.App;
using Android.OS;
using Android.Widget;
using System.IO;
using Android;

namespace SingleViewAndroid
{
    [Activity(Label = "Setup Email")]
    public class Email : Activity
    {
        string gender;
        Android.Widget.Toast mytoast;
        int numreps;

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            gender = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Email);
            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1); spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.gender_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
            Button button1 = FindViewById<Button>(Resource.Id.button1);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            var email1 = FindViewById<TextView>(Resource.Id.editText1);


            button1.Click += delegate
            {
                var persname = FindViewById<TextView>(Resource.Id.editText2);
                string ts2 = email1.Text;

                bool ok;
                numreps = 0;
                var sftp = new Renci.SshNet.SftpClient("192.168.42.123", 22, "pi", "gautampwd");
                MemoryStream s1 = new MemoryStream();
                using (sftp)
                {
                    try
                    {
                        sftp.Connect();
                        ok = true;

                    }
                    catch (Exception)
                    {
                        ok = false;
                        RunOnUiThread(() => Toast.MakeText(this, "ERROR: Check USB Tethering", ToastLength.Long).Show());
                    }
                    if (ok)
                    {
                        sftp.DownloadFile("/home/pi/DCCH/email-config.txt", s1);
                        string ts = s1.ToString();
                        string result = System.Text.Encoding.UTF8.GetString(s1.ToArray());
                        string[] lines = result.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                        int numlines = lines.Length;
                        string ts4 = lines[12].Trim();
                        bool noemailsyet;
                        if (ts4 == "]")
                        {
                            noemailsyet = true;
                        }
                        else
                        {
                            noemailsyet = false;
                        }
                        string[] emails = ts2.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                        int numemails = emails.Length;
                        string ts3 = "";
                        int count = 0;
                        do
                        {
                            ts3 = ts3 + "      " + emails[count];
                            if (count < (numemails - 1) || noemailsyet == false)
                            {
                                ts3 = ts3 + ",";
                            }

                            ts3 = ts3 + System.Environment.NewLine;
                            ++count;
                        }
                        while (count < numemails);

                        count = 0;
                        string newdata = "";
                        do
                        {
                            newdata = newdata + lines[count] + System.Environment.NewLine;
                            ++count;
                        }
                        while (count < 12);
                        newdata = newdata + ts3;
                        count = 12;
                        do
                        {
                            newdata = newdata + lines[count] + System.Environment.NewLine;
                            ++count;
                        }
                        while (count < (numlines));

                        sftp.WriteAllText("/home/pi/DCCH/email-config.txt", newdata, System.Text.Encoding.GetEncoding("ASCII"));
                        s1.Dispose();
                        s1 = new MemoryStream();
                        sftp.DownloadFile("/home/pi/DCCH/anomaly-config.txt", s1);
                        ts = s1.ToString();
                        ts2 = persname.Text;
                        ts3 = spinner.SelectedItem.ToString();
                        int temp = ts3.IndexOf("(");
                        ts3 = Android.Text.TextUtils.Substring(ts3, 0, temp);
                        ts3 = ts3.Trim();
                        ts3 = ts3.ToUpper();
                        result = "";
                        result = System.Text.Encoding.UTF8.GetString(s1.ToArray());
                        lines = result.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                        lines[6] = "  " + "\"name\"" + ":" + "\"" + ts2 + "\"";
                        lines[7] = "  " + "\"gender\"" + ":" + "\"" + ts3 + "\"";
                        newdata = "";
                        count = 0;
                        do
                        {
                            newdata = newdata + lines[count] + System.Environment.NewLine;
                            ++count;
                        }
                        while (count < 13);
                        sftp.WriteAllText("/home/pi/DCCH/anomaly-config.txt", newdata, System.Text.Encoding.GetEncoding("ASCII"));

                        RunOnUiThread(() => Toast.MakeText(this, "SAVED", ToastLength.Long).Show());

                        s1.Dispose();
                        sftp.Disconnect();
                    }
                }
            };

            button2.Click += delegate
            {
                //go back to the main menu
                Finish();
            };
        }
    }
}