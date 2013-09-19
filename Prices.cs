using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using JsonFx.Json;

namespace Auction.mod
{
    class Prices
    {

        public int[] lowerprice = new int[0];
        public int[] upperprice = new int[0];
        public int[] sugprice = new int[0];
        public bool roundwts = false;
        public bool wtsroundup = true;
        public int wtsroundmode = 0;
        public bool roundwtb = false;
        public bool wtbroundup = false;
        public int wtbroundmode = 0;
        public int takewtsgenint = 2;
        public int takewtbgenint = 0;
        public Dictionary<string, string> wtspricelist1 = new Dictionary<string, string>();
        public Dictionary<string, string> wtbpricelist1 = new Dictionary<string, string>();

        public int pricerounder(int index, bool wts)
        {
            int price = 0;
            if (wts)
            {
                if (takewtsgenint == 0) price = this.lowerprice[index];
                if (takewtsgenint == 1) price = this.sugprice[index];
                if (takewtsgenint == 2) price = this.upperprice[index];
                if (roundwts)
                {
                    if (wtsroundup)
                    {


                        if (wtsroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price + 5 - lastdigit; }
                            if (lastdigit > 5) { price = price + 10 - lastdigit; }
                        }
                        if (wtsroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price + 10 - lastdigit; }
                        }
                        if (wtsroundmode == 2)
                        {
                            int lastdigit = price % 100;
                            if (lastdigit > 0 && lastdigit < 50) { price = price + 50 - lastdigit; }
                            if (lastdigit > 50) { price = price + 100 - lastdigit; }
                        }

                    }
                    else
                    {
                        if (wtsroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price - lastdigit; }
                            if (lastdigit > 5) { price = price + 5 - lastdigit; }
                        }
                        if (wtsroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price - lastdigit; }
                        }
                        if (wtsroundmode == 2)
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
                if (takewtbgenint == 0) price = this.lowerprice[index];
                if (takewtbgenint == 1) price = this.sugprice[index];
                if (takewtbgenint == 2) price = this.upperprice[index];
                if (roundwtb)
                {
                    if (wtbroundup)
                    {


                        if (wtbroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price + 5 - lastdigit; }
                            if (lastdigit > 5) { price = price + 10 - lastdigit; }
                        }
                        if (wtbroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price + 10 - lastdigit; }
                        }
                        if (wtbroundmode == 2)
                        {
                            int lastdigit = price % 100;
                            if (lastdigit > 0 && lastdigit < 50) { price = price + 50 - lastdigit; }
                            if (lastdigit > 50) { price = price + 100 - lastdigit; }
                        }

                    }
                    else
                    {
                        if (wtbroundmode == 0)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0 && lastdigit < 5) { price = price - lastdigit; }
                            if (lastdigit > 5) { price = price + 5 - lastdigit; }
                        }
                        if (wtbroundmode == 1)
                        {
                            int lastdigit = price % 10;
                            if (lastdigit > 0) { price = price - lastdigit; }
                        }
                        if (wtbroundmode == 2)
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

        public void PriceChecker(int index, bool SPtarget, string SPretindex)
        {

            
                if (SPtarget)
                {
                    int price = this.upperprice[index];
                    price = pricerounder(index, SPtarget);



                    wtspricelist1[SPretindex.ToLower()] = price.ToString();

                }
                else
                {

                    int price = this.lowerprice[index];
                    price = pricerounder(index, SPtarget);


                    wtbpricelist1[SPretindex.ToLower()] = price.ToString();


                }
            
        }

        public void totalpricecheck(int[] cardids)
        {

            //WebClient wc = new WebClient(); // webclient has no timeout
            //string ressi= wc.DownloadString(new Uri("http://api.scrollspost.com/v1/price/1-day/" + search + ""));

            WebRequest myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/1-day/");
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            //Console.WriteLine(ressi);

            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object>[] dictionary = (Dictionary<string, object>[])jsonReader.Read(ressi);
            for (int i = 0; i < dictionary.GetLength(0); i++)
            {
                int id = Convert.ToInt32(dictionary[i]["card_id"]);
                Dictionary<string, object> d = (Dictionary<string, object>)dictionary[i]["price"];
                int lower = 0; int higher = 0; int sugger = 0;

                int sug = (int)d["suggested"];
                int high = (int)d["buy"];
                int low = (int)d["sell"];
                if (sug != 0)
                {
                    lower = sug;
                    higher = lower;
                    sugger = sug;
                }
                if (high != 0)
                {
                    int value = high;
                    if (value < lower || (value != 0 && lower == 0)) { lower = value; }
                    if (value > higher || (value != 0 && higher == 0)) { higher = value; }
                }
                if (low != 0)
                {
                    int value = low;
                    if (value < lower || (value != 0 && lower == 0)) { lower = value; }
                    if (value > higher || (value != 0 && higher == 0)) { higher = value; }
                }


                int index = Array.FindIndex(cardids, element => element == id);
                if (index >= 0)
                {
                    lowerprice[index] = lower;
                    upperprice[index] = higher;
                    sugprice[index] = sugger;
                }

            }

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
