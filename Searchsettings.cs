using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class Searchsettings
    {
        class settingcopy
        {
            public bool boolean0;
            public bool boolean1;
            public bool boolean2;
            public bool boolean3;
            public bool boolean4;
            public bool boolean5;
            public bool boolean6;
            public bool boolean7;
            public bool boolean8;
            public bool boolean9;
            public bool boolean10;
            public string strings0;
            public string strings1;
            public string strings2;
            public string strings3;
            public string strings4;
            public int sorting;
            public bool sortreverse;
        }

        private settingcopy ahwtssettings = new settingcopy();
        private settingcopy ahwtbsettings = new settingcopy();
        private settingcopy genwtssettings = new settingcopy();
        private settingcopy genwtbsettings = new settingcopy();

        public bool growthbool=true;
        public bool orderbool=true;
        public bool energybool=true;
        public bool decaybool=true;
        public bool commonbool=true;
        public bool uncommonbool=true;
        public bool rarebool=true;
        public bool threebool=false;
        public bool onebool=false;
        public bool ignore0=false;
        public bool takepriceformgenarator=false;
        public string timesearchstring = "";
        public string wtssearchstring = "";
        public string sellersearchstring = "";
        public string pricesearchstring = "";
        public string pricesearchstring2 = "";
        public int sortmode = 0;
        public bool reverse = false;
        public string shortgeneratedwtsmessage = "";
        public string shortgeneratedwtbmessage = "";
        public string generatedwtsmessage = "";
        public string generatedwtbmessage = "";

        public Searchsettings()
        {
            this.growthbool = true;
            this.orderbool = true;
            this.energybool = true;
            this.decaybool = true;
            this.commonbool = true;
            this.uncommonbool = true;
            this.rarebool = true;
            this.threebool = false;
            this.onebool = false;
            this.ignore0 = false;
            this.wtssearchstring = "";
            this.sellersearchstring = "";
            this.pricesearchstring = "";
            this.timesearchstring = "";
            this.pricesearchstring2 = "";
            this.sortmode = 0;
            this.reverse = false;
            this.takepriceformgenarator = false;
        }

        public void resetsearchsettings()
        {
            this.growthbool = true;
            this.orderbool = true;
            this.energybool = true;
            this.decaybool = true;
            this.commonbool = true;
            this.uncommonbool = true;
            this.rarebool = true;
            this.threebool = false;
            this.onebool = false;
            this.ignore0 = false;
            this.wtssearchstring = "";
            this.sellersearchstring = "";
            this.pricesearchstring = "";
            this.timesearchstring = "";
            this.pricesearchstring2 = "";
            this.takepriceformgenarator = false;
        }

        public void resetgensearchsettings(bool wts)
        {
            this.wtssearchstring = "";
            this.pricesearchstring = "";
            this.sellersearchstring = "";
            this.growthbool = true;
            this.orderbool = true;
            this.energybool = true;
            this.decaybool = true;
            this.commonbool = true;
            this.uncommonbool = true;
            this.rarebool = true;
            this.threebool = false;
            this.onebool = false;
            if (wts)
            {
                this.generatedwtsmessage = "";
                this.shortgeneratedwtsmessage = "";
            }
            else
            {
                this.generatedwtbmessage = "";
                this.shortgeneratedwtbmessage = "";
            }
        }

        public void saveall()
        {
            this.savesettings(true, true);
            this.savesettings(true, false);
            this.savesettings(false, true);
            this.savesettings(false, false);
        }

        public void savesettings(bool ah, bool wts)
        {
            settingcopy copy=new settingcopy() ;
            if (ah && wts) copy= this.ahwtssettings;
            if (ah && !wts) copy = this.ahwtbsettings;
            if (!ah && wts) copy = this.genwtssettings;
            if (!ah && !wts) copy = this.genwtbsettings;
            copy.boolean0 = growthbool;
            copy.boolean1 = orderbool;
            copy.boolean2 = energybool;
            copy.boolean3 = decaybool;
            copy.boolean4 = commonbool;
            copy.boolean5 = uncommonbool;
            copy.boolean6 = rarebool;
            copy.boolean7 = threebool;
            copy.boolean8 = onebool;
            copy.boolean9 = ignore0;
            copy.boolean10 = takepriceformgenarator;
            copy.strings0 = wtssearchstring;
            copy.strings1 = sellersearchstring;
            copy.strings2 = pricesearchstring;//shortwts/wtbstring
            copy.strings3 = timesearchstring;
            copy.strings4 = pricesearchstring2;
            copy.sorting = sortmode;
            copy.sortreverse = reverse;
        }

        public void setsettings(bool ah, bool wts)
        {
            settingcopy copy = new settingcopy();
            if (ah && wts) copy = this.ahwtssettings;
            if (ah && !wts) copy = this.ahwtbsettings;
            if (!ah && wts) copy = this.genwtssettings;
            if (!ah && !wts) copy = this.genwtbsettings;
            growthbool = copy.boolean0;
            orderbool = copy.boolean1;
            energybool = copy.boolean2;
            decaybool = copy.boolean3;
            commonbool = copy.boolean4;
            uncommonbool = copy.boolean5;
            rarebool = copy.boolean6;
            threebool = copy.boolean7;
            onebool = copy.boolean8;
            ignore0 = copy.boolean9;
            takepriceformgenarator = copy.boolean10;
            wtssearchstring = copy.strings0;
            sellersearchstring = copy.strings1;
            pricesearchstring = copy.strings2;
            timesearchstring = copy.strings3;
            pricesearchstring2 = copy.strings4;
            sortmode = copy.sorting;
            reverse = copy.sortreverse;

        }

        public string getshortgenmsg(bool wts)
        {
            settingcopy copy=new settingcopy();
            if ( wts) copy = this.genwtssettings;
            if (!wts) copy = this.genwtbsettings;
            return copy.strings2;
        
        }

    }
}
