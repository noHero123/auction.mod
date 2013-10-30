using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Auction.mod
{
    class AuctionHouseUI 
    {

        Vector2 scrolll = new Vector2(0, 0);
        public bool wtsinah = true;//remembers which menupoint in ah was the last one
        private Auction tradeitem;
        private Offer offeritem;
        private bool selectable=true;
        private bool clickableItems=false;
        private float opacity;
        public Vector2 scrollPos, scrollPos2, scrollPos3,scrollPos4;
        Rectomat recto;
        Prices prcs;
        Cardviewer crdvwr;
        Searchsettings srchsvr;
        Network ntwrk;
        Settings sttngs;
        Helpfunktions helpf;
        AuctionHouse ah;
        OfferHouse offers;
        List<Auction> ahlist;
        //ReadOnlyCollection<Auction> ahlist;
        bool bothstarttrading = false;
        Card clickedOfferCard=null;
        int clickedOfferId = 0;
        Auction.OfferType clickerdOfferType = Auction.OfferType.SELL;
        private int OrOrAnd = 0;// in offermenu select price and no additonal card, "and" a additional card  or "or" an additional card
        private string OfferPrice = "0";// in offermenu the offered price
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
            this.ntwrk = Network.Instance;
            this.sttngs = Settings.Instance;
            this.ah = AuctionHouse.Instance;
            this.offers = OfferHouse.Instance;
        }

        public void ahbuttonpressed()
        {
            helpf.inauchouse = true;
            helpf.settings = false;
            helpf.generator = false;
            //this.hideInformation();
            helpf.hideInformationinfo.Invoke(helpf.storeinfo, null);

            if (sttngs.spampreventtime != "")
            {
                ah.spamFilter.setSpamTime(new TimeSpan(0, sttngs.spamprevint, 0));
                ah.buyOfferFilter.filtersChanged = true;
                ah.sellOfferFilter.filtersChanged = true;
            }
            else
            {
                ah.spamFilter.disableSpamFilter();
                ah.buyOfferFilter.filtersChanged = true;
                ah.sellOfferFilter.filtersChanged = true;
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
            if (this.wtsinah)
            {

                srchsvr.setsettings(true, true);
                helpf.wtsmenue = true;

            }
            else
            {

                srchsvr.setsettings(true, false);
                helpf.wtsmenue = false;

            }
            //lstfltrs.fullupdatelist(alists.ahlist, alists.ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
            helpf.targetchathightinfo.SetValue(helpf.target, (float)Screen.height * 0.25f);
        
        }


        
        public void drawAH()
        {
            if (helpf.ownoffermenu)
            {
                // for getting mouse wheel-scrolling in overlayed window, we have to paint it first
                if (helpf.showtradedialog && !(helpf.makeOfferMenu && !helpf.offerMenuSelectCardMenu)) { this.startaccepting(); }
                
                this.drawAHlist(true);
                drawoffers();
                if (helpf.showtradedialog) { this.startaccepting(); }
                return;
            }

            if (helpf.bothmenue)
            {
                // for getting mouse wheel-scrolling in overlayed window, we have to paint it first
                if (helpf.showtradedialog && !(helpf.makeOfferMenu && !helpf.offerMenuSelectCardMenu)) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, this.bothstarttrading, tradeitem.message, tradeitem.card.getType()); }
                
                srchsvr.setsettings(true, true);
                ah.setSellSortMode(srchsvr.sortmode);
                this.drawAHlist(true);

                srchsvr.setsettings(true, false);
                ah.setBuySortMode(srchsvr.sortmode);
                this.drawAHlist(false);

                if (helpf.showtradedialog) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, this.bothstarttrading, tradeitem.message,tradeitem.card.getType()); }
                return;
            }
            // for getting mouse wheel-scrolling in overlayed window, we have to paint it first
            if (helpf.showtradedialog && !(helpf.makeOfferMenu && !helpf.offerMenuSelectCardMenu)) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, helpf.wtsmenue, tradeitem.message, tradeitem.card.getType()); }
            //draw regular menu + searchmenu
            this.drawAHandSearch();
        }

        private void drawAHandSearch()
        {
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

                GUI.skin = helpf.cardListPopupBigLabelSkin;
                GUI.Label(recto.sbtimelabel, "not older than");
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.sbtimerect, string.Empty);
                string timecopy = srchsvr.timesearchstring;
                srchsvr.timesearchstring = Regex.Replace(GUI.TextField(recto.sbtimerect, srchsvr.timesearchstring, 2, helpf.chatLogStyle), @"[^0-9]", "");
                if (srchsvr.timesearchstring != "" && Convert.ToInt32(srchsvr.timesearchstring) > helpf.deleteTime) { srchsvr.timesearchstring = ((int)helpf.deleteTime).ToString(); }
                GUI.color = Color.white;


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
                bool owp = GUI.Button(recto.sbonlywithpricebox, "");
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

                GUI.skin = helpf.cardListPopupSkin;

                if (ntwrk.contonetwork)
                {
                    GUI.Label(recto.sbnetworklabel, "User online: " + ntwrk.getnumberofaucusers());
                }



                if (recto._showSearchDropdown) recto.OnGUI_drawSearchPulldown(recto.sbrect);// draw pulldown again (for overlay)

                GUI.contentColor = Color.red;
                bool closeclick = GUI.Button(recto.sbclearrect, "X");
                GUI.contentColor = Color.white;

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
                    }
                    else
                    {
                        ah.buyOfferFilter.resetFilters();
                    }

                }

                if (helpf.wtsmenue) { srchsvr.savesettings(true, true); } else { srchsvr.savesettings(true, false); }

                //set filters
                if (tpfgen)
                {
                    if (helpf.wtsmenue)
                    {
                        ah.sellOfferFilter.setTakeWTB(srchsvr.takepriceformgenarator);
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
                    }
                    else
                    {
                        ah.buyOfferFilter.setRarityFilter(srchsvr.commonbool, srchsvr.uncommonbool, srchsvr.rarebool);
                    }

                }

                

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
                if (helpf.wtsmenue) this.ahlist = ah.getSellOffers();
                else this.ahlist = ah.getBuyOffers();
                

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

                if (helpf.wtsmenue)
                {
                    this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }
                else
                {
                    this.scrollPos2 = GUI.BeginScrollView(recto.position3, this.scrollPos2, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }

                //this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                if (srchsvr.reverse) { this.ahlist.Reverse(); }
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
                            GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Buy");


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
                if (deleteOldEntrys) { ah.removeOldEntrys(); };

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
                //wts / wtb menue buttons

                if (helpf.wtsmenue)
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

                    helpf.wtsmenue = true; this.wtsinah = true;
                    srchsvr.setsettings(true, true);
                    ah.setSellSortMode(srchsvr.sortmode);

                }
                GUI.skin.button.normal.textColor = Color.white;
                GUI.skin.button.hover.textColor = Color.white;




                if (!helpf.wtsmenue)
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
                    helpf.wtsmenue = false; this.wtsinah = false;

                    //lstfltrs.sortlist(alists.ahlist);
                    srchsvr.setsettings(true, false);
                    ah.setBuySortMode(srchsvr.sortmode);
                }
                GUI.skin.button.normal.textColor = Color.white;
                GUI.skin.button.hover.textColor = Color.white;

                //bothbutton
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                if (GUI.Button(recto.bothbuttonrect, "All") && !helpf.showtradedialog)
                {
                    helpf.bothmenue = true;
                    recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                }
                if (GUI.Button(recto.ownbuttonrect, "Own") && !helpf.showtradedialog)
                {
                    helpf.ownoffermenu = true;
                    recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                }


                GUI.skin.button.normal.textColor = Color.white;
                GUI.skin.button.hover.textColor = Color.white;


                GUI.color = Color.white;

                if (ntwrk.realycontonetwork)
                {
                    if (GUI.Button(recto.updatebuttonrect, "discon"))
                    {
                        ntwrk.disconfromaucnet();
                    }
                }
                else
                {
                    if (GUI.Button(recto.updatebuttonrect, "connect"))
                    {
                        ntwrk.connect();
                        //App.Communicator.sendRequest(new RoomChatMessageMessage(chatRooms.GetCurrentRoomName(), "aucc aucstart"));
                        // short explanation of my network:

                        // joining in network:
                        // goto auc-1
                        // you are in auc-i -> save users from roominfo message -> the first infomessage one will be the biggest, if the biggest is longer than 50 people goto next room (auc-i+1), repeat
                        // now your are in room auc-j with #users <=50 -> send added users aucstay! + your roomnumber. (to the first 5 users send aucstay? + roomnumber)
                        // form the users u send aucstay? you got an number with room, you have to visit also ( rooms with numbers bigger than your "stay-room")
                        // visit them , add the users and send them aucstay!

                        // be-ing in network:
                        // if a user joins your channel -> add him to the network list
                        // if you got a message with aucstay! save the user-room
                        // if you got a message with aucstay? save the user-room and send the writer a list with roomnumbers, that are bigger then the writers stay-room and where 
                        //+ people stay too.

                        // if you are not in room 1, and there are too few people in 1 (cause they leave) disconnect and rejoin
                        //(because this algo needs people in auc-1, to communicate the other rooms where people "live" :D

                        // communication in network if not joining:
                        // whisper to all users of the network (in this.aucusers) your wts and wtb message if you click "post"

                        // leaving the network:
                        // whisper to all users that you leave, and if you are in room 1, tell a user not in 1, to join 1.
                    }

                }

                if (helpf.showtradedialog) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.price, helpf.wtsmenue, tradeitem.message, tradeitem.card.getType()); }






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
                        if (wtsmenue) { srchsvr.savesettings(true, true); } else { srchsvr.savesettings(true, false); }
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
                                srchsvr.savesettings(true, true);
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

                                srchsvr.savesettings(true, false);
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
                        if (wtsmenue) { srchsvr.savesettings(true, true); } else { srchsvr.savesettings(true, false); }
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
                    if (wtsmenue) { srchsvr.savesettings(true, true); } else { srchsvr.savesettings(true, false); }
                    //lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                }



                int num = 0;
                Card card = null;

                // set drawn cards
                if (helpf.ownoffermenu)
                {
                    this.ahlist = offers.getSellOffers();
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
                float anzcards = anzcards = (float)this.ahlist.Count();
                if (srchsvr.timesearchstring != "")//doesnt show "old" offers filtered by time-filter
                {
                    time = Convert.ToInt32(srchsvr.timesearchstring);
                    currenttime = DateTime.Now.AddMinutes(-1 * time); usetimefilter = true;
                    anzcards = (float)this.ahlist.Count(delegate(Auction p1) { return (p1.time).CompareTo(currenttime) >= 0; });
                }

                if (wtsmenue)
                {
                    this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }
                else
                {
                    this.scrollPos2 = GUI.BeginScrollView(recto.position3, this.scrollPos2, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                }
                if (srchsvr.reverse) { this.ahlist.Reverse(); }
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
                        if (helpf.ownoffermenu)
                        {
                            if (GUI.Button(position7, string.Empty))
                            {
                                //this.callback.ItemClicked(this, current);
                                this.clickedOfferCard = current.card;
                                this.clickedOfferId = current.card.getType();
                                this.clickerdOfferType = current.offer;
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
                        if (wtsmenue)
                        {
                            if (!helpf.showtradedialog)
                            {
                                if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                {
                                    if (helpf.ownoffermenu)
                                    {
                                        this.clickedOfferCard = current.card;
                                        this.clickedOfferId = current.card.getType();
                                        this.clickerdOfferType = current.offer;
                                    }
                                    else
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
                            }
                            else { GUI.Box(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                            GUI.skin = helpf.cardListPopupBigLabelSkin;
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            if (helpf.bothmenue)GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Buy");
                            if (helpf.ownoffermenu)
                            {
                                if (current.offer == Auction.OfferType.SELL) { GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Sell"); }
                                else { GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Buy"); }
                            }
                            
                                


                        }
                        else
                        {
                            if (!helpf.showtradedialog)
                            {
                                if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                {
                                    if (helpf.ownoffermenu)
                                    {
                                        this.clickedOfferCard = current.card;
                                        this.clickedOfferId = current.card.getType();
                                        this.clickerdOfferType = current.offer;
                                    }
                                    else
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

                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    if (ah.newSellOffers)
                    {
                        GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                        GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
                    }


                    if (GUI.Button(recto.wtsbuttonrect, "WTS") && !helpf.showtradedialog)
                    {
                        recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                        helpf.wtsmenue = true; this.wtsinah = true;
                        srchsvr.setsettings(true, true);
                        ah.setSellSortMode(srchsvr.sortmode);
                        helpf.bothmenue = false;
                        helpf.ownoffermenu = false;
                    }
                    GUI.skin.button.normal.textColor = Color.white;
                    GUI.skin.button.hover.textColor = Color.white;



                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
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
                        srchsvr.setsettings(true, false);
                        ah.setBuySortMode(srchsvr.sortmode);
                        helpf.bothmenue = false;
                        helpf.ownoffermenu = false;
                    }
                    GUI.skin.button.normal.textColor = Color.white;
                    GUI.skin.button.hover.textColor = Color.white;

                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    if (helpf.bothmenue) GUI.color = Color.white;
                    if (GUI.Button(recto.bothbuttonrect, "All") && !helpf.showtradedialog)
                    {
                        helpf.bothmenue = true;
                        helpf.ownoffermenu = false;
                        recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                    }

                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    if (helpf.ownoffermenu) GUI.color = Color.white;
                    if (GUI.Button(recto.ownbuttonrect, "Own") && !helpf.showtradedialog)
                    {
                        helpf.bothmenue = false;
                        helpf.ownoffermenu = true;
                        recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                    }
                    
                }


                GUI.color = Color.white;

                if (wtsmenue)
                {
                    if (ntwrk.realycontonetwork)
                    {
                        if (GUI.Button(recto.updatebuttonrect, "discon"))
                        {
                            ntwrk.disconfromaucnet();
                        }
                    }
                    else
                    {
                        if (GUI.Button(recto.updatebuttonrect, "connect"))
                        {
                            ntwrk.connect();
                        }

                    }
                }

               

            }
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
            if (!wtsmenue)
            {
                if (ntwrk.contonetwork)
                {
                    GUI.skin = helpf.cardListPopupSkin;
                    GUI.Label(recto.sbnetworklabel, "User online: " + ntwrk.getnumberofaucusers());
                }
                recto.innerRect.x = recto.innerRect.x - Screen.width * 0.495f;
                recto.innerBGRect.x = recto.innerBGRect.x - Screen.width * 0.495f;
                recto.outerRect.x = recto.outerRect.x - Screen.width * 0.495f;
                recto.screenRect.x = recto.screenRect.x - Screen.width * 0.495f;
                recto.filtermenurect.x = recto.filtermenurect.x - Screen.width * 0.495f;
                recto.calcguirects();
            }
        }


        private void drawoffers()
        {
            //start, calculate guirects new
            recto.innerRect.x = recto.innerRect.x + Screen.width * 0.495f;
            recto.innerBGRect.x = recto.innerBGRect.x + Screen.width * 0.495f;
            recto.outerRect.x = recto.outerRect.x + Screen.width * 0.495f;
            recto.screenRect.x = recto.screenRect.x + Screen.width * 0.495f;
            recto.filtermenurect.x = recto.filtermenurect.x + Screen.width * 0.495f;
            recto.calcguirects();
            // draw offers!
            GUI.depth = 15;
            this.opacity = 1f;
            GUI.skin = helpf.cardListPopupSkin;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

            GUI.Box(recto.position, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
            GUI.Box(recto.position2, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);


            //write your offer
            string overwriting = "    ";
            string whatppldo = "Want to sell ";
            if (this.clickerdOfferType == Auction.OfferType.SELL) whatppldo = "Want to buy ";
            if(clickedOfferCard!=null) overwriting=whatppldo +clickedOfferCard.getName() + " Offers"; //in german ueberschrift ;D
            Vector2 vec11 = GUI.skin.button.CalcSize(new GUIContent(overwriting));
            GUI.Box(new Rect(recto.innerRect.xMin + recto.labelX, recto.screenRect.yMin - 4, vec11.x, 20), string.Empty);
            GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX, recto.screenRect.yMin - 4, vec11.x, 20), overwriting);

            List<Offer> offlist = this.offers.getOffers(this.clickedOfferId,this.clickerdOfferType);
            this.scrollPos3 = GUI.BeginScrollView(recto.position3, this.scrollPos3, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * offlist.Count));

            Card card = null;
            //Draw stuff
            int num = 0;
            float testy = this.scrollPos3.y;
            foreach (Offer current in offlist)
            {
                
                GUI.skin = helpf.cardListPopupGradientSkin;
                //draw boxes
                Rect position7 = recto.position7offers(num);

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


                    Texture texture=null;
                        Vector2 vector;

                    //draw seller name
                    string sellername = current.seller;
                    GUI.skin = helpf.cardListPopupBigLabelSkin;
                    vector = GUI.skin.label.CalcSize(new GUIContent(sellername));
                    
                    Rect position8 = recto.position8offers(num);
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(position8, (vector.x >= position8.width) ? (sellername.Substring(0, Mathf.Min(sellername.Length, recto.maxCharsName)) + "...") : sellername);
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    //draw timestamp
                    GUI.skin = helpf.cardListPopupSkin;
                    DateTime temptime = DateTime.Now;
                    TimeSpan ts = temptime.Subtract(current.time);
                    if (ts.TotalMinutes >= 1.0)
                    {
                        sellername = "" + ts.Minutes + " minutes ago";
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
                    Rect position9 = recto.position9offers(num);
                    GUI.skin.label.alignment = TextAnchor.UpperCenter;
                    GUI.Label(position9, sellername);
                    GUI.skin.label.alignment = TextAnchor.UpperLeft;


                    //draw gold cost
                    float nextx = position9.xMax + recto.costIconWidth;
                    string gold = current.price + "g";
                    if (current.price == 0) gold = "-";
                    GUI.skin = helpf.cardListPopupBigLabelSkin;
                    //vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                    //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                    //Rect position12 = new Rect(position7.xMax-recto.goldlength-2f, position8.yMin, recto.goldlength, recto.cardHeight);
                    Rect position10 = new Rect(nextx + 2f, position8.yMin, recto.labelsWidth / 2f, recto.cardHeight);
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(position10, gold);
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                    // draw suggested price
                    string suggeprice = "total: " + current.calcprice+"g";
                    GUI.skin = helpf.cardListPopupSkin;
                    Rect position11 = new Rect(nextx + 2f, position9.y, recto.labelsWidth / 2f, recto.fieldHeight);
                    GUI.skin.label.alignment = TextAnchor.UpperCenter;
                    GUI.Label(position11, suggeprice);
                    GUI.skin.label.alignment = TextAnchor.UpperLeft;


                    if (current.cardOffer != null)
                    {

                        string name = current.cardOffer.getName();

                        string txt = helpf.cardIDtoimageid(current.cardOffer.getType()).ToString();
                        texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        if (sttngs.shownumberscrolls) name = "(" + helpf.cardIDToNumberOwned[current.cardOffer.getType()] + ") " + name;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        vector = GUI.skin.label.CalcSize(new GUIContent(name));


                        nextx = position10.xMax + recto.costIconWidth;
                        //drawcardname
                        Rect position12 = new Rect(nextx + 2f, position8.yMin, recto.labelsWidth, recto.cardHeight);

                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position12, (vector.x >= position12.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        GUI.skin = helpf.cardListPopupSkin;
                        string text = current.cardOffer.getPieceKind().ToString();
                        string str = text.Substring(0, 1) + text.Substring(1).ToLower();
                        string text2 = string.Empty;
                        int num2 = recto.maxCharsRK;
                        if (current.cardOffer.level > 0)
                        {
                            string text3 = text2;
                            text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.cardOffer.level + 1, "</color>, " });
                            num2 += "<color=#rrggbb></color>".Length;
                        }
                        text2 = text2 + current.cardOffer.getRarityString() + ", " + str;
                        Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(text2));

                        //draw cardsubstring
                        Rect position13 = new Rect(nextx + 2f, position9.y, recto.labelsWidth, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position13, (vector2.x >= position13.width) ? (text2.Substring(0, Mathf.Min(text2.Length, num2)) + "...") : text2);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;


                    }
                    GUI.skin = helpf.cardListPopupLeftButtonSkin;
                    if (!this.selectable)
                    {
                        GUI.enabled = false;
                    }
                    position10=recto.position10offers(num);
                    if (GUI.Button(position10, string.Empty))
                    {
                        if (current.cardOffer!=null) card = current.cardOffer;
                        App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                    }
                    if (!this.selectable)
                    {
                        GUI.enabled = true;
                    }
                    //draw picture
                    if (texture != null)
                    {
                        GUI.DrawTexture(new Rect(position10.x + 4f, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight), texture);
                    }

                    // draw buy/sell button
                    GUI.skin = helpf.cardListPopupGradientSkin;
                    if (this.clickerdOfferType == Auction.OfferType.SELL)
                    {
                        if (!helpf.showtradedialog)
                        {
                            if (GUI.Button(new Rect(position10.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                            {

                                // start trading with seller
                                if (current.seller != App.MyProfile.ProfileInfo.name)
                                {
                                    helpf.showtradedialog = true;
                                    offeritem = current;
                                    this.bothstarttrading = this.clickerdOfferType == Auction.OfferType.SELL;
                                }
                            }
                        }
                        else { GUI.Box(new Rect(position10.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(new Rect(position10.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Sell");


                    }
                    else
                    {
                        if (!helpf.showtradedialog)
                        {
                            if (GUI.Button(new Rect(position10.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                            {

                                // start trading with seller
                                if (current.seller != App.MyProfile.ProfileInfo.name)
                                {
                                    helpf.showtradedialog = true;
                                    offeritem = current;
                                    this.bothstarttrading = this.clickerdOfferType == Auction.OfferType.SELL;
                                }
                            }
                        }
                        else { GUI.Box(new Rect(position10.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(new Rect(position10.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Buy");
                    }
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    num++;
                }
            }





            GUI.EndScrollView();
            //end, calculate guirects new

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


            if (GUI.Button(recto.chancelbutton(), "Del"))
            {
                if (this.clickedOfferId != 0) { helpf.showtradedialog = true; helpf.deleteAuctionMenu = true; }
            }

            if (ntwrk.contonetwork)
            {
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Label(recto.sbnetworklabel, "User online: " + ntwrk.getnumberofaucusers());
            }
            recto.innerRect.x = recto.innerRect.x - Screen.width * 0.495f;
            recto.innerBGRect.x = recto.innerBGRect.x - Screen.width * 0.495f;
            recto.outerRect.x = recto.outerRect.x - Screen.width * 0.495f;
            recto.screenRect.x = recto.screenRect.x - Screen.width * 0.495f;
            recto.filtermenurect.x = recto.filtermenurect.x - Screen.width * 0.495f;
            recto.calcguirects();
        }


        private void starttrading(string name, string cname, int price, bool wts, string orgmsg, int cid)
        {
            
            if (helpf.makeOfferMenu)
            {
                this.offerMenu(wts,cname,cid,name);
                return;
            }
            // asks the user if he wants to trade
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "sell";
            if (wts) text = "buy";
            int anzcard = helpf.cardIDToNumberOwned[cid];
            string message = "You want to " + text + "\r\n" + cname + " for " + price + "g" + "\r\nYou own this card " + anzcard + " times\r\n\r\nOriginal Message:";
            GUI.Label(recto.tbmessage, message);
            GUI.skin.label.wordWrap = true;
            float msghigh = GUI.skin.label.CalcHeight(new GUIContent(orgmsg), recto.tbmessage.width - 30f);
            GUI.skin = helpf.cardListPopupSkin;
            scrolll = GUI.BeginScrollView(recto.tbmessagescroll, scrolll, new Rect(0f, 0f, recto.tbmessagescroll.width - 20f, msghigh));
            GUI.skin = helpf.cardListPopupBigLabelSkin;

            GUI.Label(new Rect(5f, 5f, recto.tbmessagescroll.width - 30f, msghigh), orgmsg);

            //Console.WriteLine(message);

            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.EndScrollView();
            GUI.skin.label.wordWrap = false;
            GUI.skin = helpf.cardListPopupLeftButtonSkin;

            if (GUI.Button(recto.tbok, "OK"))
            {
                helpf.showtradedialog = false;
                App.GameActionManager.TradeUser(helpf.globalusers[name]);
                helpf.postmsgmsg = "You want to " + text + ": " + cname + " for " + price + "g " + ". You own this card " + anzcard + " times. Original Message:" + "\r\n" + orgmsg;
                helpf.postmsgontrading = true;
            };
            if (GUI.Button(recto.tbwhisper, "Whisper"))
            {
                helpf.showtradedialog = false;
                App.ArenaChat.ChatRooms.OpenWhisperRoom(name);
            };
            if (GUI.Button(recto.tbcancel, "Cancel")) { helpf.showtradedialog = false; };
            if (GUI.Button(recto.tboffer, "Offer")) { helpf.makeOfferMenu = true; this.OrOrAnd = 0; this.OfferPrice = "0"; this.OfferCard = null; };
        }

        private void offerMenu(bool wts, string cname,int cid,string playername)
        {
            if (helpf.offerMenuSelectCardMenu)
            {
                this.ccardlist(!wts);// want to show the opposit prices in cardlist
                return;
            }

            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "selling";
            if (wts) { text = "buying"; }
            int anzcard = helpf.cardIDToNumberOwned[cid];
            int index = helpf.cardidToArrayIndex(cid);
            string message = "You want to make an offer for " + text + " a " + cname + " (SG:" + prcs.getPrice(index, sttngs.wtbAHpriceType) + ")" + "\r\nYou own this card " + anzcard + " times.\r\n Your Messge:\r\n\r\n";
            if (wts) message = "You want to make an offer for " + text + " a " + cname + " (SG:" + prcs.getPrice(index, sttngs.wtsAHpriceType) + ")" + "\r\nYou own this card " + anzcard + " times.\r\n Your Messge:\r\n\r\n";
            string yourmessage1 = "WTS my " + cname + " for ";
            if (wts) yourmessage1 = "WTB your " + cname + " for ";
            GUI.Label(recto.tbmessage, message + yourmessage1);

            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tbpriceinput, string.Empty);
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            helpf.chatLogStyle.alignment = TextAnchor.MiddleCenter;
            this.OfferPrice = Regex.Replace(GUI.TextField(recto.tbpriceinput, this.OfferPrice, helpf.chatLogStyle), @"[^0-9]", "");
            helpf.chatLogStyle.alignment = TextAnchor.MiddleLeft;
            if (this.OfferPrice == "") this.OfferPrice = "0";
            if (Convert.ToInt32(this.OfferPrice) > 999999) this.OfferPrice = "999999";
            if (GUI.Button(recto.tbororand, ""))
            {
                this.OrOrAnd = (this.OrOrAnd + 1) % 3;
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (this.OrOrAnd == 0) { GUI.Label(recto.tbororand, ""); }
            if (this.OrOrAnd == 1) { GUI.Label(recto.tbororand, "and"); }
            if (this.OrOrAnd == 2) { GUI.Label(recto.tbororand, "or"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            if (this.OrOrAnd >= 1)
            {
                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.tbcard, string.Empty);
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                if (GUI.Button(recto.tbcard, ""))
                {
                    helpf.offerMenuSelectCardMenu = true;
                }

                if (this.OfferCard != null)
                {
                    Texture texture = App.AssetLoader.LoadTexture2D(helpf.cardIDtoimageid(this.OfferCard.getType()).ToString());//current.getCardImage())
                    if (texture != null)
                    {
                        GUI.DrawTexture(new Rect (recto.tbcard.x + 4f, recto.tbcard.y +4f, 2*recto.cardWidth, 2*recto.cardHeight), texture);
                    }
                }

            }

            if (GUI.Button(recto.tbok, "OK"))
            {
                string sendmessage = "WTS my " + cname + " for ";
                if (wts) sendmessage = "WTB your " + cname + " for ";
                if (this.OfferPrice != "0")
                {
                    sendmessage = sendmessage + this.OfferPrice + "g ";
                    if (this.OrOrAnd == 1 && this.OfferCard != null) { sendmessage = sendmessage + "and " + this.OfferCard.getName(); }
                    if (this.OrOrAnd == 2 && this.OfferCard != null) { sendmessage = sendmessage + "or " + this.OfferCard.getName(); }
                }
                else
                {
                    if (this.OrOrAnd == 1 && this.OfferCard != null) { sendmessage = sendmessage  + this.OfferCard.getName(); }
                    if (this.OrOrAnd == 2 && this.OfferCard != null) { sendmessage = sendmessage  + this.OfferCard.getName(); }
                }
                WhisperMessage wmsg = new WhisperMessage(playername,sendmessage);
                App.Communicator.sendRequest(wmsg);

                helpf.makeOfferMenu = false; this.OrOrAnd = 0; this.OfferPrice = "0"; this.OfferCard = null;
            };
            if (GUI.Button(recto.tbcancel, "Cancel")) { helpf.makeOfferMenu = false; this.OrOrAnd = 0; this.OfferPrice = "0"; this.OfferCard = null; };
        }

        private void startaccepting()
        {
            if (helpf.deleteAuctionMenu)
            {
                this.deletingAuction();
                return;
            }
            string name =offeritem.seller; string cname= offeritem.cardTarget.getName(); int price= offeritem.price;bool wts= this.bothstarttrading;
            string orgmsg = offeritem.message; int cid = offeritem.cardTarget.getType(); int calcprice = offeritem.calcprice;
            // asks the user if he wants to trade
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "buy a";
            if (wts) text = "sell a";
            int anzcard = helpf.cardIDToNumberOwned[cid];
            string message = "You want to " + text + "\r\n" + cname + " for " + price + "g" + "\r\nYou own this card " + anzcard + " times\r\n\r\nOriginal Message:";
            if (offeritem.cardOffer != null) message = "You want to " + text + "\r\n" + cname + " for " + price + "g and a " + offeritem.cardOffer.getName()+ "\r\n"+"(total: " + calcprice + "g)\r\n"+"You own " +cname+ " " + anzcard + " times\r\n\r\nOriginal Message:";
            GUI.Label(recto.tbmessage, message);
            GUI.skin.label.wordWrap = true;
            float msghigh = GUI.skin.label.CalcHeight(new GUIContent(orgmsg), recto.tbmessage.width - 30f);
            GUI.skin = helpf.cardListPopupSkin;
            scrolll = GUI.BeginScrollView(recto.tbmessagescroll, scrolll, new Rect(0f, 0f, recto.tbmessagescroll.width - 20f, msghigh));
            GUI.skin = helpf.cardListPopupBigLabelSkin;

            GUI.Label(new Rect(5f, 5f, recto.tbmessagescroll.width - 30f, msghigh), orgmsg);

            //Console.WriteLine(message);

            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.EndScrollView();
            GUI.skin.label.wordWrap = false;
            GUI.skin = helpf.cardListPopupLeftButtonSkin;

            if (GUI.Button(recto.tbok, "OK"))
            {
                helpf.showtradedialog = false;
                App.GameActionManager.TradeUser(helpf.globalusers[name]);
                helpf.postmsgmsg = "You want to " + text + ": " + cname + " for " + price + "g" + ". You own this card " + anzcard + " times. Original Message:" + "\r\n" + orgmsg;
                if (offeritem != null) helpf.postmsgmsg = "You want to " + text + ": " + cname + " for " + price + "g and a " + offeritem.cardOffer.getName() + " " + "(total: " + calcprice + "g)." + " You own " + cname + " " + anzcard + " times. Original Message:" + "\r\n" + orgmsg;
                helpf.postmsgontrading = true;
            };
            if (GUI.Button(recto.tbwhisper, "Whisper"))
            {
                helpf.showtradedialog = false;
                App.ArenaChat.ChatRooms.OpenWhisperRoom(name);
            };
            if (GUI.Button(recto.tbcancel, "Cancel")) { helpf.showtradedialog = false; };
        }

        private void deletingAuction()
        {

            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "wtb";
            if (this.clickerdOfferType == Auction.OfferType.SELL) text = "wts";
            string message = "You realy want to delete your auction: " + text + " " + helpf.cardidsToCardnames[clickedOfferId]+"?";
            GUI.Label(recto.tbmessage, message);
            GUI.skin = helpf.cardListPopupLeftButtonSkin;

            if (GUI.Button(recto.tbok, "Yes"))
            {
                this.offers.removeEntry(this.clickedOfferId, this.clickerdOfferType);
                this.clickedOfferCard = null;
                this.clickedOfferId = 0;
                helpf.showtradedialog = false; helpf.deleteAuctionMenu = false;
            };
            if (GUI.Button(recto.tbcancel, "No")) { helpf.showtradedialog = false; helpf.deleteAuctionMenu = false; };

        }

        private void ccardlist(bool wtsmenue)//select card you want to offer
        {
            // dont know how to get a carlistpopup to work
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
            Rect innerRect = new Rect(recto.tradingbox.x + 12f, recto.tradingbox.y + 12f, recto.tradingbox.width - 24f, recto.tbok.y - recto.tradingbox.yMin -12f);
            GUI.Box(innerRect, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
            
            List<Auction> offlist=null;

            if (!wtsmenue) offlist = Generator.Instance.getOwnSellOffers(); // all cards YOU own ( the other user wants to buy
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
            if (card != null) helpf.offerMenuSelectCardMenu = false;
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

    


    }
}
