using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Auction.mod
{
    class TradingWithBots : ICommListener
    {

        //for faster testing some variables:
        public string botname ="auctionmod";
        public int botid = 13754;
        public string oldbotid = "1b8a4125d2634634aa76f33e1d04b0d4";
        public string spreadsheet ="0AhhxijYPL-BGdHBDYzRLcDhFU2FJZkVEaWNFREdaLUE";

         private static TradingWithBots instance;

        public static TradingWithBots Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TradingWithBots();
                }
                return instance;
            }
        }

        PopupManager pppmngr;
        Helpfunktions helpf;
        Settings sttngs;
        PlayerStore ps;


        private TradingWithBots()
        {
            this.helpf = Helpfunktions.Instance; 
            this.sttngs = Settings.Instance;
            this.pppmngr = PopupManager.Instance;
            this.ps = PlayerStore.Instance;
            try
            {
                App.Communicator.addListener(this);
            }
            catch 
            {
                Console.WriteLine("cant add listener");
            }
        }

        public void handleMessage(Message msg)
        {

            // trading with auctionbot#############################################################################################################


            if (this.sttngs.waitForAuctionBot && msg is FailMessage) // bot is offline! shame on him!
            {
                FailMessage fm = (FailMessage)msg;
                if (fm.op == "Whisper" && fm.info == "Could not find the user '" + botname + "'.")
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

                if (wm.from == botname && wm.text == "invite me")
                {
                    //invite the auctionmod for trade
                    //App.Communicator.sendRequest(new TradeAcceptMessage("1b8a4125d2634634aa76f33e1d04b0d4"));
                    App.Communicator.send(new TradeInviteMessage(botid));
                    this.sttngs.actualTrading = true;
                    this.sttngs.addedCard = false;
                }
            }

            if ((this.sttngs.waitForAuctionBot || this.sttngs.actualTrading) && msg is WhisperMessage)
            {
                WhisperMessage wm = msg as WhisperMessage;

                if (wm.from == botname && wm.text == "invite me")
                {
                    //invite the auctionmod for trade
                    //App.Communicator.sendRequest(new TradeAcceptMessage("1b8a4125d2634634aa76f33e1d04b0d4"));

                    this.sttngs.actualTrading = true;
                    this.sttngs.addedCard = false;
                }

                if (wm.from == botname && wm.text == "there is not such an auction")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("not such an auction");
                    pppmngr.startOKPopup("auctionmodresponse", "non exsisting", "the auction you chosed, doesnt exist anymore");
                }

                if (wm.from == botname && wm.text == "to slow")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("you are to slow bro");
                    pppmngr.startOKPopup("auctionmodresponse", "to slow", "you were responding to slow");
                }

                if (wm.from == botname && wm.text == "auctionlimit reached")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("not more than 10 auctions");
                    pppmngr.startOKPopup("auctionmodresponse", "to much auctions", "You cant create more auctions");
                }

                if (wm.from == botname && wm.text == "dont fool me")
                {
                    this.sttngs.AucBotMode = "";
                    this.sttngs.waitForAuctionBot = false;
                    this.sttngs.actualTrading = false;
                    Console.WriteLine("there are no auctions open");
                    pppmngr.startOKPopup("auctionmodresponse", "no auctions", "you dont own a finished auction");
                }

                if (wm.from == botname && wm.text == "hit accept" && this.sttngs.AucBotMode == "getauc")
                {
                    new Thread(new ThreadStart(this.acceptTrade)).Start();
                }
            }


            //trade finished
            if (this.sttngs.actualTrading && msg is TradeViewMessage && (msg as TradeViewMessage).to.profile.name == botname && (msg as TradeViewMessage).modified == false && (msg as TradeViewMessage).to.accepted == true && (msg as TradeViewMessage).from.accepted == true)
            {

                if (this.sttngs.AucBotMode == "setauc")
                {
                    pppmngr.startOKPopup("auctionmodresponse", "created auction", "you created the following auction:\r\n" + this.helpf.createdAuctionText);
                }
                if (this.sttngs.AucBotMode == "multisetauc")
                { 
                    pppmngr.startOKPopup("auctionmodresponse", "created auction", "you created some auctions!");
                    this.ps.clearAuctions();
                }
                if (this.sttngs.AucBotMode == "bidauc")
                {
                    pppmngr.startOKPopup("auctionmodresponse", "scroll bought", "you bought " + this.helpf.auctionBotCardsToNames[(msg as TradeViewMessage).to.cardIds[0]] + " for " + (msg as TradeViewMessage).from.gold + " gold.");
                }
                if (this.sttngs.AucBotMode == "getauc")
                {
                    string reccards = "";
                    foreach (long x in (msg as TradeViewMessage).to.cardIds)
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
                if ((((LibraryViewMessage)msg).profileId == this.botid))
                {
                    // the libViewMessage is from auctionmod :D
                    helpf.setAuctionModCards(msg);
                }
            }

            //accept in bidmode, after he adds card + we adds money
            if (this.sttngs.actualTrading && msg is TradeViewMessage && this.sttngs.AucBotMode == "bidauc" && (msg as TradeViewMessage).to.profile.name == botname && (msg as TradeViewMessage).to.cardIds.Length == 1 && (msg as TradeViewMessage).to.cardIds[0] == this.sttngs.tradeCardID && (msg as TradeViewMessage).from.gold == this.sttngs.bidgold && (msg as TradeViewMessage).modified == true)
            { // trading with bot, he has added the wanted card, and we added the money... lets click ok in 5 seconds! (if modified = true (if he accept, it will be falls), so we accept only once
                new Thread(new ThreadStart(this.acceptTrade)).Start();
            }

            //if trading with bot and wants to bid on an auction: add money, + wait till he adds card accept
            if (this.sttngs.waitForAuctionBot && !this.sttngs.addedCard && this.sttngs.AucBotMode == "bidauc" && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == botname && (msg as TradeResponseMessage).status == "ACCEPT")
            {
                new Thread(new ThreadStart(this.setGold)).Start();
            }

            //if trading with bot and wants to set an auction: add card, accept after 5 seconds (threaded)
            if (this.sttngs.waitForAuctionBot && !this.sttngs.addedCard && this.sttngs.AucBotMode == "setauc" && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == botname && (msg as TradeResponseMessage).status == "ACCEPT")
            {
                App.Communicator.sendRequest(new TradeAddCardsMessage(new long[] { this.sttngs.tradeCardID }));
                this.sttngs.addedCard = true;
                new Thread(new ThreadStart(this.acceptTrade)).Start();

            }

            //if trading with bot and wants to set some auctions: add cards, accept after 5 seconds (threaded)
            if (this.sttngs.waitForAuctionBot && !this.sttngs.addedCard && this.sttngs.AucBotMode == "multisetauc" && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == botname && (msg as TradeResponseMessage).status == "ACCEPT")
            {


                App.Communicator.sendRequest(new TradeAddCardsMessage(this.helpf.cardsForTradeIds.ToArray()));
                this.sttngs.addedCard = true;
                new Thread(new ThreadStart(this.acceptTrade)).Start();

            }

            // trade was canceled
            if (this.sttngs.waitForAuctionBot && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == botname && (msg as TradeResponseMessage).status == "CANCEL_BARGAIN")
            {
                this.sttngs.AucBotMode = "";
                this.sttngs.waitForAuctionBot = false;
                this.sttngs.actualTrading = false;
            }
            // trade was accepted
            if (this.sttngs.waitForAuctionBot && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == botname && (msg as TradeResponseMessage).status == "ACCEPT")
            {
                // nice!
                this.sttngs.waitForAuctionBot = false;
            }
            // trade was timeouted :D
            if (this.sttngs.waitForAuctionBot && msg is TradeResponseMessage && (msg as TradeResponseMessage).to.name == botname && (msg as TradeResponseMessage).status == "TIMEOUT")
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
                    this.sttngs.auctionScrollsMessagesCounter++;
                }
                if (this.sttngs.auctionScrollsMessagesCounter == 3)
                {
                    App.ArenaChat.ChatRooms.LeaveRoom((msg as RoomChatMessageMessage).roomName);
                    App.Communicator.send(new RoomExitMessage((msg as RoomChatMessageMessage).roomName));
                }

            }

            return;
        }



        public void onConnect(OnConnectData ocd)
        {
            //lol
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
            if (this.sttngs.AucBotMode == "bidauc" || this.sttngs.AucBotMode == "multisetauc" || this.sttngs.AucBotMode == "setauc" || this.sttngs.AucBotMode == "getauc")
            { App.Communicator.sendRequest(new LibraryViewMessage()); }
            this.sttngs.AucBotMode = "";
            this.sttngs.waitForAuctionBot = false;
            this.sttngs.actualTrading = false;
        }
       



    }
}
