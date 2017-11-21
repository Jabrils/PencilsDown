using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Android.Graphics;

namespace jsontools
{
    public class Backend
    {
        /// <summary>
        /// this generates a full api url using channel id & api key
        /// </summary>
        /// <param name="channelID">The ID for your channel, the one formated with random characters seems to be the only one that works</param>
        /// <param name="apiKey">The API key that youre going to use to pull request from YouTube's servers</param>
        /// <returns></returns>
        public static string GetFullURL(string channelID, string apiKey)
        {
            return String.Format("https://www.googleapis.com/youtube/v3/activities?part=snippet,contentDetails&channelId={0}&key={1}", channelID, apiKey);
        }

        /// <summary>
        /// this function downloads the data from the web & gives you that data as a string
        /// </summary>
        /// <param name="api">use the GetFullURL function</param>
        /// <returns></returns>
        public static string DownloadData(string api)
        {
            string ret = "";

            try
            {
                WebClient wC = new WebClient();
                ret = wC.DownloadString(api);
            }
            catch(Exception e)
            {
                ret = e.Message;
            }

            return ret;
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string text = e.Result;

        }
        /// <summary>
        /// This function does the complicated task of date subtraction for you, it compares the past date to today
        /// </summary>
        /// <param name="pastDate">Enter the past date</param>
        /// <returns></returns>
        public static int DateSubtraction(DateTime pastDate)
        {
            int ret = DateTime.Today.Subtract(pastDate).Days+1;

            return ret;
        }

        // 
        public static Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

    }

    public class Videos
    {
        public List<Item> items { get; set; }

        public class Item
        {
            public Snip snippet { get; set; }

            public class Snip
            {
                public DateTime publishedAt { get; set; }
                public string title;
                public Thumbs thumbnails { get; set; }

                public class Thumbs
                {
                    public High high { get; set; }
                    public Standard standard { get; set; }

                    public class High
                    {
                        public string url { get; set; }
                    }

                    public class Standard
                    {
                        public string url { get; set; }
                    }
                }
            }
        }
    }

}
