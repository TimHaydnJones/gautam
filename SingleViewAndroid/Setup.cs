using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
//using Rebex.TerminalEmulation;
using System.IO;
//using Rebex.Net;

namespace SingleViewAndroid
{
    [Activity(Label = "Setup")]
    public class Setup : Activity
    {
        string directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
        string ssid, psk;

        public static string convertchar(char original, int offset)
        {
            string outcome;
            int tmp;
            tmp = Convert.ToInt32(original) + offset;
            char c = ((char)(tmp));
            outcome = "" + c;
            //outcome = char (tmp);
            //outcome = Java.Lang.Character.(tmp);
            return outcome;
        }


        public static string encodename(string sysname)
        {

            string encoded_sysid;
            //char[] mychars = Android.Text.TextUtils.GetChars(prompt2,0,prompt2.Length);
            char[] mychars = sysname.ToCharArray();
            char c1, c2, c3, c4, c5;
            c1 = mychars[6];
            c2 = mychars[7];
            c3 = mychars[8];
            c4 = mychars[9];
            c5 = mychars[10];
            string t1, t2, t3, t4, t5;
            t1 = "";
            t2 = "";
            t3 = "";
            t4 = "";
            t5 = "";
            t1 = convertchar(c1, 1);
            t2 = convertchar(c2, 2);
            t3 = convertchar(c3, 3);
            t4 = convertchar(c4, 4);
            t5 = convertchar(c5, 5);

            encoded_sysid = t1 + "^!7£-" + t2 + "~@:5-" + t3 + "`¬,x-" + t4 + "|<*^-" + t5 + "¬&4#/";
            return encoded_sysid;
        }



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Rebex.Licensing.Key = "==AcZUj/hMBZU54ZVv569fX92AmfrrCuoeQN/qxph2u6kc==";
            // Create your application here
            SetContentView(Resource.Layout.Setup);

            // Get our button from the layout resource,
            // and attach an event to it
            //TextView tb = FindViewById<Button>(Resource.Id.textView1);
            Button button1 = FindViewById<Button>(Resource.Id.button1);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            Button button4 = FindViewById<Button>(Resource.Id.button4);
            Button button5 = FindViewById<Button>(Resource.Id.button5);
            Button button6 = FindViewById<Button>(Resource.Id.button6);

            button1.Click += delegate
            //this is the BIND function
            {
                bool ok;
                //string prompt2 = "";
                string encoded_sysid;

                var methods = new List<Renci.SshNet.AuthenticationMethod>();
                methods.Add(new Renci.SshNet.PasswordAuthenticationMethod("pi", "gautampwd"));
                //Renci.SshNet.ConnectionInfo sshcon = new Renci.SshNet.ConnectionInfo("192.168.1.16", 22, "pi", methods.ToArray());
                Renci.SshNet.ConnectionInfo sshcon = new Renci.SshNet.ConnectionInfo("192.168.42.123", 22, "pi", methods.ToArray());
                sshcon.Timeout = TimeSpan.FromSeconds(20);
                // sftpClient2 = new SftpClient(sftpConn);


                //var connectionInfo = new Ren(sshcon);

                using (var ssh = new Renci.SshNet.SshClient(sshcon))
                {
                    try
                    {
                        ssh.Connect();
                        var command = ssh.CreateCommand("hostname");
                        var prompt = command.Execute();
                        encoded_sysid = encodename(prompt);

                        File.WriteAllText(directory + "/B9Eye-cfg.txt", encoded_sysid);
                        Toast.MakeText(this, "Bound to " + prompt, ToastLength.Long).Show();
                        ok = true;
                        button1.Text = "BOND [COMPLETED]";
                        //Console.Out.WriteLine(prompt);
                        ssh.Disconnect();

                    }

                    catch (Exception)
                    {
                        Toast.MakeText(this, "ERROR: Check USB Tethering is active.", ToastLength.Long).Show();
                    }

                }

            };

            button2.Click += delegate
            {
                //go to the wifi setup page
                StartActivity(typeof(wifi));
            };

