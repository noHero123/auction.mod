using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class listfilters
    {
        public int sortmode = 0;
        Settings sttngs;
        Prices prcs;
        public List<aucitem> allcardsavailable = new List<aucitem>();
        public Dictionary<string, int> available = new Dictionary<string, int>();


        class aucitemnamecomparer : IComparer<aucitem>
        {
            public int Compare(aucitem x, aucitem y)
            {
                return (x.card.getName()).CompareTo(y.card.getName());
            }
        }
        class aucitemgoldcomparer : IComparer<aucitem>
        {
            public int Compare(aucitem x, aucitem y)
            {
                return (x.priceinint).CompareTo(y.priceinint);
            }
        }
        class aucitemsellercomparer : IComparer<aucitem>
        {
            public int Compare(aucitem x, aucitem y)
            {
                return (x.seller).CompareTo(y.seller);
            }
        }

        public listfilters(Settings sets, Prices prs)
        {
            this.sttngs = sets;
            this.prcs = prs;
        }

        public void sortauciteminlist(aucitem ai, List<aucitem> list)
        {
            if (this.sortmode == 0)
            {
                list.Insert(0, ai);
            }
            if (this.sortmode == 1)
            {
                var index = list.BinarySearch(ai, new aucitemnamecomparer());
                if (index < 0) index = ~index;
                list.Insert(index, ai);
            }
            if (this.sortmode == 2)
            {
                var index = list.BinarySearch(ai, new aucitemgoldcomparer());
                if (index < 0) index = ~index;
                list.Insert(index, ai);
            }
            if (this.sortmode == 3)
            {
                var index = list.BinarySearch(ai, new aucitemsellercomparer());
                if (index < 0) index = ~index;
                list.Insert(index, ai);
            }

        }

        public void sortlist(List<aucitem> list)
        {
            //sortmode==0 = sort by date so dont sort wtsfulltimed
            if (sttngs.sortmode == 1)
            {
                list.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });
            }
            if (sttngs.sortmode == 2)
            {
                list.Sort(delegate(aucitem p1, aucitem p2) { return (p1.priceinint).CompareTo(p2.priceinint); });
            }
            if (sttngs.sortmode == 3)
            {
                list.Sort(delegate(aucitem p1, aucitem p2) { return (p1.seller).CompareTo(p2.seller); });
            }
        }


        private void searchlessthan3(List<aucitem> list)
        {
            AucItemFilter.filterList(list, (aucitem a) => (!available.ContainsKey(a.card.getName()) || available[a.card.getName()] < 3));
        }

        public void searchmorethan3(List<aucitem> list)
        {
            AucItemFilter.filterList(list, (aucitem a) => (available.ContainsKey(a.card.getName()) && available[a.card.getName()] > 3));
        }

        public void searchmorethan0(List<aucitem> list)
        {
            AucItemFilter.filterList(list, (aucitem a) => (available.ContainsKey(a.card.getName()) && available[a.card.getName()] > 0));
        }

        public void musthaveprice(List<aucitem> list)
        {
            AucItemFilter.filterList(list, (aucitem a) => (a.priceinint >= 1));
        }

        public void priceishigher(string price, List<aucitem> list)
        {
            // called form wtb-ah
            int priceinint = -1;
            if (price != "") priceinint = Convert.ToInt32(price);
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)//this.orgicardsPlayer1)
            {
                if (card.priceinint == 0 && !sttngs.ignore0)
                {
                    list.Add(card);
                }
                else
                {
                    if (sttngs.takepriceformgenarator)
                    {

                        if (prcs.wtspricelist1[card.card.getName().ToLower()] != "")
                        {
                            //Console.WriteLine(card.card.getName() + " " + wtspricelist1[card.card.getName().ToLower()]);
                            if (card.priceinint >= Convert.ToInt32(prcs.wtspricelist1[card.card.getName().ToLower()])) { list.Add(card); };
                        }
                        else
                        {
                            if (card.priceinint >= priceinint) { list.Add(card); };
                        }


                    }
                    else
                    {
                        if (card.priceinint >= priceinint) { list.Add(card); };
                    }
                }
            }

        }

        public void priceislower(string price, List<aucitem> list)
        {
            //called form wts menu (we want small prices :D )
            int priceinint = int.MaxValue;
            if (price != "") priceinint = Convert.ToInt32(price);

            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)//this.orgicardsPlayer1)
            {

                if (sttngs.takepriceformgenarator)
                {
                    if (prcs.wtbpricelist1[card.card.getName().ToLower()] != "")
                    {
                        if (card.priceinint <= Convert.ToInt32(prcs.wtbpricelist1[card.card.getName().ToLower()])) { list.Add(card); };
                    }
                    else
                    {
                        if (card.priceinint <= priceinint) { list.Add(card); };
                    }

                }
                else
                {
                    if (card.priceinint <= priceinint) { list.Add(card); };
                }
            }

        }

        public void containsseller(string ignoredNames, List<aucitem> list)
        {

            // "contains seller not" should its name be :D
            string[] ignoredSellers = new string[] { ignoredNames };
            if (ignoredNames.Contains(" ")) ignoredSellers = ignoredNames.ToLower().Split(' ');

            AucItemFilter.filterList(list, (aucitem a) => !ignoredSellers.Any(a.seller.ToLower().Equals));
        }

        public void containsname(string name, List<aucitem> list)
        {
            AucItemFilter.filterList(list, (aucitem a) => (a.card.getName().ToLower().Contains(name.ToLower())));
        }

        public void searchforownenergy(string[] resources, List<aucitem> list)
        {
            AucItemFilter.filterList(list, (aucitem a) => (resources.Contains(a.card.getResourceString().ToLower())));
        }

        public void searchforownrarity(int[] rare, List<aucitem> list)
        {
            AucItemFilter.filterList(list, (aucitem a) => (rare.Contains(a.card.getRarity())));
        }

        public void fullupdatelist(List<aucitem> list, List<aucitem> fulllist,bool inauchouse, bool wtsmenue, bool generator)
        {
            list.Clear();
            list.AddRange(fulllist);
            string[] res = { "", "", "", "" };
            if (sttngs.decaybool) { res[0] = "decay"; };
            if (sttngs.energybool) { res[1] = "energy"; };
            if (sttngs.growthbool) { res[2] = "growth"; };
            if (sttngs.orderbool) { res[3] = "order"; };
            int[] rare = { -1, -1, -1 };
            if (sttngs.rarebool) { rare[2] = 2; };
            if (sttngs.uncommonbool) { rare[1] = 1; };
            if (sttngs.commonbool) { rare[0] = 0; };
            if (sttngs.threebool)
            {
                if (inauchouse)
                {
                    if (wtsmenue)
                    {
                        this.searchlessthan3(list);
                    }
                    else
                    {
                        this.searchmorethan3(list);
                    }
                }
                if (generator)
                {
                    if (wtsmenue)
                    {
                        this.searchmorethan3(list);

                    }
                    else
                    {
                        this.searchlessthan3(list);
                    }
                }

            }
            if (sttngs.onebool)
            {
                if (inauchouse && !wtsmenue)
                {
                    this.searchmorethan0(list);
                }

            }
            //this.onlytradeableself();
            if (sttngs.wtssearchstring != "")
            {
                this.containsname(sttngs.wtssearchstring, list);
            }
            if (inauchouse)
            {
                if (sttngs.sellersearchstring != "")
                {
                    this.containsseller(sttngs.sellersearchstring, list);
                }
            }

            if (inauchouse)
            {
                if (sttngs.pricesearchstring != "" || sttngs.takepriceformgenarator)
                {
                    this.priceishigher(sttngs.pricesearchstring, list);
                }
                if (sttngs.pricesearchstring2 != "" || sttngs.takepriceformgenarator)
                {
                    this.priceislower(sttngs.pricesearchstring2, list);

                }

            }



            if (sttngs.ignore0)
            {
                //this.musthaveprice(list);
                this.priceishigher("1", list);
            }

            this.searchforownenergy(res, list);
            this.searchforownrarity(rare, list);

        }




    }
}
