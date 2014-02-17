using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    public class PlayerStore
    {

        private static PlayerStore instance;

        public static PlayerStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerStore();
                }
                return instance;
            }
        }

        private PlayerStore()
        {
            this.helpf = Helpfunktions.Instance; 
            sellOfferFilter = new AuctionFilter();
            createCardsFilter = new AuctionFilter();
            this.prcs = Prices.Instance;
            this.sttngs = Settings.Instance;
        }

        int maxLen = 1000;
        Helpfunktions helpf;
        public AuctionFilter sellOfferFilter;
        public AuctionFilter createCardsFilter;
        protected List<Auction> fullSellOfferList = new List<Auction>();
        protected List<Auction> sellOfferListFiltered = new List<Auction>();
        protected List<Auction> createOfferListFiltered = new List<Auction>();
        protected AuctionHouse.SortMode sellSortMode = AuctionHouse.SortMode.TIME;
        private AuctionHouse.SortMode sellSortModeCopy = AuctionHouse.SortMode.TIME;
        public bool newSellOffers { get; private set; }
        private Settings sttngs;
        private Prices prcs;


        public List<Auction> getSellOffers()
        {
            newSellOffers = false;
            if (sellOfferFilter.filtersChanged || this.sellSortMode != this.sellSortModeCopy)
            {
                //sellOfferListFiltered = new List<Auction> (fullSellOfferList);
                sellOfferListFiltered.Clear(); 
                sellOfferListFiltered.AddRange(fullSellOfferList);
                sellOfferListFiltered.RemoveAll(sellOfferFilter.isFiltered);
                sellOfferFilter.filtersChanged = false;
                this.sellSortModeCopy = this.sellSortMode;
                sellOfferListFiltered.Sort(Auction.getComparison(sellSortMode));
            }

            return new List<Auction>(sellOfferListFiltered);
        }

        public List<Auction> getCreateOffers()
        {
            if (createCardsFilter.filtersChanged)
            {
                //sellOfferListFiltered = new List<Auction> (fullSellOfferList);
                createOfferListFiltered.Clear();
                createOfferListFiltered.AddRange(Generator.Instance.getAllOwnSellOffers());
                foreach (Auction c in this.createOfferListFiltered)
                {

                    int index = helpf.cardidToArrayIndex(c.card.getType());
                    if(index>=1)c.setPrice(prcs.getPrice(index, sttngs.wtbAHpriceType));
                    if (c.price == 0) c.setPrice(1);

                }
                createOfferListFiltered.RemoveAll(createCardsFilter.isFiltered);
                createCardsFilter.filtersChanged = false;
                createOfferListFiltered.Sort(Auction.getComparison(AuctionHouse.SortMode.CARD));
            }
            return new List<Auction>(createOfferListFiltered);
        }

        public List<Auction> getOwnOffers()
        {
            List<Auction> sellOwnOfferListFiltered = new List<Auction>();
            foreach (Auction x in this.fullSellOfferList)
            {
                //if (x.message == "sold" || (x.message.Split(';')[3] == App.MyProfile.ProfileInfo.id && x.time < DateTime.Now )) sellOwnOfferListFiltered.Add(x);
                if (x.message.Split(';')[3] == App.MyProfile.ProfileInfo.id ) sellOwnOfferListFiltered.Add(x);

            }

            sellOwnOfferListFiltered.Sort(Auction.getComparison(sellSortMode));

                return new List<Auction>(sellOwnOfferListFiltered);
        }


        public void setSellSortMode(AuctionHouse.SortMode sortMode)
        {
            this.sellSortMode = sortMode;
        }
        public void setSellSortMode(int sortint)
        {
            if (sortint == 1) this.sellSortMode = AuctionHouse.SortMode.CARD;
            if (sortint == 2) this.sellSortMode = AuctionHouse.SortMode.PRICE;
            if (sortint == 3) this.sellSortMode = AuctionHouse.SortMode.SELLER;
            if (sortint == 0) this.sellSortMode = AuctionHouse.SortMode.TIME;
        }

        public void addAuctions(List<Auction> list)
        {
            list.ForEach(addAuction);
            if (fullSellOfferList.Count > maxLen) { fullSellOfferList.RemoveRange(maxLen, fullSellOfferList.Count - maxLen); this.sellOfferFilter.filtersChanged = true; }
            sellOfferListFiltered.Sort(Auction.getComparison(sellSortMode));
        }

        private void addAuction(Auction a)
        {
            if (a.offer == Auction.OfferType.SELL)
            {
                fullSellOfferList.Insert(0, a);

                if (!sellOfferFilter.isFiltered(a))
                {
                    sellOfferListFiltered.Insert(0, a);
                    newSellOffers = true;
                }
            }
        }

        public void removeMessages(string seller, Auction.OfferType aot, string cname)
        {
            fullSellOfferList.RemoveAll(a => a.seller.Equals(seller) && a.card.getType() == helpf.cardnamesToID[cname] && a.offer == aot);
            sellOfferListFiltered.RemoveAll(a => a.seller.Equals(seller) && a.card.getType() == helpf.cardnamesToID[cname] && a.offer == aot);
            
        }

        public void removeAllMessages()
        {
            fullSellOfferList.Clear();
            sellOfferListFiltered.Clear();

        }

        public void removeSeller(string seller)
        {
            fullSellOfferList.RemoveAll(a => a.seller.Equals(seller));
            sellOfferListFiltered.RemoveAll(a => a.seller.Equals(seller));
        }
        

        public void removeOldEntrys()
        {
            DateTime n = DateTime.Now;
            fullSellOfferList.RemoveAll(a => (a.time < n) && (a.message.Split(';')[3] != App.MyProfile.ProfileInfo.id));
            sellOfferListFiltered.RemoveAll(a => (a.time < n) && (a.message.Split(';')[3] != App.MyProfile.ProfileInfo.id));
            
        }



    }
}
