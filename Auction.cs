using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JsonFx.Json;
using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
using Irrelevant.Assets;
using ScrollsModLoader;
using System.Net;

namespace Auction.mod
{


    using System;
    using UnityEngine;


    public class Auction : BaseMod, ICommListener, iEffect, iCardRule, ICardListCallback
    {

        private bool hidewispers = false;




        string SPretindex=""; bool SPtarget=false;

        int[] lowerprice=new int[0];
        int[] upperprice = new int[0];

        public void totalpricecheck()
            {

                //WebClient wc = new WebClient(); // webclient has no timeout
                //string ressi= wc.DownloadString(new Uri("http://api.scrollspost.com/v1/price/1-day/" + search + ""));

                WebRequest myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/1-day/");
                myWebRequest.Timeout = 10000;
                WebResponse myWebResponse = myWebRequest.GetResponse();
                System.IO.Stream stream = myWebResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
                string ressi = reader.ReadToEnd();
                //Console.WriteLine(ressi);

                JsonReader jsonReader = new JsonReader();
                Dictionary<string, object>[] dictionary = (Dictionary<string, object>[])jsonReader.Read(ressi);
                for (int i = 0; i < dictionary.GetLength(0); i++)
                {
                    int id = Convert.ToInt32(dictionary[i]["card_id"]); 
                    Dictionary<string, object> d = (Dictionary<string, object>)dictionary[i]["price"];
                    int lower = 0; int higher = 0;

                    int sug = (int)d["suggested"];
                    int high = (int)d["buy"];
                    int low = (int)d["sell"];
                    if (sug != 0) 
                    { lower = sug; 
                        higher = lower; 
                    }
                    if (high != 0) 
                    {
                        int value = high;
                        if (value < lower || (value != 0 && lower == 0)) { lower = value; }
                        if (value > higher || (value != 0 && higher == 0)) { higher = value; }
                    }
                    if (low != 0)
                    {
                        int value = low;
                        if (value < lower || (value != 0 && lower == 0)) { lower = value; }
                        if (value > higher || (value != 0 && higher == 0)) { higher = value; }
                    }


                    int index=Array.FindIndex(cardids, element => element == id);
                    if (index >= 0)
                    {
                        lowerprice[index] = lower;
                        upperprice[index] = higher;
                    }

                }
                
            }

        /*
        public void PriceCheck( String search)
            {

                //WebClient wc = new WebClient(); // webclient has no timeout
               //string ressi= wc.DownloadString(new Uri("http://api.scrollspost.com/v1/price/1-day/" + search + ""));

                WebRequest myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/price/1-day/" + search + "");
                if (search == "") { myWebRequest = WebRequest.Create("http://api.scrollspost.com/v1/prices/1-day/"); }
               myWebRequest.Timeout = 10000;
               WebResponse myWebResponse = myWebRequest.GetResponse();
               System.IO.Stream stream = myWebResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
                string ressi = reader.ReadToEnd();
               //Console.WriteLine(ressi);
                string price="0";
                string sugprice = "0";
                if (ressi.Contains("\"suggested\":"))
                {

                    sugprice = (ressi.Split(new string[] { "\"suggested\":" }, StringSplitOptions.None))[1];
                    sugprice = sugprice.Split(',')[0];
                    if (this.SPtarget)
                    {
                        //price = (ressi.Split(new string[] { "\"suggested\":" }, StringSplitOptions.None))[1];
                        //price = price.Split(',')[0];
                        price = (ressi.Split(new string[] { "\"sell\":" }, StringSplitOptions.None))[1];
                        price = price.Split('}')[0];
                        if (price == "0") { price = sugprice; }

                    }
                    else
                    {
                        price = (ressi.Split(new string[] { "\"buy\":" }, StringSplitOptions.None))[1];
                        price = price.Split(',')[0];
                        if (price == "0") { price = sugprice; }
                    }
                }
                else { price="error";}
                if (this.SPtarget) { wtspricelist1[SPretindex.ToLower()] = price; } else { wtbpricelist1[SPretindex.ToLower()] = price; } 

            }
         */

        public void PriceChecker( String search)
        {
            int index=Array.FindIndex(cardnames, element => element.Equals(search.ToLower()));
            if (index >= 0)
            {
                if (this.SPtarget) { wtspricelist1[SPretindex.ToLower()] = this.lowerprice[index].ToString(); }
                else { wtbpricelist1[SPretindex.ToLower()] = this.upperprice[index].ToString(); }
            }
        }


        struct cardtextures
        {
        public Texture cardimgimg;
        public Rect cardimgrec;
        }

        struct renderwords
        {
            public string text;
            public GUIStyle style;
            public Rect rect;
            public Color color;
        }

        struct aucitem
        {
            public Card card;
            public string seller;
            public string price;
            public int priceinint;
            public string time;
            public DateTime dtime;
            public string whole;
        }

        class settingcopy
        {
            public bool boolean0 ;
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
            public string strings0 ;
            public string strings1;
            public string strings2;
            public string strings3;
            public string strings4;

        
        }

        class aucitemnamecomparer : IComparer<aucitem>
        {
            public int Compare(aucitem x, aucitem y)
            {
                return (x.card.getName()).CompareTo(y.card.getName());
            }
        }

        class aucitemgoldcomparer : IComparer<aucitem>
        {
            public int Compare(aucitem x, aucitem y)
            {
                return (x.priceinint).CompareTo(y.priceinint);
            }
        }
        class aucitemsellercomparer : IComparer<aucitem>
        {
            public int Compare(aucitem x, aucitem y)
            {
                return (x.seller).CompareTo(y.seller);
            }
        }

        private Dictionary<string, string> aucusers=new Dictionary<string,string>();
        int idtesting = 0;

        bool contonetwork = false;
        List<string> roooms= new List<string>();
        DateTime joindate = DateTime.Now;
        Dictionary<string, string> usertoaucroom=new Dictionary<string,string>();
        int ownroomnumber = 0;
        bool rooomsearched = false;

        private bool inbattle = false;
        private bool newwtsmsgs = false;
        private bool newwtbmsgs = false;
        private string shortgeneratedwtsmessage = "";
        private string shortgeneratedwtbmessage = "";
        private string generatedwtsmessage = "";
        private string generatedwtbmessage = "";
        private Dictionary<string, string> wtspricelist1 = new Dictionary<string, string>();
        private Dictionary<string, string> wtbpricelist1 = new Dictionary<string, string>();
        Color dblack = new Color(1f, 1f, 1f, 0.5f);
        private bool wtsmenue=false;
        private bool selectable;
        private bool clickableItems;
        private Store storeinfo;
        private bool generator;
        private bool inauchouse;
        private string ownname;
        private string ownid;

        Vector2 scrolll = new Vector2(0, 0);

        private int longestcardname;
        private GUISkin lobbySkin;
        private List<aucitem> ahlist = new List<aucitem>();
        private List<aucitem> ahlistfull = new List<aucitem>();
        private List<aucitem> wtslistfull = new List<aucitem>();
        private List<aucitem> wtblistfull = new List<aucitem>();
        private List<aucitem> wtslistfulltimed = new List<aucitem>();
        private List<aucitem> wtblistfulltimed = new List<aucitem>();
        private List<aucitem> wtslist = new List<aucitem>();
        private List<aucitem> wtblist = new List<aucitem>();
        private int sortmode = 0;
        private bool reverse = false;
        //private List<Card> genlist=new List<Card>();
        //private List<Card> genlistfull = new List<Card>();
        private List<aucitem> wtsPlayer = new List<aucitem>();
        private List<aucitem> orgicardsPlayerwountrade = new List<aucitem>(); // cards player owns minus the untradable cards
        private List<Card> orgicardsPlayer = new List<Card>(); // all cards the player owns
        private List<aucitem> wtbPlayer = new List<aucitem>();
        private List<aucitem> allcardsavailable = new List<aucitem>();
        private List<aucitem> addingwtscards = new List<aucitem>();
        private List<aucitem> addingwtbcards = new List<aucitem>();

        
        //searchmenue
        private bool growthbool;
        private bool orderbool;
        private bool energybool;
        private bool decaybool;
        private bool commonbool;
        private bool uncommonbool;
        private bool rarebool;
        private bool threebool;
        private bool onebool;
        private bool ignore0;
        private bool takepriceformgenarator;
        private string timesearchstring = "";
        private string wtssearchstring = "";
        private string sellersearchstring = "";
        private string pricesearchstring = "";
        private string pricesearchstring2 = "";

        private settingcopy ahwtssettings = new settingcopy();
        private settingcopy ahwtbsettings = new settingcopy();
        private settingcopy genwtssettings = new settingcopy();
        private settingcopy genwtbsettings = new settingcopy();


        
        private Rect filtermenurect;
        private Rect sbarlabelrect;
        private Rect sbrect;
        private Rect sbgrect; private Rect sborect; private Rect sberect; private Rect sbdrect;
        private Rect sbcommonrect; private Rect sbuncommonrect; private Rect sbrarerect; private Rect sbthreerect; private Rect sbonerect;
        private Rect sbsellerlabelrect;private Rect sbsellerrect;
        private Rect sbpricelabelrect; private Rect sbpricerect; private Rect sbclearrect; private Rect sbgeneratebutton;
        private Rect sbpricerect2;
        private Rect sbonlywithpricebox; private Rect sbonlywithpricelabelbox;
        private Rect tradingbox; private Rect tbok; private Rect tbcancel; private Rect tbmessage; private Rect tbmessagescroll;
        private Rect sbtpfgen; private Rect sbtpfgenlabel;
        private Rect sbclrearpricesbutton; Rect sbnetworklabel;
        private Rect sbtimelabel; Rect sbtimerect;

        private bool showtradedialog = false; private aucitem tradeitem;

        //cardlistpopup#######################
        private float opacity;
        public Vector2 scrollPos;
        private Texture itemButtonTexture;
        private List<Card> selectedCards = new List<Card>();
        private float costIconSize;
	    private float costIconWidth;
	    private float costIconHeight;
	    private float cardHeight;
    	private float cardWidth;
    	private float labelX;
    	private float labelsWidth;
    	private int maxCharsName;
	    private int maxCharsRK;
        private float scrollBarSize = 20f;
        private Vector4 margins;
        private float BOTTOM_MARGIN_EXTRA = (float)Screen.height * 0.047f;
        Rect screenRect;
        Rect outerRect;
        Rect innerBGRect;
        Rect innerRect;
        Rect buttonLeftRect;
        Rect buttonRightRect;
        Rect wtsbuttonrect;
        Rect wtbbuttonrect;
        Rect updatebuttonrect;

        private float fieldHeight;
        private GUISkin cardListPopupSkin ;
        private GUISkin cardListPopupGradientSkin ;
        private GUISkin cardListPopupBigLabelSkin ;
        private GUISkin cardListPopupLeftButtonSkin;


        private List<renderwords> textsArr = new List<renderwords>();
        private List<cardtextures> gameObjects = new List<cardtextures>();
        private FieldInfo icoField;
        private FieldInfo statsBGField;
        private FieldInfo icoBGField;
        private FieldInfo gosNumHitPointsField;
        private FieldInfo gosNumAttackPowerField;
        private FieldInfo gosNumCountdownField;
        private FieldInfo gosNumCostField;
        private FieldInfo gosactiveAbilityField;
        private FieldInfo textsArrField;
        private FieldInfo cardImageField;

        int screenh = 0;
        int screenw = 0;
        float scalefactor=1.0f;
        Camera cam=new Camera();
        Texture cardtext;
        Rect cardrect;
        int clicked = 0;
        private const bool debug = false;

        private MethodInfo hideInformationinfo;
        private FieldInfo showBuyinfo;
        private FieldInfo showSellinfo;

        private FieldInfo chatScrollinfo;
        private FieldInfo allowSendingChallengesinfo; 
        private FieldInfo userContextMenuinfo ;
        private FieldInfo chatlogAreaInnerinfo;
        private FieldInfo chatRoomsinfo;
        private FieldInfo timeStampStyleinfo;
        private FieldInfo chatLogStyleinfo;
        private FieldInfo userContextMenuField;

        private MethodInfo CloseUserMenuinfo;
        private MethodInfo createUserMenuinfo;
        private MethodInfo challengeUserMethod;    
        private MethodInfo profileUserMethod;
        private MethodInfo tradeUserMethod;
        private MethodInfo drawsubmenu;
        private MethodInfo getchatlogMethod;

        private ChatUI target = null;
        private ChatRooms chatRooms;
        private GUIStyle chatLogStyle;
        
        //private MethodInfo createUserMenu;
        private Regex userRegex;
        private Regex linkFinder;
        private Regex cardlinkfinder;

        private Dictionary<string, ChatUser> globalusers = new Dictionary<string, ChatUser>();
        private int[] cardids;
        private string[] cardnames;
        private int[] cardImageid;
        private string[] cardType;

        private GameObject cardRule;
        private GameObject GUIObject;
        bool mytext=false;
        Dictionary<string, int> available = new Dictionary<string, int>();
        //CardOverlay cardOverlay;
        

        private int cardnametoid(string name) { return cardids[Array.FindIndex(cardnames, element => element.Equals(name))]; }
        private int cardnametoimageid(string name) { return cardImageid[Array.FindIndex(cardnames, element => element.Equals(name))]; }

        private Regex priceregex; private Regex numberregx; private Regex priceregexpriceonname;

        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");
        Texture2D arrowdown = ResourceManager.LoadTexture("ChatUI/dropdown_arrow");
        FieldInfo buymen; FieldInfo sellmen;

        bool chatisshown = false;

        string[] auccontroler = new string[] { };

