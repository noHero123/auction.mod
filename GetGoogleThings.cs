using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;

namespace Auction.mod
{
    public class GetGoogleThings : ICommListener, IOkCallback, ICancelCallback, IOkCancelCallback, IOkStringCallback, IOkStringCancelCallback
    {
        public bool loadeddata=false;
        public volatile bool workthreadready = true;
        public volatile bool workthreadreadyOwnOffers = true;
        public volatile bool workthreadreadyCreateOffers = true;
        public volatile bool dataisready = false;
        public volatile bool dataisreadyOwnOffers = false;
        private int dataOffer = 0;

        public Card sellingCard = null;

        public long createCardID = -1;

        private PlayerStore pstore;

        public int clickedItemLevel = 0;
        public int clickedItemForSales = 0;
        public int clickedItemPrice = 0;
        public long clickedItemBuyID = -1;
        public string clickedItemName = "";

        Dictionary<int, TransactionInfo> soldScrollTransactions = new Dictionary<int, TransactionInfo>();
        TransactionInfo transactionBeingClaimed = null;

        public struct sharedItem
        {
            public string time;
            public string status;
            public string id;
            public string seller;
        }

        public List<sharedItem> pStoreItems = new List<sharedItem>();
        public List<Auction> pstoreAucs = new List<Auction>();


        private static GetGoogleThings instance;

