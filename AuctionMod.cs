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
using System.Threading;

namespace Auction.mod 
{


    using System;
    using UnityEngine;


    public struct nickelement
    {
        public string nick;
        public string cardname;
    }


    public class AuctionMod : BaseMod, ICommListener
    {
        /*
#if DEBUG
		private bool hideNetworkMessages = false; //  false = testmodus
#else
		private bool hideNetworkMessages = true; //  false = testmodus
#endif
        */


         // = realcardnames + loadedscrollsnicks
        private string[] aucfiles;
        int screenh = 0;
        int screenw = 0;
        bool deckchanged = false;

        private FieldInfo chatLogStyleinfo;
        private MethodInfo drawsubmenu;

        //Texture2D arrowdown = ResourceManager.LoadTexture("ChatUI/dropdown_arrow");

        Settings sttngs;
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
        VersionCheck vc;
        PopupManager pppmngr;

        public static string GetName()
        {
            return "auc";
        }

        public static int GetVersion()
        {
            return 12;
        }


        public void onConnect(OnConnectData ocd)
        {
        //lol
        }


        public void handleMessage(Message msg)
        {

            if (msg is CardTypesMessage)
            {
                generator.setallavailablecards(msg);
                mssgprsr.searchscrollsnicks.AddRange(helpf.loadedscrollsnicks);
            }

            if (msg is LibraryViewMessage)
            {
                if ((((LibraryViewMessage)msg).profileId == App.MyProfile.ProfileInfo.id))
                {
                    generator.setowncards(msg);
                    helpf.setOwnCards(msg);
                    this.ahui.clearOffercard();
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

           // trading with auctionbot#############################################################################################################


            if (this.sttngs.waitForAuctionBot && msg is FailMessage) // bot is offline! shame on him!
            {   
                FailMessage fm = (FailMessage)msg;
                if (fm.op == "Whisper" && fm.info == "Could not find the user 'auctionmod'.")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    pppmngr.startOKPopup("auctionmodresponse", "offline", "the auctionbot is offline");
                }
            }


            if (this.sttngs.waitForAuctionBot && msg is WhisperMessage)
            {
                WhisperMessage wm = msg as WhisperMessage;

                if (wm.from == "auctionmod" && wm.text == "invite me")
                {
                    //invite the auctionmod for trade
                    //App.Communicator.sendRequest(new TradeAcceptMessage("1b8a4125d2634634aa76f33e1d04b0d4"));
                    App.Communicator.send(new TradeInviteMessage("1b8a4125d2634634aa76f33e1d04b0d4"));
                    this.sttngs.actualTrading = true;
                    this.sttngs.addedCard = false;
                }
            }

            if ((this.sttngs.waitForAuctionBot||this.sttngs.actualTrading) && msg is WhisperMessage)
            {
                WhisperMessage wm=msg as WhisperMessage;

                if (wm.from == "auctionmod" && wm.text == "invite me")
                {
                    //invite the auctionmod for trade
                    //App.Communicator.sendRequest(new TradeAcceptMessage("1b8a4125d2634634aa76f33e1d04b0d4"));

                    this.sttngs.actualTrading = true;
                    this.sttngs.addedCard = false;
                }

                if(wm.from=="auctionmod" && wm.text=="there is not such an auction")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("not such an auction");
                    pppmngr.startOKPopup("auctionmodresponse", "non exsisting", "the auction you chosed, doesnt exist anymore");
                }

                if (wm.from == "auctionmod" && wm.text == "to slow")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("you are to slow bro");
                    pppmngr.startOKPopup("auctionmodresponse", "to slow", "you were responding to slow");
                }

                if (wm.from == "auctionmod" && wm.text == "auctionlimit reached")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("not more than 10 auctions");
                    pppmngr.startOKPopup("auctionmodresponse", "to much auctions", "You cant create more auctions");
                }

                if (wm.from == "auctionmod" && wm.text == "dont fool me")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("there are no auctions open");
                    pppmngr.startOKPopup("auctionmodresponse", "no auctions", "you dont own a finished auction");
                }

