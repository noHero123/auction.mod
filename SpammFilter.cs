using System;
using System.Collections.Generic;
namespace Auction.mod
{


        public class AuctionEqualityComparer : IEqualityComparer<Auction>
        {
            public AuctionEqualityComparer()
            {
            }

            public bool Equals(Auction a1, Auction a2)
            {
                return a1.offer.Equals(a2.offer) && a1.price.Equals(a2.price) && a1.seller.Equals(a2.seller) && a1.card.getCardType().id.Equals(a2.card.getCardType().id);
            }


            public int GetHashCode(Auction a)
            {
                int hCode = (((251 * a.seller.GetHashCode()) + a.card.getType()) * 251 + a.price) * 251 + a.offer.GetHashCode();
                return hCode.GetHashCode();
            }
        }


    public class SpamFilter
    {
        Dictionary<Auction, List<DateTime>> whenThisAuctionHasBeenPosted;
        TimeSpan spamTime = TimeSpan.Zero;
        public SpamFilter()
        {
            whenThisAuctionHasBeenPosted = new Dictionary<Auction, List<DateTime>>(new AuctionEqualityComparer());
        }

        public void disableSpamFilter()
        {
            this.spamTime = TimeSpan.Zero;
        }
        public void setSpamTime(TimeSpan spamTime)
        {
            this.spamTime = spamTime;
        }
        public void addAuction(Auction a)
        {
            List<DateTime> timesTheSameAuctionHasBeenPosted;
            if (!whenThisAuctionHasBeenPosted.TryGetValue(a, out timesTheSameAuctionHasBeenPosted))
            {
                //Auction has never been posted before
                List<DateTime> list = new List<DateTime>();
                list.Add(a.time);
                whenThisAuctionHasBeenPosted.Add(a, list);
            }
            else
            {
                DateTime lastTimeTheAuctionHasBeenPosted = timesTheSameAuctionHasBeenPosted[0];
                if (lastTimeTheAuctionHasBeenPosted < a.time)
                {
                    timesTheSameAuctionHasBeenPosted.Insert(0, a.time);
                    if (timesTheSameAuctionHasBeenPosted.Count > 2)
                    {
                        //Only keep the last 2 entrys
                        timesTheSameAuctionHasBeenPosted.RemoveAt(2);
                    }
                }
                else
                {
                    throw new ArgumentException("Can only add auctions to the spamfilter in a time-sorted manner");
                }
            }
        }

        public bool isFilteredBySpamFilter(Auction a)
        {
            if (spamTime.Equals(TimeSpan.Zero))
                return false;
            List<DateTime> timesTheSameAuctionHasBeenPosted;
            if (!whenThisAuctionHasBeenPosted.TryGetValue(a, out timesTheSameAuctionHasBeenPosted))
            {
                //Have never seen this one before => no spam
                return false; //should not happen, tho...
            }
            if (timesTheSameAuctionHasBeenPosted.Count == 1)
            {
                return false; //Only one auction => no spam
            }
            else if (timesTheSameAuctionHasBeenPosted.Count > 1)
            {
                if (timesTheSameAuctionHasBeenPosted[1] > a.time)
                {
                    return true; //Don't show auctions that are older than the last 2 auctions
                }
                TimeSpan diffBetweenLastToPosts = timesTheSameAuctionHasBeenPosted[0].Subtract(timesTheSameAuctionHasBeenPosted[1]);
                if (diffBetweenLastToPosts > spamTime)
                {
                    //Filter the older of the equal Auctions
                    return a.time < timesTheSameAuctionHasBeenPosted[0];
                }
                else
                {
                    //Filter the newer auctions
                    return a.time > timesTheSameAuctionHasBeenPosted[1];
                }
            }
            else
            {
                return false; //Should not happen, as we do not have empty lists in the dict...
            }
        }
    }
}