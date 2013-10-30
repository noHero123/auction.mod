using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class Offer
    {
    public readonly string seller;
        public readonly DateTime time;
        public readonly Auction.OfferType offert;
        public readonly Card cardTarget;
        public readonly Card cardOffer;
        /// <summary>
        /// The price. 0 indicates unknown.
        /// </summary>
        public readonly int price;
        public readonly string message;
        public readonly int calcprice;
        public Offer (String seller, DateTime time, Card c,Auction.OfferType ot ,string mssg) : this(seller, time, c,ot,mssg, 0, null) {
        }
        public Offer(String seller, DateTime time, Card ct, Auction.OfferType ot, string mssg, int price, Card co)
        {
            this.offert = ot;
            this.seller = seller;
            this.time = time;
            this.cardTarget = ct;
            this.cardOffer = co;
            this.price = price;
            this.message = mssg;

            int cardprice = 0;
            if (co != null) {
                int index = Helpfunktions.Instance.cardidToArrayIndex(co.getType());
                cardprice = (int)((Prices.Instance.lowerprice[index]+Prices.Instance.upperprice[index])/2f);
            }
            calcprice = cardprice + price;

        }

        public static int CompareCardName(Offer a1, Offer a2)
        {
            return a1.cardTarget.getName().CompareTo(a2.cardTarget.getName());
        }
        public static int CompareSellerName(Offer a1, Offer a2)
        {
            return a1.seller.CompareTo (a2.seller);
        }
        public static int CompareTime(Offer a1, Offer a2)
        {
            return -a1.time.CompareTo (a2.time); //We want the most recent on the top.
        }
        public static int CompareSellPrice(Offer a1, Offer a2)
        {
            return a1.calcprice.CompareTo(a2.calcprice);
        }
        public static int CompareBuyPrice(Offer a1, Offer a2)
        {
            return -a1.calcprice.CompareTo(a2.calcprice);
        }
        public static Comparison<Offer> getComparison(OfferHouse.SortOfferMode mode)
        {
            switch(mode) {
                case OfferHouse.SortOfferMode.CARD:
                return CompareCardName;
                case OfferHouse.SortOfferMode.SPRICE:
                return CompareSellPrice;
                case OfferHouse.SortOfferMode.BPRICE:
                return CompareBuyPrice;
                case OfferHouse.SortOfferMode.SELLER:
                return CompareSellerName;
                case OfferHouse.SortOfferMode.TIME:
                return CompareTime;
            default:
                throw new ArgumentException ();
            }
        }
    }
}
