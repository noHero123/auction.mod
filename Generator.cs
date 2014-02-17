using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class Generator
    {

         private static Generator instance;

        public static Generator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Generator();
                }
                return instance;
            }
        }


        private Generator()
        {
            helpf = Helpfunktions.Instance;
            prcs = Prices.Instance;
            this.mssgprsr = Messageparser.Instance;

            sellOwnCardsFilter = new AuctionFilter();
            buyOwnCardsFilter = new AuctionFilter();
        }

        Helpfunktions helpf;
        Prices prcs;
        Messageparser mssgprsr;

        //public List<Auction> allcardsavailable = new List<Auction>(); //=buyOwnListFiltered
        

        public readonly AuctionFilter sellOwnCardsFilter;
        public readonly AuctionFilter buyOwnCardsFilter;
        protected List<Auction> fullSellOwnList = new List<Auction>();
        protected List<Auction> fullBuyOwnList = new List<Auction>();
        protected List<Auction> sellOwnListFiltered = new List<Auction>();
        protected List<Auction> buyOwnListFiltered = new List<Auction>();
        protected AuctionHouse.SortMode sortMode = AuctionHouse.SortMode.CARD;

        public void setSellSortMode(int sortint)
        {
            if (sortint == 1) this.sortMode = AuctionHouse.SortMode.CARD;
            if (sortint == 2) this.sortMode = AuctionHouse.SortMode.PRICE;
            if (sortint == 3) this.sortMode = AuctionHouse.SortMode.SELLER;
            if (sortint == 0) this.sortMode = AuctionHouse.SortMode.TIME;
        }

        public List<Auction> getOwnBuyOffers() {
            if (buyOwnCardsFilter.filtersChanged)
            {
                buyOwnListFiltered = new List<Auction> (fullBuyOwnList);
                buyOwnListFiltered.RemoveAll (buyOwnCardsFilter.isFiltered);
                buyOwnCardsFilter.filtersChanged = false;
            }
            return buyOwnListFiltered;
        }
        public List<Auction> getOwnSellOffers() {
            if (sellOwnCardsFilter.filtersChanged) {
                sellOwnListFiltered = new List<Auction> (fullSellOwnList);
                sellOwnListFiltered.RemoveAll(sellOwnCardsFilter.isFiltered);
                sellOwnCardsFilter.filtersChanged = false;
            }
            return sellOwnListFiltered;
        }
        public List<Auction> getAllOwnBuyOffers() { return fullBuyOwnList; }
        public List<Auction> getAllOwnSellOffers() { return fullSellOwnList; }


        public void setowncards(Message msg)
        {
            List<Card> orgicardsPlayer = new List<Card>();
            this.fullSellOwnList.Clear();
            PlayerStore.Instance.createCardsFilter.filtersChanged = true;
            orgicardsPlayer.AddRange(((LibraryViewMessage)msg).cards);
            List<string> checklist = new List<string>();
            helpf.cardIDToNumberOwned.Clear();
            Console.WriteLine("add cards to cardIDToNumberOwned");
            foreach (Auction ai in this.fullBuyOwnList) //fullbuyownlist == all cards in game
            {
                if (!helpf.cardIDToNumberOwned.ContainsKey(ai.card.getType()))
                {
                    helpf.cardIDToNumberOwned.Add(ai.card.getType(), 0);
                }
            }

            foreach (Card c in orgicardsPlayer)
            {
                if (c.tradable && !(checklist.Contains(helpf.cardidsToCardnames[c.getType()])))
                {
                    Auction ai = new Auction(App.MyProfile.ProfileInfo.name,DateTime.Now,Auction.OfferType.SELL,c,"");
                    this.fullSellOwnList.Add(ai);
                    checklist.Add(helpf.cardidsToCardnames[c.getType()]);
                }


                helpf.cardIDToNumberOwned[c.getType()] = helpf.cardIDToNumberOwned[c.getType()] + 1;
            }

            this.fullSellOwnList.Sort(delegate(Auction p1, Auction p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });

            prcs.wtspricelist1.Clear();
            for (int i = 0; i < fullSellOwnList.Count; i++)
            {
                prcs.wtspricelist1.Add(fullSellOwnList[i].card.getType(), "");

            }

            this.sellOwnCardsFilter.filtersChanged = true;
        

        }

        public void setallavailablecards(Message msg)
        {
            // get all available cards, save them!
            helpf.setarrays(msg);

            prcs.resetarrays(helpf.cardids.Length);
            if (helpf.nicks) helpf.readnicksfromfile();
            mssgprsr.searchscrollsnicks.Clear();
            prcs.wtbpricelist1.Clear();
            this.fullBuyOwnList.Clear();
            Console.WriteLine("add cards to fullbuyownList");
            for (int j = 0; j < helpf.cardnames.Length; j++)
            {
                prcs.wtbpricelist1.Add(helpf.cardids[j], "");
                CardType type = CardTypeManager.getInstance().get(helpf.cardids[j]);
                Card card = new Card(helpf.cardids[j], type, true);
                //aucitem ai = new aucitem();
                //ai.card = card;
                //ai.price = "";
                //ai.priceinint = helpf.allcardsavailable.Count;
                //ai.seller = "me";
                Auction ai = new Auction(" ", DateTime.Now, Auction.OfferType.BUY, card,"");
                if (App.MyProfile.ProfileInfo.name != null) { ai = new Auction(App.MyProfile.ProfileInfo.name, DateTime.Now, Auction.OfferType.BUY, card,""); }
                this.fullBuyOwnList.Add(ai);
                nickelement nele;
                nele.nick = helpf.cardnames[j];
                nele.cardname = helpf.cardnames[j];
                mssgprsr.searchscrollsnicks.Add(nele);
            };
            mssgprsr.searchscrollsnicks.AddRange(helpf.loadedscrollsnicks);

            this.fullBuyOwnList.Sort(delegate(Auction p1, Auction p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });
            this.buyOwnCardsFilter.filtersChanged = true;
            prcs.totalpricecheck();//helpf.cardids

            // set cardIDToNumberOwned to zeros, or the mod will crash, if you got an offer, but dont visit the deckbuilder /store etc (where you get your own cards) before
            helpf.cardIDToNumberOwned.Clear();
            Console.WriteLine("zeros cardIDToNumberOwned");
            foreach (Auction ai in this.fullBuyOwnList) //fullbuyownlist == all cards in game
            {
                if (!helpf.cardIDToNumberOwned.ContainsKey(ai.card.getType()))
                {
                    helpf.cardIDToNumberOwned.Add(ai.card.getType(), 0);
                }
            }

        }


    }
}

