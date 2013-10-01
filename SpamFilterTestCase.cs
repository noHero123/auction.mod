using NUnit.Framework;
using System;

namespace Auction.mod
{
    [TestFixture()]
    public class SpamFilterTestCase
    {
        [Test()]
        public void TestSpamFilterBlock()
        {
            SpamFilter sf = new SpamFilter();
            sf.setSpamTime(new TimeSpan(0, 1, 0));
            DateTime time1 = new DateTime(2013, 1, 1, 0, 0, 0);
            CardType cType = new CardType();
            cType.id = 1;
            Auction aBob1 = new Auction("bob", time1, Auction.OfferType.BUY, new Card(1, cType, true),"");
            Auction aAlice1 = new Auction("alice", new DateTime(2013, 1, 1, 0, 0, 15), Auction.OfferType.BUY, new Card(1, cType, true),"");

            DateTime time2 = new DateTime(2013, 1, 1, 0, 0, 30);
            Auction aBob2 = new Auction("bob", time2, Auction.OfferType.BUY, new Card(1, cType, true),"");
            sf.addAuction(aBob1);
            sf.addAuction(aAlice1);
            sf.addAuction(aBob2);
            Assert.IsFalse(sf.isFilteredBySpamFilter(aBob1)); //Block the newer message and show the older one
            Assert.IsFalse(sf.isFilteredBySpamFilter(aAlice1));
            Assert.IsTrue(sf.isFilteredBySpamFilter(aBob2));
        }



        [Test()]
        public void TestSpamFilterNotBlock()
        {
            SpamFilter sf = new SpamFilter();
            sf.setSpamTime(new TimeSpan(0, 0, 15));
            DateTime time1 = new DateTime(2013, 1, 1, 0, 0, 0);
            CardType cType = new CardType();
            cType.id = 1;
            Auction aBob1 = new Auction("bob", time1, Auction.OfferType.BUY, new Card(1, cType, true),"");
            Auction aAlice1 = new Auction("alice", new DateTime(2013, 1, 1, 0, 0, 15), Auction.OfferType.BUY, new Card(1, cType, true),"");

            DateTime time2 = new DateTime(2013, 1, 1, 0, 0, 30);
            Auction aBob2 = new Auction("bob", time2, Auction.OfferType.BUY, new Card(1, cType, true),"");
            sf.addAuction(aBob1);
            sf.addAuction(aAlice1);
            sf.addAuction(aBob2);
            Assert.IsTrue(sf.isFilteredBySpamFilter(aBob1)); //Block the oldest message and show the new
            Assert.IsFalse(sf.isFilteredBySpamFilter(aAlice1));
            Assert.IsFalse(sf.isFilteredBySpamFilter(aBob2));
        }
        [Test()]
        public void TestSpamFilterDisabled()
        {
            SpamFilter sf = new SpamFilter(); //No SpamTime set => Show everything
            DateTime time1 = new DateTime(2013, 1, 1, 0, 0, 0);
            CardType cType = new CardType();
            cType.id = 1;
            Auction aBob1 = new Auction("bob", time1, Auction.OfferType.BUY, new Card(1, cType, true),"");
            Auction aAlice1 = new Auction("alice", new DateTime(2013, 1, 1, 0, 0, 15), Auction.OfferType.BUY, new Card(1, cType, true),"");

            DateTime time2 = new DateTime(2013, 1, 1, 0, 0, 30);
            Auction aBob2 = new Auction("bob", time2, Auction.OfferType.BUY, new Card(1, cType, true),"");
            sf.addAuction(aBob1);
            sf.addAuction(aAlice1);
            sf.addAuction(aBob2);
            Assert.IsFalse(sf.isFilteredBySpamFilter(aBob1));
            Assert.IsFalse(sf.isFilteredBySpamFilter(aAlice1));
            Assert.IsFalse(sf.isFilteredBySpamFilter(aBob2));
        }
    }
}

