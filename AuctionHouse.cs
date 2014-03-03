using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Auction.mod
{
    public class AuctionHouse
    {
        public enum SortMode {
            TIME, CARD, PRICE, SELLER
        }

    private static AuctionHouse instance;

        public static AuctionHouse Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AuctionHouse();
                }
                return instance;
            }
        }

        private AuctionHouse()
        {
            this.helpf = Helpfunktions.Instance; 
            buyOfferFilter = new AuctionFilter();
            sellOfferFilter = new AuctionFilter();
        }
        int maxLen = 1000;
        Helpfunktions helpf;
        public AuctionFilter sellOfferFilter;
        public AuctionFilter buyOfferFilter;
        public readonly SpamFilter spamFilter = new SpamFilter();
        //protected List<Auction> fullList = new List<Auction>();
        protected List<Auction> fullSellOfferList = new List<Auction>();
        protected List<Auction> fullBuyOfferList = new List<Auction>();
        protected List<Auction> sellOfferListFiltered = new List<Auction>();
        protected List<Auction> buyOfferListFiltered = new List<Auction>();
        protected SortMode sellSortMode = SortMode.TIME;
        protected SortMode buySortMode = SortMode.TIME;
        private SortMode sellSortModeCopy = SortMode.TIME;
        private SortMode buySortModeCopy = SortMode.TIME;
        /// <summary>
        /// Gets a value indicating whether this <see cref="Auction.mod.AuctionHouse"/> has unseen sell offers.
        /// </summary>
        /// <value><c>true</c> if new sell offers where added; otherwise, <c>false</c>.</value>
        public bool newSellOffers { get; private set;}
        /// <summary>
        /// Gets a value indicating whether this <see cref="Auction.mod.AuctionHouse"/> new unseen buy offers.
        /// </summary>
        /// <value><c>true</c> if new buy offers where added; otherwise, <c>false</c>.</value>
        public bool newBuyOffers { get; private set;}

        private void updateCardFilter()
        {  // the own cards have changed, we have to update the CardFilter (because the amountfilter needs our own cards)
            sellOfferFilter.setCardFilterAmountfilter();
            buyOfferFilter.setCardFilterAmountfilter();
            this.helpf.auctionHouseAllCardsChanged = false;
        }

        public List<Auction> getBuyOffers() {
            newBuyOffers = false;
            if (this.helpf.auctionHouseAllCardsChanged) this.updateCardFilter();
            if (buyOfferFilter.filtersChanged || this.buySortMode != this.buySortModeCopy)
            {
                buyOfferListFiltered = new List<Auction> (fullBuyOfferList);// is even refiltered if sortmode changed, so order issnt changed at sort-change
                buyOfferListFiltered.RemoveAll (buyOfferFilter.isFiltered);
                buyOfferFilter.filtersChanged = false;

                this.buySortModeCopy = this.buySortMode;
                buyOfferListFiltered.Sort(Auction.getComparison(buySortMode));
                buyOfferListFiltered.RemoveAll(spamFilter.isFilteredBySpamFilter);
            }
            
            return new List<Auction>(buyOfferListFiltered);
        }

        public List<Auction> getSellOffers()
        {
            newSellOffers = false;
            if (this.helpf.auctionHouseAllCardsChanged) this.updateCardFilter();
            if (sellOfferFilter.filtersChanged||this.sellSortMode != this.sellSortModeCopy) 
            {
                //sellOfferListFiltered = new List<Auction> (fullSellOfferList);
                sellOfferListFiltered.Clear(); sellOfferListFiltered.AddRange(fullSellOfferList);
                sellOfferListFiltered.RemoveAll (sellOfferFilter.isFiltered);
                sellOfferFilter.filtersChanged = false;
                this.sellSortModeCopy = this.sellSortMode;
                sellOfferListFiltered.Sort(Auction.getComparison(sellSortMode));
                sellOfferListFiltered.RemoveAll(spamFilter.isFilteredBySpamFilter);
            }
            
            return new List<Auction>(sellOfferListFiltered);
        }

        
        public void setBuySortMode(SortMode sortMode) {
            this.buySortMode = sortMode;
        }
        public void setBuySortMode(int sortint)
        {
          if (sortint == 1)  this.buySortMode = AuctionHouse.SortMode.CARD;
          if (sortint == 2) this.buySortMode = AuctionHouse.SortMode.PRICE;
          if (sortint == 3) this.buySortMode = AuctionHouse.SortMode.SELLER;
          if (sortint == 0) this.buySortMode = AuctionHouse.SortMode.TIME;
        }
        
        public void setSellSortMode(SortMode sortMode)
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
        
        public void addAuctions(List<Auction> list) {
            list.ForEach (addAuction);
            if (fullBuyOfferList.Count > maxLen) { fullBuyOfferList.RemoveRange(maxLen, fullBuyOfferList.Count - maxLen); this.buyOfferFilter.filtersChanged = true; }
            if (fullSellOfferList.Count > maxLen) { fullSellOfferList.RemoveRange(maxLen, fullSellOfferList.Count - maxLen); this.sellOfferFilter.filtersChanged = true; }

            //fullBuyOfferList.Sort (Auction.getComparison(buySortMode));
            buyOfferListFiltered.Sort (Auction.getComparison(buySortMode));
            //fullSellOfferList.Sort (Auction.getComparison(sellSortMode));
            sellOfferListFiltered.Sort (Auction.getComparison(sellSortMode));

        }
        private void addAuction(Auction a)
        {
            //Console.WriteLine("add auction with time" + a.time.ToLongTimeString() + a.time.Millisecond);
            spamFilter.addAuction(a);
            if (a.offer == Auction.OfferType.BUY)
            {
                fullBuyOfferList.Insert(0, a);

                if (!buyOfferFilter.isFiltered(a) && !this.spamFilter.isFilteredBySpamFilter(a))
                {
                    buyOfferListFiltered.Insert(0, a);
                    newBuyOffers = true;
                }
            }
            else if (a.offer == Auction.OfferType.SELL)
            {
                fullSellOfferList.Insert(0, a);

                if (!sellOfferFilter.isFiltered(a) && !this.spamFilter.isFilteredBySpamFilter(a))
                {
                    sellOfferListFiltered.Insert(0, a);
                    newSellOffers = true;
                }
            }
        }

        public void removeMessages(string seller, Auction.OfferType aot, string cname)
        {
            fullSellOfferList.RemoveAll(a => a.seller.Equals(seller)&& a.card.getType() == helpf.cardnamesToID[cname] && a.offer==aot);
            sellOfferListFiltered.RemoveAll(a => a.seller.Equals(seller) && a.card.getType() == helpf.cardnamesToID[cname] && a.offer == aot);
            fullBuyOfferList.RemoveAll(a => a.seller.Equals(seller) && a.card.getType() == helpf.cardnamesToID[cname] && a.offer == aot);
            buyOfferListFiltered.RemoveAll(a => a.seller.Equals(seller) && a.card.getType() == helpf.cardnamesToID[cname] && a.offer == aot);
        }

        public void removeSeller(string seller)
        {
            fullSellOfferList.RemoveAll(a => a.seller.Equals(seller));
            sellOfferListFiltered.RemoveAll(a => a.seller.Equals(seller));
        }
        public void removeBuyer(string buyer)
        {
            fullBuyOfferList.RemoveAll(a => a.seller.Equals(buyer));
            buyOfferListFiltered.RemoveAll(a => a.seller.Equals(buyer));
        }

        public void removeOldEntrys()
        {
            DateTime n = DateTime.Now;
                fullSellOfferList.RemoveAll(a => (n.Subtract(a.time)).TotalMinutes >= helpf.deleteTime);
                sellOfferListFiltered.RemoveAll(a => (n.Subtract(a.time)).TotalMinutes >= helpf.deleteTime);
                fullBuyOfferList.RemoveAll(a => (n.Subtract(a.time)).TotalMinutes >= helpf.deleteTime);
                buyOfferListFiltered.RemoveAll(a => (n.Subtract(a.time)).TotalMinutes >= helpf.deleteTime);
        }

    }
}

