using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net;
using System.Threading;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Collections.Generic;
using Android.Content;

namespace SingleViewAndroid
{
    [Activity(Label = "Report", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Icon = "@drawable/icon")]
    public class Report : Activity
    {

        string directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
        string b9eyename;
        bool emulated;
        Android.Widget.Toast mytoast;
        string pir2name = "Not Installed";
        string chartdata = "uninitialised";
        string deviceid, devicealias, lastknownip, proxy, servername, serverport, authtoken;
        int numreps, sp;
        decimal[] pir1averages;
        decimal[] pir2averages;
        decimal[] kettleaverages;
        decimal[] fridgeaverages;
        string pir1today, pir2today, fridgetoday, kettletoday;
        //string pir1today, pir1ave;


        public static string Right(string original, int numberCharacters)
        {
            return original.Substring(original.Length - numberCharacters);
        }

        public string SendRequest(WebRequest target, byte[] jsonDataBytes, string contentType, string method, string authtoken)
        {
            string res;
            WebHeaderCollection WebHeaderCollection;
            target.ContentType = contentType;
            WebHeaderCollection = target.Headers;
            if ((authtoken == ""))
            {
                // this is a REST call to openhab, no headers required
            }
            else
            {
                WebHeaderCollection.Add("apikey:WeavedDemoKey$2015");
                WebHeaderCollection.Add(("token:" + authtoken));
            }

            target.Method = method;
            target.ContentLength = jsonDataBytes.Length;
            System.IO.Stream dataStream = target.GetRequestStream();
            dataStream.Write(jsonDataBytes, 0, jsonDataBytes.Length);
            dataStream.Close();
            WebResponse response = target.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            res = reader.ReadToEnd();
            return res;
        }


        public void GetAuthToken()
        {
            int keypos;
            HttpWebRequest myHttpWebRequest;
            object WebHeaderCollection;
            string tempstr;

            TextView res = FindViewById<TextView>(Resource.Id.textView1);

            myHttpWebRequest = ((HttpWebRequest)(WebRequest.Create("https://api.weaved.com/v22/api/user/login/timhaydnjones@yahoo.co.uk/timsdemoweaved")));
            WebHeaderCollection = myHttpWebRequest.Headers;
            myHttpWebRequest.ContentType = "application/json";
            myHttpWebRequest.Accept = "*/*";
            myHttpWebRequest.Headers.Add("apikey:WeavedDemoKey$2015");
            myHttpWebRequest.Headers.Add("Accept-Encoding:gzip, deflate");
            WebResponse response = myHttpWebRequest.GetResponse();
            Stream dataStream = response.GetResponseStream();
            System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(dataStream, encode);
            string jsondata = readStream.ReadToEnd();

            jsondata = jsondata.Replace("\"", "");
            keypos = (jsondata.IndexOf("token") + 1);
            jsondata = Right(jsondata, jsondata.Length - keypos - 5);
            keypos = (jsondata.IndexOf(",") + 1);
            authtoken = jsondata.Substring(0, (keypos - 1));

            //tempstr = "Authorising........DONE" + System.Environment.NewLine + "Finding Server.....";
            //var ttt = Toast.MakeText(this, "FINDING SERVER", ToastLength.Long);

            RunOnUiThread(() => mytoast.SetText("FINDING SERVER"));
            RunOnUiThread(() => mytoast.SetGravity(GravityFlags.Center, 0, 0));
            RunOnUiThread(() => mytoast.Show());
            if (numreps == 0)
            {
                ThreadPool.QueueUserWorkItem(o => GetDetailsOfSSHService());
            }
            else
            {
                ThreadPool.QueueUserWorkItem(o => GetDetailsOfRESTService());
            }

        }

        public void GetDetailsOfRESTService()
        {
            TextView res = FindViewById<TextView>(Resource.Id.textView1);

            HttpWebRequest myHttpWebRequest;
            object WebHeaderCollection;
            bool OK;
            int keypos;
            //string tempstr;

            //STEP TWO - GET A LIST OF SERVICES FOR THIS ACCOUNT
            myHttpWebRequest = ((HttpWebRequest)(WebRequest.Create("https://api.weaved.com/v22/api/device/list/all")));
            WebHeaderCollection = null;
            WebHeaderCollection = myHttpWebRequest.Headers;
            myHttpWebRequest.ContentType = "application/json";
            myHttpWebRequest.Headers.Add("apikey:WeavedDemoKey$2015");
            myHttpWebRequest.Headers.Add("token:" + authtoken);
            WebResponse response = myHttpWebRequest.GetResponse();
            response = myHttpWebRequest.GetResponse();
            Stream dataStream = response.GetResponseStream();
            System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(dataStream, encode);
            string jsondata = readStream.ReadToEnd();
            jsondata = jsondata.Replace("\"", "");

            OK = false;
            while ((OK == false))
            {

                keypos = (jsondata.IndexOf("deviceaddress") + 1);
                jsondata = jsondata.Substring((jsondata.Length - (jsondata.Length
                                - (keypos + 13))));
                keypos = (jsondata.IndexOf(",") + 1);
                deviceid = jsondata.Substring(0, (keypos - 1));
                keypos = (jsondata.IndexOf("devicealias") + 1);
                jsondata = jsondata.Substring((jsondata.Length - (jsondata.Length
                                - (keypos + 11))));
                keypos = (jsondata.IndexOf(",") + 1);
                devicealias = jsondata.Substring(0, (keypos - 1));
                string compstr = b9eyename + "-REST";
                if (((devicealias == compstr)))

                {
                    OK = true;
                    keypos = (jsondata.IndexOf("devicelastip") + 1);
                    jsondata = jsondata.Substring((jsondata.Length - (jsondata.Length
                                    - (keypos + 12))));
                    keypos = (jsondata.IndexOf(",") + 1);
                    lastknownip = jsondata.Substring(0, (keypos - 1));
                }

            }
            //RunOnUiThread(() => res.Text = "Authorising.........DONE" + System.Environment.NewLine + "Finding Server....." + "DONE" + System.Environment.NewLine + "Connecting.........");
            ThreadPool.QueueUserWorkItem(o => Connect());
        }

