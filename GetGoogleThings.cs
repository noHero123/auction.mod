using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;

namespace Auction.mod
{
    public class GetGoogleThings : IOkCallback, ICancelCallback, IOkCancelCallback, IOkStringCallback, IOkStringCancelCallback
    {
        public bool loadeddata=false;

        public volatile bool workthreadclaimall = false;
        private int claimeditems = 0;
        private int claimeditemstaxes = 0;
        private int claimeditemsmoney = 0;

        public volatile bool workthreadready = true;
        public volatile bool workthreadreadyOwnOffers = true;
        public volatile bool workthreadreadyCreateOffers = true;
        public volatile bool dataisready = false;
        public volatile bool dataisreadyOwnOffers = false;
        private int dataOffer = 0;

        public Card sellingCard = null;
        public int sellingType = 1;
        public int sellingTypeLevel = 0;

        public int cancelType = 1;
        public int cancelTypeLevel = 0;


        public long createCardID = -1;

        private PlayerStore pstore;

        public int clickedItemLevel = 0;
        public int clickedItemForSales = 0;
        public int clickedItemPrice = 0;
        public long clickedItemBuyID = -1;
        public int clickedItemtypeid = -1;
        public string clickedItemName = "";
        public int clickedItemPriceFromOfferMessage = 0;

        Dictionary<int, TransactionInfo> soldScrollTransactions = new Dictionary<int, TransactionInfo>();
        TransactionInfo transactionBeingClaimed = null;

        public struct sharedItem
        {
            public string time;
            public string status;
            public string id;
            public string seller;
        }

        bool needSoldAucs = true;

        public List<sharedItem> pStoreItems = new List<sharedItem>();

        public List<Auction> pstoreAucs = new List<Auction>();
        public List<Auction> pstoreOwnAucs = new List<Auction>();

        public List<Auction> pstoreSOLDAucs = new List<Auction>();


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
        }

        public void onConnect(OnConnectData ocd)
        {
            //lol
        }

