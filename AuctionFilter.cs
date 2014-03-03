using System;
using System.Collections.Generic;
using System.Linq;
namespace Auction.mod
{
    public class AuctionFilter
    {

        public AuctionFilter()
        {
            helpf = Helpfunktions.Instance;
            prcs = Prices.Instance;
            filtersChanged = true;
        }

        Helpfunktions helpf;
        Prices prcs;
        public bool filtersChanged;


        public bool isFiltered(Auction a) {
            return isBeyondPriceRange(a) || isFilteredByCardFilter(a) || isIgnoredSellerName(a) || isFilteredByAmountFilter(a) || isFilteredByRareFilter(a) || isFilteredByResourceFilter(a) ||  isFilteredByTypeCount(a);
        }

        #region RarityFilter
        bool[] rareFilter= { true, true, true }; // false = filtered out
        public void setRarityFilter(bool common, bool uncommon, bool rare) 
        {
            bool[] test = new bool[] { false, false, false };
            rareFilter.CopyTo(test,0);
            rareFilter = new bool[] { common, uncommon, rare };
            
            if (!Enumerable.SequenceEqual(test, rareFilter)) filtersChanged = true;
        }

        public bool isFilteredByRareFilter(Auction a)
        {
            return !rareFilter[a.card.getRarity()];
        }

        #endregion
        #region ResourceFilter
        string[] resourceFilter = { "decay", "energy", "growth", "order" };//""= filtered out 

        public void setResourceFilter(string[] filter)
        {
            if (!Enumerable.SequenceEqual(filter, resourceFilter)) filtersChanged = true;
            filter.CopyTo(resourceFilter,0);
        }
        public bool isFilteredByResourceFilter(Auction a)
        {
            return !resourceFilter.Contains(a.card.getResourceString().ToLower());
        }

        #endregion
        #region AmountFilter
        int amountFilter = 0; //0=no filter, 1= ">0" Filter ; 2=">3" Filter; 3="<3" Filter 

        public void setAmountFilter(int value)
        {
            if (amountFilter != value) filtersChanged = true;
            amountFilter = value;
        }

        public bool isFilteredByAmountFilter(Auction a)
        {
            //if (!helpf.cardIDToNumberOwned.ContainsKey(a.card.getType())) Console.WriteLine("#key is not in " + a.card.getName());
            int anz = helpf.cardIDToNumberOwned[a.card.getType()];
            if (amountFilter == 1 && anz == 0 ) return true;
            if (amountFilter == 2 && anz <= 3) return true;
            if (amountFilter == 3 && anz >= 3) return true;
            return false;
        }
        #endregion

        #region PriceFilter
        bool takeWtsFromGen = false;
        bool takeWtbFromGen = false;
        int priceUpperBound = -1;
        int priceLowerBound = -1;
        bool dontshowScrollsWithNoPrice = false;
        public void setTakeWTS(bool b) { if (this.takeWtsFromGen != b) this.filtersChanged = true; this.takeWtsFromGen = b; }
        public void setTakeWTB(bool b) { if (this.takeWtbFromGen != b) this.filtersChanged = true; this.takeWtbFromGen = b; }
        public void setPriceUpperBound(string upper) {
            int parsed;
            if (!Int32.TryParse(upper,out parsed) || parsed < 0) {
                parsed = -1;
            }
            if (parsed != this.priceUpperBound) {
                this.priceUpperBound = parsed;
                filtersChanged = true;
            }
        }
        public void setPriceLowerBound(string lower) {
            int parsed;
            if (!Int32.TryParse(lower,out parsed) || parsed < 0) {
                parsed = -1;
            }
            if (parsed != this.priceLowerBound) {
                this.priceLowerBound = parsed;
                filtersChanged = true;
            }
        }
        public void setDontShowNoPrice(bool b)
        {
            if (b != dontshowScrollsWithNoPrice) this.filtersChanged = true;
            dontshowScrollsWithNoPrice = b;
        }
        public bool isBeyondPriceRange(Auction a) {
            if (this.dontshowScrollsWithNoPrice && a.price == 0) return true;
            if (a.price == 0) return false;
            if (takeWtbFromGen && prcs.wtbpricelist1[a.card.getType()] != "")
            { if (a.price > Convert.ToInt32(prcs.wtbpricelist1[a.card.getType()])) return true; }

            if (takeWtsFromGen && prcs.wtspricelist1[a.card.getType()] != "")
            { if (a.price < Convert.ToInt32(prcs.wtspricelist1[a.card.getType()])) return true; }

            if (priceLowerBound >= 0 && a.price >0 && priceUpperBound >= 0) {
                return a.price < priceLowerBound || priceUpperBound < a.price;
            } else if (priceLowerBound >= 0 && a.price>0) {
                return a.price < priceLowerBound; //Price is lower than lower Bound
            } else if (priceUpperBound >= 0) {
                return priceUpperBound < a.price; //price is higher than upper bound
            } else {
                return false;
            }
        }
        #endregion

