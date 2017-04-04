
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using System;

namespace SingleViewAndroid
{
    [Activity(Label = "Graph", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class Graph : Activity
    {
        int numlines;
        string[] todayvals;
        string[] avevalstr;
        int lastgraph, lasttitle;
        int count;
        int max;
        string graphtitle;

        public static PlotModel CreatePlotModel(string title, double[] data1, double[] data2, int max)
        {

            var plotModel = new PlotModel { Title = title };
            plotModel.Background = OxyColors.Ivory;
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, IntervalLength = 40 });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = max + 5, Minimum = 0 });

            var series1 = new LineSeries
            {
                MarkerType = MarkerType.None,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White

            };

            //today
            series1.Points.Add(new DataPoint(0.5, data1[0]));
            series1.Points.Add(new DataPoint(1.5, data1[1]));
            series1.Points.Add(new DataPoint(2.5, data1[2]));
            series1.Points.Add(new DataPoint(3.5, data1[3]));
            series1.Points.Add(new DataPoint(4.5, data1[4]));
            series1.Points.Add(new DataPoint(5.5, data1[5]));
            series1.Points.Add(new DataPoint(6.5, data1[6]));
            series1.Points.Add(new DataPoint(7.5, data1[7]));
            series1.Points.Add(new DataPoint(8.5, data1[8]));
            series1.Points.Add(new DataPoint(9.5, data1[9]));
            series1.Points.Add(new DataPoint(10.5, data1[10]));
            series1.Points.Add(new DataPoint(11.5, data1[11]));
            series1.Points.Add(new DataPoint(12.5, data1[12]));
            series1.Points.Add(new DataPoint(13.5, data1[13]));
            series1.Points.Add(new DataPoint(14.5, data1[14]));
            series1.Points.Add(new DataPoint(15.5, data1[15]));
            series1.Points.Add(new DataPoint(16.5, data1[16]));
            series1.Points.Add(new DataPoint(17.5, data1[17]));
            series1.Points.Add(new DataPoint(18.5, data1[18]));
            series1.Points.Add(new DataPoint(19.5, data1[19]));
            series1.Points.Add(new DataPoint(20.5, data1[20]));
            series1.Points.Add(new DataPoint(21.5, data1[21]));
            series1.Points.Add(new DataPoint(22.5, data1[22]));
            series1.Points.Add(new DataPoint(23.5, data1[23]));

            var series2 = new LineSeries
            {
                MarkerType = MarkerType.None,
                MarkerSize = 4,
                MarkerStroke = OxyColors.Blue
            };
            //average
            series2.Points.Add(new DataPoint(0.5, data2[0]));
            series2.Points.Add(new DataPoint(1.5, data2[1]));
            series2.Points.Add(new DataPoint(2.5, data2[2]));
            series2.Points.Add(new DataPoint(3.5, data2[3]));
            series2.Points.Add(new DataPoint(4.5, data2[4]));
            series2.Points.Add(new DataPoint(5.5, data2[5]));
            series2.Points.Add(new DataPoint(6.5, data2[6]));
            series2.Points.Add(new DataPoint(7.5, data2[7]));
            series2.Points.Add(new DataPoint(8.5, data2[8]));
            series2.Points.Add(new DataPoint(9.5, data2[9]));
            series2.Points.Add(new DataPoint(10.5, data2[10]));
            series2.Points.Add(new DataPoint(11.5, data2[11]));
            series2.Points.Add(new DataPoint(12.5, data2[12]));
            series2.Points.Add(new DataPoint(13.5, data2[13]));
            series2.Points.Add(new DataPoint(14.5, data2[14]));
            series2.Points.Add(new DataPoint(15.5, data2[15]));
            series2.Points.Add(new DataPoint(16.5, data2[16]));
            series2.Points.Add(new DataPoint(17.5, data2[17]));
            series2.Points.Add(new DataPoint(18.5, data2[18]));
            series2.Points.Add(new DataPoint(19.5, data2[19]));
            series2.Points.Add(new DataPoint(20.5, data2[20]));
            series2.Points.Add(new DataPoint(21.5, data2[21]));
            series2.Points.Add(new DataPoint(22.5, data2[22]));
            series2.Points.Add(new DataPoint(23.5, data2[23]));

            series1.Smooth = true;
            series2.Smooth = true;
            series1.Title = "Yesterday";