        public void handleMessage(Message msg)
        {

            if (msg is MessageMessage)
            {
                MessageMessage omsg = (MessageMessage)msg;
                if (omsg.type == MessageMessage.Type.SOLD_MARKET_SCROLLS)
                {
                    this.needSoldAucs = true;
                    App.Communicator.send(new MarketplaceSoldListViewMessage());
                }
            }

            if (msg is OkMessage)
            {
                OkMessage omsg = (OkMessage)msg;
                if (Helpfunktions.Instance.createAuctionMenu)
                {

                    if (omsg.op == "MarketplaceCreateOffer" && this.sellingCard != null)
                    {
                        Helpfunktions.Instance.cardIDToNumberOwned[this.sellingType]--;
                        Helpfunktions.Instance.cardIDToNumberOwnedTiered[this.sellingType] -= (int)Math.Pow(3, sellingTypeLevel);
                        PlayerStore.Instance.createCardsFilter.filtersChanged = true;
                        PlayerStore.Instance.sellOfferFilter.filtersChanged = true;

                        this.dataOffer = 0;
                        this.sellingCard = null;
                        App.Communicator.send(new MarketplaceOffersViewMessage());
                        //App.Communicator.send(new MarketplaceSoldListViewMessage());
                        App.Communicator.sendRequest(new LibraryViewMessage());
                    }
                    if (omsg.op == "MarketplaceCancelOffer")
                    {
                        Helpfunktions.Instance.cardIDToNumberOwned[this.cancelType]++;
                        Helpfunktions.Instance.cardIDToNumberOwnedTiered[this.cancelType] += (int)Math.Pow(3, cancelTypeLevel);
                        PlayerStore.Instance.createCardsFilter.filtersChanged = true;
                        PlayerStore.Instance.sellOfferFilter.filtersChanged = true;

                        this.dataOffer = 0;
                        App.Communicator.send(new MarketplaceOffersViewMessage());
                        //App.Communicator.send(new MarketplaceSoldListViewMessage());
                        App.Communicator.sendRequest(new LibraryViewMessage());
                    }

                    if (omsg.op == "MarketplaceClaim")
                    {
                        
                        if (transactionBeingClaimed == null) return;
                        if (this.workthreadclaimall)
                        {
                            this.claimeditemstaxes += this.transactionBeingClaimed.fee;
                            this.claimeditemsmoney += this.transactionBeingClaimed.sellPrice;
                            System.Threading.Thread.Sleep(150);
                            this.claimlast();
                            return;
                        }
                        App.AudioScript.PlaySFX("Sounds/hyperduck/UI/ui_coin_tally_end");
                        CardType cardType = CardTypeManager.getInstance().get(this.transactionBeingClaimed.cardType);
                        this.dataOffer = 0;
                        this.needSoldAucs = true;
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
                        this.transactionBeingClaimed = null;
                    }
                }

                if (Helpfunktions.Instance.playerStoreMenu)
                {
                    if (omsg.op == "MarketplaceMakeDeal")
                    {
                        App.Communicator.sendRequest(new GetStoreItemsMessage());
                        App.Communicator.sendRequest(new LibraryViewMessage());
                        App.Popups.ShowOk(this, "dealmade", "Purchase complete!", clickedItemName + " has been added to your collection.", "Ok");
                        Helpfunktions.Instance.cardIDToNumberOwned[this.clickedItemtypeid]++;
                        Helpfunktions.Instance.cardIDToNumberOwnedTiered[this.clickedItemtypeid]+=(int)Math.Pow(3, clickedItemLevel);
                        PlayerStore.Instance.createCardsFilter.filtersChanged = true;
                        PlayerStore.Instance.sellOfferFilter.filtersChanged = true;
                        clickedItemBuyID = -1;
                    }
                }

            }

            if (msg is FailMessage)
            {
                FailMessage failMessage = (FailMessage)msg;
                if (failMessage.isType(typeof(MarketplaceMakeDealMessage)))
                {
                    App.Popups.ShowOk(this, "dealNOTmade", "Purchase failed", failMessage.info, "Ok");
                    PlayerStore.Instance.createCardsFilter.filtersChanged = true;
                    PlayerStore.Instance.sellOfferFilter.filtersChanged = true;
                    clickedItemBuyID = -1;
                }

                if (failMessage.isType(typeof(MarketplaceCreateOfferMessage)) && this.sellingCard != null)
                {
                    App.Popups.ShowOk(this, "cantcreate", "Create failed", failMessage.info, "Ok");
                    this.sellingCard = null;
                }

                if (failMessage.isType(typeof(MarketplaceClaimMessage)))
                {
                    if (transactionBeingClaimed == null) return;
                    if (this.workthreadclaimall)
                    {
                        System.Threading.Thread.Sleep(150);
                        this.claimlast();
                        return;
                    }
                    this.dataOffer = 0;
                    this.needSoldAucs = true;
                    App.Communicator.send(new MarketplaceOffersViewMessage());
                    App.Communicator.send(new MarketplaceSoldListViewMessage());
                    App.Communicator.sendRequest(new LibraryViewMessage());
                    transactionBeingClaimed = null;
                }
            }

            if (msg is CheckCardDependenciesMessage && sellingCard !=null )
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

            if (msg is MarketplaceCreateOfferInfoMessage && sellingCard != null)
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
                clickedItemtypeid = marketplaceOffersViewMessage.offer.card.getType();

            }

            if (msg is MarketplaceOffersViewMessage)
            {
                //if (this.dataisreadyOwnOffers) return;

                MarketplaceOffersViewMessage marketplaceOffersViewMessage = (MarketplaceOffersViewMessage)msg;
                MarketplaceOffer[] offers = marketplaceOffersViewMessage.offers;
                this.pstoreOwnAucs.Clear();
                DateTime tme = DateTime.Now;
                tme = tme.AddMilliseconds(1000);
                for (int i = 0; i < offers.Length; i++)
                {
                    MarketplaceOffer marketplaceOffer = offers[i];
                    Auction a = new Auction(App.MyProfile.ProfileInfo.name, tme, Auction.OfferType.SELL, marketplaceOffer.card, "" + marketplaceOffer.id, marketplaceOffer.price);
                    tme = tme.AddMilliseconds(1);
                    //Console.WriteLine("add owm auction: " + a.card.getName() + " " + a.price);
                    this.pstoreOwnAucs.Add(a);
                }

                this.dataOffer++;
                
                if (this.dataOffer >= 2 || !this.needSoldAucs)
                {
                    this.dataisreadyOwnOffers = true;
                }
                if (this.dataOffer >= 2)
                {
                    this.needSoldAucs = false;
                }
            }

