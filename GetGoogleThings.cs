﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;

namespace Auction.mod
{
    public class GetGoogleThings
    {
        public bool loadeddata=false;
        public volatile bool workthreadready = true;
        public volatile bool dataisready = false;
        private PlayerStore pstore;
        private TradingWithBots twb;

        public struct sharedItem
        {
            public string time;
            public string status;
            public string id;
            public string seller;
        }

        public List<sharedItem> pStoreItems = new List<sharedItem>();


        private static GetGoogleThings instance;

        public static GetGoogleThings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GetGoogleThings();
                }
                return instance;
            }
        }

        private GetGoogleThings()
        {
            this.pstore = PlayerStore.Instance;
            this.twb = TradingWithBots.Instance;
        }

        public string getDataFromGoogleDocs()
        {
            WebRequest myWebRequest;
            if (this.loadeddata)
            {
                myWebRequest = WebRequest.Create("http://spreadsheets.google.com/feeds/list/" + this.twb.spreadsheet + "/od6/public/values?alt=json");
            }
            else
            {
                myWebRequest = WebRequest.Create("https://docs.google.com/spreadsheet/pub?key=" + this.twb.spreadsheet + "&output=txt");
            }
            //System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            
            string ressi = reader.ReadToEnd();
            return ressi;
        }

        public void readJsonfromGoogle(string txt)
        {
            //Console.WriteLine(txt);
            pStoreItems.Clear();
            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(txt);
            dictionary = (Dictionary<string, object>)dictionary["feed"];
            if (!dictionary.ContainsKey("entry")) { this.pstore.removeAllMessages();return; }
            Dictionary<string, object>[] entrys = (Dictionary<string, object>[])dictionary["entry"];
            for (int i = 0; i < entrys.GetLength(0); i++)
            {
                sharedItem si = new sharedItem();
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$timestamp"];
                si.time = (string)dictionary["$t"];
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$status"];
                si.status = (string)dictionary["$t"];
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$id"];
                si.id = (string)dictionary["$t"];
                dictionary = (Dictionary<string, object>)entrys[i]["gsx$seller"];
                si.seller = (string)dictionary["$t"];

                //clear the database (its googles job, but he may be to slow)
                if (si.status.StartsWith("SOLD") && si.id.Split(';')[3]!=App.MyProfile.ProfileInfo.id.ToString()) continue;

                if (si.status.StartsWith("BUY") )
                {
                    this.pStoreItems.RemoveAll(x => x.id == si.id && si.status.StartsWith("active") ); // remove all with active und same id

                    if (si.id.Split(';')[3] != App.MyProfile.ProfileInfo.id.ToString())//if not my id, ignore them
                    {
                        continue;
                    }
                }

                if (si.status.StartsWith("DELETE")) // delete the auctions (even if they are mine)
                {
                    foreach (string a in si.id.Split(','))
                    {
                        this.pStoreItems.RemoveAll(x => x.id == a);
                    }
                    continue;
                }
                
                this.pStoreItems.Add(si);
                Console.WriteLine(si.status + " " + si.id);
            }

            //addDataToPlayerStore();
        }

        public void readTxtfromGoogle(string txt)
        {
            //Console.WriteLine(txt);
            pStoreItems.Clear();
            foreach (string s in txt.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] data = s.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (data[0] == "Timestamp") continue;
                sharedItem si = new sharedItem();
                si.time = data[0];
                si.status = data[1];
                si.id = data[2];
                si.seller = data[3];

                //clear the database (its googles job, but he may be to slow)
                if (si.status.StartsWith("SOLD") && si.id.Split(';')[3] != App.MyProfile.ProfileInfo.id.ToString()) continue;

                if (si.status.StartsWith("BUY"))
                {
                    this.pStoreItems.RemoveAll(x => x.id == si.id && si.status.StartsWith("active")); // remove all with active und same id

                    if (si.id.Split(';')[3] != App.MyProfile.ProfileInfo.id.ToString())//if not my id, ignore them
                    {
                        continue;
                    }
                }

                if (si.status.StartsWith("DELETE")) // delete the auctions (even if they are mine)
                {
                    foreach (string a in si.id.Split(','))
                    {
                        this.pStoreItems.RemoveAll(x => x.id == a);
                    }
                    continue;
                }

                this.pStoreItems.Add(si);
                //Console.WriteLine(si.status + " " + si.id);
            }

            //addDataToPlayerStore();
        }

        public int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (int)(dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

        public DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); 
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public void addDataToPlayerStore()
        {
            this.pstore.removeAllMessages();
            List<Auction> auctionsToAdd = new List<Auction>();
            foreach (sharedItem si in this.pStoreItems)
            {
                Console.WriteLine("parsing: "+si.time+" "+si.status + " " + si.id);
                /*DateTime d = DateTime.ParseExact(si.time, "M/d/yyyy H:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                if (si.status.StartsWith("active:"))
                {
                    int hours = Convert.ToInt32(si.status.Split(':')[1]);
                    d = d.AddMinutes(hours);
                }
                 */
                DateTime d = UnixTimeStampToDateTime(Convert.ToInt32(si.id.Split(';')[4]));
                int level = 0;
                if (si.id.Split(';').Length == 6) level = Convert.ToInt32(si.id.Split(';')[5]);
                int id=Convert.ToInt32(si.id.Split(';')[0]);
                int price=Convert.ToInt32(si.id.Split(';')[2]);
                CardType type = CardTypeManager.getInstance().get(id);
                Card card = new Card(id, type, true);
                card.level = level;
                string text = si.id;
                if (si.status == "SOLD" || si.status == "BUY")
                {
                    text = "sold;"+text ;
                    d = DateTime.Now.AddYears(-10);
                }
                else { text = "active;"+text ; }
                Auction a = new Auction(si.seller,d,Auction.OfferType.SELL,card,text,price);
                auctionsToAdd.Add(a);
            }
            this.pstore.addAuctions(auctionsToAdd);
            this.pstore.removeOldEntrys();
            this.dataisready = false;
        }


        public void workthread()
        {
            this.workthreadready = false;
            Console.WriteLine("collect data");
            pStoreItems.Clear();
            try
            {
                if (this.loadeddata)
                {
                    this.readJsonfromGoogle(this.getDataFromGoogleDocs());
                }
                else
                {
                    this.readTxtfromGoogle(this.getDataFromGoogleDocs());
                    this.loadeddata = true;
                }
                //
                this.dataisready = true;
                System.Threading.Thread.Sleep(5000);
            }
            catch
            {
                Console.WriteLine("google error!");
            }

            this.workthreadready = true;
        }


    }
}