                if (wm.from == "auctionmod" && wm.text == "hit accept" && this.sttngs.AucBotMode == "getauc")
                {
                    new Thread(new ThreadStart(this.acceptTrade)).Start();
                }
            }


            //trade finished
            if (this.sttngs.actualTrading && msg is TradeViewMessage && (msg as TradeViewMessage).to.profile.name == "auctionmod" && (msg as TradeViewMessage).modified == false && (msg as TradeViewMessage).to.accepted == true && (msg as TradeViewMessage).from.accepted == true)
            {

                if (this.sttngs.AucBotMode == "setauc") pppmngr.startOKPopup("auctionmodresponse", "created auction", "you created the following auction:\r\n" + this.ahui.createdAuctionText);
                if (this.sttngs.AucBotMode == "bidauc") pppmngr.startOKPopup("auctionmodresponse", "scroll bought", "you bought " + this.helpf.auctionBotCardsToNames[(msg as TradeViewMessage).to.cardIds[0]] + " for " + (msg as TradeViewMessage).from.gold + " gold.");
                if (this.sttngs.AucBotMode == "getauc")
                {
                    string reccards = "";
                    foreach( long x in (msg as TradeViewMessage).to.cardIds)
                    {
                        if (reccards != "") reccards = reccards + ", ";
                        reccards = reccards + this.helpf.auctionBotCardsToNames[x];
                    }
                    string output = "you received: " + (msg as TradeViewMessage).to.gold + " gold and some scrolls (look in Chat).";
                    if (reccards == "")
                    {
                        output = "you received: " + (msg as TradeViewMessage).to.gold + " gold."; 
                    }
                    pppmngr.startOKPopup("auctionmodresponse", "stuff received", output);
                    if (reccards != "")
                    {
                        RoomChatMessageMessage nrcmm = new RoomChatMessageMessage("[note]", "You received these scrolls: " + reccards + ".");
                        nrcmm.from = "AuctionHouse";
                        App.ArenaChat.handleMessage(nrcmm);
                    }
                }
                new Thread(new ThreadStart(this.getCardsAndGold)).Start();
                
            }

            // get cardIDs+ Types from auctionbot, for knowing which cards you got :D

            if (this.sttngs.actualTrading && msg is LibraryViewMessage)
            {
                if ((((LibraryViewMessage)msg).profileId == "1b8a4125d2634634aa76f33e1d04b0d4"))
                {
                    // the libViewMessage is from auctionmod :D
                    helpf.setAuctionModCards(msg);
                }
            }

            //accept in bidmode, after he adds card + we adds money
            if (this.sttngs.actualTrading && msg is TradeViewMessage && this.sttngs.AucBotMode == "bidauc" && (msg as TradeViewMessage).to.profile.name == "auctionmod" && (msg as TradeViewMessage).to.cardIds.Length == 1 && (msg as TradeViewMessage).to.cardIds[0] == this.sttngs.tradeCardID && (msg as TradeViewMessage).from.gold == this.sttngs.bidgold && (msg as TradeViewMessage).modified == true)
            { // trading with bot, he has added the wanted card, and we added the money... lets click ok in 5 seconds! (if modified = true (if he accept, it will be falls), so we accept only once
                new Thread(new ThreadStart(this.acceptTrade)).Start();
            }

            //if trading with bot and wants to bid on an auction: add money, + wait till he adds card accept
            if (this.sttngs.waitForAuctionBot && !this.sttngs.addedCard && this.sttngs.AucBotMode == "bidauc" && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == "auctionmod" && (msg as TradeResponseMessage).status == "ACCEPT")
            {
                new Thread(new ThreadStart(this.setGold)).Start();
            }

            //if trading with bot and wants to set an auction: add card, accept after 5 seconds (threaded)
            if (this.sttngs.waitForAuctionBot && !this.sttngs.addedCard && this.sttngs.AucBotMode=="setauc" && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == "auctionmod" && (msg as TradeResponseMessage).status == "ACCEPT")
            { 
                App.Communicator.sendRequest(new TradeAddCardsMessage(new long[] { this.sttngs.tradeCardID }));
                this.sttngs.addedCard = true;
                new Thread(new ThreadStart(this.acceptTrade)).Start();

            }
            // trade was canceled
            if (this.sttngs.waitForAuctionBot && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == "auctionmod" && (msg as TradeResponseMessage).status == "CANCEL_BARGAIN")
            {
                this.sttngs.AucBotMode = "";
                this.sttngs.waitForAuctionBot = false;
                this.sttngs.actualTrading = false;
            }
            // trade was accepted
            if (this.sttngs.waitForAuctionBot && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == "auctionmod" && (msg as TradeResponseMessage).status == "ACCEPT")
            {
                // nice!
                this.sttngs.waitForAuctionBot = false;
            }
            // trade was timeouted :D
            if (this.sttngs.waitForAuctionBot && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == "auctionmod" && (msg as TradeResponseMessage).status == "TIMEOUT")
            {
                this.sttngs.AucBotMode = "";
                this.sttngs.waitForAuctionBot = false;
                this.sttngs.actualTrading = false;
            }

            if (msg is RoomEnterMessage && (this.sttngs.waitForAuctionBot || this.sttngs.actualTrading))
            {
                this.sttngs.autoAuctionRoom = (msg as RoomEnterMessage).roomName;
                this.sttngs.auctionScrollsMessagesCounter = 1;
            }

            if (msg is RoomChatMessageMessage && this.sttngs.auctionScrollsMessagesCounter >= 1)
            {
                if ((msg as RoomChatMessageMessage).from == "Scrolls") 
                {
                    this.sttngs.auctionScrollsMessagesCounter ++;
                }
                if (this.sttngs.auctionScrollsMessagesCounter == 3)
                {
                    App.ArenaChat.ChatRooms.LeaveRoom((msg as RoomChatMessageMessage).roomName);
                    App.Communicator.send(new RoomExitMessage((msg as RoomChatMessageMessage).roomName));
                }
                
            }
            

            // following stuff writes the offer you want to buy/sell in chatroom after entering the trade-chat-room#####################

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


            if (helpf.postmsggetnextroomenter && msg is RoomEnterMessage )
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

            return;
        }

        public void onReconnect()
        {
            return; // don't care
        }

        public AuctionMod()
        {
            pppmngr = new PopupManager();
            vc = new VersionCheck();
            DateTime itze= DateTime.Now;
            helpf = Helpfunktions.Instance;
            helpf.setOwnAucPath(this.OwnFolder() + System.IO.Path.DirectorySeparatorChar + "auc" + System.IO.Path.DirectorySeparatorChar);
            sttngs = Settings.Instance;
            srchsvr = Searchsettings.Instance;
            crdvwr = Cardviewer.Instance;
            prcs = Prices.Instance;
            recto = Rectomat.Instance;
            mssgprsr = Messageparser.Instance;
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
            Console.WriteLine("### not an Auction House loaded in "+(DateTime.Now.Subtract(itze)).TotalMilliseconds + " ms ###");
        }

        

        public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
        {
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["ChatUI"].Methods.GetMethod("Initiate")[0],
                    scrollsTypes["ChatUI"].Methods.GetMethod("Show", new Type[]{typeof(bool)}),
                    scrollsTypes["ChatRooms"].Methods.GetMethod("SetRoomInfo", new Type[] {typeof(RoomInfoMessage)}),
                    scrollsTypes["ChatRooms"].Methods.GetMethod("ChatMessage", new Type[]{typeof(RoomChatMessageMessage)}),
                    scrollsTypes["ArenaChat"].Methods.GetMethod("handleMessage", new Type[]{typeof(Message)}),
                    //scrollsTypes["BattleMode"].Methods.GetMethod("_handleMessage", new Type[]{typeof(Message)}),
                    scrollsTypes["Store"].Methods.GetMethod("Start")[0],
                    scrollsTypes["Store"].Methods.GetMethod("showSellMenu")[0],
                    scrollsTypes["Store"].Methods.GetMethod("showBuyMenu")[0],
                    scrollsTypes["Store"].Methods.GetMethod("OnGUI")[0],
                    scrollsTypes["TradeSystem"].Methods.GetMethod("StartTrade", new Type[]{typeof(List<Card>) , typeof(List<Card>), typeof(string), typeof(string), typeof(int)}),

                    //scrollsTypes["EndGameScreen"].Methods.GetMethod("GoToLobby")[0],
                    //scrollsTypes["GameSocket"].Methods.GetMethod("OnDestroy")[0],
                    //for trading with auctionbot
                    scrollsTypes["InviteManager"].Methods.GetMethod("handleMessage",new Type[]{typeof(Message)}),
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
            /*
            // the auctionbot isnt inviting you anymore
            if ((this.sttngs.waitForAuctionBot || this.sttngs.actualTrading) && info.target is InviteManager && info.targetMethod.Equals("handleMessage") && info.arguments[0] is TradeInviteForwardMessage && (info.arguments[0] as TradeInviteForwardMessage).inviter.name == "auctionmod")
            { // return true if you are waiting for auctionbot
                //App.Communicator.sendRequest(new TradeAcceptBargainMessage());
                return true;
            }
             */

            if ((this.sttngs.waitForAuctionBot || this.sttngs.actualTrading) && info.target is InviteManager && info.targetMethod.Equals("handleMessage") && info.arguments[0] is TradeResponseMessage && (info.arguments[0] as TradeResponseMessage).to.name == "auctionmod")
            { // return true if you are waiting for auctionbot
                //because we dont want to display the trading-gui
                return true;
            }


            if (info.target is Store && info.targetMethod.Equals("OnGUI"))
            {
                // dont want to see the orginal store-interface in our AH
                if (helpf.inauchouse || helpf.generator || helpf.settings) return true;
            }
            
            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is WhisperMessage)
                {
                    WhisperMessage wmsg = (WhisperMessage)msg;
                    //dont want to see whisper messages from/to the auctionbot
                    if (this.sttngs.actualTrading && wmsg.from == "auctionmod") return true;
                    if (this.sttngs.waitForAuctionBot && wmsg.toProfileName == "auctionmod") return true;
                    if ((wmsg.text.Equals("to slow") || wmsg.text.Contains("dont fool me") || wmsg.text.Contains("there is not such an auction") || wmsg.text.Contains("auctionlimit reached")) && wmsg.from == "auctionmod") return true;
                    
                }

                if (msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage rem = (RoomChatMessageMessage)msg;
                    if ((this.sttngs.auctionScrollsMessagesCounter >= 1) && rem.roomName.StartsWith("trade-")) { if (this.sttngs.auctionScrollsMessagesCounter == 3)this.sttngs.auctionScrollsMessagesCounter = 0; return true; }
                    if (rem.text.StartsWith("auc parsertest")) { helpf.messegparsingtest(); return true; }
                }

                if (msg is RoomEnterMessage)
                {   
                    RoomEnterMessage rem = (RoomEnterMessage) msg;
                    if ((this.sttngs.waitForAuctionBot || this.sttngs.actualTrading) && rem.roomName.StartsWith("trade-")) return true;
                }

                if (msg is RoomInfoMessage)
                {
                    RoomInfoMessage rem = (RoomInfoMessage)msg;
                    if ((this.sttngs.waitForAuctionBot || this.sttngs.actualTrading) && rem.roomName.StartsWith("trade-")) return true;
                }


            }
            return false;
        }

        public override void ReplaceMethod(InvocationInfo info, out object returnValue)
        {

            if (this.sttngs.waitForAuctionBot && info.target is InviteManager && info.targetMethod.Equals("handleMessage") && info.arguments[0] is TradeInviteForwardMessage && (info.arguments[0] as TradeInviteForwardMessage).inviter.name == "auctionmod")
            { // return true if you are waiting for auctionbot
                App.Communicator.sendRequest(new TradeAcceptMessage("1b8a4125d2634634aa76f33e1d04b0d4"));
                this.sttngs.actualTrading = true;
                this.sttngs.addedCard = false;
                //App.Communicator.sendRequest(new TradeAcceptBargainMessage());
            }

			//Replace Methods by NOPs
            returnValue = null;
        }

        public override void BeforeInvoke(InvocationInfo info)
        {

            return;
        }

        public override void AfterInvoke(InvocationInfo info, ref object returnValue)
        {

            //if (info.target is EndGameScreen && info.targetMethod.Equals("GoToLobby")) { ntwrk.inbattle = false; } // user leaved a battle

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

            if (info.target is Store && info.targetMethod.Equals("OnGUI")) // drawing our buttons and stuff in store
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
                        recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);// need  it to calc fieldhight even if bothmenue=true
                        if ((helpf.bothmenue || (helpf.createAuctionMenu && !helpf.offerMenuSelectCardMenu)) && !helpf.generator) recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                        recto.setupsettingpositions(helpf.chatLogStyle, helpf.cardListPopupBigLabelSkin);

                    }
                   
                    
                    // delete picture on click!
                    if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && crdvwr.clicked >= 3) { crdvwr.clearallpics(); }
                    
                    //klick button AH
                    if (LobbyMenu.drawButton(recto.ahbutton, "AH", helpf.lobbySkin) && !helpf.showtradedialog)
                    {
                        if (this.deckchanged)
                        { App.Communicator.sendRequest(new LibraryViewMessage()); this.deckchanged = false; }
                        if (helpf.bothmenue || helpf.createAuctionMenu ) recto.setupPositionsboth(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                        else recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                        ahui.ahbuttonpressed();
                        
                    }
                    // klick button Gen
                    if (LobbyMenu.drawButton(recto.genbutton, "Gen", helpf.lobbySkin) && !helpf.showtradedialog)
                    {
                        if (this.deckchanged)
                        { App.Communicator.sendRequest(new LibraryViewMessage()); this.deckchanged = false; }
                        recto.setupPositions(helpf.chatisshown, sttngs.rowscale, helpf.chatLogStyle, helpf.cardListPopupSkin);
                        genui.genbuttonpressed();
                    }
                    //klick settings-button
                    if (LobbyMenu.drawButton(recto.settingsbutton, "Settings", helpf.lobbySkin) && !helpf.showtradedialog)
                    {
                        recto.setupsettingpositions(helpf.chatLogStyle, helpf.cardListPopupBigLabelSkin);
                        setui.setbuttonpressed();
                        
                    }    


                    // draw ah oder gen-menu

                    if (helpf.inauchouse) ahui.drawAH();
                    if (helpf.generator) genui.drawgenerator();
                    if (helpf.settings) setui.drawsettings();
                    GUI.color = Color.white;
                    GUI.contentColor = Color.white;

                    crdvwr.draw();// drawing the card you have clicked
                    
                
            }
            else if (info.target is Store && (info.targetMethod.Equals("showBuyMenu") || info.targetMethod.Equals("showSellMenu")))
            {
                //disable auc.house + generator
                Store.ENABLE_SHARD_PURCHASES = true;
                helpf.inauchouse = false;
                helpf.generator = false;
                helpf.settings = false;
                helpf.showtradedialog = false;
                helpf.makeOfferMenu = false;
                helpf.offerMenuSelectCardMenu = false;
                helpf.showtradedialog = false;
                if (info.targetMethod.Equals("showSellMenu")) { this.deckchanged = false; }

            }

            if (info.target is ChatRooms && info.targetMethod.Equals("ChatMessage"))
            {
                //get trademessages from chatmessages
                RoomChatMessageMessage msg = (RoomChatMessageMessage)info.arguments[0];
                if (msg.from != "Scrolls")
                {
                    AuctionHouse.Instance.addAuctions(mssgprsr.GetAuctionsFromMessage(msg.text, msg.from, msg.roomName));
                }
            }

            return;
        }

        private void acceptTrade()
        { // accept the trade with auctionbot
            System.Threading.Thread.Sleep(5300);
            App.Communicator.sendRequest(new TradeAcceptBargainMessage());
        }

        private void setGold()
        {
            //set the gold while trading with auctionbot
            App.Communicator.sendRequest(new TradeSetGoldMessage(this.sttngs.bidgold));
        }
        

        private void getCardsAndGold()
        {
            // need to be refreshed with small delay, because sometimes its to fast after an trade
            System.Threading.Thread.Sleep(1000);
            if (this.sttngs.AucBotMode == "bidauc" || this.sttngs.AucBotMode == "getauc")
            { App.Communicator.sendRequest(new GetStoreItemsMessage()); }
            if (this.sttngs.AucBotMode == "setauc" || this.sttngs.AucBotMode == "getauc")
            { App.Communicator.sendRequest(new LibraryViewMessage()); }
            this.sttngs.AucBotMode = "";
            this.sttngs.waitForAuctionBot = false;
            this.sttngs.actualTrading = false;
        }
       
        
        
     
    }
}