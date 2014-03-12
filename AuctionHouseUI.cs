using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading;

namespace Auction.mod
{
    class AuctionHouseUI : IOkCancelCallback
    {
        TradingWithBots twb;

        GetGoogleThings gglthngs;
        PlayerStore ps;
        private int durationIndex = 0;

        Vector2 scrolll = new Vector2(0, 0);
        public bool wtsinah = true;//remembers which menupoint in ah was the last one
        private Auction tradeitem;
        private bool selectable=true;
        private float opacity;
        public Vector2 scrollPos, scrollPos2,scrollPos4;
        Rectomat recto;
        Prices prcs;
        Cardviewer crdvwr;
        Searchsettings srchsvr;
        Settings sttngs;
        Helpfunktions helpf;
        AuctionHouse ah;
        List<Auction> ahlist;
        //ReadOnlyCollection<Auction> ahlist;
        bool bothstarttrading = false;
        private string OfferPrice = "1000";// in offermenu the offered price
        private Card OfferCard = null;

        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");

        Color dblack = new Color(1f, 1f, 1f, 0.5f);


        private static AuctionHouseUI instance;

        public static AuctionHouseUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AuctionHouseUI();
                }
                return instance;
            }
        }

        private AuctionHouseUI()
        {
            this.helpf = Helpfunktions.Instance;
            this.recto = Rectomat.Instance;
            this.prcs = Prices.Instance;
            this.crdvwr = Cardviewer.Instance;
            this.srchsvr = Searchsettings.Instance;
            this.sttngs = Settings.Instance;
            this.ah = AuctionHouse.Instance;
            this.ps = PlayerStore.Instance;
            this.gglthngs = GetGoogleThings.Instance;
            this.twb = TradingWithBots.Instance;
        }

        public void ahbuttonpressed()
        {
            helpf.inauchouse = true;
            helpf.settings = false;
            helpf.generator = false;
            helpf.offerMenuSelectCardMenu = false;
            //this.hideInformation();
            helpf.hideInformationinfo.Invoke(helpf.storeinfo, null);

            if (sttngs.spampreventtime != "")
            {
                ah.spamFilter.setSpamTime(new TimeSpan(0, sttngs.spamprevint, 0));
                ah.buyOfferFilter.filtersChanged = true;
                ah.sellOfferFilter.filtersChanged = true;
                ps.createCardsFilter.filtersChanged = true;
                ps.sellOfferFilter.filtersChanged = true;
            }
            else
            {
                ah.spamFilter.disableSpamFilter();
                ah.buyOfferFilter.filtersChanged = true;
                ah.sellOfferFilter.filtersChanged = true;
                ps.createCardsFilter.filtersChanged = true;
                ps.sellOfferFilter.filtersChanged = true;
            }

            iTween.MoveTo((GameObject)(helpf.buymen).GetValue(helpf.storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
            helpf.showBuyinfo.SetValue(helpf.storeinfo, false);

            ((GameObject)helpf.sellmen.GetValue(helpf.storeinfo)).SetActive(false);
            iTween.MoveTo((GameObject)helpf.sellmen.GetValue(helpf.storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
            ((GameObject)helpf.sellmen.GetValue(helpf.storeinfo)).SetActive(true);
            helpf.showSellinfo.SetValue(helpf.storeinfo, false);

            Store.ENABLE_SHARD_PURCHASES = false;


            ah.sellOfferFilter.filtersChanged = true;
            ah.buyOfferFilter.filtersChanged = true;
            ps.createCardsFilter.filtersChanged = true;
            ps.sellOfferFilter.filtersChanged = true;

            if (this.wtsinah)
            {

                srchsvr.setsettings(0, true);
                helpf.wtsmenue = true;

            }
            else
            {

                srchsvr.setsettings(0, false);
                helpf.wtsmenue = false;

            }
            if (helpf.playerStoreMenu)
            {
                srchsvr.setsettings(2, true);
                helpf.wtsmenue = true;
            }
            if (helpf.createAuctionMenu)
            {
                srchsvr.setsettings(2, false);
            }

            //lstfltrs.fullupdatelist(alists.ahlist, alists.ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
            helpf.targetchathightinfo.SetValue(helpf.target, (float)Screen.height * 0.25f);
        
        }


        
        public void drawAH()
        {

            // update playerstore auctions:
            if (helpf.playerStoreMenu || helpf.createAuctionMenu)
            {
                if (this.gglthngs.dataisready)
                {
                    this.gglthngs.addDataToPlayerStore();
                }
            }


            if (helpf.offerMenuSelectCardMenu)
            {
                //this.ccardlist(false);// want to show the sell prices
                
                this.drawgenerator();
                return;
            }

            if (helpf.createAuctionMenu)
            {
                if (OfferCard == null && this.helpf.allOwnTradeableAuctions.Count >= 1)
                { OfferCard = this.helpf.allOwnTradeableAuctions[0].card; }
                // for getting mouse wheel-scrolling in overlayed window, we have to paint it first
                drawCreateMenu();
                this.drawAHlist(false);
                return;
            }


            if (helpf.bothmenue)
            {
                // for getting mouse wheel-scrolling in overlayed window, we have to paint it first
                if (helpf.showtradedialog && !(helpf.makeOfferMenu && !helpf.offerMenuSelectCardMenu)) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, this.bothstarttrading, tradeitem.message, tradeitem.card.getType(), tradeitem.card.level); }
                
                srchsvr.setsettings(0, true);
                ah.setSellSortMode(srchsvr.sortmode);
                this.drawAHlist(true);

                if (helpf.bothmenue) // could be changed in drawAHlist(true), because there is the menu buttons drawn.
                {
                    srchsvr.setsettings(0, false);
                    ah.setBuySortMode(srchsvr.sortmode);
                    this.drawAHlist(false);
                }

                if (helpf.showtradedialog) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, this.bothstarttrading, tradeitem.message,tradeitem.card.getType(),tradeitem.card.level); }
                return;
            }
            // for getting mouse wheel-scrolling in overlayed window, we have to paint it first
            if (helpf.showtradedialog && !(helpf.makeOfferMenu && !helpf.offerMenuSelectCardMenu)) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, helpf.wtsmenue, tradeitem.message, tradeitem.card.getType(),tradeitem.card.level); }
            //draw regular menu + searchmenu
            this.drawAHandSearch();
        }

        private void drawAHandSearch()
        {
            
            bool clickableItems = false;
            // have to draw textfield in front of scrollbar or otherwise you lose focus in textfield (lol)
            if (helpf.inauchouse)
            {

                
                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.filtermenurect, string.Empty);
                // wts filter menue
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                GUI.Label(recto.sbarlabelrect, "Scroll:");
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.sbrect, string.Empty);
                string selfcopy = srchsvr.wtssearchstring;
                GUI.SetNextControlName("dbSearchfield");
                srchsvr.wtssearchstring = GUI.TextField(recto.sbrect, srchsvr.wtssearchstring, helpf.chatLogStyle);
                recto.drawsearchpulldown();// draw here to be the pull down menue the first clicked object


                GUI.contentColor = Color.white;
                GUI.color = Color.white;
                if (!srchsvr.growthbool) { GUI.color = dblack; }
                bool growthclick = GUI.Button(recto.sbgrect, growthres);
                GUI.color = Color.white;
                if (!srchsvr.orderbool) { GUI.color = dblack; }
                bool orderclick = GUI.Button(recto.sborect, orderres);
                GUI.color = Color.white;
                if (!srchsvr.energybool) { GUI.color = dblack; }
                bool energyclick = GUI.Button(recto.sberect, energyres);
                GUI.color = Color.white;
                if (!srchsvr.decaybool) { GUI.color = dblack; }
                bool decayclick = GUI.Button(recto.sbdrect, decayres);


                GUI.contentColor = Color.gray;
                GUI.color = Color.white;
                if (!srchsvr.commonbool) { GUI.color = dblack; }
                bool commonclick = GUI.Button(recto.sbcommonrect, "C");
                GUI.color = Color.white;
                if (!srchsvr.uncommonbool) { GUI.color = dblack; }
                GUI.contentColor = Color.white;
                bool uncommonclick = GUI.Button(recto.sbuncommonrect, "U");
                GUI.color = Color.white;
                if (!srchsvr.rarebool) { GUI.color = dblack; }
                GUI.contentColor = Color.yellow;
                bool rareclick = GUI.Button(recto.sbrarerect, "R");
                GUI.color = Color.white;
                if (!srchsvr.threebool) { GUI.color = dblack; }
                //if (!p1mt3bool) { GUI.color = dblack; }
                bool mt3click;
                bool mt0click = false;
                if (helpf.wtsmenue)
                {
                    mt3click = GUI.Button(recto.sbthreerect, "<3"); // >3 bei wtsmenue=false
                }
                else { mt3click = GUI.Button(recto.sbthreerect, ">3"); }
                GUI.color = Color.white;
                if (!srchsvr.onebool) { GUI.color = dblack; }
                if (!helpf.wtsmenue) { mt0click = GUI.Button(recto.sbonerect, ">0"); };
                GUI.color = Color.white;
                GUI.contentColor = Color.white;

                // draw seller filter
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                if (helpf.wtsmenue)
                {
                    GUI.Label(recto.sbsellerlabelrect, "ignore Seller:");
                }
                else { GUI.Label(recto.sbsellerlabelrect, "ignore Buyer:"); }

                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.sbsellerrect, string.Empty);
                string sellercopy = srchsvr.sellersearchstring;
                GUI.SetNextControlName("sellerframe");
                srchsvr.sellersearchstring = GUI.TextField(recto.sbsellerrect, srchsvr.sellersearchstring, helpf.chatLogStyle);

                // draw price filter


                GUI.skin = helpf.cardListPopupBigLabelSkin;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(recto.sbpricelabelrect, "<= Price <=");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;


                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.sbpricerect, string.Empty);
                GUI.Box(recto.sbpricerect2, string.Empty);
                string pricecopy = srchsvr.pricesearchstring;
                string pricecopy2 = srchsvr.pricesearchstring2;
                srchsvr.pricesearchstring = Regex.Replace(GUI.TextField(recto.sbpricerect, srchsvr.pricesearchstring, helpf.chatLogStyle), @"[^0-9]", "");
                srchsvr.pricesearchstring2 = Regex.Replace(GUI.TextField(recto.sbpricerect2, srchsvr.pricesearchstring2, helpf.chatLogStyle), @"[^0-9]", "");
                GUI.color = Color.white;

                // draw time filter
                if (!helpf.playerStoreMenu)
                {
                    GUI.skin = helpf.cardListPopupBigLabelSkin;
                    GUI.Label(recto.sbtimelabel, "not older than");
                    GUI.skin = helpf.cardListPopupSkin;
                    GUI.Box(recto.sbtimerect, string.Empty);
                    string timecopy = srchsvr.timesearchstring;
                    srchsvr.timesearchstring = Regex.Replace(GUI.TextField(recto.sbtimerect, srchsvr.timesearchstring, 2, helpf.chatLogStyle), @"[^0-9]", "");
                    if (srchsvr.timesearchstring != "" && Convert.ToInt32(srchsvr.timesearchstring) > helpf.deleteTime) { srchsvr.timesearchstring = ((int)helpf.deleteTime).ToString(); }
                    GUI.color = Color.white;
                }


                bool tpfgen = GUI.Button(recto.sbtpfgen, "");
                if (srchsvr.takepriceformgenarator)
                {
                    GUI.DrawTexture(recto.sbtpfgen, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
                }
                else
                {
                    GUI.DrawTexture(recto.sbtpfgen, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
                }
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                if (helpf.wtsmenue)
                {
                    GUI.Label(recto.sbtpfgenlabel, "Price <= wtb-generator");
                }
                else { GUI.Label(recto.sbtpfgenlabel, "Price >= wts-generator"); }


                // only scrolls with price
                bool owp = false;
                if (!helpf.playerStoreMenu)
                {
                    owp = GUI.Button(recto.sbonlywithpricebox, "");
                    if (srchsvr.ignore0)
                    {
                        GUI.DrawTexture(recto.sbonlywithpricebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
                    }
                    else
                    {
                        GUI.DrawTexture(recto.sbonlywithpricebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
                    }
                    GUI.skin = helpf.cardListPopupBigLabelSkin;
                    GUI.Label(recto.sbonlywithpricelabelbox, "only Scrolls with Price");
                }
                GUI.skin = helpf.cardListPopupSkin;

                GUI.contentColor = Color.red;
                bool closeclick = GUI.Button(recto.sbclearrect, "X");
                GUI.contentColor = Color.white;

                if (this.helpf.playerStoreMenu)
                {
                    if (GUI.Button(recto.updateGoogleThings, "Update"))
                    {
                        if (this.gglthngs.workthreadready) new Thread(new ThreadStart(this.gglthngs.workthread)).Start();
                    }
                }

                

                if (growthclick) { srchsvr.growthbool = !srchsvr.growthbool; };
                if (orderclick) { srchsvr.orderbool = !srchsvr.orderbool; }
                if (energyclick) { srchsvr.energybool = !srchsvr.energybool; };
                if (decayclick) { srchsvr.decaybool = !srchsvr.decaybool; }
                if (commonclick) { srchsvr.commonbool = !srchsvr.commonbool; };
                if (uncommonclick) { srchsvr.uncommonbool = !srchsvr.uncommonbool; }
                if (rareclick) { srchsvr.rarebool = !srchsvr.rarebool; };
                if (mt3click) { srchsvr.threebool = !srchsvr.threebool; }
                if (mt0click) { srchsvr.onebool = !srchsvr.onebool; }
                if (owp) { srchsvr.ignore0 = !srchsvr.ignore0; }
                if (tpfgen) { srchsvr.takepriceformgenarator = !srchsvr.takepriceformgenarator; }
                if (closeclick)
                {
                    srchsvr.resetsearchsettings();
                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.resetFilters();
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.resetFilters();
                    }
                    else
                    {
                        ah.buyOfferFilter.resetFilters();
                    }

                }

                if (helpf.wtsmenue)
                {
                    if (helpf.playerStoreMenu) srchsvr.savesettings(2, true); 
                    else srchsvr.savesettings(0, true); 
                } 
                else { srchsvr.savesettings(0, false); }

                //set filters
                if (tpfgen)
                {
                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setTakeWTB(srchsvr.takepriceformgenarator);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setTakeWTB(srchsvr.takepriceformgenarator);
                    }
                    else
                    {
                        ah.buyOfferFilter.setTakeWTS(srchsvr.takepriceformgenarator);
                    } 
                }
                if (selfcopy != srchsvr.wtssearchstring)
                {
                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setCardFilter(srchsvr.wtssearchstring);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setCardFilter(srchsvr.wtssearchstring);
                    }
                    else
                    {
                        ah.buyOfferFilter.setCardFilter(srchsvr.wtssearchstring);
                    }

                }
                if (owp)
                {
                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setDontShowNoPrice(srchsvr.ignore0);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setDontShowNoPrice(srchsvr.ignore0);
                    }
                    else
                    {
                        ah.buyOfferFilter.setDontShowNoPrice(srchsvr.ignore0);
                    }
                }
                if (mt3click||mt0click)
                {
                    int filter = 0;
                    
                    if (helpf.wtsmenue)
                    {
                        if (srchsvr.threebool) filter = 3;
                        ah.sellOfferFilter.setAmountFilter(filter);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setAmountFilter(filter);
                    }
                    else
                    {
                        if (srchsvr.onebool) filter = 1;
                        if (srchsvr.threebool) filter = 2;//(onebool < threebool)
                        ah.buyOfferFilter.setAmountFilter(filter);
                    }
                }

                if (sellercopy != srchsvr.sellersearchstring)
                {

                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setIgnoredSellers(srchsvr.sellersearchstring);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setIgnoredSellers(srchsvr.sellersearchstring);
                    }
                    else
                    {
                        ah.buyOfferFilter.setIgnoredSellers(srchsvr.sellersearchstring);
                    }


                }
                if (pricecopy != srchsvr.pricesearchstring)
                {
                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setPriceLowerBound(srchsvr.pricesearchstring);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setPriceLowerBound(srchsvr.pricesearchstring);
                    }
                    else
                    {
                        ah.buyOfferFilter.setPriceLowerBound(srchsvr.pricesearchstring);
                    }
                }
                if (pricecopy2 != srchsvr.pricesearchstring2)
                {

                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setPriceUpperBound(srchsvr.pricesearchstring2);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setPriceUpperBound(srchsvr.pricesearchstring2);
                    }
                    else
                    {
                        ah.buyOfferFilter.setPriceUpperBound(srchsvr.pricesearchstring2);
                    }
                }


                if (growthclick || orderclick || energyclick || decayclick)
                {
                    string[] res = { "", "", "", "" };
                    if (srchsvr.decaybool) { res[0] = "decay"; };
                    if (srchsvr.energybool) { res[1] = "energy"; };
                    if (srchsvr.growthbool) { res[2] = "growth"; };
                    if (srchsvr.orderbool) { res[3] = "order"; };


                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setResourceFilter(res);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setResourceFilter(res);
                    }
                    else
                    {
                        ah.buyOfferFilter.setResourceFilter(res);
                    }


                }
                if (commonclick || uncommonclick || rareclick)
                {
                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setRarityFilter(srchsvr.commonbool, srchsvr.uncommonbool, srchsvr.rarebool);
                        if (helpf.playerStoreMenu) ps.sellOfferFilter.setRarityFilter(srchsvr.commonbool, srchsvr.uncommonbool, srchsvr.rarebool);
                    }
                    else
                    {
                        ah.buyOfferFilter.setRarityFilter(srchsvr.commonbool, srchsvr.uncommonbool, srchsvr.rarebool);
                    }

                }


                if (recto._showSearchDropdown) recto.OnGUI_drawSearchPulldown(recto.sbrect);// draw pulldown again (for overlay)
            }
            // Draw Auctionhouse here:
            if (helpf.inauchouse)
            {
                bool deleteOldEntrys = false;
                //Console.WriteLine(GUI.GetNameOfFocusedControl());
                GUI.depth = 15;
                this.opacity = 1f;
                GUI.skin = helpf.cardListPopupSkin;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

                GUI.Box(recto.position, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
                GUI.Box(recto.position2, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);



                // draw sort buttons:###############################################
                Vector2 vec11 = GUI.skin.button.CalcSize(new GUIContent("Scroll"));

                if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX, recto.screenRect.yMin - 4, vec11.x, 20), "Scroll"))
                {
                    if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }// this will toggle the reverse mode
                    if (srchsvr.sortmode == 1) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                    srchsvr.sortmode = 1;
                    if (helpf.wtsmenue)
                    {
                        ah.setSellSortMode(AuctionHouse.SortMode.CARD);
                        if (helpf.playerStoreMenu) ps.setSellSortMode(AuctionHouse.SortMode.CARD);
                    }
                    else
                    {
                        ah.setBuySortMode(AuctionHouse.SortMode.CARD);
                    }

                    //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);

                }
                float datelength = GUI.skin.button.CalcSize(new GUIContent("Date")).x;
                float datebeginn = 0;
                if (helpf.wtsmenue)
                {
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Seller"));

                    if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2f - datelength / 2f - 2f, recto.screenRect.yMin - 4f, vec11.x, 20f), "Seller"))
                    {
                        if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                        if (srchsvr.sortmode == 3) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                        srchsvr.sortmode = 3;
                        ah.setSellSortMode(AuctionHouse.SortMode.SELLER);
                        if (helpf.playerStoreMenu) ps.setSellSortMode(AuctionHouse.SortMode.SELLER);
                        //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                    }
                }
                else
                {
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Buyer"));
                    if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2f - datelength / 2f - 2f, recto.screenRect.yMin - 4f, vec11.x, 20f), "Buyer"))
                    {
                        if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                        if (srchsvr.sortmode == 3) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                        srchsvr.sortmode = 3;
                        ah.setBuySortMode(AuctionHouse.SortMode.SELLER);
                        //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                    }
                }
                datebeginn = recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2 - datelength / 2 - 2 + vec11.x;
                vec11 = GUI.skin.button.CalcSize(new GUIContent("Price"));
                if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + 2f * recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + 2 * recto.costIconWidth + recto.labelsWidth / 4f - vec11.x / 2, recto.screenRect.yMin - 4, vec11.x, 20), "Price"))
                {
                    if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                    if (srchsvr.sortmode == 2) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                    srchsvr.sortmode = 2;
                    if (helpf.wtsmenue)
                    {
                        ah.setSellSortMode(AuctionHouse.SortMode.PRICE);
                        if (helpf.playerStoreMenu) ps.setSellSortMode(AuctionHouse.SortMode.PRICE);
                    }
                    else
                    {
                        ah.setBuySortMode(AuctionHouse.SortMode.PRICE);
                    }

                    //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                }
                vec11 = GUI.skin.button.CalcSize(new GUIContent("Date"));
                //if (GUI.Button(new Rect(this.innerRect.x + offX , this.screenRect.yMin - 4, vec11.x * 2, 20), "Date"))
                if (GUI.Button(new Rect(datebeginn + 4, recto.screenRect.yMin - 4, vec11.x, 20), "Date"))
                {
                    if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                    if (srchsvr.sortmode == 0) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                    srchsvr.sortmode = 0;
                    if (helpf.wtsmenue)
                    {
                        ah.setSellSortMode(AuctionHouse.SortMode.TIME);
                        if (helpf.playerStoreMenu) ps.setSellSortMode(AuctionHouse.SortMode.TIME);
                    }
                    else
                    {
                        ah.setBuySortMode(AuctionHouse.SortMode.TIME);
                    }
                    //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                }



                int num = 0;
                Card card = null;

                // set drawn cards
                if (helpf.playerStoreMenu)
                {
                    this.ahlist = ps.getSellOffers();
                }
                else
                {

                    if (helpf.wtsmenue) this.ahlist = ah.getSellOffers();
                    else this.ahlist = ah.getBuyOffers();
                }
                

                DateTime currenttime = DateTime.Now;
                // draw auctimes################################################
                //timefilter: 
                int time = 0;
                bool usetimefilter = false;
                float anzcards = anzcards = (float)this.ahlist.Count();
                if (srchsvr.timesearchstring != "")//doesnt show "old" offers filtered by time-filter
                {
                    time = Convert.ToInt32(srchsvr.timesearchstring);
                    currenttime = DateTime.Now.AddMinutes(-1 * time); usetimefilter = true;
                    anzcards = (float)this.ahlist.Count(delegate(Auction p1) { return (p1.time).CompareTo(currenttime) >= 0; });
                }

                GUI.skin = helpf.cardListPopupSkin;
                if (helpf.wtsmenue)
                {
                    this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }
                else
                {
                    this.scrollPos2 = GUI.BeginScrollView(recto.position3, this.scrollPos2, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }

                //this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                if ((!(this.helpf.playerStoreMenu && this.srchsvr.sortmode == 0) && srchsvr.reverse) || (this.helpf.playerStoreMenu && this.srchsvr.sortmode == 0 && !srchsvr.reverse)) { this.ahlist.Reverse(); }

                GUI.skin = helpf.cardListPopupBigLabelSkin;


                float testy = this.scrollPos.y;
                if (!helpf.wtsmenue) testy = this.scrollPos2.y;
                foreach (Auction current in this.ahlist)
                {
                    if (usetimefilter && (current.time).CompareTo(currenttime) < 0) { continue; }
                    if (!current.card.tradable)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    GUI.skin = helpf.cardListPopupGradientSkin;
                    //draw boxes
                    Rect position7 = recto.position7(num);
                    if (position7.yMax < testy || position7.y > testy + recto.position3.height)
                    {
                        num++;
                        GUI.color = Color.white;
                    }
                    else
                    {
                        if (clickableItems)
                        {
                            if (GUI.Button(position7, string.Empty))
                            {
                                //this.callback.ItemClicked(this, current);
                            }
                        }
                        else
                        {
                            GUI.Box(position7, string.Empty);
                        }
                        string name = current.card.getName();

                        string txt = helpf.cardIDtoimageid(current.card.getType()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        if (sttngs.shownumberscrolls) name = "(" + helpf.cardIDToNumberOwned[current.card.getType()] + ") " + name;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                        // draw text
                        Rect position8 = recto.position8(num);
                        GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                        GUI.skin = helpf.cardListPopupSkin;
                        string text = current.card.getPieceKind().ToString();
                        string str = text.Substring(0, 1) + text.Substring(1).ToLower();
                        string text2 = string.Empty;
                        int num2 = recto.maxCharsRK;
                        if (current.card.level > 0)
                        {
                            string text3 = text2;
                            text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.card.level + 1, "</color>, " });
                            num2 += "<color=#rrggbb></color>".Length;
                        }
                        text2 = text2 + current.card.getRarityString() + ", " + str;
                        Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(text2));

                        Rect position9 = recto.position9(num);
                        GUI.Label(position9, (vector2.x >= position9.width) ? (text2.Substring(0, Mathf.Min(text2.Length, num2)) + "...") : text2);
                        Rect restyperect = recto.restyperect(num);
                        //draw resource type
                        this.RenderCost(restyperect, current.card);
                        //draw seller name

                        string sellername = current.seller;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;

                        vector = GUI.skin.label.CalcSize(new GUIContent(sellername));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                        //Rect position11 = new Rect(restyperect.xMax + 2f, (float)num * this.fieldHeight, this.labelsWidth, this.fieldHeight);
                        Rect position11 = new Rect(restyperect.xMax + 2f, position8.yMin, recto.labelsWidth, recto.cardHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position11, (vector.x >= position11.width) ? (sellername.Substring(0, Mathf.Min(sellername.Length, recto.maxCharsName)) + "...") : sellername);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        //draw timestamp
                        GUI.skin = helpf.cardListPopupSkin;
                        DateTime temptime = DateTime.Now;
                        TimeSpan ts = temptime.Subtract(current.time);
                        if (this.helpf.playerStoreMenu)
                        {
                            if (current.message.Split(';')[0] == "sold")
                            { sellername = "sold"; }
                            else
                            {
                                ts = (current.time).Subtract(temptime);
                                if (ts.TotalHours >= 1.0)
                                {
                                    sellername = ((int)ts.TotalHours+1) + " hours left";
                                }
                                else
                                {

                                    if (ts.TotalMinutes >= 1.0)
                                    {
                                        sellername = (ts.Minutes+1) + " min left";

                                    }
                                    else
                                    {
                                        sellername = "ends in a minute";
                                        if (ts.Seconds <= 40) { sellername = "ends in 40 seconds"; }
                                        if (ts.Seconds <= 20) { sellername = "ends in 20 seconds"; }
                                        if (ts.Seconds <= 10) { sellername = "ends in 10 seconds"; }
                                        if (ts.Seconds <= 5) { sellername = "ends in 5 seconds"; }
                                        if (ts.Seconds <= 0) { sellername = "ended"; }
                                        if (ts.TotalSeconds <= 0 && current.message.Split(';')[4] != App.MyProfile.ProfileInfo.id) deleteOldEntrys = true;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (ts.TotalMinutes >= 1.0)
                            {
                                sellername = "" + ts.Minutes + " minutes ago";
                                if (ts.TotalMinutes >= helpf.deleteTime) deleteOldEntrys = true;
                            }
                            else
                            {
                                //sellername = "" + ts.Seconds + " seconds ago"; // to mutch changing numbers XD
                                if (ts.Seconds >= 40) { sellername = "40 seconds ago"; }
                                else if (ts.Seconds >= 20) { sellername = "20 seconds ago"; }
                                else if (ts.Seconds >= 10) { sellername = "10 seconds ago"; }
                                else if (ts.Seconds >= 5) { sellername = "5 seconds ago"; }
                                else sellername = "seconds ago";
                            }
                        }
                        


                        Rect position13 = new Rect(restyperect.xMax + 2f, position9.y, recto.labelsWidth, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position13, sellername);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;
                        //testonly
                        //    restyperect = new Rect(position11.xMax , (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight);
                        //       draw resource type
                        //     this.RenderCost(restyperect, current);

                        //draw gold cost
                        float nextx = position11.xMax + recto.costIconWidth;
                        string gold = current.price + "g";
                        if (current.price == 0) gold = "?";
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f

                        //Rect position12 = new Rect(nextx + 2f, position8.yMin, recto.goldlength, recto.cardHeight);
                        Rect position12 = new Rect(nextx + 2f, position8.yMin, recto.labelsWidth / 2f, recto.cardHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position12, gold);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                        // draw suggested price
                        //int index = Array.FindIndex(helpf.cardids, element => element == current.card.getType());
                        int index = helpf.cardidToArrayIndex(current.card.getType());
                        string suggeprice = "";
                        if (index >= 0)
                        {
                            int p1 = 0, p2 = 0;
                            if (helpf.wtsmenue)
                            {
								p1 = prcs.getPrice(index, sttngs.wtsAHpriceType);
                            }
                            else
                            {
								p1 = prcs.getPrice(index, sttngs.wtbAHpriceType);
                            }
                            suggeprice = "SG: " + p1;
                            if (sttngs.showsugrange)
                            {
                                if (helpf.wtsmenue)
                                {
									p2 = prcs.getPrice(index, sttngs.wtsAHpriceType2);
                                }
                                else
                                {
									p2 = prcs.getPrice(index, sttngs.wtbAHpriceType2);
                                }
                            }
                            if (sttngs.showsugrange && p1 != p2) suggeprice = "SG: " + Math.Min(p1, p2) + "-" + Math.Max(p1, p2);
                        }
                        GUI.skin = helpf.cardListPopupSkin;
                        Rect position14 = new Rect(nextx + 2f, position9.y, recto.labelsWidth / 2f, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position14, suggeprice);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;



                        GUI.skin = helpf.cardListPopupLeftButtonSkin;
                        if (!this.selectable)
                        {
                            GUI.enabled = false;
                        }
                        if (GUI.Button(recto.position10(num), string.Empty) && current.card.tradable)
                        {
                            card = current.card;
                            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                        }
                        if (!this.selectable)
                        {
                            GUI.enabled = true;
                        }
                        //draw picture
                        if (texture != null)
                        {
                            GUI.DrawTexture(new Rect(4f, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight), texture);
                        }
                        // draw buy/sell button
                        GUI.skin = helpf.cardListPopupGradientSkin;
                        if (helpf.wtsmenue)
                        {
                            if (!helpf.showtradedialog)
                            {
                                if ((this.sttngs.actualTrading || this.sttngs.waitForAuctionBot || current.seller == App.MyProfile.ProfileInfo.name))
                                {
                                    GUI.color = dblack;
                                }
                                if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                {

                                    // start trading with seller
                                    if (current.seller != App.MyProfile.ProfileInfo.name && current.price <= App.MyProfile.ProfileData.gold)
                                    {
                                        if (!(this.helpf.playerStoreMenu && (this.sttngs.actualTrading || this.sttngs.waitForAuctionBot)))
                                        { // we cant bid on an auction (playerstore stuff) if we are on queue :D
                                            helpf.showtradedialog = true;
                                            tradeitem = current;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GUI.Box(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "");
                            }
                            
                            GUI.skin = helpf.cardListPopupBigLabelSkin;
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Buy");
                            GUI.color = Color.white;

                        }
                        else
                        {
                            if (!helpf.showtradedialog)
                            {
                                if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                {

                                    // start trading with seller
                                    if (current.seller != App.MyProfile.ProfileInfo.name)
                                    {
                                        helpf.showtradedialog = true;
                                        tradeitem = current;
                                    }
                                }
                            }
                            else { GUI.Box(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                            GUI.skin = helpf.cardListPopupBigLabelSkin;
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Sell");
                        }
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        if (!current.card.tradable)
                        {
                            GUI.color = Color.white;
                        }
                        num++;
                    }
                }

                GUI.EndScrollView();

                // delete old entrys
                

                GUI.color = Color.white;
                // show clicked card
                if (card != null)
                {
                    //this.callback.ItemButtonClicked(this, card);
                    string clink = card.getName().ToLower();
                    //int arrindex = Array.FindIndex(helpf.cardnames, element => element.Equals(clink));
                    int arrindex= helpf.cardnameToArrayIndex(clink);
                    if (arrindex >= 0)
                    {

                        crdvwr.createcard(arrindex, helpf.cardids[arrindex]);
                    }

                }
                if (helpf.playerStoreMenu)
                {
                    GUI.skin = helpf.cardListPopupSkin;

                    GUI.contentColor = Color.red;
                    if ((this.sttngs.actualTrading || this.sttngs.waitForAuctionBot))
                    {
                        if (this.sttngs.actualTrading)
                        {
                            GUI.Label(recto.sbnetworklabel, "struggling with bot");
                            
                        }
                        else
                        {
                            GUI.Label(recto.sbnetworklabel, "waiting for bot");
                        }
                    }
                    GUI.contentColor = Color.white;
                }

                drawButtonsBelow();

                GUI.skin.button.normal.textColor = Color.white;
                GUI.skin.button.hover.textColor = Color.white;


                GUI.color = Color.white;

                

                if (helpf.playerStoreMenu)
                {
                    if (deleteOldEntrys)
                    {
                        ps.removeOldEntrys();
                    }
                }
                else
                {
                    if (deleteOldEntrys)
                    {
                        ah.removeOldEntrys();
                    }
                }


                if (helpf.showtradedialog) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, helpf.wtsmenue, tradeitem.message, tradeitem.card.getType(),tradeitem.card.level); }






            }
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
        }

        public void drawAHlist(bool wtsmenue)
        {
            if (!wtsmenue)
            {
                recto.innerRect.x = recto.innerRect.x + Screen.width * 0.495f;
                recto.innerBGRect.x = recto.innerBGRect.x + Screen.width * 0.495f;
                recto.outerRect.x = recto.outerRect.x + Screen.width * 0.495f;
                recto.screenRect.x = recto.screenRect.x + Screen.width * 0.495f;
                recto.filtermenurect.x = recto.filtermenurect.x + Screen.width * 0.495f;
                recto.calcguirects();
            }

            // Draw Auctionhouse here:
            if (helpf.inauchouse)
            {
                bool deleteOldEntrys = false;
                //Console.WriteLine(GUI.GetNameOfFocusedControl());
                GUI.depth = 15;
                this.opacity = 1f;
                GUI.skin = helpf.cardListPopupSkin;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

                GUI.Box(recto.position, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
                GUI.Box(recto.position2, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);



                // draw sort buttons:###############################################
                if (helpf.bothmenue)
                {
                    Vector2 vec11 = GUI.skin.button.CalcSize(new GUIContent("Scroll"));

                    if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX, recto.screenRect.yMin - 4, vec11.x, 20), "Scroll"))
                    {
                        if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }// this will toggle the reverse mode
                        if (srchsvr.sortmode == 1) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                        srchsvr.sortmode = 1;
                        if (helpf.bothmenue)
                        {
                            if (wtsmenue)
                            {
                                ah.setSellSortMode(AuctionHouse.SortMode.CARD);
                            }
                            else
                            {
                                ah.setBuySortMode(AuctionHouse.SortMode.CARD);
                            }
                            if (wtsmenue) { srchsvr.savesettings(0, true); } else { srchsvr.savesettings(0, false); }
                        }
                        //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);

                    }
                    float datelength = GUI.skin.button.CalcSize(new GUIContent("Date")).x;
                    float datebeginn = 0;
                    if (helpf.bothmenue)
                    {
                        if (wtsmenue)
                        {
                            vec11 = GUI.skin.button.CalcSize(new GUIContent("Seller"));

                            if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2f - datelength / 2f - 2f, recto.screenRect.yMin - 4f, vec11.x, 20f), "Seller"))
                            {
                                if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                                if (srchsvr.sortmode == 3) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                                srchsvr.sortmode = 3;
                                if (helpf.bothmenue)
                                {
                                    ah.setSellSortMode(AuctionHouse.SortMode.SELLER);
                                    //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                                    srchsvr.savesettings(0, true);
                                }
                            }
                        }
                        else
                        {
                            vec11 = GUI.skin.button.CalcSize(new GUIContent("Buyer"));
                            if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2f - datelength / 2f - 2f, recto.screenRect.yMin - 4f, vec11.x, 20f), "Buyer"))
                            {
                                if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                                if (srchsvr.sortmode == 3) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                                srchsvr.sortmode = 3;
                                if (helpf.bothmenue)
                                {
                                    ah.setBuySortMode(AuctionHouse.SortMode.SELLER);

                                    srchsvr.savesettings(0, false);
                                }
                                //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                            }
                        }
                    }
                    datebeginn = recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2 - datelength / 2 - 2 + vec11.x;
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Price"));
                    if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + 2f * recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + 2 * recto.costIconWidth + recto.labelsWidth / 4f - vec11.x / 2, recto.screenRect.yMin - 4, vec11.x, 20), "Price"))
                    {
                        if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                        if (srchsvr.sortmode == 2) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                        srchsvr.sortmode = 2;

                        if (helpf.bothmenue)
                        {
                            if (wtsmenue)
                            {
                                ah.setSellSortMode(AuctionHouse.SortMode.PRICE);
                            }
                            else
                            {
                                ah.setBuySortMode(AuctionHouse.SortMode.PRICE);
                            }
                            if (wtsmenue) { srchsvr.savesettings(0, true); } else { srchsvr.savesettings(0, false); }
                        }
                        //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                    }
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Date"));
                    //if (GUI.Button(new Rect(this.innerRect.x + offX , this.screenRect.yMin - 4, vec11.x * 2, 20), "Date"))
                    if (GUI.Button(new Rect(datebeginn + 4, recto.screenRect.yMin - 4, vec11.x, 20), "Date"))
                    {
                        if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                        if (srchsvr.sortmode == 0) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                        srchsvr.sortmode = 0;
                        if (wtsmenue)
                        {
                            ah.setSellSortMode(AuctionHouse.SortMode.TIME);
                        }
                        else
                        {
                            ah.setBuySortMode(AuctionHouse.SortMode.TIME);
                        }
                        if (wtsmenue) { srchsvr.savesettings(0, true); } else { srchsvr.savesettings(0, false); }
                        //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                    }
                }
                else
                {
                    GUI.skin = helpf.cardListPopupSkin;
                    GUI.Box(new Rect(recto.position.x + recto.position.width * 0.03f, recto.screenRect.yMin - 4, (recto.position.xMax - recto.position.width * 0.03f) - (recto.position.x + recto.position.width * 0.03f), 20), "");
                    GUI.skin = helpf.cardListPopupBigLabelSkin;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    if (this.helpf.addmode)
                    {
                        GUI.Label(new Rect(recto.position.x + recto.position.width * 0.03f, recto.screenRect.yMin - 4, (recto.position.xMax - recto.position.width * 0.03f) - (recto.position.x + recto.position.width * 0.03f), 20), "auctions you want to create");
                    }
                    else
                    {
                        GUI.Label(new Rect(recto.position.x + recto.position.width * 0.03f, recto.screenRect.yMin - 4, (recto.position.xMax - recto.position.width * 0.03f) - (recto.position.x + recto.position.width * 0.03f), 20), "own auctions");
                    }
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                }


                int num = 0;
                Card card = null;

                // set drawn cards
                if (helpf.createAuctionMenu)
                {
                    if (this.helpf.addmode)
                    {
                        this.ahlist = this.ps.getAddOffers();
                    }
                    else
                    {
                        this.ahlist = this.ps.getOwnOffers();
                    }
                }
                else
                {
                        if (wtsmenue) this.ahlist = ah.getSellOffers();
                        else this.ahlist = ah.getBuyOffers();
                }



                DateTime currenttime = DateTime.Now;
                // draw auctimes################################################
                //timefilter: 
                int time = 0;
                bool usetimefilter = false;
                float anzcards  = (float)this.ahlist.Count();
                if (srchsvr.timesearchstring != "")//doesnt show "old" offers filtered by time-filter
                {
                    time = Convert.ToInt32(srchsvr.timesearchstring);
                    currenttime = DateTime.Now.AddMinutes(-1 * time); usetimefilter = true;
                    anzcards = (float)this.ahlist.Count(delegate(Auction p1) { return (p1.time).CompareTo(currenttime) >= 0; });
                }
                
                GUI.skin = helpf.cardListPopupSkin;
                if (wtsmenue)
                {
                    this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }
                else
                {
                    this.scrollPos2 = GUI.BeginScrollView(recto.position3, this.scrollPos2, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }
                if (srchsvr.reverse || (this.helpf.createAuctionMenu)) { this.ahlist.Reverse(); }
                GUI.skin = helpf.cardListPopupBigLabelSkin;

                float testy = this.scrollPos.y;
                if (!wtsmenue) testy = this.scrollPos2.y;

                foreach (Auction current in this.ahlist)
                {
                    if (usetimefilter && (current.time).CompareTo(currenttime) < 0) { continue; }
                    if (!current.card.tradable)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    GUI.skin = helpf.cardListPopupGradientSkin;
                    //draw boxes
                    Rect position7 = recto.position7(num);
                    

                    if (position7.yMax < testy || position7.y > testy + recto.position3.height)
                    {
                        num++;
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.Box(position7, string.Empty);

                        string name = current.card.getName();

                        string txt = helpf.cardIDtoimageid(current.card.getType()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        if (sttngs.shownumberscrolls) name = "(" + helpf.cardIDToNumberOwned[current.card.getType()] + ") " + name;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                        // draw text
                        Rect position8 = recto.position8(num);
                        GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                        GUI.skin = helpf.cardListPopupSkin;
                        string text = current.card.getPieceKind().ToString();
                        string str = text.Substring(0, 1) + text.Substring(1).ToLower();
                        string text2 = string.Empty;
                        int num2 = recto.maxCharsRK;
                        if (current.card.level > 0)
                        {
                            string text3 = text2;
                            text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.card.level + 1, "</color>, " });
                            num2 += "<color=#rrggbb></color>".Length;
                        }
                        text2 = text2 + current.card.getRarityString() + ", " + str;
                        Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(text2));

                        Rect position9 = recto.position9(num);
                        GUI.Label(position9, (vector2.x >= position9.width) ? (text2.Substring(0, Mathf.Min(text2.Length, num2)) + "...") : text2);
                        Rect restyperect = recto.restyperect(num);
                        //draw resource type
                        this.RenderCost(restyperect, current.card);
                        //draw seller name

                        string sellername = current.seller;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;

                        vector = GUI.skin.label.CalcSize(new GUIContent(sellername));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                        //Rect position11 = new Rect(restyperect.xMax + 2f, (float)num * this.fieldHeight, this.labelsWidth, this.fieldHeight);
                        Rect position11 = new Rect(restyperect.xMax + 2f, position8.yMin, recto.labelsWidth, recto.cardHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position11, (vector.x >= position11.width) ? (sellername.Substring(0, Mathf.Min(sellername.Length, recto.maxCharsName)) + "...") : sellername);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        //draw timestamp
                        GUI.skin = helpf.cardListPopupSkin;
                        DateTime temptime = DateTime.Now;
                        if (helpf.createAuctionMenu)
                        {
                            if (current.message.Split(';')[0] == "sold")
                            { sellername = "sold"; }
                            else
                            {
                                TimeSpan ts = temptime.Subtract(current.time);
                                ts = (current.time).Subtract(temptime);
                                if (ts.TotalHours >= 1.0)
                                {
                                    sellername = ((int)ts.TotalHours + 1) + " hours left";
                                }
                                else
                                {

                                    if (ts.TotalMinutes >= 1.0)
                                    {
                                        sellername = (ts.Minutes + 1) + " min left";

                                    }
                                    else
                                    {
                                        sellername = "ends in a minute";
                                        if (ts.Seconds <= 40) { sellername = "ends in 40 seconds"; }
                                        if (ts.Seconds <= 20) { sellername = "ends in 20 seconds"; }
                                        if (ts.Seconds <= 10) { sellername = "ends in 10 seconds"; }
                                        if (ts.Seconds <= 5) { sellername = "ends in 5 seconds"; }
                                        if (ts.Seconds <= 0) { sellername = "ended"; }
                                        if (ts.TotalSeconds <= 0 && current.message.Split(';')[4] != App.MyProfile.ProfileInfo.id) deleteOldEntrys = true;
                                    }
                                }
                            }

                        }
                        else
                        {
                            TimeSpan ts = temptime.Subtract(current.time);
                            if (ts.TotalMinutes >= 1.0)
                            {
                                sellername = "" + ts.Minutes + " minutes ago";
                                if (ts.TotalMinutes >= helpf.deleteTime) deleteOldEntrys = true;
                            }
                            else
                            {
                                //sellername = "" + ts.Seconds + " seconds ago"; // to mutch changing numbers XD
                                if (ts.Seconds >= 40) { sellername = "40 seconds ago"; }
                                else if (ts.Seconds >= 20) { sellername = "20 seconds ago"; }
                                else if (ts.Seconds >= 10) { sellername = "10 seconds ago"; }
                                else if (ts.Seconds >= 5) { sellername = "5 seconds ago"; }
                                else sellername = "seconds ago";
                            }
                        }

                        Rect position13 = new Rect(restyperect.xMax + 2f, position9.y, recto.labelsWidth, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position13, sellername);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;
                        //testonly
                        //    restyperect = new Rect(position11.xMax , (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight);
                        //       draw resource type
                        //     this.RenderCost(restyperect, current);

                        //draw gold cost
                        float nextx = position11.xMax + recto.costIconWidth;
                        string gold = current.price + "g";
                        if (current.price == 0) gold = "?";
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        //vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                        //Rect position12 = new Rect(position7.xMax-recto.goldlength-2f, position8.yMin, recto.goldlength, recto.cardHeight);
                        Rect position12 = new Rect(nextx + 2f, position8.yMin, recto.labelsWidth / 2f, recto.cardHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position12, gold);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                        // draw suggested price
                        //int index = Array.FindIndex(helpf.cardids, element => element == current.card.getType());
                        int index = helpf.cardidToArrayIndex(current.card.getType());
                        string suggeprice = "";
                        if (index >= 0)
                        {
                            int p1 = 0, p2 = 0;
                            if (wtsmenue)
                            {
                                p1 = prcs.getPrice(index, sttngs.wtsAHpriceType);
                            }
                            else
                            {
                                p1 = prcs.getPrice(index, sttngs.wtbAHpriceType);
                            }
                            suggeprice = "SG: " + p1;
                            if (sttngs.showsugrange)
                            {
                                if (wtsmenue)
                                {
                                    p2 = prcs.getPrice(index, sttngs.wtsAHpriceType2);
                                }
                                else
                                {
                                    p2 = prcs.getPrice(index, sttngs.wtbAHpriceType2);
                                }
                            }
                            if (sttngs.showsugrange && p1 != p2) suggeprice = "SG: " + Math.Min(p1, p2) + "-" + Math.Max(p1, p2);
                        }
                        GUI.skin = helpf.cardListPopupSkin;
                        Rect position14 = new Rect(nextx + 2f, position9.y, recto.labelsWidth / 2f, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position14, suggeprice);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;



                        GUI.skin = helpf.cardListPopupLeftButtonSkin;
                        if (!this.selectable)
                        {
                            GUI.enabled = false;
                        }
                        if (GUI.Button(recto.position10(num), string.Empty) && current.card.tradable)
                        {
                            card = current.card;
                            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                        }
                        if (!this.selectable)
                        {
                            GUI.enabled = true;
                        }
                        //draw picture
                        if (texture != null)
                        {
                            GUI.DrawTexture(new Rect(4f, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight), texture);
                        }
                        // draw buy/sell button
                        GUI.skin = helpf.cardListPopupGradientSkin;
                        if (helpf.createAuctionMenu) // dont need a sell button in own-auctions-menu
                        {
                            if (helpf.addmode)
                            {
                                if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                {
                                    this.ps.delOffer(current);
                                }
                                GUI.skin = helpf.cardListPopupBigLabelSkin;
                                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Del");


                            }
                        }
                        else
                        {
                            if (wtsmenue)
                            {
                                if (!helpf.showtradedialog)
                                {
                                    if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                    {
                                        // start trading with seller
                                        if (current.seller != App.MyProfile.ProfileInfo.name)
                                        {
                                            helpf.showtradedialog = true;
                                            tradeitem = current;
                                            this.bothstarttrading = wtsmenue;
                                        }
                                    }
                                }
                                else { GUI.Box(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                                GUI.skin = helpf.cardListPopupBigLabelSkin;
                                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                if (helpf.bothmenue) GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Buy");

                            }
                            else
                            {
                                if (!helpf.showtradedialog)
                                {
                                    if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                    {
                                        // start trading with seller
                                        if (current.seller != App.MyProfile.ProfileInfo.name)
                                        {
                                            helpf.showtradedialog = true;
                                            tradeitem = current;
                                            this.bothstarttrading = wtsmenue;
                                        }
                                    }
                                }
                                else { GUI.Box(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                                GUI.skin = helpf.cardListPopupBigLabelSkin;
                                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Sell");
                            }
                        }
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        if (!current.card.tradable)
                        {
                            GUI.color = Color.white;
                        }
                        num++;
                    }
                }

                GUI.EndScrollView();
                
                // delete old entrys
                if (deleteOldEntrys) { ah.removeOldEntrys(); };

                GUI.color = Color.white;
                // show clicked card
                if (card != null)
                {
                    //this.callback.ItemButtonClicked(this, card);
                    string clink = card.getName().ToLower();
                    //int arrindex = Array.FindIndex(helpf.cardnames, element => element.Equals(clink));
                    int arrindex = helpf.cardnameToArrayIndex(clink);
                    if (arrindex >= 0)
                    {

                        crdvwr.createcard(arrindex, helpf.cardids[arrindex]);
                    }

                }
                //wts / wtb menue buttons

                if (wtsmenue)
                {

                    this.drawButtonsBelow();
                    
                }

                if (!wtsmenue && helpf.createAuctionMenu)
                {
                    if (helpf.addmode)
                    {
                        if (GUI.Button(recto.updateGoogleThings, "Clear"))
                        {
                            this.ps.clearAuctions();
                            
                        }

                        if (GUI.Button(recto.getOwnStuffButton, "Create") && this.sttngs.waitForAuctionBot == false && this.sttngs.actualTrading == false)
                        {
                            this.helpf.createdAuctionText = "";

                            App.Popups.ShowOkCancel(this, "wantToCreateMultiAuction", "You are sure?", "You really want to create these Auctions?\r\n" + this.helpf.createdAuctionText, "OK", "Cancel");
                        }
                    }
                    else
                    {
                        if (GUI.Button(recto.updateGoogleThings, "Update"))
                        {
                            if (this.gglthngs.workthreadready) new Thread(new ThreadStart(this.gglthngs.workthread)).Start();
                        }

                        // draw button for getting cards here!
                        if (GUI.Button(recto.getOwnStuffButton, "GetStuff") && this.sttngs.waitForAuctionBot == false && this.sttngs.actualTrading == false)
                        {
                            string sendmessage = " \\getauc " + "profileid:" + App.MyProfile.ProfileInfo.id + ",";
                            WhisperMessage wmsg = new WhisperMessage(this.twb.botname, sendmessage);
                            this.sttngs.waitForAuctionBot = true;
                            this.sttngs.tradeCardID = 0;
                            this.sttngs.waitForAuctionBot = true;
                            App.Communicator.sendRequest(wmsg);
                            this.sttngs.AucBotMode = "getauc";

                        }
                    }
                }

                GUI.color = Color.white;  

            }
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            if (!wtsmenue)
            {
                recto.innerRect.x = recto.innerRect.x - Screen.width * 0.495f;
                recto.innerBGRect.x = recto.innerBGRect.x - Screen.width * 0.495f;
                recto.outerRect.x = recto.outerRect.x - Screen.width * 0.495f;
                recto.screenRect.x = recto.screenRect.x - Screen.width * 0.495f;
                recto.filtermenurect.x = recto.filtermenurect.x - Screen.width * 0.495f;
                recto.calcguirects();
            }
        }

        private void drawCreateMenu()
        {


            GUI.depth = 15;
            //draw boxes
            GUI.skin = helpf.cardListPopupSkin;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
            GUI.Box(recto.position, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
            GUI.Box(recto.position2, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

            drawButtonsBelow();

                    if (helpf.offerMenuSelectCardMenu)
                    {
                        //this.ccardlist(false);// want to show the sell prices
                        this.drawgenerator();
                        return;
                    }

                    if (this.sttngs.actualTrading)
                    {
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(recto.tbmessage, "...struggle with bot...");
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        return;
                    }
                    if (this.sttngs.waitForAuctionBot)
                    {
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(recto.tbmessage, "...wait for bot...");
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        return;
                    }


            GUI.skin = helpf.cardListPopupSkin;
            //GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string cname = ""; int cid = 0; long cardid = 0;
            if (this.OfferCard != null)
            {
                cname = OfferCard.getName(); cid = OfferCard.getType(); cardid = OfferCard.getId();

                int anzcard = helpf.cardIDToNumberOwned[cid];
                int index = helpf.cardidToArrayIndex(cid);
                string message = "You want to create an auction for a " + cname + " (SG:" + prcs.getPrice(index, sttngs.wtbAHpriceType) + ")" + "\r\nYou own this card " + anzcard + " times.\r\n";
                string yourmessage1 = "WTS my " + cname + " for " + this.OfferPrice + "g.";
                GUI.Label(recto.crtmessage, message + yourmessage1);

                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.crtpriceinput, string.Empty);
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                helpf.chatLogStyle.alignment = TextAnchor.MiddleCenter;
                this.OfferPrice = Regex.Replace(GUI.TextField(recto.crtpriceinput, this.OfferPrice, helpf.chatLogStyle), @"[^0-9]", "");
                helpf.chatLogStyle.alignment = TextAnchor.MiddleLeft;
                if (this.OfferPrice == "") this.OfferPrice = "0";
                if (Convert.ToInt32(this.OfferPrice) > 999999) this.OfferPrice = "999999";
                GUI.skin = helpf.cardListPopupBigLabelSkin;

                if (GUI.Button(recto.crtdurationInput, ""))
                {
                    this.durationIndex = (this.durationIndex + 1) % 5;
                }

                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                if (this.durationIndex == 0) { GUI.Label(recto.crtdurationInput, "12h"); }
                if (this.durationIndex == 1) { GUI.Label(recto.crtdurationInput, "24h"); }
                if (this.durationIndex == 2) { GUI.Label(recto.crtdurationInput, "48h"); }
                if (this.durationIndex == 3) { GUI.Label(recto.crtdurationInput, "60h"); }
                if (this.durationIndex == 4) { GUI.Label(recto.crtdurationInput, "72h"); }

                GUI.Label(recto.crtororand, "for");
                GUI.Label(recto.crtduration, "Duration:");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }

            if (true)
            {
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.crtcard, string.Empty);
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                if (GUI.Button(recto.crtcard, ""))
                {
                    helpf.offerMenuSelectCardMenu = true;
                    recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                    srchsvr.setsettings(2, false);
                }

                if (this.OfferCard != null)
                {
                    Texture texture = App.AssetLoader.LoadTexture2D(helpf.cardIDtoimageid(this.OfferCard.getType()).ToString());//current.getCardImage())
                    if (texture != null)
                    {
                        GUI.DrawTexture(new Rect(recto.crtcard.x + 4f, recto.crtcard.y + 4f, 2 * recto.cardWidth, 2 * recto.cardHeight), texture);
                    }
                }

            }

            if (GUI.Button(recto.tbadd, "+"))
            {
                this.helpf.addmode = true;
                bool offeredCardInYouPossesion = true;
                Auction add = null;
                foreach (Auction a in this.ps.getAddOffers())
                {
                    if (a.card.id == this.OfferCard.id)
                    {
                        offeredCardInYouPossesion = false; 
                        break; 
                    }
                }
                if (offeredCardInYouPossesion && this.OfferCard != null && this.OfferPrice != "0")
                {
                    int duration = 12;
                    if (this.durationIndex == 1) duration = 24;
                    if (this.durationIndex == 2) duration = 48;
                    if (this.durationIndex == 3) duration = 60;
                    if (this.durationIndex == 4) duration = 72;

                    // final test if the generated message may be to long to post:

                    string genAucMessage = " \\msetauc " + "profileid:" + App.MyProfile.ProfileInfo.id + ", duration:"+duration+"h, data:";
                    string tradedata = "";
                    foreach (Auction a in this.ps.getAddOffers())
                    {
                            if (tradedata != "") tradedata = tradedata + ";";
                            tradedata = tradedata + a.card.id + ":" + a.price +":"+ a.message.Split(';')[5];
                        

                       
                    }
                    // add the new data to the testmessage
                    if (tradedata != "") tradedata = tradedata + ";";
                    tradedata = tradedata + this.OfferCard.getId() + ":" + this.OfferPrice + ":" + duration;
                    tradedata = tradedata + ",";
                    string sendmessage = genAucMessage + tradedata;
                    if (sendmessage.Length <= 506)
                    {
                        string id = "active;" + this.OfferCard.getType() + ";" + this.OfferCard.getId() + ";" + this.OfferPrice + ";" + App.MyProfile.ProfileInfo.id + ";" + duration;
                        Auction au = new Auction(App.MyProfile.ProfileInfo.name, DateTime.Now.AddHours((double)duration), Auction.OfferType.SELL, this.OfferCard, id, Convert.ToInt32(this.OfferPrice));
                        this.ps.addOffer(au);
                    }
                }
     

            }
            /*
            if (GUI.Button(recto.tbok, "OK"))
            {
                if (helpf.addmode)
                {
                    this.helpf.createdAuctionText = "all the stuff on the right side=>";

                    App.Popups.ShowOkCancel(this, "wantToCreateMultiAuction", "You are sure?", "You really want to create these Auctions?\r\n" + this.helpf.createdAuctionText, "OK", "Cancel");
                }
                else
                {
                    bool offeredCardInYouPossesion = false;
                    foreach (Auction a in this.helpf.allOwnTradeableAuctions)
                    {
                        if (a.card == this.OfferCard) { offeredCardInYouPossesion = true; break; }
                    }
                    if (offeredCardInYouPossesion && this.OfferCard != null && this.OfferPrice != "0")
                    {
                        string duration = "12";
                        if (this.durationIndex == 1) duration = "24";
                        if (this.durationIndex == 2) duration = "48";
                        if (this.durationIndex == 3) duration = "60";
                        if (this.durationIndex == 4) duration = "72";
                        this.helpf.createdAuctionText = "Wts " + cname + " for " + this.OfferPrice + "g.\r\nThe auction ends in " + duration + "hours";
                        this.sttngs.tradeCardID = cardid;
                        //this.OfferPrice = "0"; this.OfferCard = null;

                        App.Popups.ShowOkCancel(this, "wantToCreateAuction", "You are sure?", "You really want to create the following Auction?\r\n" + this.helpf.createdAuctionText, "OK", "Cancel");

                    }
                }
                
            }*/
            

        }

        private void starttrading(string name, string cname, int price, bool wts, string orgmsg, int cid, int level)
        {
            
            // asks the user if he wants to trade
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "sell";
            if (wts) text = "buy";
            int anzcard = helpf.cardIDToNumberOwned[cid];
            string message = "You want to " + text + "\r\n" + cname + " for " + price + "g" + "\r\nYou own this card " + anzcard + " times\r\n\r\nOriginal Message:";
            if (this.helpf.playerStoreMenu) message = "You want to buy\r\n" + cname + " for " + price + "g" + "\r\nYou own this card " + anzcard + " times";
            GUI.Label(recto.tbmessage, message);
            GUI.skin.label.wordWrap = true;
            float msghigh = GUI.skin.label.CalcHeight(new GUIContent(orgmsg), recto.tbmessage.width - 30f);
            GUI.skin = helpf.cardListPopupSkin;
            if (!this.helpf.playerStoreMenu)
            {
                scrolll = GUI.BeginScrollView(recto.tbmessagescroll, scrolll, new Rect(0f, 0f, recto.tbmessagescroll.width - 20f, msghigh));
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                GUI.Label(new Rect(5f, 5f, recto.tbmessagescroll.width - 30f, msghigh), orgmsg);
                //Console.WriteLine(message);
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUI.EndScrollView();
            }
            GUI.skin.label.wordWrap = false;
            GUI.skin = helpf.cardListPopupLeftButtonSkin;

            if (GUI.Button(recto.tbok, "OK"))
            {
                if (!this.helpf.playerStoreMenu)
                {
                    helpf.showtradedialog = false;
                    App.GameActionManager.TradeUser(helpf.globalusers[name]);
                    helpf.postmsgmsg = "You want to " + text + ": " + cname + " for " + price + "g " + ". You own this card " + anzcard + " times. Original Message:" + "\r\n" + orgmsg;
                    helpf.postmsgontrading = true;
                }
                else
                {
                    if (!(this.sttngs.actualTrading || this.sttngs.waitForAuctionBot))
                    {
                        helpf.showtradedialog = false;
                        // \pidauc profileid:894cb62d9bca4791bfa77d0659f2c7d8, target:232;20958472;price,
                        if (orgmsg.Split(';')[0] == "active")
                        {
                            string target = orgmsg.Split(';')[1] + ";" + orgmsg.Split(';')[2] + ";" + orgmsg.Split(';')[3] + ";" + orgmsg.Split(';')[4] + ";" + orgmsg.Split(';')[5];
                            if (level >= 1) target = target + ";" + level;
                            string sendmessage = " \\pidauc " + "profileid:" + App.MyProfile.ProfileInfo.id + ", target:" + target + ",";
                            WhisperMessage wmsg = new WhisperMessage(this.twb.botname, sendmessage);
                            this.sttngs.waitForAuctionBot = true;
                            this.sttngs.bidgold = price;
                            this.sttngs.tradeCardID = Convert.ToInt64(orgmsg.Split(';')[2]);
                            App.Communicator.sendRequest(wmsg);
                            this.sttngs.waitForAuctionBot = true;
                            this.sttngs.AucBotMode = "bidauc";
                        }
                    }

                }
            };
            if (!this.helpf.playerStoreMenu && GUI.Button(recto.tbwhisper, "Whisper") )
            {
                helpf.showtradedialog = false;
                App.ArenaChat.ChatRooms.OpenWhisperRoom(name);
            };
            if (GUI.Button(recto.tbcancel, "Cancel")) { helpf.showtradedialog = false; };
            //if ( !this.helpf.playerStoreMenu && GUI.Button(recto.tboffer, "Offer") ) { helpf.makeOfferMenu = true; this.OrOrAnd = 0; this.OfferPrice = "0"; this.OfferCard = null; };
        }


        private void ccardlist(bool wtsmenue)//select card you want to offer
        {
            // dont know how to get a carlistpopup to work
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.innerBGRect, "");
            
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
            Rect innerRect = new Rect(recto.innerBGRect.x + 12f, recto.innerBGRect.y + 12f, recto.innerBGRect.width - 24f, recto.tbok.y - recto.innerBGRect.yMin - 12f);
            GUI.Box(innerRect, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
            
            List<Auction> offlist=null;

            if (!wtsmenue) offlist = this.helpf.allOwnTradeableAuctions; // all cards YOU own ( the other user wants to buy
            else offlist = Generator.Instance.getAllOwnBuyOffers(); // all cards available
            this.scrollPos4 = GUI.BeginScrollView(innerRect, this.scrollPos4, new Rect(0f, 0f, innerRect.width - 20f, recto.fieldHeight * offlist.Count));
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            Card card = null;
            //Draw stuff
            int num = 0;
            float testy = this.scrollPos4.y;
            foreach (Auction current in offlist)
            {

                GUI.skin = helpf.cardListPopupGradientSkin;
                //draw boxes
                Rect position7 = new Rect(recto.cardWidth + 10f, (float)num * recto.fieldHeight, innerRect.width - recto.scrollBarSize  - recto.costIconWidth - 12f, recto.fieldHeight);

                if (position7.yMax < testy || position7.y > testy + innerRect.height)
                {
                    num++;
                    GUI.color = Color.white;
                }
                else
                {

                        if (GUI.Button(position7, string.Empty))
                        {
                            card = current.card;
                            //this.callback.ItemClicked(this, current);
                        }



                    Texture texture = null;
                    Vector2 vector;
                    string name = current.card.getName();

                    string txt = helpf.cardIDtoimageid(current.card.getType()).ToString();
                    texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())

                    GUI.skin = helpf.cardListPopupLeftButtonSkin;
                    Rect position10 = recto.position10(num);
                    if (GUI.Button(position10, string.Empty))
                    {
                        if (current.card != null) card = current.card;
                        App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                    }
                    //draw picture
                    if (texture != null)
                    {
                        GUI.DrawTexture(new Rect(position10.x + 4f, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight), texture);
                    }



                        if (sttngs.shownumberscrolls) name = "(" + helpf.cardIDToNumberOwned[current.card.getType()] + ") " + name;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        vector = GUI.skin.label.CalcSize(new GUIContent(name));


                        float nextx = position10.xMax + 8f;
                        //drawcardname
                        Rect position12 = new Rect(nextx + 2f, (float)num * recto.fieldHeight - 3f + recto.fieldHeight * 0.01f, recto.labelsWidth, recto.cardHeight);

                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position12, (vector.x >= position12.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        GUI.skin = helpf.cardListPopupSkin;
                        string text = current.card.getPieceKind().ToString();
                        string str = text.Substring(0, 1) + text.Substring(1).ToLower();
                        string text2 = string.Empty;
                        int num2 = recto.maxCharsRK;
                        if (current.card.level > 0)
                        {
                            string text3 = text2;
                            text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.card.level + 1, "</color>, " });
                            num2 += "<color=#rrggbb></color>".Length;
                        }
                        text2 = text2 + current.card.getRarityString() + ", " + str;
                        Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(text2));

                        //draw cardsubstring
                        Rect position13 = new Rect(nextx + 2f, (float)num * recto.fieldHeight - 3f + recto.fieldHeight * 0.57f, recto.labelsWidth, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position13, (vector2.x >= position13.width) ? (text2.Substring(0, Mathf.Min(text2.Length, num2)) + "...") : text2);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;

                        // draw suggested price
                        //int index = Array.FindIndex(helpf.cardids, element => element == current.card.getType());
                        int index = helpf.cardidToArrayIndex(current.card.getType());
                        string suggeprice = "";
                        if (index >= 0)
                        {
                            int p1 = 0, p2 = 0;
                            if (wtsmenue)
                            {
                                p1 = prcs.getPrice(index, sttngs.wtsAHpriceType);
                            }
                            else
                            {
                                p1 = prcs.getPrice(index, sttngs.wtbAHpriceType);
                            }
                            suggeprice = "SG: " + p1;
                            if (sttngs.showsugrange)
                            {
                                if (wtsmenue)
                                {
                                    p2 = prcs.getPrice(index, sttngs.wtsAHpriceType2);
                                }
                                else
                                {
                                    p2 = prcs.getPrice(index, sttngs.wtbAHpriceType2);
                                }
                            }
                            if (sttngs.showsugrange && p1 != p2) suggeprice = "SG: " + Math.Min(p1, p2) + "-" + Math.Max(p1, p2);
                        }
                        nextx = position12.xMax + recto.costIconWidth;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        Rect position14 = new Rect(nextx + 2f, position12.y, recto.labelsWidth / 2f, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position14, suggeprice);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    

                   
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    num++;
                }
            }

            GUI.EndScrollView();
            this.OfferCard = card;
            if (card != null) { helpf.offerMenuSelectCardMenu = false; this.OfferPrice = "1000"; }
        }


        public void drawgenerator()
        {
            // have to draw textfield in front of scrollbar or otherwise you lose focus in textfield (lol)
            GUI.depth = 15;
            bool clickableItems = true;
            if (crdvwr.cardReadyToDraw) clickableItems = false;
            if (true)
            {
                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.filtermenurect, string.Empty);
                // wts filter menue
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                GUI.Label(recto.sbarlabelrect, "Scroll:");
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.sbrect, string.Empty);
                string selfcopy = srchsvr.wtssearchstring;
                GUI.SetNextControlName("dbSearchfield");
                srchsvr.wtssearchstring = GUI.TextField(recto.sbrect, srchsvr.wtssearchstring, helpf.chatLogStyle);
                recto.drawsearchpulldown();// draw here to be the pull down menue the first clicked object

                GUI.contentColor = Color.white;
                GUI.color = Color.white;
                if (!srchsvr.growthbool) { GUI.color = dblack; }
                bool growthclick = GUI.Button(recto.sbgrect, growthres);
                GUI.color = Color.white;
                if (!srchsvr.orderbool) { GUI.color = dblack; }
                bool orderclick = GUI.Button(recto.sborect, orderres);
                GUI.color = Color.white;
                if (!srchsvr.energybool) { GUI.color = dblack; }
                bool energyclick = GUI.Button(recto.sberect, energyres);
                GUI.color = Color.white;
                if (!srchsvr.decaybool) { GUI.color = dblack; }
                bool decayclick = GUI.Button(recto.sbdrect, decayres);


                GUI.contentColor = Color.gray;
                GUI.color = Color.white;
                if (!srchsvr.commonbool) { GUI.color = dblack; }
                bool commonclick = GUI.Button(recto.sbcommonrect, "C");
                GUI.color = Color.white;
                if (!srchsvr.uncommonbool) { GUI.color = dblack; }
                GUI.contentColor = Color.white;
                bool uncommonclick = GUI.Button(recto.sbuncommonrect, "U");
                GUI.color = Color.white;
                if (!srchsvr.rarebool) { GUI.color = dblack; }
                GUI.contentColor = Color.yellow;
                bool rareclick = GUI.Button(recto.sbrarerect, "R");
                GUI.color = Color.white;
                if (!srchsvr.threebool) { GUI.color = dblack; }
                //if (!p1mt3bool) { GUI.color = dblack; }
                bool mt3click;
                bool mt0click = false;

                mt3click = GUI.Button(recto.sbthreerect, ">3"); // >3 bei wtsmenue=false

                GUI.color = Color.white;
                if (!srchsvr.onebool) { GUI.color = dblack; }
                //if (this.wtsmenue) { mt0click = GUI.Button(sbonerect, ">0"); };
                GUI.color = Color.white;
                GUI.contentColor = Color.white;


                // draw price filter

                GUI.skin = helpf.cardListPopupBigLabelSkin;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(recto.sbpricelabelrect, "<= Price <=");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;


                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.sbpricerect, string.Empty);
                GUI.Box(recto.sbpricerect2, string.Empty);
                string pricecopy = srchsvr.pspricesearchstring;
                string pricecopy2 = srchsvr.pspricesearchstring2;
                srchsvr.pspricesearchstring = Regex.Replace(GUI.TextField(recto.sbpricerect, srchsvr.pspricesearchstring, helpf.chatLogStyle), @"[^0-9]", "");
                srchsvr.pspricesearchstring2 = Regex.Replace(GUI.TextField(recto.sbpricerect2, srchsvr.pspricesearchstring2, helpf.chatLogStyle), @"[^0-9]", "");
                GUI.color = Color.white;

                GUI.color = Color.white;
                GUI.contentColor = Color.red;
                bool closeclick = GUI.Button(recto.sbclearrect, "X");
                GUI.contentColor = Color.white;

                GUI.skin = helpf.cardListPopupSkin;

                if (recto._showSearchDropdown) recto.OnGUI_drawSearchPulldown(recto.sbrect);// draw pulldown again (for overlay)

                if (growthclick) { srchsvr.growthbool = !srchsvr.growthbool; };
                if (orderclick) { srchsvr.orderbool = !srchsvr.orderbool; }
                if (energyclick) { srchsvr.energybool = !srchsvr.energybool; };
                if (decayclick) { srchsvr.decaybool = !srchsvr.decaybool; }
                if (commonclick) { srchsvr.commonbool = !srchsvr.commonbool; };
                if (uncommonclick) { srchsvr.uncommonbool = !srchsvr.uncommonbool; }
                if (rareclick) { srchsvr.rarebool = !srchsvr.rarebool; };
                if (mt3click) { srchsvr.threebool = !srchsvr.threebool; }
                if (mt0click) { srchsvr.onebool = !srchsvr.onebool; }
                if (closeclick)
                {
                    ps.createCardsFilter.resetFilters();

                    srchsvr.resetpssearchsettings(helpf.wtsmenue);
                }
                
                 

                if (selfcopy != srchsvr.wtssearchstring)
                {
                    ps.createCardsFilter.setCardFilter(srchsvr.wtssearchstring);

                }
                if (mt3click || mt0click)
                {
                    int filter = 0;
                    if (srchsvr.threebool) filter = 2;
                    ps.createCardsFilter.setAmountFilter(filter);

                }
                if (growthclick || orderclick || energyclick || decayclick)
                {
                    string[] res = { "", "", "", "" };
                    if (srchsvr.decaybool) { res[0] = "decay"; };
                    if (srchsvr.energybool) { res[1] = "energy"; };
                    if (srchsvr.growthbool) { res[2] = "growth"; };
                    if (srchsvr.orderbool) { res[3] = "order"; };

                    ps.createCardsFilter.setResourceFilter(res);

                }
                if (commonclick || uncommonclick || rareclick)
                {
                    ps.createCardsFilter.setRarityFilter(srchsvr.commonbool, srchsvr.uncommonbool, srchsvr.rarebool);

                }

                if (pricecopy != srchsvr.pspricesearchstring)
                {
                    ps.createCardsFilter.setPriceLowerBound(srchsvr.pspricesearchstring);
                }
                if (pricecopy2 != srchsvr.pspricesearchstring2)
                {

                    ps.createCardsFilter.setPriceUpperBound(srchsvr.pspricesearchstring2);
                }


                srchsvr.savesettings(2, false);


            }

            // Draw generator here:
            if (true)
            {
                //Console.WriteLine(GUI.GetNameOfFocusedControl());
                this.opacity = 1f;
                GUI.skin = helpf.cardListPopupSkin;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
                GUI.Box(recto.position, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
                GUI.Box(recto.position2, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

                this.ahlist = ps.getCreateOffers();

                this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * (float)this.ahlist.Count));
                int num = 0;
                Card card = null;
                Card clickcard = null;
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                foreach (Auction current in this.ahlist)
                {

                    if (!current.card.tradable)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    GUI.skin = helpf.cardListPopupGradientSkin;
                    //draw boxes
                    Rect position7 = recto.position7(num);
                    if (position7.yMax < this.scrollPos.y || position7.y > this.scrollPos.y + recto.position3.height)
                    {
                        num++;
                        GUI.color = Color.white;
                    }
                    else
                    {
                        if (clickableItems)
                        {
                            if (GUI.Button(position7, string.Empty))
                            {
                                //this.callback.ItemClicked(this, current);
                                clickcard = current.card;
                            }
                        }
                        else
                        {
                            GUI.Box(position7, string.Empty);
                        }
                        string name = current.card.getName();

                        string txt = helpf.cardIDtoimageid(current.card.getType()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        if (sttngs.shownumberscrolls) name = "(" + helpf.cardIDToNumberOwned[current.card.getType()] + ") " + name;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                        // draw text
                        Rect position8 = recto.position8(num);
                        GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                        GUI.skin = helpf.cardListPopupSkin;
                        string text = current.card.getPieceKind().ToString();
                        string str = text.Substring(0, 1) + text.Substring(1).ToLower();
                        string text2 = string.Empty;
                        int num2 = recto.maxCharsRK;
                        if (current.card.level > 0)
                         {
                             string text3 = text2;
                             text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.card.level + 1, "</color>, " });
                             num2 += "<color=#rrggbb></color>".Length;
                         }
                        text2 = text2 + current.card.getRarityString() + ", " + str;
                        Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(text2));
                        Rect position9 = recto.position9(num);
                        GUI.Label(position9, (vector2.x >= position9.width) ? (text2.Substring(0, Mathf.Min(text2.Length, num2)) + "...") : text2);
                        Rect restyperect = recto.restyperect(num);
                        //draw resource type
                        this.RenderCost(restyperect, current.card);
                        // write PRICE
                        //GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        float nextx = restyperect.xMax + recto.costIconWidth / 2;
                        string gold = "";
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        Rect position12 = new Rect(nextx + 2f, (float)num * recto.fieldHeight, recto.labelsWidth / 2f, recto.fieldHeight);
                        //GUI.Label(position12, gold);


                        int index = helpf.cardidToArrayIndex(current.card.getType());
                        string suggeprice = "";
                        int p1 = 0, p2 = 0;
                        if (index >= 0)
                        {
                            
                            //p1 = prcs.getPrice(index, sttngs.wtsAHpriceType);
                             p1 = prcs.getPrice(index, sttngs.wtbAHpriceType);
                            suggeprice = "SG: " + p1;
                            if (sttngs.showsugrange)
                            {
                                
                                //    p2 = prcs.getPrice(index, sttngs.wtsAHpriceType2);
                                    p2 = prcs.getPrice(index, sttngs.wtbAHpriceType2);
                            }
                            if (sttngs.showsugrange && p1 != p2) suggeprice = "SG: " + Math.Min(p1, p2) + "-" + Math.Max(p1, p2);
                        }
                        nextx = position12.xMax + recto.costIconWidth;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        Rect position14 = new Rect(nextx + 2f, position12.y, recto.labelsWidth / 2f, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position14, suggeprice);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                        //draw pricebox
                        //
                        //Rect position11 = new Rect(position12.xMax + 2f, (float)(num + 1) * recto.fieldHeight - (recto.fieldHeight + vector2.y) / 2 - 2, recto.labelsWidth, vector2.y + 4);
                        //GUI.skin = helpf.cardListPopupSkin;
                        //GUI.Box(position11, string.Empty);

                        GUI.skin = helpf.cardListPopupLeftButtonSkin;
                        if (!this.selectable)
                        {
                            GUI.enabled = false;
                        }
                        if (GUI.Button(recto.position10(num), string.Empty) && current.card.tradable)
                        {
                            card = current.card;
                            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                        }
                        if (!this.selectable)
                        {
                            GUI.enabled = true;
                        }
                        //draw picture
                        if (texture != null)
                        {
                            GUI.DrawTexture(new Rect(4f, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight), texture);
                        }

                        if (!current.card.tradable)
                        {
                            GUI.color = Color.white;
                        }
                        num++;
                    }
                }
                GUI.EndScrollView();
                GUI.color = Color.white;
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(recto.footlinerect,"Select a Scroll you want to sell!");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                if (GUI.Button(recto.updatebuttonrect, "Close"))
                {
                    helpf.offerMenuSelectCardMenu = false;
                    recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin); 
                }

                if (card != null)
                {
                    //this.callback.ItemButtonClicked(this, card);
                    string clink = card.getName().ToLower();
                    //int arrindex = Array.FindIndex(helpf.cardnames, element => element.Equals(clink));
                    int arrindex = helpf.cardnameToArrayIndex(clink);
                    if (arrindex >= 0)
                    {
                        crdvwr.createcard(arrindex, helpf.cardids[arrindex]);
                    }
                }

                if (clickcard != null) 
                { 
                    this.OfferCard = clickcard; 
                    helpf.offerMenuSelectCardMenu = false; 
                    this.OfferPrice = "1000"; 
                    recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin); 
                }
                //wts / wtb menue buttons

                GUI.color = Color.white;
                GUI.contentColor = Color.white;
            }
        }





        private void RenderCost(Rect rect, Card card)
        {
            //draw resource-icon
            ResourceType resource = card.getCardType().getResource();
            int cost = card.getCardType().getCost();
            Texture texture = ResourceManager.LoadTexture("BattleUI/battlegui_icon_" + resource.ToString().ToLower());
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture);

            }
        }

        private void drawButtonsBelow()
        {

            if (helpf.wtsmenue && !(helpf.bothmenue || helpf.createAuctionMenu || helpf.playerStoreMenu))
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            if (ah.newSellOffers)
            {
                GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
            }


            if (GUI.Button(recto.wtsbuttonrect, "WTS") && !helpf.showtradedialog)
            {
                recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                helpf.wtsmenue = true; this.wtsinah = true;
                srchsvr.setsettings(0, true);
                ah.setSellSortMode(srchsvr.sortmode);
                helpf.bothmenue = false;
                helpf.createAuctionMenu = false;
                helpf.playerStoreMenu = false;
            }
            GUI.skin.button.normal.textColor = Color.white;
            GUI.skin.button.hover.textColor = Color.white;



            if (!helpf.wtsmenue && !(helpf.bothmenue || helpf.createAuctionMenu || helpf.playerStoreMenu))
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            if (ah.newBuyOffers)
            {
                GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
            }
            if (GUI.Button(recto.wtbbuttonrect, "WTB") && !helpf.showtradedialog)
            {
                //alists.wtblistfull.Clear(); alists.wtblistfull.AddRange(alists.wtblistfulltimed);
                //sortlist(wtblistfull);
                //alists.setAhlistsToAHWtbLists(true);
                recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                helpf.wtsmenue = false; this.wtsinah = false;

                //lstfltrs.sortlist(alists.ahlist);
                srchsvr.setsettings(0, false);
                ah.setBuySortMode(srchsvr.sortmode);
                helpf.bothmenue = false;
                helpf.createAuctionMenu = false;
                helpf.playerStoreMenu = false;
            }
            GUI.skin.button.normal.textColor = Color.white;
            GUI.skin.button.hover.textColor = Color.white;

            GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            if (helpf.bothmenue) GUI.color = Color.white;
            if (GUI.Button(recto.bothbuttonrect, "All") && !helpf.showtradedialog)
            {
                helpf.createAuctionMenu = false;
                helpf.bothmenue = true;
                helpf.playerStoreMenu = false;
                recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
            }

            GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            if (helpf.playerStoreMenu) GUI.color = Color.white;

            if (GUI.Button(recto.auctionhousebuttonrect, "PStr") && !helpf.showtradedialog)
            {
                helpf.createAuctionMenu = false;
                helpf.playerStoreMenu = true;
                helpf.bothmenue = false;
                helpf.wtsmenue = true;
                srchsvr.setsettings(2, true);
                ah.setSellSortMode(srchsvr.sortmode);
                recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);

                if(this.gglthngs.workthreadready) new Thread(new ThreadStart(this.gglthngs.workthread)).Start();

            }


            GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            if (helpf.createAuctionMenu) GUI.color = Color.white;
            if (GUI.Button(recto.createbuttonrect, "Crt") && !helpf.showtradedialog)
            {
                helpf.createAuctionMenu = true;
                helpf.playerStoreMenu = false;
                helpf.bothmenue = false;
                recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);

                if(this.gglthngs.workthreadready) new Thread(new ThreadStart(this.gglthngs.workthread)).Start();
            }
            GUI.color = Color.white;
 
        }

        public void PopupOk(string popupType)
        {
            if (popupType == "wantToCreateAuction")
            {
                string duration = "12";
                if (this.durationIndex == 1) duration = "24";
                if (this.durationIndex == 2) duration = "48";
                if (this.durationIndex == 3) duration = "60";
                if (this.durationIndex == 4) duration = "72";
                string cname = ""; int cid = 0; long cardid = 0;
                cname = OfferCard.getName(); cid = OfferCard.getType(); cardid = OfferCard.getId();
                string sendmessage = " \\setauc " + "profileid:" + App.MyProfile.ProfileInfo.id + ", cardid:" + cardid + ", cardtype:" + cid + ", price:" + this.OfferPrice + ", duration:" + duration + "h" + ",";
                WhisperMessage wmsg = new WhisperMessage(this.twb.botname, sendmessage);
                this.sttngs.waitForAuctionBot = true;
                
                this.sttngs.waitForAuctionBot = true;
                App.Communicator.sendRequest(wmsg);
                this.sttngs.AucBotMode = "setauc";
            }

            if (popupType == "wantToCreateMultiAuction")
            {
                string genAucMessage = " \\msetauc " + "profileid:" + App.MyProfile.ProfileInfo.id + ", data:";
                string tradedata = "";
                this.helpf.cardsForTradeIds.Clear();
                foreach (Auction a in this.ps.getAddOffers())
                {
                    if (tradedata != "") tradedata = tradedata + ";";
                    tradedata = tradedata + a.card.id + ":" + a.price + ":" +a.message.Split(';')[5];

                    this.helpf.cardsForTradeIds.Add(a.card.id);

                }

                string sendmessage = genAucMessage + tradedata+",";
                Console.WriteLine("#sendmessage: " + sendmessage);
                WhisperMessage wmsg = new WhisperMessage(this.twb.botname, sendmessage);
                this.sttngs.waitForAuctionBot = true;
                
                this.sttngs.waitForAuctionBot = true;
                App.Communicator.sendRequest(wmsg);
                this.sttngs.AucBotMode = "multisetauc";
            }
            
        }

        public void PopupCancel(string popupType)
        {
        }

        public void clearOffercard() { this.OfferCard = null; }

    }
}