            if (msg is MarketplaceSoldListViewMessage)
            {
                //if (this.dataisreadyOwnOffers) return;

                MarketplaceSoldListViewMessage marketplaceOffersViewMessage = (MarketplaceSoldListViewMessage)msg;
                TransactionInfo[] offers = marketplaceOffersViewMessage.sold;
                this.pstoreSOLDAucs.Clear();
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
                    this.pstoreSOLDAucs.Add(a);
                }
                this.dataOffer++;
                if (this.dataOffer >= 2) this.dataisreadyOwnOffers = true;
                this.needSoldAucs = false;
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
                    tme = tme.AddMilliseconds(-1);
                    //Console.WriteLine("add auction: " + a.card.getName() + " " + a.price);
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

            if (popupType == "claimgold")
            {
                //Console.WriteLine("#transaction was claimed");
                
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
                    this.sellingType = this.sellingCard.getType();
                    this.sellingTypeLevel = this.sellingCard.level;
                    App.Communicator.send(new MarketplaceCreateOfferMessage(this.sellingCard.getId(), price));
                    //this.sellingCard = null;
                }
                else
                {
                    this.sellingCard = null;
                    App.Popups.ShowOk(this, "infopopup", "Error", "Something went wrong! Did you enter a numeric price?", "Ok");
                }
            }
        }

        public void PopupCancel(string popupType)
        {
            if (popupType == "deckinvalidationwarning" || popupType == "sellcard")
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
            this.dataisready = false;
            App.Communicator.send(new MarketplaceOffersViewMessage());
            if(needSoldAucs)App.Communicator.send(new MarketplaceSoldListViewMessage());
            //for prices:
            App.Communicator.send(new MarketplaceAvailableOffersListViewMessage());
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
            List<Auction> auctionsToAdd = new List<Auction>(this.pstoreOwnAucs);
            List<Auction> auctionsToAdd2 = new List<Auction>(this.pstoreSOLDAucs);
            auctionsToAdd.AddRange(auctionsToAdd2);
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
            if (this.workthreadclaimall == true) return;

            if (this.soldScrollTransactions.ContainsKey(cardid))
            {
                TransactionInfo transactionInfo = this.soldScrollTransactions[cardid];
                this.transactionBeingClaimed = transactionInfo;
                App.Communicator.send(new MarketplaceClaimMessage(transactionInfo.transactionId));
                this.soldScrollTransactions.Remove(cardid);
            }
        }

        private void claimlast()
        {
            int cardid = -1;
            TransactionInfo transactionInfo = null;
            foreach (KeyValuePair<int, TransactionInfo> kvp in this.soldScrollTransactions)
            {
                cardid = kvp.Key;
                transactionInfo = kvp.Value;
            }
            if (cardid != -1)
            {
                this.claimeditems++;
                this.soldScrollTransactions.Remove(cardid);
                this.transactionBeingClaimed = transactionInfo;
                App.Communicator.send(new MarketplaceClaimMessage(transactionInfo.transactionId));
            }
            else
            {
                //finished claiming
                if (this.claimeditems >= 1)
                {
                    this.dataOffer = 0;
                    this.needSoldAucs = true;
                    App.Communicator.send(new MarketplaceOffersViewMessage());
                    App.Communicator.send(new MarketplaceSoldListViewMessage());
                    App.Communicator.sendRequest(new LibraryViewMessage());
                    App.Popups.ShowOk(this, "claimgold", "Gold added", string.Concat(new object[]
							{   
                                "<color=#bbaa88>",
								"Your Scrolls where sold for ",
								this.claimeditemsmoney,
								" gold!\nEarned <color=#ffd055>",
								this.claimeditemsmoney-this.claimeditemstaxes,
								" gold</color> (the fence collects ",
								this.claimeditemstaxes,
								").</color>"
							}), "Ok");
                }

                this.claimeditems = 0;
                this.claimeditemstaxes = 0;
                this.claimeditemsmoney = 0;
                this.transactionBeingClaimed = null;
                this.workthreadclaimall = false;
            }

        }

        public void ClaimAllThread()
        {
            if (this.workthreadclaimall == true) return;
            this.workthreadclaimall = true;
            this.claimeditems = 0;
            this.claimeditemstaxes = 0;
            this.claimeditemsmoney = 0;
            claimlast();

            //this.workthreadclaimall = false;
        }


    }
}
