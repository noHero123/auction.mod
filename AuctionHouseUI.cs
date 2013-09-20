using System;
using System.Collections.Generic;
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
        private aucitem tradeitem;
        private bool selectable=true;
        private bool clickableItems=false;
        private float opacity;
        public Vector2 scrollPos;
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

        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");

        Color dblack = new Color(1f, 1f, 1f, 0.5f);


        public AuctionHouseUI(Messageparser mssgprsr,Auclists alists,Rectomat recto,Listfilters lstfltrs,Prices prcs,Cardviewer crdvwr,Searchsettings srchsvr,Network ntwrk,Settings sttngs,Helpfunktions h)
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

        public void ahbuttonpressed()
        {
            helpf.inauchouse = true;
            helpf.settings = false;
            helpf.generator = false;
            //this.hideInformation();
            helpf.hideInformationinfo.Invoke(helpf.storeinfo, null);


            iTween.MoveTo((GameObject)(helpf.buymen).GetValue(helpf.storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
            helpf.showBuyinfo.SetValue(helpf.storeinfo, false);

            ((GameObject)helpf.sellmen.GetValue(helpf.storeinfo)).SetActive(false);
            iTween.MoveTo((GameObject)helpf.sellmen.GetValue(helpf.storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
            ((GameObject)helpf.sellmen.GetValue(helpf.storeinfo)).SetActive(true);
            helpf.showSellinfo.SetValue(helpf.storeinfo, false);

            Store.ENABLE_SHARD_PURCHASES = false;



            if (this.wtsinah)
            {
                alists.wtslistfull.Clear();
                alists.wtslistfull.AddRange(alists.wtslistfulltimed);
                //lstfltrs.sortlist(mssgprsr.wtslistfull);

                alists.setAhlistsToAHWtsLists(true);

                helpf.wtsmenue = true;

            }
            else
            {
                alists.wtblistfull.Clear();
                alists.wtblistfull.AddRange(alists.wtblistfulltimed);

                //lstfltrs.sortlist(mssgprsr.wtblistfull);

                alists.setAhlistsToAHWtbLists(true);
                helpf.wtsmenue = false;

            }
            //lstfltrs.fullupdatelist(alists.ahlist, alists.ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
            helpf.targetchathightinfo.SetValue(helpf.target, (float)Screen.height * 0.25f);
            if (helpf.wtsmenue) { mssgprsr.newwtsmsgs = false; } else { mssgprsr.newwtbmsgs = false; }
        
        }

        public void drawAH()
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
                srchsvr.wtssearchstring = GUI.TextField(recto.sbrect, srchsvr.wtssearchstring, helpf.chatLogStyle);


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
                if (srchsvr.timesearchstring != "" && Convert.ToInt32(srchsvr.timesearchstring) > 30) { srchsvr.timesearchstring = "30"; }
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
                }

                if (helpf.wtsmenue) { srchsvr.savesettings(true, true); } else { srchsvr.savesettings(true, false); }

                bool pricecheck = false;
                //if (wtsmenue) { pricecheck = (pricecopy2.Length < this.pricesearchstring2.Length) || (pricecopy2.Length != this.pricesearchstring2.Length && pricesearchstring2 == "") || (tpfgen); } else { pricecheck = pricecopy.Length > this.pricesearchstring.Length || (tpfgen); }

                pricecheck = (pricecopy2.Length < srchsvr.pricesearchstring2.Length) || (pricecopy2.Length != srchsvr.pricesearchstring2.Length && srchsvr.pricesearchstring2 == "") || (tpfgen) || pricecopy.Length > srchsvr.pricesearchstring.Length || (tpfgen);

                //clear p1moddedlist only if necessary
                //if (selfcopy.Length > this.wtssearchstring.Length || (owp&&!ignore0)|| sellercopy.Length > this.sellersearchstring.Length || pricecheck || closeclick || (growthclick && growthbool) || (orderclick && orderbool) || (energyclick && energybool) || (decayclick && decaybool) || (commonclick && commonbool) || (uncommonclick && uncommonbool) || (rareclick && rarebool) || mt3click || mt0click)
                if (selfcopy.Length != srchsvr.wtssearchstring.Length || (owp && !srchsvr.ignore0) || sellercopy.Length > srchsvr.sellersearchstring.Length || pricecheck || closeclick || (growthclick && srchsvr.growthbool) || (orderclick && srchsvr.orderbool) || (energyclick && srchsvr.energybool) || (decayclick && srchsvr.decaybool) || (commonclick && srchsvr.commonbool) || (uncommonclick && srchsvr.uncommonbool) || (rareclick && srchsvr.rarebool) || mt3click || mt0click)
                {
                    //Console.WriteLine("delete dings####");
                    lstfltrs.fullupdatelist(alists.ahlist, alists.ahlistfull, helpf.inauchouse, helpf.wtsmenue, helpf.generator);

                }
                else
                {

                    if (selfcopy != srchsvr.wtssearchstring)
                    {

                        if (srchsvr.wtssearchstring != "")
                        {
                            lstfltrs.containsname(srchsvr.wtssearchstring, alists.ahlist);
                        }


                    }
                    if (srchsvr.ignore0)
                    {
                        //this.musthaveprice(ahlist);
                        lstfltrs.priceishigher("1", alists.ahlist);
                    }

                    if (sellercopy != srchsvr.sellersearchstring)
                    {

                        if (srchsvr.sellersearchstring != "")
                        {
                            lstfltrs.containsseller(srchsvr.sellersearchstring, alists.ahlist);
                        }


                    }
                    if (pricecopy != srchsvr.pricesearchstring)
                    {

                        if (srchsvr.pricesearchstring != "")
                        {
                            lstfltrs.priceishigher(srchsvr.pricesearchstring, alists.ahlist);

                        }


                    }
                    if (pricecopy2 != srchsvr.pricesearchstring2)
                    {

                        if (srchsvr.pricesearchstring2 != "")
                        {
                            lstfltrs.priceislower(srchsvr.pricesearchstring2, alists.ahlist);
                        }


                    }

                    if (growthclick || orderclick || energyclick || decayclick)
                    {
                        string[] res = { "", "", "", "" };
                        if (srchsvr.decaybool) { res[0] = "decay"; };
                        if (srchsvr.energybool) { res[1] = "energy"; };
                        if (srchsvr.growthbool) { res[2] = "growth"; };
                        if (srchsvr.orderbool) { res[3] = "order"; };
                        lstfltrs.searchforownenergy(res, alists.ahlist);


                    }
                    if (commonclick || uncommonclick || rareclick)
                    {

                        int[] rare = { -1, -1, -1 };
                        if (srchsvr.rarebool) { rare[2] = 2; };
                        if (srchsvr.uncommonbool) { rare[1] = 1; };
                        if (srchsvr.commonbool) { rare[0] = 0; };
                        lstfltrs.searchforownrarity(rare, alists.ahlist);

                    }

                }

            }
            // Draw Auctionhouse here:
            if (helpf.inauchouse)
            {
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

                    lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);

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

                        lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
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

                        lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                    }
                }
                datebeginn = recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2 - datelength / 2 - 2 + vec11.x;
                vec11 = GUI.skin.button.CalcSize(new GUIContent("Price"));
                if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + 2f * recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + 2 * recto.costIconWidth + recto.labelsWidth / 4f - vec11.x / 2, recto.screenRect.yMin - 4, vec11.x, 20), "Price"))
                {
                    if (srchsvr.reverse == true) { srchsvr.sortmode = -1; }
                    if (srchsvr.sortmode == 2) { srchsvr.reverse = true; } else { srchsvr.reverse = false; };
                    srchsvr.sortmode = 2;

                    lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
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
                        alists.wtslistfull.Clear();
                        alists.wtslistfull.AddRange(alists.wtslistfulltimed);
                        lstfltrs.fullupdatelist(alists.ahlist, alists.ahlistfull, helpf.inauchouse, helpf.wtsmenue, helpf.generator);
                    }
                    else
                    {
                        alists.wtblistfull.Clear();
                        alists.wtblistfull.AddRange(alists.wtblistfulltimed);
                        lstfltrs.fullupdatelist(alists.ahlist, alists.ahlistfull, helpf.inauchouse, helpf.wtsmenue, helpf.generator);

                    }
                    lstfltrs.sortlist(alists.ahlist); lstfltrs.sortlist(alists.ahlistfull);
                }



                int num = 0;
                Card card = null;

                // delete old cards:
                DateTime currenttime = DateTime.Now.AddMinutes(-30);
                if (alists.wtslistfulltimed.Count > 0 && alists.wtslistfulltimed[alists.wtslistfulltimed.Count - 1].dtime.CompareTo(currenttime) < 0)
                {
                    alists.wtslistfulltimed.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    alists.wtslistfull.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    alists.wtslist.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                }
                if (alists.wtblistfulltimed.Count > 0 && alists.wtblistfulltimed[alists.wtblistfulltimed.Count - 1].dtime.CompareTo(currenttime) < 0)
                {
                    alists.wtblistfulltimed.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    alists.wtblistfull.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    alists.wtblist.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                }
                // draw auctimes################################################
                //timefilter: 
                int time = 0;
                bool usetimefilter = false;
                float anzcards = anzcards = (float)alists.ahlist.Count();
                if (srchsvr.timesearchstring != "")
                {
                    time = Convert.ToInt32(srchsvr.timesearchstring);
                    currenttime = DateTime.Now.AddMinutes(-1 * time); usetimefilter = true;
                    anzcards = (float)alists.ahlist.Count(delegate(aucitem p1) { return (p1.dtime).CompareTo(currenttime) >= 0; });
                }

                this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                if (srchsvr.reverse) { alists.ahlist.Reverse(); }
                GUI.skin = helpf.cardListPopupBigLabelSkin;
                foreach (aucitem current in alists.ahlist)
                {
                    if (usetimefilter && (current.dtime).CompareTo(currenttime) < 0) { continue; }
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
                            }
                        }
                        else
                        {
                            GUI.Box(position7, string.Empty);
                        }
                        string name = current.card.getName();

                        string txt = helpf.cardnametoimageid(name.ToLower()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        if (sttngs.shownumberscrolls) name = name + " (" + lstfltrs.available[current.card.getName()] + ")";
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
                        TimeSpan ts = temptime.Subtract(current.dtime);

                        if (ts.Minutes >= 1) { sellername = "" + ts.Minutes + " minutes ago"; }
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
                        string gold = current.price + " G";
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
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
                            suggeprice = "SP: " + p1;
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
                            if (sttngs.showsugrange && p1 != p2) suggeprice = "SP: " + Math.Min(p1, p2) + "-" + Math.Max(p1, p2);
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
                if (srchsvr.reverse) { alists.ahlist.Reverse(); }
                GUI.EndScrollView();
                GUI.color = Color.white;
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
                if (mssgprsr.newwtsmsgs)
                {
                    GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                    GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
                }


                if (GUI.Button(recto.wtsbuttonrect, "WTS") && !helpf.showtradedialog)
                {

                    alists.wtslistfull.Clear(); alists.wtslistfull.AddRange(alists.wtslistfulltimed);
                    //sortlist(wtslistfull);

                    alists.setAhlistsToAHWtsLists(true);

                    helpf.wtsmenue = true; this.wtsinah = true;

                    //lstfltrs.sortlist(alists.ahlist); 
                    mssgprsr.newwtsmsgs = false;
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
                if (mssgprsr.newwtbmsgs)
                {
                    GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                    GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
                }
                if (GUI.Button(recto.wtbbuttonrect, "WTB") && !helpf.showtradedialog)
                {
                    alists.wtblistfull.Clear(); alists.wtblistfull.AddRange(alists.wtblistfulltimed);
                    //sortlist(wtblistfull);
                    alists.setAhlistsToAHWtbLists(true);
                    helpf.wtsmenue = false; this.wtsinah = false;

                    //lstfltrs.sortlist(alists.ahlist);

                    mssgprsr.newwtbmsgs = false;
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

                if (helpf.showtradedialog) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.priceinint, helpf.wtsmenue, tradeitem.whole); }






            }
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
        }

        private void starttrading(string name, string cname, int price, bool wts, string orgmsg)
        {
            // asks the user if he wants to trade
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "sell";
            if (wts) text = "buy";
            int anzcard = lstfltrs.available[cname];
            string message = "You want to " + text + "\r\n" + cname + " for " + price + " Gold" + "\r\nYou own this card " + anzcard + " times\r\n\r\nOriginal Message:";
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
                helpf.postmsgmsg = "You want to " + text + ": " + cname + " for " + price + " Gold" + ". You own this card " + anzcard + " times. Original Message:" + "\r\n" + orgmsg;
                helpf.postmsgontrading = true;
            };
            if (GUI.Button(recto.tbwhisper, "Whisper"))
            {
                helpf.showtradedialog = false;
                App.ArenaChat.ChatRooms.OpenWhisperRoom(name);
            };
            if (GUI.Button(recto.tbcancel, "Cancel")) { helpf.showtradedialog = false; };
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
