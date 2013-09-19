using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
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
        public int takewtsgenint = 2, takewtbgenint = 0;
        public int takewtsahint = 1, takewtbahint = 1, takewtsahint2 = 1, takewtbahint2 = 1;

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
                    takewtsgenint = Convert.ToInt32(value);
                }
                if (setting.Equals("takegenb"))
                {
                    takewtbgenint = Convert.ToInt32(value);
                }
                if (setting.Equals("takeahs1"))
                {
                    takewtsahint = Convert.ToInt32(value);
                }
                if (setting.Equals("takeahs2"))
                {
                    takewtsahint2 = Convert.ToInt32(value);
                }
                if (setting.Equals("takeahb1"))
                {
                    takewtbahint = Convert.ToInt32(value);
                }
                if (setting.Equals("takeahb2"))
                {
                    takewtbahint2 = Convert.ToInt32(value);
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
            takewtsgenint = 2;
            takewtbgenint = 0;
            takewtsahint = 1;
            takewtsahint = 1;
            takewtsahint2 = 1;
            takewtbahint2 = 1;
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
            text = text + "takegens " + takewtsgenint.ToString() + ";";
            text = text + "takegenb " + takewtbgenint.ToString() + ";";
            text = text + "takeahs1 " + takewtsahint.ToString() + ";";
            text = text + "takeahs2 " + takewtsahint2.ToString() + ";";
            text = text + "takeahb1 " + takewtbahint.ToString() + ";";
            text = text + "takeahb2 " + takewtbahint2.ToString() + ";";
            System.IO.File.WriteAllText(ownaucpath + "settingsauc.txt", text);
        }


    }
}
