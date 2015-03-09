﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Auction.mod
{
    class SettingsUI
    {

		private static ScrollsPostPriceType nextScrollsPostPriceType(ScrollsPostPriceType type) {
			return (ScrollsPostPriceType)(((int)type + 1) % 4);
		}
		private static string scrollsPostPriceTypeToString(ScrollsPostPriceType type) {
			switch(type){
			case ScrollsPostPriceType.LOWER: return "lower";
			case ScrollsPostPriceType.SUGGESTED: return "sugg.";
			case ScrollsPostPriceType.UPPER: return "upper";
            case ScrollsPostPriceType.BLACKMARKET: return "BM";
			default: throw new ArgumentException();
			}
		}

        Messageparser mssgprsr;
        Rectomat recto;
        Prices prcs;
        Cardviewer crdvwr;
        Searchsettings srchsvr;
        Settings sttngs;
        Helpfunktions helpf;

        private static SettingsUI instance;

        public static SettingsUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsUI();
                }
                return instance;
            }
        }

        private SettingsUI()
        {
            this.helpf = Helpfunktions.Instance;
            this.mssgprsr = Messageparser.Instance;
            this.recto = Rectomat.Instance;
            this.prcs = Prices.Instance;
            this.crdvwr = Cardviewer.Instance;
            this.srchsvr = Searchsettings.Instance;
            this.sttngs = Settings.Instance;
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

                if (helpf.bothmenue || helpf.createAuctionMenu) recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                else recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);


            }
            if (GUI.Button(recto.setload, "Load"))
            {
                sttngs.loadsettings(helpf.ownaucpath,helpf.deleteTime);

                if (helpf.bothmenue || helpf.createAuctionMenu) recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                else recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
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
            if (sttngs.spamprevint > helpf.deleteTime) { sttngs.spampreventtime = ((int)helpf.deleteTime).ToString(); sttngs.spamprevint = (int)helpf.deleteTime; }

            //anz cards
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.Label(recto.setowncardsanzlabel, "show owned number of scrolls ahead scrollname");
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
            GUI.Label(recto.setsugrangelabel, "show ScrollsGuide price as range");
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
            if (!rowcopy.Equals(sttngs.rowscalestring))
            {
                if (helpf.bothmenue || helpf.createAuctionMenu) recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                else recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
            }

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
            GUI.Label(recto.setwtslabel1, "round ScrollsGuide prices in WTS-generator ");
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
            GUI.Label(recto.setwtblabel1, "round ScrollsGuide prices in WTB-generator ");
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
				sttngs.wtsGeneratorPriceType = nextScrollsPostPriceType(sttngs.wtsGeneratorPriceType);
            }
            GUI.Label(recto.settakewtsgenlabel2, "ScrollsGuide price");
            GUI.Label(recto.settakewtbgenlabel, "WTB-Generator takes ");
            if (GUI.Button(recto.settakewtbgenbutton, ""))
            {
				sttngs.wtbGeneratorPriceType = nextScrollsPostPriceType(sttngs.wtbGeneratorPriceType);
            }
            GUI.Label(recto.settakewtbgenlabel2, "ScrollsGuide price");
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.Label(recto.settakewtsgenbutton, scrollsPostPriceTypeToString(sttngs.wtsGeneratorPriceType));
			GUI.Label(recto.settakewtbgenbutton, scrollsPostPriceTypeToString(sttngs.wtbGeneratorPriceType));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            //show price ah
            if (sttngs.showsugrange)
            {
                GUI.Label(recto.setwtsahlabel, "show in WTS-AH the ");
                if (GUI.Button(recto.setwtsahbutton, ""))
                {
                    sttngs.wtsAHpriceType = nextScrollsPostPriceType(sttngs.wtsAHpriceType);
                }
                if (GUI.Button(recto.setwtsahbutton2, ""))
                {
                    sttngs.wtsAHpriceType2 = nextScrollsPostPriceType(sttngs.wtsAHpriceType2);
                }
                GUI.Label(recto.setwtsahlabel3, "and");
                GUI.Label(recto.setwtsahlabel4, ((sttngs.wtsAHpriceType == ScrollsPostPriceType.BLACKMARKET) ? "" : "ScrollsGuide ") + "prices");
                GUI.Label(recto.setwtbahlabel, "show in WTB-AH the ");
                if (GUI.Button(recto.setwtbahbutton, ""))
                {
					sttngs.wtbAHpriceType = nextScrollsPostPriceType (sttngs.wtbAHpriceType);
                }
                if (GUI.Button(recto.setwtbahbutton2, ""))
                {
                    sttngs.wtbAHpriceType2 = nextScrollsPostPriceType(sttngs.wtbAHpriceType2);
                }
                GUI.Label(recto.setwtbahlabel3, "and");
                GUI.Label(recto.setwtbahlabel4, ((sttngs.wtbAHpriceType == ScrollsPostPriceType.BLACKMARKET) ? "" : "ScrollsGuide ") + "prices");
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUI.Label(recto.setwtsahbutton, scrollsPostPriceTypeToString(sttngs.wtsAHpriceType));
				GUI.Label(recto.setwtbahbutton, scrollsPostPriceTypeToString(sttngs.wtbAHpriceType));
				GUI.Label(recto.setwtsahbutton2, scrollsPostPriceTypeToString(sttngs.wtsAHpriceType2));
				GUI.Label(recto.setwtbahbutton2, scrollsPostPriceTypeToString(sttngs.wtbAHpriceType2)); 

                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            else
            {
                GUI.Label(recto.setwtsahlabel, "show in WTS-AH the ");
                if (GUI.Button(recto.setwtsahbutton, ""))
                {
                    sttngs.wtsAHpriceType = nextScrollsPostPriceType(sttngs.wtsAHpriceType);
                }
                GUI.Label(recto.setwtsahlabel2, "ScrollsGuide price");
                GUI.Label(recto.setwtbahlabel, "show in WTB-AH the ");
                if (GUI.Button(recto.setwtbahbutton, ""))
                {
                    sttngs.wtbAHpriceType = nextScrollsPostPriceType(sttngs.wtbAHpriceType);
                }
                GUI.Label(recto.setwtbahlabel2, "ScrollsGuide price");
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUI.Label(recto.setwtsahbutton, scrollsPostPriceTypeToString(sttngs.wtsAHpriceType));
				GUI.Label(recto.setwtbahbutton, scrollsPostPriceTypeToString(sttngs.wtbAHpriceType));
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            // which version of scrollpost price:
            /*
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.Label(recto.scrollpostlabel1, "Version of ScrollsGuide-Price: ");
            GUI.Label(recto.scrollpostlabel2, " (restart needed to take effect)");
            if (GUI.Button(recto.scrollpostbutton, ""))
            {
                sttngs.scrollspostday = (ScrollsPostDayType)(((int)sttngs.scrollspostday + 1) % 6);
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (sttngs.scrollspostday == ScrollsPostDayType.one) { GUI.Label(recto.scrollpostbutton, "1-day"); }
            if (sttngs.scrollspostday == ScrollsPostDayType.three) { GUI.Label(recto.scrollpostbutton, "3-days"); }
            if (sttngs.scrollspostday == ScrollsPostDayType.seven) { GUI.Label(recto.scrollpostbutton, "7-days"); }
            if (sttngs.scrollspostday == ScrollsPostDayType.fourteen) { GUI.Label(recto.scrollpostbutton, "14-days"); }
            if (sttngs.scrollspostday == ScrollsPostDayType.thirty) { GUI.Label(recto.scrollpostbutton, "30-days"); }
            if (sttngs.scrollspostday == ScrollsPostDayType.hour) { GUI.Label(recto.scrollpostbutton, "1-hour"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            */

            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
        }

        
        
    }
}
