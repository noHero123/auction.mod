using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Auction.mod
{
    class Messageparser
    {

        //private List<Auction> addingcards = new List<Auction>();




        Helpfunktions helpf;
        private Regex priceregexOffer = new Regex(@".*[0-9]{1,9}[g]?.*");
        private Regex priceregex = new Regex(@".*[^x0-9]+[0-9]{2,9}[g]?[^x0-9]+.*");
        private Regex priceregexpriceonname = new Regex(@"[^x0-9]{2,}[0-9]{2,9}[g]?[^x0-9]+.*");
        Regex numberregx = new Regex(@"[0-9]{2,9}");
        Regex numberregxOffer = new Regex(@"[0-9]{1,9}");
        public List<nickelement> searchscrollsnicks = new List<nickelement>();

        // private int cardnametoid(string name) { return helpf.cardids[Array.FindIndex(helpf.cardnames, element => element.Equals(name))]; }


        private static Messageparser instance;

        public static Messageparser Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Messageparser();
                }
                return instance;
            }
        }

        private Messageparser()
        {
            this.helpf = Helpfunktions.Instance;
        }


        private string pricetestfirst(string price)
        {
            string result = "";
            //Console.WriteLine("PREIS: " + price);
            Match match = this.priceregexpriceonname.Match(" " + price + " ");
            if (match.Success) { result = match.Value; }

            return result;
        }

        private string pricetest(string price)
        {
            string result = "";
            //Console.WriteLine("PREIS: " + price);
            Match match = priceregex.Match(" " + price + " ");
            if (match.Success) { result = match.Value; }

            return result;
        }


        /*private void additemtolist(Card c, string from, int gold, bool wts, string wholemsg)
{
if (wts)
{
this.addingcards.Add(new Auction(from, DateTime.Now, Auction.OfferType.SELL, c, wholemsg, gold));
}
else
{

this.addingcards.Add(new Auction(from, DateTime.Now, Auction.OfferType.BUY, c, wholemsg, gold));
}

}*/

        private static void GetAuctionsFromShortIntoList(string shortList, string from, Auction.OfferType offerType, List<Auction> outList)
        {
            string[] words = shortList.Split(';');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "" || words[i] == " ") break;
                string price;
                string ids;
                if (words[i].Contains(' '))
                {
                    price = words[i].Split(' ')[1];
                    if (price == "") { price = "0"; }
                    ids = words[i].Split(' ')[0];
                }

                else
                {
                    price = "0";
                    ids = words[i];
                }

                string[] ideen = ids.Split(','); //When string does not contain a , the whole string is the first element of the returned array.
                foreach (string idd in ideen)
                {
                    int id = Convert.ToInt32(idd);
                    CardType type = CardTypeManager.getInstance().get(id);
                    Card card = new Card(id, type, true);
                    outList.Add(new Auction(from, DateTime.Now, offerType, card, "(Network Auction)", Convert.ToInt32(price)));
                }

            }
        }
        public static List<Auction> GetAuctionsFromShortMessage(string msg, string from)
        {

            List<Auction> addingcards = new List<Auction>();
            //bool wts = true;
            string secmsg = "";
            if (msg.StartsWith("aucs "))
            {
                //wts = true;
                msg = msg.Remove(0, 5);
                if (msg.Contains("aucb "))
                {
                    secmsg = (msg.Split(new string[] { "aucb " }, StringSplitOptions.None))[1];
                    msg = (msg.Split(new string[] { "aucb " }, StringSplitOptions.None))[0];
                    GetAuctionsFromShortIntoList(secmsg, from, Auction.OfferType.BUY, addingcards);
                }
                GetAuctionsFromShortIntoList(msg, from, Auction.OfferType.SELL, addingcards);

            }
            if (msg.StartsWith("aucb "))
            {
                msg = msg.Remove(0, 5);
                if (msg.Contains("aucs "))
                {
                    secmsg = (msg.Split(new string[] { "aucs " }, StringSplitOptions.None))[1];
                    msg = (msg.Split(new string[] { "aucs " }, StringSplitOptions.None))[0];
                    GetAuctionsFromShortIntoList(secmsg, from, Auction.OfferType.SELL, addingcards);
                }
                GetAuctionsFromShortIntoList(msg, from, Auction.OfferType.BUY, addingcards);
            }
            return addingcards;
        }

        public List<Auction> GetAuctionsFromMessage(string msgg, string from, string room)
        {
            string msg = Regex.Replace(msgg, @"(<color=#[A-Za-z0-9]{0,6}>)|(</color>)", String.Empty);
            // todo: delete old msgs from author
            if (msg.StartsWith("aucs ") || msg.StartsWith("aucb ")) { return GetAuctionsFromShortMessage(msg, from); }
            //if (msg.StartsWith("aucc ")) { respondtocommand(msg,from); return; }
            Auction.OfferType currentOfferType = Auction.OfferType.BUY; //Will be overwritten
            //string[] words=msg.Split(' ');
            List<Auction> addingAuctions = new List<Auction>();
            char[] delimiters = new char[] { '\r', '\n', ' ', ',', ';' };
            string[] words = msg.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            //words = Regex.Split(msg, @"");

            if (!msg.ToLower().Contains("wts") && !msg.ToLower().Contains("wtb") && !msg.ToLower().Contains("sell") && !msg.ToLower().Contains("buy")) return addingAuctions;
            bool wtxfound = false;

            for (int i = 0; i < words.Length; i++)
            {
                Card c; int price = 0;
                string word = words[i].ToLower();
                // save in wts or wtb?
                if (word.Contains("wts") || word.Contains("sell")) { currentOfferType = Auction.OfferType.SELL; wtxfound = true; };
                if (word.Contains("wtb") || word.Contains("buy")) { currentOfferType = Auction.OfferType.BUY; wtxfound = true; };
                if (!wtxfound) continue;// if no wts or wtb was found, skip card search
                //int arrindex = Array.FindIndex(this.cardnames, element => word.Contains(element.Split(' ')[0])); // changed words[i] and element!
                int arrindex = this.searchscrollsnicks.FindIndex(element => word.Contains(element.nick.Split(' ')[0]));
                int iadder = 0;
                if (arrindex >= 0) // wort in cardlist enthalten
                {
                    //Console.WriteLine(word + " " + arrindex);
                    //string[] possiblecards = Array.FindAll(this.cardnames, element => word.Contains(element.Split(' ')[0]));
                    List<nickelement> possibnics = this.searchscrollsnicks.FindAll(element => word.Contains(element.nick.Split(' ')[0]));
                    bool findcard = false;
                    string foundedcard = "";
                    string textplace = "";

                    for (int ii = 0; ii < possibnics.Count; ii++)
                    {
                        //string match = possiblecards[ii].ToLower();
                        string match = possibnics[ii].nick.ToLower();
                        int posleng = Math.Min(match.Split(' ').Length, words.Length - i);
                        string searchob = string.Join(" ", words, i, posleng).ToLower();
                        if (searchob.Contains(match)) { findcard = true; foundedcard = possibnics[ii].cardname.ToLower(); iadder = posleng; textplace = searchob; break; };


                    }
                    //

                    i = i + iadder;

                    if (findcard)
                    {
                        //CardType type = CardTypeManager.getInstance().get(cardnametoid(foundedcard.ToLower()));
                        CardType type = CardTypeManager.getInstance().get(helpf.cardnamesToID[foundedcard.ToLower()]);
                        //int realarrindex = Array.FindIndex(helpf.cardnames, element => foundedcard.Equals(element));
                        int realarrindex = helpf.cardnameToArrayIndex(foundedcard);
                        c = new Card(helpf.cardids[realarrindex], type, true);
                        //Console.WriteLine("found " + foundedcard + " in " + textplace);
                        string tmpgold = pricetestfirst((textplace.Split(' '))[(textplace.Split(' ')).Length - 1]);
                        if (!(tmpgold == "")) // && iadder >1
                        { // case: cardnamegold
                            //Console.WriteLine("found " + this.numberregx.Match(tmpgold).Value);
                            price = Convert.ToInt32(this.numberregx.Match(tmpgold).Value);
                        }
                        else if (i < words.Length)
                        {
                            int j = i;
                            tmpgold = pricetest(words[j]);
                            while (tmpgold == "")
                            {
                                if (j + 1 < words.Length)
                                {
                                    j++;
                                    tmpgold = pricetest(words[j]);
                                }
                                else { tmpgold = "fail"; }

                            }

                            if (!(tmpgold == "fail"))
                            { // cardname gold
                                //Console.WriteLine("found gold " + this.numberregx.Match(tmpgold).Value);
                                price = Convert.ToInt32(this.numberregx.Match(tmpgold).Value);
                            }
                        }
                        addingAuctions.Add(new Auction(from, DateTime.Now, currentOfferType, c, msgg,price));
                        //additemtolist(c, from, price, wts, msgg);
                        i--;


                    }//if (find) ende
                }




            }
            return addingAuctions;
        }

        private string pricetestOffer(string price)
        {
            string result = "";
            //Console.WriteLine("PREIS: " + price);
            Match match = priceregexOffer.Match(" " + price + " ");
            if (match.Success) { result = match.Value; }

            return result;
        }

        public List<Offer> GetOffersFromWMessage(string msgg, string from)
        {
            List<Offer> addingOffers = new List<Offer>();
            string msg = Regex.Replace(msgg, @"(<color=#[A-Za-z0-9]{0,6}>)|(</color>)", String.Empty);
            char[] delimiters = new char[] { ' ',',', ';' };
            string[] words = msg.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                Card cardtarget=null;
                int price = 0;
                Card cardoffer=null;
                string word = words[i].ToLower();
                Auction.OfferType offert = Auction.OfferType.BUY;

                if (word.Equals("wtb")) { offert = Auction.OfferType.SELL; }
                else
                { if (word.Equals("wts")) { offert = Auction.OfferType.BUY; } else { continue; } }
                
                i++;

                i++;
                word = words[i].ToLower();
                int arrindex = this.searchscrollsnicks.FindIndex(element => word.Equals(element.nick.Split(' ')[0]));
                int iadder = 0;
                if (arrindex >= 0) // wort in cardlist enthalten
                {
                    //Console.WriteLine(word + " " + arrindex);
                    //string[] possiblecards = Array.FindAll(this.cardnames, element => word.Contains(element.Split(' ')[0]));

                    //search the card you offered
                    List<nickelement> possibnics = this.searchscrollsnicks.FindAll(element => word.Equals(element.nick.Split(' ')[0]));
                    bool findcard = false;
                    string foundedcard = "";
                    string textplace = "";

                    for (int ii = 0; ii < possibnics.Count; ii++)
                    {
                        //string match = possiblecards[ii].ToLower();
                        string match = possibnics[ii].nick.ToLower();
                        int posleng = Math.Min(match.Split(' ').Length, words.Length - i);
                        string searchob = string.Join(" ", words, i, posleng).ToLower();
                        if (searchob.Contains(match)) { findcard = true; foundedcard = possibnics[ii].cardname.ToLower(); iadder = posleng; textplace = searchob; break; };


                    }
                    if (!findcard) continue;
                    i = i + iadder;
                    if (findcard)
                    {
                        Console.WriteLine("targetcard found");
                        CardType type = CardTypeManager.getInstance().get(helpf.cardnamesToID[foundedcard.ToLower()]);
                        int realarrindex = helpf.cardnameToArrayIndex(foundedcard);
                        cardtarget = new Card(helpf.cardids[realarrindex], type, true);
                    }
                    if (words[i].ToLower() != "for") { Console.WriteLine("cant find: for"); break; }
                    i++;
                    string tmpgold = pricetestOffer(words[i]);
                    bool nocard = false;
                    bool twooffers = false;

                    if (tmpgold == "" || tmpgold == null)
                    {
                        Console.WriteLine("cant find tmpgold:" + tmpgold);
                        price = 0;
                    }
                    else
                    {
                        Console.WriteLine(tmpgold);
                        price = Convert.ToInt32(this.numberregxOffer.Match(tmpgold).Value);
                        if (i+1 < words.Length)
                        {
                            i++;
                            if (words[i].ToLower() != "and" && words[i].ToLower() != "or") { Console.WriteLine("no card"); cardoffer = null; nocard = true; }
                            if (words[i].ToLower() == "and") { i++; twooffers = false; } else { if (words[i].ToLower() == "or") { i++; twooffers = true; } }
                        }
                        else 
                        {
                            nocard = true;
                            cardoffer = null;
                            twooffers = false;
                        }

                    }
                    word = words[i].ToLower();
                    if (!nocard)//search offered card
                    {
                        arrindex = this.searchscrollsnicks.FindIndex(element => word.Equals(element.nick.Split(' ')[0]));
                        iadder = 0;
                        if (arrindex >= 0) // wort in cardlist enthalten
                        {
                            //Console.WriteLine(word + " " + arrindex);
                            //string[] possiblecards = Array.FindAll(this.cardnames, element => word.Contains(element.Split(' ')[0]));

                            //search the card you offered
                            possibnics = this.searchscrollsnicks.FindAll(element => word.Equals(element.nick.Split(' ')[0]));
                            findcard = false;
                            foundedcard = "";
                            textplace = "";

                            for (int ii = 0; ii < possibnics.Count; ii++)
                            {
                                //string match = possiblecards[ii].ToLower();
                                string match = possibnics[ii].nick.ToLower();
                                int posleng = Math.Min(match.Split(' ').Length, words.Length - i);
                                string searchob = string.Join(" ", words, i, posleng).ToLower();
                                if (searchob.Contains(match)) { findcard = true; foundedcard = possibnics[ii].cardname.ToLower(); iadder = posleng; textplace = searchob; break; };


                            }
                            i = i + iadder;
                            if (findcard)
                            {
                                Console.WriteLine("found offercard");
                                CardType type = CardTypeManager.getInstance().get(helpf.cardnamesToID[foundedcard.ToLower()]);
                                int realarrindex = helpf.cardnameToArrayIndex(foundedcard);
                                cardoffer = new Card(helpf.cardids[realarrindex], type, true);
                            }
                        }
                    }

                    //create offer/s
                    if (twooffers)
                    { 
                        // offer contains an "or"
                        addingOffers.Add(new Offer(from, DateTime.Now, cardtarget,offert,msgg,price,null));
                        addingOffers.Add(new Offer(from, DateTime.Now, cardtarget,offert, msgg, 0, cardoffer));
 
                    }
                    else
                    {
                        addingOffers.Add(new Offer(from, DateTime.Now, cardtarget,offert, msgg, price, cardoffer));
                    }

                }

            }
            return addingOffers;

        }
        
    }
}
