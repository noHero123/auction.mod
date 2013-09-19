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
using System.Net;
using System.IO;

namespace Auction.mod
{


    using System;
    using UnityEngine;

	public struct aucitem
	{
		public Card card;
		public string seller;
		public string price;
		public int priceinint;
		public string time;
		public DateTime dtime;
		public string whole;
	}

    struct nickelement
    {
        public string nick;
        public string cardname;
    }


    public class Auction : BaseMod, ICommListener
    {

        private bool hidewispers = true; //  false = testmodus

        
       


        // some nicknames variables
        private bool nicks = false;
        private List<nickelement> loadedscrollsnicks = new List<nickelement>();
         // = realcardnames + loadedscrollsnicks

        

        private bool wtsinah = true;
        private bool wtsingen = true;
        private string ownaucpath = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "auc" + System.IO.Path.DirectorySeparatorChar;
        private bool wtsmsgload = false;
        private bool wtbmsgload = false;
        private string[] aucfiles;
        Color dblack = new Color(1f, 1f, 1f, 0.5f);
        private bool wtsmenue=false;
        private bool selectable;
        private bool clickableItems;
        private Store storeinfo;
        private bool settings=false;
        private bool generator=false;
        private bool inauchouse=false;
        private string ownname;
        private string ownid;

        Vector2 scrolll = new Vector2(0, 0);

        private int longestcardname;
        private GUISkin lobbySkin;
        private List<aucitem> ahlist = new List<aucitem>();
        private List<aucitem> ahlistfull = new List<aucitem>();
        private List<aucitem> wtsPlayer = new List<aucitem>();
        private List<aucitem> orgicardsPlayerwountrade = new List<aucitem>(); // cards player owns minus the untradable cards
        private List<Card> orgicardsPlayer = new List<Card>(); // all cards the player owns
        private List<aucitem> wtbPlayer = new List<aucitem>();
        //settings
        private bool shownumberscrolls;
        private string rowscalestring = "10";
        private float rowscale = 1.0f;
        private bool showsugrange;
        private bool wtsroundup = true; private int wtsroundmode = 0; private bool roundwts = false;
        private bool wtbroundup = false; private int wtbroundmode = 0; private bool roundwtb = false;
        private int takewtsgenint = 2, takewtbgenint = 0;
        private int takewtsahint = 1, takewtbahint = 1, takewtsahint2 = 1, takewtbahint2 = 1;

        


        //filter
        


        private bool showtradedialog = false; private aucitem tradeitem;

        //cardlistpopup#######################
        private float opacity;
        public Vector2 scrollPos;
        

        
        
        
        

        
        private GUISkin cardListPopupSkin ;
        private GUISkin cardListPopupGradientSkin ;
        private GUISkin cardListPopupBigLabelSkin ;
        private GUISkin cardListPopupLeftButtonSkin;

        int screenh = 0;
        int screenw = 0;
        
        private const bool debug = false;

        private MethodInfo hideInformationinfo;
        private FieldInfo showBuyinfo;
        private FieldInfo showSellinfo;
        private FieldInfo chatRoomsinfo;
        private FieldInfo chatLogStyleinfo;
        private FieldInfo targetchathightinfo;
        private MethodInfo drawsubmenu;

        private ChatUI target = null;
        private ChatRooms chatRooms;
        private GUIStyle chatLogStyle;
        
        
        private Regex userRegex;
        private Regex linkFinder;
        private Regex cardlinkfinder;

        private Dictionary<string, ChatUser> globalusers = new Dictionary<string, ChatUser>();
        private int[] cardids;
        private string[] cardnames;
        private int[] cardImageid;
        private string[] cardType;

        

        Texture2D growthres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_growth");
        Texture2D energyres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_energy");
        Texture2D orderres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_order");
        Texture2D decayres = ResourceManager.LoadTexture("BattleUI/battlegui_icon_decay");
        Texture2D arrowdown = ResourceManager.LoadTexture("ChatUI/dropdown_arrow");
        FieldInfo buymen; FieldInfo sellmen;

        string postmsgmsg = "";
        bool postmsgontrading = false;
        bool postmsggetnextroomenter = false;

        bool chatisshown = false;
        bool deckchanged = false;

        string[] auccontroler = new string[] { };

        Network ntwrk;
        Settings sttngs;
        cardviewer crdvwr;
        Prices prcs;
        listfilters auclsts;
        messageparser mssgprsr;
        Rectomat recto;

        private int cardnametoimageid(string name) { return cardImageid[Array.FindIndex(cardnames, element => element.Equals(name))]; }

        public void handleMessage(Message msg)
        {

            if (msg is BuyStoreItemResponseMessage)
            {
                List<Card> boughtCards = null;
                BuyStoreItemResponseMessage buyStoreItemResponseMessage = (BuyStoreItemResponseMessage)msg;
                if (buyStoreItemResponseMessage.cards.Length > 0)
                {
                    boughtCards = new List<Card>(buyStoreItemResponseMessage.cards);
                }
                else
                {
                    boughtCards = null;
                }
                DeckInfo deckInfo = buyStoreItemResponseMessage.deckInfo;
                if (boughtCards != null)
                {
                    deckchanged = true;
                }
                else
                {
                    if (deckInfo != null)
                    {
                        deckchanged = true; 
                    }
                }
            }



            if (msg is TradeResponseMessage)
            {
                TradeResponseMessage trm = (TradeResponseMessage)msg;
                if (trm.status != "ACCEPT")
                {
                    this.postmsgontrading = false;
                    this.postmsggetnextroomenter = false;
                    this.postmsgmsg = "";
                }
            }


            if (msg is RoomEnterMessage && this.postmsggetnextroomenter)
            {
                RoomEnterMessage rmem = (RoomEnterMessage)msg;
                if (rmem.roomName.StartsWith("trade-"))
                {
                    this.postmsggetnextroomenter = false;
                    // post the msg here!:
                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(rmem.roomName, "<color=#777460>" + postmsgmsg + "</color>");
                    joinmessage.from = "Scrolls";

                    //App.ChatUI.handleMessage(new RoomChatMessageMessage(rmem.roomName, "<color=#777460>" + postmsgmsg + "</color>"));
                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);
                    this.postmsgmsg = "";
                }
            }

            if (msg is GameInfoMessage && ntwrk.contonetwork)
            {
                GameInfoMessage gim =(GameInfoMessage) msg;
                if (ntwrk.inbattle == false) { ntwrk.inbattle = true; ntwrk.disconfromaucnet(); Console.WriteLine("discon"); }
            }

            if (msg is RoomInfoMessage && ntwrk.contonetwork)
            {
                
                RoomInfoMessage roominfo = (RoomInfoMessage)msg;
                
                if (roominfo.roomName.StartsWith("auc-"))
                {
                    ntwrk.enteraucroom(roominfo);
                }
            
            }

            if ( msg is FailMessage)
            {   // delete user if he cant be whispered ( so he doesnt check out... blame on him!)
                FailMessage fm = (FailMessage)msg;
                if (ntwrk.idtesting > 0)
                {

                    if (fm.op == "ProfilePageInfo") { ntwrk.idtesting--; };
                }
                if (fm.op == "Whisper" && fm.info.StartsWith("Could not find the user "))
                {
                    string name = "";
                    name = (fm.info).Split('\'')[1];
                    //Console.WriteLine("could not find: " + name);

                }
            }

