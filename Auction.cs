using System;

namespace Auction.mod
{
    public class Auction
    {
        public enum OfferType {
            BUY,SELL
        }
        public readonly string seller;
        public readonly DateTime time;
        public readonly OfferType offer;
        public readonly Card card;
        /// <summary>
        /// The price. 0 indicates unknown.
        /// </summary>
        public int price {get; private set;}
        /// <summary>
        /// The amount offered. 1 indicates unknown.
        /// </summary>
        public readonly int amountOffered;
        public readonly string message;
        public Auction (String seller, DateTime time, OfferType offer, Card c,string mssg) : this(seller, time, offer, c,mssg, 0) {
        }
        public Auction(String seller, DateTime time, OfferType offer, Card c, string mssg, int price)
            : this(seller, time, offer, c,mssg, price, 1)
        {
        }
        public Auction(String seller, DateTime time, OfferType offer, Card card, string mssg, int price, int amountOffered)
        {
            this.seller = seller;
            this.time = time;
            this.offer = offer;
            this.card = card;
            this.price = price;
            this.message = mssg;
            this.amountOffered = amountOffered;
        }

        public static int CompareCardName(Auction a1, Auction a2) {
            return a1.card.getName ().CompareTo (a2.card.getName ());
        }
        public static int CompareSellerName(Auction a1, Auction a2) {
            return a1.seller.CompareTo (a2.seller);
        }
        public static int CompareTime(Auction a1, Auction a2) {
            return -a1.time.CompareTo (a2.time); //We want the most recent on the top.
        }
        public static int ComparePrice(Auction a1, Auction a2) {
            return a1.price.CompareTo (a2.price);
        }
        public static Comparison<Auction> getComparison(AuctionHouse.SortMode mode) {
            switch(mode) {
            case AuctionHouse.SortMode.CARD:
                return CompareCardName;
            case AuctionHouse.SortMode.PRICE:
                return ComparePrice;
            case AuctionHouse.SortMode.SELLER:
                return CompareSellerName;
            case AuctionHouse.SortMode.TIME:
                return CompareTime;
            default:
                throw new ArgumentException ();
            }
        }
        public void setPrice(int p)
        { this.price=p;}

    }
}