        public void GetDetailsOfSSHService()
        {
            TextView res = FindViewById<TextView>(Resource.Id.textView1);

            HttpWebRequest myHttpWebRequest;
            object WebHeaderCollection;
            bool OK, abort;
            int keypos;
            //string tempstr;
            abort = false;
            //STEP TWO - GET A LIST OF SERVICES FOR THIS ACCOUNT
            myHttpWebRequest = ((HttpWebRequest)(WebRequest.Create("https://api.weaved.com/v22/api/device/list/all")));
            WebHeaderCollection = null;
            WebHeaderCollection = myHttpWebRequest.Headers;
            myHttpWebRequest.ContentType = "application/json";
            myHttpWebRequest.Headers.Add("apikey:WeavedDemoKey$2015");
            myHttpWebRequest.Headers.Add("token:" + authtoken);
            WebResponse response = myHttpWebRequest.GetResponse();
            response = myHttpWebRequest.GetResponse();
            Stream dataStream = response.GetResponseStream();
            System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(dataStream, encode);
            string jsondata = readStream.ReadToEnd();
            jsondata = jsondata.Replace("\"", "");

            OK = false;
            while ((OK == false) & (abort == false))
            {
                try
                {
                    keypos = (jsondata.IndexOf("deviceaddress") + 1);
                    jsondata = jsondata.Substring((jsondata.Length - (jsondata.Length
                                    - (keypos + 13))));
                    keypos = (jsondata.IndexOf(",") + 1);
                    deviceid = jsondata.Substring(0, (keypos - 1));
                    keypos = (jsondata.IndexOf("devicealias") + 1);
                    jsondata = jsondata.Substring((jsondata.Length - (jsondata.Length
                                    - (keypos + 11))));
                    keypos = (jsondata.IndexOf(",") + 1);
                    devicealias = jsondata.Substring(0, (keypos - 1));
                    string compstr;
                    compstr = b9eyename + "-SSH";

                    if (((devicealias == compstr)))

                    {
                        OK = true;
                        keypos = (jsondata.IndexOf("devicelastip") + 1);
                        jsondata = jsondata.Substring((jsondata.Length - (jsondata.Length
                                        - (keypos + 12))));
                        keypos = (jsondata.IndexOf(",") + 1);
                        lastknownip = jsondata.Substring(0, (keypos - 1));
                    }

                }
                catch (Exception)
                {
                    abort = true;
                    //throw;
                }

            }
            if (abort)
            {
                RunOnUiThread(() => mytoast.SetText("ERROR: Invalid Account"));
                RunOnUiThread(() => mytoast.SetGravity(GravityFlags.Center, 0, 0));
                RunOnUiThread(() => mytoast.Show());
                Finish();
            }
            else
            {
                RunOnUiThread(() => mytoast.SetText("CONNECTING"));
                RunOnUiThread(() => mytoast.SetGravity(GravityFlags.Center, 0, 0));
                RunOnUiThread(() => mytoast.Show());
                ThreadPool.QueueUserWorkItem(o => Connect());

            }
        }



        public void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }



        public static string Calc5dayTrend(string[] summary)
        {
            int tbyte;
            int avgpir1 = 0;
            int tc = 0;
            string ts5, debug;
            int tsum2 = 0;
            int num = summary.Length;
            int c1 = 0, c2 = 0, c3 = 0, c4 = 0, c5 = 0;
            do
            {
                ts5 = summary[tc];
                int tsum = 0;

                if (ts5 == "")
                {
                    //ignore it - no data collected
                    num = num - 1;
                }
                else
                {
                    if (ts5.IndexOf("/") > 0)
                    {
                        ts5 = Right(ts5, ts5.Length - 12);

                    }

                    string[] hrs = ts5.Split(',');
                    debug = "0";
                    int tc2 = 0;
                    do
                    {
                        tbyte = Convert.ToInt32(hrs[tc2]);
                        tsum = tsum + tbyte;
                        debug = debug + " (" + tc2 + ") + " + tbyte;
                        tc2 = tc2 + 1;
                    }
                    while (tc2 < 24);
                    //avgpir1 = tsum / 24;
                    if (tc == num - 1)
                    {
                        c1 = tsum;
                    }
                    else
                    {
                        if (tc == (num - 2))
                        {
                            c2 = tsum;
                        }
                        else
                        {
                            if (tc == (num - 3))
                            {
                                c3 = tsum;
                            }
                            else
                            {
                                if (tc == (num - 4))
                                {
                                    c4 = tsum;
                                }
                                else
                                {
                                    if (tc == (num - 5))
                                    {
                                        c5 = tsum;
                                    }
                                }
                            }
                        }
                    }
                }
                tc = tc + 1;
                tsum2 = tsum2 + tsum;
            }
            while (tc < num);

            string c1s, c2s, c3s, c4s, c5s;

            avgpir1 = tsum2 / (num - 1);
            if (c1 < (avgpir1 * 0.9))
            {
                c1s = "-";
            }
            else
            {
                if (c1 > (avgpir1 * 1.1))
                {
                    c1s = "+";
                }
                else
                {
                    c1s = "o";
                }
            }
            if (c2 < (avgpir1 * 0.9))
            {
                c2s = "-";
            }
            else
            {
                if (c2 > (avgpir1 * 1.1))
                {
                    c2s = "+";
                }
                else
                {
                    c2s = "o";
                }
            }
            if (c3 < (avgpir1 * 0.9))
            {
                c3s = "-";
            }
            else
            {
                if (c3 > (avgpir1 * 1.1))
                {
                    c3s = "+";
                }
                else
                {
                    c3s = "o";
                }
            }
            if (c4 < (avgpir1 * 0.9))
            {
                c4s = "-";
            }
            else
            {
                if (c4 > (avgpir1 * 1.1))
                {
                    c4s = "+";
                }
                else
                {
                    c4s = "o";
                }
            }
            if (c5 < (avgpir1 * 0.9))
            {
                c5s = "-";
            }
            else
            {
                if (c5 > (avgpir1 * 1.1))
                {
                    c5s = "+";
                }
                else
                {
                    c5s = "o";
                }
            }
            string trend = c5s + c4s + c3s + c2s + c1s;
            return trend;
        }

        public decimal[] calc_averages(string[] summarydata)
        {
            string averages;
            averages = "";
            int count = 0;
            int count2 = 0;
            int numvals = summarydata.Length;
            int numvals2 = 0;
            numvals2 = numvals;
            string[] hourvalstr;
            int[] hourval = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] hourtotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            decimal[] hourave = { 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m };
            string currline;
            do
            {
                currline = summarydata[count];
                if (currline == "")
                {
                    //ignore it
                    numvals2--;
                }
                else
                {
                    if (currline.IndexOf("/") > 0)
                    {
                        currline = Right(currline, currline.Length - 12);
                    }

                    hourvalstr = currline.Split(new string[] { "," }, StringSplitOptions.None);
                    count2 = 0;
                    do
                    {
                        hourval[count2] = Convert.ToInt32(hourvalstr[count2]);
                        hourtotals[count2] = hourtotals[count2] + hourval[count2];
                        count2++;
                    } while (count2 < 23);

                }
                count++;
            }
            while (count < numvals);
            count2 = 0;
            int temp, currtotal;
            string tempstr;
            decimal currval;
            averages = "";
            do
            {
                currtotal = hourtotals[count2];
                currval = Decimal.Divide(currtotal, numvals2);
                hourave[count2] = currval;
                count2++;
            } while (count2 < 23);