        #region IgnoredSellersFilter
        List<string> ignoredSellers = new List<string>();
        String ignoredSellersString = "";
        public void setIgnoredSellers(String ignoredSellersString) {
            if (!this.ignoredSellersString.Equals (ignoredSellersString)) {
                ignoredSellers.Clear ();
                string[] s = ignoredSellersString.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string seller in s) {
                    ignoredSellers.Add (seller.ToLower ());
                }
                filtersChanged = true;
            }
        }
        private bool isIgnoredSellerName(Auction a) {
            return ignoredSellers.Contains (a.seller.ToLower ());
        }
        #endregion

        #region CardFilter
        CardFilter cardFilter = CardFilter.from("");
        string cardFilterString = "";
        Predicate<int> f=null;
        List<int> cardFilterAmountfilter = new List<int>();
        bool everSetCFAmountFiler = false;

        public void setCardFilterAmountfilter()
        {
            cardFilterAmountfilter.Clear();
            this.everSetCFAmountFiler = true;
            foreach (Card c in cardFilter.getFiltered(this.helpf.allOwnCards))
            {
                if (!cardFilterAmountfilter.Contains(c.getType())) cardFilterAmountfilter.Add(c.getType());
            }
        }

        public void setCardFilter(string cardFilterString) {
            if (!this.cardFilterString.Equals (cardFilterString)) {
                cardFilter = CardFilter.from(cardFilterString);
                this.cardFilterString = cardFilterString;
                this.setCardFilterAmountfilter();
                // amount filter
                /*
                string s = (!(this.cardFilterString == "Search   ")) ? this.cardFilterString : string.Empty;
                string[] array = CardFilter.SplitPairs(s);
                string[] array2 = array;
                this.f = null;
                for (int i = 0; i < array2.Length; i++)
                {
                    string s2 = array2[i];
                    string a;
                    string countString;
                    if (CardFilter.PairToKeyValue(s2, out a, out countString) && a == "#:")
                    {
                        this.f = CardFilter.CreateComparer(countString);
                        break;
                    }
                }*/

                filtersChanged = true;
            }
        }

        private bool isFilteredByCardFilter(Auction a) {
            if (this.everSetCFAmountFiler) return !cardFilterAmountfilter.Contains(a.card.getType());
            return !cardFilter.isIncluded(a.card);

            
        }

        private bool isFilteredByTypeCount(Auction a)
        {
            
            if (f == null)
            {
                return false;
            }

            return !f(helpf.cardIDToNumberOwned[a.card.typeId]);
        }

        #endregion

        public void resetFilters()
        {
            cardFilterString = ""; cardFilter = CardFilter.from("");
            this.setCardFilterAmountfilter();
            ignoredSellersString = ""; ignoredSellers = new List<string>();
            priceUpperBound = -1;
            priceLowerBound = -1;
            amountFilter = 0;
            resourceFilter =  new string[] { "decay", "energy", "growth", "order" };
            rareFilter= new bool[]{ true, true, true };
            this.dontshowScrollsWithNoPrice = false;
            takeWtbFromGen = false;
            takeWtsFromGen = false;
            this.f = null;

            this.filtersChanged = true;
        }
    }
}

