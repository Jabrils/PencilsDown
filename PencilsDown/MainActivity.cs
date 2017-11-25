using Android.App;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using jsontools;
using Android.Graphics;
using Android.Views;
using SQLite.Net;

namespace PencilsDown
{
    [Activity(Label = "PencilsDown", MainLauncher = true, Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen")]
    public class MainActivity : Activity
    {
        string dataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string dataFile;
        string save = "savedata.txt";

        public int daysCycle = 7;
        public string channelID = "UCQALLeQPoZdZC4JNUboVEUg";

        int[] dayFrac;
        int whichVid = 0;

        Random r = new Random();

        // INSERT YOUR PERSONAL API KEY HERE FOR THIS TO WORK AT ALL! If you don't know how to do this, please visit https://developers.google.com/youtube/v3/getting-started & actually read it lol
	string apiKey = "";
        string json;

        Color[] bg = new Color[]{
            Color.Rgb(152,251,152),
            Color.Rgb(80,0,0),
};

        int[] gfxs = new int[]{
            // 0
            Resource.Drawable.target,
            // 1
            Resource.Drawable.relax, 
            // 2
            Resource.Drawable.working, 
            // 3
            Resource.Drawable.wrap, 
            // 4
            Resource.Drawable.pencil, 
            // 5
            Resource.Drawable.youtube, 
            // 6
            Resource.Drawable.late,
            // 7
            Resource.Drawable.skull,
        };
        int prevHour;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            dataFile = System.IO.Path.Combine(dataPath, save);

            // 
            if (!File.Exists(dataFile))
            {
                SaveData(dataFile);

                // For Debugging purposes
                // AlertDialog.Builder a = new AlertDialog.Builder(this);
                // a.SetMessage("CREATED!").Show();
            }

            // this demo mode is simply for the video
            WhichScreen(false);
        }

        /// <summary>
        /// Only set this to true if you want to view the raw data, I used this for the video I published
        /// </summary>
        /// <param name="demo"></param>
        void WhichScreen(bool demo)
        {
            if (!demo)
            {
                // 
                    string loadedSave = ReadSaveData(dataFile);
                channelID = loadedSave.Split(',')[0];
                daysCycle = int.Parse(loadedSave.Split(',')[1]);

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.Main);

                // 
                Button b = FindViewById<Button>(Resource.Id.button1);
                b.Click += ButClick;
                b.SetHighlightColor(Color.Aqua);

                // 
                FindViewById<View>(Resource.Id.inputs).Visibility = ViewStates.Gone;

                FindViewById<EditText>(Resource.Id.editText1).Text = channelID;
                FindViewById<EditText>(Resource.Id.editText2).Text = "" + daysCycle;

                LoadData();

                //
                Thread t = new Thread(abc);
                t.Start();
            }
            else
            {
                SetContentView(Resource.Layout.demo);

                TextView tV = FindViewById<TextView>(Resource.Id.textView1);
                tV.SetTextColor(Color.Black);
                tV.TextSize = 10;

                View v = FindViewById(Resource.Id.top);
                v.SetBackgroundColor(Color.LightGreen);

                // 
                string api = Backend.GetFullURL(channelID, apiKey);

                // 
                //json = Backend.DownloadData(api);
                json = Backend.DownloadData(api);
            }
        }

        string ReadSaveData(string dataFile)
        {
            StreamReader sR = new StreamReader(dataFile);
            string ss = sR.ReadToEnd();
            sR.Close();
            return ss;
        }

        void SaveData(string dataFile)
        {
            StreamWriter sW = new StreamWriter(dataFile);
            sW.Write(channelID + "," + daysCycle);
            sW.Close();
        }

        /// <summary>
        /// This updates the fraction intervals for determining what gfx to display
        /// </summary>
        void UpdateDayFract()
        {
            // 
            dayFrac = new int[]
            {
                (int)Math.Round(daysCycle*.143f),
                (int)Math.Round(daysCycle*.286f),
                (int)Math.Round(daysCycle*.714f),
                (int)Math.Round(daysCycle*.857f),
            };
        }

        // 
        void ButClick(object sender, EventArgs e)
        {
            View v = FindViewById<View>(Resource.Id.inputs);
            Button b = FindViewById<Button>(Resource.Id.button1);
            EditText eT1 = FindViewById<EditText>(Resource.Id.editText1);
            EditText eT2 = FindViewById<EditText>(Resource.Id.editText2);


            bool vis = (v.Visibility == ViewStates.Visible);

            v.Visibility = (vis) ? ViewStates.Gone : ViewStates.Visible;
            b.Text = (vis) ? "Settings" : "Submit";

            if(vis)
            {
                if (eT1.Text != channelID)
                {
                    channelID = eT1.Text;
                    LoadData();
                    SaveData(dataFile);
                }

                if (eT2.Text != ""+daysCycle)
                {
                    daysCycle = int.Parse(eT2.Text);
                    UpdateUI();
                    SaveData(dataFile);
                }
            }
        }

        // 

        // 
        void abc()
        {
            // 
            while (true)
            {
                RunOnUiThread(Update);
                Thread.Sleep((1000*60));
            }
        }

        // 
        void Update()
        {
            // 
            if (DateTime.Now.Hour != prevHour)
            {
                LoadData();
            }
        }

        // 
        void LoadData()
        {
            // 
            ImageView task = FindViewById<ImageView>(Resource.Id.imageView1);
            ImageView thumb = FindViewById<ImageView>(Resource.Id.imageView2);
            TextView tV = FindViewById<TextView>(Resource.Id.textView1);
            View v = FindViewById(Resource.Id.top);

            // 
            tV.SetBackgroundColor(Color.White);

            // 
            string api = Backend.GetFullURL(channelID, apiKey);

            // 
            json = Backend.DownloadData(api);

            // 
            if (!json.Contains("Error"))
            {
                // 
                Videos ch = JsonConvert.DeserializeObject<Videos>(json);

                // 
                int DaysAgo = Backend.DateSubtraction(ch.items[whichVid].snippet.publishedAt);

                // 
                v.SetBackgroundColor(FindBGCol(bg[0], bg[1], DaysAgo));

                // 
                var imageBitmap = Backend.GetImageBitmapFromUrl(ch.items[whichVid].snippet.thumbnails.standard.url);
                thumb.SetImageBitmap(imageBitmap);

                // 
                tV.SetTextColor(Color.Black);
                tV.Text = "You last uploaded " + ch.items[whichVid].snippet.title + " " + DaysAgo + " days ago.";
                tV.Gravity = GravityFlags.Center;

                // 
                task.SetImageResource(DrawGFX(DaysAgo));

                // 
                prevHour = DateTime.Now.Hour;
            }
            else if(json.Contains("Error"))
            {
                task.SetImageResource(0);
                thumb.SetImageResource(0);
                tV.Text = json;
            }
            else
            {
                task.SetImageResource(0);
                thumb.SetImageResource(0);
                tV.Text = "CHANNEL DOESN'T EXIST! PLEASE TRY AGAIN!";
            }
        }

        // 
        void UpdateUI()
        {
            // 
            ImageView task = FindViewById<ImageView>(Resource.Id.imageView1);
            ImageView thumb = FindViewById<ImageView>(Resource.Id.imageView2);
            TextView tV = FindViewById<TextView>(Resource.Id.textView1);
            View v = FindViewById(Resource.Id.top);

            // 
            Videos ch = JsonConvert.DeserializeObject<Videos>(json);

            // 
            int DaysAgo = Backend.DateSubtraction(ch.items[whichVid].snippet.publishedAt);

            // 
            v.SetBackgroundColor(FindBGCol(bg[0], bg[1], DaysAgo));

            // 
            var imageBitmap = Backend.GetImageBitmapFromUrl(ch.items[whichVid].snippet.thumbnails.standard.url);
            thumb.SetImageBitmap(imageBitmap);

            // 
            tV.SetTextColor(Color.Black);
            tV.Text = "You last uploaded " + ch.items[whichVid].snippet.title + " " + DaysAgo + " days ago.";
            tV.Gravity = GravityFlags.Center;

            // 
            task.SetImageResource(DrawGFX(DaysAgo));
        }

        // 
        Color FindBGCol(Color color1, Color color2, int dA)
        {
            int[] loss = new int[3];
            loss[0] = color1.R - color2.R;
            loss[1] = color1.G - color2.G;
            loss[2] = color1.B - color2.B;

            int[] nRBG = new int[3];

            float[] transpose = new float[2];
            transpose[0] = dA;
            transpose[1] = daysCycle;

            nRBG[0] = (color1.R - (int)(loss[0] * Clamp((float)dA / (float)daysCycle)));
            nRBG[1] = (color1.G - (int)(loss[1] * Clamp((float)dA / (float)daysCycle)));
            nRBG[2] = (color1.B - (int)(loss[2] * Clamp((float)dA/ (float)daysCycle)));

            return new Color(nRBG[0], nRBG[1], nRBG[2]);
        }

        /// <summary>
        /// This is used to clamp the r g b values of the background from 0-1f
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        float Clamp(float v)
        {
            v = (v > 1) ? 1 : (v < 0) ? 0 : v;
            return v;
        }

        //
        int DrawGFX(int DaysAgo)
        {
            UpdateDayFract();

            int ret = 0;

            // 
            if (DaysAgo >= 0 && DaysAgo < dayFrac[0])
            {
                ret = gfxs[0];
            }
            else if (DaysAgo >= dayFrac[0] && DaysAgo < dayFrac[1])
            {
                ret = gfxs[1];
            }
            else if (DaysAgo >= dayFrac[1] && DaysAgo < dayFrac[2])
            {
                ret = gfxs[2];
            }
            else if (DaysAgo >= dayFrac[2] && DaysAgo < dayFrac[3])
            {
                ret = gfxs[3];
            }
            else if (DaysAgo >= dayFrac[3] && DaysAgo < daysCycle)
            {
                ret = gfxs[4];
            }
            else if (DaysAgo == daysCycle)
            {
                ret = gfxs[5];
            }
            else if (DaysAgo > daysCycle && DaysAgo < daysCycle*2)
            {
                ret = gfxs[6];
            }
            else if (DaysAgo >= daysCycle*2)
            {
                ret = gfxs[7];
            }
            return ret;
        }
    }
}
