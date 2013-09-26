using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Auction.mod
{
    class Messageparser
    {


        private List<Auction> addingwtscards = new List<Auction>();
        private List<Auction> addingwtbcards = new List<Auction>();



        AuctionHouse ah;
        Helpfunktions helpf;
        //Listfilters lstfltrs;
        //Auclists alist;
        Settings sttngs;
        private Regex priceregex=new Regex(@".*[^x0-9]+[0-9]{2,9}[g]?[^x0-9]+.*");
        private Regex priceregexpriceonname = new Regex(@"[^x0-9]{2,}[0-9]{2,9}[g]?[^x0-9]+.*");
        Regex numberregx = new Regex(@"[0-9]{2,9}");

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
            this.ah = AuctionHouse.Instance;
            this.sttngs=Settings.Instance;
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


        private void addcardstolist()
        {
            if (addingwtscards.Count() > 0)
            {
                //addingwtscards.Reverse();
                string seller = addingwtscards[0].seller;
                //ah.removeSeller(seller);


            }

            if (addingwtbcards.Count() > 0)
            {
                //addingwtbcards.Reverse();
                string seller = addingwtbcards[0].seller;
                //ah.removeBuyer(seller);
            }
            //add auctions to ah-lists
            ah.addAuctions(this.addingwtscards);
            ah.addAuctions(this.addingwtbcards);

           
        }

        private void additemtolist(Card c, string from, int gold, bool wts, string wholemsg)
        {
            /*
            // spampreventor
            if (wts)
            {
                if (alist.wtslistfulltimed.FindIndex(element => element.seller.Equals(from) && (element.whole.ToLower().Contains("wts") || (element.whole.ToLower().Contains("sell")))) >= 0)
                {
                    aucitem aii = (alist.wtslistfulltimed.Find(element => element.seller.Equals(from) && (element.whole.ToLower().Contains("wts") || (element.whole.ToLower().Contains("sell")))));
                    if (aii.whole.Equals(wholemsg) && sttngs.spampreventtime != "" && (aii.dtime).CompareTo(DateTime.Now.AddMinutes(-1 * sttngs.spamprevint)) > 0) { return; }
                }
            }
            else
            {
                if (alist.wtblistfulltimed.FindIndex(element => element.seller.Equals(from) && (element.whole.ToLower().Contains("wtb") || (element.whole.ToLower().Contains("buy")))) >= 0)
                {
                    aucitem aii = (alist.wtblistfulltimed.Find(element => element.seller.Equals(from) && (element.whole.ToLower().Contains("wtb") || (element.whole.ToLower().Contains("buy")))));
                    if (aii.whole.Equals(wholemsg) && sttngs.spampreventtime != "" && (aii.dtime).CompareTo(DateTime.Now.AddMinutes(-1 * sttngs.spamprevint)) > 0) { return; }
                }
            }
            */

            /*
            aucitem ai = new aucitem();
            ai.card = c;
            ai.seller = from;
            ai.priceinint = gold;
            ai.price = gold.ToString();
            ai.time = DateTime.Now.ToString("hh:mm:ss tt");//DateTime.Now.ToShortTimeString();
            ai.dtime = DateTime.Now;
            ai.whole = wholemsg;
            if (gold == 0) ai.price = "?";
             */
            if (wts)
            {
                this.addingwtscards.Add(new Auction(from,DateTime.Now,Auction.OfferType.SELL,c,wholemsg,gold));
            }
            else
            {

                this.addingwtbcards.Add(new Auction(from, DateTime.Now, Auction.OfferType.BUY, c, wholemsg, gold));
            }

        }

        private void getaucitemsformshortmsg(string msg, string from, string room)
        {


            bool aucbtoo = false;
            bool wts = true;
            string secmsg = "";
            if (msg.StartsWith("aucs "))
            {
                wts = true;
                msg = msg.Remove(0, 5);
                if (msg.Contains("aucb "))
                {
                    aucbtoo = true;
                    secmsg = (msg.Split(new string[] { "aucb " }, StringSplitOptions.None))[1];
                    msg = (msg.Split(new string[] { "aucb " }, StringSplitOptions.None))[0];
                }
            }
            if (msg.StartsWith("aucb "))
            {
                wts = false;
                msg = msg.Remove(0, 5);


                if (msg.Contains("aucs "))
                {
                    wts = true;
                    aucbtoo = true;
                    secmsg = (msg.Split(new string[] { "aucs " }, StringSplitOptions.None))[0];
                    msg = (msg.Split(new string[] { "aucs " }, StringSplitOptions.None))[1];
                }

            }
            //Console.WriteLine(msg + "##" + secmsg);

            string[] words = msg.Split(';');
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
                if (ids.Contains(","))
                {
                    string[] ideen = ids.Split(',');
                    foreach (string idd in ideen)
                    {
                        int id = Convert.ToInt32(idd);
                        CardType type = CardTypeManager.getInstance().get(id);
                        Card card = new Card(id, type, true);
                        additemtolist(card, from, Convert.ToInt32(price), wts, "");
                    }

                }
                else
                {
                    int id = Convert.ToInt32(ids);
                    CardType type = CardTypeManager.getInstance().get(id);
                    Card card = new Card(id, type, true);
                    additemtolist(card, from, Convert.ToInt32(price), wts, "");

                }



            }

            if (aucbtoo)
            {
                wts = false;
                words = secmsg.Split(';');
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "" || words[i] == " ") break;
                    string price = words[i].Split(' ')[1];
                    if (price == "") { price = "0"; }
                    string ids = words[i].Split(' ')[0];
                    if (ids.Contains(","))
                    {
                        string[] ideen = ids.Split(',');
                        foreach (string idd in ideen)
                        {
                            int id = Convert.ToInt32(idd);
                            CardType type = CardTypeManager.getInstance().get(id);
                            Card card = new Card(id, type, true);
                            additemtolist(card, from, Convert.ToInt32(price), wts, "");
                        }

                    }
                    else
                    {
                        int id = Convert.ToInt32(ids);
                        CardType type = CardTypeManager.getInstance().get(id);
                        Card card = new Card(id, type, true);
                        additemtolist(card, from, Convert.ToInt32(price), wts, "");

                    }

                }

            }

            addcardstolist();
        }

        public void getaucitemsformmsg(string msgg, string from, string room)
        {
            string msg = Regex.Replace(msgg, @"(<color=#[A-Za-z0-9]{0,6}>)|(</color>)", String.Empty);
            this.addingwtbcards.Clear();
            this.addingwtscards.Clear();
            // todo: delete old msgs from author
            if (msg.StartsWith("aucs ") || msg.StartsWith("aucb ")) { getaucitemsformshortmsg(msg, from, room); return; }
            //if (msg.StartsWith("aucc ")) { respondtocommand(msg,from); return; }
            bool wts = true; ;
            //string[] words=msg.Split(' ');

            char[] delimiters = new char[] { '\r', '\n', ' ', ',', ';' };
            string[] words = msg.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            //words = Regex.Split(msg, @"");

            if (!msg.ToLower().Contains("wts") && !msg.ToLower().Contains("wtb") && !msg.ToLower().Contains("sell") && !msg.ToLower().Contains("buy")) return;
            bool wtxfound = false;

            for (int i = 0; i < words.Length; i++)
            {
                Card c; int price = 0;
                string word = words[i].ToLower();
                // save in wts or wtb?
                if (word.Contains("wts") || word.Contains("sell")) { wts = true; wtxfound = true; }
                if (word.Contains("wtb") || word.Contains("buy")) { wts = false; wtxfound = true; }
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
                        {   // case: cardnamegold
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
                        additemtolist(c, from, price, wts, msgg);
                        i--;


                    }//if (find) ende



                }




            }

            addcardstolist();

        }

        
        
    }
}
