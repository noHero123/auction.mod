using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class Network:ICommListener
    {
        public bool contonetwork;
        private DateTime joindate = DateTime.Now;
        public bool rooomsearched=false;
        public int ownroomnumber = 0;
        private Dictionary<string, string> aucusers = new Dictionary<string, string>();
        public int idtesting = 0;
        public bool realycontonetwork = false;
        List<string> roooms = new List<string>();
        Dictionary<string, string> usertoaucroom = new Dictionary<string, string>();
        public bool inbattle=false;

		Searchsettings searchSettings;
		Messageparser messageParser;
		Helpfunktions helpf;
        AuctionHouse ah;

        private static Network instance;

        public static Network Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Network();
                }
                return instance;
            }
        }

		private Network() {
			this.searchSettings = Searchsettings.Instance;
			this.messageParser = Messageparser.Instance;
			this.helpf = Helpfunktions.Instance;
            this.ah = AuctionHouse.Instance;

		}

        public void connect()
        {
            this.contonetwork = true;
			App.Communicator.addListener (this);
            App.Communicator.sendRequest(new RoomEnterMessage("auc-1"));
            this.joindate = DateTime.Now;
        }


		public void handleMessage(Message msg) {
			if (msg is WhisperMessage)
			{
				WhisperMessage wmsg = (WhisperMessage)msg;
				string text = wmsg.text;
                
				if (text.StartsWith("aucdeletes"))
				{
                    ah.removeSeller(wmsg.from);
                
				}
				if (text.StartsWith("aucdeleteb"))
				{
                    ah.removeBuyer(wmsg.from);
				}
                

				if (text.StartsWith("aucs ") || text.StartsWith("aucb "))
				{
					//messageParser.getaucitemsformmsg(text, wmsg.from, wmsg.GetChatroomName());
                    AuctionHouse.Instance.addAuctions(Messageparser.GetAuctionsFromShortMessage(text, wmsg.from));
					//need playerid (wispering doesnt send it)
					if (!helpf.globalusers.ContainsKey(wmsg.from)) { WhisperMessage needid = new WhisperMessage(wmsg.from, "needaucid"); App.Communicator.sendRequest(needid); }
				}

				if(wmsg.from==App.MyProfile.ProfileInfo.name)  return;

				if (text.StartsWith("aucto1please") && this.contonetwork)
				{
					App.Communicator.sendRequest(new RoomExitMessage("auc-" + this.ownroomnumber));
					this.ownroomnumber = 0;
					App.Communicator.sendRequest(new RoomEnterMessage("auc-1"));
					Console.WriteLine("aucto1please");

				}

				if (text.StartsWith("aucstay? ") && this.contonetwork)
				{   // user founded a room, but dont know if this is all
					this.aucstayquestion(text, wmsg.from, searchSettings.shortgeneratedwtsmessage, searchSettings.shortgeneratedwtbmessage);
				}

				if (text.StartsWith("aucstay! "))
				{   // user founded a room, and he dont want to get the room-list
					this.aucstay(text, wmsg.from, searchSettings.shortgeneratedwtsmessage, searchSettings.shortgeneratedwtbmessage);
				}

				if (text.StartsWith("aucrooms ") && !this.rooomsearched && this.contonetwork)
				{
					if (text.EndsWith("aucrooms ")) { this.realycontonetwork = true; }
					else
					{
						this.visitrooms(text);

					}
				}

				if (text.StartsWith("aucstop"))
				{
					this.deleteuser(wmsg.from);
				}



				if (text.StartsWith("aucupdate"))  
				{
					this.sendownauctionstosingleuser(searchSettings.shortgeneratedwtsmessage, searchSettings.shortgeneratedwtbmessage);
				}



				//dont needed anymore left in only to be shure :D
				if (text.StartsWith("needaucid"))
				{
					this.needid(wmsg.from);
				}
				//dont needed anymore
				if (text.StartsWith("aucid "))
				{
					this.saveaucid(text,wmsg.from);
				}
			}
		}

		public void onReconnect ()
		{
		}



        public void disconfromaucnet()
        {
            this.rooomsearched = false;
            this.contonetwork = false;

            
            foreach (KeyValuePair<string, string> pair in this.aucusers)
            {

                senttosingleusr("aucstop", pair.Key);

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

                if (biggestroomnumber > 1) { senttosingleusr("aucto1please", name); };

            };

            App.Communicator.sendRequest(new RoomExitMessage("auc-" + this.ownroomnumber));
            this.aucusers.Clear();
            this.usertoaucroom.Clear();
            this.ownroomnumber = 0;
            this.rooomsearched = false;
            this.contonetwork = false;
            this.realycontonetwork = false;
			App.Communicator.removeListener (this);

        }

        public void enteraucroom(RoomInfoMessage roominfo) 
        {
            // if the update list contains more than 1 user, then ,
            RoomInfoProfile[] rip = roominfo.updated;
            if (rip.Length >= 1) // he joins the room, add him
            {

                foreach (RoomInfoProfile roinpro in rip) // add the new user to the aucusers and globalusers
                {
                    if (!helpf.globalusers.ContainsKey(roinpro.name))
                    {
                        ChatUser newuser = new ChatUser();
                        newuser.acceptChallenges = false;
                        newuser.acceptTrades = true;
                        newuser.adminRole = AdminRole.None;
                        newuser.name = roinpro.name;
                        newuser.id = roinpro.id;
                        helpf.globalusers.Add(roinpro.name, newuser);
                    }
                    if (!this.aucusers.ContainsKey(roinpro.name) && roinpro.name != App.MyProfile.ProfileInfo.name) { this.aucusers.Add(roinpro.name, roinpro.id); }
                    if (!this.usertoaucroom.ContainsKey(roinpro.name)) this.usertoaucroom.Add(roinpro.name, roominfo.roomName.Split('-')[1]);
                }
                if (roominfo.roomName == "auc-1" && rip.Length == 1)
                {//noone is there... you are connected! 
                    this.realycontonetwork = true;
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
                        int sendasks = 0;
                        foreach (KeyValuePair<string, string> pair in this.aucusers)
                        {
                            if (sendasks < 5) // only send the room-asking message to the first 10 users (it would be to spammy)
                            {
                                senttosingleusr("aucstay? " + ownroomnumber, pair.Key);
                                sendasks++;
                            }
                            else { senttosingleusr("aucstay! " + ownroomnumber, pair.Key); }

                        }

                    }
                    else
                    { // stayed in ownroom, but have to visit others :D
                        // so just leave this room (and join the next if multijoin doesnt work)

                        
                        if (ownroomnumber != Convert.ToInt32(roominfo.roomName.Split('-')[1])) // leave room, only when its not the own room
                        {
                            foreach (RoomInfoProfile roinpro in rip)//whisper to the users in this channel, what your channel is
                            {
                                if (App.MyProfile.ProfileInfo.name == roinpro.name) continue;
                                senttosingleusr("aucstay! " + ownroomnumber, roinpro.name);
                            }
                            App.Communicator.sendRequest(new RoomExitMessage(roominfo.roomName));
                        }
                        if (this.roooms.Count >= 1) { App.Communicator.sendRequest(new RoomEnterMessage(this.roooms[0])); this.roooms.RemoveAt(0); }
                        else { this.realycontonetwork = true; }

                    }



                }


            }

                  
        }

        public void deleteuser(string name)
        {
            if (this.usertoaucroom.ContainsKey(name)) { this.usertoaucroom.Remove(name); }
            if (this.aucusers.ContainsKey(name)) { this.aucusers.Remove(name); this.deleteuserfromnet(); }
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
                App.Communicator.sendRequest(new RoomExitMessage("auc-" + this.ownroomnumber));
                this.ownroomnumber = 0;
                App.Communicator.sendRequest(new RoomEnterMessage("auc-1"));
            }


        }

        public void adduser( ChatUser newuser)
        {
            if (!this.aucusers.ContainsKey(newuser.name) && newuser.name != App.MyProfile.ProfileInfo.name) { this.aucusers.Add(newuser.name, newuser.id); }
            this.idtesting--;
        }
        public void addusernoidtest(ChatUser newuser)
        {
            if (!this.aucusers.ContainsKey(newuser.name) && newuser.name != App.MyProfile.ProfileInfo.name) { this.aucusers.Add(newuser.name, newuser.id); }
        }

        public void sendownauctionstosingleuser(string wtsmsg, string wtbmsg)
        {
            foreach (KeyValuePair<string, string> pair in this.aucusers)
            {
                string from = pair.Key;
                if (wtsmsg != "")
                {
                    senttosingleusr(wtsmsg, from);
                }

                if (wtbmsg != "")
                {
                    senttosingleusr(wtbmsg, from);
                }
            }
        }

        public void sendownauctiontoall(bool wts,string wtsdings, string wtbdings)
        {
            if (wts)
            {
                foreach (KeyValuePair<string, string> pair in this.aucusers)
                {
                    if (wtsdings != "")
                    {

                        senttosingleusr(wtsdings, pair.Key);
                    }
                }
            }
            else
            {

                foreach (KeyValuePair<string, string> pair in this.aucusers)
                {
                    if (wtbdings != "")
                    {
                        senttosingleusr(wtbdings, pair.Key);
                    }
                }
            }
        }

       private void senttosingleusr(string msg, string to)
        {
            WhisperMessage wmsg=new WhisperMessage(to, msg);
            if (this.inbattle) App.Communicator.sendBattleRequest(wmsg);
            else
                App.Communicator.sendRequest(wmsg);
        }

       public void aucstayquestion(string text, string from,string wts, string wtb) 
       {
           string stayroom = text.Split(' ')[1];
           
           aucstay(text,from,wts,wtb);

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
           if (respondstring == "") respondstring = " ";
           respondstring = "aucrooms" + respondstring;
           WhisperMessage sendrooms = new WhisperMessage(from, respondstring);
           App.Communicator.sendRequest(sendrooms);
       }

       public void aucstay(string text, string from, string wts, string wtb)
       {
           if (!this.usertoaucroom.ContainsKey(from)) { this.usertoaucroom.Add(from, text.Split(' ')[1]); }//save his aucroom
           else { this.usertoaucroom.Remove(from); this.usertoaucroom.Add(from, text.Split(' ')[1]); }
           //send your offers
           if (wts != "")
           {
               senttosingleusr(wts, from);
           }

           if (wtb != "")
           {
               senttosingleusr(wtb, from);
           }
       }

       public int getnumberofaucusers()
       {
           return this.aucusers.Count();
       }

       public void deleteownmessage(bool wts)
       {
           if (wts)
           {

               foreach (KeyValuePair<string, string> pair in this.aucusers)
               {
                   App.Communicator.sendRequest(new WhisperMessage(pair.Key, "aucdeletes"));
               }
           }
           else
           {
               foreach (KeyValuePair<string, string> pair in this.aucusers)
               {
                   App.Communicator.sendRequest(new WhisperMessage(pair.Key, "aucdeleteb"));
               }
           }
       }

       public void visitrooms(string text)
       {
           Console.WriteLine(text);
           string[] rms = (text.Remove(0, 9)).Split(' ');
           this.roooms.Clear();
           foreach (string str in rms)
           {
               roooms.Add("auc-" + str);
               //Console.WriteLine("auc-" + str);
           }
           //App.Communicator.sendRequest(new RoomEnterMultiMessage(roooms));//doesnt seems to work prooperly, scrolls (not me) is producing an error when i receive an chatmassage form this rooms
           App.Communicator.sendRequest(new RoomEnterMessage(roooms[0]));
           roooms.RemoveAt(0);
           this.rooomsearched = true;
       }

       public void saveaucid(string text,string from)
       {
           if (!helpf.globalusers.ContainsKey(from))
           {
               string id = text.Split(new string[] { "aucid " }, StringSplitOptions.None)[1];
               //test aucid:
               this.idtesting++;
               ProfilePageInfoMessage ppim = new ProfilePageInfoMessage(id);
               App.Communicator.sendRequest(ppim);

           }
           else
           {
               this.addusernoidtest(helpf.globalusers[from]);
           }
       }
		
       public void needid(string from) { WhisperMessage sendid = new WhisperMessage(from, "aucid " + App.MyProfile.ProfileInfo.id); App.Communicator.sendRequest(sendid); }
		public static bool isNetworkCommand(WhisperMessage wmsg) {
			return (wmsg.text).StartsWith ("aucdeletes") || (wmsg.text).StartsWith ("aucdeleteb") || (wmsg.text).StartsWith ("aucupdate") || (wmsg.text).StartsWith ("aucto1please") || (wmsg.text).StartsWith ("aucstay? ") || (wmsg.text).StartsWith ("aucstay! ") || (wmsg.text).StartsWith ("aucrooms ") || (wmsg.text).StartsWith ("aucstop") || (wmsg.text).StartsWith ("aucs ") || (wmsg.text).StartsWith ("aucb ") || (wmsg.text).StartsWith ("needaucid") || (wmsg.text).StartsWith ("aucid ");
		}
    }
}
