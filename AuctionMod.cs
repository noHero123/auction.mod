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

    public struct nickelement
    {
        public string nick;
        public string cardname;
    }


    public class AuctionMod : BaseMod, ICommListener
    {
#if DEBUG
		private bool hideNetworkMessages = false; //  false = testmodus
#else
		private bool hideNetworkMessages = true; //  false = testmodus
#endif
        


         // = realcardnames + loadedscrollsnicks
        private string[] aucfiles;
        int screenh = 0;
        int screenw = 0;
        bool deckchanged = false;
        private FieldInfo chatLogStyleinfo;
        private MethodInfo drawsubmenu;

        //Texture2D arrowdown = ResourceManager.LoadTexture("ChatUI/dropdown_arrow");

        Settings sttngs;
        Network ntwrk;
        Searchsettings srchsvr;
        Cardviewer crdvwr;
        Prices prcs;
        Messageparser mssgprsr;
        Rectomat recto;
        Helpfunktions helpf;
        AuctionHouseUI ahui;
        GeneratorUI genui;
        SettingsUI setui;
        Generator generator;

        

        public void handleMessage(Message msg)
        {

            if (msg is LibraryViewMessage)
            {
                if (!(((LibraryViewMessage)msg).profileId == "test"))
                {
                    generator.setowncards(msg);
                }
            }

            if (msg is BuyStoreItemResponseMessage)
            {
                // if we buy a card in the store, we have to reload the own cards , nexttime we open ah/generator
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
                //if he doesnt accept the trade, reset the variables
                TradeResponseMessage trm = (TradeResponseMessage)msg;
                if (trm.status != "ACCEPT")
                {
                    helpf.postmsgontrading = false;
                    helpf.postmsggetnextroomenter = false;
                    helpf.postmsgmsg = "";
                }
            }


            if (msg is RoomEnterMessage && helpf.postmsggetnextroomenter)
            {// he accept your trade, post the auction message to yourself
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
            {// you are connected to network and start a battle -> disconnect
                GameInfoMessage gim =(GameInfoMessage) msg;
                if (ntwrk.inbattle == false) { ntwrk.inbattle = true; ntwrk.disconfromaucnet(); Console.WriteLine("discon"); }
            }

            if (msg is RoomInfoMessage && ntwrk.contonetwork) //TODO: move to network class
            {
                // you enter a auc-x room , while connected to network... so do communication stuff, like adding the users etc
                RoomInfoMessage roominfo = (RoomInfoMessage)msg;
                if (roominfo.roomName.StartsWith("auc-"))
                {
                    ntwrk.enteraucroom(roominfo);
                }
            
            }

            if ( msg is FailMessage) //TODO: move to Network - detect if it was a TradeNetwork-Message that faild!
            {   // delete user if he cant be whispered ( so he doesnt check out propperly... blame on him!)
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
                if (!helpf.globalusers.ContainsKey(newuser.name)) { helpf.globalusers.Add(newuser.name, newuser); }
                ntwrk.adduser(newuser);

            }

            if (msg is CardTypesMessage)
            {
                generator.setallavailablecards(msg);
                
                mssgprsr.searchscrollsnicks.AddRange(helpf.loadedscrollsnicks);
            }

            return;
        }
        public void onReconnect()
        {
            return; // don't care
        }

        public AuctionMod()
        {
            helpf = Helpfunktions.Instance;
            sttngs = Settings.Instance;
            srchsvr = Searchsettings.Instance;
            crdvwr = Cardviewer.Instance;
            prcs = Prices.Instance;
            recto = Rectomat.Instance;
            mssgprsr = Messageparser.Instance;
            ntwrk = Network.Instance;
            ahui = AuctionHouseUI.Instance;
            generator = Generator.Instance;
            genui = GeneratorUI.Instance;
            setui = SettingsUI.Instance;
            
            drawsubmenu = typeof(Store).GetMethod("drawSubMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            chatLogStyleinfo = typeof(ChatUI).GetField("chatMsgStyle", BindingFlags.Instance | BindingFlags.NonPublic);


            

            
            
            Directory.CreateDirectory(helpf.ownaucpath);
            this.aucfiles = Directory.GetFiles(helpf.ownaucpath, "*auc.txt");
            if (aucfiles.Contains(helpf.ownaucpath + "wtsauc.txt"))//File.Exists() was slower
            {
                helpf.canLoadWTSmsg = true;
            }
            if (aucfiles.Contains(helpf.ownaucpath + "wtbauc.txt"))//File.Exists() was slower
            {
                helpf.canLoadWTBmsg = true;
            }
            if (aucfiles.Contains(helpf.ownaucpath + "nicauc.txt"))//File.Exists() was slower
            {
                helpf.nicks = true;
            }

            if (aucfiles.Contains(helpf.ownaucpath + "settingsauc.txt"))//File.Exists() was slower
            {
                sttngs.loadsettings(helpf.ownaucpath,helpf.deleteTime);
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
            return 5;
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
                   scrollsTypes["BattleMode"].Methods.GetMethod("_handleMessage", new Type[]{typeof(Message)}),
                   scrollsTypes["Store"].Methods.GetMethod("Start")[0],
                    scrollsTypes["Store"].Methods.GetMethod("showSellMenu")[0],
                     scrollsTypes["Store"].Methods.GetMethod("showBuyMenu")[0],
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
                    if (hideNetworkMessages && Network.isNetworkCommand(wmsg)) return true;
                }
            }
            
            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
					if (hideNetworkMessages && Network.isNetworkCommand(wmsg))
                    { // hides all whisper messages from auc-mod
						return true;
                    }
                }
                if (msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage rem = (RoomChatMessageMessage)msg;
					if (hideNetworkMessages  && ntwrk.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }

                if (msg is RoomEnterMessage)
                {   
                    RoomEnterMessage rem = (RoomEnterMessage) msg;
					if (hideNetworkMessages && ntwrk.contonetwork && rem.roomName.StartsWith("auc-")) return true;
                }

                if (msg is RoomInfoMessage)
                {
                    RoomInfoMessage rem = (RoomInfoMessage)msg;
					if (hideNetworkMessages && ntwrk.contonetwork && rem.roomName.StartsWith("auc-")) return true;
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
			//Replace Methods by NOPs
            returnValue = null;
        }

        public override void BeforeInvoke(InvocationInfo info)
        {

            return;
        }

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {

            if (info.target is EndGameScreen && info.targetMethod.Equals("GoToLobby")) { ntwrk.inbattle = false; } // user leaved a battle

            if (info.target is ChatUI && info.targetMethod.Equals("Show")) { helpf.chatisshown = (bool)info.arguments[0]; this.screenh = 0; }// so position will be calculatet new on next ongui

            if (info.target is ChatUI && info.targetMethod.Equals("Initiate"))
            {
                helpf.target = (ChatUI)info.target;
                helpf.setchatlogstyle((GUIStyle)this.chatLogStyleinfo.GetValue(info.target));
            }

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
                helpf.setlobbyskin((GUISkin)typeof(Store).GetField("lobbySkin", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.target));
                helpf.storeinfo=(Store)info.target;
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
                    if (!helpf.globalusers.ContainsKey(user.name)) { helpf.globalusers.Add(user.name, user); };
                } 
            }

            if (info.target is Store && info.targetMethod.Equals("OnGUI"))
            {

               
                GUI.color = Color.white;
                GUI.contentColor = Color.white;
                drawsubmenu.Invoke(info.target, null);
                    Vector2 screenMousePos = GUIUtil.getScreenMousePos();

                    if (!(Screen.height == screenh) || !(Screen.width == screenw)|| helpf.chatLogStyle==null) // if resolution was changed, recalc positions
                    {
                        Console.WriteLine("change resolution");
                        screenh = Screen.height;
                        screenw = Screen.width;
                        helpf.chatLogStyle = (GUIStyle)chatLogStyleinfo.GetValue(helpf.target);
                        //recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                        //helpf.adjustskins(recto.fieldHeight);
                        recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                        recto.setupsettingpositions(helpf.chatLogStyle, helpf.cardListPopupBigLabelSkin);

                    }
                   
                    
                    // delete picture on click!
                    if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && crdvwr.clicked >= 3) { crdvwr.clearallpics(); }
                    
                    //klick button AH
                    if (LobbyMenu.drawButton(recto.ahbutton, "AH", helpf.lobbySkin) && !helpf.showtradedialog)
                    {
                        if (this.deckchanged)
                        { App.Communicator.sendRequest(new LibraryViewMessage()); this.deckchanged = false; }
                        ahui.ahbuttonpressed();
                        
                    }
                    // klick button Gen
                    if (LobbyMenu.drawButton(recto.genbutton, "Gen", helpf.lobbySkin) && !helpf.showtradedialog)
                    {
                        if (this.deckchanged)
                        { App.Communicator.sendRequest(new LibraryViewMessage()); this.deckchanged = false; }
                        genui.genbuttonpressed();
                    }

                    if (LobbyMenu.drawButton(recto.settingsbutton, "Settings", helpf.lobbySkin) && !helpf.showtradedialog)
                    {
                        setui.setbuttonpressed();
                        
                    }    


                    // draw ah oder gen-menu

                    if (helpf.inauchouse) ahui.drawAH();
                    if (helpf.generator) genui.drawgenerator();
                    if (helpf.settings) setui.drawsettings();
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


        
       
        
        
     
    }
}