using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using JsonFx.Json;
using System.IO;

namespace Auction.mod
{
    public class Prices
    {

            private static Prices instance;

        public static Prices Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Prices();
                }
                return instance;
            }
        }

        private Prices()
        {
            helpf = Helpfunktions.Instance;
            sttngs = Settings.Instance;
        }

        public int[] lowerprice = new int[0];
        public int[] upperprice = new int[0];
        public int[] sugprice = new int[0];
        //public bool roundwts = false;
        //public bool wtsroundup = true;
        //public int wtsroundmode = 0;
        //public bool roundwtb = false;
        //public bool wtbroundup = false;
        //public int wtbroundmode = 0;
        //public int takewtsgenint = 2;
        //public int takewtbgenint = 0;
        public Dictionary<int, string> wtspricelist1 = new Dictionary<int, string>();
        public Dictionary<int, string> wtbpricelist1 = new Dictionary<int, string>();
        Helpfunktions helpf;
        Settings sttngs;


		public int getPrice(int index, ScrollsPostPriceType type ) {
			switch(type) {
			case ScrollsPostPriceType.LOWER:
				return lowerprice [index];
			case ScrollsPostPriceType.SUGGESTED:
				return sugprice [index];
			case ScrollsPostPriceType.UPPER:
				return upperprice [index];
			default:
				throw new ArgumentException ();
			}
		}

        public int pricerounder(int index, bool wts)
        {
            int price = 0;
            if (wts)
            {
                if ((int)sttngs.wtsGeneratorPriceType == 0) price = this.lowerprice[index];
                if ((int)sttngs.wtsGeneratorPriceType == 1) price = this.sugprice[index];
                if ((int)sttngs.wtsGeneratorPriceType == 2) price = this.upperprice[index];
                if (sttngs.roundwts)
                {
                    if (sttngs.wtsroundup)
                    {


                        if (sttngs.wtsroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price + 5 - lastdigit; }
                            if (lastdigit > 5) { price = price + 10 - lastdigit; }
                        }
                        if (sttngs.wtsroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price + 10 - lastdigit; }
                        }
                        if (sttngs.wtsroundmode == 2)
                        {
                            int lastdigit = price % 100;
                            if (lastdigit > 0 && lastdigit < 50) { price = price + 50 - lastdigit; }
                            if (lastdigit > 50) { price = price + 100 - lastdigit; }
                        }

                    }
                    else
                    {
                        if (sttngs.wtsroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price - lastdigit; }
                            if (lastdigit > 5) { price = price + 5 - lastdigit; }
                        }
                        if (sttngs.wtsroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price - lastdigit; }
                        }
                        if (sttngs.wtsroundmode == 2)
                        {
                            int lastdigit = price % 100;
                            if (lastdigit > 0 && lastdigit < 50) { price = price - lastdigit; }
                            if (lastdigit > 50) { price = price + 50 - lastdigit; }
                        }

                    }
                }
            }
            else
            {
                if ((int)sttngs.wtbGeneratorPriceType == 0) price = this.lowerprice[index];
                if ((int)sttngs.wtbGeneratorPriceType == 1) price = this.sugprice[index];
                if ((int)sttngs.wtbGeneratorPriceType == 2) price = this.upperprice[index];
                if (sttngs.roundwtb)
                {
                    if (sttngs.wtbroundup)
                    {


                        if (sttngs.wtbroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price + 5 - lastdigit; }
                            if (lastdigit > 5) { price = price + 10 - lastdigit; }
                        }
                        if (sttngs.wtbroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price + 10 - lastdigit; }
                        }
                        if (sttngs.wtbroundmode == 2)
                        {
                            int lastdigit = price % 100;
                            if (lastdigit > 0 && lastdigit < 50) { price = price + 50 - lastdigit; }
                            if (lastdigit > 50) { price = price + 100 - lastdigit; }
                        }

                    }
                    else
                    {
                        if (sttngs.wtbroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price - lastdigit; }
                            if (lastdigit > 5) { price = price + 5 - lastdigit; }
                        }
                        if (sttngs.wtbroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price - lastdigit; }
                        }
                        if (sttngs.wtbroundmode == 2)
                        {
                            int lastdigit = price % 100;
                            if (lastdigit > 0 && lastdigit < 50) { price = price - lastdigit; }
                            if (lastdigit > 50) { price = price + 50 - lastdigit; }
                        }

                    }
                }

            }


            return price;
        }

        public void PriceChecker(int index, bool SPtarget, int SPretindex)
        {

            
                if (SPtarget)
                {
                    int price = this.upperprice[index];
                    price = pricerounder(index, SPtarget);



                    wtspricelist1[SPretindex] = price.ToString();

                }
                else
                {

                    int price = this.lowerprice[index];
                    price = pricerounder(index, SPtarget);


                    wtbpricelist1[SPretindex] = price.ToString();


                }
            
        }


        public void totalpricecheck()//int[] cardids
        {
            
            WebRequest myWebRequest;
            myWebRequest = WebRequest.Create("http://a.scrollsguide.com/prices");
           
            /*
            if (sttngs.scrollspostday == ScrollsPostDayType.thirty)
            { myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/30-days/"); }
            else 
            {
                if (sttngs.scrollspostday == ScrollsPostDayType.fourteen)
                { myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/14-days/"); }
                else
                {
                    if (sttngs.scrollspostday == ScrollsPostDayType.seven)
                    { myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/7-days/"); }
                    else
                    {
                        if (sttngs.scrollspostday == ScrollsPostDayType.three)
                        { myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/3-days/"); }
                        else
                        {
                            if (sttngs.scrollspostday == ScrollsPostDayType.hour)
                            { myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/1-hour/"); }
                            else
                            {
                                myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/1-day/");
                            }
                        }
                    }
                }
            }
            */
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            //Console.WriteLine(ressi);

            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(ressi);
            if ((string)(dictionary["msg"]) == "success")
            {
                Dictionary<string, object>[] d = (Dictionary<string, object>[])dictionary["data"];
                for (int i = 0; i < d.GetLength(0); i++)
                {
                    int id = Convert.ToInt32(d[i]["id"]);

                    int lower = 0; int higher = 0; int sugger = 0;
                    int high = (int)d[i]["buy"];
                    int low = (int)d[i]["sell"];
                    if (high != 0)
                    {
                        int value = high;
                        lower = value;
                        higher = value;
                    }
                    if (low != 0)
                    {
                        int value = low;
                        if (value < lower || (value != 0 && lower == 0)) { lower = value; }
                        if (value > higher || (value != 0 && higher == 0)) { higher = value; }
                    }
                    sugger = (lower + higher) / 2;

                    //int index = Array.FindIndex(cardids, element => element == id);
                    int index = helpf.cardidToArrayIndex(id);
                    if (index >= 0)
                    {
                        lowerprice[index] = lower;
                        upperprice[index] = higher;
                        sugprice[index] = sugger;
                    }

                }
            }

        }

        public void resetarrays(int len)
        {
            this.lowerprice = new int[len];
            this.upperprice = new int[len];
            this.sugprice = new int[len];
        }

        /*
        public void PriceCheck( String search)
            {

                //WebClient wc = new WebClient(); // webclient has no timeout
               //string ressi= wc.DownloadString(new Uri("http://api.scrollspost.com/v1/price/1-day/" + search + ""));

                WebRequest myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/price/1-day/" + search + "");
                if (search == "") { myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/1-day/"); }
               myWebRequest.Timeout = 10000;
               WebResponse myWebResponse = myWebRequest.GetResponse();
               System.IO.Stream stream = myWebResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
                string ressi = reader.ReadToEnd();
               //Console.WriteLine(ressi);
                string price="0";
                string sugprice = "0";
                if (ressi.Contains("\"suggested\":"))
                {

                    sugprice = (ressi.Split(new string[] { "\"suggested\":" }, StringSplitOptions.None))[1];
                    sugprice = sugprice.Split(',')[0];
                    if (this.SPtarget)
                    {
                        //price = (ressi.Split(new string[] { "\"suggested\":" }, StringSplitOptions.None))[1];
                        //price = price.Split(',')[0];
                        price = (ressi.Split(new string[] { "\"sell\":" }, StringSplitOptions.None))[1];
                        price = price.Split('}')[0];
                        if (price == "0") { price = sugprice; }

                    }
                    else
                    {
                        price = (ressi.Split(new string[] { "\"buy\":" }, StringSplitOptions.None))[1];
                        price = price.Split(',')[0];
                        if (price == "0") { price = sugprice; }
                    }
                }
                else { price="error";}
                if (this.SPtarget) { wtspricelist1[SPretindex.ToLower()] = price; } else { wtbpricelist1[SPretindex.ToLower()] = price; } 

            }
         */

    }
}
