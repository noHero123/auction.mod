using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonFx.Json;
using UnityEngine;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Auction.mod
{
    public class Helpfunktions
    {

        private static Helpfunktions instance;

        public static Helpfunktions Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Helpfunktions();
                }
                return instance;
            }
        }

        private Helpfunktions()
        {
            this.hideInformationinfo = typeof(Store).GetMethod("hideInformation", BindingFlags.Instance | BindingFlags.NonPublic);
            this.showBuyinfo = typeof(Store).GetField("showBuy", BindingFlags.Instance | BindingFlags.NonPublic);
            this.showSellinfo = typeof(Store).GetField("showSell", BindingFlags.Instance | BindingFlags.NonPublic);
            this.targetchathightinfo = typeof(ChatUI).GetField("targetChatHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            this.buymen = typeof(Store).GetField("buyMenuObj", BindingFlags.Instance | BindingFlags.NonPublic);
            this.sellmen = typeof(Store).GetField("sellMenuObj", BindingFlags.Instance | BindingFlags.NonPublic);
            this.setskins((GUISkin)Resources.Load("_GUISkins/CardListPopup"), (GUISkin)Resources.Load("_GUISkins/CardListPopupGradient"), (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel"), (GUISkin)Resources.Load("_GUISkins/CardListPopupLeftButton"));
        }


        public readonly double deleteTime = 30.0;
        public FieldInfo targetchathightinfo;
        public ChatUI target = null;
        public MethodInfo hideInformationinfo;
        public FieldInfo buymen, sellmen;

        public Store storeinfo;
        public FieldInfo showBuyinfo;
        public FieldInfo showSellinfo;


        public List<long> cardsForTradeIds = new List<long>();
        public bool createAuctionMenu = false; // true if the Create-Auction menu is shown
        public bool playerStoreMenu = false; // true if the playerStore menu is shown
        public bool wtsmenue = false;
        public bool bothmenue = false;


        public bool addmode = false;
        public string createdAuctionText = "";

        public bool chatisshown = false;
        public bool canLoadWTSmsg = false;
        public bool canLoadWTBmsg = false;
        public string ownaucpath = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "auc" + System.IO.Path.DirectorySeparatorChar;
        
        public bool inauchouse = false;
        public bool generator = false;
        public bool settings = false;

        public string postmsgmsg = "";
        public bool postmsgontrading = false;
        public bool postmsggetnextroomenter = false;
        public bool showtradedialog = false;
        public int[] cardids;
        public string[] cardnames;
        public int[] cardImageid;
        public string[] cardType;

        public Dictionary<long, string> auctionBotCardsToNames = new Dictionary<long, string>();
        
        public bool makeOfferMenu = false;
        public bool offerMenuSelectCardMenu = false;
        public bool deleteAuctionMenu = false;

        public Dictionary<string, int> cardnamesToIndex = new Dictionary<string, int>();
        public Dictionary<int, int> cardidsToIndex = new Dictionary<int, int>();
        public Dictionary<int, int> cardIDToImgidDic = new Dictionary<int, int>();
        public Dictionary<int, string> cardidsToCardnames = new Dictionary<int, string>();
        public Dictionary<string, int> cardnamesToID = new Dictionary<string, int>();
        public Dictionary<int, int> cardIDToNumberOwned = new Dictionary<int, int>();
        public Dictionary<int, long> cardIDToCardNumber = new Dictionary<int, long>();

        public Dictionary<string, ChatUser> globalusers = new Dictionary<string, ChatUser>();
        public GUISkin cardListPopupSkin;
        public GUISkin cardListPopupGradientSkin;
        public GUISkin cardListPopupBigLabelSkin;
        public GUISkin cardListPopupLeftButtonSkin;
        public GUISkin lobbySkin;
        public GUIStyle chatLogStyle;

        public bool nicks = false;
        public List<nickelement> loadedscrollsnicks = new List<nickelement>();
        // for card-AmountFilters
        public List<Card> allOwnCards = new List<Card>();
        public List<Auction> allOwnTradeableAuctions = new List<Auction>();
        public bool playerstoreAllCardsChanged = false;
        public bool auctionHouseAllCardsChanged = false;
        public bool generatorAllCardsChanged = false;


        public void setOwnAucPath(string s)
        {
            this.ownaucpath = s;
        }

        public int cardnameToArrayIndex(string name)
        {
            int ret;
            if (cardnamesToIndex.TryGetValue(name, out ret)) return ret;
            return -1;
        }
        public int cardidToArrayIndex(int id)
        {
            int ret;
            if (cardidsToIndex.TryGetValue(id, out ret)) return ret;
            return -1;
        }
        public int cardIDtoimageid(int id)
        {
            int ret;
            if (cardIDToImgidDic.TryGetValue(id, out ret)) return ret;
            return -1;
        }

        public void setskins(GUISkin cllps, GUISkin clpgs, GUISkin clpbls, GUISkin clplbs)
        {
            this.cardListPopupSkin = cllps;
            this.cardListPopupGradientSkin = clpgs;
            this.cardListPopupBigLabelSkin = clpbls;
            this.cardListPopupLeftButtonSkin = clplbs;

        }

        public void adjustskins(float fieldHeight)
        {
            this.cardListPopupBigLabelSkin.label.fontSize = (int)(fieldHeight / 1.7f);
            this.cardListPopupSkin.label.fontSize = (int)(fieldHeight / 2.5f);
        }

        public void setlobbyskin(GUISkin lby)
        {
            this.lobbySkin = lby;
        }

        public void setchatlogstyle(GUIStyle cls)
        {
            this.chatLogStyle = cls;
        }

        public void setarrays(Message msg)
        {
            
           
            /*
            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(msg.getRawText());
            Dictionary<string, object>[] d = (Dictionary<string, object>[])dictionary["cardTypes"];
            this.cardids = new int[d.GetLength(0)];
            this.cardnames = new string[d.GetLength(0)];
            this.cardImageid = new int[d.GetLength(0)];
            this.cardType = new string[d.GetLength(0)];
             */

            CardTypesMessage cmsg = (CardTypesMessage)msg;
            this.cardids = new int[cmsg.cardTypes.Length];
            this.cardnames = new string[cmsg.cardTypes.Length];
            this.cardImageid = new int[cmsg.cardTypes.Length];
            this.cardType = new string[cmsg.cardTypes.Length];
            int i = 0;
            foreach (CardType c in cmsg.cardTypes)
            {
                this.cardids[i] = c.id;
                this.cardnames[i] = c.name.ToLower();
                this.cardImageid[i] = c.cardImage;
                this.cardType[i] = c.kind.ToString();
                i++;
            }
            /*
            for (int i = 0; i < d.GetLength(0); i++)
            {
                this.cardids[i] = Convert.ToInt32(d[i]["id"]);
                this.cardnames[i] = d[i]["name"].ToString().ToLower();
                this.cardImageid[i] = Convert.ToInt32(d[i]["cardImage"]);
                this.cardType[i] = d[i]["kind"].ToString();
            }*/
            generatedictionarys();
        }

        public void readnicksfromfile()
        {
            if (this.nicks)
            {
                string[] lines = System.IO.File.ReadAllLines(this.ownaucpath + "nicauc.txt");
                foreach (string s in lines)
                {
                    if (s == "" || s == " ") continue;
                    string cardname = s.Split(':')[0];
                    if (this.cardnames.Contains(cardname.ToLower()))
                    {
                        string[] nickes = (s.Split(':')[1]).Split(',');
                        foreach (string n in nickes)
                        {
                            nickelement nele;
                            nele.nick = n.ToLower();
                            nele.cardname = cardname.ToLower();
                            this.loadedscrollsnicks.Add(nele);

                        }
                    }

                }


            }
        }


        private void generatedictionarys()
        {
            this.cardidsToIndex.Clear();
            this.cardnamesToIndex.Clear();
            this.cardIDToImgidDic.Clear();
            this.cardidsToCardnames.Clear();
            this.cardnamesToID.Clear();
            for (int i = 0; i < cardids.Length; i++)
            {
                this.cardidsToIndex.Add(this.cardids[i], i);
                this.cardnamesToIndex.Add(cardnames[i], i);
                this.cardIDToImgidDic.Add(cardids[i], cardImageid[i]);
                this.cardidsToCardnames.Add(cardids[i], cardnames[i]);
                this.cardnamesToID.Add(cardnames[i], cardids[i]);

            }

        }


        public void setOwnCards(Message msg)
        {//set id-cardid dictionary
            this.cardIDToCardNumber.Clear();
            this.allOwnTradeableAuctions.Clear();
            foreach (Card c in (((LibraryViewMessage)msg).cards))
            {
                if (c.tradable)
                {
                    Auction a = new Auction(App.MyProfile.ProfileInfo.name, DateTime.Now, Auction.OfferType.SELL, c, "");
                    this.allOwnTradeableAuctions.Add(a);
                }

                if (c.tradable && !this.cardIDToCardNumber.ContainsKey(c.typeId))
                {
                    this.cardIDToCardNumber.Add(c.typeId, c.id);
                }
            }

            this.allOwnCards.Clear();
            this.allOwnCards.AddRange(((LibraryViewMessage)msg).cards);

            this.playerstoreAllCardsChanged = true;
            this.auctionHouseAllCardsChanged = true;
            this.generatorAllCardsChanged = true;
            }

        public void setAuctionModCards(Message msg)
        {//set id-cardid dictionary
            this.auctionBotCardsToNames.Clear();
            foreach (Card c in (((LibraryViewMessage)msg).cards))
            {
                if (c.tradable)
                {
                    this.auctionBotCardsToNames.Add(c.id,c.getName());
                }
            }
        }

        public void messegparsingtest()
        {
            RoomChatMessageMessage msg = new RoomChatMessageMessage("parsertest", "");
            msg.from = "Bob";

            msg.text = "wts burn 100, quake 200";
            App.ArenaChat.ChatRooms.ChatMessage(msg);

            msg.text = "wtb bunny 100, husk";
            App.ArenaChat.ChatRooms.ChatMessage(msg);

            msg.text = "aucb 198 4;176 3;171 2;65 1";
            App.ArenaChat.ChatRooms.ChatMessage(msg);

            msg.text = "aucs 198,176 3;171,65 0";
            App.ArenaChat.ChatRooms.ChatMessage(msg);
        }


    }
        
}