        public void handleMessage(Message msg)
        {

            if (msg is GameInfoMessage && this.contonetwork)
            {
                GameInfoMessage gim =(GameInfoMessage) msg;
                if (this.inbattle == false) { this.inbattle = true; this.disconfromaucnet(); Console.WriteLine("discon"); }
            }

            if (msg is RoomInfoMessage && this.contonetwork)
            {
                RoomInfoMessage roominfo = (RoomInfoMessage)msg;
                if (roominfo.roomName.StartsWith("auc-"))
                {
                    // if the update list contains more than 1 user, then ,
                    RoomInfoProfile[] rip =roominfo.updated;
                    if (rip.Length >= 1) // he joins the room, add him
                    {
                        
                        foreach (RoomInfoProfile roinpro in rip) // add the new user to the aucusers and globalusers
                        {
                            if (!this.globalusers.ContainsKey(roinpro.name))
                            {
                                ChatUser newuser = new ChatUser();
                                newuser.acceptChallenges = false;
                                newuser.acceptTrades = true;
                                newuser.adminRole = AdminRole.None;
                                newuser.name = roinpro.name;
                                newuser.id = roinpro.id;
                                this.globalusers.Add(roinpro.name, newuser);
                            }
                            if (!this.aucusers.ContainsKey(roinpro.name) && roinpro.name != this.ownname) {  this.aucusers.Add(roinpro.name, roinpro.id); }
                            if (!this.usertoaucroom.ContainsKey(roinpro.name)) this.usertoaucroom.Add(roinpro.name, roominfo.roomName.Split('-')[1]);
                        }

                        if (rip.Length > 50 && ownroomnumber == 0) //goto next room
                        {
                            int roomnumber = Convert.ToInt32(roominfo.roomName.Split('-')[1]) + 1;
                            App.Communicator.sendRequest(new RoomExitMessage(roominfo.roomName));
                            App.Communicator.sendRequest(new RoomEnterMessage("auc-" + roomnumber));
                        }
                        else
                        {
                            if (ownroomnumber == 0)
                            {
                                // stay here, write others, that you stay here.
                                ownroomnumber = Convert.ToInt32(roominfo.roomName.Remove(0, 4));
                                int sendasks=0;
                                foreach (KeyValuePair<string, string> pair in this.aucusers)
                                {
                                    if (sendasks < 5) // only send the room-asking message to the first 10 users (it would be to spammy)
                                    {
                                        dowhisper("aucstay? " + ownroomnumber, pair.Key);
                                        sendasks++;
                                    }
                                    else { dowhisper("aucstay! " + ownroomnumber, pair.Key); }

                                }

                            }
                            else 
                            { // stayed in ownroom, but have to visit others :D
                                // so just leave this room (and join the next if multijoin doesnt work)

                                foreach (RoomInfoProfile roinpro in rip)//whisper to the users in this channel, what your channel is
                                {
                                    if (this.ownname == roinpro.name) continue;
                                    dowhisper("aucstay! " + ownroomnumber, roinpro.name);
                                }
                                if (ownroomnumber != Convert.ToInt32( roominfo.roomName.Split('-')[1])) // leave room, only when its not the own room
                                {
                                    App.Communicator.sendRequest(new RoomExitMessage(roominfo.roomName));
                                }
                                if (this.roooms.Count >= 1) { App.Communicator.sendRequest(new RoomEnterMessage(this.roooms[0])); this.roooms.RemoveAt(0); }

                            }



                        }


                    }

                   



                }
            
            }

            if ( msg is FailMessage)
            {   // delete user if he cant be whispered ( so he doesnt check out... blame on him!)
                FailMessage fm = (FailMessage)msg;
                if (this.idtesting > 0)
                {
                    
                    if (fm.op == "ProfilePageInfo") { this.idtesting--; };
                }
                if (fm.op == "Whisper" && fm.info.StartsWith("Could not find the user "))
                {
                    string name = "";
                    name = (fm.info).Split('\'')[1];
                    //Console.WriteLine("could not find: " + name);
                    if (this.usertoaucroom.ContainsKey(name)) { this.usertoaucroom.Remove(name); }
                    if (this.aucusers.ContainsKey(name)) { this.aucusers.Remove(name);  this.deleteuserfromnet(); }
                }
            }

            if (this.idtesting>0 && msg is ProfilePageInfoMessage)//doesnt needed anymore
            {
                ProfilePageInfoMessage ppim = (ProfilePageInfoMessage)msg;
                ChatUser newuser = new ChatUser();
                newuser.acceptChallenges = false;
                newuser.acceptTrades = true;
                newuser.adminRole = AdminRole.None;
                newuser.name = ppim.name;
                newuser.id = ppim.id;
                if (!this.globalusers.ContainsKey(newuser.name)) {this.globalusers.Add(newuser.name,newuser); }
                if (!this.aucusers.ContainsKey(newuser.name) && newuser.name!=this.ownname) { this.aucusers.Add(newuser.name, newuser.id); }
                this.idtesting--;
            }

            if (msg is ProfileInfoMessage) // this could be done simplier, with app.myprofile...
            {

                ProfileInfoMessage pmsg = (ProfileInfoMessage)msg;
                this.ownname = pmsg.profile.name;
                this.ownid = pmsg.profile.id;
            }

            if (msg is CardTypesMessage)
            {
                
                JsonReader jsonReader = new JsonReader();
                Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(msg.getRawText());
                Dictionary<string, object>[] d = (Dictionary<string, object>[])dictionary["cardTypes"];
                this.cardids = new int[d.GetLength(0)];
                this.cardnames = new string[d.GetLength(0)];
                this.cardImageid = new int[d.GetLength(0)];
                this.cardType = new string[d.GetLength(0)];
                this.longestcardname = 0;
                this.lowerprice = new int[d.GetLength(0)]; 
                this.upperprice = new int[d.GetLength(0)];
                for (int i = 0; i < d.GetLength(0); i++)
                {
                    cardids[i] = Convert.ToInt32(d[i]["id"]);
                    cardnames[i] = d[i]["name"].ToString().ToLower();
                    cardImageid[i] = Convert.ToInt32(d[i]["cardImage"]);
                    cardType[i] = d[i]["kind"].ToString();
                    if (cardnames[i].Split(' ').Length > longestcardname) { longestcardname = cardnames[i].Split(' ').Length; };

                }
                
                this.wtbpricelist1.Clear();
                allcardsavailable.Clear();
                for (int j = 0; j < cardnames.Length; j++)
                {
                    wtbpricelist1.Add(cardnames[j].ToLower(), "");
                    CardType type = CardTypeManager.getInstance().get(this.cardids[j]);
                    Card card = new Card(cardids[j], type, true);
                    aucitem ai = new aucitem();
                    ai.card = card;
                    ai.price = "";
                    ai.priceinint = allcardsavailable.Count;
                    ai.seller="me";
                    allcardsavailable.Add(ai);
                };
                this.allcardsavailable.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });
                //test
                //foreach (aucitem ai in allcardsavailable)
                //{ Console.WriteLine(ai.card.getName()); }
                //App.Communicator.removeListener(this);//dont need the listener anymore
                this.totalpricecheck();
            }