            //series1.FontSize = 100;
            //series2.FontSize = 50.0;
            series2.Title = "Average";
            series1.Color = OxyColors.Blue;
            series2.Color = OxyColors.Green;
            plotModel.Series.Add(series1);
            plotModel.TitleFontSize = 30;
            plotModel.Series.Add(series2);
            //plotModel.Background.ChangeSaturation;

            return plotModel;
        }

        public static string Right(string original, int numberCharacters)
        {
            return original.Substring(original.Length - numberCharacters);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Graph);
            string text = Intent.GetStringExtra("chartdata") ?? "uninitialised";
            string[] lines2 = text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            //string today = lines2[2];
            numlines = lines2.Length;

            //start with the first block, lines 0-2 inclusive
            lastgraph = 2;
            lasttitle = 0;
            graphtitle = lines2[0];
            //lines2[2] =  Right(lines2[2], lines2[2].Length - 12);
            string[] todayvals = lines2[2].Split(new string[] { "," }, StringSplitOptions.None);
            string[] avevalstr = lines2[1].Split(new string[] { "," }, StringSplitOptions.None);
            //double[] today;
            //double[] averages;
            double[] averages = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] today = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            count = 0;
            max = 0;
            do
            {
                averages[count] = Convert.ToDouble(avevalstr[count]);
                if (averages[count] > max)
                {
                    max = Convert.ToInt32(averages[count]);
                }

                today[count] = Convert.ToDouble(todayvals[count]);
                if (today[count] > max)
                {
                    max = Convert.ToInt32(today[count]);
                }
                count++;
            } while (count < 23);

            // Create your application here
            //var plotView = new PlotView(this);
            var plotView = FindViewById<OxyPlot.Xamarin.Android.PlotView>(Resource.Id.plotView1);

            plotView.Model = CreatePlotModel(lines2[0], today, averages, max);
            var b2 = FindViewById<Button>(Resource.Id.button2);
            var b3 = FindViewById<Button>(Resource.Id.button3);
            //b2.Text = "NEXT >>";
            //this.AddContentView(plotView,
            //    new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            b2.Click += delegate
            //next graph
            {
                //RunOnUiThread(() => Toast.MakeText(this, "NEXT", ToastLength.Long).Show());
                //if (lastgraph == 2)
                //{
                if ((lastgraph + 3) < numlines)
                {
                    lastgraph = lastgraph + 3;
                    lasttitle = lasttitle + 3;
                    graphtitle = lines2[lasttitle];
                    //lines2[lastgraph] = Right(lines2[lastgraph], lines2[lastgraph].Length - 12);
                    todayvals = lines2[lastgraph].Split(new string[] { "," }, StringSplitOptions.None);
                    avevalstr = lines2[lastgraph - 1].Split(new string[] { "," }, StringSplitOptions.None);
                    int z;
                    z = lastgraph;
                }
                else
                {
                    //we're at the end of the list, cycle back to the start
                    lasttitle = 0;
                    lastgraph = 2;
                    graphtitle = lines2[lasttitle];
                    //lines2[2] = Right(lines2[2], lines2[2].Length - 12);
                    todayvals = lines2[2].Split(new string[] { "," }, StringSplitOptions.None);
                    avevalstr = lines2[1].Split(new string[] { "," }, StringSplitOptions.None);
                    int z2;
                    z2 = lastgraph;
                }
                //}
                //else
                //{

                //}

                //double[] today2;
                //double[] averages2;
                double[] averages2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                double[] today2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int count2 = 0;
                int max2 = 0;
                do
                {
                    averages2[count2] = Convert.ToDouble(avevalstr[count2]);
                    if (averages2[count2] > max2)
                    {
                        max2 = Convert.ToInt32(averages2[count2]);
                    }

                    today2[count2] = Convert.ToDouble(todayvals[count2]);
                    if (today2[count2] > max2)
                    {
                        max2 = Convert.ToInt32(today2[count2]);
                    }
                    count2++;
                } while (count2 < 23);

                // Create your application here
                //var plotView = new PlotView(this);
                plotView = FindViewById<OxyPlot.Xamarin.Android.PlotView>(Resource.Id.plotView1);

                plotView.Model = CreatePlotModel(graphtitle, today2, averages2, max2);
                //b1 = FindViewById<Button>(Resource.Id.button1);
                //var b2 = FindViewById<Button>(Resource.Id.button2);
                //var b3 = FindViewById<Button>(Resource.Id.button3);
                //b2.Text = "NEXT >>";

            };

            b3.Click += delegate {
                Finish();
            };
        }
    }
}