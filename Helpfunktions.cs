using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonFx.Json;
using UnityEngine;
using System.Reflection;

namespace Auction.mod
{
    class Helpfunktions
    {
        public FieldInfo targetchathightinfo;
        public ChatUI target = null;
        public MethodInfo hideInformationinfo;
        public FieldInfo buymen, sellmen;

        public Store storeinfo;
        public FieldInfo showBuyinfo;
        public FieldInfo showSellinfo;

        public bool chatisshown = false;
        public bool wtsmsgload = false;
        public bool wtbmsgload = false;
        public string ownaucpath = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "auc" + System.IO.Path.DirectorySeparatorChar;
        public bool inauchouse = false;
        public bool wtsmenue=false;
        public bool generator=false;
        public bool settings = false;
        public string postmsgmsg = "";
        public bool postmsgontrading=false;
        public bool postmsggetnextroomenter = false;
        public bool showtradedialog=false;
        public int[] cardids;
        public string[] cardnames;
        public int[] cardImageid;
        public string[] cardType;
        public Dictionary<string, ChatUser> globalusers = new Dictionary<string, ChatUser>();
        public GUISkin cardListPopupSkin;
        public GUISkin cardListPopupGradientSkin;
        public GUISkin cardListPopupBigLabelSkin;
        public GUISkin cardListPopupLeftButtonSkin;
        public GUISkin lobbySkin;
        public GUIStyle chatLogStyle;



        public int cardnametoimageid(string name) { return cardImageid[Array.FindIndex(cardnames, element => element.Equals(name))]; }

        public void setskins(GUISkin cllps, GUISkin clpgs, GUISkin clpbls, GUISkin clplbs)
        {
            this.cardListPopupSkin = cllps;
            this.cardListPopupGradientSkin = clpgs;
            this.cardListPopupBigLabelSkin = clpbls;
            this.cardListPopupLeftButtonSkin = clplbs;

        }

        public void setlobbyskin(GUISkin lby)
        {
            this.lobbySkin = lby;
        }

        public void setchatlogstyle(GUIStyle cls)
        {
            this.chatLogStyle = cls;
        }


        public void  setarrays(Message msg)
        {
            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(msg.getRawText());
            Dictionary<string, object>[] d = (Dictionary<string, object>[])dictionary["cardTypes"];
            this.cardids = new int[d.GetLength(0)];
            this.cardnames = new string[d.GetLength(0)];
            this.cardImageid = new int[d.GetLength(0)];
            this.cardType = new string[d.GetLength(0)];
            for (int i = 0; i < d.GetLength(0); i++)
            {
                this.cardids[i] = Convert.ToInt32(d[i]["id"]);
                this.cardnames[i] = d[i]["name"].ToString().ToLower();
                this.cardImageid[i] = Convert.ToInt32(d[i]["cardImage"]);
                this.cardType[i] = d[i]["kind"].ToString();
            }
        }
    }
}
