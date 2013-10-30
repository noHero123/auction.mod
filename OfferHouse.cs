using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class OfferHouse
    {

    private static OfferHouse instance;

    public enum SortOfferMode
    {
        TIME, CARD, SPRICE,BPRICE, SELLER
    }

        public static OfferHouse Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OfferHouse();
                }
                return instance;
            }
        }

        private OfferHouse()
        {
            this.helpf = Helpfunktions.Instance;
            this.gen = Generator.Instance;
        }
        int maxLen = 1000;
        Generator gen;
        Helpfunktions helpf;
        public readonly SpamFilter spamFilter = new SpamFilter();
        //protected List<Auction> fullList = new List<Auction>();
        protected List<Auction> fullSellOfferList = new List<Auction>();
        protected List<Auction> sellOfferListFiltered = new List<Auction>();
        protected AuctionHouse.SortMode sellSortMode = AuctionHouse.SortMode.TIME;
        private AuctionHouse.SortMode sellSortModeCopy = AuctionHouse.SortMode.TIME;
        /// <summary>
        /// Gets a value indicating whether this <see cref="Auction.mod.AuctionHouse"/> has unseen sell offers.
        /// </summary>
        /// <value><c>true</c> if new sell offers where added; otherwise, <c>false</c>.</value>
        public bool newSellOffers { get; private set;}
        public bool newOffers { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="Auction.mod.AuctionHouse"/> new unseen buy offers.
        /// </summary>
        /// <value><c>true</c> if new buy offers where added; otherwise, <c>false</c>.</value>
        private int oldOfferId = 0;
        private Auction.OfferType oldoffert = Auction.OfferType.BUY;

        protected List<Offer> fullOfferList = new List<Offer>();
        protected List<Offer> offerListFiltered = new List<Offer>();

        public List<Auction> getSellOffers()
        {
            if (this.newSellOffers ||this.sellSortMode != this.sellSortModeCopy) 
            {
                //sellOfferListFiltered = new List<Auction> (fullSellOfferList);
                sellOfferListFiltered.Clear(); sellOfferListFiltered.AddRange(fullSellOfferList);
                this.sellSortModeCopy = this.sellSortMode;
                sellOfferListFiltered.Sort(Auction.getComparison(sellSortMode));
                this.newSellOffers = false;
            }
            
            return new List<Auction>(sellOfferListFiltered);
        }

        public List<Offer> getOffers(int cardid, Auction.OfferType offert)
        {
           
            if (newOffers || cardid != this.oldOfferId || offert != this.oldoffert)
            {
                offerListFiltered.Clear(); offerListFiltered.AddRange(fullOfferList);
                //this.sellSortModeCopy = this.sellSortMode;
                offerListFiltered.RemoveAll(a => a.cardTarget.getType()!= cardid || a.offert!= offert);
                if(offert==Auction.OfferType.SELL)offerListFiltered.Sort(Offer.getComparison(OfferHouse.SortOfferMode.BPRICE));
                if (offert == Auction.OfferType.BUY) offerListFiltered.Sort(Offer.getComparison(OfferHouse.SortOfferMode.SPRICE));


                this.oldOfferId = cardid;
                this.oldoffert = offert;
            }
            newOffers = false;
            
            return new List<Offer>(offerListFiltered);
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
        
        public void addAuctions(List<Auction> list) {
            list.ForEach (addAuction);
            if (fullSellOfferList.Count > maxLen) { fullSellOfferList.RemoveRange(maxLen, fullSellOfferList.Count - maxLen); }

            //fullSellOfferList.Sort (Auction.getComparison(sellSortMode));
            sellOfferListFiltered.Sort (Auction.getComparison(sellSortMode));

        }
        private void addAuction(Auction a)
        {
            int oldprice = 0;
            bool deleted = false;
            for (int i=0;i < fullSellOfferList.Count;i++)
            {
                Auction au = fullSellOfferList[i];
                if (au.card.getType() == a.card.getType() && au.offer==a.offer)
                {
                    fullSellOfferList.Remove(au);
                    sellOfferListFiltered.Remove(au);
                    oldprice = au.price;
                    deleted = true;
                    i--;
                }
            }
            if (!deleted)
            {
                fullSellOfferList.Insert(0, a);
                sellOfferListFiltered.Insert(0, a);
            }
            else
            {
                int price=a.price;
                if (a.offer==Auction.OfferType.SELL && oldprice > price) price = oldprice;
                if (a.offer == Auction.OfferType.BUY && oldprice < price) price = oldprice;
                Auction aa = new Auction(a.seller, a.time, a.offer, a.card, a.message, price, a.amountOffered);
                fullSellOfferList.Insert(0, aa);
                sellOfferListFiltered.Insert(0, aa);
            }
                newSellOffers = true;
        }

        public void addOffers(List<Offer> list)
        {
            list.ForEach(addOffer);

        }


        private void addOffer(Offer a)
        {
            // test if the offer wants a card you dont own
            if (a.offert == Auction.OfferType.BUY && a.cardOffer != null)
            { // you want to buy a card, the offer contains a card, the seller wants.. but you dont may have this card 
                List<Auction> yourowncards = gen.getAllOwnSellOffers();
                bool ownthiscard = false;
                foreach (Auction auau in yourowncards)
                {
                    if (auau.card.tradable && auau.card.getType() == a.cardOffer.getType())
                    {
                        ownthiscard = true;
                    }
                }
                if (!ownthiscard) return; // you dont own this card ;_;
            }

            Auction aua=null;
            bool found = false;
            for (int i = 0; i < fullSellOfferList.Count; i++)
            {
                Auction au= fullSellOfferList[i];
                Console.WriteLine("offertypes"+ au.offer.ToString() +" " +  a.offert.ToString());
                if (au.card.getType() == a.cardTarget.getType() && au.offer == a.offert)
                {
                    found = true;
                }
                if (au.offer == Auction.OfferType.SELL && au.card.getType() == a.cardTarget.getType() && au.offer == a.offert && au.price < a.calcprice)
                {
                    fullSellOfferList.Remove(au);
                    sellOfferListFiltered.Remove(au);
                    aua = new Auction(au.seller, DateTime.Now, au.offer, au.card, au.message, a.calcprice, au.amountOffered);
                    break;
                    
                }
                if (au.offer == Auction.OfferType.BUY && au.card.getType() == a.cardTarget.getType() && au.offer == a.offert && (au.price > a.calcprice || (au.price==0 && a.calcprice>=1)))
                {
                    fullSellOfferList.Remove(au);
                    sellOfferListFiltered.Remove(au);
                    aua = new Auction(au.seller, DateTime.Now, au.offer, au.card, au.message, a.calcprice, au.amountOffered);
                    break;

                }
            }
            
            if (found) 
            {
                fullOfferList.Insert(0, a);
                newOffers = true;
                if (aua == null)
                {
                    // send update to the offerer, he could be new
                    string s = "auction update wts " + helpf.cardidsToCardnames[a.cardOffer.getType()] + " " + a.calcprice;
                    if (a.offert == Auction.OfferType.BUY) { s = "auction update wtb " + helpf.cardidsToCardnames[a.cardOffer.getType()] + " " + a.calcprice; }
                    WhisperMessage wmsg = new WhisperMessage(a.seller, s);
                    App.Communicator.sendRequest(wmsg);

                }
            }
            if (aua != null)
            {
                fullSellOfferList.Insert(0, aua);
                sellOfferListFiltered.Insert(0, aua);
                newSellOffers = true;
                sendAuctionUpdate(aua.offer, aua.price, aua.card.getType());

                
            }


        }

        private void sendAuctionUpdate(Auction.OfferType aot, int newprice, int id)
        {
            string s = "auction update wts " + helpf.cardidsToCardnames[id] +" " + newprice;
            if (aot == Auction.OfferType.BUY) { s = "auction update wtb " + helpf.cardidsToCardnames[id] + " " + newprice; }
            WhisperMessage wmsg = new WhisperMessage("", s);
            List<string> alreadySended = new List<string>();
            foreach (Offer o in fullOfferList)
            {
                if (o.cardTarget.getType() == id && o.offert == aot && !alreadySended.Contains(o.seller))
                {
                    wmsg.toProfileName = o.seller;
                    App.Communicator.sendRequest(wmsg);
                    alreadySended.Add(o.seller);
                };
            }
        }


        public void removeEntry(int id, Auction.OfferType ao)
        {
            // send auction done to all listener
            string s = "auction ended wts " + helpf.cardidsToCardnames[id];
            if (ao == Auction.OfferType.BUY) { s = "auction ended wtb " + helpf.cardidsToCardnames[id]; }
            WhisperMessage wmsg = new WhisperMessage("",s);
            List<string> alreadySended = new List<string>();
            foreach (Offer o in fullOfferList)
            {
                if (o.cardTarget.getType() == id && o.offert == ao && !alreadySended.Contains(o.seller))
                {
                    wmsg.toProfileName = o.seller;
                    App.Communicator.sendRequest(wmsg);
                    alreadySended.Add(o.seller);
                };
            }

            fullSellOfferList.RemoveAll(a => a.card.getType() == id && a.offer==ao);
            sellOfferListFiltered.RemoveAll(a => a.card.getType() == id && a.offer == ao);
            fullOfferList.RemoveAll(a => a.cardTarget.getType() == id && a.offert == ao);
            
        }

        public void removeOldEntrys()
        {
            DateTime n = DateTime.Now;
                fullSellOfferList.RemoveAll(a => (n.Subtract(a.time)).TotalMinutes >= helpf.deleteTime);
                sellOfferListFiltered.RemoveAll(a => (n.Subtract(a.time)).TotalMinutes >= helpf.deleteTime);
        }

        public static bool isOfferMessage(WhisperMessage wmsg)
        {
            return (wmsg.text.ToLower()).StartsWith("wtb your ") || (wmsg.text.ToLower()).StartsWith("wts my ") || (wmsg.text.ToLower()).StartsWith("auction ended wts ") || (wmsg.text.ToLower()).StartsWith("auction ended wtb ") || (wmsg.text.ToLower()).StartsWith("auction update wts ") || (wmsg.text.ToLower()).StartsWith("auction update wtb ");
        }

    }
}
