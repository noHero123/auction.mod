using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Auction.mod
{
    class SettingsUI
    {

        Messageparser mssgprsr;
        Auclists alists;
        Rectomat recto;
        Listfilters lstfltrs;
        Prices prcs;
        Cardviewer crdvwr;
        Searchsettings srchsvr;
        Network ntwrk;
        Settings sttngs;
        Helpfunktions helpf;

        public SettingsUI(Messageparser mssgprsr, Auclists alists, Rectomat recto, Listfilters lstfltrs, Prices prcs, Cardviewer crdvwr, Searchsettings srchsvr, Network ntwrk, Settings sttngs, Helpfunktions h)
        {
            this.helpf = h;
            this.mssgprsr = mssgprsr;
            this.alists = alists;
            this.recto = recto;
            this.lstfltrs = lstfltrs;
            this.prcs = prcs;
            this.crdvwr = crdvwr;
            this.srchsvr = srchsvr;
            this.ntwrk = ntwrk;
            this.sttngs = sttngs;
        }

       public void setbuttonpressed()
       {
           //this.hideInformation();
           helpf.hideInformationinfo.Invoke(helpf.storeinfo, null);
           iTween.MoveTo((GameObject)helpf.buymen.GetValue(helpf.storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
           helpf.showBuyinfo.SetValue(helpf.storeinfo, false);
           ((GameObject)helpf.sellmen.GetValue(helpf.storeinfo)).SetActive(false);
           iTween.MoveTo((GameObject)helpf.sellmen.GetValue(helpf.storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
           ((GameObject)helpf.sellmen.GetValue(helpf.storeinfo)).SetActive(true);
           helpf.showSellinfo.SetValue(helpf.storeinfo, false);
           Store.ENABLE_SHARD_PURCHASES = false;
           helpf.inauchouse = false;
           helpf.generator = false;
           helpf.settings = true;
           helpf.targetchathightinfo.SetValue(helpf.target, (float)Screen.height * 0.25f);
       }


        public void drawsettings()
        {
            GUI.depth = 15;
            GUI.color = Color.white;
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.settingRect, string.Empty);
            GUI.skin = helpf.cardListPopupLeftButtonSkin;
            if (GUI.Button(recto.setreset, "Reset"))
            {
                sttngs.resetsettings();

                recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);


            }
            if (GUI.Button(recto.setload, "Load"))
            {
                sttngs.loadsettings(helpf.ownaucpath);
                recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
            }
            if (GUI.Button(recto.setsave, "Save"))
            {
                //save stuff
                sttngs.savesettings(helpf.ownaucpath);


            }

            // spam preventor
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.Label(recto.setpreventspammlabel, "dont update messages which are younger than:");
            GUI.Label(recto.setpreventspammlabel2, "minutes");

            GUI.Box(recto.setpreventspammrect, "");
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.setpreventspammrect, string.Empty);
            helpf.chatLogStyle.alignment = TextAnchor.MiddleCenter;
            sttngs.spampreventtime = Regex.Replace(GUI.TextField(recto.setpreventspammrect, sttngs.spampreventtime, helpf.chatLogStyle), @"[^0-9]", "");
            helpf.chatLogStyle.alignment = TextAnchor.MiddleLeft;
            if (sttngs.spampreventtime != "") sttngs.spamprevint = Convert.ToInt32(sttngs.spampreventtime);
            if (sttngs.spamprevint > 30) { sttngs.spampreventtime = "30"; sttngs.spamprevint = 30; }

            //anz cards
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.Label(recto.setowncardsanzlabel, "show owned number of scrolls beside scrollname");
            bool owp = GUI.Button(recto.setowncardsanzbox, "");
            if (owp) sttngs.shownumberscrolls = !sttngs.shownumberscrolls;
            if (sttngs.shownumberscrolls)
            {
                GUI.DrawTexture(recto.setowncardsanzbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else
            {
                GUI.DrawTexture(recto.setowncardsanzbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }

            // show range
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.Label(recto.setsugrangelabel, "show ScrollsPost price as range");
            bool oowp = GUI.Button(recto.setsugrangebox, "");
            if (oowp) sttngs.showsugrange = !sttngs.showsugrange;
            if (sttngs.showsugrange)
            {
                GUI.DrawTexture(recto.setsugrangebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else
            {
                GUI.DrawTexture(recto.setsugrangebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }

            GUI.Label(recto.setrowhightlabel, "scale row hight by factor");
            GUI.Label(recto.setrowhightlabel2, "/10");
            GUI.Box(recto.setrowhightbox, "");
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.setrowhightbox, string.Empty);
            helpf.chatLogStyle.alignment = TextAnchor.MiddleCenter;
            string rowcopy = sttngs.rowscalestring;
            sttngs.rowscalestring = Regex.Replace(GUI.TextField(recto.setrowhightbox, sttngs.rowscalestring, helpf.chatLogStyle), @"[^0-9]", "");
            helpf.chatLogStyle.alignment = TextAnchor.MiddleLeft;
            if (sttngs.rowscalestring != "") { sttngs.rowscale = (float)Convert.ToDouble(sttngs.rowscalestring) / 10f; } else { sttngs.rowscale = 1.0f; }
            if (sttngs.rowscale > 2f) { sttngs.rowscale = 2f; sttngs.rowscalestring = "20"; }
            if (sttngs.rowscale < 0.5f) { sttngs.rowscale = .5f; }
            if (!rowcopy.Equals(sttngs.rowscalestring)) { recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin); }

            //round wts


            bool ooowp = GUI.Button(recto.setwtsbox, "");
            if (ooowp) sttngs.roundwts = !sttngs.roundwts;
            if (sttngs.roundwts)
            {
                GUI.DrawTexture(recto.setwtsbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else
            {
                GUI.DrawTexture(recto.setwtsbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.Label(recto.setwtslabel1, "round ScrollsPost prices in WTS-generator ");
            if (GUI.Button(recto.setwtsbutton1, ""))
            {
                sttngs.wtsroundup = !sttngs.wtsroundup;
            }
            GUI.Label(recto.setwtslabel2, " to next ");
            if (GUI.Button(recto.setwtsbutton2, ""))
            {
                sttngs.wtsroundmode = (sttngs.wtsroundmode + 1) % 3;
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (sttngs.wtsroundup) { GUI.Label(recto.setwtsbutton1, "up"); } else { GUI.Label(recto.setwtsbutton1, "down"); }
            if (sttngs.wtsroundmode == 0) { GUI.Label(recto.setwtsbutton2, "5"); }
            if (sttngs.wtsroundmode == 1) { GUI.Label(recto.setwtsbutton2, "10"); }
            if (sttngs.wtsroundmode == 2) { GUI.Label(recto.setwtsbutton2, "50"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            //round wtb


            bool ooowwp = GUI.Button(recto.setwtbbox, "");
            if (ooowwp) sttngs.roundwtb = !sttngs.roundwtb;
            if (sttngs.roundwtb)
            {
                GUI.DrawTexture(recto.setwtbbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else
            {
                GUI.DrawTexture(recto.setwtbbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.Label(recto.setwtblabel1, "round ScrollsPost prices in WTB-generator ");
            if (GUI.Button(recto.setwtbbutton1, ""))
            {
                sttngs.wtbroundup = !sttngs.wtbroundup;
            }
            GUI.Label(recto.setwtblabel2, " to next ");
            if (GUI.Button(recto.setwtbbutton2, ""))
            {
                sttngs.wtbroundmode = (sttngs.wtbroundmode + 1) % 3;
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (sttngs.wtbroundup) { GUI.Label(recto.setwtbbutton1, "up"); } else { GUI.Label(recto.setwtbbutton1, "down"); }
            if (sttngs.wtbroundmode == 0) { GUI.Label(recto.setwtbbutton2, "5"); }
            if (sttngs.wtbroundmode == 1) { GUI.Label(recto.setwtbbutton2, "10"); }
            if (sttngs.wtbroundmode == 2) { GUI.Label(recto.setwtbbutton2, "50"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            //take price generator
            GUI.Label(recto.settakewtsgenlabel, "WTS-Generator takes ");
            if (GUI.Button(recto.settakewtsgenbutton, ""))
            {
                sttngs.takewtsgenint = (sttngs.takewtsgenint + 1) % 3;
            }
            GUI.Label(recto.settakewtsgenlabel2, "ScrollsPost price");
            GUI.Label(recto.settakewtbgenlabel, "WTB-Generator takes ");
            if (GUI.Button(recto.settakewtbgenbutton, ""))
            {
                sttngs.takewtbgenint = (sttngs.takewtbgenint + 1) % 3;
            }
            GUI.Label(recto.settakewtbgenlabel2, "ScrollsPost price");
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (sttngs.takewtsgenint == 0) { GUI.Label(recto.settakewtsgenbutton, "lower"); }
            if (sttngs.takewtsgenint == 1) { GUI.Label(recto.settakewtsgenbutton, "sugg."); }
            if (sttngs.takewtsgenint == 2) { GUI.Label(recto.settakewtsgenbutton, "upper"); }
            if (sttngs.takewtbgenint == 0) { GUI.Label(recto.settakewtbgenbutton, "lower"); }
            if (sttngs.takewtbgenint == 1) { GUI.Label(recto.settakewtbgenbutton, "sugg."); }
            if (sttngs.takewtbgenint == 2) { GUI.Label(recto.settakewtbgenbutton, "upper"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            //show price ah
            if (sttngs.showsugrange)
            {
                GUI.Label(recto.setwtsahlabel, "show in WTS-AH the ");
                if (GUI.Button(recto.setwtsahbutton, ""))
                {
                    sttngs.takewtsahint = (sttngs.takewtsahint + 1) % 3;
                }
                if (GUI.Button(recto.setwtsahbutton2, ""))
                {
                    sttngs.takewtsahint2 = (sttngs.takewtsahint2 + 1) % 3;
                }
                GUI.Label(recto.setwtsahlabel3, "and");
                GUI.Label(recto.setwtsahlabel4, "ScrollsPost prices");
                GUI.Label(recto.setwtbahlabel, "show in WTB-AH the ");
                if (GUI.Button(recto.setwtbahbutton, ""))
                {
                    sttngs.takewtbahint = (sttngs.takewtbahint + 1) % 3;
                }
                if (GUI.Button(recto.setwtbahbutton2, ""))
                {
                    sttngs.takewtbahint2 = (sttngs.takewtbahint2 + 1) % 3;
                }
                GUI.Label(recto.setwtbahlabel3, "and");
                GUI.Label(recto.setwtbahlabel4, "ScrollsPost prices");
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                if (sttngs.takewtsahint == 0) { GUI.Label(recto.setwtsahbutton, "lower"); }
                if (sttngs.takewtsahint == 1) { GUI.Label(recto.setwtsahbutton, "sugg."); }
                if (sttngs.takewtsahint == 2) { GUI.Label(recto.setwtsahbutton, "upper"); }
                if (sttngs.takewtbahint == 0) { GUI.Label(recto.setwtbahbutton, "lower"); }
                if (sttngs.takewtbahint == 1) { GUI.Label(recto.setwtbahbutton, "sugg."); }
                if (sttngs.takewtbahint == 2) { GUI.Label(recto.setwtbahbutton, "upper"); }
                if (sttngs.takewtsahint2 == 0) { GUI.Label(recto.setwtsahbutton2, "lower"); }
                if (sttngs.takewtsahint2 == 1) { GUI.Label(recto.setwtsahbutton2, "sugg."); }
                if (sttngs.takewtsahint2 == 2) { GUI.Label(recto.setwtsahbutton2, "upper"); }
                if (sttngs.takewtbahint2 == 0) { GUI.Label(recto.setwtbahbutton2, "lower"); }
                if (sttngs.takewtbahint2 == 1) { GUI.Label(recto.setwtbahbutton2, "sugg."); }
                if (sttngs.takewtbahint2 == 2) { GUI.Label(recto.setwtbahbutton2, "upper"); }
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            else
            {
                GUI.Label(recto.setwtsahlabel, "show in WTS-AH the ");
                if (GUI.Button(recto.setwtsahbutton, ""))
                {
                    sttngs.takewtsahint = (sttngs.takewtsahint + 1) % 3;
                }
                GUI.Label(recto.setwtsahlabel2, "ScrollsPost price");
                GUI.Label(recto.setwtbahlabel, "show in WTB-AH the ");
                if (GUI.Button(recto.setwtbahbutton, ""))
                {
                    sttngs.takewtbahint = (sttngs.takewtbahint + 1) % 3;
                }
                GUI.Label(recto.setwtbahlabel2, "ScrollsPost price");
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                if (sttngs.takewtsahint == 0) { GUI.Label(recto.setwtsahbutton, "lower"); }
                if (sttngs.takewtsahint == 1) { GUI.Label(recto.setwtsahbutton, "sugg."); }
                if (sttngs.takewtsahint == 2) { GUI.Label(recto.setwtsahbutton, "upper"); }
                if (sttngs.takewtbahint == 0) { GUI.Label(recto.setwtbahbutton, "lower"); }
                if (sttngs.takewtbahint == 1) { GUI.Label(recto.setwtbahbutton, "sugg."); }
                if (sttngs.takewtbahint == 2) { GUI.Label(recto.setwtbahbutton, "upper"); }
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }

            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
        }

        
        
    }
}