            return;
        }
        public void onReconnect()
        {
            return; // don't care
        }

        private void savesettings(settingcopy copy)
        {
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
            copy.strings2 = pricesearchstring;
            copy.strings3 = timesearchstring;
            copy.strings4 = pricesearchstring2;
        }

        private void setsettings(settingcopy copy)
        {
             growthbool = copy.boolean0;
             orderbool= copy.boolean1;
             energybool= copy.boolean2;
             decaybool= copy.boolean3;
             commonbool= copy.boolean4;
             uncommonbool= copy.boolean5;
             rarebool= copy.boolean6;
             threebool= copy.boolean7;
             onebool = copy.boolean8;
             ignore0 = copy.boolean9;
             takepriceformgenarator = copy.boolean10;
             wtssearchstring= copy.strings0;
             sellersearchstring= copy.strings1;
             pricesearchstring = copy.strings2;
             timesearchstring=copy.strings3 ;
             pricesearchstring2 = copy.strings4;

        }

        private void searchlessthan3(List<aucitem> list)
        {
            
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            // add the t1 cards first ( the t3 cards at least)
            foreach (aucitem card in temp)
            {
                if (available.ContainsKey(card.card.getName()))
                {
                    if (available[card.card.getName()] < 3)
                    {
                        list.Add(card);
                    };
                }
                else { list.Add(card); } // if you dont have the card, you have less than 3 :D

            }


        }

        private void searchmorethan3(List<aucitem> list)
        {
            
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            // add the t1 cards first ( the t3 cards at least)
            foreach (aucitem card in temp)
            {
                if ( available[card.card.getName()] > 3)
                {
                    list.Add(card);
                };

            }
            

        }

        private void searchmorethan0(List<aucitem> list)
        {

            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            // add the t1 cards first ( the t3 cards at least)
            foreach (aucitem card in temp)
            {
                if (available[card.card.getName()] >=1)
                {
                    list.Add(card);
                };

            }


        }

        private void musthaveprice(List<aucitem> list)
        {
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)//this.orgicardsPlayer1)
            {
                if (card.priceinint >= 1) { list.Add(card); };
            }
        }

        private void priceishigher(string price, List<aucitem> list)
        {
            // called form wtb-ah
            int priceinint = -1;
            if (price != "") priceinint = Convert.ToInt32(price);
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)//this.orgicardsPlayer1)
            {
                if (this.takepriceformgenarator)
                {
                    if (wtspricelist1[card.card.getName().ToLower()] != "")
                    {
                        //Console.WriteLine(card.card.getName() + " " + wtspricelist1[card.card.getName().ToLower()]);
                        if (card.priceinint >= Convert.ToInt32(wtspricelist1[card.card.getName().ToLower()])) { list.Add(card); };
                    }
                    else
                    {
                        if (card.priceinint >= priceinint) { list.Add(card); };
                    }
               
                }
                else
                {
                    if (card.priceinint >= priceinint) { list.Add(card); };
                }
            }

        }

        private void priceislower(string price, List<aucitem> list)
        {
            //called form wts menu (we want small prices :D )
            int priceinint =int.MaxValue;
            if (price != "") priceinint = Convert.ToInt32(price);
             
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)//this.orgicardsPlayer1)
            {

                if (this.takepriceformgenarator)
                {
                    if (wtbpricelist1[card.card.getName().ToLower()] != "")
                    {
                        if (card.priceinint <= Convert.ToInt32(wtbpricelist1[card.card.getName().ToLower()])) { list.Add(card); };
                    }
                    else
                    {
                        if (card.priceinint <= priceinint) { list.Add(card); };
                    }

                }
                else
                {
                    if (card.priceinint <= priceinint) { list.Add(card); };
                }
            }

        }

        private void containsseller(string name, List<aucitem> list)
        {
            // "contains seller not" should its name be :D
            string[] sellers = new string[]{name};
            if (name.Contains(" ")) sellers = name.ToLower().Split(' ');
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)//this.orgicardsPlayer1)
            {
                //if (card.seller.ToLower().Contains(name.ToLower())) { list.Add(card); };
                if (!sellers.Any(card.seller.ToLower().Equals)) { list.Add(card); };
            }

        }

        private void containsname(string name, List<aucitem> list)
        {
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)//this.orgicardsPlayer1)
            {
                if (card.card.getName().ToLower().Contains(name.ToLower())) { list.Add(card); };

            }

        }

        private void searchforownenergy(string[] rare, List<aucitem> list)
        {
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)
            {
                if (rare.Contains(card.card.getResourceString().ToLower())) { list.Add(card); };

            }

        }

        private void searchforownrarity(int[] rare, List<aucitem> list)
        {
            List<aucitem> temp = new List<aucitem>(list);
            list.Clear();
            foreach (aucitem card in temp)
            {
                if (rare.Contains(card.card.getRarity())) { list.Add(card); };

            }

        }


        public Auction()
        {
            // match until first instance of ':' (finds the username)
            userRegex = new Regex(@"[^:]*"
                /*, RegexOptions.Compiled*/); // the version of Mono used by Scrolls version of Unity does not support compiled regexes
            // from http://daringfireball.net/2010/07/improved_regex_for_matching_urls
            // I had to remove a " in there to make it work, but it should match well enough anyway
            linkFinder = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'.,<>?«»“”‘’]))"
                /*, RegexOptions.Compiled*/);
            cardlinkfinder = new Regex(@"\[[a-zA-Z]+[a-zA-Z_\t]*[a-zA-z]+\]");//search for "[blub_blub_blub]"

            priceregex = new Regex(@".*[^x0-9]+[0-9]{2,9}[g]?[^x0-9]+.*");
            priceregexpriceonname = new Regex(@"[^x0-9]{2,}[0-9]{2,9}[g]?[^x0-9]+.*");
            numberregx = new Regex(@"[0-9]{2,9}");

            statsBGField = typeof(CardView).GetField("statsBG", BindingFlags.Instance | BindingFlags.NonPublic);
            icoBGField = typeof(CardView).GetField("icoBG", BindingFlags.Instance | BindingFlags.NonPublic);
            icoField = typeof(CardView).GetField("ico", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumAttackPowerField = typeof(CardView).GetField("gosNumAttackPower", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumCountdownField = typeof(CardView).GetField("gosNumCountdown", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumCostField = typeof(CardView).GetField("gosNumCost", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumHitPointsField = typeof(CardView).GetField("gosNumHitPoints", BindingFlags.Instance | BindingFlags.NonPublic);
            textsArrField = typeof(CardView).GetField("textsArr", BindingFlags.Instance | BindingFlags.NonPublic);
            cardImageField = typeof(CardView).GetField("cardImage", BindingFlags.Instance | BindingFlags.NonPublic);
            gosactiveAbilityField = typeof(CardView).GetField("gosActiveAbilities", BindingFlags.Instance | BindingFlags.NonPublic);

            hideInformationinfo = typeof(Store).GetMethod("hideInformation", BindingFlags.Instance | BindingFlags.NonPublic);
            showBuyinfo = typeof(Store).GetField("showBuy", BindingFlags.Instance | BindingFlags.NonPublic);
            showSellinfo = typeof(Store).GetField("showSell", BindingFlags.Instance | BindingFlags.NonPublic);

            drawsubmenu = typeof(Store).GetMethod("drawSubMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            CloseUserMenuinfo = typeof(ChatUI).GetMethod("CloseUserMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            createUserMenuinfo = typeof(ChatUI).GetMethod("CreateUserMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            challengeUserMethod = typeof(ChatUI).GetMethod("ChallengeUser", BindingFlags.Instance | BindingFlags.NonPublic);
            tradeUserMethod = typeof(ChatUI).GetMethod("TradeUser", BindingFlags.Instance | BindingFlags.NonPublic);
            profileUserMethod = typeof(ChatUI).GetMethod("ProfileUser", BindingFlags.Instance | BindingFlags.NonPublic);
            getchatlogMethod = typeof(ChatRooms).GetMethod("GetChatLog", BindingFlags.Instance | BindingFlags.NonPublic);

            userContextMenuField = typeof(ChatUI).GetField("userContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            chatRoomsinfo = typeof(ChatUI).GetField("chatRooms", BindingFlags.Instance | BindingFlags.NonPublic);
            timeStampStyleinfo = typeof(ChatUI).GetField("timeStampStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            chatLogStyleinfo = typeof(ChatUI).GetField("chatMsgStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            chatlogAreaInnerinfo = typeof(ChatUI).GetField("chatlogAreaInner", BindingFlags.Instance | BindingFlags.NonPublic);
            chatScrollinfo = typeof(ChatUI).GetField("chatScroll", BindingFlags.Instance | BindingFlags.NonPublic);
            allowSendingChallengesinfo = typeof(ChatUI).GetField("allowSendingChallenges", BindingFlags.Instance | BindingFlags.NonPublic);
            userContextMenuinfo = typeof(ChatUI).GetField("userContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            
            this.GUIObject = new GameObject();
            this.GUIObject.transform.parent = Camera.main.transform;
            this.GUIObject.transform.localPosition = new Vector3(0f, 0f, Camera.main.transform.position.y - 0.3f);

            this.cardListPopupSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopup");
            this.cardListPopupGradientSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopupGradient");
            this.cardListPopupBigLabelSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel");
            this.cardListPopupLeftButtonSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopupLeftButton");

            buymen = typeof(Store).GetField("buyMenuObj", BindingFlags.Instance | BindingFlags.NonPublic);
            sellmen = typeof(Store).GetField("sellMenuObj", BindingFlags.Instance | BindingFlags.NonPublic);

            this.orderbool = true;
            this.growthbool = true;
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
            savesettings(this.ahwtssettings);
            savesettings(this.ahwtbsettings);
            savesettings(this.genwtssettings);
            savesettings(this.genwtbsettings);

            try
            {
                App.Communicator.addListener(this);
            }
            catch { }

        }

        public static string GetName()
        {
            return "auc";
        }

        public static int GetVersion()
        {
            return 4;
        }

        public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
        {
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["ChatRooms"].Methods.GetMethod("SetRoomInfo", new Type[] {typeof(RoomInfoMessage)}),
                    scrollsTypes["ChatUI"].Methods.GetMethod("Initiate")[0],
                    scrollsTypes["ChatUI"].Methods.GetMethod("Show", new Type[]{typeof(bool)}),
                    scrollsTypes["Store"].Methods.GetMethod("OnGUI")[0],
                    scrollsTypes["ChatRooms"].Methods.GetMethod("ChatMessage", new Type[]{typeof(RoomChatMessageMessage)}),
                   scrollsTypes["ArenaChat"].Methods.GetMethod("handleMessage", new Type[]{typeof(Message)}),
                   //scrollsTypes["Lobby"].Methods.GetMethod("handleMessage", new Type[]{typeof(Message)}),
                   scrollsTypes["BattleMode"].Methods.GetMethod("_handleMessage", new Type[]{typeof(Message)}),
                   scrollsTypes["Store"].Methods.GetMethod("Start")[0],
                    scrollsTypes["Store"].Methods.GetMethod("showSellMenu")[0],
                     scrollsTypes["Store"].Methods.GetMethod("showBuyMenu")[0],
                     scrollsTypes["Store"].Methods.GetMethod("handleMessage", new Type[]{typeof(Message)}),

                    // only for testing:
                    //scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)}),  
                };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
        }


        public override bool WantsToReplace(InvocationInfo info)
        {
            if (info.target is Store && info.targetMethod.Equals("OnGUI"))
            {
                if (this.inauchouse || this.generator) return true;
            }
            
            if (info.target is BattleMode && info.targetMethod.Equals("_handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
                    if (this.contonetwork)
                    {

                        if ((wmsg.text).StartsWith("aucupdate") || (wmsg.text).StartsWith("aucto1please") || (wmsg.text).StartsWith("aucstay? ") || (wmsg.text).StartsWith("aucstay! ") || (wmsg.text).StartsWith("aucrooms ") || (wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucs ") || (wmsg.text).StartsWith("aucb ") || (wmsg.text).StartsWith("needaucid") || (wmsg.text).StartsWith("aucid ")) return true;
                    }
                    else
                    {
                        if ((wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucto1please")) return true;
                    }
                }
            }
            
            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
                    if (this.contonetwork)
                    {

                        if ((wmsg.text).StartsWith("aucupdate") || (wmsg.text).StartsWith("aucto1please") || (wmsg.text).StartsWith("aucstay? ") || (wmsg.text).StartsWith("aucstay! ") || (wmsg.text).StartsWith("aucrooms ") || (wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucs ") || (wmsg.text).StartsWith("aucb ") || (wmsg.text).StartsWith("needaucid") || (wmsg.text).StartsWith("aucid ")) return true;
                    }
                    else
                    {
                        if ((wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucto1please")) return true;
                    }
                }
                if (msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage rem = (RoomChatMessageMessage)msg;
                    if (this.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }

                if (msg is RoomEnterMessage)
                {   
                    RoomEnterMessage rem = (RoomEnterMessage) msg;
                    if (this.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }

                if (msg is RoomInfoMessage)
                {
                    RoomInfoMessage rem = (RoomInfoMessage)msg;
                    if (this.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }


            }
            /*if (info.target is Lobby && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];

                if (msg is RoomEnterMessage)
                {
                    RoomEnterMessage rem = (RoomEnterMessage)msg;
                    if (this.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }

                if (msg is RoomInfoMessage)
                {
                    RoomInfoMessage rem = (RoomInfoMessage)msg;
                    if (this.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }


            }*/
            return false;
        }


        public override void ReplaceMethod(InvocationInfo info, out object returnValue)
        {

            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
                    string text = wmsg.text;

                    if (text.StartsWith("aucto1please")&& this.contonetwork)
                    {
                        App.Communicator.sendRequest(new RoomExitMessage("auc-" + ownroomnumber));
                        this.ownroomnumber = 0;
                        App.Communicator.sendRequest(new RoomEnterMessage("auc-1"));
                    
                    }

                    if (text.StartsWith("aucstay? ") && this.contonetwork)
                    {   // user founded a room, but dont know if this is all
                        string stayroom=text.Split(' ')[1];
                        if(!this.usertoaucroom.ContainsKey(wmsg.from)) this.usertoaucroom.Add(wmsg.from, stayroom);//save his aucroom

                        string respondstring = "";
                        // whispering user already visited all room till his room
                        List<string> allreadyadded = new List<string>();
                        for (int i = 0; i < Convert.ToInt32(stayroom); i++)
                        {
                            allreadyadded.Add(i.ToString());
                        }

                        foreach (KeyValuePair<string, string> pair in this.usertoaucroom)
                        {
                            if (!allreadyadded.Contains(pair.Value))
                            {
                                respondstring = respondstring + " " + pair.Value;
                                allreadyadded.Add(pair.Value);
                            }
                        
                        
                        }

                        if (respondstring != "") 
                        { 
                            respondstring = "aucrooms" + respondstring; 
                            WhisperMessage sendrooms = new WhisperMessage(wmsg.from, respondstring); 
                            App.Communicator.sendRequest(sendrooms); 
                        }
                        //send your offers
                        if (this.genwtssettings.strings2 != "")
                        {
                            dowhisper(this.genwtssettings.strings2, wmsg.from);
                        }

                        if (this.genwtbsettings.strings2 != "")
                        {
                            dowhisper(this.genwtbsettings.strings2, wmsg.from);
                        }

                    
                    }

                    if (text.StartsWith("aucstay! "))
                    {   // user founded a room, and he dont want to get the room-list
                        if (!this.usertoaucroom.ContainsKey(wmsg.from)) { this.usertoaucroom.Add(wmsg.from, text.Split(' ')[1]); }//save his aucroom
                        else { this.usertoaucroom.Remove(wmsg.from); this.usertoaucroom.Add(wmsg.from, text.Split(' ')[1]); }
                        //send your offers
                        if (this.genwtssettings.strings2 != "")
                        {
                            dowhisper(this.genwtssettings.strings2, wmsg.from);
                        }

                        if (this.genwtbsettings.strings2 != "")
                        {
                            dowhisper(this.genwtbsettings.strings2, wmsg.from);
                        }
                    }

                    if (text.StartsWith("aucrooms ") && !rooomsearched && this.contonetwork)
                    {
                         string[] rms = (text.Remove(0, 9)).Split(' ');
                         
                         this.roooms.Clear();
                         foreach (string str in rms)
                         { roooms.Add("auc-" + str); Console.WriteLine("auc-" + str); }
                         //App.Communicator.sendRequest(new RoomEnterMultiMessage(roooms));//doesnt seems to work prooperly, scrolls (not me) is producing an error when i receive an chatmassage form this rooms
                         App.Communicator.sendRequest(new RoomEnterMessage(roooms[0]));
                         roooms.RemoveAt(0);
                         this.rooomsearched = true;
                    }

                    if (text.StartsWith("aucstop"))
                    {
                        if (this.usertoaucroom.ContainsKey(wmsg.from)) { this.usertoaucroom.Remove(wmsg.from); }
                        if (this.aucusers.ContainsKey(wmsg.from)) { this.aucusers.Remove(wmsg.from);  this.deleteuserfromnet(); }
                    }

                    if (text.StartsWith("aucs ") || text.StartsWith("aucb "))
                    {
                        getaucitemsformmsg(text, wmsg.from, wmsg.GetChatroomName());
                        //need playerid (wispering doesnt send it)
                        if (!this.globalusers.ContainsKey(wmsg.from)) { WhisperMessage needid = new WhisperMessage(wmsg.from, "needaucid"); App.Communicator.sendRequest(needid); }
                    }

                    if (text.StartsWith("aucupdate"))  
                    {
                        foreach(KeyValuePair<string,string> pair in this.aucusers)
                        {
                            string from=pair.Key;
                            if(this.genwtssettings.strings2 != "")
                            {
                                dowhisper(this.genwtssettings.strings2, from); 
                            }

                            if ( this.genwtbsettings.strings2 != "")
                            { 
                                dowhisper(this.genwtbsettings.strings2, from); 
                            }
                        }
                    }
                    

                    
                    //dont needed anymore left in only to be shure :D
                    if (text.StartsWith("needaucid"))
                    {
                        WhisperMessage sendid = new WhisperMessage(wmsg.from, "aucid " +this.ownid); App.Communicator.sendRequest(sendid); 
                    }
                     //dont needed anymore
                    if (text.StartsWith("aucid "))
                    {
                        if (!this.globalusers.ContainsKey(wmsg.from))
                        {
                            string id = text.Split(new string[] { "aucid " }, StringSplitOptions.None)[1];
                            //test aucid:
                            this.idtesting++;
                            ProfilePageInfoMessage ppim = new ProfilePageInfoMessage(id);
                            App.Communicator.sendRequest(ppim);

                        }
                        else
                        {
                            if (contonetwork)
                            {
                                if (!this.aucusers.ContainsKey(wmsg.from) && wmsg.from!=this.ownname)
                                { this.aucusers.Add(wmsg.from, this.globalusers[wmsg.from].id); }
                            }
                        }
                        
                    }


                }
            }

            returnValue = null;
            

        }

        public override void BeforeInvoke(InvocationInfo info)
        {

            return;
        }


        

        private void createcard(string cardname)
        {
            string clink = cardname.ToLower();
            int arrindex = Array.FindIndex(this.cardnames, element => element.Equals(clink));
            if (arrindex >= 0)
            {

                CardType type = CardTypeManager.getInstance().get(this.cardids[arrindex]);
                Card card = new Card(cardids[arrindex], type, false);
                UnityEngine.Object.Destroy(this.cardRule);
                cardRule = PrimitiveFactory.createPlane();
                cardRule.name = "CardRule";
                CardView cardView = cardRule.AddComponent<CardView>();
                cardView.init(this, card, -1);
                cardView.applyHighResTexture();
                cardView.setLayer(8);
                Vector3 vccopy = Camera.main.transform.localPosition;
                Camera.main.transform.localPosition = new Vector3(0f, 1f, -10f);
                cardRule.transform.localPosition = Camera.main.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.6f, (float)Screen.height * 0.6f, 0.9f)); ;

                cardRule.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
                cardRule.transform.localScale = new Vector3(9.3f, 0.1f, 15.7f);// CardView.CardLocalScale(100f);
                cardtext = cardRule.renderer.material.mainTexture;
                Vector3 ttvec1 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.min);
                Vector3 ttvec2 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.max);
                Rect ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);

                scalefactor = (float)(Screen.height / 1.9) / ttrec.height;
                cardRule.transform.localScale = new Vector3(cardRule.transform.localScale.x * scalefactor, cardRule.transform.localScale.y, cardRule.transform.localScale.z * scalefactor);
                ttvec1 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.min);
                ttvec2 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.max);
                ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
                cardrect = ttrec;
                gettextures(cardView);
                this.mytext = true;
                Camera.main.transform.localPosition = vccopy;
                //Console.WriteLine("CARD: " + clink);

            }
        
        
        }

        private string pricetestfirst(string price)
        {
            string result = "";
            //Console.WriteLine("PREIS: " + price);
            Match match = this.priceregexpriceonname.Match(" " + price + " ");
            if (match.Success) { result = match.Value; }

            return result;
        }

        private string pricetest(string price)
        {
            string result = "";
            //Console.WriteLine("PREIS: " + price);
            Match match = priceregex.Match(" "+price+" ");
            if (match.Success) { result = match.Value; }
            
            return result;
        }

        private void sortauciteminlist(aucitem ai, List<aucitem> list)
        {
            if (this.sortmode == 0)
            {
                list.Insert(0, ai);
            }
            if (this.sortmode == 1)
            {
                var index = list.BinarySearch(ai, new aucitemnamecomparer());
                if (index < 0) index = ~index;
                list.Insert(index, ai);
            }
            if (this.sortmode == 2)
            {
                var index = list.BinarySearch(ai, new aucitemgoldcomparer());
                if (index < 0) index = ~index;
                list.Insert(index, ai);
            }
            if (this.sortmode == 2)
            {
                var index = list.BinarySearch(ai, new aucitemsellercomparer());
                if (index < 0) index = ~index;
                list.Insert(index, ai);
            }
        
        }

        private void sortlist(List<aucitem> list)
        {
            //sortmode==0 = sort by date so dont sort wtsfulltimed
            if (this.sortmode == 1)
            {
                list.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });
            }
            if (this.sortmode == 2)
            {
                list.Sort(delegate(aucitem p1, aucitem p2) { return (p1.priceinint).CompareTo(p2.priceinint); });
            }
            if (this.sortmode == 3)
            {
                list.Sort(delegate(aucitem p1, aucitem p2) { return (p1.seller).CompareTo(p2.seller); });
            }
        }

        private void addcardstolist()
        {
            //Console.WriteLine("##addcars");
            if (this.generator||!this.inauchouse) { this.newwtsmsgs = true; this.newwtbmsgs = true; }
            else { if (this.wtsmenue) { this.newwtbmsgs = true; } else { this.newwtsmsgs = true; } }
            if (addingwtscards.Count() > 0)
            {
                addingwtscards.Reverse();
                string seller = addingwtscards[0].seller;
                this.wtslistfulltimed.RemoveAll(element => element.seller == seller);
                this.wtslistfull.RemoveAll(element => element.seller == seller);
                this.wtslist.RemoveAll(element => element.seller == seller);

            }

            if (addingwtbcards.Count() > 0)
            {
                addingwtbcards.Reverse();
                string seller = addingwtbcards[0].seller;
                this.wtblistfulltimed.RemoveAll(element => element.seller == seller);
                this.wtblistfull.RemoveAll(element => element.seller == seller);
                this.wtblist.RemoveAll(element => element.seller == seller);
            }
            foreach (aucitem ai in this.addingwtscards)
            {
                this.wtslistfulltimed.Insert(0, ai);
            }
            foreach (aucitem ai in this.addingwtbcards)
            {
                this.wtblistfulltimed.Insert(0, ai);
            }

            if (this.wtsmenue)
            {
                //add cards to wtslistfull
                
                

                foreach (aucitem ai in this.addingwtscards) 
                {
                    sortauciteminlist(ai, this.wtslistfull);
                }

                // add cards to wtslist but filter these first
                List<aucitem> tempfull = new List<aucitem>();
                tempfull.AddRange(this.addingwtscards);
                fullupdatelist(this.addingwtscards, tempfull);
                // add them to wtslist
                foreach (aucitem ai in this.addingwtscards)
                {
                    sortauciteminlist(ai, this.wtslist);
                }

            }
            else 
            {
                //add cards to wtblistfull

                

                foreach (aucitem ai in this.addingwtbcards)
                {
                    sortauciteminlist(ai, this.wtblistfull);
                }

                // add cards to wtslist but filter these first
                List<aucitem> tempfull = new List<aucitem>();
                tempfull.AddRange(this.addingwtbcards);
                fullupdatelist(this.addingwtbcards, tempfull);
                // add them to wtslist
                foreach (aucitem ai in this.addingwtbcards)
                {
                    sortauciteminlist(ai, this.wtblist);
                }
            
            }
        }

        private void additemtolist(Card c, string from, int gold, bool wts,string wholemsg)
        {
            aucitem ai = new aucitem();
            ai.card = c;
            ai.seller = from;
            ai.priceinint = gold;
            ai.price = gold.ToString();
            ai.time = DateTime.Now.ToString("hh:mm:ss tt");//DateTime.Now.ToShortTimeString();
            ai.dtime = DateTime.Now;
            ai.whole = wholemsg;
            if (gold == 0) ai.price = "?";
            if (wts) 
            {
                this.addingwtscards.Add(ai);
                
                
                
            }
            else 
            { 
                
                this.addingwtbcards.Add(ai);
            }
        
        }

        private void dowhisper(string msg, string to)
        {
            WhisperMessage wmsg=new WhisperMessage(to, msg);
            if (this.inbattle) App.Communicator.sendBattleRequest(wmsg);
            else
                App.Communicator.sendRequest(wmsg);
        }

        private void respondtocommand(string msg, string from)
        {
            if (from == this.ownname) return;

            string commando = msg.Split(' ')[1];
            if (commando != "" && this.contonetwork)
            {
                if (commando == "sendwts" && this.genwtssettings.strings2 != "") { dowhisper(this.genwtssettings.strings2, from); }
                if (commando == "sendwtb" && this.genwtbsettings.strings2 != "") { dowhisper(this.genwtbsettings.strings2, from); }
                if (commando == "aucstart") 
                {
                    if (!this.aucusers.ContainsKey(from)&& from!=this.ownname) { this.aucusers.Add(from, this.globalusers[from].id); }
                        WhisperMessage sendid = new WhisperMessage(from, "aucid " + this.ownid); App.Communicator.sendRequest(sendid);
                }

            
            }
        
        }

        private void getaucitemsformshortmsg(string msg, string from, string room)
        {
            
            
            bool aucbtoo = false;
            bool wts=true;
            string secmsg = "";
            if (msg.StartsWith("aucs ")) 
            {
                  wts=true;
                msg = msg.Remove(0, 5); 
                if (msg.Contains("aucb "))
                {
                    aucbtoo = true;
                    secmsg = (msg.Split(new string[] { "aucb " }, StringSplitOptions.None))[1]; 
                    msg = (msg.Split(new string[]{"aucb "},StringSplitOptions.None))[0]; 
                }
            }
            if (msg.StartsWith("aucb "))
            {
                  wts=false;
                msg = msg.Remove(0, 5);
                

                if (msg.Contains("aucs "))
                {    wts=true;
                    aucbtoo = true;
                    secmsg = (msg.Split(new string[] { "aucs " }, StringSplitOptions.None))[0];
                    msg = (msg.Split(new string[] { "aucs " }, StringSplitOptions.None))[1];
                }

            }
            //Console.WriteLine(msg + "##" + secmsg);

            string[] words = msg.Split(';');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "" || words[i] == " ") break;
                string price;
                string ids;
                if (words[i].Contains(' '))
                {
                    price = words[i].Split(' ')[1];
                    if (price == "") { price = "0"; }
                    ids = words[i].Split(' ')[0];
                }

                else
                {
                    price = "0";
                    ids = words[i];
                }
                    if (ids.Contains(","))
                    {
                    string[] ideen = ids.Split(',');
                        foreach(string idd in ideen)
                        {
                        int id = Convert.ToInt32(idd);
                        CardType type = CardTypeManager.getInstance().get(id);
                        Card card = new Card(id, type, true);
                        additemtolist(card, from, Convert.ToInt32(price), wts,"");
                        }
                    
                    }
                    else
                    {
                        int id = Convert.ToInt32(ids);
                        CardType type = CardTypeManager.getInstance().get(id);
                        Card card = new Card(id, type, true);
                        additemtolist(card, from, Convert.ToInt32(price), wts,"");
                    
                    }


                
            }

            if (aucbtoo)
            {  wts=false;
                words = secmsg.Split(';');
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "" || words[i] == " ") break;
                    string price = words[i].Split(' ')[1];
                    if (price == "") { price = "0"; }
                    string ids = words[i].Split(' ')[0];
                    if (ids.Contains(","))
                    {
                        string[] ideen = ids.Split(',');
                        foreach (string idd in ideen)
                        {
                            int id = Convert.ToInt32(idd);
                            CardType type = CardTypeManager.getInstance().get(id);
                            Card card = new Card(id, type, true);
                            additemtolist(card, from, Convert.ToInt32(price), wts,"");
                        }

                    }
                    else
                    {
                        int id = Convert.ToInt32(ids);
                        CardType type = CardTypeManager.getInstance().get(id);
                        Card card = new Card(id, type, true);
                        additemtolist(card, from, Convert.ToInt32(price), wts,"");

                    }

                }
            
            }

            addcardstolist();
        }

        private void getaucitemsformmsg(string msgg, string from, string room)
        {
            string msg = Regex.Replace(msgg, @"(<color=#[A-Za-z0-9]{0,6}>)|(</color>)", String.Empty);
            this.addingwtbcards.Clear();
            this.addingwtscards.Clear();
            // todo: delete old msgs from author
            if (msg.StartsWith("aucs ") || msg.StartsWith("aucb ")) {getaucitemsformshortmsg(msg,from, room); return; }
            //if (msg.StartsWith("aucc ")) { respondtocommand(msg,from); return; }
            bool wts = true; ;
            //string[] words=msg.Split(' ');

            char[] delimiters = new char[] { '\r', '\n', ' ', ',', ';' };
            string[] words = msg.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            //words = Regex.Split(msg, @"");

            if (!msg.ToLower().Contains("wts") && !msg.ToLower().Contains("wtb") && !msg.ToLower().Contains("sell") && !msg.ToLower().Contains("buy")) return;

            for (int i = 0; i < words.Length; i++)
            {
                Card c; int price=0;
                string word = words[i].ToLower();
                // save in wts or wtb?
                if (word.Contains("wts") || word.Contains("sell")) { wts = true; }
                if (word.Contains("wtb") || word.Contains("buy")) { wts = false; }
                int arrindex = Array.FindIndex(this.cardnames, element => word.Contains(element.Split(' ')[0])); // changed words[i] and element!
                int iadder = 0;
                if (arrindex >= 0) // wort in cardlist enthalten
                {
                    //Console.WriteLine(word + " " + arrindex);
                    string[] possiblecards = Array.FindAll(this.cardnames, element => word.Contains(element.Split(' ')[0]));
                    bool findcard = false;
                    string foundedcard = "";
                    string textplace = "";
                    
                    for (int ii = 0; ii < possiblecards.Length; ii++)
                    {
                        string match = possiblecards[ii].ToLower();
                        int posleng = Math.Min(match.Split(' ').Length,words.Length-i);
                        string searchob = string.Join(" ", words, i, posleng).ToLower();
                        if (searchob.Contains(match)) { findcard = true; foundedcard = match; iadder = posleng; textplace = searchob; break; };


                    }
                    //

                    i = i + iadder;

                    if (findcard)
                    {
                        CardType type = CardTypeManager.getInstance().get(cardnametoid(foundedcard.ToLower()));
                        c = new Card(cardids[arrindex], type, true);
                        //Console.WriteLine("found " + foundedcard + " in " + textplace);
                        string tmpgold = pricetestfirst((textplace.Split(' '))[(textplace.Split(' ')).Length - 1]);
                        if (!(tmpgold == "") ) // && iadder >1
                        {   // case: cardnamegold
                            //Console.WriteLine("found " + this.numberregx.Match(tmpgold).Value);
                            price = Convert.ToInt32(this.numberregx.Match(tmpgold).Value);
                        }
                        else if (i< words.Length)
                        {
                            int j = i;
                            tmpgold = pricetest(words[j]);
                            while (tmpgold == "")
                            {
                                if (j + 1 < words.Length)
                                { j++;
                                tmpgold = pricetest(words[j]);
                                }
                                else { tmpgold = "fail"; }
                            
                            }

                            if (!(tmpgold == "fail"))
                            { // cardname gold
                                //Console.WriteLine("found gold " + this.numberregx.Match(tmpgold).Value);
                                price = Convert.ToInt32(this.numberregx.Match(tmpgold).Value);
                            }
                        }
                        additemtolist(c, from, price, wts,msgg);
                        i--;


                    }//if (find) ende
                    
                    
                    
                }



                
            }

            addcardstolist();

        }

        private void generatewtxmsg(List<aucitem> liste)
        {
            string msg = "";
            string shortmsg = "";
            List<aucitem> postlist = new List<aucitem>();
            for (int i = 0; i < liste.Count;i++ )
            {
                if (this.wtsmenue)
                {
                    if (this.wtspricelist1[liste[i].card.getName().ToLower()] != "")
                    {
                        aucitem ai = liste[i];
                        ai.price = this.wtspricelist1[liste[i].card.getName().ToLower()];
                        ai.priceinint = Convert.ToInt32( ai.price);
                        postlist.Add(ai);
                        //msg = msg + liste[i].card.getName() + " " + this.wtspricelist1[liste[i].card.getName().ToLower()] + ";";
                        //shortmsg = shortmsg + liste[i].card.getType() + " " + this.wtspricelist1[liste[i].card.getName().ToLower()] + ";"; 
                    }
                } else
                {
                    if (this.wtbpricelist1[liste[i].card.getName().ToLower()] != "")
                    {
                        aucitem ai = liste[i];
                        ai.price = this.wtbpricelist1[liste[i].card.getName().ToLower()];
                        ai.priceinint = Convert.ToInt32(ai.price);
                        postlist.Add(ai);
                        //msg = msg + liste[i].card.getName() + " " + this.wtbpricelist1[liste[i].card.getName().ToLower()] + ";";
                        //shortmsg = shortmsg + liste[i].card.getType() + " " + this.wtbpricelist1[liste[i].card.getName().ToLower()] + ";"; 
                    }
                }
            }

            postlist.Sort(delegate(aucitem p1, aucitem p2) { return (p1.priceinint).CompareTo(p2.priceinint); });
            postlist.Reverse();
            Dictionary<string, string> shortlist = new Dictionary<string, string>();

            for (int i = 0; i < postlist.Count;i++ )
                {
                    aucitem ai = postlist[i];
                    if (i < postlist.Count - 1 && postlist[i + 1].priceinint == ai.priceinint)
                    {
                        msg = msg + ai.card.getName() + ", ";
                        shortmsg = shortmsg + ai.card.getType() + ",";
                    }
                    else
                    {
                        if (ai.price == "0")
                        {
                            msg = msg + ai.card.getName() + ";";
                            shortmsg = shortmsg + ai.card.getType() +";";
                        }
                        else
                        {

                            msg = msg + ai.card.getName() + " " + ai.price + "g, ";
                            shortmsg = shortmsg + ai.card.getType() + " " + ai.price + ";";
                        }
                        
                    }
                }

            if (msg!="")
            {
                if (this.wtsmenue) { msg = "wts " + msg; shortmsg = "aucs " + shortmsg; } else { msg = "wtb " + msg; shortmsg = "aucb " + shortmsg; }
            }
            msg = msg.Remove(msg.Length - 2);
            shortmsg = shortmsg.Remove(shortmsg.Length - 1);
            if (this.wtsmenue) { this.generatedwtsmessage = msg; this.shortgeneratedwtsmessage = shortmsg; } else { this.generatedwtbmessage = msg; this.shortgeneratedwtbmessage = shortmsg; }
            //Console.WriteLine(msg);
            //Console.WriteLine(shortmsg);
            this.sellersearchstring = msg;
            this.pricesearchstring = shortmsg;
        
        }


        private void fullupdatelist(List<aucitem> list, List<aucitem> fulllist)
        {
            list.Clear();
            list.AddRange(fulllist);
            string[] res = { "", "", "", "" };
            if (decaybool) { res[0] = "decay"; };
            if (energybool) { res[1] = "energy"; };
            if (growthbool) { res[2] = "growth"; };
            if (orderbool) { res[3] = "order"; };
            int[] rare = { -1, -1, -1 };
            if (rarebool) { rare[2] = 2; };
            if (uncommonbool) { rare[1] = 1; };
            if (commonbool) { rare[0] = 0; };
            if (this.threebool)
            {
                if (this.inauchouse)
                {
                    if (this.wtsmenue)
                    {
                        this.searchlessthan3(list);
                    }
                    else
                    {
                        this.searchmorethan3(list);
                    }
                }
                if (this.generator)
                {
                    if (this.wtsmenue)
                    {
                        this.searchmorethan3(list);
                        
                    }
                    else
                    {
                        this.searchlessthan3(list);  
                    }
                }

            }
            if (this.onebool)
            {
                if (this.inauchouse && !this.wtsmenue)
                {
                    this.searchmorethan0(list);
                }

            }
            //this.onlytradeableself();
            if (this.wtssearchstring != "")
            {
                this.containsname(this.wtssearchstring, list);
            }
            if (this.inauchouse)
            {
                if (this.sellersearchstring != "")
                {
                    this.containsseller(this.sellersearchstring, list);
                }
            }

            if (this.inauchouse)
            {
                if (this.pricesearchstring != "" || this.takepriceformgenarator)
                {
                    this.priceishigher(this.pricesearchstring, list);
                }
                if (this.pricesearchstring2 != "" || this.takepriceformgenarator)
                {
                    this.priceislower(this.pricesearchstring2, list);
                    
                }

            }
                
            

            if (this.ignore0) { this.musthaveprice(list); }

            this.searchforownenergy(res, list);
            this.searchforownrarity(rare, list);
        
        }

        private void deleteuserfromnet()
        {
            // user is deleted, check if there are too few in room 1.
            if (ownroomnumber == 1) return;

            int usersin1 = 0;
            foreach (KeyValuePair<string, string> pair in this.usertoaucroom)
            {
                if (pair.Value == "1") usersin1++;
            }
            if (usersin1 < 10) 
            {
                App.Communicator.sendRequest(new RoomExitMessage("auc-" + ownroomnumber));
                this.ownroomnumber = 0;
                App.Communicator.sendRequest(new RoomEnterMessage("auc-1"));
            }

        
        }

        private void disconfromaucnet()
        {
            this.rooomsearched = false;
            this.contonetwork = false;

            foreach (KeyValuePair<string, string> pair in this.aucusers)
            {

                dowhisper("aucstop", pair.Key);

            }
            if (this.ownroomnumber == 1)
            {// say user with biggest roomnumber he should come to 1.
                int biggestroomnumber = 0;
                string name = "";
                foreach (KeyValuePair<string, string> pair in this.usertoaucroom)
                {
                    if (biggestroomnumber < Convert.ToInt32(pair.Value))
                    {
                        biggestroomnumber = Convert.ToInt32(pair.Value);
                        name = pair.Key;
                    }

                }

                if (biggestroomnumber > 1) { dowhisper("aucto1please",name);};

            };
            this.aucusers.Clear();
            this.usertoaucroom.Clear();
            this.ownroomnumber = 0;
            this.rooomsearched = false;
            this.contonetwork = false;

        }

        private void drawAH()
        {
            // have to draw textfield in front of scrollbar or otherwise you lose focus in textfield (lol)
            
            if (this.inauchouse)
            {

                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(filtermenurect, string.Empty);
                // wts filter menue
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(sbarlabelrect, "Scroll:");
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(this.sbrect, string.Empty);
                string selfcopy = wtssearchstring;
                this.wtssearchstring = GUI.TextField(this.sbrect, this.wtssearchstring, chatLogStyle);


                GUI.contentColor = Color.white;
                GUI.color = Color.white;
                if (!growthbool) { GUI.color = dblack; }
                bool growthclick = GUI.Button(sbgrect, growthres);
                GUI.color = Color.white;
                if (!orderbool) { GUI.color = dblack; }
                bool orderclick = GUI.Button(sborect, orderres);
                GUI.color = Color.white;
                if (!energybool) { GUI.color = dblack; }
                bool energyclick = GUI.Button(sberect, energyres);
                GUI.color = Color.white;
                if (!decaybool) { GUI.color = dblack; }
                bool decayclick = GUI.Button(sbdrect, decayres);


                GUI.contentColor = Color.gray;
                GUI.color = Color.white;
                if (!commonbool) { GUI.color = dblack; }
                bool commonclick = GUI.Button(sbcommonrect, "C");
                GUI.color = Color.white;
                if (!uncommonbool) { GUI.color = dblack; }
                GUI.contentColor = Color.white;
                bool uncommonclick = GUI.Button(sbuncommonrect, "U");
                GUI.color = Color.white;
                if (!rarebool) { GUI.color = dblack; }
                GUI.contentColor = Color.yellow;
                bool rareclick = GUI.Button(sbrarerect, "R");
                GUI.color = Color.white;
                if (!threebool) { GUI.color = dblack; }
                //if (!p1mt3bool) { GUI.color = dblack; }
                bool mt3click;
                bool mt0click = false;
                if (this.wtsmenue)
                {
                    mt3click = GUI.Button(sbthreerect, "<3"); // >3 bei wtsmenue=false
                }
                else { mt3click = GUI.Button(sbthreerect, ">3"); }
                GUI.color = Color.white;
                if (!onebool) { GUI.color = dblack; }
                if (!this.wtsmenue) { mt0click = GUI.Button(sbonerect, ">0"); };
                GUI.color = Color.white;
                GUI.contentColor = Color.white;

                // draw seller filter
                GUI.skin = this.cardListPopupBigLabelSkin;
                if (this.wtsmenue)
                {
                    GUI.Label(sbsellerlabelrect, "ignore Seller:");
                }
                else { GUI.Label(sbsellerlabelrect, "ignore Buyer:"); }

                GUI.skin = this.cardListPopupSkin;
                GUI.Box(this.sbsellerrect, string.Empty);
                string sellercopy = sellersearchstring;
                GUI.SetNextControlName("sellerframe");
                this.sellersearchstring = GUI.TextField(this.sbsellerrect, this.sellersearchstring, chatLogStyle);

                // draw price filter


                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(sbpricelabelrect, "<= Price <=");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                

                GUI.skin = this.cardListPopupSkin;
                GUI.Box(this.sbpricerect, string.Empty);
                GUI.Box(this.sbpricerect2, string.Empty);
                string pricecopy = pricesearchstring;
                string pricecopy2 = pricesearchstring2;
                this.pricesearchstring =Regex.Replace( GUI.TextField(this.sbpricerect, this.pricesearchstring, chatLogStyle),@"[^0-9]","");
                this.pricesearchstring2 = Regex.Replace(GUI.TextField(this.sbpricerect2, this.pricesearchstring2, chatLogStyle), @"[^0-9]", "");
                GUI.color = Color.white;

                // draw time filter

                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(sbtimelabel, "not older than");
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(this.sbtimerect, string.Empty);
                string timecopy = timesearchstring;
                this.timesearchstring = Regex.Replace(GUI.TextField(this.sbtimerect, this.timesearchstring,2, chatLogStyle), @"[^0-9]", "");
                if (timesearchstring!=""&&Convert.ToInt32(timesearchstring) > 30) { timesearchstring = "30"; }
                GUI.color = Color.white;


                bool tpfgen = GUI.Button(sbtpfgen, "");
                if (this.takepriceformgenarator)
                {
                    GUI.DrawTexture(sbtpfgen, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
                }
                else
                {
                    GUI.DrawTexture(sbtpfgen, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
                }
                GUI.skin = this.cardListPopupBigLabelSkin;
                if (this.wtsmenue)
                {
                    GUI.Label(sbtpfgenlabel, "Price <= wtb-generator");
                }
                else { GUI.Label(sbtpfgenlabel, "Price >= wts-generator"); }
                

                /*
                bool owp= GUI.Button(sbonlywithpricebox, "");
                if (this.ignore0)
                {
                    GUI.DrawTexture(sbonlywithpricebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
                }
                else
                {
                    GUI.DrawTexture(sbonlywithpricebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
                }
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(sbonlywithpricelabelbox, "only Scrolls with Price");
                 */

                GUI.skin = this.cardListPopupSkin;

                if (this.contonetwork)
                {
                    GUI.Label(sbnetworklabel, "User online: " + this.aucusers.Count() );
                }

                GUI.contentColor = Color.red;
                bool closeclick = GUI.Button(sbclearrect, "X");
                GUI.contentColor = Color.white;

                if (growthclick) { growthbool = !growthbool; };
                if (orderclick) { orderbool = !orderbool; }
                if (energyclick) { energybool = !energybool; };
                if (decayclick) { decaybool = !decaybool; }
                if (commonclick) { commonbool = !commonbool; };
                if (uncommonclick) { uncommonbool = !uncommonbool; }
                if (rareclick) { rarebool = !rarebool; };
                if (mt3click) { threebool = !threebool; }
                if (mt0click) { onebool = !onebool; }
                //if (owp) { ignore0 = !ignore0; }
                if (tpfgen) { takepriceformgenarator = !takepriceformgenarator; }
                if (closeclick)
                {
                    this.wtssearchstring = "";
                    this.pricesearchstring = "";
                    this.pricesearchstring2 = "";
                    this.sellersearchstring = "";
                    this.timesearchstring = "";
                    growthbool = true;
                    orderbool = true;
                    energybool = true;
                    decaybool = true;
                    commonbool = true;
                    uncommonbool = true;
                    rarebool = true;
                    threebool = false;
                    onebool = false;
                    ignore0 = false;
                    takepriceformgenarator = false;
                }

                if (this.wtsmenue) { savesettings(this.ahwtssettings); } else { savesettings(this.ahwtbsettings); }

                bool pricecheck = false;
                //if (wtsmenue) { pricecheck = (pricecopy2.Length < this.pricesearchstring2.Length) || (pricecopy2.Length != this.pricesearchstring2.Length && pricesearchstring2 == "") || (tpfgen); } else { pricecheck = pricecopy.Length > this.pricesearchstring.Length || (tpfgen); }

                pricecheck = (pricecopy2.Length < this.pricesearchstring2.Length) || (pricecopy2.Length != this.pricesearchstring2.Length && pricesearchstring2 == "") || (tpfgen) || pricecopy.Length > this.pricesearchstring.Length || (tpfgen); 
                
                //clear p1moddedlist only if necessary
                //if (selfcopy.Length > this.wtssearchstring.Length || (owp&&!ignore0)|| sellercopy.Length > this.sellersearchstring.Length || pricecheck || closeclick || (growthclick && growthbool) || (orderclick && orderbool) || (energyclick && energybool) || (decayclick && decaybool) || (commonclick && commonbool) || (uncommonclick && uncommonbool) || (rareclick && rarebool) || mt3click || mt0click)
                if (selfcopy.Length > this.wtssearchstring.Length || sellercopy.Length > this.sellersearchstring.Length || pricecheck || closeclick || (growthclick && growthbool) || (orderclick && orderbool) || (energyclick && energybool) || (decayclick && decaybool) || (commonclick && commonbool) || (uncommonclick && uncommonbool) || (rareclick && rarebool) || mt3click || mt0click)
                {
                    //Console.WriteLine("delete dings####");
                    this.fullupdatelist(ahlist, ahlistfull);

                }
                else
                {

                    if (selfcopy != this.wtssearchstring)
                    {

                        if (this.wtssearchstring != "")
                        {
                            this.containsname(this.wtssearchstring, ahlist);
                        }


                    }
                    if (ignore0)
                    {
                        this.musthaveprice(ahlist);
                    }

                    if (sellercopy != this.sellersearchstring)
                    {

                        if (this.sellersearchstring != "")
                        {
                            this.containsseller(this.sellersearchstring, ahlist);
                        }


                    }
                    if (pricecopy != this.pricesearchstring)
                    {

                        if (this.pricesearchstring != "" )
                        {
                            this.priceishigher(this.pricesearchstring, ahlist);

                        }


                    }
                    if (pricecopy2 != this.pricesearchstring2)
                    {

                        if (this.pricesearchstring2 != "")
                        {
                            this.priceislower(this.pricesearchstring2, ahlist);
                            

                        }


                    }
                    
                    if (growthclick || orderclick || energyclick || decayclick)
                    {
                        string[] res = { "", "", "", "" };
                        if (decaybool) { res[0] = "decay"; };
                        if (energybool) { res[1] = "energy"; };
                        if (growthbool) { res[2] = "growth"; };
                        if (orderbool) { res[3] = "order"; };
                        this.searchforownenergy(res, ahlist);


                    }
                    if (commonclick || uncommonclick || rareclick)
                    {

                        int[] rare = { -1, -1, -1 };
                        if (rarebool) { rare[2] = 2; };
                        if (uncommonbool) { rare[1] = 1; };
                        if (commonbool) { rare[0] = 0; };
                        this.searchforownrarity(rare, ahlist);

                    }

                }

            }
            // Draw Auctionhouse here:
            if (this.inauchouse)
            {
                //Console.WriteLine(GUI.GetNameOfFocusedControl());
                GUI.depth = 15;
                float offX = 0;
                this.opacity = 1f;
                GUI.skin = this.cardListPopupSkin;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
                Rect position = new Rect(this.outerRect.x + offX, this.outerRect.y, this.outerRect.width, this.outerRect.height);
                Rect position2 = new Rect(this.innerBGRect.x + offX, this.innerBGRect.y, this.innerBGRect.width, this.innerBGRect.height);
                GUI.Box(position, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
                GUI.Box(position2, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

                Rect position3 = new Rect(this.innerRect.x + offX, this.innerRect.y, this.innerRect.width, this.innerRect.height);
                
                // draw sort buttons:###############################################
                Vector2 vec11 = GUI.skin.button.CalcSize(new GUIContent("Scroll"));

                if (GUI.Button(new Rect(this.innerRect.xMin + this.labelX, this.screenRect.yMin - 4, vec11.x, 20), "Scroll"))
                {
                    if (this.reverse == true) { this.sortmode = -1; }// this will toggle the reverse mode
                    if (this.sortmode == 1) { this.reverse = true; } else { this.reverse = false; };
                    this.sortmode = 1;

                    sortlist(ahlist); sortlist(ahlistfull);

                }
                float datelength = GUI.skin.button.CalcSize(new GUIContent("Date")).x;
                float datebeginn = 0;
                if (this.wtsmenue)
                {
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Seller"));

                    if (GUI.Button(new Rect(this.innerRect.xMin + this.labelX + this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f + this.costIconWidth + (labelsWidth - vec11.x) / 2f - datelength/2f -2f, this.screenRect.yMin - 4f, vec11.x, 20f), "Seller"))
                    {
                        if (this.reverse == true) { this.sortmode = -1; }
                        if (this.sortmode == 3) { this.reverse = true; } else { this.reverse = false; };
                        this.sortmode = 3;

                        sortlist(ahlist); sortlist(ahlistfull);
                    }
                }
                else
                {
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Buyer"));
                    if (GUI.Button(new Rect(this.innerRect.xMin + this.labelX + this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f + this.costIconWidth + (labelsWidth - vec11.x) / 2f - datelength / 2f - 2f, this.screenRect.yMin - 4f, vec11.x, 20f), "Buyer"))
                    {
                        if (this.reverse == true) { this.sortmode = -1; }
                        if (this.sortmode == 3) { this.reverse = true; } else { this.reverse = false; };
                        this.sortmode = 3;

                        sortlist(ahlist); sortlist(ahlistfull);
                    }
                }
                datebeginn = this.innerRect.xMin + this.labelX + this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f + this.costIconWidth + (labelsWidth - vec11.x) / 2 - datelength / 2 - 2 + vec11.x;
                vec11 = GUI.skin.button.CalcSize(new GUIContent("Price"));
                if (GUI.Button(new Rect(this.innerRect.xMin + this.labelX + 2f * this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f + 2 * this.costIconWidth + labelsWidth / 4f - vec11.x/2, this.screenRect.yMin - 4, vec11.x , 20), "Price"))
                {
                    if (this.reverse == true) { this.sortmode = -1; }
                    if (this.sortmode == 2) { this.reverse = true; } else { this.reverse = false; };
                    this.sortmode = 2;

                    sortlist(ahlist); sortlist(ahlistfull);
                }
                vec11 = GUI.skin.button.CalcSize(new GUIContent("Date"));
                //if (GUI.Button(new Rect(this.innerRect.x + offX , this.screenRect.yMin - 4, vec11.x * 2, 20), "Date"))
                if (GUI.Button(new Rect(datebeginn + 4, this.screenRect.yMin - 4, vec11.x, 20), "Date"))
                {
                    if (this.reverse == true) { this.sortmode = -1; }
                    if (this.sortmode == 0) { this.reverse = true; } else { this.reverse = false; };
                    this.sortmode = 0;
                    if (this.wtsmenue)
                    {
                        wtslistfull.Clear(); wtslistfull.AddRange(this.wtslistfulltimed);
                        fullupdatelist(ahlist, ahlistfull);
                    }
                    else 
                    {
                        wtblistfull.Clear(); wtblistfull.AddRange(this.wtblistfulltimed);
                        fullupdatelist(ahlist, ahlistfull);
                    
                    }
                    sortlist(ahlist); sortlist(ahlistfull);
                }


                
                int num = 0;
                Card card = null;
                
                // delete old cards:
                DateTime currenttime = DateTime.Now.AddMinutes(-30);
                if (wtslistfulltimed.Count >0&& wtslistfulltimed[wtslistfulltimed.Count - 1].dtime.CompareTo(currenttime) < 0)
                {
                    this.wtslistfulltimed.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    this.wtslistfull.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    this.wtslist.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                }
                if (wtblistfulltimed.Count > 0 && wtblistfulltimed[wtblistfulltimed.Count - 1].dtime.CompareTo(currenttime) < 0)
                {
                    this.wtblistfulltimed.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    this.wtblistfull.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    this.wtblist.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                }
                // draw auctimes################################################
                //timefilter: 
                int time = 0;
                bool usetimefilter = false;
                float anzcards = anzcards = (float)this.ahlist.Count();
                if (this.timesearchstring != "") {
                    time = Convert.ToInt32(timesearchstring); 
                    currenttime = DateTime.Now.AddMinutes(-1 * time); usetimefilter = true;
                    anzcards = (float)this.ahlist.Count(delegate(aucitem p1) { return (p1.dtime).CompareTo(currenttime) >= 0; });
                }
               
                this.scrollPos = GUI.BeginScrollView(position3, this.scrollPos, new Rect(0f, 0f, this.innerRect.width - 20f, this.fieldHeight * anzcards));
                if (this.reverse) { this.ahlist.Reverse(); }
                GUI.skin = this.cardListPopupBigLabelSkin;
                foreach (aucitem current in this.ahlist)
                {
                    if (usetimefilter && (current.dtime).CompareTo(currenttime) < 0) { continue; }
                    if (!current.card.tradable)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    GUI.skin = this.cardListPopupGradientSkin;
                    //draw boxes
                    Rect position7 = new Rect(this.cardWidth + 10f, (float)num * this.fieldHeight, this.innerRect.width - this.scrollBarSize - this.cardWidth - this.costIconWidth - 12f, this.fieldHeight);
                    if (position7.yMax < this.scrollPos.y || position7.y > this.scrollPos.y + position3.height )
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
                        string txt = cardnametoimageid(name.ToLower()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())

                        GUI.skin = this.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                        // draw text
                        Rect position8 = new Rect(this.labelX, (float)num * this.fieldHeight - 3f + this.fieldHeight * 0.01f, this.labelsWidth, this.cardHeight);
                        GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, this.maxCharsName)) + "...") : name);
                        GUI.skin = this.cardListPopupSkin;
                        string text = current.card.getPieceKind().ToString();
                        string str = text.Substring(0, 1) + text.Substring(1).ToLower();
                        string text2 = string.Empty;
                        int num2 = this.maxCharsRK;
                        if (current.card.level > 0)
                        {
                            string text3 = text2;
                            text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.card.level + 1, "</color>, " });
                            num2 += "<color=#rrggbb></color>".Length;
                        }
                        text2 = text2 + current.card.getRarityString() + ", " + str;
                        Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(text2));
                        
                        Rect position9 = new Rect(this.labelX, (float)num * this.fieldHeight - 3f + this.fieldHeight * 0.57f, this.labelsWidth, this.cardHeight);
                        GUI.Label(position9, (vector2.x >= position9.width) ? (text2.Substring(0, Mathf.Min(text2.Length, num2)) + "...") : text2);
                        Rect restyperect = new Rect(this.labelX + this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f, (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight);
                        //draw resource type
                        this.RenderCost(restyperect, current.card);
                        //draw seller name

                        string sellername = current.seller;
                        GUI.skin = this.cardListPopupBigLabelSkin;

                        vector = GUI.skin.label.CalcSize(new GUIContent(sellername));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                        //Rect position11 = new Rect(restyperect.xMax + 2f, (float)num * this.fieldHeight, this.labelsWidth, this.fieldHeight);
                        Rect position11 = new Rect(restyperect.xMax + 2f, position8.yMin, this.labelsWidth, this.cardHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position11, (vector.x >= position11.width) ? (sellername.Substring(0, Mathf.Min(sellername.Length, this.maxCharsName)) + "...") : sellername);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        //draw timestamp
                        GUI.skin = this.cardListPopupSkin;
                        DateTime temptime = DateTime.Now;
                        TimeSpan ts=temptime.Subtract(current.dtime);

                        if (ts.Minutes >= 1) { sellername = "" + ts.Minutes + " minutes ago"; }
                        else { 
                            //sellername = "" + ts.Seconds + " seconds ago"; // to mutch changing numbers XD
                            sellername = "seconds ago";
                        }
                       
                        Rect position13 = new Rect(restyperect.xMax + 2f, position9.y, this.labelsWidth, this.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position13,sellername);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;
                        //testonly
                        //    restyperect = new Rect(position11.xMax , (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight);
                        //       draw resource type
                        //     this.RenderCost(restyperect, current);

                        //draw gold cost
                        float nextx = position11.xMax + this.costIconWidth;
                        string gold = current.price + " G";
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                        Rect position12 = new Rect(nextx + 2f, (float)num * this.fieldHeight, this.labelsWidth / 2f, this.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position12, gold);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;



                        GUI.skin = this.cardListPopupLeftButtonSkin;
                        Rect position10 = new Rect(0f, (float)num * this.fieldHeight, this.cardWidth + 8f, this.fieldHeight);
                        if (this.itemButtonTexture == null && !this.selectable)
                        {
                            GUI.enabled = false;
                        }
                        if (GUI.Button(position10, string.Empty) && current.card.tradable)
                        {
                            card = current.card;
                            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                        }
                        if (this.itemButtonTexture == null && !this.selectable)
                        {
                            GUI.enabled = true;
                        }
                        //draw picture
                        if (texture != null)
                        {
                            GUI.DrawTexture(new Rect(4f, (float)num * this.fieldHeight + (this.fieldHeight - this.cardHeight) * 0.43f, this.cardWidth, this.cardHeight), texture);
                        }
                        // draw buy/sell button
                        GUI.skin = this.cardListPopupGradientSkin;
                        if (this.wtsmenue)
                        {
                            if (!showtradedialog)
                            {
                                if (GUI.Button(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), ""))
                                {

                                    // start trading with seller
                                    if (current.seller != this.ownname)
                                    {
                                        showtradedialog = true;
                                        tradeitem = current;
                                    }
                                }
                            }
                            else { GUI.Box(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), ""); }
                            GUI.skin = this.cardListPopupBigLabelSkin;
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            GUI.Label(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), "Buy");
                            

                        }
                        else
                        {
                            if (!showtradedialog)
                            {
                                if (GUI.Button(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), ""))
                                {

                                    // start trading with seller
                                    if (current.seller != this.ownname)
                                    {
                                        showtradedialog = true;
                                        tradeitem = current;
                                    }
                                }
                            }
                            else { GUI.Box(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), ""); }
                            GUI.skin = this.cardListPopupBigLabelSkin;
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            GUI.Label(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), "Sell");
                        }
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        if (!current.card.tradable)
                        {
                            GUI.color = Color.white;
                        }
                        num++;
                    }
                }
                if (this.reverse) { this.ahlist.Reverse(); }
                GUI.EndScrollView();
                GUI.color = Color.white;
                if (card != null)
                {
                    //this.callback.ItemButtonClicked(this, card);
                    createcard(card.getName());
                    this.clicked = 0;
                }
                //wts / wtb menue buttons

                if (this.wtsmenue)
                {
                    GUI.color = Color.white;
                }
                else 
                { 
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f); 
                }
                if (this.newwtsmsgs) 
                { 
                    GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                    GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
                }

                
                if (GUI.Button(wtsbuttonrect, "WTS"))
                {

                    wtslistfull.Clear(); wtslistfull.AddRange(this.wtslistfulltimed);
                    sortlist(wtslistfull);

                    this.ahlist = this.wtslist; this.ahlistfull = this.wtslistfull; this.wtsmenue = true;
                    setsettings(this.ahwtssettings);
                    fullupdatelist(ahlist, ahlistfull);
                    this.newwtsmsgs = false;
                }
                GUI.skin.button.normal.textColor = Color.white;
                GUI.skin.button.hover.textColor = Color.white;

                


                if (!this.wtsmenue)
                {
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                if (this.newwtbmsgs)
                {
                    GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                    GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
                }
                if (GUI.Button(wtbbuttonrect, "WTB"))
                {
                    wtblistfull.Clear(); wtblistfull.AddRange(this.wtblistfulltimed);
                    //sortmode==0 = sort by date so dont sort wtsfulltimed
                    sortlist(wtblistfull);
                    this.ahlist = this.wtblist; this.ahlistfull = this.wtblistfull; this.wtsmenue = false;
                    setsettings(this.ahwtbsettings);
                    fullupdatelist(ahlist, ahlistfull);
                    this.newwtbmsgs = false;
                }
                GUI.skin.button.normal.textColor = Color.white;
                GUI.skin.button.hover.textColor = Color.white;

                
                GUI.color = Color.white;

                if (this.contonetwork)
                {
                    if (GUI.Button(this.updatebuttonrect, "discon"))
                    {
                        disconfromaucnet();
                    }
                }
                else
                {
                    if (GUI.Button(this.updatebuttonrect, "connect"))
                    {
                        this.contonetwork = true;
                        App.Communicator.sendRequest(new RoomEnterMessage("auc-1"));
                        this.joindate = DateTime.Now;
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

                if (this.showtradedialog) { this.starttrading(tradeitem.seller,tradeitem.card.getName(),tradeitem.priceinint, this.wtsmenue,tradeitem.whole); }






            }
        }

        private void drawgenerator()
        {
            // have to draw textfield in front of scrollbar or otherwise you lose focus in textfield (lol)

            if (this.generator)
            {
                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(filtermenurect, string.Empty);
                // wts filter menue
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(sbarlabelrect, "Scroll:");
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(this.sbrect, string.Empty);
                string selfcopy = wtssearchstring;
                this.wtssearchstring = GUI.TextField(this.sbrect, this.wtssearchstring, chatLogStyle);


                GUI.contentColor = Color.white;
                GUI.color = Color.white;
                if (!growthbool) { GUI.color = dblack; }
                bool growthclick = GUI.Button(sbgrect, growthres);
                GUI.color = Color.white;
                if (!orderbool) { GUI.color = dblack; }
                bool orderclick = GUI.Button(sborect, orderres);
                GUI.color = Color.white;
                if (!energybool) { GUI.color = dblack; }
                bool energyclick = GUI.Button(sberect, energyres);
                GUI.color = Color.white;
                if (!decaybool) { GUI.color = dblack; }
                bool decayclick = GUI.Button(sbdrect, decayres);


                GUI.contentColor = Color.gray;
                GUI.color = Color.white;
                if (!commonbool) { GUI.color = dblack; }
                bool commonclick = GUI.Button(sbcommonrect, "C");
                GUI.color = Color.white;
                if (!uncommonbool) { GUI.color = dblack; }
                GUI.contentColor = Color.white;
                bool uncommonclick = GUI.Button(sbuncommonrect, "U");
                GUI.color = Color.white;
                if (!rarebool) { GUI.color = dblack; }
                GUI.contentColor = Color.yellow;
                bool rareclick = GUI.Button(sbrarerect, "R");
                GUI.color = Color.white;
                if (!threebool) { GUI.color = dblack; }
                //if (!p1mt3bool) { GUI.color = dblack; }
                bool mt3click;
                bool mt0click = false;
                if (this.wtsmenue)
                {
                    mt3click = GUI.Button(sbthreerect, ">3"); // >3 bei wtsmenue=false
                }
                else { mt3click = GUI.Button(sbthreerect, "<3"); }
                GUI.color = Color.white;
                if (!onebool) { GUI.color = dblack; }
                //if (this.wtsmenue) { mt0click = GUI.Button(sbonerect, ">0"); };
                GUI.color = Color.white;
                GUI.contentColor = Color.white;

                GUI.skin = this.cardListPopupBigLabelSkin;
                if (this.wtsmenue)
                {
                    GUI.Label(sbsellerlabelrect, "wts msg:");
                }
                else { GUI.Label(sbsellerlabelrect, "wtb msg:"); }

                GUI.skin = this.cardListPopupSkin;
                GUI.Box(this.sbsellerrect, string.Empty);
                string sellercopy = sellersearchstring;
                GUI.SetNextControlName("sellerframe");
                GUI.TextField(this.sbsellerrect, this.sellersearchstring, chatLogStyle);

                /*
                GUI.skin = this.cardListPopupBigLabelSkin;
                if (this.wtsmenue)
                {
                    GUI.Label(sbpricelabelrect, "short wts msg");
                }
                else { GUI.Label(sbpricelabelrect, "short wtb msg"); }

                GUI.skin = this.cardListPopupSkin;
                GUI.Box(this.sbpricerect, string.Empty);
                string pricecopy = pricesearchstring;
                GUI.SetNextControlName("priceframe");
                GUI.TextField(this.sbpricerect, this.pricesearchstring, chatLogStyle);
                */
                GUI.color = Color.white;
                GUI.contentColor = Color.red;
                bool closeclick = GUI.Button(sbclearrect, "X");
                GUI.contentColor = Color.white;

                if (growthclick) { growthbool = !growthbool; };
                if (orderclick) { orderbool = !orderbool; }
                if (energyclick) { energybool = !energybool; };
                if (decayclick) { decaybool = !decaybool; }
                if (commonclick) { commonbool = !commonbool; };
                if (uncommonclick) { uncommonbool = !uncommonbool; }
                if (rareclick) { rarebool = !rarebool; };
                if (mt3click) { threebool = !threebool; }
                if (mt0click) { onebool = !onebool; }
                if (closeclick)
                {
                    this.wtssearchstring = "";
                    this.pricesearchstring = "";
                    this.sellersearchstring = "";
                    growthbool = true;
                    orderbool = true;
                    energybool = true;
                    decaybool = true;
                    commonbool = true;
                    uncommonbool = true;
                    rarebool = true;
                    threebool = false;
                    onebool = false;
                }
                if (this.wtsmenue) { savesettings(this.genwtssettings); } else { savesettings(this.genwtbsettings); }
                //if (wtsmenue) { pricecheck = (pricecopy.Length < this.pricesearchstring.Length) || (pricecopy.Length != this.pricesearchstring.Length && pricesearchstring == ""); } else { pricecheck = pricecopy.Length > this.pricesearchstring.Length; }
                //clear p1moddedlist only if necessary
                if (selfcopy.Length > this.wtssearchstring.Length  || closeclick || (growthclick && growthbool) || (orderclick && orderbool) || (energyclick && energybool) || (decayclick && decaybool) || (commonclick && commonbool) || (uncommonclick && uncommonbool) || (rareclick && rarebool) || mt3click || mt0click)
                {
                    //Console.WriteLine("delete dings####");
                    this.fullupdatelist(ahlist, ahlistfull);

                }
                else
                {

                    if (selfcopy != this.wtssearchstring)
                    {

                        if (this.wtssearchstring != "")
                        {
                            this.containsname(this.wtssearchstring, ahlist);

                        }


                    }
                    
                    if (growthclick || orderclick || energyclick || decayclick)
                    {
                        string[] res = { "", "", "", "" };
                        if (decaybool) { res[0] = "decay"; };
                        if (energybool) { res[1] = "energy"; };
                        if (growthbool) { res[2] = "growth"; };
                        if (orderbool) { res[3] = "order"; };
                        this.searchforownenergy(res, ahlist);


                    }
                    if (commonclick || uncommonclick || rareclick)
                    {

                        int[] rare = { -1, -1, -1 };
                        if (rarebool) { rare[2] = 2; };
                        if (uncommonbool) { rare[1] = 1; };
                        if (commonbool) { rare[0] = 0; };
                        this.searchforownrarity(rare, ahlist);

                    }

                }
                // draw generate button!

                

                if (GUI.Button(this.sbclrearpricesbutton, "Post to Network"))
                {
                    if (this.contonetwork)
                    {
                        string wtsdings = this.genwtssettings.strings2;
                        string wtbdings = this.genwtbsettings.strings2;
                        foreach (KeyValuePair<string, string> pair in this.aucusers)
                        {
                            if (wtsdings != "")
                            {

                                dowhisper(wtsdings, pair.Key);
                            }
                            if (wtbdings != "")
                            {
                                dowhisper(wtbdings, pair.Key);
                            }
                        }
                    }
                    
                }
                


                     if (this.wtsmenue)
                        {
                            if (GUI.Button(sbgeneratebutton, "Gen WTS msg"))
                            {
                                // start trading with seller
                                generatewtxmsg(this.ahlistfull);
                            }
                        }
                        else
                        {
                            if (GUI.Button(sbgeneratebutton, "Gen WTB msg"))
                            {
                                // start trading bith buyer
                                generatewtxmsg(this.ahlistfull);
                            }
                        }

            }
            // Draw Auctionhouse here:
            if (this.generator)
            {
                //Console.WriteLine(GUI.GetNameOfFocusedControl());
                GUI.depth = 15;
                float offX = 0;
                this.opacity = 1f;
                GUI.skin = this.cardListPopupSkin;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
                Rect position = new Rect(this.outerRect.x + offX, this.outerRect.y, this.outerRect.width, this.outerRect.height);
                Rect position2 = new Rect(this.innerBGRect.x + offX, this.innerBGRect.y, this.innerBGRect.width, this.innerBGRect.height);
                GUI.Box(position, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
                GUI.Box(position2, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

                Rect position3 = new Rect(this.innerRect.x + offX, this.innerRect.y, this.innerRect.width, this.innerRect.height);

                this.scrollPos = GUI.BeginScrollView(position3, this.scrollPos, new Rect(0f, 0f, this.innerRect.width - 20f, this.fieldHeight * (float)this.ahlist.Count));
                int num = 0;
                Card card = null;
                GUI.skin = this.cardListPopupBigLabelSkin;
                foreach (aucitem current in this.ahlist)
                {

                    if (!current.card.tradable)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    GUI.skin = this.cardListPopupGradientSkin;
                    //draw boxes
                    Rect position7 = new Rect(this.cardWidth + 10f, (float)num * this.fieldHeight, this.innerRect.width - this.scrollBarSize - this.cardWidth - this.costIconWidth - 12f, this.fieldHeight);
                    if (position7.yMax < this.scrollPos.y || position7.y > this.scrollPos.y + position3.height)
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
                        string txt = cardnametoimageid(name.ToLower()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                        // draw text
                        Rect position8 = new Rect(this.labelX, (float)num * this.fieldHeight - 3f + this.fieldHeight * 0.01f, this.labelsWidth, this.cardHeight);
                        GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, this.maxCharsName)) + "...") : name);
                        GUI.skin = this.cardListPopupSkin;
                        string text = current.card.getPieceKind().ToString();
                        string str = text.Substring(0, 1) + text.Substring(1).ToLower();
                        string text2 = string.Empty;
                        int num2 = this.maxCharsRK;
                       /* if (current.card.level > 0)
                        {
                            string text3 = text2;
                            text2 = string.Concat(new object[] { text3, "<color=#ddbb44>Tier ", current.card.level + 1, "</color>, " });
                            num2 += "<color=#rrggbb></color>".Length;
                        }*/
                        text2 = text2 + current.card.getRarityString() + ", " + str;
                        Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent(text2));
                        Rect position9 = new Rect(this.labelX, (float)num * this.fieldHeight - 3f + this.fieldHeight * 0.57f, this.labelsWidth, this.cardHeight);
                        GUI.Label(position9, (vector2.x >= position9.width) ? (text2.Substring(0, Mathf.Min(text2.Length, num2)) + "...") : text2);
                        Rect restyperect = new Rect(this.labelX + this.labelsWidth + (this.costIconSize - this.costIconWidth) / 2f - 5f, (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight);
                        //draw resource type
                        this.RenderCost(restyperect, current.card);
                        // write PRICE
                        //GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        float nextx = restyperect.xMax + this.costIconWidth/2;
                        string gold = "Price";
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        Rect position12 = new Rect(nextx + 2f, (float)num * this.fieldHeight, this.labelsWidth / 2f, this.fieldHeight);
                        GUI.Label(position12, gold);
                        

                        //draw pricebox
                        //
                        Rect position11 = new Rect(position12.xMax + 2f, (float)(num + 1) * this.fieldHeight - (this.fieldHeight + vector2.y) / 2 - 2, this.labelsWidth, vector2.y + 4);
                        GUI.skin = this.cardListPopupSkin;
                        GUI.Box(position11, string.Empty);
                        // priceinint wurde bei der genliste missbraucht

                        this.chatLogStyle.alignment = TextAnchor.MiddleCenter;
                        if (this.wtsmenue)
                        {
                            this.wtspricelist1[current.card.getName().ToLower()] = Regex.Replace(GUI.TextField(position11, this.wtspricelist1[current.card.getName().ToLower()], chatLogStyle), @"[^0-9]", "");
                        }
                        else
                        {
                            this.wtbpricelist1[current.card.getName().ToLower()] = Regex.Replace(GUI.TextField(position11, this.wtbpricelist1[current.card.getName().ToLower()], chatLogStyle), @"[^0-9]", "");
                        }
                        this.chatLogStyle.alignment = TextAnchor.MiddleLeft;
                        //string sellername = current.seller;
                        GUI.skin = this.cardListPopupBigLabelSkin;

                        //GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                        //vector = GUI.skin.label.CalcSize(new GUIContent(sellername));
                        //Rect position11 = new Rect(restyperect.xMax + 2f, (float)num * this.fieldHeight, this.labelsWidth, this.fieldHeight);
                        //GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        //GUI.Label(position11, (vector.x >= position11.width) ? (sellername.Substring(0, Mathf.Min(sellername.Length, this.maxCharsName)) + "...") : sellername);
                        //GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                        //testonly
                        //    restyperect = new Rect(position11.xMax , (float)num * this.fieldHeight + (this.fieldHeight - this.costIconHeight) / 2f, this.costIconWidth, this.costIconHeight);
                        //       draw resource type
                        //     this.RenderCost(restyperect, current);

                        //draw gold cost




                        GUI.skin = this.cardListPopupLeftButtonSkin;
                        Rect position10 = new Rect(0f, (float)num * this.fieldHeight, this.cardWidth + 8f, this.fieldHeight);
                        if (this.itemButtonTexture == null && !this.selectable)
                        {
                            GUI.enabled = false;
                        }
                        if (GUI.Button(position10, string.Empty) && current.card.tradable)
                        {
                            card = current.card;
                            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                        }
                        if (this.itemButtonTexture == null && !this.selectable)
                        {
                            GUI.enabled = true;
                        }
                        //draw picture
                        if (texture != null)
                        {
                            GUI.DrawTexture(new Rect(4f, (float)num * this.fieldHeight + (this.fieldHeight - this.cardHeight) * 0.43f, this.cardWidth, this.cardHeight), texture);
                        }
                        // draw buy/sell button
                        if (this.wtsmenue)
                        {
                            if (GUI.Button(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), "SP"))
                            {
                                this.SPretindex = current.card.getName();
                                this.SPtarget = true;
                                this.PriceChecker(current.card.getName());
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(position7.xMax+2, (float)num * this.fieldHeight, this.costIconWidth, this.fieldHeight), "SP"))
                            {
                                this.SPretindex = current.card.getName();
                                this.SPtarget = false;
                                this.PriceChecker(current.card.getName());
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
                    createcard(card.getName());
                    this.clicked = 0;
                }
                //wts / wtb menue buttons
                if (this.wtsmenue)
                {

                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                
                if (GUI.Button(wtsbuttonrect, "WTS"))
                {
                    this.ahlist = this.wtsPlayer; this.ahlistfull = this.orgicardsPlayerwountrade; this.wtsmenue = true;
                    setsettings(this.genwtssettings);
                    fullupdatelist(ahlist, ahlistfull);
                }
                if (!this.wtsmenue)
                {

                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                if (GUI.Button(wtbbuttonrect, "WTB"))
                {
                    this.ahlist = this.wtbPlayer; this.ahlistfull = this.allcardsavailable; this.wtsmenue = false;
                    setsettings(this.genwtbsettings);
                    fullupdatelist(ahlist, ahlistfull);
                }
                GUI.color = Color.white;
                if (GUI.Button(updatebuttonrect, "Clear"))
                {
                    if (this.wtsmenue)
                    {
                        for (int i = 0; i < this.wtspricelist1.Count; i++)
                        {
                            KeyValuePair<string, string> item = this.wtspricelist1.ElementAt(i);
                            this.wtspricelist1[item.Key] = "";

                        }

                    }
                    else
                    {
                        for (int i = 0; i < this.wtbpricelist1.Count; i++)
                        {
                            KeyValuePair<string, string> item = this.wtbpricelist1.ElementAt(i);
                            this.wtbpricelist1[item.Key] = "";

                        }
                    }
                    
                }
                

            }
        }


        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            if(info.target is ChatUI && info.targetMethod.Equals("Show"))
            {
                this.chatisshown = (bool)info.arguments[0];
                this.screenh = 0;// so position will be calculatet new on next ongui
            }

            if (info.target is ChatUI && info.targetMethod.Equals("Initiate")) 
            { 
                target = (ChatUI)info.target;
                this.chatLogStyle =(GUIStyle)this.chatLogStyleinfo.GetValue(info.target);
                chatRooms = (ChatRooms)chatRoomsinfo.GetValue(info.target);
            
            }

            if (info.target is Store && info.targetMethod.Equals("Start"))
            {
                this.lobbySkin = (GUISkin)typeof(Store).GetField("lobbySkin", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                this.storeinfo=(Store)info.target;
                //this.AHFrame = new GameObject("Card List / AH List").AddComponent<CardListPopup>();
                //this.AHFrame.Init(new Rect((float)Screen.width * 0.01f, (float)Screen.height * 0.18f, (float)Screen.height * 0.8f, (float)Screen.height * 0.7f), false, true, this.ahlist, this, null, new GUIContent("BLUBB"), false, true, false, false, null, false);
                //this.AHFrame.SetOpacity(1f);
                
                
            }

            if (info.target is ChatRooms && info.targetMethod.Equals("SetRoomInfo"))
            {
                RoomInfoMessage roomInfo = (RoomInfoMessage)info.arguments[0];
                RoomInfoProfile[] profiles = roomInfo.updated;
                for (int i = 0; i < profiles.Length; i++)
                {
                    RoomInfoProfile p = profiles[i];
                    ChatUser user = ChatUser.FromRoomInfoProfile(p) ;
                    if (!globalusers.ContainsKey(user.name)) { globalusers.Add(user.name, user); };
                } 
            }

            if (info.target is Store && info.targetMethod.Equals("handleMessage"))// update orginal cards!
            {
                
                Message msg = (Message)info.arguments[0];
                if (msg is LibraryViewMessage)
                {
                    if (!(((LibraryViewMessage)msg).profileId == "test"))
                    {
                        this.orgicardsPlayer.Clear();
                        this.orgicardsPlayerwountrade.Clear();
                        this.orgicardsPlayer.AddRange(((LibraryViewMessage)msg).cards);
                        List<string> checklist = new List<string>();
                        this.available.Clear();
                        foreach (aucitem ai in allcardsavailable)
                        {
                            if (!available.ContainsKey(ai.card.getName()))
                            {
                                available.Add(ai.card.getName(), 0);
                            }
                        }

                        foreach (Card c in orgicardsPlayer)
                        {
                            if (c.tradable&& !(checklist.Contains(c.getName())))
                            {
                                aucitem ai = new aucitem();
                                ai.card = c;
                                ai.seller = "me";
                                ai.price = "";
                                ai.priceinint = orgicardsPlayerwountrade.Count;
                                this.orgicardsPlayerwountrade.Add(ai);
                                checklist.Add(c.getName());
                            }

                                
                                available[c.getName()] = available[c.getName()] + 1;
                        }

                        this.orgicardsPlayerwountrade.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });

                        this.wtspricelist1.Clear();
                        for (int i = 0; i < orgicardsPlayerwountrade.Count; i++)
                        {
                            this.wtspricelist1.Add(orgicardsPlayerwountrade[i].card.getName().ToLower(), "");

                        }


                        if (this.generator && this.wtsmenue)
                        {
                            fullupdatelist(ahlist, ahlistfull);
                        }
                        
                    }
                }
            }

            else if (info.target is Store && info.targetMethod.Equals("OnGUI"))
            {

               
                GUI.color = Color.white;
                GUI.contentColor = Color.white;
                drawsubmenu.Invoke(info.target, null);
                    Vector2 screenMousePos = GUIUtil.getScreenMousePos();
                   

                    if (!(Screen.height == screenh) || !(Screen.width == screenw)|| chatLogStyle==null) // if resolution was changed, recalc positions
                    {
                        screenh = Screen.height;
                        screenw = Screen.width;
                        chatLogStyle = (GUIStyle)chatLogStyleinfo.GetValue(target);
                        setupPositions();

                    }
                   
                    
                    // delete picture on click!
                    if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && this.clicked >=3) { this.clearallpics(); }

                    //auction house...
                    GUIPositioner subMenuPositioner = App.LobbyMenu.getSubMenuPositioner(1f, 4);
                    //klick button AH
                    if (LobbyMenu.drawButton(subMenuPositioner.getButtonRect(2f), "AH", this.lobbySkin))
                    {
                        this.inauchouse = true;
                        //this.hideInformation();
                        hideInformationinfo.Invoke(storeinfo, null);

                        showBuyinfo.SetValue(storeinfo, false);
                        iTween.MoveTo((GameObject)buymen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));

                        showSellinfo.SetValue(storeinfo, false);
                        iTween.MoveTo((GameObject)sellmen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));

                        Store.ENABLE_SHARD_PURCHASES = false;
                        
                        this.generator = false;
                        this.clickableItems = false;
                        this.selectable = true;
                        wtslistfull.Clear(); wtslistfull.AddRange(this.wtslistfulltimed);

                        sortlist(wtslistfull);

                        this.ahlist = this.wtslist; this.ahlistfull = this.wtslistfull; this.wtsmenue = true;

                        setsettings(this.ahwtssettings);
                        fullupdatelist(ahlist,ahlistfull);
                    }
                // klick button Gen
                    
                    if (LobbyMenu.drawButton(subMenuPositioner.getButtonRect(3f), "Gen", this.lobbySkin))
                    {
                        //this.hideInformation();
                        hideInformationinfo.Invoke(storeinfo, null);
                        showBuyinfo.SetValue(storeinfo, false);
                        iTween.MoveTo((GameObject)buymen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        showSellinfo.SetValue(storeinfo, false);
                        iTween.MoveTo((GameObject)sellmen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        Store.ENABLE_SHARD_PURCHASES = false;
                        this.inauchouse = false;
                        this.generator = true;

                        this.clickableItems = false;
                        this.selectable = true;

                        this.ahlist = this.wtsPlayer; this.ahlistfull = this.orgicardsPlayerwountrade; this.wtsmenue = true;

                        //this.genlist.AddRange(this.genlistfull);

                        setsettings(this.genwtssettings);
                        fullupdatelist(ahlist, ahlistfull);
                    }

                    // draw ah oder gen-menu

                    if (this.inauchouse) drawAH();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;
                    if (this.generator) drawgenerator();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;
                    // draw cardoverlay again!
                    if (this.mytext)
                    {
                        if (this.clicked < 3) clicked++;
                        //GUI.depth =1;
                        Rect rect = new Rect(100,100,100,Screen.height-200);
                        foreach (cardtextures cd in this.gameObjects)
                        {
                            GUI.DrawTexture(cd.cardimgrec, cd.cardimgimg); 
                        }

                        foreach (renderwords rw in this.textsArr)
                        {
                            
                            float width =rw.style.CalcSize(new GUIContent(rw.text)).x;
                           GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0),
                            Quaternion.identity, new Vector3(rw.rect.width / width, rw.rect.width / width, 1));
                            
                           Rect lol = new Rect(rw.rect.x * width / rw.rect.width, rw.rect.y * width / rw.rect.width, rw.rect.width * width / rw.rect.width, rw.rect.height * width / rw.rect.width);
                           GUI.contentColor = rw.color;
                           GUI.Label(lol, rw.text, rw.style);
                            
                        }

                    }

                    
                
            }
            else if (info.target is Store && (info.targetMethod.Equals("showBuyMenu") || info.targetMethod.Equals("showSellMenu")))
            {
                //disable auc.house + generator
                Store.ENABLE_SHARD_PURCHASES = true;
                inauchouse = false;
                generator = false;

            }

            if (info.target is ChatRooms && info.targetMethod.Equals("ChatMessage"))
            {
                //get trademessages from chatmessages
                RoomChatMessageMessage msg = (RoomChatMessageMessage)info.arguments[0];
                getaucitemsformmsg(msg.text, msg.from, msg.roomName);
            }

            return;
        }

        //need following 6 methods for icardrule
        public void HideCardView()
        {
           
        }
        public void SetLoadedImage(Texture2D image, string imageName)
        {
            ResourceManager.instance.assignTexture2D(imageName, image);
            
        }
        public Texture2D GetLoadedImage(string imageName)
        {
            return ResourceManager.instance.tryGetTexture2D(imageName);
        }
        public void ActivateTriggeredAbility(string id, TilePosition pos)
        {
        }
        public void effectAnimDone(EffectPlayer theEffect, bool loop)
        {
            if (loop)
            {
                theEffect.playEffect(0);
            }
            else
            {
                UnityEngine.Object.Destroy(theEffect);
            }
        }
        public void locator(EffectPlayer effect, AnimLocator loc)
        {
        }
        
        
        private void gettextures(CardView cardView)
        { // get textures from cardview (because cardview issnt painted above ongui drawing, so we draw the textures ongui :D)
            this.gameObjects.Clear();
            GameObject go1 = (GameObject)cardImageField.GetValue(cardView);
            cardtextures temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            Vector3 vec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            Vector3 vec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            Rect rec = new Rect(vec1.x, Screen.height - vec2.y, vec2.x - vec1.x, vec2.y - vec1.y);
            temp1.cardimgrec = rec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }

            // card-texture
            temp1 = new cardtextures();
            temp1.cardimgimg = cardtext;
            temp1.cardimgrec = cardrect;
            this.gameObjects.Add(temp1);
            //icon background
            go1 = (GameObject)icoBGField.GetValue(cardView);
            temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            Vector3 ttvec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            Vector3 ttvec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            Rect ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
            temp1.cardimgrec = ttrec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }
            //stats background
            go1 = (GameObject)statsBGField.GetValue(cardView);
            temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            ttvec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            ttvec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
            temp1.cardimgrec = ttrec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }
            //ico
            go1 = (GameObject)icoField.GetValue(cardView);
            temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            ttvec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            ttvec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
            temp1.cardimgrec = ttrec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }

            



            List<GameObject> Images = (List<GameObject>)gosNumHitPointsField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled){ this.gameObjects.Add(temp);}
            }

            //ability background
            Images = (List<GameObject>)gosactiveAbilityField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                if (go.name == "Trigger_Ability_Button")
                {
                    cardtextures temp = new cardtextures();
                    temp.cardimgimg = go.renderer.material.mainTexture;
                    Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                    Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                    Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                    temp.cardimgrec = trec;
                    if (go.renderer.enabled) { this.gameObjects.Add(temp); }
                    break;
                }
            }

            Images = (List<GameObject>)gosNumCostField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled) { this.gameObjects.Add(temp); }
            }
            Images = (List<GameObject>)gosNumAttackPowerField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled) { this.gameObjects.Add(temp); }
            }
            Images = (List<GameObject>)gosNumCountdownField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled) { this.gameObjects.Add(temp); }
            }

            textsArr.Clear();
            Images = (List<GameObject>)textsArrField.GetValue(cardView);
  
            foreach (GameObject go in Images)
            {
                TextMesh lol = go.GetComponentInChildren<TextMesh>();
                renderwords stuff;
                stuff.text = lol.text;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                stuff.rect = trec;
                GUIStyle style = new GUIStyle();
                style.font = lol.font;
                style.alignment = (TextAnchor)lol.alignment;
                style.fontSize = (int)(lol.fontSize);
                style.wordWrap = false;
                style.stretchHeight = false;
                style.stretchWidth = false;
                stuff.color = new Color(go.renderer.material.color.r, go.renderer.material.color.g, go.renderer.material.color.b, 0.9f);
                style.normal.textColor = stuff.color;
                stuff.style = style;
                textsArr.Add(stuff);

            }

        }

        
        private void clearallpics()
        {
            UnityEngine.Object.Destroy(this.cardRule);
        textsArr = new List<renderwords>();
        gameObjects = new List<cardtextures>();
        this.mytext = false;
        }
    
    //icardlistcallback

        public void ButtonClicked(CardListPopup popup, ECardListButton button)
        {
            
        }
        public void ButtonClicked(CardListPopup popup, ECardListButton button, List<Card> selectedCards)
        {
            
        }
        public void ItemButtonClicked(CardListPopup popup, Card card)
        {
            
        }
        public void ItemHovered(CardListPopup popup, Card card)
        {
        }
        public void ItemClicked(CardListPopup popup, Card card)
        {
        }

        private void starttrading(string name, string cname, int price, bool wts, string orgmsg)
        {
            // asks the user if he wants to trade
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(tradingbox, "");
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "sell";
            if (wts) text = "buy";
            int anzcard =available[cname];
            string message = "You want to " + text + "\r\n" + cname + " for " + price + " Gold" +"\r\nYou own this card "  + anzcard+" times\r\n\r\nOriginal Message:";
            GUI.Label(tbmessage, message);
            GUI.skin.label.wordWrap = true;
            float msghigh = GUI.skin.label.CalcHeight(new GUIContent(orgmsg), tbmessage.width - 30f);
            GUI.skin = this.cardListPopupSkin;
            scrolll = GUI.BeginScrollView(tbmessagescroll, scrolll, new Rect(0f, 0f, this.tbmessagescroll.width - 20f, msghigh));
            GUI.skin = this.cardListPopupBigLabelSkin;
            
            GUI.Label(new Rect(5f, 5f, tbmessagescroll.width - 30f, msghigh), orgmsg);
            //GUI.skin.label.wordWrap = false;
            //GUI.Label(tbmessage, message);
            Console.WriteLine(message);
            
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.EndScrollView();
            GUI.skin.label.wordWrap = false;
            GUI.skin = this.cardListPopupLeftButtonSkin;

            if (GUI.Button(tbok, "OK")) { this.showtradedialog = false; App.GameActionManager.TradeUser(this.globalusers[name]); };
            if (GUI.Button(tbcancel, "Cancel")) { this.showtradedialog = false;};
        }

        private void setupPositions()
        {// set rects for menus
            this.screenRect = new Rect((float)Screen.width * 0.01f, (float)Screen.height * 0.18f, (float)Screen.width * 0.6f, (float)Screen.height * 0.57f);
            if (!chatisshown) { this.screenRect = new Rect((float)Screen.width * 0.01f, (float)Screen.height * 0.18f, (float)Screen.width * 0.6f, (float)Screen.height * 0.80f); }
            this.filtermenurect = new Rect(screenRect.xMax + (float)Screen.width * 0.01f, screenRect.y, (float)Screen.width * 0.37f, (float)Screen.height * 0.57f);
            
            this.itemButtonTexture = null;
            this.margins = new Vector4(12f, 12f, 12f, 12f + this.BOTTOM_MARGIN_EXTRA);
            this.outerRect = this.screenRect;
            this.innerBGRect = new Rect(this.outerRect.x + this.margins.x, this.outerRect.y + this.margins.y, this.outerRect.width - (this.margins.x + this.margins.z), this.outerRect.height - (this.margins.y + this.margins.w));
            float num = 0.005f * (float)Screen.width;
            this.innerRect = new Rect(this.innerBGRect.x + num, this.innerBGRect.y + num, this.innerBGRect.width - 2f * num, this.innerBGRect.height - 2f * num);
            float num2 = this.BOTTOM_MARGIN_EXTRA - 0.01f * (float)Screen.height;
            this.buttonLeftRect = new Rect(this.innerRect.x + this.innerRect.width * 0.03f, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.45f, num2);
            this.buttonRightRect = new Rect(this.innerRect.xMax - this.innerRect.width * 0.48f, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.45f, num2);

            this.wtsbuttonrect = new Rect(this.innerRect.x + this.innerRect.width * 0.03f, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.10f, num2);
            this.wtbbuttonrect = new Rect(wtsbuttonrect.xMax + num, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.10f, num2);
            this.updatebuttonrect = new Rect(this.innerRect.xMax - this.innerRect.width * 0.10f - this.innerRect.width * 0.03f, this.innerBGRect.yMax + num2 * 0.28f, this.innerRect.width * 0.10f, num2);

            num = (float)Screen.height / (float)Screen.width * 0.16f;
            this.fieldHeight = (this.innerRect.width - this.scrollBarSize) / (1f / num + 1f);
            this.costIconSize = this.fieldHeight;
            this.costIconWidth = this.fieldHeight / 1.1f;
            this.costIconHeight = this.costIconWidth * 72f / 73f;
            this.cardHeight = this.fieldHeight * 0.72f;
            this.cardWidth = this.cardHeight * 100f / 75f;
            this.labelX = this.cardWidth * 1.45f;
            this.labelsWidth = this.innerRect.width - this.labelX - 2 * this.costIconSize - this.scrollBarSize - this.costIconWidth;
            this.labelsWidth = this.labelsWidth /2.5f ;
            this.maxCharsName = (int)(this.labelsWidth / 12f);
            this.maxCharsRK = (int)(this.labelsWidth / 10f);

            float sbiconwidth = (filtermenurect.width - 2 * num2 - 6f * 4f) / 6f;
            float sbiconhight = costIconHeight;
            float chatheight = this.chatLogStyle.CalcHeight(new GUIContent("JScrollg"), 1000);
            float texthight = chatheight+2;//(filtermenurect.height - 3 * sbiconhight-7*4-2*num2)/3;

            this.sbarlabelrect = new Rect(filtermenurect.x + num2, filtermenurect.y + num2, filtermenurect.width * 0.2f, texthight);
            this.sbrect = new Rect(sbarlabelrect.xMax + num2, sbarlabelrect.y, filtermenurect.xMax - sbarlabelrect.xMax - 2 * num2, texthight);


            this.sbgrect = new Rect(sbarlabelrect.x, sbarlabelrect.yMax + 4, sbiconwidth, sbiconhight);
            this.sborect = new Rect(sbgrect.xMax + 4, sbgrect.y, sbiconwidth, sbiconhight);
            this.sberect = new Rect(sborect.xMax + 4, sbgrect.y, sbiconwidth, sbiconhight);
            this.sbdrect = new Rect(sberect.xMax + 4, sbgrect.y, sbiconwidth, sbiconhight);

            this.sbcommonrect = new Rect(sbarlabelrect.x, sbgrect.yMax + 4, sbiconwidth, sbiconhight);
            this.sbuncommonrect = new Rect(sbcommonrect.xMax + 4, sbcommonrect.y, sbiconwidth, sbiconhight);
            this.sbrarerect = new Rect(sbuncommonrect.xMax + 4, sbcommonrect.y, sbiconwidth, sbiconhight);
            this.sbthreerect = new Rect(sbdrect.xMax + 4, sbcommonrect.y, sbiconwidth, sbiconhight);
            this.sbonerect = new Rect(sbthreerect.xMax + 4, sbcommonrect.y, sbiconwidth, sbiconhight);

            this.sbsellerlabelrect = new Rect(sbarlabelrect.x, sbcommonrect.yMax + 4, sbrarerect.xMax-sbarlabelrect.x, texthight);
            this.sbsellerrect = new Rect(sbdrect.x, sbsellerlabelrect.y, filtermenurect.xMax - sbdrect.x - num2, texthight);

            this.sbpricerect = new Rect(sbgrect.x, sbsellerlabelrect.yMax + 4, sborect.xMax - sbgrect.x, texthight);
            this.sbpricelabelrect = new Rect(sberect.x, sbpricerect.y, sbdrect.xMax - sberect.x, texthight);
            this.sbpricerect2 = new Rect(sbthreerect.x, sbpricerect.y, sbrect.xMax - sbthreerect.x, texthight);

            this.sbtimelabel = new Rect(sbarlabelrect.x, sbpricelabelrect.yMax + 4, sbsellerlabelrect.width, texthight);
            this.sbtimerect = new Rect(sbsellerrect.x, sbtimelabel.y, sbsellerrect.width, texthight);

            this.sbtpfgen = new Rect(sbarlabelrect.x, sbtimelabel.yMax + 4, texthight, texthight);
            this.sbtpfgenlabel = new Rect(sbtpfgen.xMax + 4, sbtimelabel.yMax + 4, filtermenurect.width - sbtpfgen.width - num2, texthight);

            this.sbonlywithpricebox = new Rect(sbarlabelrect.x, sbtpfgen.yMax + 4, texthight, texthight);
            this.sbonlywithpricelabelbox = new Rect(sbonlywithpricebox.xMax + 4, sbtpfgen.yMax + 4, filtermenurect.width - sbonlywithpricebox.width - num2, texthight);

            this.sbclearrect = new Rect(filtermenurect.xMax - num2 - costIconWidth, filtermenurect.yMax - num2 - texthight, costIconWidth, texthight);
            this.sbclrearpricesbutton= new Rect(sbarlabelrect.x, sbclearrect.y, sbclearrect.x - sbarlabelrect.x - num2, texthight);
            this.sbgeneratebutton = new Rect(sbarlabelrect.x, sbclearrect.y - 4 - texthight, sbclearrect.x - sbarlabelrect.x - num2, texthight);

            GUI.skin = this.cardListPopupSkin;
            float smalltexthight = GUI.skin.label.CalcHeight(new GUIContent("LOL"),1000);
            this.sbnetworklabel = new Rect(filtermenurect.x+4, filtermenurect.yMax - smalltexthight-4, filtermenurect.width, smalltexthight);

            this.tradingbox = new Rect((float)Screen.width / 2f - (float)Screen.width * 0.15f, (float)Screen.height / 2f - (float)Screen.height * 0.15f, (float)Screen.width * 0.3f, (float)Screen.height * 0.3f);
            this.tradingbox = new Rect(innerRect);
            this.tradingbox.x = tradingbox.x + this.cardWidth;
            this.tradingbox.width = tradingbox.width - this.cardWidth - this.costIconWidth;
            this.tbok = new Rect(tradingbox.xMin + (float)Screen.height * 0.05f, tradingbox.yMax - (float)Screen.height * 0.05f, (float)Screen.width * 0.15f - 2*(float)Screen.height * 0.05f, (float)Screen.height * 0.05f-2f);
            this.tbcancel = new Rect(tradingbox.xMax - (float)Screen.width * 0.15f +  (float)Screen.height * 0.05f, tradingbox.yMax - (float)Screen.height * 0.05f, (float)Screen.width * 0.15f - 2 * (float)Screen.height * 0.05f, (float)Screen.height * 0.05f-2f);
            this.tbmessage = new Rect(this.tradingbox.x, this.tradingbox.y, this.tradingbox.width, (this.tradingbox.height - (float)Screen.height * 0.05f)/2f);
            this.tbmessagescroll = new Rect(this.tradingbox.x, this.tbmessage.yMax, this.tradingbox.width, (this.tradingbox.height - (float)Screen.height * 0.05f) / 2f);
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