            return hourave;
        }


        public void GetEventsLog()
        {

            string prevday, prevmonth, prevyear;

            //prevmonth = "??";

            TextView res = FindViewById<TextView>(Resource.Id.textView1);
            MemoryStream s1 = new MemoryStream();
            MemoryStream s2 = new MemoryStream();
            MemoryStream s3 = new MemoryStream();

            RunOnUiThread(() => mytoast.SetText("PROCESSING"));
            RunOnUiThread(() => mytoast.SetGravity(GravityFlags.Center, 0, 0));
            RunOnUiThread(() => mytoast.Show());
            bool connected = false;
            sp = Convert.ToInt32(serverport);




            var sftp = new Renci.SshNet.SftpClient(servername, sp, "pi", "gautampwd");
            using (sftp)
            {
                sftp.Connect();
                int portno = Convert.ToInt32(serverport);
                try
                {
                    connected = true;
                }
                catch (Exception)
                {
                    RunOnUiThread(() => Toast.MakeText(this, "CONNECT FAIL", ToastLength.Long).Show());
                    connected = false;
                }

                //var directory2 = sftp.Dire
                //var smf_log_name = directory2.GetFiles().OrderByDescending(f => f.LastWriteTime).First();

                //string highestfilename;
                //highestfilename = "dennis";
                if (connected)
                {
                    string currfile;
                    int highestfilenum = 0;
                    
                    int currfilenum = 0;
                    string currfileno;

                    string prefix = "???";
                    string suffix = "???";
                    string filetype = "----";
                    var files = sftp.ListDirectory("/home/pi/openhab/logs");
                    //var files = sftp.ListDirectory("/home/pi/");
                    foreach (var file in files)
                    {
                        currfile = file.Name;
                        string currency = file.LastWriteTime.ToString();
                        //this does get the time and date - just need to identify and retain the latest

                        int len = currfile.Length;
                        if (len > 5)
                        {
                            suffix = Android.Text.TextUtils.Substring(currfile, len - 4, len);
                            filetype = Android.Text.TextUtils.Substring(currfile, 0, 5);

                        }
                        else
                        {
                            suffix = "????";
                        }

                        if ((suffix == ".zip") && (filetype == "event"))
                        {
                            currfileno = Android.Text.TextUtils.Substring(currfile, 12, 14);
                            prefix = Android.Text.TextUtils.Substring(currfile, 0, 12);
                            currfilenum = Convert.ToInt32(currfileno);
                            if (currfilenum > highestfilenum)
                            {
                                highestfilenum = currfilenum;
                            }

                        }
                        else
                        {
                            suffix = suffix;
                        }
                    }
                    string highestfilename = prefix + highestfilenum + ".log.zip";
                    string zipfile = "/home/pi/openhab/logs/" + highestfilename;
                    string result3;
                    if ((prefix == "???") && (highestfilenum == 0))
                    {
                        result3 = "";
                    }
                    else
                    {
                        sftp.DownloadFile(zipfile, s3);
                        FileStream fs = new FileStream(directory + "/" + highestfilename, FileMode.Create, FileAccess.Write);

                        //write the memory stream to the file stream
                        s3.WriteTo(fs);
                        fs.Close();

                        

                        ExtractZipFile(directory + "/" + highestfilename, "", directory);

                        using (FileStream fs2 = File.OpenRead(directory + "/" + prefix + highestfilenum + ".log"))
                        {
                            fs2.CopyTo(s1);
                        }
                        string ts4 = s1.ToString();
                        result3 = System.Text.Encoding.UTF8.GetString(s1.ToArray());
                        //delete the zip and unzipped copy from the local device

                        var FileToDelete = directory + "/" + prefix + highestfilenum + ".log.zip";
                        if (System.IO.File.Exists(FileToDelete) == true)
                        {
                            System.IO.File.Delete(FileToDelete);
                        }

                        FileToDelete = directory + "/" + prefix + highestfilenum + ".log";
                        if (System.IO.File.Exists(FileToDelete) == true)
                        {
                            System.IO.File.Delete(FileToDelete);
                        }

                    }
                    MemoryStream s4 = new MemoryStream();

                    sftp.DownloadFile("/home/pi/openhab/configurations/items/MotionSensor.items", s4);
                    //sftp.DownloadFile("/home/pi/openhab/configurations/MotionSensor.items", s4);
                    string pir1name = "thisllnevermatch";

                    string result2 = System.Text.Encoding.UTF8.GetString(s4.ToArray());
                    string[] lines2 = result2.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);

                    int numlines2;
                    numlines2 = lines2.Length;
                    //numlines2 = 2;


                    if (numlines2 > 3)
                    {
                        pir1name = lines2[0];
                        pir1name = Android.Text.TextUtils.Substring(pir1name, 7, 20);
                        int testpos = pir1name.IndexOf("\t");
                        pir1name = Android.Text.TextUtils.Substring(pir1name, 0, testpos);

                        //the fact that there are more than 3 lines doesn't mean that there are two sensors,
                        //there may be one multi-sensor providing temperature and (optionally) lux
                        //therefore, take a look at the sensor type
                        pir2name = lines2[2];
                        string sensortype;
                        sensortype = Android.Text.TextUtils.Substring(pir2name, 0, 6);
                        if (sensortype == "Switch")
                        {
                            pir2name = Android.Text.TextUtils.Substring(pir2name, 7, 20);
                            testpos = pir2name.IndexOf("\t");
                            pir2name = Android.Text.TextUtils.Substring(pir2name, 0, testpos);

                        }
                        else
                        {
                            //there is not a second sensor
                            sensortype = "ignored";
                            pir2name = "Not Installed";
                        }

                    }
                    else
                    {
                        pir1name = lines2[0];
                        pir1name = Android.Text.TextUtils.Substring(pir1name, 7, 20);
                        int testpos = pir1name.IndexOf("\t");
                        pir1name = Android.Text.TextUtils.Substring(pir1name, 0, testpos);
                    }

                    //******************************** EXTERNALISE AT SOME POINT *******************************************
                    string sumfile;
                    string[] pir1summary;
                    string temptrend1 = "";
                    string temptrend2 = "";
                    string temptrend3 = "";
                    string temptrend4 = "";

                    MemoryStream s5 = new MemoryStream();
                    sumfile = "/home/pi/DCCH/summary/summary-" + pir1name + ".txt";
                    string result4 = "";
                    try
                    {
                        sftp.DownloadFile(sumfile, s5);
                        result4 = System.Text.Encoding.UTF8.GetString(s5.ToArray());
                    }
                    catch (Exception)
                    {
                        result4 = "";
                    }

                    if (result4 == "")
                    {
                        temptrend1 = "";
                    }
                    else
                    {
                        pir1summary = result4.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                        int numpir1sum = pir1summary.Length;
                        pir1today = pir1summary[numpir1sum - 1];
                        pir1averages = calc_averages(pir1summary);
                        s5.SetLength(0);
                        temptrend1 = Calc5dayTrend(pir1summary);
                    }

                    if (pir2name == "Not Installed")
                    {
                        //no point
                    }
                    else
                    {
                        sumfile = "/home/pi/DCCH/summary/summary-" + pir2name + ".txt";

                        try
                        {
                            sftp.DownloadFile(sumfile, s5);
                            result4 = System.Text.Encoding.UTF8.GetString(s5.ToArray());
                        }
                        catch (Exception)
                        {
                            result4 = "";
                        }

                        if (result4 == "")
                        {
                            temptrend2 = "";
                        }
                        else
                        {
                            string[] pir2summary = result4.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                            int numpir2sum = pir2summary.Length;
                            pir2today = pir2summary[numpir2sum - 1];
                            pir2averages = calc_averages(pir2summary);
                            s5.SetLength(0);
                            temptrend2 = Calc5dayTrend(pir2summary);
                        }

                    }

                    sumfile = "/home/pi/DCCH/summary/summary-Kettle.txt";

                    result4 = "";
                    try
                    {
                        sftp.DownloadFile(sumfile, s5);
                        result4 = System.Text.Encoding.UTF8.GetString(s5.ToArray());
                    }
                    catch (Exception)
                    {
                        result4 = "";
                    }

                    if (result4 == "")
                    {
                        temptrend3 = "";
                    }
                    else
                    {
                        string[] kettlesummary = result4.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                        int numkettle = kettlesummary.Length;
                        kettletoday = kettlesummary[numkettle - 1];
                        if (kettletoday == "")
                        {

                        }
                        else
                        {
                            kettleaverages = calc_averages(kettlesummary);
                            s5.SetLength(0);
                            temptrend3 = Calc5dayTrend(kettlesummary);

                        }

                        kettleaverages = calc_averages(kettlesummary);
                        s5.SetLength(0);
                        temptrend3 = Calc5dayTrend(kettlesummary);
                    }



                    sumfile = "/home/pi/DCCH/summary/summary-Fridge.txt";

                    result4 = "";
                    try
                    {
                        sftp.DownloadFile(sumfile, s5);
                        result4 = System.Text.Encoding.UTF8.GetString(s5.ToArray());
                    }
                    catch (Exception)
                    {
                        result4 = "";
                    }

                    if (result4 == "")
                    {
                        temptrend4 = "";
                    }
                    else
                    {
                        string[] fridgesummary = result4.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                        int numfridge = fridgesummary.Length;
                        fridgetoday = fridgesummary[numfridge - 1];
                        fridgeaverages = calc_averages(fridgesummary);
                        s5.SetLength(0);
                        temptrend4 = Calc5dayTrend(fridgesummary);
                    }




                    //****************************************************************************************************

                    sftp.DownloadFile("/home/pi/openhab/logs/events.log", s2);
                    string ts = s2.ToString();
                    string result = System.Text.Encoding.UTF8.GetString(s2.ToArray());

                    string bigstr;
                    bigstr = result3 + System.Environment.NewLine + result;

                    string[] lines = bigstr.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                    int numlines = lines.Length;

                    string currdate = DateTime.Now.ToShortDateString();
                    string currday;
                    string currmonth;
                    string curryear;

                    var currdate2 = DateTime.Now.Date.ToString();
                    string ver = Android.OS.Build.VERSION.Sdk;

                    string[] Fields = currdate.Split(new string[] { "/" }, StringSplitOptions.None);


                    //I would have liked to put out an interim Toast message but this doesn't appear until this
                    //code terminates, so there wasn't much point
                    //RunOnUiThread(() => mytoast.SetText("PROCESSING"));
                    //RunOnUiThread(() => mytoast.SetGravity(GravityFlags.Center, 0, 0));
                    //RunOnUiThread(() => mytoast.Show());

                    emulated = true;
                    if (emulated)
                    {
                        //Emulator values
                        currday = Fields[1];
                        currmonth = Fields[0];
                        curryear = Fields[2].Trim();

                    }
                    else
                    {
                        //Samsung Galaxy S5 values
                        currday = Fields[0];
                        currmonth = Fields[1];
                        curryear = Fields[2].Trim();

                    }


                    if (currmonth.Length == 1)
                    {
                        currmonth = "0" + currmonth;
                        currmonth = currmonth.Trim();
                    }

                    if (currday.Length == 1)
                    {
                        currday = "0" + currday;
                        currday = currday.Trim();
                    }

                    var list = new string[] { "01", "02", "04", "06", "08", "09", "11", "01" };

                    if (list.Contains(currmonth))
                    {
                        //if we transition to the previous month, it will be a 31 day month

                        int t1 = Convert.ToInt32(currday);
                        if ((t1 - 1) == 0)
                        {
                            prevday = "31";
                            int t2 = Convert.ToInt32(currmonth);
                            int t3 = Convert.ToInt32(curryear);
                            if (currmonth == "01")
                            {
                                prevmonth = "12";
                                prevyear = (t3 - 1).ToString();
                            }
                            else
                            {
                                prevmonth = (t2 - 1).ToString();
                                prevyear = curryear;
                            }
                        }
                        else
                        {
                            //we're going to go back a day in the same month so this is easy
                            prevday = (t1 - 1).ToString();
                            if (prevday.Length == 1)
                            {
                                prevday = ("0" + prevday).Trim();
                            }
                            prevmonth = currmonth;
                            prevyear = curryear;

                        }
                    }
                    else
                    {
                        var list2 = new string[] { "10", "05", "07", "12" };
                        if (list2.Contains(currmonth))
                        {
                            //if we transition to the previous month, it will be a 30 day month
                            int t1 = Convert.ToInt32(currday);
                            int t2 = Convert.ToInt32(currmonth);
                            if ((t1 - 1) == 0)
                            {
                                prevday = "30";
                                prevmonth = (t2 - 1).ToString();
                            }
                            else
                            {
                                //we're going to go back a day in the same month so this is easy
                                prevday = (t1 - 1).ToString();
                                if (prevday.Length == 1)
                                {
                                    prevday = ("0" + prevday).Trim();
                                }
                                prevmonth = currmonth;
                            }
                            prevyear = curryear;
                        }
                        else
                        {
                            //we must be transitioning into the end of February
                            //is it a leap year
                            int t1 = Convert.ToInt32(currday);
                            if ((t1 - 1) == 0)
                            {
                                if (curryear == "2016" || curryear == "2020" || curryear == "2024")
                                {
                                    prevday = "29";
                                }
                                else
                                {
                                    prevday = "28";
                                }
                            }
                            else
                            {
                                //we're going to go back a day in the same month so this is easy
                                prevday = (t1 - 1).ToString();
                                if (prevday.Length == 1)
                                {
                                    prevday = ("0" + prevday).Trim();
                                }

                                prevmonth = currmonth;
                                //prevmonth = "february";

                                string tempstr = prevmonth;
                            }
                            prevyear = curryear;
                        }
                    }

                    string firstnode1 = "uninitialised";
                    string firstnode2 = "uninitialised";
                    string firstnode3 = "uninitialised";
                    string firstnode4 = "uninitialised";
                    string lastnode1 = "No activity in " + pir1name;
                    string lastnode2 = "uninitialised";
                    if (pir2name == "Not Installed")
                    {
                        //leave it alone
                    }
                    else
                    {
                        lastnode2 = "No activity in " + pir2name;

                    }

                    string lastnode3 = "Kettle not used yet";
                    string lastnode4 = "Fridge not used yet";
                    string prevlast1 = "No activity in " + pir1name;
                    string prevlast2 = "uninitialised";
                    if (pir2name == "Not Installed")
                    {
                        //leave it alone
                    }
                    else
                    {
                        prevlast2 = "No activity in " + pir2name;

                    }

                    string prevlast3 = "Kettle not used.";
                    string prevlast4 = "Fridge not used.";

                    string firstdate = lines[0];
                    if (firstdate == "")
                    {
                        firstdate = lines[1];
                    }

                    string ts2, ts3;
                    ts2 = Android.Text.TextUtils.Substring(firstdate, 8, 10);
                    ts3 = Android.Text.TextUtils.Substring(firstdate, 5, 7);

                    int t4 = Convert.ToInt32(ts2);
                    int t5 = Convert.ToInt32(prevday);
                    int t6 = Convert.ToInt32(ts3);
                    int t7 = Convert.ToInt32(currmonth);

                    //I think the following test has been overtaken by events.
                    //we now always have the contents of the previous zip file so we always have everything we need
                    //The only exception to this is when a user does an activity report on the first day the system is operating
                    //but even then, we can come in here and not find anything for yesterday.
                    //Therefore, I am commenting this out on 11-10-2016
                    //if (ts2 == prevday || t4 < t5 || t6 < t7)
                    //{
                    //we have everything we need in the current events log
                    //we don't need to access any zip archive of a previous log file
                    int lineno = 0;
                    int numnode1 = 0;
                    int numnode2 = 0;
                    int numnode3 = 0;
                    int numnode4 = 0;
                    prevmonth = currmonth;
                    do
                    {
                        string currline = lines[lineno];
                        string testdate = curryear + "-" + currmonth + "-" + currday;
                        if (prevmonth.Length == 1)
                        {
                            prevmonth = "0" + prevmonth;
                        }
                        if (prevday.Length == 1)
                        {
                            prevday = "0" + prevday;
                        }

                        string testdate2 = prevyear + "-" + prevmonth + "-" + prevday;

                        int testpos = currline.IndexOf(testdate);
                        int testpos2 = currline.IndexOf(testdate2);
                        int testpos3 = currline.IndexOf("Battery");
                        if (testpos == 0 || testpos2 == 0)
                        {
                            if (testpos3 == -1)
                            {
                                //we are interested in this - the date is today or yesterday
                                //and it doesn't involve a Battery event

                                if (currline.IndexOf(pir1name + " state") > 0)
                                {
                                    if (currline.IndexOf(testdate) == 0)
                                    {
                                        ++numnode1;
                                        if ((firstnode1 == "uninitialised") && (currline.IndexOf(" ON") > 0))
                                        {
                                            firstnode1 = currline;
                                            testpos = firstnode1.IndexOf(" ");
                                            firstnode1 = Android.Text.TextUtils.Substring(firstnode1, testpos, firstnode1.Length);
                                            firstnode1 = firstnode1.Trim();
                                            firstnode1 = Android.Text.TextUtils.Substring(firstnode1, 0, 5);
                                            firstnode1 = pir1name + " @ " + firstnode1;
                                            firstnode1 = firstnode1;

                                        }
                                        if (currline.IndexOf(" ON") > 0)
                                        {
                                            lastnode1 = currline;
                                            testpos = lastnode1.IndexOf(" ");
                                            lastnode1 = Android.Text.TextUtils.Substring(lastnode1, testpos, lastnode1.Length);
                                            lastnode1 = lastnode1.Trim();
                                            lastnode1 = Android.Text.TextUtils.Substring(lastnode1, 0, 5);
                                            lastnode1 = pir1name + " @ " + lastnode1;
                                            lastnode1 = lastnode1;

                                        }
                                    }
                                    else
                                    {
                                        if (currline.IndexOf(" ON") > 0)
                                        {
                                            prevlast1 = currline;
                                            testpos = prevlast1.IndexOf(" ");
                                            prevlast1 = Android.Text.TextUtils.Substring(prevlast1, testpos, prevlast1.Length);
                                            prevlast1 = prevlast1.Trim();
                                            prevlast1 = Android.Text.TextUtils.Substring(prevlast1, 0, 5);
                                            prevlast1 = pir1name + " @ " + prevlast1;
                                            prevlast1 = prevlast1;

                                        }

                                    }
                                }

                                if (currline.IndexOf(pir2name + " state") > 0)
                                {
                                    if (currline.IndexOf(testdate) == 0)
                                    {
                                        ++numnode2;
                                        if ((firstnode2 == "uninitialised") && (currline.IndexOf(" ON") > 0))
                                        {
                                            firstnode2 = currline;
                                            testpos = firstnode2.IndexOf(" ");
                                            firstnode2 = Android.Text.TextUtils.Substring(firstnode2, testpos, firstnode2.Length);
                                            firstnode2 = firstnode2.Trim();
                                            firstnode2 = Android.Text.TextUtils.Substring(firstnode2, 0, 5);
                                            firstnode2 = pir2name + " @ " + firstnode2;
                                            firstnode2 = firstnode2;

                                        }
                                        if (currline.IndexOf(" ON") > 0)
                                        {
                                            lastnode2 = currline;
                                            testpos = lastnode2.IndexOf(" ");
                                            lastnode2 = Android.Text.TextUtils.Substring(lastnode2, testpos, lastnode2.Length);
                                            lastnode2 = lastnode2.Trim();
                                            lastnode2 = Android.Text.TextUtils.Substring(lastnode2, 0, 5);
                                            lastnode2 = pir2name + " @ " + lastnode2;
                                            lastnode2 = lastnode2;

                                        }
                                    }
                                    else
                                    {
                                        if (currline.IndexOf(" ON") > 0)
                                        {
                                            prevlast2 = currline;
                                            testpos = prevlast2.IndexOf(" ");
                                            prevlast2 = Android.Text.TextUtils.Substring(prevlast2, testpos, prevlast2.Length);
                                            prevlast2 = prevlast2.Trim();
                                            prevlast2 = Android.Text.TextUtils.Substring(prevlast2, 0, 5);
                                            prevlast2 = pir2name + " @ " + prevlast2;
                                            prevlast2 = prevlast2;

                                        }

                                    }
                                }



                                if (currline.IndexOf("Kettle state") > 0)
                                {
                                    if (currline.IndexOf(testdate) == 0)
                                    {
                                        ++numnode3;
                                        if ((firstnode3 == "uninitialised") && (currline.IndexOf("Kettle state updated to 0") < 0))
                                        {
                                            firstnode3 = currline;
                                            testpos = firstnode3.IndexOf(" ");
                                            firstnode3 = Android.Text.TextUtils.Substring(firstnode3, testpos, firstnode3.Length);
                                            firstnode3 = firstnode3.Trim();
                                            firstnode3 = Android.Text.TextUtils.Substring(firstnode3, 0, 5);
                                            firstnode3 = "Kettle" + " @ " + firstnode3;
                                            firstnode3 = firstnode3;

                                        }
                                        if (currline.IndexOf("Kettle state updated to 0") < 0)
                                        {
                                            lastnode3 = currline;
                                            testpos = lastnode3.IndexOf(" ");
                                            lastnode3 = Android.Text.TextUtils.Substring(lastnode3, testpos, lastnode3.Length);
                                            lastnode3 = lastnode3.Trim();
                                            lastnode3 = Android.Text.TextUtils.Substring(lastnode3, 0, 5);
                                            lastnode3 = "Kettle" + " @ " + lastnode3;
                                            lastnode3 = lastnode3;

                                        }
                                    }
                                    else
                                    {
                                        if (currline.IndexOf("Kettle state updated to 0") < 0)
                                        {
                                            prevlast3 = currline;
                                            testpos = prevlast3.IndexOf(" ");
                                            prevlast3 = Android.Text.TextUtils.Substring(prevlast3, testpos, prevlast3.Length);
                                            prevlast3 = prevlast3.Trim();
                                            prevlast3 = Android.Text.TextUtils.Substring(prevlast3, 0, 5);
                                            prevlast3 = "Kettle" + " @ " + prevlast3;
                                            prevlast3 = prevlast3;

                                        }

                                    }

                                }


                                if (currline.IndexOf("Fridge state") > 0)
                                {
                                    if (currline.IndexOf(testdate) == 0)
                                    {
                                        ++numnode4;
                                        if ((firstnode4 == "uninitialised") && (currline.IndexOf(" ON") > 0))
                                        {
                                            firstnode4 = currline;
                                            testpos = firstnode4.IndexOf(" ");
                                            firstnode4 = Android.Text.TextUtils.Substring(firstnode4, testpos, firstnode4.Length);
                                            firstnode4 = firstnode4.Trim();
                                            firstnode4 = Android.Text.TextUtils.Substring(firstnode4, 0, 5);
                                            firstnode4 = "Fridge" + " @ " + firstnode4;
                                            firstnode4 = firstnode4;

                                        }
                                        if (currline.IndexOf(" ON") > 0)
                                        {
                                            lastnode4 = currline;
                                            testpos = lastnode4.IndexOf(" ");
                                            lastnode4 = Android.Text.TextUtils.Substring(lastnode4, testpos, lastnode4.Length);
                                            lastnode4 = lastnode4.Trim();
                                            lastnode4 = Android.Text.TextUtils.Substring(lastnode4, 0, 5);
                                            lastnode4 = "Fridge" + " @ " + lastnode4;
                                            lastnode4 = lastnode4;

                                        }
                                    }
                                    else
                                    {
                                        if (currline.IndexOf(" ON") > 0)
                                        {
                                            prevlast4 = currline;
                                            testpos = prevlast4.IndexOf(" ");
                                            prevlast4 = Android.Text.TextUtils.Substring(prevlast4, testpos, prevlast4.Length);
                                            prevlast4 = prevlast4.Trim();
                                            prevlast4 = Android.Text.TextUtils.Substring(prevlast4, 0, 5);
                                            prevlast4 = "Fridge" + " @ " + prevlast4;
                                            prevlast4 = prevlast4;

                                        }
                                    }
                                }
                            }
                        }
                        ++lineno;
                    }
                    while (lineno < numlines);
                    //}

                    string resultstr;

                    resultstr = "Last events yesterday: " + System.Environment.NewLine;

                    if (temptrend1 == "")
                    {
                        resultstr = resultstr + "      " + prevlast1 + System.Environment.NewLine;
                    }
                    else
                    {
                        resultstr = resultstr + "      " + prevlast1 + " (" + temptrend1 + ")" + System.Environment.NewLine;

                    }
                    if (prevlast2 == "uninitialised")
                    {
                        //not interested - this is just to test the spacing of the display
                    }
                    else
                    {
                        if (temptrend2 == "")
                        {
                            resultstr = resultstr + "      " + prevlast2 + System.Environment.NewLine;
                        }
                        else
                        {
                            resultstr = resultstr + "      " + prevlast2 + " (" + temptrend2 + ")" + System.Environment.NewLine;
                        }

                    }

                    if (temptrend3 == "")
                    {
                        resultstr = resultstr + "      " + prevlast3 + System.Environment.NewLine;
                    }
                    else
                    {
                        resultstr = resultstr + "      " + prevlast3 + " (" + temptrend3 + ")" + System.Environment.NewLine;
                    }

                    if (temptrend4 == "")
                    {
                        resultstr = resultstr + "      " + prevlast4 + System.Environment.NewLine + System.Environment.NewLine;
                    }
                    else
                    {
                        resultstr = resultstr + "      " + prevlast4 + " (" + temptrend4 + ")" + System.Environment.NewLine + System.Environment.NewLine;
                    }

                    //'resultstr = resultstr & lastnode1b & System.Environment.NewLine
                    resultstr = resultstr + "First events today: " + System.Environment.NewLine;
                    if (firstnode1 == "uninitialised")
                    {
                        firstnode1 = pir1name + " not triggered yet";
                        //firstnode1 = "Kitchen" + " not triggered yet";
                    }

                    resultstr = resultstr + "      " + firstnode1 + System.Environment.NewLine;

                    if (firstnode2 == "uninitialised")
                    {
                        firstnode2 = pir2name + " not triggered yet";
                    }
                    if (pir2name == "Not Installed")
                    {
                        //nothing to write
                    }
                    else
                    {
                        resultstr = resultstr + "      " + firstnode2 + System.Environment.NewLine;
                    }

                    if (firstnode3 == "uninitialised")
                    {
                        firstnode3 = "Kettle not used yet";
                    }

                    resultstr = resultstr + "      " + firstnode3 + System.Environment.NewLine;
                    if (firstnode4 == "uninitialised")
                    {
                        firstnode4 = "Fridge not used yet";
                    }
                    resultstr = resultstr + "      " + firstnode4 + System.Environment.NewLine + System.Environment.NewLine;

                    resultstr = resultstr + "Most recent events today: " + System.Environment.NewLine;
                    resultstr = resultstr + "      " + lastnode1 + System.Environment.NewLine;
                    if (pir2name == "Not Installed")
                    {
                        //nothing to write
                    }
                    else
                    {
                        resultstr = resultstr + "      " + lastnode2 + System.Environment.NewLine;
                    }

                    resultstr = resultstr + "      " + lastnode3 + System.Environment.NewLine;
                    resultstr = resultstr + "      " + lastnode4 + System.Environment.NewLine + System.Environment.NewLine;

                    resultstr = resultstr + System.Environment.NewLine + "(" + b9eyename + ")";
                    RunOnUiThread(() => res.Text = resultstr);
                    sftp.Disconnect();

                    if ((pir1today == null) && (pir2today == null) && (kettletoday == null) && (fridgetoday == null))
                    {
                        //nothing to chart, so don't bother offering the option
                        string tim = "whatever";
                    }
                    else
                    {

                        Button button5 = FindViewById<Button>(Resource.Id.button5);
                        RunOnUiThread(() => button5.Visibility = ViewStates.Visible);

                    }
                    //string ts99 = resultstr;

                }
                else
                {
                    RunOnUiThread(() => res.Text = "Sorry, we were not able to connect to your BenignEye system. " + System.Environment.NewLine + "Please try again later and, should the fault persist, send an email to reporting@benigneye.com.");
                }

            }

        }

        public void Connect()
        {
            HttpWebRequest myHttpWebRequest;
            object WebHeaderCollection;
            string postdata, jsondata;
            int keypos;
            string tempstr;
            bool connectok;

            proxy = "uninitialised";

            TextView res = FindViewById<TextView>(Resource.Id.textView1);

            //STEP THREE - GET A PROXY SERVER AND PORT FOR THE REST SERVICE

            postdata = "{" + char.ConvertFromUtf32(34) + "deviceaddress" + char.ConvertFromUtf32(34) + ":" + char.ConvertFromUtf32(34) + deviceid + char.ConvertFromUtf32(34)
                + "," + char.ConvertFromUtf32(34) + "hostip" + char.ConvertFromUtf32(34) + ":" + char.ConvertFromUtf32(34) + lastknownip + char.ConvertFromUtf32(34)
                + "," + char.ConvertFromUtf32(34) + "wait" + char.ConvertFromUtf32(34) + ":" + char.ConvertFromUtf32(34) + "true" + char.ConvertFromUtf32(34) + "}";

            //------------------------------------------------------------------------------------
            //System.Uri target = new System.Uri("https://api.weaved.com/v22/api/device/connect");
            WebRequest request = WebRequest.Create("https://api.weaved.com/v22/api/device/connect");
            request.ContentType = "application/json";
            byte[] data = Encoding.UTF8.GetBytes(postdata);
            try
            {
                jsondata = SendRequest(request, data, "application/json", "POST", authtoken);
                connectok = true;
            }
            catch (System.Exception ex)
            {
                connectok = false;
                jsondata = "";
            }
            if (connectok)
            {
                tempstr = jsondata;
                keypos = (jsondata.IndexOf("proxy") + 1);
                jsondata = jsondata.Substring((jsondata.Length - (jsondata.Length
                                - (keypos + 5))));
                keypos = (jsondata.IndexOf(",") + 1);
                proxy = jsondata.Substring(0, (keypos - 1));
                proxy = proxy.Substring((proxy.Length - (proxy.Length - 11)));
                proxy = proxy.Substring(0, (proxy.Length - 1));

                servername = proxy.Substring(0, ((proxy.IndexOf(":") + 1) - 1));
                keypos = (proxy.IndexOf(":") + 1);
                serverport = proxy.Substring((proxy.Length - 5));
                //RunOnUiThread(() => res.Text = "Authorising........DONE" + System.Environment.NewLine + "Finding Server....." + "DONE" + System.Environment.NewLine + servername + ":" + serverport + System.Environment.NewLine + "Connecting.....DONE!" + System.Environment.NewLine + "Requesting.....");

                if (numreps > 0)
                {
                    ThreadPool.QueueUserWorkItem(o => SendREPORTrequest());
                }
                else
                {
                    //RunOnUiThread(() => Toast.MakeText(this, "FETCHING EVENTS", ToastLength.Long).Show());

                    ThreadPool.QueueUserWorkItem(o => GetEventsLog());
                }


            }
            else
            {
                RunOnUiThread(() => res.Text = "Authorising..........DONE" + System.Environment.NewLine + "Finding Server....." + "DONE" + System.Environment.NewLine + "Connecting..........FAILED!" + System.Environment.NewLine + System.Environment.NewLine + "UNSUCCESSFUL: Please try again later and, should the issue persist, email the details to reporting@benigneye.com");
            }
        }

        public void SendREPORTrequest()
        {
            string postdata, jsondata, tempstr;
            bool RESTok;
            TextView res = FindViewById<TextView>(Resource.Id.textView1);

            postdata = "REPORT " + numreps;
            WebRequest request = WebRequest.Create("http://" + proxy + "/rest/items/Diags");
            byte[] data2 = System.Text.UTF8Encoding.UTF8.GetBytes(postdata);
            try
            {
                jsondata = SendRequest(request, data2, "text/plain", "POST", "");
                RESTok = true;
            }
            catch (System.Exception ex)
            {
                RESTok = false;

            }
            if (RESTok)
            {
                tempstr = "Authorising..........DONE" + System.Environment.NewLine + "Finding Server......DONE" + System.Environment.NewLine + servername + ":" + serverport + System.Environment.NewLine + "Connecting..........DONE!" + System.Environment.NewLine + "Requesting..........DONE!" + System.Environment.NewLine + System.Environment.NewLine + "SUCCESS: You will receive the Activity Report by email momentarily.";
                RunOnUiThread(() => res.Text = tempstr);
            }
            else
            {
                tempstr = "Authorising..........DONE" + System.Environment.NewLine + "Finding Server......DONE" + System.Environment.NewLine + servername + ":" + serverport + System.Environment.NewLine + "Connecting..........DONE!" + System.Environment.NewLine + "Requesting..........FAILED!" + System.Environment.NewLine + System.Environment.NewLine + "UNSUCCESSFUL: Please try again later and, should the issue persist, email the details to reporting@benigneye.com";
                RunOnUiThread(() => res.Text = tempstr);
            }
            //return;
        }

        public static string convertchar2(char original, int offset)
        {
            string outcome;
            int tmp;
            tmp = Convert.ToInt32(original) - offset;
            char c = ((char)(tmp));
            outcome = "" + c;
            return outcome;
        }

        public static string unencodename(string encodedname)
        {

            string sysname;
            //char[] mychars = Android.Text.TextUtils.GetChars(prompt2,0,prompt2.Length);
            char[] mychars = encodedname.ToCharArray();
            char c1, c2, c3, c4, c5;
            c1 = mychars[0];
            c2 = mychars[6];
            c3 = mychars[12];
            c4 = mychars[18];
            c5 = mychars[24];
            string t1, t2, t3, t4, t5;
            t1 = "";
            t2 = "";
            t3 = "";
            t4 = "";
            t5 = "";
            t1 = convertchar2(c1, 1);
            t2 = convertchar2(c2, 2);
            t3 = convertchar2(c3, 3);
            t4 = convertchar2(c4, 4);
            t5 = convertchar2(c5, 5);

            sysname = "B9Eye-" + t1 + t2 + t3 + t4 + t5;
            return sysname;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Report);
            Button button5 = FindViewById<Button>(Resource.Id.button5);
            button5.Visibility = ViewStates.Invisible;

            //check which B9Eye system has been bonded with this mobile device
            string filename = Path.Combine(directory, "B9Eye-cfg.txt");
            try
            {
                using (var streamReader = new StreamReader(filename))
                {
                    string content = streamReader.ReadToEnd();
                    b9eyename = content.Trim();
                    if (b9eyename.Length == 11)
                    {
                        //old format - upgrade it
                        string encoded_sysid = Setup.encodename(b9eyename);

                        File.WriteAllText(directory + "/B9Eye-cfg.txt", encoded_sysid);
                    }
                    else
                    {
                        string temp = unencodename(b9eyename);
                        b9eyename = temp;
                    }

                    emulated = false;
                }

            }
            catch (Exception)
            {
                //this is the emulated route
                b9eyename = "1^!7£-2~@:5-3`¬,x-4|<*^-7¬&4#/";
                string temp = unencodename(b9eyename);
                b9eyename = temp;
                emulated = true;
                //throw;
            }

            b9eyename = "B9Eye-00002";

            // Get our button from the layout resource,
            // and attach an event to it
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            Button button4 = FindViewById<Button>(Resource.Id.button4);
            TextView res = FindViewById<TextView>(Resource.Id.textView1);
            res.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();


            button3.Click += delegate {
                Finish();
            };


            button4.Click += delegate
            {
                numreps = 0;
                mytoast = Toast.MakeText(this, "AUTHORISING", ToastLength.Long);
                mytoast.SetGravity(GravityFlags.Center, 0, 0);
                RunOnUiThread(() => mytoast.Show());
                ThreadPool.QueueUserWorkItem(o => GetAuthToken());
                //Button button5 = FindViewById<Button>(Resource.Id.button5);
                //button5.Visibility = ViewStates.Visible;
                //ThreadPool.QueueUserWorkItem(o => GetEventsLog());
            };

            //Button button5 = FindViewById<Button>(Resource.Id.button5);
            button5.Click += delegate
            {
                //string chartdata = "";
                if (chartdata == "uninitialised")
                {
                    chartdata = "";
                    string tempstr = "";
                    string tempstr2 = "";
                    string tempstr3 = "";
                    string tempstr4 = "";
                    decimal[] crap = pir2averages;
                    int count = 0;
                    do
                    {
                        if (pir1averages == null)
                        {
                            //no point
                        }
                        else
                        {
                            tempstr = tempstr + pir1averages[count].ToString() + ",";
                        }

                        if (pir2averages == null)
                        {
                            //no point
                        }
                        else
                        {
                            tempstr2 = tempstr2 + pir2averages[count].ToString() + ",";
                        }

                        if (fridgeaverages == null)
                        {
                            //no point
                        }
                        else
                        {
                            tempstr4 = tempstr4 + fridgeaverages[count].ToString() + ",";
                        }

                        if (kettleaverages == null)
                        {
                            //no point
                        }
                        else
                        {
                            tempstr3 = tempstr3 + kettleaverages[count].ToString() + ",";
                        }


                        count++;
                    } while (count < 23);

                    if ((pir1today == null) | (pir1today == ""))
                    {
                        //no point
                    }
                    else
                    {
                        pir1today = Right(pir1today, pir1today.Length - 12);
                        chartdata = "Kitchen Sensor" + System.Environment.NewLine + tempstr + System.Environment.NewLine + pir1today + System.Environment.NewLine;
                    }

                    if ((fridgetoday == null) | (fridgetoday == ""))
                    {
                        //no point
                    }
                    else
                    {
                        fridgetoday = Right(fridgetoday, fridgetoday.Length - 12);
                        chartdata = chartdata + "Fridge Sensor" + System.Environment.NewLine + tempstr4 + System.Environment.NewLine + fridgetoday + System.Environment.NewLine;

                    }

                    if ((kettletoday == null) | (kettletoday == ""))
                    {
                        //no point
                    }
                    else
                    {
                        kettletoday = Right(kettletoday, kettletoday.Length - 12);
                        chartdata = chartdata + "Kettle Sensor" + System.Environment.NewLine + tempstr3 + System.Environment.NewLine + kettletoday + System.Environment.NewLine;
                    }

                    if ((pir2today == null) | (pir2today == ""))
                    {
                        //no point
                    }
                    else
                    {
                        pir2today = Right(pir2today, pir2today.Length - 12);
                        chartdata = chartdata + pir2name + System.Environment.NewLine + tempstr2 + System.Environment.NewLine + pir2today;
                    }

                }
                else
                {
                    //chartdata is whatever it was last time
                    string notused = "breakpoint";
                }


                if ((chartdata == "") || (chartdata == "uninitialised"))
                {
                    mytoast = Toast.MakeText(this, "CHART NOT AVAILABLE", ToastLength.Long);
                    mytoast.SetGravity(GravityFlags.Center, 0, 0);
                    RunOnUiThread(() => mytoast.Show());

                }
                else
                {
                    //temporarily excluded
                    var Graph = new Intent(this, typeof(Graph));
                    Graph.PutExtra("chartdata", chartdata);
                    StartActivity(Graph);
                }

                //StartActivity(typeof(Graph));
            };
        }
    }
}