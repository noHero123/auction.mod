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

        

        
        
       
        
        private string[] aucfiles;
        Color dblack = new Color(1f, 1f, 1f, 0.5f);
        private Store storeinfo;

        

        private GUISkin lobbySkin;

        
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
        
        
        private Regex cardlinkfinder;

        
        
        

        


        Texture2D arrowdown = ResourceManager.LoadTexture("ChatUI/dropdown_arrow");
        FieldInfo buymen; FieldInfo sellmen;

        
        

        bool chatisshown = false;
        bool deckchanged = false;

        string[] auccontroler = new string[] { };

        Settings sttngs;
        Network ntwrk;
        Searchsettings srchsvr;
        cardviewer crdvwr;
        Prices prcs;
        listfilters lstfltrs;
        messageparser mssgprsr;
        Rectomat recto;
        auclists alists;
        Helpfunktions helpf;
        AuctionHouseUI ahui;
        GeneratorUI genui;
        
        

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
                    helpf.postmsgontrading = false;
                    helpf.postmsggetnextroomenter = false;
                    helpf.postmsgmsg = "";
                }
            }


            if (msg is RoomEnterMessage && helpf.postmsggetnextroomenter)
            {
                RoomEnterMessage rmem = (RoomEnterMessage)msg;
                if (rmem.roomName.StartsWith("trade-"))
                {
                    helpf.postmsggetnextroomenter = false;
                    // post the msg here!:
                    RoomChatMessageMessage joinmessage = new RoomChatMessageMessage(rmem.roomName, "<color=#777460>" + helpf.postmsgmsg + "</color>");
                    joinmessage.from = "Scrolls";

                    //App.ChatUI.handleMessage(new RoomChatMessageMessage(rmem.roomName, "<color=#777460>" + postmsgmsg + "</color>"));
                    App.ArenaChat.ChatRooms.ChatMessage(joinmessage);
                    helpf.postmsgmsg = "";
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
                if (!helpf.globalusers.ContainsKey(newuser.name)) { helpf.globalusers.Add(newuser.name, newuser); ntwrk.addglobalusers(newuser); }
                ntwrk.adduser(newuser);

            }

            if (msg is CardTypesMessage)
            {

                
                helpf.setarrays(msg);
                prcs.lowerprice = new int[helpf.cardids.Length];
                prcs.upperprice = new int[helpf.cardids.Length];
                prcs.sugprice = new int[helpf.cardids.Length];
                if (this.nicks) readnicksfromfile();
                mssgprsr.searchscrollsnicks.Clear();
                prcs.wtbpricelist1.Clear();
                lstfltrs.allcardsavailable.Clear();
                for (int j = 0; j < helpf.cardnames.Length; j++)
                {
                    prcs.wtbpricelist1.Add(helpf.cardnames[j].ToLower(), "");
                    CardType type = CardTypeManager.getInstance().get(helpf.cardids[j]);
                    Card card = new Card(helpf.cardids[j], type, true);
                    aucitem ai = new aucitem();
                    ai.card = card;
                    ai.price = "";
                    ai.priceinint = lstfltrs.allcardsavailable.Count;
                    ai.seller="me";
                    lstfltrs.allcardsavailable.Add(ai);
                    nickelement nele;
                    nele.nick = helpf.cardnames[j];
                    nele.cardname = helpf.cardnames[j];
                    mssgprsr.searchscrollsnicks.Add(nele);
                };
                mssgprsr.searchscrollsnicks.AddRange(this.loadedscrollsnicks);

                lstfltrs.allcardsavailable.Sort(delegate(aucitem p1, aucitem p2) { return (p1.card.getName()).CompareTo(p2.card.getName()); });
                //test
                //foreach (aucitem ai in allcardsavailable)
                //{ Console.WriteLine(ai.card.getName()); }
                //App.Communicator.removeListener(this);//dont need the listener anymore
                prcs.totalpricecheck(helpf.cardids);
            }

            return;
        }
        public void onReconnect()
        {
            return; // don't care
        }

        public Auction()
        {
            helpf = new Helpfunktions();
            sttngs = new Settings();
            ntwrk = new Network();
            srchsvr = new Searchsettings();
            Console.WriteLine("saveall");
            srchsvr.saveall();
            Console.WriteLine("savealldone");
            crdvwr = new cardviewer();
            prcs = new Prices();
            lstfltrs = new listfilters(srchsvr, prcs);
            recto = new Rectomat();
            alists = new auclists(lstfltrs, prcs, srchsvr);
            mssgprsr = new messageparser(alists, lstfltrs, this.sttngs, this.helpf);
            ahui = new AuctionHouseUI(mssgprsr,alists,recto,lstfltrs,prcs,crdvwr,srchsvr,ntwrk,sttngs,this.helpf);
            ahui.setskins((GUISkin)Resources.Load("_GUISkins/CardListPopup"), (GUISkin)Resources.Load("_GUISkins/CardListPopupGradient"), (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel"), (GUISkin)Resources.Load("_GUISkins/CardListPopupLeftButton"));
            genui = new GeneratorUI(mssgprsr, alists, recto, lstfltrs, prcs, crdvwr, srchsvr, ntwrk, sttngs, this.helpf);
            genui.setskins((GUISkin)Resources.Load("_GUISkins/CardListPopup"), (GUISkin)Resources.Load("_GUISkins/CardListPopupGradient"), (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel"), (GUISkin)Resources.Load("_GUISkins/CardListPopupLeftButton"));

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

            
            
            Directory.CreateDirectory(helpf.ownaucpath);
            this.aucfiles = Directory.GetFiles(helpf.ownaucpath, "*auc.txt");
            if (aucfiles.Contains(helpf.ownaucpath + "wtsauc.txt"))//File.Exists() was slower
            {
                helpf.wtsmsgload = true;
            }
            if (aucfiles.Contains(helpf.ownaucpath + "wtbauc.txt"))//File.Exists() was slower
            {
                helpf.wtbmsgload = true;
            }
            if (aucfiles.Contains(helpf.ownaucpath + "nicauc.txt"))//File.Exists() was slower
            {
                this.nicks = true;
            }

            if (aucfiles.Contains(helpf.ownaucpath + "settingsauc.txt"))//File.Exists() was slower
            {
                sttngs.loadsettings(helpf.ownaucpath);
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
                if (helpf.inauchouse || helpf.generator || helpf.settings) return true;
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

                            alists.wtslistfulltimed.RemoveAll(element => element.seller == wmsg.from);
                            alists.wtslistfull.RemoveAll(element => element.seller == wmsg.from);
                            alists.wtslist.RemoveAll(element => element.seller == wmsg.from);


                        

                    }
                    if (text.StartsWith("aucdeleteb"))
                    {

                        alists.wtblistfulltimed.RemoveAll(element => element.seller == wmsg.from);
                        alists.wtblistfull.RemoveAll(element => element.seller == wmsg.from);
                        alists.wtblist.RemoveAll(element => element.seller == wmsg.from);
                    

                    }

                    if (text.StartsWith("aucs ") || text.StartsWith("aucb "))
                    {
                        mssgprsr.getaucitemsformmsg(text, wmsg.from, wmsg.GetChatroomName(), helpf.generator, helpf.inauchouse, helpf.settings, helpf.wtsmenue);
                        //need playerid (wispering doesnt send it)
                        if (!helpf.globalusers.ContainsKey(wmsg.from)) { WhisperMessage needid = new WhisperMessage(wmsg.from, "needaucid"); App.Communicator.sendRequest(needid); }
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

                        ntwrk.aucstayquestion(text, wmsg.from, srchsvr.shortgeneratedwtsmessage, srchsvr.shortgeneratedwtbmessage);

                    
                    }

                    if (text.StartsWith("aucstay! "))
                    {   // user founded a room, and he dont want to get the room-list
                        ntwrk.aucstay(text, wmsg.from, srchsvr.shortgeneratedwtsmessage, srchsvr.shortgeneratedwtbmessage);
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
                        ntwrk.sendownauctionstosingleuser(srchsvr.shortgeneratedwtsmessage, srchsvr.shortgeneratedwtbmessage);
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
                string[] lines = System.IO.File.ReadAllLines(helpf.ownaucpath + "nicauc.txt");
                foreach (string s in lines)
                {
                    if (s == "" || s == " ") continue;
                    string cardname = s.Split(':')[0];
                    if (helpf.cardnames.Contains(cardname.ToLower()))
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

        
       
       
      

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {
            if (info.target is EndGameScreen && info.targetMethod.Equals("GoToLobby")) { ntwrk.inbattle = false; } // user leaved a battle

            if (info.target is ChatUI && info.targetMethod.Equals("Show")) { this.chatisshown = (bool)info.arguments[0]; this.screenh = 0; }// so position will be calculatet new on next ongui

            if (info.target is ChatUI && info.targetMethod.Equals("Initiate")) 
            {  target = (ChatUI)info.target;
                this.chatLogStyle =(GUIStyle)this.chatLogStyleinfo.GetValue(info.target);
                ahui.setchatlogstyle(this.chatLogStyle);
                genui.setchatlogstyle(this.chatLogStyle);
                chatRooms = (ChatRooms)chatRoomsinfo.GetValue(info.target); }

            if (info.target is TradeSystem && info.targetMethod.Equals("StartTrade"))// user start a trade, show the buy-message
            {
                if (helpf.postmsgontrading == true)
                {
                    helpf.postmsgontrading = false;
                    helpf.postmsggetnextroomenter = true;// the next RoomEnterMsg is the tradeRoom!
                }
            }

            if (info.target is Store && info.targetMethod.Equals("Start"))//user opened store
            {
                this.lobbySkin = (GUISkin)typeof(Store).GetField("lobbySkin", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target);
                this.storeinfo=(Store)info.target;
                helpf.showtradedialog = false;
                helpf.inauchouse = false;
                helpf.generator = false;
                helpf.settings = false;
                
            }

            if (info.target is ChatRooms && info.targetMethod.Equals("SetRoomInfo")) //adding new users to userlist
            {
                RoomInfoMessage roomInfo = (RoomInfoMessage)info.arguments[0];
                RoomInfoProfile[] profiles = roomInfo.updated;
                for (int i = 0; i < profiles.Length; i++)
                {
                    RoomInfoProfile p = profiles[i];
                    ChatUser user = ChatUser.FromRoomInfoProfile(p) ;
                    if (!helpf.globalusers.ContainsKey(user.name)) { helpf.globalusers.Add(user.name, user); ntwrk.addglobalusers(user); };
                } 
            }

            if (info.target is Store && info.targetMethod.Equals("handleMessage"))// update orginal cards!
            {
                
                Message msg = (Message)info.arguments[0];
                if (msg is LibraryViewMessage)
                {
                    if (!(((LibraryViewMessage)msg).profileId == "test"))
                    {
                        alists.setowncards(msg, helpf.inauchouse, helpf.generator, helpf.wtsmenue);
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
                        recto.setupPositions(this.chatisshown,sttngs.rowscale,this.chatLogStyle,this.cardListPopupSkin);
                        recto.setupsettingpositions(this.chatLogStyle, this.cardListPopupLeftButtonSkin);

                    }
                   
                    
                    // delete picture on click!
                    if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && crdvwr.clicked >= 3) { crdvwr.clearallpics(); }

                    //auction house...
                    GUIPositioner subMenuPositioner = App.LobbyMenu.getSubMenuPositioner(1f, 5);
                    //klick button AH
                    if (LobbyMenu.drawButton(subMenuPositioner.getButtonRect(2f), "AH", this.lobbySkin) && !helpf.showtradedialog)
                    {
                        if (this.deckchanged)
                        { App.Communicator.sendRequest(new LibraryViewMessage()); this.deckchanged = false; }
                        helpf.inauchouse = true;
                        helpf.settings = false;
                        helpf.generator = false;
                        //this.hideInformation();
                        hideInformationinfo.Invoke(storeinfo, null);

                        
                        iTween.MoveTo((GameObject)buymen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        showBuyinfo.SetValue(storeinfo, false);

                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(false);
                        iTween.MoveTo((GameObject)sellmen.GetValue(storeinfo), iTween.Hash(new object[] { "x", -0.5f, "time", 1f, "easetype", iTween.EaseType.easeInExpo }));
                        ((GameObject)sellmen.GetValue(storeinfo)).SetActive(true);
                        showSellinfo.SetValue(storeinfo, false);
                        
                        Store.ENABLE_SHARD_PURCHASES = false;
                        
                        
                        
                        if (ahui.wtsinah)
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
                        this.targetchathightinfo.SetValue(this.target, (float)Screen.height * 0.25f);
                        if (helpf.wtsmenue) { mssgprsr.newwtsmsgs = false; } else { mssgprsr.newwtbmsgs = false; }
                    }
                // klick button Gen

                    if (LobbyMenu.drawButton(subMenuPositioner.getButtonRect(3f), "Gen", this.lobbySkin) && !helpf.showtradedialog)
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
                        helpf.inauchouse = false;
                        helpf.generator = true;
                        helpf.settings = false;


                        if (genui.wtsingen)
                        {
                            helpf.wtsmenue = true;
                            alists.setAhlistsToGenWtsLists();
                            
                            
                        }
                        else
                        {
                            helpf.wtsmenue = false;
                            alists.setAhlistsToGenWtbLists();
                             
                            
                        }

                        //this.genlist.AddRange(this.genlistfull);


                        
                        this.targetchathightinfo.SetValue(this.target, (float)Screen.height * 0.25f);
                    }
                    Rect setrecto = subMenuPositioner.getButtonRect(4f);
                    setrecto.x = Screen.width - setrecto.width;// -subMenuPositioner.getButtonRect(0f).x;
                    if (LobbyMenu.drawButton(setrecto, "settings", this.lobbySkin) && !helpf.showtradedialog)
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
                        helpf.inauchouse = false;
                        helpf.generator = false;
                        helpf.settings = true;
                        this.targetchathightinfo.SetValue(this.target, (float)Screen.height * 0.25f);
                    }    


                    // draw ah oder gen-menu

                    if (helpf.inauchouse) helpf.wtsmenue = ahui.drawAH(helpf.wtsmenue, helpf.generator, helpf.showtradedialog);
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;
                    if (helpf.generator) genui.drawgenerator();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;
                    if (helpf.settings) drawsettings();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;

                    crdvwr.draw();
                    
                
            }
            else if (info.target is Store && (info.targetMethod.Equals("showBuyMenu") || info.targetMethod.Equals("showSellMenu")))
            {
                //disable auc.house + generator
                Store.ENABLE_SHARD_PURCHASES = true;
                helpf.inauchouse = false;
                helpf.generator = false;
                helpf.settings = false;
                helpf.showtradedialog = false;
                if (info.targetMethod.Equals("showSellMenu")) { this.deckchanged = false; }

            }

            if (info.target is ChatRooms && info.targetMethod.Equals("ChatMessage"))
            {
                //get trademessages from chatmessages
                RoomChatMessageMessage msg = (RoomChatMessageMessage)info.arguments[0];
                if (msg.from != "Scrolls")
                {
                    mssgprsr.getaucitemsformmsg(msg.text, msg.from, msg.roomName, helpf.generator, helpf.inauchouse, helpf.settings, helpf.wtsmenue);
                }
            }

            return;
        }

        //draw stuff

        
       
        private void drawsettings()
        {
            GUI.depth = 15;
            GUI.color = Color.white;
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.settingRect, string.Empty);
            GUI.skin = this.cardListPopupLeftButtonSkin;
            if (GUI.Button(recto.setreset, "Reset"))
            {
                sttngs.resetsettings();
                
                recto.setupPositions(this.chatisshown,sttngs.rowscale,this.chatLogStyle,this.cardListPopupSkin);
                

            }
            if (GUI.Button(recto.setload, "Load"))
            {
                sttngs.loadsettings(helpf.ownaucpath);
                recto.setupPositions(this.chatisshown, sttngs.rowscale, this.chatLogStyle, this.cardListPopupSkin);
            }
            if (GUI.Button(recto.setsave, "Save"))
            {
                //save stuff
                sttngs.savesettings(helpf.ownaucpath);
                

            }

            // spam preventor
            GUI.Label(recto.setpreventspammlabel, "dont update messages which are younger than:");
            GUI.Label(recto.setpreventspammlabel2, "minutes");

            GUI.Box(recto.setpreventspammrect, "");
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.setpreventspammrect, string.Empty);
            chatLogStyle.alignment = TextAnchor.MiddleCenter;
            sttngs.spampreventtime = Regex.Replace(GUI.TextField(recto.setpreventspammrect, sttngs.spampreventtime, chatLogStyle), @"[^0-9]", "");
            chatLogStyle.alignment = TextAnchor.MiddleLeft;
            if (sttngs.spampreventtime != "") sttngs.spamprevint = Convert.ToInt32(sttngs.spampreventtime);
            if (sttngs.spamprevint > 30) { sttngs.spampreventtime = "30"; sttngs.spamprevint = 30; }

            //anz cards
            GUI.skin = this.cardListPopupLeftButtonSkin;
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
            GUI.skin = this.cardListPopupLeftButtonSkin;
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
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(recto.setrowhightbox, string.Empty);
            chatLogStyle.alignment = TextAnchor.MiddleCenter;
            string rowcopy = sttngs.rowscalestring;
            sttngs.rowscalestring = Regex.Replace(GUI.TextField(recto.setrowhightbox, sttngs.rowscalestring, chatLogStyle), @"[^0-9]", "");
            chatLogStyle.alignment = TextAnchor.MiddleLeft;
            if (sttngs.rowscalestring != "") { sttngs.rowscale = (float)Convert.ToDouble(sttngs.rowscalestring) / 10f; } else { sttngs.rowscale = 1.0f; }
            if (sttngs.rowscale > 2f) { sttngs.rowscale = 2f; sttngs.rowscalestring = "20"; }
            if (sttngs.rowscale < 0.5f) { sttngs.rowscale = .5f; }
            if (!rowcopy.Equals(sttngs.rowscalestring)) { recto.setupPositions(this.chatisshown, sttngs.rowscale, this.chatLogStyle, this.cardListPopupSkin); }

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
            GUI.skin = this.cardListPopupLeftButtonSkin;
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
            GUI.skin = this.cardListPopupLeftButtonSkin;
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

            GUI.skin = this.cardListPopupLeftButtonSkin;
        }

        
        
        
     
    }
}