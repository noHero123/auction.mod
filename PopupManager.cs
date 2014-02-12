using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auction.mod
{
    class PopupManager : IOkCallback
    {

        public void startOKPopup(string popptype,string headerstring,string descriptionstring)
        {
            App.Popups.ShowOk(this, popptype, headerstring, descriptionstring, "OK");
        }

        public void PopupOk(string popupType)
        {



            if (popupType == "auctionmodresponse")
            { }
        }


    }
}
