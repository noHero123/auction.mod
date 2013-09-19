using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrollsModLoader.Interfaces;
using UnityEngine;
using Irrelevant.Assets;
using System.Reflection;


namespace Auction.mod
{
    class cardviewer : iEffect, iCardRule, ICardListCallback
    {

        struct cardtextures
        {
            public Texture cardimgimg;
            public Rect cardimgrec;
        }

        struct renderwords
        {
            public string text;
            public GUIStyle style;
            public Rect rect;
            public Color color;
        }

        bool mytext = false;
        Texture cardtext;
        float scalefactor = 1.0f;
        public int clicked = 0;
        private GameObject cardRule;
        private List<renderwords> textsArr = new List<renderwords>();
        private List<cardtextures> gameObjects = new List<cardtextures>();
        private FieldInfo icoField;
        private FieldInfo statsBGField;
        private FieldInfo icoBGField;
        private FieldInfo gosNumHitPointsField;
        private FieldInfo gosNumAttackPowerField;
        private FieldInfo gosNumCountdownField;
        private FieldInfo gosNumCostField;
        private FieldInfo gosactiveAbilityField;
        private FieldInfo textsArrField;
        private FieldInfo cardImageField;
        Rect cardrect=new Rect();

        public cardviewer()
        {
            //needed for getting the textures of the drawing card
            statsBGField = typeof(CardView).GetField("statsBG", BindingFlags.Instance | BindingFlags.NonPublic);
            icoBGField = typeof(CardView).GetField("icoBG", BindingFlags.Instance | BindingFlags.NonPublic);
            icoField = typeof(CardView).GetField("ico", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumAttackPowerField = typeof(CardView).GetField("gosNumAttackPower", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumCountdownField = typeof(CardView).GetField("gosNumCountdown", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumCostField = typeof(CardView).GetField("gosNumCost", BindingFlags.Instance | BindingFlags.NonPublic);
            gosNumHitPointsField = typeof(CardView).GetField("gosNumHitPoints", BindingFlags.Instance | BindingFlags.NonPublic);
            textsArrField = typeof(CardView).GetField("textsArr", BindingFlags.Instance | BindingFlags.NonPublic);
            cardImageField = typeof(CardView).GetField("cardImage", BindingFlags.Instance | BindingFlags.NonPublic);
            gosactiveAbilityField = typeof(CardView).GetField("gosActiveAbilities", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void createcard(int arrindex, int cardid)
        {


            CardType type = CardTypeManager.getInstance().get(cardid);
            Card card = new Card(cardid, type, false);
                UnityEngine.Object.Destroy(this.cardRule);
                cardRule = PrimitiveFactory.createPlane();
                cardRule.name = "CardRule";
                CardView cardView = cardRule.AddComponent<CardView>();
                cardView.init(this, card, -1);
                cardView.applyHighResTexture();
                cardView.setLayer(8);
                Vector3 vccopy = Camera.main.transform.localPosition;
                Camera.main.transform.localPosition = new Vector3(0f, 1f, -10f);
                cardRule.transform.localPosition = Camera.main.ScreenToWorldPoint(new Vector3((float)Screen.width * 0.3f, (float)Screen.height * 0.6f, 0.9f)); ;

                cardRule.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
                cardRule.transform.localScale = new Vector3(9.3f, 0.1f, 15.7f);// CardView.CardLocalScale(100f);
                cardtext = cardRule.renderer.material.mainTexture;
                Vector3 ttvec1 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.min);
                Vector3 ttvec2 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.max);
                Rect ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);

                scalefactor = (float)(Screen.height / 1.9) / ttrec.height;
                cardRule.transform.localScale = new Vector3(cardRule.transform.localScale.x * scalefactor, cardRule.transform.localScale.y, cardRule.transform.localScale.z * scalefactor);
                ttvec1 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.min);
                ttvec2 = Camera.main.WorldToScreenPoint(cardRule.renderer.bounds.max);
                ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
                cardrect = ttrec;
                gettextures(cardView);
                this.mytext = true;
                Camera.main.transform.localPosition = vccopy;
                //Console.WriteLine("CARD: " + clink);

                this.clicked = 0;


        }

        private void gettextures(CardView cardView)
        { // get textures from cardview (because cardview issnt painted above ongui drawing, so we draw the textures ongui :D)
            this.gameObjects.Clear();
            GameObject go1 = (GameObject)cardImageField.GetValue(cardView);
            cardtextures temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            Vector3 vec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            Vector3 vec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            Rect rec = new Rect(vec1.x, Screen.height - vec2.y, vec2.x - vec1.x, vec2.y - vec1.y);
            temp1.cardimgrec = rec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }

            // card-texture
            temp1 = new cardtextures();
            temp1.cardimgimg = cardtext;
            temp1.cardimgrec = cardrect;
            this.gameObjects.Add(temp1);
            //icon background
            go1 = (GameObject)icoBGField.GetValue(cardView);
            temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            Vector3 ttvec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            Vector3 ttvec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            Rect ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
            temp1.cardimgrec = ttrec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }
            //stats background
            go1 = (GameObject)statsBGField.GetValue(cardView);
            temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            ttvec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            ttvec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
            temp1.cardimgrec = ttrec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }
            //ico
            go1 = (GameObject)icoField.GetValue(cardView);
            temp1 = new cardtextures();
            temp1.cardimgimg = go1.renderer.material.mainTexture;
            ttvec1 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.min);
            ttvec2 = Camera.main.WorldToScreenPoint(go1.renderer.bounds.max);
            ttrec = new Rect(ttvec1.x, Screen.height - ttvec2.y, ttvec2.x - ttvec1.x, ttvec2.y - ttvec1.y);
            temp1.cardimgrec = ttrec;
            if (go1.renderer.enabled) { this.gameObjects.Add(temp1); }





            List<GameObject> Images = (List<GameObject>)gosNumHitPointsField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled) { this.gameObjects.Add(temp); }
            }

            //ability background
            Images = (List<GameObject>)gosactiveAbilityField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                if (go.name == "Trigger_Ability_Button")
                {
                    cardtextures temp = new cardtextures();
                    temp.cardimgimg = go.renderer.material.mainTexture;
                    Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                    Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                    Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                    temp.cardimgrec = trec;
                    if (go.renderer.enabled) { this.gameObjects.Add(temp); }
                    break;
                }
            }

            Images = (List<GameObject>)gosNumCostField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled) { this.gameObjects.Add(temp); }
            }
            Images = (List<GameObject>)gosNumAttackPowerField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled) { this.gameObjects.Add(temp); }
            }
            Images = (List<GameObject>)gosNumCountdownField.GetValue(cardView);
            foreach (GameObject go in Images)
            {
                cardtextures temp = new cardtextures();
                temp.cardimgimg = go.renderer.material.mainTexture;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                temp.cardimgrec = trec;
                if (go.renderer.enabled) { this.gameObjects.Add(temp); }
            }

            textsArr.Clear();
            Images = (List<GameObject>)textsArrField.GetValue(cardView);

            foreach (GameObject go in Images)
            {
                TextMesh lol = go.GetComponentInChildren<TextMesh>();
                renderwords stuff;
                stuff.text = lol.text;
                Vector3 tvec1 = Camera.main.WorldToScreenPoint(go.renderer.bounds.min);
                Vector3 tvec2 = Camera.main.WorldToScreenPoint(go.renderer.bounds.max);
                Rect trec = new Rect(tvec1.x, Screen.height - tvec2.y, tvec2.x - tvec1.x, tvec2.y - tvec1.y);
                stuff.rect = trec;
                GUIStyle style = new GUIStyle();
                style.font = lol.font;
                style.alignment = (TextAnchor)lol.alignment;
                style.fontSize = (int)(lol.fontSize);
                style.wordWrap = false;
                style.stretchHeight = false;
                style.stretchWidth = false;
                stuff.color = new Color(go.renderer.material.color.r, go.renderer.material.color.g, go.renderer.material.color.b, 0.9f);
                style.normal.textColor = stuff.color;
                stuff.style = style;
                textsArr.Add(stuff);

            }

        }


        public void clearallpics()
        {
            UnityEngine.Object.Destroy(this.cardRule);
            textsArr = new List<renderwords>();
            gameObjects = new List<cardtextures>();
            this.mytext = false;
        }

        public void draw()
        {
            // draw cardoverlay again!
            if (this.mytext)
            {
                if (this.clicked < 3) clicked++;
                //GUI.depth =1;
                Rect rect = new Rect(100, 100, 100, Screen.height - 200);
                foreach (cardtextures cd in this.gameObjects)
                {
                    GUI.DrawTexture(cd.cardimgrec, cd.cardimgimg);
                }

                foreach (renderwords rw in this.textsArr)
                {

                    float width = rw.style.CalcSize(new GUIContent(rw.text)).x;
                    GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0),
                     Quaternion.identity, new Vector3(rw.rect.width / width, rw.rect.width / width, 1));

                    Rect lol = new Rect(rw.rect.x * width / rw.rect.width, rw.rect.y * width / rw.rect.width, rw.rect.width * width / rw.rect.width, rw.rect.height * width / rw.rect.width);
                    GUI.contentColor = rw.color;
                    GUI.Label(lol, rw.text, rw.style);

                }

            }
        }


        //need following 6 methods for icardrule
        public void HideCardView()
        {

        }
        public void SetLoadedImage(Texture2D image, string imageName)
        {
            ResourceManager.instance.assignTexture2D(imageName, image);

        }
        public Texture2D GetLoadedImage(string imageName)
        {
            return ResourceManager.instance.tryGetTexture2D(imageName);
        }
        public void ActivateTriggeredAbility(string id, TilePosition pos)
        {
        }
        public void effectAnimDone(EffectPlayer theEffect, bool loop)
        {
            if (loop)
            {
                theEffect.playEffect(0);
            }
            else
            {
                UnityEngine.Object.Destroy(theEffect);
            }
        }
        public void locator(EffectPlayer effect, AnimLocator loc)
        {
        }
        //icardlistcallback

        public void ButtonClicked(CardListPopup popup, ECardListButton button)
        {

        }
        public void ButtonClicked(CardListPopup popup, ECardListButton button, List<Card> selectedCards)
        {

        }
        public void ItemButtonClicked(CardListPopup popup, Card card)
        {

        }
        public void ItemHovered(CardListPopup popup, Card card)
        {
        }
        public void ItemClicked(CardListPopup popup, Card card)
        {
        }

        
    }
}
