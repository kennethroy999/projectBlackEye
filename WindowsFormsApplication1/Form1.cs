﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using MetroFramework.Controls;
using System.Net;
using System.Web;
using System.Linq;
using System.Net.Http;
using MetroFramework;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Yeah Called");
            MakeMainTiles();
           
        }
       
        private void MakeMainTiles()
        {
            
            List<mainTile> mainTile_list = new List<mainTile>();
            metroPanelmainTile.Controls.Clear();
            string specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlackEye");

            if (!Directory.Exists(specificFolder))
                Directory.CreateDirectory(specificFolder);

            string fileName = Path.Combine(specificFolder, "data_file.txt");

            if (!File.Exists(fileName))
            {
               var k = File.Create(fileName);
                k.Close();
            }

            using (StreamReader file = File.OpenText(fileName))
                {
                
                List<mainTile> mainTile_list_tmp = new List<mainTile>();
                JsonSerializer serializer_read = new JsonSerializer();
                mainTile_list_tmp = (List<mainTile>)serializer_read.Deserialize(file, typeof(List<mainTile>));
                if(mainTile_list_tmp!=null)
                {
                    mainTile_list = mainTile_list_tmp;
                }      
            }

            // TODO: StreamReaderJSON and put the code in loop
           // mainTile m = new mainTile("New York Times", "https://www.facebook.com/nytimes/");
           // mainTile_list.Add(m);
            if(mainTile_list != null)
            {
                for (int i = 0; i < mainTile_list.Count; i++)
                {
                    MetroFramework.Controls.MetroTile _tile = new MetroTile();
                    _tile.Size = new Size(150, 50);
                    _tile.Tag = mainTile_list[i].Link;
                    _tile.Text = mainTile_list[i].Title;
                    _tile.Cursor = Cursors.Hand;
                    _tile.Location = new Point(i * 160, 0);
                    _tile.TextAlign = ContentAlignment.TopLeft;
                    _tile.TileTextFontWeight = MetroTileTextWeight.Bold;
                    _tile.Click += _tile_MainTile_Click;
                    metroPanelmainTile.Controls.Add(_tile);
                }
            }            
        }

        private void _tile_MainTile_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(Splash));
            t.Start();
            string url = Convert.ToString((sender as MetroTile).Tag);
            metroPanelLinks.Controls.Clear();
            List<mainTile> list = new List<mainTile>();
            list = getTheLinks(url);
            Console.WriteLine(list.Count);
            displayTiles(list);
            t.Abort();
        }

        private void Splash()
        {
            SplashScreen.SplashForm frm = new SplashScreen.SplashForm();
            frm.AppName = "Loading";
            //   frm.Size = new Size(700,350);
            frm.Text = "Getting the Links";
            Application.Run(frm);
           
        }

   
        private void displayTiles(List<mainTile> list)
        {
            int flag = 0;
            for (int i = 1; i <= list.Count; i++)
            {
                MetroFramework.Controls.MetroTile _tile = new MetroTile();
                Label namelabel = new Label();
                namelabel.AutoSize = true;
                namelabel.MaximumSize = new System.Drawing.Size(400, 60); ;
                namelabel.Text = list[(i - 1)].Title;
                namelabel.Font = new Font(namelabel.Font.Name, 12, FontStyle.Bold);
                _tile.Size = new Size(400, 200);
                _tile.Tag = list[(i - 1)].Link;
                _tile.Cursor = Cursors.Hand;
                if (flag == 0)
                {
                    int k = (i - 1) / 2;
                    _tile.Location = new Point(k * 410, 50);
                    namelabel.Location = new Point(k * 410, 260);
                    flag = 1;
                }
                else
                {
                    int k = (i - 1) / 2;
                    _tile.Location = new Point(k * 410, 350);
                    namelabel.Location = new Point(k * 410, 560);
                    flag = 0;

                }

                //_tile.Style = (MetroFramework.MetroColorStyle)i;
                _tile.Click += _tile_Click;
                //  _tile.Text = list[(i-1)*3];
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(list[(i - 1)].Img);
                request.Method = "GET";
                request.Accept = "text/html";
                request.UserAgent = "Fooo";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                {

                    _tile.TileImage = Image.FromStream(stream);

                }
                _tile.TextAlign = ContentAlignment.TopLeft;
                _tile.UseTileImage = true;
                //   _tile.TileTextFontWeight = MetroTileTextWeight.Bold;
                metroPanelLinks.Controls.Add(_tile);
                metroPanelLinks.Controls.Add(namelabel);
            }
        }

        private List<mainTile> getTheLinks(string url)
        {
            List<mainTile> list = new List<mainTile>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:48.0) Gecko/20100101 Firefox/48.0.1 Waterfox/48.0.1");
            string html = client.GetStringAsync(url).Result;
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);
            var anchors = document.DocumentNode.SelectNodes("//div[@class='lfloat _ohe']/span[@class='_3m6-']/div[@class='_6ks']/a");
            foreach (var a in anchors)
            {
                string onmouseover = HttpUtility.HtmlDecode(a.Attributes.AttributesWithName("onmouseover").First().Value);
                string image = HttpUtility.HtmlDecode(a.FirstChild.FirstChild.FirstChild.Attributes.AttributesWithName("src").First().Value);
                string title = a.ParentNode.NextSibling.FirstChild.FirstChild.InnerText;
                var pattern = new Regex(@"LinkshimAsyncLink.swap\(this, ""([^""]+)""\);");
                var pattern_image = new Regex(@"&url=([^&]+)");

                var link = pattern.Match(onmouseover).Groups[1].Value.Replace("\\", "");
                var img = HttpUtility.UrlDecode(pattern_image.Match(image).Groups[1].Value);
                mainTile newsTile = new mainTile();
                newsTile.add_newsTile(title, link, img);
                list.Add(newsTile);
            }

            return list;
        }

        private void _tile_Click(object sender, EventArgs e)
        {
            string link = Convert.ToString((sender as MetroTile).Tag);
            System.Diagnostics.Process.Start(link);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlackEye");
            string fileName = Path.Combine(specificFolder, "data_file.txt");


        }

        private void addButton_Click(object sender, EventArgs e)
        {
            List<mainTile> mainTile_list = new List<mainTile>();
            string specificFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlackEye");

            if (!Directory.Exists(specificFolder))
                Directory.CreateDirectory(specificFolder);

            string fileName = Path.Combine(specificFolder, "data_file.txt");

            using (StreamReader file = File.OpenText(fileName))
            {
                if (!File.Exists(fileName))
                {
                    File.Create(fileName);
                }
                List<mainTile> mainTile_list_tmp = new List<mainTile>();
                JsonSerializer serializer_read = new JsonSerializer();
                mainTile_list_tmp = (List<mainTile>)serializer_read.Deserialize(file, typeof(List<mainTile>));
                if (mainTile_list_tmp != null)
                {
                    mainTile_list = mainTile_list_tmp;
                }
            }
            // TODO: Get link from input
            mainTile m1 = new mainTile();
            m1.add_mainTile("Science Alert", "https://www.facebook.com/ScienceAlert/");
            mainTile_list.Add(m1);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                if (!File.Exists(fileName))
                    File.Create(fileName);

                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, mainTile_list);
                    // {"ExpiryDate":new Date(1230375600000),"Price":0}
                }
            }
            MakeMainTiles();
        }
    }

   
}