            if (ntwrk.idtesting > 0 && msg is ProfilePageInfoMessage)//doesnt needed anymore
            {
                ProfilePageInfoMessage ppim = (ProfilePageInfoMessage)msg;
                ChatUser newuser = new ChatUser();
                newuser.acceptChallenges = false;
                newuser.acceptTrades = true;
                newuser.adminRole = AdminRole.None;
                newuser.name = ppim.name;
                newuser.id = ppim.id;
                if (!this.globalusers.ContainsKey(newuser.name)) { this.globalusers.Add(newuser.name, newuser); ntwrk.addglobalusers(newuser); }
                ntwrk.adduser(newuser);

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
                prcs.lowerprice = new int[d.GetLength(0)];
                prcs.upperprice = new int[d.GetLength(0)];
                prcs.sugprice = new int[d.GetLength(0)];
                for (int i = 0; i < d.GetLength(0); i++)
                {
                    cardids[i] = Convert.ToInt32(d[i]["id"]);
                    cardnames[i] = d[i]["name"].ToString().ToLower();
                    cardImageid[i] = Convert.ToInt32(d[i]["cardImage"]);
                    cardType[i] = d[i]["kind"].ToString();
                    if (cardnames[i].Split(' ').Length > longestcardname) { longestcardname = cardnames[i].Split(' ').Length; };
                }
                mssgprsr.setarrays(cardids,cardnames);
                if (this.nicks) readnicksfromfile();
                mssgprsr.searchscrollsnicks.Clear();
                prcs.wtbpricelist1.Clear();
                auclsts.allcardsavailable.Clear();
                for (int j = 0; j < cardnames.Length; j++)
                {
                    prcs.wtbpricelist1.Add(cardnames[j].ToLower(), "");
                    CardType type = CardTypeManager.getInstance().get(this.cardids[j]);
                    Card card = new Card(cardids[j], type, true);
                    aucitem ai = new aucitem();
                    ai.card = card;
                    ai.price = "";
                    ai.priceinint = auclsts.allcardsavailable.Count;
                    ai.seller="me";
                    auclsts.allcardsavailable.Add(ai);
                    nickelement nele;
                    nele.nick = cardnames[j];
                    nele.cardname = cardnames[j];
                    mssgprsr.searchscrollsnicks.Add(nele);
                };
                mssgprsr.searchscrollsnicks.AddRange(this.loadedscrollsnicks);

                auclsts.allcardsavailable.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });
                //test
                //foreach (aucitem ai in allcardsavailable)
                //{ Console.WriteLine(ai.card.getName()); }
                //App.Communicator.removeListener(this);//dont need the listener anymore
                prcs.totalpricecheck(this.cardids);
            }

            return;
        }
        public void onReconnect()
        {
            return; // don't care
        }

        public Auction()
        {
            ntwrk = new Network();
            sttngs = new Settings();
            crdvwr = new cardviewer();
            prcs = new Prices();
            auclsts = new listfilters(sttngs, prcs);
            recto = new Rectomat();
            mssgprsr = new messageparser(auclsts, this.cardids, this.cardnames);

            cardlinkfinder = new Regex(@"\[[a-zA-Z]+[a-zA-Z_\t]*[a-zA-z]+\]");//search for "[blub_blub_blub]"

            
            hideInformationinfo = typeof(Store).GetMethod("hideInformation", BindingFlags.Instance | BindingFlags.NonPublic);
            showBuyinfo = typeof(Store).GetField("showBuy", BindingFlags.Instance | BindingFlags.NonPublic);
            showSellinfo = typeof(Store).GetField("showSell", BindingFlags.Instance | BindingFlags.NonPublic);

            drawsubmenu = typeof(Store).GetMethod("drawSubMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            chatRoomsinfo = typeof(ChatUI).GetField("chatRooms", BindingFlags.Instance | BindingFlags.NonPublic);
            chatLogStyleinfo = typeof(ChatUI).GetField("chatMsgStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            targetchathightinfo = typeof(ChatUI).GetField("targetChatHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            
            this.cardListPopupSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopup");
            this.cardListPopupGradientSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopupGradient");
            this.cardListPopupBigLabelSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel");
            this.cardListPopupLeftButtonSkin = (GUISkin)Resources.Load("_GUISkins/CardListPopupLeftButton");

            buymen = typeof(Store).GetField("buyMenuObj", BindingFlags.Instance | BindingFlags.NonPublic);
            sellmen = typeof(Store).GetField("sellMenuObj", BindingFlags.Instance | BindingFlags.NonPublic);

            
            sttngs.saveall();
            Directory.CreateDirectory(this.ownaucpath);
            this.aucfiles = Directory.GetFiles(this.ownaucpath, "*auc.txt");
            if (aucfiles.Contains(this.ownaucpath +  "wtsauc.txt"))//File.Exists() was slower
            {
                this.wtsmsgload = true;
            }
            if (aucfiles.Contains(this.ownaucpath + "wtbauc.txt"))//File.Exists() was slower
            {
                this.wtbmsgload = true;
            }
            if (aucfiles.Contains(this.ownaucpath + "nicauc.txt"))//File.Exists() was slower
            {
                this.nicks = true;
            }

            if (aucfiles.Contains(this.ownaucpath + "settingsauc.txt"))//File.Exists() was slower
            {
                loadsettings();
            }



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
                     scrollsTypes["TradeSystem"].Methods.GetMethod("StartTrade", new Type[]{typeof(List<Card>) , typeof(List<Card>), typeof(string), typeof(string), typeof(int)}),
                     scrollsTypes["EndGameScreen"].Methods.GetMethod("GoToLobby")[0],
                     
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
                if (this.inauchouse || this.generator || this.settings) return true;
            }
            
            if (info.target is BattleMode && info.targetMethod.Equals("_handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
                    if ((wmsg.text).StartsWith("aucdeletes") || (wmsg.text).StartsWith("aucdeleteb") || (wmsg.text).StartsWith("aucupdate") || (wmsg.text).StartsWith("aucto1please") || (wmsg.text).StartsWith("aucstay? ") || (wmsg.text).StartsWith("aucstay! ") || (wmsg.text).StartsWith("aucrooms ") || (wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucs ") || (wmsg.text).StartsWith("aucb ") || (wmsg.text).StartsWith("needaucid") || (wmsg.text).StartsWith("aucid ")) return true;
                }
            }
            
            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
                    if (hidewispers)
                    { // hides all whisper messages from auc-mod
                        if ((wmsg.text).StartsWith("aucdeletes") || (wmsg.text).StartsWith("aucdeleteb") || (wmsg.text).StartsWith("aucupdate") || (wmsg.text).StartsWith("aucto1please") || (wmsg.text).StartsWith("aucstay? ") || (wmsg.text).StartsWith("aucstay! ") || (wmsg.text).StartsWith("aucrooms ") || (wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucs ") || (wmsg.text).StartsWith("aucb ") || (wmsg.text).StartsWith("needaucid") || (wmsg.text).StartsWith("aucid ")) return true;
                    }
                    else
                    {// show some whispers if not connected (testmode)
                        if (ntwrk.contonetwork)
                        {

                            if ((wmsg.text).StartsWith("aucdeletes") || (wmsg.text).StartsWith("aucdeleteb") || (wmsg.text).StartsWith("aucupdate") || (wmsg.text).StartsWith("aucto1please") || (wmsg.text).StartsWith("aucstay? ") || (wmsg.text).StartsWith("aucstay! ") || (wmsg.text).StartsWith("aucrooms ") || (wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucs ") || (wmsg.text).StartsWith("aucb ") || (wmsg.text).StartsWith("needaucid") || (wmsg.text).StartsWith("aucid ")) return true;
                        }
                        else
                        {
                            if ((wmsg.text).StartsWith("aucstop") || (wmsg.text).StartsWith("aucto1please")) return true;
                        }
                    }
                }
                if (msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage rem = (RoomChatMessageMessage)msg;
                    if (ntwrk.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }

                if (msg is RoomEnterMessage)
                {   
                    RoomEnterMessage rem = (RoomEnterMessage) msg;
                    if (ntwrk.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }

                if (msg is RoomInfoMessage)
                {
                    RoomInfoMessage rem = (RoomInfoMessage)msg;
                    if (ntwrk.contonetwork && rem.roomName.StartsWith("auc-")) return true;
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
            returnValue = null;
            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
                    string text = wmsg.text;

                    if (text.StartsWith("aucdeletes"))
                    {

                            mssgprsr.wtslistfulltimed.RemoveAll(element => element.seller == wmsg.from);
                            mssgprsr.wtslistfull.RemoveAll(element => element.seller == wmsg.from);
                            mssgprsr.wtslist.RemoveAll(element => element.seller == wmsg.from);


                        

                    }
                    if (text.StartsWith("aucdeleteb"))
                    {

                        mssgprsr.wtblistfulltimed.RemoveAll(element => element.seller == wmsg.from);
                        mssgprsr.wtblistfull.RemoveAll(element => element.seller == wmsg.from);
                        mssgprsr.wtblist.RemoveAll(element => element.seller == wmsg.from);
                    

                    }

                    if (text.StartsWith("aucs ") || text.StartsWith("aucb "))
                    {
                        mssgprsr.getaucitemsformmsg(text, wmsg.from, wmsg.GetChatroomName(),this.generator,this.inauchouse,this.settings,this.wtsmenue);
                        //need playerid (wispering doesnt send it)
                        if (!this.globalusers.ContainsKey(wmsg.from)) { WhisperMessage needid = new WhisperMessage(wmsg.from, "needaucid"); App.Communicator.sendRequest(needid); }
                    }

                    if(wmsg.from==App.MyProfile.ProfileInfo.name)  return;

                    if (text.StartsWith("aucto1please") && ntwrk.contonetwork)
                    {
                        App.Communicator.sendRequest(new RoomExitMessage("auc-" + ntwrk.ownroomnumber));
                        ntwrk.ownroomnumber = 0;
                        App.Communicator.sendRequest(new RoomEnterMessage("auc-1"));
                        Console.WriteLine("aucto1please");
                    
                    }

                    if (text.StartsWith("aucstay? ") && ntwrk.contonetwork)
                    {   // user founded a room, but dont know if this is all

                        ntwrk.aucstayquestion(text, wmsg.from, sttngs.shortgeneratedwtsmessage, sttngs.shortgeneratedwtbmessage);

                    
                    }

                    if (text.StartsWith("aucstay! "))
                    {   // user founded a room, and he dont want to get the room-list
                        ntwrk.aucstay(text, wmsg.from, sttngs.shortgeneratedwtsmessage, sttngs.shortgeneratedwtbmessage);
                    }

                    if (text.StartsWith("aucrooms ") && !ntwrk.rooomsearched && ntwrk.contonetwork)
                    {
                        if (text.EndsWith("aucrooms ")) { ntwrk.realycontonetwork = true; }
                        else
                        {
                            ntwrk.visitrooms(text);
                            
                        }
                    }
                    
                    if (text.StartsWith("aucstop"))
                    {
                        ntwrk.deleteuser(wmsg.from);
                    }

                    

                    if (text.StartsWith("aucupdate"))  
                    {
                        ntwrk.sendownauctionstosingleuser(sttngs.shortgeneratedwtsmessage, sttngs.shortgeneratedwtbmessage);
                    }
                    

                    
                    //dont needed anymore left in only to be shure :D
                    if (text.StartsWith("needaucid"))
                    {
                        ntwrk.needid(wmsg.from);
                    }
                     //dont needed anymore
                    if (text.StartsWith("aucid "))
                    {
                        ntwrk.saveaucid(text,wmsg.from);
                        
                        
                    }


                }
            }

            
            

        }

        public override void BeforeInvoke(InvocationInfo info)
        {

            return;
        }



        private void readnicksfromfile()
        {
            if (this.nicks)
            {
                string[] lines = System.IO.File.ReadAllLines(this.ownaucpath + "nicauc.txt");
                foreach (string s in lines)
                {
                    if (s == "" || s == " ") continue;
                    string cardname = s.Split(':')[0];
                    if (cardnames.Contains(cardname.ToLower()))
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

        private void loadsettings()
        {

            string text = System.IO.File.ReadAllText(this.ownaucpath + "settingsauc.txt");
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
                    mssgprsr.spampreventtime = value;
                    if (mssgprsr.spampreventtime != "") mssgprsr.spamprevint = Convert.ToInt32(mssgprsr.spampreventtime);
                    if (mssgprsr.spamprevint > 30) { mssgprsr.spampreventtime = "30"; mssgprsr.spamprevint = 30; }
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

        private void generatewtxmsg(List<aucitem> liste)
        {
            string msg = "";
            string shortmsg = "";
            List<aucitem> postlist = new List<aucitem>();
            for (int i = 0; i < liste.Count; i++)
            {
                if (this.wtsmenue)
                {
                    if (prcs.wtspricelist1[liste[i].card.getName().ToLower()] != "")
                    {
                        aucitem ai = liste[i];
                        ai.price = prcs.wtspricelist1[liste[i].card.getName().ToLower()];
                        ai.priceinint = Convert.ToInt32(ai.price);
                        postlist.Add(ai);
                        //msg = msg + liste[i].card.getName() + " " + this.wtspricelist1[liste[i].card.getName().ToLower()] + ";";
                        //shortmsg = shortmsg + liste[i].card.getType() + " " + this.wtspricelist1[liste[i].card.getName().ToLower()] + ";"; 
                    }
                }
                else
                {
                    if (prcs.wtbpricelist1[liste[i].card.getName().ToLower()] != "")
                    {
                        aucitem ai = liste[i];
                        ai.price = prcs.wtbpricelist1[liste[i].card.getName().ToLower()];
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

            for (int i = 0; i < postlist.Count; i++)
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
                if (this.wtsmenue) { msg = "WTS " + msg; shortmsg = "aucs " + shortmsg; } else { msg = "WTB " + msg; shortmsg = "aucb " + shortmsg; }
                msg = msg.Remove(msg.Length - 2);
                shortmsg = shortmsg.Remove(shortmsg.Length - 1);
            }
            if (msg.Length >= 512) { msg = "msg to long"; }
            if (shortmsg.Length >= 512) { shortmsg = ""; msg = msg + ", networkmsg too"; }
            if (this.wtsmenue) { sttngs.generatedwtsmessage = msg; sttngs.shortgeneratedwtsmessage = shortmsg; } else { sttngs.generatedwtbmessage = msg; sttngs.shortgeneratedwtbmessage = shortmsg; }
            //Console.WriteLine(msg);
            //Console.WriteLine(shortmsg);
            sttngs.sellersearchstring = msg;
            sttngs.pricesearchstring = shortmsg;

        }

        
       
      

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            if (info.target is EndGameScreen && info.targetMethod.Equals("GoToLobby")) { ntwrk.inbattle = false; }
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

            if (info.target is TradeSystem && info.targetMethod.Equals("StartTrade"))
            {
                if (this.postmsgontrading == true)
                {
                    this.postmsgontrading = false;
                    this.postmsggetnextroomenter = true;// the next RoomEnterMsg is the tradeRoom!
                }
            }

            if (info.target is Store && info.targetMethod.Equals("Start"))
            {
                this.lobbySkin = (GUISkin)typeof(Store).GetField("lobbySkin", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                this.storeinfo=(Store)info.target;
                this.showtradedialog = false;
                this.inauchouse = false;
                this.generator = false;
                this.settings = false;
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
                    if (!globalusers.ContainsKey(user.name)) { globalusers.Add(user.name, user); ntwrk.addglobalusers(user); };
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
                        auclsts.available.Clear();
                        foreach (aucitem ai in auclsts.allcardsavailable)
                        {
                            if (!auclsts.available.ContainsKey(ai.card.getName()))
                            {
                                auclsts.available.Add(ai.card.getName(), 0);
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


                            auclsts.available[c.getName()] = auclsts.available[c.getName()] + 1;
                        }

                        this.orgicardsPlayerwountrade.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });

                        prcs.wtspricelist1.Clear();
                        for (int i = 0; i < orgicardsPlayerwountrade.Count; i++)
                        {
                            prcs.wtspricelist1.Add(orgicardsPlayerwountrade[i].card.getName().ToLower(), "");

                        }


                        if (this.generator && this.wtsmenue)
                        {
                            auclsts.fullupdatelist(ahlist, ahlistfull,this.inauchouse,this.wtsmenue,this.generator);
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
                        recto.setupPositions(this.chatisshown,this.rowscale,this.chatLogStyle,this.cardListPopupSkin);
                        recto.setupsettingpositions(this.chatLogStyle, this.cardListPopupLeftButtonSkin);

                    }
                   
                    
                    // delete picture on click!
                    if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && crdvwr.clicked >= 3) { crdvwr.clearallpics(); }

                    //auction house...
                    GUIPositioner subMenuPositioner = App.LobbyMenu.getSubMenuPositioner(1f, 5);
                    //klick button AH
                    if (LobbyMenu.drawButton(subMenuPositioner.getButtonRect(2f), "AH", this.lobbySkin) && !this.showtradedialog)
                    {
                        if (this.deckchanged)
                        { App.Communicator.sendRequest(new LibraryViewMessage()); this.deckchanged = false; }
                        this.inauchouse = true;
                        this.settings = false;
                        this.generator = false;
                        //this.hideInformation();
                        hideInformationinfo.Invoke(storeinfo, null);

                        
                        iTween.MoveTo((GameObject)buymen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        showBuyinfo.SetValue(storeinfo, false);

                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(false);
                        iTween.MoveTo((GameObject)sellmen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(true);
                        showSellinfo.SetValue(storeinfo, false);
                        
                        Store.ENABLE_SHARD_PURCHASES = false;
                        
                        
                        this.clickableItems = false;

                        this.selectable = true;
                        if (this.wtsinah)
                        {
                            mssgprsr.wtslistfull.Clear(); mssgprsr.wtslistfull.AddRange(mssgprsr.wtslistfulltimed);

                            auclsts.sortlist(mssgprsr.wtslistfull);

                            this.ahlist = mssgprsr.wtslist; this.ahlistfull = mssgprsr.wtslistfull; this.wtsmenue = true;

                            sttngs.setsettings(true, true);
                        }
                        else 
                        {
                            mssgprsr.wtblistfull.Clear(); mssgprsr.wtblistfull.AddRange(mssgprsr.wtblistfulltimed);

                            auclsts.sortlist(mssgprsr.wtblistfull);

                            this.ahlist = mssgprsr.wtblist; this.ahlistfull = mssgprsr.wtblistfull; this.wtsmenue = false;
                            sttngs.setsettings(true, false);
                        }
                        auclsts.fullupdatelist(ahlist, ahlistfull,this.inauchouse,this.wtsmenue,this.generator);
                        this.targetchathightinfo.SetValue(this.target, (float)Screen.height * 0.25f);
                        if (this.wtsmenue) { mssgprsr.newwtsmsgs = false; } else { mssgprsr.newwtbmsgs = false; }
                    }
                // klick button Gen

                    if (LobbyMenu.drawButton(subMenuPositioner.getButtonRect(3f), "Gen", this.lobbySkin) && !this.showtradedialog)
                    {
                        if (this.deckchanged)
                        { App.Communicator.sendRequest(new LibraryViewMessage()); this.deckchanged = false; }
                        //this.hideInformation();
                        hideInformationinfo.Invoke(storeinfo, null);
                        iTween.MoveTo((GameObject)buymen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        showBuyinfo.SetValue(storeinfo, false);
                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(false);
                        iTween.MoveTo((GameObject)sellmen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(true);
                        showSellinfo.SetValue(storeinfo, false);
                        Store.ENABLE_SHARD_PURCHASES = false;
                        this.inauchouse = false;
                        this.generator = true;
                        this.settings = false;

                        this.clickableItems = false;
                        this.selectable = true;

                        if (this.wtsingen)
                        {
                            this.ahlist = this.wtsPlayer; this.ahlistfull = this.orgicardsPlayerwountrade; this.wtsmenue = true;
                            sttngs.setsettings(false,true);
                        }
                        else
                        {
                            this.ahlist = this.wtbPlayer; this.ahlistfull = auclsts.allcardsavailable; this.wtsmenue = false;
                            sttngs.setsettings(false, false);
                        }

                        //this.genlist.AddRange(this.genlistfull);


                        auclsts.fullupdatelist(ahlist, ahlistfull,this.inauchouse,this.wtsmenue,this.generator);
                        this.targetchathightinfo.SetValue(this.target, (float)Screen.height * 0.25f);
                    }
                    Rect setrecto = subMenuPositioner.getButtonRect(4f);
                    setrecto.x = Screen.width - setrecto.width;// -subMenuPositioner.getButtonRect(0f).x;
                    if (LobbyMenu.drawButton(setrecto, "settings", this.lobbySkin) && !this.showtradedialog)
                    {
                        //this.hideInformation();
                        hideInformationinfo.Invoke(storeinfo, null);
                        iTween.MoveTo((GameObject)buymen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        showBuyinfo.SetValue(storeinfo, false);
                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(false);
                        iTween.MoveTo((GameObject)sellmen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(true);
                        showSellinfo.SetValue(storeinfo, false);
                        Store.ENABLE_SHARD_PURCHASES = false;
                        this.inauchouse = false;
                        this.generator = false;
                        this.settings = true;
                        this.targetchathightinfo.SetValue(this.target, (float)Screen.height * 0.25f);
                    }    


                    // draw ah oder gen-menu

                    if (this.inauchouse) drawAH();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;
                    if (this.generator) drawgenerator();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;
                    if (this.settings) drawsettings();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;

                    crdvwr.draw();
                    
                
            }
            else if (info.target is Store && (info.targetMethod.Equals("showBuyMenu") || info.targetMethod.Equals("showSellMenu")))
            {
                //disable auc.house + generator
                Store.ENABLE_SHARD_PURCHASES = true;
                inauchouse = false;
                generator = false;
                this.settings = false;
                this.showtradedialog=false;
                if (info.targetMethod.Equals("showSellMenu")) { this.deckchanged = false; }

            }

            if (info.target is ChatRooms && info.targetMethod.Equals("ChatMessage"))
            {
                //get trademessages from chatmessages
                RoomChatMessageMessage msg = (RoomChatMessageMessage)info.arguments[0];
                if (msg.from != "Scrolls")
                {
                    mssgprsr.getaucitemsformmsg(msg.text, msg.from, msg.roomName,this.generator,this.inauchouse,this.settings, this.wtsmenue);
                }
            }

            return;
        }

        //draw stuff

        private void drawAH()
        {
            // have to draw textfield in front of scrollbar or otherwise you lose focus in textfield (lol)

            if (this.inauchouse)
            {

                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.filtermenurect, string.Empty);
                // wts filter menue
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(recto.sbarlabelrect, "Scroll:");
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.sbrect, string.Empty);
                string selfcopy = sttngs.wtssearchstring;
                sttngs.wtssearchstring = GUI.TextField(recto.sbrect, sttngs.wtssearchstring, chatLogStyle);


                GUI.contentColor = Color.white;
                GUI.color = Color.white;
                if (!sttngs.growthbool) { GUI.color = dblack; }
                bool growthclick = GUI.Button(recto.sbgrect, growthres);
                GUI.color = Color.white;
                if (!sttngs.orderbool) { GUI.color = dblack; }
                bool orderclick = GUI.Button(recto.sborect, orderres);
                GUI.color = Color.white;
                if (!sttngs.energybool) { GUI.color = dblack; }
                bool energyclick = GUI.Button(recto.sberect, energyres);
                GUI.color = Color.white;
                if (!sttngs.decaybool) { GUI.color = dblack; }
                bool decayclick = GUI.Button(recto.sbdrect, decayres);


                GUI.contentColor = Color.gray;
                GUI.color = Color.white;
                if (!sttngs.commonbool) { GUI.color = dblack; }
                bool commonclick = GUI.Button(recto.sbcommonrect, "C");
                GUI.color = Color.white;
                if (!sttngs.uncommonbool) { GUI.color = dblack; }
                GUI.contentColor = Color.white;
                bool uncommonclick = GUI.Button(recto.sbuncommonrect, "U");
                GUI.color = Color.white;
                if (!sttngs.rarebool) { GUI.color = dblack; }
                GUI.contentColor = Color.yellow;
                bool rareclick = GUI.Button(recto.sbrarerect, "R");
                GUI.color = Color.white;
                if (!sttngs.threebool) { GUI.color = dblack; }
                //if (!p1mt3bool) { GUI.color = dblack; }
                bool mt3click;
                bool mt0click = false;
                if (this.wtsmenue)
                {
                    mt3click = GUI.Button(recto.sbthreerect, "<3"); // >3 bei wtsmenue=false
                }
                else { mt3click = GUI.Button(recto.sbthreerect, ">3"); }
                GUI.color = Color.white;
                if (!sttngs.onebool) { GUI.color = dblack; }
                if (!this.wtsmenue) { mt0click = GUI.Button(recto.sbonerect, ">0"); };
                GUI.color = Color.white;
                GUI.contentColor = Color.white;

                // draw seller filter
                GUI.skin = this.cardListPopupBigLabelSkin;
                if (this.wtsmenue)
                {
                    GUI.Label(recto.sbsellerlabelrect, "ignore Seller:");
                }
                else { GUI.Label(recto.sbsellerlabelrect, "ignore Buyer:"); }

                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.sbsellerrect, string.Empty);
                string sellercopy = sttngs.sellersearchstring;
                GUI.SetNextControlName("sellerframe");
                sttngs.sellersearchstring = GUI.TextField(recto.sbsellerrect, sttngs.sellersearchstring, chatLogStyle);

                // draw price filter


                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(recto.sbpricelabelrect, "<= Price <=");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;


                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.sbpricerect, string.Empty);
                GUI.Box(recto.sbpricerect2, string.Empty);
                string pricecopy = sttngs.pricesearchstring;
                string pricecopy2 = sttngs.pricesearchstring2;
                sttngs.pricesearchstring = Regex.Replace(GUI.TextField(recto.sbpricerect, sttngs.pricesearchstring, chatLogStyle), @"[^0-9]", "");
                sttngs.pricesearchstring2 = Regex.Replace(GUI.TextField(recto.sbpricerect2, sttngs.pricesearchstring2, chatLogStyle), @"[^0-9]", "");
                GUI.color = Color.white;

                // draw time filter

                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(recto.sbtimelabel, "not older than");
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.sbtimerect, string.Empty);
                string timecopy = sttngs.timesearchstring;
                sttngs.timesearchstring = Regex.Replace(GUI.TextField(recto.sbtimerect, sttngs.timesearchstring, 2, chatLogStyle), @"[^0-9]", "");
                if (sttngs.timesearchstring != "" && Convert.ToInt32(sttngs.timesearchstring) > 30) { sttngs.timesearchstring = "30"; }
                GUI.color = Color.white;


                bool tpfgen = GUI.Button(recto.sbtpfgen, "");
                if (sttngs.takepriceformgenarator)
                {
                    GUI.DrawTexture(recto.sbtpfgen, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
                }
                else
                {
                    GUI.DrawTexture(recto.sbtpfgen, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
                }
                GUI.skin = this.cardListPopupBigLabelSkin;
                if (this.wtsmenue)
                {
                    GUI.Label(recto.sbtpfgenlabel, "Price <= wtb-generator");
                }
                else { GUI.Label(recto.sbtpfgenlabel, "Price >= wts-generator"); }


                // only scrolls with price
                bool owp = GUI.Button(recto.sbonlywithpricebox, "");
                if (sttngs.ignore0)
                {
                    GUI.DrawTexture(recto.sbonlywithpricebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
                }
                else
                {
                    GUI.DrawTexture(recto.sbonlywithpricebox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
                }
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(recto.sbonlywithpricelabelbox, "only Scrolls with Price");

                GUI.skin = this.cardListPopupSkin;

                if (ntwrk.contonetwork)
                {
                    GUI.Label(recto.sbnetworklabel, "User online: " + ntwrk.getnumberofaucusers());
                }

                GUI.contentColor = Color.red;
                bool closeclick = GUI.Button(recto.sbclearrect, "X");
                GUI.contentColor = Color.white;

                if (growthclick) { sttngs.growthbool = !sttngs.growthbool; };
                if (orderclick) { sttngs.orderbool = !sttngs.orderbool; }
                if (energyclick) { sttngs.energybool = !sttngs.energybool; };
                if (decayclick) { sttngs.decaybool = !sttngs.decaybool; }
                if (commonclick) { sttngs.commonbool = !sttngs.commonbool; };
                if (uncommonclick) { sttngs.uncommonbool = !sttngs.uncommonbool; }
                if (rareclick) { sttngs.rarebool = !sttngs.rarebool; };
                if (mt3click) { sttngs.threebool = !sttngs.threebool; }
                if (mt0click) { sttngs.onebool = !sttngs.onebool; }
                if (owp) { sttngs.ignore0 = !sttngs.ignore0; }
                if (tpfgen) { sttngs.takepriceformgenarator = !sttngs.takepriceformgenarator; }
                if (closeclick)
                {
                    sttngs.resetsearchsettings();
                }

                if (this.wtsmenue) { sttngs.savesettings(true, true); } else { sttngs.savesettings(true, false); }

                bool pricecheck = false;
                //if (wtsmenue) { pricecheck = (pricecopy2.Length < this.pricesearchstring2.Length) || (pricecopy2.Length != this.pricesearchstring2.Length && pricesearchstring2 == "") || (tpfgen); } else { pricecheck = pricecopy.Length > this.pricesearchstring.Length || (tpfgen); }

                pricecheck = (pricecopy2.Length < sttngs.pricesearchstring2.Length) || (pricecopy2.Length != sttngs.pricesearchstring2.Length && sttngs.pricesearchstring2 == "") || (tpfgen) || pricecopy.Length > sttngs.pricesearchstring.Length || (tpfgen);

                //clear p1moddedlist only if necessary
                //if (selfcopy.Length > this.wtssearchstring.Length || (owp&&!ignore0)|| sellercopy.Length > this.sellersearchstring.Length || pricecheck || closeclick || (growthclick && growthbool) || (orderclick && orderbool) || (energyclick && energybool) || (decayclick && decaybool) || (commonclick && commonbool) || (uncommonclick && uncommonbool) || (rareclick && rarebool) || mt3click || mt0click)
                if (selfcopy.Length > sttngs.wtssearchstring.Length || (owp && !sttngs.ignore0) || sellercopy.Length > sttngs.sellersearchstring.Length || pricecheck || closeclick || (growthclick && sttngs.growthbool) || (orderclick && sttngs.orderbool) || (energyclick && sttngs.energybool) || (decayclick && sttngs.decaybool) || (commonclick && sttngs.commonbool) || (uncommonclick && sttngs.uncommonbool) || (rareclick && sttngs.rarebool) || mt3click || mt0click)
                {
                    //Console.WriteLine("delete dings####");
                    auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);

                }
                else
                {

                    if (selfcopy != sttngs.wtssearchstring)
                    {

                        if (sttngs.wtssearchstring != "")
                        {
                            auclsts.containsname(sttngs.wtssearchstring, ahlist);
                        }


                    }
                    if (sttngs.ignore0)
                    {
                        //this.musthaveprice(ahlist);
                        auclsts.priceishigher("1", ahlist);
                    }

                    if (sellercopy != sttngs.sellersearchstring)
                    {

                        if (sttngs.sellersearchstring != "")
                        {
                            auclsts.containsseller(sttngs.sellersearchstring, ahlist);
                        }


                    }
                    if (pricecopy != sttngs.pricesearchstring)
                    {

                        if (sttngs.pricesearchstring != "")
                        {
                            auclsts.priceishigher(sttngs.pricesearchstring, ahlist);

                        }


                    }
                    if (pricecopy2 != sttngs.pricesearchstring2)
                    {

                        if (sttngs.pricesearchstring2 != "")
                        {
                            auclsts.priceislower(sttngs.pricesearchstring2, ahlist);
                        }


                    }

                    if (growthclick || orderclick || energyclick || decayclick)
                    {
                        string[] res = { "", "", "", "" };
                        if (sttngs.decaybool) { res[0] = "decay"; };
                        if (sttngs.energybool) { res[1] = "energy"; };
                        if (sttngs.growthbool) { res[2] = "growth"; };
                        if (sttngs.orderbool) { res[3] = "order"; };
                        auclsts.searchforownenergy(res, ahlist);


                    }
                    if (commonclick || uncommonclick || rareclick)
                    {

                        int[] rare = { -1, -1, -1 };
                        if (sttngs.rarebool) { rare[2] = 2; };
                        if (sttngs.uncommonbool) { rare[1] = 1; };
                        if (sttngs.commonbool) { rare[0] = 0; };
                        auclsts.searchforownrarity(rare, ahlist);

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
                
                GUI.Box(recto.position, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
                GUI.Box(recto.position2, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

                

                // draw sort buttons:###############################################
                Vector2 vec11 = GUI.skin.button.CalcSize(new GUIContent("Scroll"));

                if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX, recto.screenRect.yMin - 4, vec11.x, 20), "Scroll"))
                {
                    if (sttngs.reverse == true) { sttngs.sortmode = -1; }// this will toggle the reverse mode
                    if (sttngs.sortmode == 1) { sttngs.reverse = true; } else { sttngs.reverse = false; };
                    sttngs.sortmode = 1;

                    auclsts.sortlist(ahlist); auclsts.sortlist(ahlistfull);

                }
                float datelength = GUI.skin.button.CalcSize(new GUIContent("Date")).x;
                float datebeginn = 0;
                if (this.wtsmenue)
                {
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Seller"));

                    if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2f - datelength / 2f - 2f, recto.screenRect.yMin - 4f, vec11.x, 20f), "Seller"))
                    {
                        if (sttngs.reverse == true) { sttngs.sortmode = -1; }
                        if (sttngs.sortmode == 3) { sttngs.reverse = true; } else { sttngs.reverse = false; };
                        sttngs.sortmode = 3;

                        auclsts.sortlist(ahlist); auclsts.sortlist(ahlistfull);
                    }
                }
                else
                {
                    vec11 = GUI.skin.button.CalcSize(new GUIContent("Buyer"));
                    if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2f - datelength / 2f - 2f, recto.screenRect.yMin - 4f, vec11.x, 20f), "Buyer"))
                    {
                        if (sttngs.reverse == true) { sttngs.sortmode = -1; }
                        if (sttngs.sortmode == 3) { sttngs.reverse = true; } else { sttngs.reverse = false; };
                        sttngs.sortmode = 3;

                        auclsts.sortlist(ahlist); auclsts.sortlist(ahlistfull);
                    }
                }
                datebeginn = recto.innerRect.xMin + recto.labelX + recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + recto.costIconWidth + (recto.labelsWidth - vec11.x) / 2 - datelength / 2 - 2 + vec11.x;
                vec11 = GUI.skin.button.CalcSize(new GUIContent("Price"));
                if (GUI.Button(new Rect(recto.innerRect.xMin + recto.labelX + 2f * recto.labelsWidth + (recto.costIconSize - recto.costIconWidth) / 2f - 5f + 2 * recto.costIconWidth + recto.labelsWidth / 4f - vec11.x / 2, recto.screenRect.yMin - 4, vec11.x, 20), "Price"))
                {
                    if (sttngs.reverse == true) { sttngs.sortmode = -1; }
                    if (sttngs.sortmode == 2) { sttngs.reverse = true; } else { sttngs.reverse = false; };
                    sttngs.sortmode = 2;

                    auclsts.sortlist(ahlist); auclsts.sortlist(ahlistfull);
                }
                vec11 = GUI.skin.button.CalcSize(new GUIContent("Date"));
                //if (GUI.Button(new Rect(this.innerRect.x + offX , this.screenRect.yMin - 4, vec11.x * 2, 20), "Date"))
                if (GUI.Button(new Rect(datebeginn + 4, recto.screenRect.yMin - 4, vec11.x, 20), "Date"))
                {
                    if (sttngs.reverse == true) { sttngs.sortmode = -1; }
                    if (sttngs.sortmode == 0) { sttngs.reverse = true; } else { sttngs.reverse = false; };
                    sttngs.sortmode = 0;
                    if (this.wtsmenue)
                    {
                        mssgprsr.wtslistfull.Clear(); mssgprsr.wtslistfull.AddRange(mssgprsr.wtslistfulltimed);
                        auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
                    }
                    else
                    {
                        mssgprsr.wtblistfull.Clear(); mssgprsr.wtblistfull.AddRange(mssgprsr.wtblistfulltimed);
                        auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);

                    }
                    auclsts.sortlist(ahlist); auclsts.sortlist(ahlistfull);
                }



                int num = 0;
                Card card = null;

                // delete old cards:
                DateTime currenttime = DateTime.Now.AddMinutes(-30);
                if (mssgprsr.wtslistfulltimed.Count > 0 && mssgprsr.wtslistfulltimed[mssgprsr.wtslistfulltimed.Count - 1].dtime.CompareTo(currenttime) < 0)
                {
                    mssgprsr.wtslistfulltimed.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    mssgprsr.wtslistfull.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    mssgprsr.wtslist.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                }
                if (mssgprsr.wtblistfulltimed.Count > 0 && mssgprsr.wtblistfulltimed[mssgprsr.wtblistfulltimed.Count - 1].dtime.CompareTo(currenttime) < 0)
                {
                    mssgprsr.wtblistfulltimed.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    mssgprsr.wtblistfull.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                    mssgprsr.wtblist.RemoveAll(element => element.dtime.CompareTo(currenttime) < 0);
                }
                // draw auctimes################################################
                //timefilter: 
                int time = 0;
                bool usetimefilter = false;
                float anzcards = anzcards = (float)this.ahlist.Count();
                if (sttngs.timesearchstring != "")
                {
                    time = Convert.ToInt32(sttngs.timesearchstring);
                    currenttime = DateTime.Now.AddMinutes(-1 * time); usetimefilter = true;
                    anzcards = (float)this.ahlist.Count(delegate(aucitem p1) { return (p1.dtime).CompareTo(currenttime) >= 0; });
                }

                this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));
                if (sttngs.reverse) { this.ahlist.Reverse(); }
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

                        string txt = cardnametoimageid(name.ToLower()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        if (this.shownumberscrolls) name = name + " (" + auclsts.available[current.card.getName()] + ")";
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                        // draw text
                        Rect position8 = recto.position8(num);
                        GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                        GUI.skin = this.cardListPopupSkin;
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
                        GUI.skin = this.cardListPopupBigLabelSkin;

                        vector = GUI.skin.label.CalcSize(new GUIContent(sellername));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                        //Rect position11 = new Rect(restyperect.xMax + 2f, (float)num * this.fieldHeight, this.labelsWidth, this.fieldHeight);
                        Rect position11 = new Rect(restyperect.xMax + 2f, position8.yMin, recto.labelsWidth, recto.cardHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position11, (vector.x >= position11.width) ? (sellername.Substring(0, Mathf.Min(sellername.Length, recto.maxCharsName)) + "...") : sellername);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        //draw timestamp
                        GUI.skin = this.cardListPopupSkin;
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
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                        Rect position12 = new Rect(nextx + 2f, position8.yMin, recto.labelsWidth / 2f, recto.cardHeight);
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(position12, gold);
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                        // draw suggested price
                        int index = Array.FindIndex(cardids, element => element == current.card.getType());
                        string suggeprice = "";
                        if (index >= 0)
                        {
                            int p1 = 0, p2 = 0;
                            if (wtsmenue)
                            {
                                if (takewtsahint == 0) p1 = prcs.lowerprice[index];
                                if (takewtsahint == 1) p1 = prcs.sugprice[index];
                                if (takewtsahint == 2) p1 = prcs.upperprice[index];
                            }
                            else
                            {
                                if (takewtbahint == 0) p1 = prcs.lowerprice[index];
                                if (takewtbahint == 1) p1 = prcs.sugprice[index];
                                if (takewtbahint == 2) p1 = prcs.upperprice[index];
                            }
                            suggeprice = "SP: " + p1;
                            if (this.showsugrange)
                            {
                                if (wtsmenue)
                                {
                                    if (takewtsahint2 == 0) p2 = prcs.lowerprice[index];
                                    if (takewtsahint2 == 1) p2 = prcs.sugprice[index];
                                    if (takewtsahint2 == 2) p2 = prcs.upperprice[index];
                                }
                                else
                                {
                                    if (takewtbahint2 == 0) p2 = prcs.lowerprice[index];
                                    if (takewtbahint2 == 1) p2 = prcs.sugprice[index];
                                    if (takewtbahint2 == 2) p2 = prcs.upperprice[index];
                                }
                            }
                            if (this.showsugrange && p1 != p2) suggeprice = "SP: " + Math.Min(p1, p2) + "-" + Math.Max(p1, p2);
                        }
                        GUI.skin = this.cardListPopupSkin;
                        Rect position14 = new Rect(nextx + 2f, position9.y, recto.labelsWidth / 2f, recto.fieldHeight);
                        GUI.skin.label.alignment = TextAnchor.UpperCenter;
                        GUI.Label(position14, suggeprice);
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;



                        GUI.skin = this.cardListPopupLeftButtonSkin;
                        if ( !this.selectable)
                        {
                            GUI.enabled = false;
                        }
                        if (GUI.Button(recto.position10(num), string.Empty) && current.card.tradable)
                        {
                            card = current.card;
                            App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_button_click");
                        }
                        if ( !this.selectable)
                        {
                            GUI.enabled = true;
                        }
                        //draw picture
                        if (texture != null)
                        {
                            GUI.DrawTexture(new Rect(4f, (float)num * recto.fieldHeight + (recto.fieldHeight - recto.cardHeight) * 0.43f, recto.cardWidth, recto.cardHeight), texture);
                        }
                        // draw buy/sell button
                        GUI.skin = this.cardListPopupGradientSkin;
                        if (this.wtsmenue)
                        {
                            if (!showtradedialog)
                            {
                                if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                {

                                    // start trading with seller
                                    if (current.seller != this.ownname)
                                    {
                                        showtradedialog = true;
                                        tradeitem = current;
                                    }
                                }
                            }
                            else { GUI.Box(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                            GUI.skin = this.cardListPopupBigLabelSkin;
                            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                            GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "Buy");


                        }
                        else
                        {
                            if (!showtradedialog)
                            {
                                if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""))
                                {

                                    // start trading with seller
                                    if (current.seller != this.ownname)
                                    {
                                        showtradedialog = true;
                                        tradeitem = current;
                                    }
                                }
                            }
                            else { GUI.Box(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), ""); }
                            GUI.skin = this.cardListPopupBigLabelSkin;
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
                if (sttngs.reverse) { this.ahlist.Reverse(); }
                GUI.EndScrollView();
                GUI.color = Color.white;
                if (card != null)
                {
                    //this.callback.ItemButtonClicked(this, card);
                    string clink = card.getName().ToLower();
                    int arrindex = Array.FindIndex(cardnames, element => element.Equals(clink));
                    if (arrindex >= 0)
                    {

                        crdvwr.createcard(arrindex, this.cardids[arrindex]);
                    }

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
                if (mssgprsr.newwtsmsgs)
                {
                    GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                    GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
                }


                if (GUI.Button(recto.wtsbuttonrect, "WTS") && !this.showtradedialog)
                {

                    mssgprsr.wtslistfull.Clear(); mssgprsr.wtslistfull.AddRange(mssgprsr.wtslistfulltimed);
                    //sortlist(wtslistfull);

                    this.ahlist = mssgprsr.wtslist; this.ahlistfull = mssgprsr.wtslistfull; this.wtsmenue = true; this.wtsinah = true;
                    sttngs.setsettings(true, true);
                    auclsts.sortlist(ahlist); auclsts.sortlist(ahlistfull);
                    auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
                    mssgprsr.newwtsmsgs = false;
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
                if (mssgprsr.newwtbmsgs)
                {
                    GUI.skin.button.normal.textColor = new Color(2f, 2f, 2f, 1f);
                    GUI.skin.button.hover.textColor = new Color(2f, 2f, 2f, 1f);
                }
                if (GUI.Button(recto.wtbbuttonrect, "WTB") && !this.showtradedialog)
                {
                    mssgprsr.wtblistfull.Clear(); mssgprsr.wtblistfull.AddRange(mssgprsr.wtblistfulltimed);
                    //sortmode==0 = sort by date so dont sort wtsfulltimed
                    //sortlist(wtblistfull);
                    this.ahlist = mssgprsr.wtblist; this.ahlistfull = mssgprsr.wtblistfull; this.wtsmenue = false; this.wtsinah = false;
                    sttngs.setsettings(true, false);
                    auclsts.sortlist(ahlist); auclsts.sortlist(ahlistfull);
                    auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
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

                if (this.showtradedialog) { this.starttrading(tradeitem.seller, tradeitem.card.getName(), tradeitem.priceinint, this.wtsmenue, tradeitem.whole); }






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
                GUI.Box(recto.filtermenurect, string.Empty);
                // wts filter menue
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.Label(recto.sbarlabelrect, "Scroll:");
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.sbrect, string.Empty);
                string selfcopy = sttngs.wtssearchstring;
                sttngs.wtssearchstring = GUI.TextField(recto.sbrect, sttngs.wtssearchstring, chatLogStyle);


                GUI.contentColor = Color.white;
                GUI.color = Color.white;
                if (!sttngs.growthbool) { GUI.color = dblack; }
                bool growthclick = GUI.Button(recto.sbgrect, growthres);
                GUI.color = Color.white;
                if (!sttngs.orderbool) { GUI.color = dblack; }
                bool orderclick = GUI.Button(recto.sborect, orderres);
                GUI.color = Color.white;
                if (!sttngs.energybool) { GUI.color = dblack; }
                bool energyclick = GUI.Button(recto.sberect, energyres);
                GUI.color = Color.white;
                if (!sttngs.decaybool) { GUI.color = dblack; }
                bool decayclick = GUI.Button(recto.sbdrect, decayres);


                GUI.contentColor = Color.gray;
                GUI.color = Color.white;
                if (!sttngs.commonbool) { GUI.color = dblack; }
                bool commonclick = GUI.Button(recto.sbcommonrect, "C");
                GUI.color = Color.white;
                if (!sttngs.uncommonbool) { GUI.color = dblack; }
                GUI.contentColor = Color.white;
                bool uncommonclick = GUI.Button(recto.sbuncommonrect, "U");
                GUI.color = Color.white;
                if (!sttngs.rarebool) { GUI.color = dblack; }
                GUI.contentColor = Color.yellow;
                bool rareclick = GUI.Button(recto.sbrarerect, "R");
                GUI.color = Color.white;
                if (!sttngs.threebool) { GUI.color = dblack; }
                //if (!p1mt3bool) { GUI.color = dblack; }
                bool mt3click;
                bool mt0click = false;
                if (this.wtsmenue)
                {
                    mt3click = GUI.Button(recto.sbthreerect, ">3"); // >3 bei wtsmenue=false
                }
                else { mt3click = GUI.Button(recto.sbthreerect, "<3"); }
                GUI.color = Color.white;
                if (!sttngs.onebool) { GUI.color = dblack; }
                //if (this.wtsmenue) { mt0click = GUI.Button(sbonerect, ">0"); };
                GUI.color = Color.white;
                GUI.contentColor = Color.white;

                GUI.skin = this.cardListPopupBigLabelSkin;
                if (this.wtsmenue)
                {
                    GUI.Label(recto.sbsellerlabelrect, "wts msg:");
                }
                else { GUI.Label(recto.sbsellerlabelrect, "wtb msg:"); }

                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.sbsellerrect, string.Empty);
                string sellercopy = sttngs.sellersearchstring;
                GUI.SetNextControlName("sellerframe");
                GUI.TextField(recto.sbsellerrect, sttngs.sellersearchstring, chatLogStyle);

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
                bool closeclick = GUI.Button(recto.sbclearrect, "X");
                GUI.contentColor = Color.white;

                if (growthclick) { sttngs.growthbool = !sttngs.growthbool; };
                if (orderclick) { sttngs.orderbool = !sttngs.orderbool; }
                if (energyclick) { sttngs.energybool = !sttngs.energybool; };
                if (decayclick) { sttngs.decaybool = !sttngs.decaybool; }
                if (commonclick) { sttngs.commonbool = !sttngs.commonbool; };
                if (uncommonclick) { sttngs.uncommonbool = !sttngs.uncommonbool; }
                if (rareclick) { sttngs.rarebool = !sttngs.rarebool; };
                if (mt3click) { sttngs.threebool = !sttngs.threebool; }
                if (mt0click) { sttngs.onebool = !sttngs.onebool; }
                if (closeclick)
                {
                    sttngs.resetgensearchsettings(this.wtsmenue);
                    if (ntwrk.realycontonetwork)
                    {
                        ntwrk.deleteownmessage(this.wtsmenue);

                    }
                }
                if (this.wtsmenue) { sttngs.savesettings(false, true); } else { sttngs.savesettings(false, false); }
                //if (wtsmenue) { pricecheck = (pricecopy.Length < this.pricesearchstring.Length) || (pricecopy.Length != this.pricesearchstring.Length && pricesearchstring == ""); } else { pricecheck = pricecopy.Length > this.pricesearchstring.Length; }
                //clear p1moddedlist only if necessary
                if (selfcopy.Length > sttngs.wtssearchstring.Length || closeclick || (growthclick && sttngs.growthbool) || (orderclick && sttngs.orderbool) || (energyclick && sttngs.energybool) || (decayclick && sttngs.decaybool) || (commonclick && sttngs.commonbool) || (uncommonclick && sttngs.uncommonbool) || (rareclick && sttngs.rarebool) || mt3click || mt0click)
                {
                    //Console.WriteLine("delete dings####");
                    auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);

                }
                else
                {

                    if (selfcopy != sttngs.wtssearchstring)
                    {

                        if (sttngs.wtssearchstring != "")
                        {
                            auclsts.containsname(sttngs.wtssearchstring, ahlist);

                        }


                    }

                    if (growthclick || orderclick || energyclick || decayclick)
                    {
                        string[] res = { "", "", "", "" };
                        if (sttngs.decaybool) { res[0] = "decay"; };
                        if (sttngs.energybool) { res[1] = "energy"; };
                        if (sttngs.growthbool) { res[2] = "growth"; };
                        if (sttngs.orderbool) { res[3] = "order"; };
                        auclsts.searchforownenergy(res, ahlist);


                    }
                    if (commonclick || uncommonclick || rareclick)
                    {

                        int[] rare = { -1, -1, -1 };
                        if (sttngs.rarebool) { rare[2] = 2; };
                        if (sttngs.uncommonbool) { rare[1] = 1; };
                        if (sttngs.commonbool) { rare[0] = 0; };
                        auclsts.searchforownrarity(rare, ahlist);

                    }

                }
                // draw generate button!


                GUI.color = Color.white;
                if (this.wtsmenue)
                {
                    if (sttngs.shortgeneratedwtsmessage == "")
                    { GUI.color = dblack; }
                }
                else
                {
                    if (sttngs.shortgeneratedwtbmessage == "")
                    { GUI.color = dblack; }
                }

                if (GUI.Button(recto.sbclrearpricesbutton, "Post to Network"))
                {

                    if (ntwrk.contonetwork)
                    {
                        ntwrk.sendownauctiontoall(this.wtsmenue, sttngs.getshortgenmsg(true), sttngs.getshortgenmsg(false));

                    }

                }
                GUI.color = Color.white;



                if (this.wtsmenue)
                {

                    if (GUI.Button(recto.sbgeneratebutton, "Gen WTS msg"))
                    {
                        // start trading with seller
                        generatewtxmsg(this.ahlistfull);
                    }

                }
                else
                {
                    if (GUI.Button(recto.sbgeneratebutton, "Gen WTB msg"))
                    {
                        // start trading bith buyer
                        generatewtxmsg(this.ahlistfull);
                    }
                }

                // draw message save/load buttons
                GUI.color = Color.white;
                if (this.wtsmenue)
                {
                    if (!this.wtsmsgload) GUI.color = dblack;
                    if (GUI.Button(recto.sbloadbutton, "load WTS msg") && this.wtsmsgload)
                    {

                        for (int i = 0; i < prcs.wtspricelist1.Count; i++)
                        {
                            KeyValuePair<string, string> item = prcs.wtspricelist1.ElementAt(i);
                            prcs.wtspricelist1[item.Key] = "";

                        }


                        string textel = System.IO.File.ReadAllText(this.ownaucpath + "wtsauc.txt");

                        string secmsg = (textel.Split(new string[] { "aucs " }, StringSplitOptions.None))[1];
                        string[] words = secmsg.Split(';');
                        foreach (string w in words)
                        {
                            if (w == "" || w == " ") continue;
                            string cardname = cardnames[Array.FindIndex(cardids, element => element == Convert.ToInt32(w.Split(' ')[0]))];
                            prcs.wtspricelist1[cardname] = w.Split(' ')[1];
                        }
                        generatewtxmsg(this.ahlistfull);


                    }

                }
                else
                {
                    if (!this.wtbmsgload) GUI.color = dblack;
                    if (GUI.Button(recto.sbloadbutton, "load WTB msg") && this.wtbmsgload)
                    {
                        for (int i = 0; i < prcs.wtbpricelist1.Count; i++)
                        {
                            KeyValuePair<string, string> item = prcs.wtbpricelist1.ElementAt(i);
                            prcs.wtbpricelist1[item.Key] = "";

                        }
                        string textel = System.IO.File.ReadAllText(this.ownaucpath + "wtbauc.txt");
                        string secmsg = (textel.Split(new string[] { "aucb " }, StringSplitOptions.None))[1];
                        string[] words = secmsg.Split(';');
                        foreach (string w in words)
                        {
                            if (w == "" || w == " ") continue;
                            string cardname = cardnames[Array.FindIndex(cardids, element => element == Convert.ToInt32(w.Split(' ')[0]))];
                            prcs.wtbpricelist1[cardname] = w.Split(' ')[1];
                        }
                        generatewtxmsg(this.ahlistfull);
                    }
                }
                GUI.color = Color.white;

                GUI.color = Color.white;
                if (this.wtsmenue)
                {
                    if (sttngs.generatedwtsmessage == "") GUI.color = dblack;
                    if (GUI.Button(recto.sbsavebutton, "save WTS msg"))
                    {
                        showtradedialog = true;


                    }

                }
                else
                {
                    if (sttngs.generatedwtbmessage == "") GUI.color = dblack;
                    if (GUI.Button(recto.sbsavebutton, "save WTB msg"))
                    {
                        showtradedialog = true;

                    }
                }
                GUI.color = Color.white;

            }
            // Draw generator here:
            if (this.generator)
            {
                //Console.WriteLine(GUI.GetNameOfFocusedControl());
                GUI.depth = 15;
                float offX = 0;
                this.opacity = 1f;
                GUI.skin = this.cardListPopupSkin;
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
                GUI.Box(recto.position, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
                GUI.Box(recto.position2, string.Empty);
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

                this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * (float)this.ahlist.Count));
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

                        string txt = cardnametoimageid(name.ToLower()).ToString();
                        Texture texture = App.AssetLoader.LoadTexture2D(txt);//current.getCardImage())
                        if (this.shownumberscrolls) name = name + " (" + auclsts.available[current.card.getName()] + ")";
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                        // draw text
                        Rect position8 = recto.position8(num);
                        GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                        GUI.skin = this.cardListPopupSkin;
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
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        vector = GUI.skin.label.CalcSize(new GUIContent(gold));
                        Rect position12 = new Rect(nextx + 2f, (float)num * recto.fieldHeight, recto.labelsWidth / 2f, recto.fieldHeight);
                        GUI.Label(position12, gold);


                        //draw pricebox
                        //
                        Rect position11 = new Rect(position12.xMax + 2f, (float)(num + 1) * recto.fieldHeight - (recto.fieldHeight + vector2.y) / 2 - 2, recto.labelsWidth, vector2.y + 4);
                        GUI.skin = this.cardListPopupSkin;
                        GUI.Box(position11, string.Empty);
                        // priceinint wurde bei der genliste missbraucht

                        this.chatLogStyle.alignment = TextAnchor.MiddleCenter;
                        if (!this.showtradedialog) //otherwise you cant hit the cancel button
                        {
                            if (this.wtsmenue)
                            {
                                prcs.wtspricelist1[current.card.getName().ToLower()] = Regex.Replace(GUI.TextField(position11, prcs.wtspricelist1[current.card.getName().ToLower()], chatLogStyle), @"[^0-9]", "");
                            }
                            else
                            {
                                prcs.wtbpricelist1[current.card.getName().ToLower()] = Regex.Replace(GUI.TextField(position11, prcs.wtbpricelist1[current.card.getName().ToLower()], chatLogStyle), @"[^0-9]", "");
                            }
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
                        if ( !this.selectable)
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
                        if (this.wtsmenue)
                        {
                            if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "SP"))
                            {
                                int index = Array.FindIndex(cardnames, element => element.Equals(current.card.getName().ToLower()));
                                if (index >= 0)
                                {
                                    prcs.PriceChecker(index, true, current.card.getName());
                                }
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth, recto.fieldHeight), "SP"))
                            {
                                int index = Array.FindIndex(cardnames, element => element.Equals(current.card.getName().ToLower()));
                                if (index >= 0)
                                {
                                    prcs.PriceChecker(index, false, current.card.getName());
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
                    int arrindex = Array.FindIndex(cardnames, element => element.Equals(clink));
                    if (arrindex >= 0)
                    {
                        crdvwr.createcard(arrindex, this.cardids[arrindex]);
                    }
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

                if (GUI.Button(recto.wtsbuttonrect, "WTS") && !this.showtradedialog)
                {
                    this.ahlist = this.wtsPlayer; this.ahlistfull = this.orgicardsPlayerwountrade; this.wtsmenue = true; this.wtsingen = true;
                    sttngs.setsettings(false, true);
                    auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
                }
                if (!this.wtsmenue)
                {

                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                if (GUI.Button(recto.wtbbuttonrect, "WTB") && !this.showtradedialog)
                {
                    this.ahlist = this.wtbPlayer; this.ahlistfull = auclsts.allcardsavailable; this.wtsmenue = false;
                    this.wtsingen = false;
                    sttngs.setsettings(false, false);
                    auclsts.fullupdatelist(ahlist, ahlistfull, this.inauchouse, this.wtsmenue, this.generator);
                }
                GUI.color = Color.white;
                if (GUI.Button(recto.fillbuttonrect, "Fill"))
                {
                    if (this.wtsmenue)
                    {
                        foreach (aucitem c in this.ahlist)
                        {
                            //int price=this.upperprice[Array.FindIndex(cardids, element => element == c.card.getType())];
                            int price = 0;
                            price = prcs.pricerounder(Array.FindIndex(cardids, element => element == c.card.getType()), wtsmenue);
                            prcs.wtspricelist1[c.card.getName().ToLower()] = price.ToString();

                        }
                    }
                    else
                    {
                        foreach (aucitem c in this.ahlist)
                        {

                            //int price = this.lowerprice[Array.FindIndex(cardids, element => element == c.card.getType())];
                            int price = 0;
                            price = prcs.pricerounder(Array.FindIndex(cardids, element => element == c.card.getType()), wtsmenue);
                            prcs.wtbpricelist1[c.card.getName().ToLower()] = price.ToString();

                        }
                    }

                }

                if (GUI.Button(recto.updatebuttonrect, "Clear"))
                {
                    if (this.wtsmenue)
                    {
                        for (int i = 0; i < prcs.wtspricelist1.Count; i++)
                        {
                            KeyValuePair<string, string> item = prcs.wtspricelist1.ElementAt(i);
                            prcs.wtspricelist1[item.Key] = "";

                        }

                    }
                    else
                    {
                        for (int i = 0; i < prcs.wtbpricelist1.Count; i++)
                        {
                            KeyValuePair<string, string> item = prcs.wtbpricelist1.ElementAt(i);
                            prcs.wtbpricelist1[item.Key] = "";

                        }
                    }

                }
                if (this.showtradedialog) { this.reallywanttosave(this.wtsmenue); }

            }
        }

        private void drawsettings()
        {
            GUI.depth = 15;
            GUI.color = Color.white;
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.settingRect, string.Empty);
            GUI.skin = this.cardListPopupLeftButtonSkin;
            if (GUI.Button(recto.setreset, "Reset"))
            {
                mssgprsr.spampreventtime = "";
                mssgprsr.spamprevint = 0;
                shownumberscrolls = false;
                showsugrange = false;
                rowscalestring = "10";
                rowscale = 1f;
                recto.setupPositions(this.chatisshown,this.rowscale,this.chatLogStyle,this.cardListPopupSkin);
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
            if (GUI.Button(recto.setload, "Load"))
            {
                this.loadsettings();
                recto.setupPositions(this.chatisshown,this.rowscale,this.chatLogStyle,this.cardListPopupSkin);
            }
            if (GUI.Button(recto.setsave, "Save"))
            {
                //save stuff
                string text = "";
                text = text + "spam " + mssgprsr.spampreventtime + ";";
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
                System.IO.File.WriteAllText(this.ownaucpath + "settingsauc.txt", text);

            }

            // spam preventor
            GUI.Label(recto.setpreventspammlabel, "dont update messages which are younger than:");
            GUI.Label(recto.setpreventspammlabel2, "minutes");

            GUI.Box(recto.setpreventspammrect, "");
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.setpreventspammrect, string.Empty);
            chatLogStyle.alignment = TextAnchor.MiddleCenter;
            mssgprsr.spampreventtime = Regex.Replace(GUI.TextField(recto.setpreventspammrect, mssgprsr.spampreventtime, chatLogStyle), @"[^0-9]", "");
            chatLogStyle.alignment = TextAnchor.MiddleLeft;
            if (mssgprsr.spampreventtime != "") mssgprsr.spamprevint = Convert.ToInt32(mssgprsr.spampreventtime);
            if (mssgprsr.spamprevint > 30) { mssgprsr.spampreventtime = "30"; mssgprsr.spamprevint = 30; }

            //anz cards
            GUI.skin = this.cardListPopupLeftButtonSkin;
            GUI.Label(recto.setowncardsanzlabel, "show owned number of scrolls beside scrollname");
            bool owp = GUI.Button(recto.setowncardsanzbox, "");
            if (owp) this.shownumberscrolls = !this.shownumberscrolls;
            if (this.shownumberscrolls)
            {
                GUI.DrawTexture(recto.setowncardsanzbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else
            {
                GUI.DrawTexture(recto.setowncardsanzbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }

            // show range
            GUI.skin = this.cardListPopupLeftButtonSkin;
            GUI.Label(recto.setsugrangelabel, "show ScrollsPost price as range");
            bool oowp = GUI.Button(recto.setsugrangebox, "");
            if (oowp) this.showsugrange = !this.showsugrange;
            if (this.showsugrange)
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
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.setrowhightbox, string.Empty);
            chatLogStyle.alignment = TextAnchor.MiddleCenter;
            string rowcopy = rowscalestring;
            rowscalestring = Regex.Replace(GUI.TextField(recto.setrowhightbox, rowscalestring, chatLogStyle), @"[^0-9]", "");
            chatLogStyle.alignment = TextAnchor.MiddleLeft;
            if (rowscalestring != "") { rowscale = (float)Convert.ToDouble(rowscalestring) / 10f; } else { rowscale = 1.0f; }
            if (rowscale > 2f) { rowscale = 2f; rowscalestring = "20"; }
            if (rowscale < 0.5f) { rowscale = .5f; }
            if (!rowcopy.Equals(rowscalestring)) { recto.setupPositions(this.chatisshown,this.rowscale,this.chatLogStyle,this.cardListPopupSkin); }

            //round wts


            bool ooowp = GUI.Button(recto.setwtsbox, "");
            if (ooowp) this.roundwts = !this.roundwts;
            if (this.roundwts)
            {
                GUI.DrawTexture(recto.setwtsbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else
            {
                GUI.DrawTexture(recto.setwtsbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }
            GUI.skin = this.cardListPopupLeftButtonSkin;
            GUI.Label(recto.setwtslabel1, "round ScrollsPost prices in WTS-generator ");
            if (GUI.Button(recto.setwtsbutton1, ""))
            {
                this.wtsroundup = !this.wtsroundup;
            }
            GUI.Label(recto.setwtslabel2, " to next ");
            if (GUI.Button(recto.setwtsbutton2, ""))
            {
                this.wtsroundmode = (this.wtsroundmode + 1) % 3;
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (this.wtsroundup) { GUI.Label(recto.setwtsbutton1, "up"); } else { GUI.Label(recto.setwtsbutton1, "down"); }
            if (this.wtsroundmode == 0) { GUI.Label(recto.setwtsbutton2, "5"); }
            if (this.wtsroundmode == 1) { GUI.Label(recto.setwtsbutton2, "10"); }
            if (this.wtsroundmode == 2) { GUI.Label(recto.setwtsbutton2, "50"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            //round wtb


            bool ooowwp = GUI.Button(recto.setwtbbox, "");
            if (ooowwp) this.roundwtb = !this.roundwtb;
            if (this.roundwtb)
            {
                GUI.DrawTexture(recto.setwtbbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb_checked"));
            }
            else
            {
                GUI.DrawTexture(recto.setwtbbox, ResourceManager.LoadTexture("Arena/scroll_browser_button_cb"));
            }
            GUI.skin = this.cardListPopupLeftButtonSkin;
            GUI.Label(recto.setwtblabel1, "round ScrollsPost prices in WTB-generator ");
            if (GUI.Button(recto.setwtbbutton1, ""))
            {
                this.wtbroundup = !this.wtbroundup;
            }
            GUI.Label(recto.setwtblabel2, " to next ");
            if (GUI.Button(recto.setwtbbutton2, ""))
            {
                this.wtbroundmode = (this.wtbroundmode + 1) % 3;
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (this.wtbroundup) { GUI.Label(recto.setwtbbutton1, "up"); } else { GUI.Label(recto.setwtbbutton1, "down"); }
            if (this.wtbroundmode == 0) { GUI.Label(recto.setwtbbutton2, "5"); }
            if (this.wtbroundmode == 1) { GUI.Label(recto.setwtbbutton2, "10"); }
            if (this.wtbroundmode == 2) { GUI.Label(recto.setwtbbutton2, "50"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            //take price generator
            GUI.Label(recto.settakewtsgenlabel, "WTS-Generator takes ");
            if (GUI.Button(recto.settakewtsgenbutton, ""))
            {
                this.takewtsgenint = (this.takewtsgenint + 1) % 3;
            }
            GUI.Label(recto.settakewtsgenlabel2, "ScrollsPost price");
            GUI.Label(recto.settakewtbgenlabel, "WTB-Generator takes ");
            if (GUI.Button(recto.settakewtbgenbutton, ""))
            {
                this.takewtbgenint = (this.takewtbgenint + 1) % 3;
            }
            GUI.Label(recto.settakewtbgenlabel2, "ScrollsPost price");
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            if (this.takewtsgenint == 0) { GUI.Label(recto.settakewtsgenbutton, "lower"); }
            if (this.takewtsgenint == 1) { GUI.Label(recto.settakewtsgenbutton, "sugg."); }
            if (this.takewtsgenint == 2) { GUI.Label(recto.settakewtsgenbutton, "upper"); }
            if (this.takewtbgenint == 0) { GUI.Label(recto.settakewtbgenbutton, "lower"); }
            if (this.takewtbgenint == 1) { GUI.Label(recto.settakewtbgenbutton, "sugg."); }
            if (this.takewtbgenint == 2) { GUI.Label(recto.settakewtbgenbutton, "upper"); }
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            //show price ah
            if (this.showsugrange)
            {
                GUI.Label(recto.setwtsahlabel, "show in WTS-AH the ");
                if (GUI.Button(recto.setwtsahbutton, ""))
                {
                    this.takewtsahint = (this.takewtsahint + 1) % 3;
                }
                if (GUI.Button(recto.setwtsahbutton2, ""))
                {
                    this.takewtsahint2 = (this.takewtsahint2 + 1) % 3;
                }
                GUI.Label(recto.setwtsahlabel3, "and");
                GUI.Label(recto.setwtsahlabel4, "ScrollsPost prices");
                GUI.Label(recto.setwtbahlabel, "show in WTB-AH the ");
                if (GUI.Button(recto.setwtbahbutton, ""))
                {
                    this.takewtbahint = (this.takewtbahint + 1) % 3;
                }
                if (GUI.Button(recto.setwtbahbutton2, ""))
                {
                    this.takewtbahint2 = (this.takewtbahint2 + 1) % 3;
                }
                GUI.Label(recto.setwtbahlabel3, "and");
                GUI.Label(recto.setwtbahlabel4, "ScrollsPost prices");
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                if (this.takewtsahint == 0) { GUI.Label(recto.setwtsahbutton, "lower"); }
                if (this.takewtsahint == 1) { GUI.Label(recto.setwtsahbutton, "sugg."); }
                if (this.takewtsahint == 2) { GUI.Label(recto.setwtsahbutton, "upper"); }
                if (this.takewtbahint == 0) { GUI.Label(recto.setwtbahbutton, "lower"); }
                if (this.takewtbahint == 1) { GUI.Label(recto.setwtbahbutton, "sugg."); }
                if (this.takewtbahint == 2) { GUI.Label(recto.setwtbahbutton, "upper"); }
                if (this.takewtsahint2 == 0) { GUI.Label(recto.setwtsahbutton2, "lower"); }
                if (this.takewtsahint2 == 1) { GUI.Label(recto.setwtsahbutton2, "sugg."); }
                if (this.takewtsahint2 == 2) { GUI.Label(recto.setwtsahbutton2, "upper"); }
                if (this.takewtbahint2 == 0) { GUI.Label(recto.setwtbahbutton2, "lower"); }
                if (this.takewtbahint2 == 1) { GUI.Label(recto.setwtbahbutton2, "sugg."); }
                if (this.takewtbahint2 == 2) { GUI.Label(recto.setwtbahbutton2, "upper"); }
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            else
            {
                GUI.Label(recto.setwtsahlabel, "show in WTS-AH the ");
                if (GUI.Button(recto.setwtsahbutton, ""))
                {
                    this.takewtsahint = (this.takewtsahint + 1) % 3;
                }
                GUI.Label(recto.setwtsahlabel2, "ScrollsPost price");
                GUI.Label(recto.setwtbahlabel, "show in WTB-AH the ");
                if (GUI.Button(recto.setwtbahbutton, ""))
                {
                    this.takewtbahint = (this.takewtbahint + 1) % 3;
                }
                GUI.Label(recto.setwtbahlabel2, "ScrollsPost price");
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                if (this.takewtsahint == 0) { GUI.Label(recto.setwtsahbutton, "lower"); }
                if (this.takewtsahint == 1) { GUI.Label(recto.setwtsahbutton, "sugg."); }
                if (this.takewtsahint == 2) { GUI.Label(recto.setwtsahbutton, "upper"); }
                if (this.takewtbahint == 0) { GUI.Label(recto.setwtbahbutton, "lower"); }
                if (this.takewtbahint == 1) { GUI.Label(recto.setwtbahbutton, "sugg."); }
                if (this.takewtbahint == 2) { GUI.Label(recto.setwtbahbutton, "upper"); }
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }

            GUI.skin = this.cardListPopupLeftButtonSkin;
        }

        private void starttrading(string name, string cname, int price, bool wts, string orgmsg)
        {
            // asks the user if he wants to trade
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string text = "sell";
            if (wts) text = "buy";
            int anzcard = auclsts.available[cname];
            string message = "You want to " + text + "\r\n" + cname + " for " + price + " Gold" +"\r\nYou own this card "  + anzcard+" times\r\n\r\nOriginal Message:";
            GUI.Label(recto.tbmessage, message);
            GUI.skin.label.wordWrap = true;
            float msghigh = GUI.skin.label.CalcHeight(new GUIContent(orgmsg), recto.tbmessage.width - 30f);
            GUI.skin = this.cardListPopupSkin;
            scrolll = GUI.BeginScrollView(recto.tbmessagescroll, scrolll, new Rect(0f, 0f, recto.tbmessagescroll.width - 20f, msghigh));
            GUI.skin = this.cardListPopupBigLabelSkin;

            GUI.Label(new Rect(5f, 5f, recto.tbmessagescroll.width - 30f, msghigh), orgmsg);

            //Console.WriteLine(message);
            
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.EndScrollView();
            GUI.skin.label.wordWrap = false;
            GUI.skin = this.cardListPopupLeftButtonSkin;

            if (GUI.Button(recto.tbok, "OK"))
            { 
                this.showtradedialog = false;
                App.GameActionManager.TradeUser(this.globalusers[name]);
                this.postmsgmsg = "You want to " + text + ": " + cname + " for " + price + " Gold" + ". You own this card " + anzcard + " times. Original Message:" + "\r\n" + orgmsg;
                this.postmsgontrading = true;
            };
            if (GUI.Button(recto.tbwhisper, "Whisper"))
            { 
                this.showtradedialog = false;
                this.chatRooms.OpenWhisperRoom(name);
            };
            if (GUI.Button(recto.tbcancel, "Cancel")) { this.showtradedialog = false; };
        }

        private void reallywanttosave(bool wts)
        {
            // asks the user if he wants to trade
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.tradingbox, "");
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            string message = "You want to override existing file?";

            GUI.Label(recto.tradingbox, message);
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin = this.cardListPopupLeftButtonSkin;

            if (GUI.Button(recto.tbok, "OK"))
            {
                if (wts)
                {
                    System.IO.File.WriteAllText(this.ownaucpath + "wtsauc.txt", sttngs.shortgeneratedwtsmessage);
                    this.wtsmsgload = true;
                }
                else
                {
                    System.IO.File.WriteAllText(this.ownaucpath + "wtbauc.txt", sttngs.shortgeneratedwtbmessage);
                    this.wtbmsgload = true;
                }
                this.showtradedialog = false;
            };
            if (GUI.Button(recto.tbcancel, "Cancel")) { this.showtradedialog = false; };
            
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