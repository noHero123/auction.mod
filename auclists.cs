using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class auclists
    {
        public List<aucitem> wtslistfull = new List<aucitem>();
        public List<aucitem> wtblistfull = new List<aucitem>();
        public List<aucitem> wtslistfulltimed = new List<aucitem>();
        public List<aucitem> wtblistfulltimed = new List<aucitem>();
        public List<aucitem> wtslist = new List<aucitem>();
        public List<aucitem> wtblist = new List<aucitem>();

        public List<aucitem> ahlist = new List<aucitem>();
        public List<aucitem> ahlistfull = new List<aucitem>();
        private List<aucitem> wtsPlayer = new List<aucitem>();
        private List<aucitem> orgicardsPlayerwountrade = new List<aucitem>(); // cards player owns minus the untradable cards
        private List<Card> orgicardsPlayer = new List<Card>(); // all cards the player owns
        private List<aucitem> wtbPlayer = new List<aucitem>();
        listfilters lstfltrs;
        Prices prcs;
        Searchsettings srchsvr;

        public auclists( listfilters l,Prices p, Searchsettings s)
        {
            this.lstfltrs = l;
            this.prcs = p;
            this.srchsvr = s;

        }

        public void setowncards(Message msg,bool inauchouse, bool generator, bool wtsmenue)
        {
            this.orgicardsPlayer.Clear();
            this.orgicardsPlayerwountrade.Clear();
            this.orgicardsPlayer.AddRange(((LibraryViewMessage)msg).cards);
            List<string> checklist = new List<string>();
            lstfltrs.available.Clear();
            foreach (aucitem ai in lstfltrs.allcardsavailable)
            {
                if (!lstfltrs.available.ContainsKey(ai.card.getName()))
                {
                    lstfltrs.available.Add(ai.card.getName(), 0);
                }
            }

            foreach (Card c in orgicardsPlayer)
            {
                if (c.tradable && !(checklist.Contains(c.getName())))
                {
                    aucitem ai = new aucitem();
                    ai.card = c;
                    ai.seller = "me";
                    ai.price = "";
                    ai.priceinint = orgicardsPlayerwountrade.Count;
                    this.orgicardsPlayerwountrade.Add(ai);
                    checklist.Add(c.getName());
                }


                lstfltrs.available[c.getName()] = lstfltrs.available[c.getName()] + 1;
            }

            this.orgicardsPlayerwountrade.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });

            prcs.wtspricelist1.Clear();
            for (int i = 0; i < orgicardsPlayerwountrade.Count; i++)
            {
                prcs.wtspricelist1.Add(orgicardsPlayerwountrade[i].card.getName().ToLower(), "");

            }


            if (generator && wtsmenue)
            {
                lstfltrs.fullupdatelist(ahlist, ahlistfull, inauchouse,wtsmenue, generator);
            }
                        
        
        }



        public void setAhlistsToGenWtsLists()
        {
            this.ahlist = this.wtsPlayer; 
            this.ahlistfull = this.orgicardsPlayerwountrade;
            srchsvr.setsettings(false, true);
            lstfltrs.fullupdatelist(this.ahlist, this.ahlistfull, false, true, true);
        }

        public void setAhlistsToGenWtbLists()
        {
            this.ahlist = this.wtbPlayer; this.ahlistfull = lstfltrs.allcardsavailable;
            srchsvr.setsettings(false, false);
            lstfltrs.fullupdatelist(this.ahlist, this.ahlistfull, false, false, true);
        }

        public void setAhlistsToAHWtsLists(bool sortlists)
        {
            this.ahlist = this.wtslist; this.ahlistfull = this.wtslistfull;
            srchsvr.setsettings(true, true);
            if (sortlists) lstfltrs.sortlist(this.ahlistfull);
            lstfltrs.fullupdatelist(this.ahlist, this.ahlistfull, true, true, false);
        }
        public void setAhlistsToAHWtbLists(bool sortlists)
        {
            this.ahlist = this.wtblist; this.ahlistfull = this.wtblistfull;
            srchsvr.setsettings(true, false);
            if (sortlists) lstfltrs.sortlist(this.ahlistfull);
            lstfltrs.fullupdatelist(this.ahlist, this.ahlistfull, true, false, false);
        }

    }
}
