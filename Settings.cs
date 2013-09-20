﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
	enum ScrollsPostPriceType {
		LOWER, SUGGESTED, UPPER
	}

    class Settings
    {
        public bool shownumberscrolls;
        public string rowscalestring = "10";
        public float rowscale = 1.0f;
        public bool showsugrange;
        public bool wtsroundup = true;
        public int wtsroundmode = 0;
        public bool roundwts = false;
        public bool wtbroundup = false;
        public int wtbroundmode = 0;
        public bool roundwtb = false;
		public ScrollsPostPriceType wtsGeneratorPriceType = ScrollsPostPriceType.UPPER, wtbGeneratorPriceType = ScrollsPostPriceType.LOWER;
		public ScrollsPostPriceType wtsAHpriceType = ScrollsPostPriceType.SUGGESTED, wtbAHpriceType = ScrollsPostPriceType.SUGGESTED, wtsAHpriceType2 = ScrollsPostPriceType.SUGGESTED, wtbAHpriceType2 = ScrollsPostPriceType.SUGGESTED;

        public string spampreventtime = "";
        public int spamprevint=0;

        public void loadsettings(string ownaucpath)
        {

            string text = System.IO.File.ReadAllText(ownaucpath + "settingsauc.txt");
            string[] txt = text.Split(';');
            foreach (string t in txt)
            {
                string setting = t.Split(' ')[0];
                string value = "";
                if (t.Split(' ').Length == 2)
                {
                    value = t.Split(' ')[1];
                }
                if (setting.Equals("spam"))
                {
                    spampreventtime = value;
                    if (spampreventtime != "") spamprevint = Convert.ToInt32(spampreventtime);
                    if (spamprevint > 30) { spampreventtime = "30"; spamprevint = 30; }
                }
                if (setting.Equals("numbers"))
                {
                    shownumberscrolls = Convert.ToBoolean(value);
                }
                if (setting.Equals("range"))
                {
                    showsugrange = Convert.ToBoolean(value);
                }
                if (setting.Equals("rowscale"))
                {
                    rowscalestring = value;
                    if (rowscalestring != "") { rowscale = (float)Convert.ToDouble(rowscalestring) / 10f; } else { rowscale = 1.0f; }
                    if (rowscale > 2f) { rowscale = 2f; rowscalestring = "20"; }
                    if (rowscale < 0.5f) { rowscale = .5f; }
                }
                if (setting.Equals("sround"))
                {
                    roundwts = Convert.ToBoolean(value);
                }
                if (setting.Equals("sroundu"))
                {
                    wtsroundup = Convert.ToBoolean(value);
                }
                if (setting.Equals("sroundm"))
                {
                    wtsroundmode = Convert.ToInt32(value);
                }
                if (setting.Equals("bround"))
                {
                    roundwtb = Convert.ToBoolean(value);
                }
                if (setting.Equals("broundu"))
                {
                    wtbroundup = Convert.ToBoolean(value);
                }
                if (setting.Equals("broundm"))
                {
                    wtbroundmode = Convert.ToInt32(value);
                }
                if (setting.Equals("takegens"))
                {
					wtsGeneratorPriceType = (ScrollsPostPriceType)Convert.ToInt32(value);
                }
                if (setting.Equals("takegenb"))
                {
					wtbGeneratorPriceType = (ScrollsPostPriceType)Convert.ToInt32(value);
                }
                if (setting.Equals("takeahs1"))
                {
                    wtsAHpriceType = (ScrollsPostPriceType)Convert.ToInt32(value);
                }
                if (setting.Equals("takeahs2"))
                {
					wtsAHpriceType2 = (ScrollsPostPriceType)Convert.ToInt32(value);
                }
                if (setting.Equals("takeahb1"))
                {
					wtbAHpriceType = (ScrollsPostPriceType)Convert.ToInt32(value);
                }
                if (setting.Equals("takeahb2"))
                {
					wtbAHpriceType2 = (ScrollsPostPriceType)Convert.ToInt32(value);
                }
            }

        }

        public void resetsettings() 
        {
            spampreventtime = "";
            spamprevint = 0;
            shownumberscrolls = false;
            showsugrange = false;
            rowscalestring = "10";
            rowscale = 1f;
            roundwts = false;
            wtsroundup = true;
            wtsroundmode = 0;
            roundwtb = false;
            wtbroundup = false;
            wtbroundmode = 0;
			wtsGeneratorPriceType = ScrollsPostPriceType.SUGGESTED;
			wtbGeneratorPriceType = ScrollsPostPriceType.LOWER;
			wtsAHpriceType = wtsAHpriceType = wtsAHpriceType2 = wtbAHpriceType2 = ScrollsPostPriceType.SUGGESTED;
        }

        public void savesettings(string ownaucpath)
        {
            string text = "";
            text = text + "spam " + spampreventtime + ";";
            text = text + "numbers " + shownumberscrolls.ToString() + ";";
            text = text + "range " + showsugrange.ToString() + ";";
            text = text + "rowscale " + rowscalestring + ";";
            text = text + "sround " + roundwts.ToString() + ";";
            text = text + "sroundu " + wtsroundup.ToString() + ";";
            text = text + "sroundm " + wtsroundmode.ToString() + ";";
            text = text + "bround " + roundwtb.ToString() + ";";
            text = text + "broundu " + wtbroundup.ToString() + ";";
            text = text + "broundm " + wtbroundmode.ToString() + ";";
            text = text + "takegens " + wtsGeneratorPriceType.ToString() + ";";
            text = text + "takegenb " + wtbGeneratorPriceType.ToString() + ";";
            text = text + "takeahs1 " + wtsAHpriceType.ToString() + ";";
            text = text + "takeahs2 " + wtsAHpriceType2.ToString() + ";";
            text = text + "takeahb1 " + wtbAHpriceType.ToString() + ";";
            text = text + "takeahb2 " + wtbAHpriceType2.ToString() + ";";
            System.IO.File.WriteAllText(ownaucpath + "settingsauc.txt", text);
        }


    }
}