        public static GetGoogleThings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GetGoogleThings();
                }
                return instance;
            }
        }

        private GetGoogleThings()
        {
            this.pstore = PlayerStore.Instance;
            try
            {
                App.Communicator.addListener(this);
            }
            catch
            {
                Console.WriteLine("cant add listener");
            }
        }

        public void onConnect(OnConnectData ocd)
        {
            //lol
        }

        public void handleMessage(Message msg)
        {
            if (msg is OkMessage)
            {
                OkMessage omsg = (OkMessage)msg;
                if (Helpfunktions.Instance.createAuctionMenu)
                {
                    
                    if (omsg.op == "MarketplaceCreateOffer")
                    {
                        this.dataOffer = 0;
                        App.Communicator.send(new MarketplaceOffersViewMessage());
                        App.Communicator.send(new MarketplaceSoldListViewMessage());
                        App.Communicator.sendRequest(new LibraryViewMessage());
                    }
                    if (omsg.op == "MarketplaceCancelOffer")
                    {
                        this.dataOffer = 0;
                        App.Communicator.send(new MarketplaceOffersViewMessage());
                        App.Communicator.send(new MarketplaceSoldListViewMessage());
                        App.Communicator.sendRequest(new LibraryViewMessage());
                    }

                    if (omsg.op == "MarketplaceClaim")
                    {
                        
                        if (transactionBeingClaimed == null) return;
                        App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_coin_tally_end");
                        CardType cardType = CardTypeManager.getInstance().get(this.transactionBeingClaimed.cardType);
                        this.dataOffer = 0;
                        App.Communicator.send(new MarketplaceOffersViewMessage());
                        App.Communicator.send(new MarketplaceSoldListViewMessage());
                        App.Communicator.sendRequest(new LibraryViewMessage());
                        App.Popups.ShowOk(this, "claimgold", "Gold added", string.Concat(new object[]
							{
								"<color=#bbaa88>Tier ",
								(int)(this.transactionBeingClaimed.level + 1),
								" ",
								cardType.name,
								" sold for ",
								this.transactionBeingClaimed.sellPrice,
								" gold!\nEarned <color=#ffd055>",
								this.transactionBeingClaimed.sellPrice - this.transactionBeingClaimed.fee,
								" gold</color> (the fence collects ",
								this.transactionBeingClaimed.fee,
								").</color>"
							}), "Ok");
                    }
                }
                if (Helpfunktions.Instance.playerStoreMenu)
                {
                    if (omsg.op == "MarketplaceMakeDeal")
                    {
                        App.Communicator.sendRequest(new GetStoreItemsMessage());
                        App.Communicator.sendRequest(new LibraryViewMessage());
                        App.Popups.ShowOk(this, "dealmade", "Purchase complete!", clickedItemName + " has been added to your collection.", "Ok");
                    }
                }

            }


            if (msg is CheckCardDependenciesMessage)
            {
                CheckCardDependenciesMessage checkCardDependenciesMessage = (CheckCardDependenciesMessage)msg;
                if (checkCardDependenciesMessage.dependencies == null || checkCardDependenciesMessage.dependencies.Length == 0)
                {
                    this.GetCreateOfferInfo();
                }
                else
                {
                    App.Popups.ShowOkCancel(this, "deckinvalidationwarning", "Notice", "Selling this scroll will make the following decks illegal:\n\n" + DeckUtil.GetFormattedDeckNames(checkCardDependenciesMessage.GetDeckNames()), "Ok", "Cancel");
                }
            }

            if (msg is MarketplaceCreateOfferInfoMessage)
            {
                MarketplaceCreateOfferInfoMessage marketplaceCreateOfferInfoMessage = (MarketplaceCreateOfferInfoMessage)msg;
                App.Popups.ShowSellCard(this, "sellcard", this.sellingCard, marketplaceCreateOfferInfoMessage.lowestPrice, marketplaceCreateOfferInfoMessage.suggestedPrice, marketplaceCreateOfferInfoMessage.copiesForSale, marketplaceCreateOfferInfoMessage.tax);
            }

            if (msg is MarketplaceOffersSearchViewMessage)
            {
                MarketplaceOffersSearchViewMessage marketplaceOffersViewMessage = (MarketplaceOffersSearchViewMessage)msg;
                clickedItemForSales = marketplaceOffersViewMessage.copiesForSale;
                clickedItemLevel = marketplaceOffersViewMessage.offer.card.level;
                clickedItemPrice = marketplaceOffersViewMessage.offer.price;
                clickedItemBuyID = marketplaceOffersViewMessage.offer.id;
                clickedItemName = marketplaceOffersViewMessage.offer.card.getName();

            }

            if (msg is MarketplaceOffersViewMessage)
            {
                //if (this.dataisreadyOwnOffers) return;

                MarketplaceOffersViewMessage marketplaceOffersViewMessage = (MarketplaceOffersViewMessage)msg;
                MarketplaceOffer[] offers = marketplaceOffersViewMessage.offers;
                this.pstoreAucs.Clear();
                DateTime tme = DateTime.Now;
                tme = tme.AddMilliseconds(1000);
                for (int i = 0; i < offers.Length; i++)
                {
                    MarketplaceOffer marketplaceOffer = offers[i];
                    Auction a = new Auction(App.MyProfile.ProfileInfo.name, tme, Auction.OfferType.SELL, marketplaceOffer.card, "" + marketplaceOffer.id, marketplaceOffer.price);
                    tme = tme.AddMilliseconds(1);
                    //Console.WriteLine("add owm auction: " + a.card.getName() + " " + a.price);
                    this.pstoreAucs.Add(a);
                }

                this.dataOffer++;
                if (this.dataOffer >= 2) this.dataisreadyOwnOffers = true;
            }

            if (msg is MarketplaceSoldListViewMessage)
            {
                //if (this.dataisreadyOwnOffers) return;

                MarketplaceSoldListViewMessage marketplaceOffersViewMessage = (MarketplaceSoldListViewMessage)msg;
                TransactionInfo[] offers = marketplaceOffersViewMessage.sold;
                //this.pstoreAucs.Clear();
                this.soldScrollTransactions.Clear();
                DateTime tme = DateTime.Now;
                
                for (int i = 0; i < offers.Length; i++)
                {
                    TransactionInfo marketplaceOffer = offers[i];
                    if(!marketplaceOffer.claimed) this.soldScrollTransactions.Add(marketplaceOffer.cardId, marketplaceOffer);
                    CardType type = CardTypeManager.getInstance().get(marketplaceOffer.cardType);
                    Card c = new Card(marketplaceOffer.cardId, type, true);
                    string aucmessage="sold " + marketplaceOffer.fee;
                    if (marketplaceOffer.claimed)
                    {
                        aucmessage += " claimed";
                        continue;
                    }
                    Auction a = new Auction(App.MyProfile.ProfileInfo.name, tme, Auction.OfferType.SELL, c, aucmessage, marketplaceOffer.sellPrice, marketplaceOffer.cardId);
                    tme = tme.AddMilliseconds(1);
                    //Console.WriteLine("add owm auction: " + a.card.getName() + " " + a.price);
                    this.pstoreAucs.Add(a);
                }
                this.dataOffer++;
                if (this.dataOffer >= 2) this.dataisreadyOwnOffers = true;
            }

            if (msg is MarketplaceAvailableOffersListViewMessage)
            {
                Prices.Instance.getBlackmarketPrices(msg as MarketplaceAvailableOffersListViewMessage);
                if (this.dataisready) return;

                MarketplaceAvailableOffersListViewMessage marketplaceAvailableOffersListViewMessage = (MarketplaceAvailableOffersListViewMessage)msg;

                MarketplaceTypeAvailability[] available = marketplaceAvailableOffersListViewMessage.available;
                this.pstoreAucs.Clear();
                DateTime tme = DateTime.Now;
                for (int i = 0; i < available.Length; i++)
                {
                    MarketplaceTypeAvailability mta = available[i];
                    CardType type = CardTypeManager.getInstance().get(mta.type);
                    Card card = new Card(1, type, true);
                    card.level = mta.level;
                    Auction a = new Auction("BlackMarket", tme, Auction.OfferType.SELL, card, "", mta.price);
                    tme = tme.AddMilliseconds(1);
                    Console.WriteLine("add auction: " + a.card.getName() + " " + a.price);
                    this.pstoreAucs.Add(a);
                }

                this.dataisready = true;
            }
            return;
        }

        private void GetCreateOfferInfo()
        {
            App.Communicator.send(new MarketplaceCreateOfferInfoMessage(this.sellingCard.getCardType().id, (byte)this.sellingCard.level));
        }

        public void PopupOk(string popupType)
        {
            if (popupType == "deckinvalidationwarning")
            {
                this.GetCreateOfferInfo();
            }
        }
        public void PopupOk(string popupType, string value)
        {
            if (popupType == "sellcard")
            {
                int price = 0;
                bool flag = int.TryParse(value, out price);
                if (flag)
                {
                    App.Communicator.send(new MarketplaceCreateOfferMessage(this.sellingCard.getId(), price));
                }
                else
                {
                    App.Popups.ShowOk(this, "infopopup", "Error", "Something went wrong! Did you enter a numeric price?", "Ok");
                }
            }
        }

        public void PopupCancel(string popupType)
        {
            if (popupType == "deckinvalidationwarning")
            {
                this.sellingCard = null;
            }
        }




        public void getOffersFromMarketPlace()
        {
            this.dataisready = false;
            App.Communicator.send(new MarketplaceAvailableOffersListViewMessage());
        }

        public void getOwnOffersFromMarketPlace()
        {
            this.dataisreadyOwnOffers = false;
            this.dataOffer = 0;
            App.Communicator.send(new MarketplaceOffersViewMessage());
            App.Communicator.send(new MarketplaceSoldListViewMessage());
        }


        public int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (int)(dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

        public DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); 
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public void addDataToPlayerStore()
        {
            this.pstore.removeAllMessages();
            List<Auction> auctionsToAdd = new List<Auction>(this.pstoreAucs);
            this.pstore.addAuctions(auctionsToAdd);
            this.dataisready = false;
        }

        public void addOwnDataToPlayerStore()
        {
            Console.WriteLine("add data to own ps");
            this.pstore.removeAllOwnMessages();
            List<Auction> auctionsToAdd = new List<Auction>(this.pstoreAucs);
            this.pstore.addOwnAuctions(auctionsToAdd);
            this.dataOffer = 0;
            this.dataisreadyOwnOffers = false;
        }


        public void workthread()
        {
            this.workthreadready = false;
            Console.WriteLine("collect data");
            pStoreItems.Clear();
            try
            {
                this.getOffersFromMarketPlace();
                System.Threading.Thread.Sleep(1000);
            }
            catch
            {
                Console.WriteLine("google error!");
            }

            this.workthreadready = true;
        }

        public void workthreadOwnOffers()
        {
            this.workthreadreadyOwnOffers = false;
            Console.WriteLine("collect own data");
            pStoreItems.Clear();
            try
            {
                this.getOwnOffersFromMarketPlace();
                System.Threading.Thread.Sleep(1000);
            }
            catch
            {
                Console.WriteLine("google error!");
            }

            this.workthreadreadyOwnOffers = true;
        }

        public void ClaimSalesMoney(int cardid)
        {
            if (this.soldScrollTransactions.ContainsKey(cardid))
            {
                TransactionInfo transactionInfo = this.soldScrollTransactions[cardid];
                this.transactionBeingClaimed = transactionInfo;
                App.Communicator.send(new MarketplaceClaimMessage(transactionInfo.transactionId));
                this.soldScrollTransactions.Remove(cardid);
            }
        }


    }
}
