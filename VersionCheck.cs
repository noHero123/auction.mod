﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;
using System.Threading;

namespace Auction.mod
{
    class VersionCheck : ICommListener
    {


        //bool getdata = false;
        int anzWarnings = 1;
        int warnings = 0;
        string currentversion = "1.0.3.8";// only change this and the  version-file in github 
        string newestversion = "0.0.0.0";// dont change this
        PopupManager pppmngr;

        public VersionCheck()
        {
            return;
            //isnt needed anymore.. i realised that summoner has an autoupdate function :D
            this.pppmngr = PopupManager.Instance;
            new Thread(new ThreadStart(this.workthread)).Start();
            try
            {
                App.Communicator.addListener(this);
            }
            catch { }
            
        }

        public void workthread()
        {
            string s = getDataFromGoogleDocs();
            //readJsonfromGoogle(s);
            s=s.Replace(" ","");
            s = s.Replace("\r", "");
            s = s.Replace("\n", "");
            this.newestversion=s;
        }

            public string getDataFromGoogleDocs()
        {
            WebRequest myWebRequest;
            //myWebRequest = WebRequest.Create("https://spreadsheets.google.com/feeds/list/0AhhxijYPL-BGdDBMUy1kdFFpa19IQTk5Ukd0T3JNU3c/od6/public/values?alt=json");
            myWebRequest = WebRequest.Create("https://raw.github.com/noHero123/auction.mod/master/Version.txt");
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            return ressi;
        }


            public void readJsonfromGoogle(string txt)// not needed anymore, but who knows? :D
            {
                JsonReader jsonReader = new JsonReader();
                Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(txt);
                dictionary = (Dictionary<string, object>)dictionary["feed"];
                Dictionary<string, object>[] entrys = (Dictionary<string, object>[])dictionary["entry"];
                for (int i = 0; i < entrys.GetLength(0); i++)
                {

                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r1"];
                    if (((string)dictionary["$t"]).ToLower() == "version")
                    {
                        dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                        this.newestversion=(string)dictionary["$t"];
                        Console.WriteLine("version string recived");
                    }

                }
            }

        public void handleMessage(Message msg)
        { // collect data for enchantments (or units who buff)
            
            if (msg is RoomChatMessageMessage)
            {
                RoomChatMessageMessage rcmm = (RoomChatMessageMessage)msg;
                if (rcmm.text.StartsWith("You have joined"))
                {
                    Console.WriteLine("##Aucversion:#" + currentversion + "#" + newestversion+"#");
                    if (this.currentversion != this.newestversion)
                    {
                        RoomChatMessageMessage nrcmm = new RoomChatMessageMessage("[note]", "your Auctionmod is outdated, please visit www.scrollsguide.com/forum and install a new version or check noHeros repository\r\n" + "your version: " + this.currentversion + "\r\n" + "latest version: " + this.newestversion);
                        nrcmm.from = "Version Checker";
                        App.ArenaChat.handleMessage(nrcmm);
                        this.pppmngr.startOKPopup("versioncheck", "Update available", "your Auctionmod is outdated, please visit www.scrollsguide.com/forum and install a new version or check noHeros repository\r\n" + "your version: " + this.currentversion + "\r\n" + "latest version: " + this.newestversion);
                    }
                    warnings++;
                    if (warnings >= anzWarnings)
                    {
                        App.Communicator.removeListener(this);
                    }
                }
            }


            return;
        }
        public void onConnect(OnConnectData ocd)
        {
            return; // don't care
        }

       

    }
}