            button5.Click += delegate
            {
                //reboot the rpi

                var methods = new List<Renci.SshNet.AuthenticationMethod>();
                methods.Add(new Renci.SshNet.PasswordAuthenticationMethod("pi", "gautampwd"));
                //Renci.SshNet.ConnectionInfo sshcon = new Renci.SshNet.ConnectionInfo("192.168.1.10", 22, "pi", methods.ToArray());
                Renci.SshNet.ConnectionInfo sshcon = new Renci.SshNet.ConnectionInfo("192.168.42.123", 22, "pi", methods.ToArray());
                sshcon.Timeout = TimeSpan.FromSeconds(20);
                // sftpClient2 = new SftpClient(sftpConn);


                //var connectionInfo = new Ren(sshcon);
                var ssh2 = new Renci.SshNet.SshClient(sshcon);
                bool connected = false;
                //using (var ssh = new Renci.SshNet.SshClient(sshcon))
                //{
                try
                {
                    ssh2.Connect();
                    connected = true;
                }
                catch (Exception)
                {
                    Toast.MakeText(this, "ERROR: Check USB Tethering is active.", ToastLength.Long).Show();
                    //throw;
                }
                if (connected)
                {
                    //var command = ssh2.CreateCommand("sudo reboot");
                    //command.Execute();
                    try
                    {
                        ssh2.RunCommand("sudo reboot");
                    }
                    catch (Exception)
                    {

                        Toast.MakeText(this, "REBOOTING", ToastLength.Long).Show();
                    }
                    //ssh2.RunCommand("sudo reboot");
                    //Toast.MakeText(this, "REBOOTING", ToastLength.Long).Show();

                }

                //}
                ssh2 = null;

            };

            button3.Click += delegate
            {
                //check that the rpi is connected to the wifi
                bool ok;



                var methods = new List<Renci.SshNet.AuthenticationMethod>();
                methods.Add(new Renci.SshNet.PasswordAuthenticationMethod("pi", "gautampwd"));
                //Renci.SshNet.ConnectionInfo sshcon = new Renci.SshNet.ConnectionInfo("192.168.1.10", 22, "pi", methods.ToArray());
                Renci.SshNet.ConnectionInfo sshcon = new Renci.SshNet.ConnectionInfo("192.168.42.123", 22, "pi", methods.ToArray());
                sshcon.Timeout = TimeSpan.FromSeconds(20);
                // sftpClient2 = new SftpClient(sftpConn);


                //var connectionInfo = new Ren(sshcon);
                var ssh2 = new Renci.SshNet.SshClient(sshcon);
                bool connected = false;
                //using (var ssh = new Renci.SshNet.SshClient(sshcon))
                //{
                try
                {
                    ssh2.Connect();
                    var command = ssh2.CreateCommand("sudo ifconfig wlan0");
                    var response = command.Execute();
                    int testpos = response.IndexOf("inet addr:192.168");
                    bool testpos2 = response.Contains("inet addr:192.168");
                    if (testpos2)
                    {
                        //button3.Text = "WIFI CONNECTED";
                        RunOnUiThread(() => Toast.MakeText(this, "WIFI is connected", ToastLength.Long).Show());
                    }
                    else
                    {
                        RunOnUiThread(() => Toast.MakeText(this, "Try SETUP WIFI again", ToastLength.Long).Show());
                        //RunOnUiThread(() => Toast.MakeText(this, message, ToastLength.Long).Show());
                    }

                    //connected = true;
                }
                catch (Exception)
                {
                    Toast.MakeText(this, "ERROR: Check USB Tethering is active.", ToastLength.Long).Show();
                    //throw;
                }
                ssh2.Disconnect();
                ssh2 = null;
            };

            button4.Click += delegate
            {
                //return to the main menu
                Finish();
                //StartActivity(typeof(MainActivity));
            };

            button6.Click += delegate
            {
                //go to the email setup page
                StartActivity(typeof(Email));
            };

        }
    }
}