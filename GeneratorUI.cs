using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Auction.mod
{
    class GeneratorUI
    {
        private List<Auction> ahlist;

        public bool wtsingen = true;
        public Vector2 scrollPos;
        private float opacity;
        private bool selectable = true;
        private bool clickableItems = false;

        Color dblack = new Color(1f, 1f, 1f, 0.5f);

        Messageparser mssgprsr;
        //Auclists alists;
        Rectomat recto;
        //Listfilters lstfltrs;
        Prices prcs;
        Cardviewer crdvwr;
        Searchsettings srchsvr;
        Network ntwrk;
        Settings sttngs;
        Helpfunktions helpf;
        Generator generator;

        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");
        Type T = typeof(GUIUtility);
        PropertyInfo systemCopyBufferProperty;


        private static GeneratorUI instance;

        public static GeneratorUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GeneratorUI();
                }
                return instance;
            }
        }

        public GeneratorUI()
        {
            this.helpf = Helpfunktions.Instance;
            this.mssgprsr = Messageparser.Instance;

            this.recto = Rectomat.Instance;
            this.prcs = Prices.Instance;
            this.crdvwr = Cardviewer.Instance;
            this.srchsvr = Searchsettings.Instance;
            this.ntwrk = Network.Instance;
            this.sttngs = Settings.Instance;
            this.generator = Generator.Instance;
            systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public void genbuttonpressed()
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
            helpf.generator = true;
            helpf.settings = false;


            generator.buyOwnCardsFilter.filtersChanged = true;
            generator.sellOwnCardsFilter.filtersChanged = true;
            if (this.wtsingen)
            {
                helpf.wtsmenue = true;
                srchsvr.setsettings(false, true);
                //alists.setAhlistsToGenWtsLists();


            }
            else
            {
                helpf.wtsmenue = false;
                srchsvr.setsettings(false, false);
                //alists.setAhlistsToGenWtbLists();


            }

            //this.genlist.AddRange(this.genlistfull);

            helpf.targetchathightinfo.SetValue(helpf.target, (float)Screen.height * 0.25f);
        }
       
        public void drawgenerator()
        {
            // have to draw textfield in front of scrollbar or otherwise you lose focus in textfield (lol)

            if (helpf.generator)
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
                    mt3click = GUI.Button(recto.sbthreerect, ">3"); // >3 bei wtsmenue=false
                }
                else { mt3click = GUI.Button(recto.sbthreerect, "<3"); }
                GUI.color = Color.white;
                if (!srchsvr.onebool) { GUI.color = dblack; }
                //if (this.wtsmenue) { mt0click = GUI.Button(sbonerect, ">0"); };
                GUI.color = Color.white;
                GUI.contentColor = Color.white;

                GUI.skin = helpf.cardListPopupBigLabelSkin;
                if (helpf.wtsmenue)
                {
                    GUI.Label(recto.sbsellerlabelrect, "wts msg:");
                }
                else { GUI.Label(recto.sbsellerlabelrect, "wtb msg:"); }

                GUI.skin = helpf.cardListPopupSkin;
                GUI.Box(recto.sbsellerrect, string.Empty);
                string sellercopy = srchsvr.sellersearchstring;
                GUI.SetNextControlName("sellerframe");
                GUI.TextField(recto.sbsellerrect, srchsvr.sellersearchstring, helpf.chatLogStyle);

                GUI.color = Color.white;
                GUI.contentColor = Color.red;
                bool closeclick = GUI.Button(recto.sbclearrect, "X");
                GUI.contentColor = Color.white;

                GUI.skin = helpf.cardListPopupSkin;

                if (ntwrk.contonetwork)
                {
                    GUI.Label(recto.sbnetworklabel, "User online: " + ntwrk.getnumberofaucusers());
                }

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
                    if (helpf.wtsmenue)
                        generator.sellOwnCardsFilter.resetFilters();
                    else
                        generator.buyOwnCardsFilter.resetFilters();

                    srchsvr.resetgensearchsettings(helpf.wtsmenue);
                    if (ntwrk.realycontonetwork)
                    {
                        ntwrk.deleteownmessage(helpf.wtsmenue);

                    }
                }
                if (helpf.wtsmenue) { srchsvr.savesettings(false, true); } else { srchsvr.savesettings(false, false); }


                if (selfcopy != srchsvr.wtssearchstring)
                {
                    if (helpf.wtsmenue)
                    {
                        generator.sellOwnCardsFilter.setCardFilter(srchsvr.wtssearchstring);
                    }
                    else
                    {
                        generator.buyOwnCardsFilter.setCardFilter(srchsvr.wtssearchstring);
                    }

                }
                if (mt3click || mt0click)
                {
                    int filter = 0;

                    if (helpf.wtsmenue)
                    {
                        if (srchsvr.threebool) filter = 2;
                        generator.sellOwnCardsFilter.setAmountFilter(filter);
                    }
                    else
                    {
                        if (srchsvr.threebool) filter = 3;
                        generator.buyOwnCardsFilter.setAmountFilter(filter);
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
                        generator.sellOwnCardsFilter.setResourceFilter(res);
                    }
                    else
                    {
                        generator.buyOwnCardsFilter.setResourceFilter(res);
                    }

                }
                if (commonclick || uncommonclick || rareclick)
                {
                    if (helpf.wtsmenue)
                    {
                        generator.sellOwnCardsFilter.setRarityFilter(srchsvr.commonbool, srchsvr.uncommonbool, srchsvr.rarebool);
                    }
                    else
                    {
                        generator.buyOwnCardsFilter.setRarityFilter(srchsvr.commonbool, srchsvr.uncommonbool, srchsvr.rarebool);
                    }

                }

                /*
                //if (wtsmenue) { pricecheck = (pricecopy.Length < this.pricesearchstring.Length) || (pricecopy.Length != this.pricesearchstring.Length && pricesearchstring == ""); } else { pricecheck = pricecopy.Length > this.pricesearchstring.Length; }
                //clear p1moddedlist only if necessary
                if (selfcopy.Length != srchsvr.wtssearchstring.Length || closeclick || (growthclick && srchsvr.growthbool) || (orderclick && srchsvr.orderbool) || (energyclick && srchsvr.energybool) || (decayclick && srchsvr.decaybool) || (commonclick && srchsvr.commonbool) || (uncommonclick && srchsvr.uncommonbool) || (rareclick && srchsvr.rarebool) || mt3click || mt0click)
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
                */

                // draw generate button!


                GUI.color = Color.white;
                if (helpf.wtsmenue)
                {
                    if (srchsvr.shortgeneratedwtsmessage == "")
                    { GUI.color = dblack; }
                }
                else
                {
                    if (srchsvr.shortgeneratedwtbmessage == "")
                    { GUI.color = dblack; }
                }

                if (GUI.Button(recto.sbclrearpricesbutton, "Post to Network"))
                {

                    if (ntwrk.contonetwork)
                    {
                        ntwrk.sendownauctiontoall(helpf.wtsmenue, srchsvr.getshortgenmsg(true), srchsvr.getshortgenmsg(false));

                    }

                }
                GUI.color = Color.white;



                if (helpf.wtsmenue)
                {

                    if (GUI.Button(recto.sbgeneratebutton, "Gen WTS msg"))
                    {
                        // start trading with seller
                        generatewtxmsg(generator.getAllOwnSellOffers());
                    }

                }
                else
                {
                    if (GUI.Button(recto.sbgeneratebutton, "Gen WTB msg"))
                    {
                        // start trading bith buyer
                        generatewtxmsg(generator.getAllOwnBuyOffers());
                    }
                }

                // draw message save/load buttons
                GUI.color = Color.white;
                if (helpf.wtsmenue)
                {
                    if (!helpf.canLoadWTSmsg) GUI.color = dblack;
                    if (GUI.Button(recto.sbloadbutton, "load WTS msg") && helpf.canLoadWTSmsg)
                    {

                        for (int i = 0; i < prcs.wtspricelist1.Count; i++)
                        {
                            KeyValuePair<int, string> item = prcs.wtspricelist1.ElementAt(i);
                            prcs.wtspricelist1[item.Key] = "";

                        }


                        string textel = System.IO.File.ReadAllText(helpf.ownaucpath + "wtsauc.txt");

                        string secmsg = (textel.Split(new string[] { "aucs " }, StringSplitOptions.None))[1];
                        string[] words = secmsg.Split(';');
                        foreach (string w in words)
                        {
                            if (w == "" || w == " ") continue;
                            //string cardname = helpf.cardnames[Array.FindIndex(helpf.cardids, element => element == Convert.ToInt32(w.Split(' ')[0]))];
                            prcs.wtspricelist1[Convert.ToInt32(w.Split(' ')[0])] = w.Split(' ')[1];
                        }
                        generatewtxmsg(generator.getAllOwnSellOffers());


                    }

                }
                else
                {
                    if (!helpf.canLoadWTBmsg) GUI.color = dblack;
                    if (GUI.Button(recto.sbloadbutton, "load WTB msg") && helpf.canLoadWTBmsg)
                    {
                        for (int i = 0; i < prcs.wtbpricelist1.Count; i++)
                        {
                            KeyValuePair<int, string> item = prcs.wtbpricelist1.ElementAt(i);
                            prcs.wtbpricelist1[item.Key] = "";

                        }
                        string textel = System.IO.File.ReadAllText(helpf.ownaucpath + "wtbauc.txt");
                        string secmsg = (textel.Split(new string[] { "aucb " }, StringSplitOptions.None))[1];
                        string[] words = secmsg.Split(';');
                        foreach (string w in words)
                        {
                            if (w == "" || w == " ") continue;
                            //string cardname = helpf.cardnames[Array.FindIndex(helpf.cardids, element => element == Convert.ToInt32(w.Split(' ')[0]))];
                            prcs.wtbpricelist1[Convert.ToInt32(w.Split(' ')[0])] = w.Split(' ')[1];
                        }
                        generatewtxmsg(generator.getAllOwnBuyOffers());
                    }
                }
                GUI.color = Color.white;

                GUI.color = Color.white;
                if (helpf.wtsmenue)
                {
                    if (srchsvr.generatedwtsmessage == "") GUI.color = dblack;
                    if (GUI.Button(recto.sbsavebutton, "save WTS msg"))
                    {
                        helpf.showtradedialog = true;


                    }

                }
                else
                {
                    if (srchsvr.generatedwtbmessage == "") GUI.color = dblack;
                    if (GUI.Button(recto.sbsavebutton, "save WTB msg"))
                    {
                        helpf.showtradedialog = true;

                    }
                }
                GUI.color = Color.white;

            }
            
            // Draw generator here:
            if (helpf.generator)
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

                if (helpf.wtsmenue) this.ahlist = generator.getOwnSellOffers();
                else this.ahlist = generator.getOwnBuyOffers();

                this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * (float)this.ahlist.Count));
                int num = 0;
                Card card = null;
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
                        /* if (current.card.level > 0)
                         {
                             string text3 = text2;
                             text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.card.level + 1, "</color>, " });
                             num2 += "<color=#rrggbb></color>".Length;
                         }*/
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
                        string gold = "Price";
                        GUI.skin = helpf.cardListPopupBigLabelSkin;
                        vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        Rect position12 = new Rect(nextx + 2f, (float)num * recto.fieldHeight, recto.labelsWidth / 2f, recto.fieldHeight);
                        GUI.Label(position12, gold);


                        //draw pricebox
                        //
                        Rect position11 = new Rect(position12.xMax + 2f, (float)(num + 1) * recto.fieldHeight - (recto.fieldHeight + vector2.y) / 2 - 2, recto.labelsWidth, vector2.y + 4);
                        GUI.skin = helpf.cardListPopupSkin;
                        GUI.Box(position11, string.Empty);
                        // priceinint wurde bei der genliste missbraucht

                        helpf.chatLogStyle.alignment = TextAnchor.MiddleCenter;
                        if (!helpf.showtradedialog) //otherwise you cant hit the cancel button
                        {
                            if (helpf.wtsmenue)
                            {
                                prcs.wtspricelist1[current.card.getType()] = Regex.Replace(GUI.TextField(position11, prcs.wtspricelist1[current.card.getType()], helpf.chatLogStyle), @"[^0-9]", "");
                            }
                            else
                            {
                                prcs.wtbpricelist1[current.card.getType()] = Regex.Replace(GUI.TextField(position11, prcs.wtbpricelist1[current.card.getType()], helpf.chatLogStyle), @"[^0-9]", "");
                            }
                        }
                        helpf.chatLogStyle.alignment = TextAnchor.MiddleLeft;
                        //string sellername = current.seller;
                        GUI.skin = helpf.cardListPopupBigLabelSkin;

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
                        if (helpf.wtsmenue)
                        {
                            if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "SP"))
                            {
                                //int index = Array.FindIndex(helpf.cardnames, element => element.Equals(current.card.getName().ToLower()));
                                int index = helpf.cardnameToArrayIndex(current.card.getName().ToLower());
                                if (index >= 0)
                                {
                                    prcs.PriceChecker(index, true, current.card.getType());
                                }
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "SP"))
                            {
                                //int index = Array.FindIndex(helpf.cardnames, element => element.Equals(current.card.getName().ToLower()));
                                int index = helpf.cardnameToArrayIndex(current.card.getName().ToLower());
                                if (index >= 0)
                                {
                                    prcs.PriceChecker(index, false, current.card.getType());
                                }
                            }
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
                if (helpf.wtsmenue)
                {

                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }

                if (GUI.Button(recto.wtsbuttonrect, "WTS") && !helpf.showtradedialog)
                {
                    //
                    helpf.wtsmenue = true; 
                    this.wtsingen = true;
                    srchsvr.setsettings(false, true);
                    //
                }
                if (!helpf.wtsmenue)
                {

                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                if (GUI.Button(recto.wtbbuttonrect, "WTB") && !helpf.showtradedialog)
                {
                    //alists.setAhlistsToGenWtbLists();
                    srchsvr.setsettings(false, false);
                    helpf.wtsmenue = false;
                    this.wtsingen = false;
                    //lstfltrs.fullupdatelist(alists.ahlist, alists.ahlistfull, helpf.inauchouse, helpf.wtsmenue, helpf.generator);
                }
                GUI.color = Color.white;
                if (GUI.Button(recto.fillbuttonrect, "Fill"))
                {
                    if (helpf.wtsmenue)
                    {
                        foreach (Auction c in this.ahlist)
                        {
                            //int price=this.upperprice[Array.FindIndex(cardids, element => element == c.card.getType())];
                            int price = 0;
                            //price = prcs.pricerounder(Array.FindIndex(helpf.cardids, element => element == c.card.getType()), helpf.wtsmenue);
                            price = prcs.pricerounder(helpf.cardidsToIndex[c.card.getType()], helpf.wtsmenue);
                            prcs.wtspricelist1[c.card.getType()] = price.ToString();

                        }
                    }
                    else
                    {
                        foreach (Auction c in this.ahlist)
                        {

                            //int price = this.lowerprice[Array.FindIndex(cardids, element => element == c.card.getType())];
                            int price = 0;
                            //price = prcs.pricerounder(Array.FindIndex(helpf.cardids, element => element == c.card.getType()), helpf.wtsmenue);
                            price = prcs.pricerounder(helpf.cardidsToIndex[c.card.getType()], helpf.wtsmenue);
                            prcs.wtbpricelist1[c.card.getType()] = price.ToString();

                        }
                    }

                }

                if (GUI.Button(recto.updatebuttonrect, "Clear"))
                {
                    if (helpf.wtsmenue)
                    {
                        for (int i = 0; i < prcs.wtspricelist1.Count; i++)
                        {
                            KeyValuePair<int, string> item = prcs.wtspricelist1.ElementAt(i);
                            prcs.wtspricelist1[item.Key] = "";

                        }

                    }
                    else
                    {
                        for (int i = 0; i < prcs.wtbpricelist1.Count; i++)
                        {
                            KeyValuePair<int, string> item = prcs.wtbpricelist1.ElementAt(i);
                            prcs.wtbpricelist1[item.Key] = "";

                        }
                    }

                }
                if (helpf.showtradedialog) { this.reallywanttosave(helpf.wtsmenue); }

            }
            GUI.color = Color.white;
            GUI.contentColor = Color.white;
        }


        private void reallywanttosave(bool wts)
        {
            // asks the user if he wants to trade
            GUI.skin = helpf.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = helpf.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string message = "You want to override existing file?";

            GUI.Label(recto.tradingbox, message);
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin = helpf.cardListPopupLeftButtonSkin;

            if (GUI.Button(recto.tbok, "OK"))
            {
                if (wts)
                {
                    System.IO.File.WriteAllText(helpf.ownaucpath + "wtsauc.txt", srchsvr.shortgeneratedwtsmessage);
                    helpf.canLoadWTSmsg = true;
                }
                else
                {
                    System.IO.File.WriteAllText(helpf.ownaucpath + "wtbauc.txt", srchsvr.shortgeneratedwtbmessage);
                    helpf.canLoadWTBmsg = true;
                }
                helpf.showtradedialog = false;
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


        private void generatewtxmsg(List<Auction> liste)
        {
            string msg = "";
            string shortmsg = "";
            List<Auction> postlist = new List<Auction>();
            for (int i = 0; i < liste.Count; i++)
            {
                if (helpf.wtsmenue)
                {
                    if (prcs.wtspricelist1[liste[i].card.getType()] != "")
                    {

                        string price = prcs.wtspricelist1[liste[i].card.getType()];
                        //ai.priceinint = ;
                        Auction ai = new Auction("", DateTime.Now, Auction.OfferType.SELL, liste[i].card, "", Convert.ToInt32(price));
                        postlist.Add(ai);
                        //msg = msg + liste[i].card.getName() + " " + this.wtspricelist1[liste[i].card.getName().ToLower()] + ";";
                        //shortmsg = shortmsg + liste[i].card.getType() + " " + this.wtspricelist1[liste[i].card.getName().ToLower()] + ";"; 
                    }
                }
                else
                {
                    if (prcs.wtbpricelist1[liste[i].card.getType()] != "")
                    {
                        string price = prcs.wtbpricelist1[liste[i].card.getType()];
                        Auction ai = new Auction("", DateTime.Now, Auction.OfferType.SELL, liste[i].card, "", Convert.ToInt32(price));
                        postlist.Add(ai);
                        //msg = msg + liste[i].card.getName() + " " + this.wtbpricelist1[liste[i].card.getName().ToLower()] + ";";
                        //shortmsg = shortmsg + liste[i].card.getType() + " " + this.wtbpricelist1[liste[i].card.getName().ToLower()] + ";"; 
                    }
                }
            }

            postlist.Sort(delegate(Auction p1, Auction p2) { return (p1.price).CompareTo(p2.price); });
            postlist.Reverse();
            Dictionary<string, string> shortlist = new Dictionary<string, string>();

            for (int i = 0; i < postlist.Count; i++)
            {
                Auction ai = postlist[i];
                if (i < postlist.Count - 1 && postlist[i + 1].price == ai.price)
                {
                    msg = msg + ai.card.getName() + ", ";
                    shortmsg = shortmsg + ai.card.getType() + ",";
                }
                else
                {
                    if (ai.price == 0)
                    {
                        msg = msg + ai.card.getName() + ";";
                        shortmsg = shortmsg + ai.card.getType() + ";";
                    }
                    else
                    {

                        msg = msg + ai.card.getName() + " " + ai.price + "g, ";
                        shortmsg = shortmsg + ai.card.getType() + " " + ai.price + ";";
                    }

                }
            }

            if (msg != "")
            {
                if (helpf.wtsmenue) { msg = "WTS " + msg; shortmsg = "aucs " + shortmsg; } else { msg = "WTB " + msg; shortmsg = "aucb " + shortmsg; }
                msg = msg.Remove(msg.Length - 2);
                shortmsg = shortmsg.Remove(shortmsg.Length - 1);
            }
            if (msg.Length < 512) systemCopyBufferProperty.SetValue(null, msg, null);
            if (msg.Length >= 512) { msg = "msg to long"; }
            if (shortmsg.Length >= 512) { shortmsg = ""; msg = msg + ", networkmsg too"; }
            if (helpf.wtsmenue) { srchsvr.generatedwtsmessage = msg; srchsvr.shortgeneratedwtsmessage = shortmsg; } else { srchsvr.generatedwtbmessage = msg; srchsvr.shortgeneratedwtbmessage = shortmsg; }
            //Console.WriteLine(msg);
            //Console.WriteLine(shortmsg);
            srchsvr.sellersearchstring = msg;
            srchsvr.pricesearchstring = shortmsg;
            
            

        }

        




    }
}
