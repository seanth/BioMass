using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac;
using UnityEngine;

namespace KSPBioMass
{
    public class BioVesselMonitoringWindow : Window<BioVesselMonitoringWindow>
    {
        private readonly SaveGame saveGame;
        private readonly string version;

        private GUIStyle labelStyle;
        private GUIStyle warningStyle;
        private GUIStyle criticalStyle;
        private GUIStyle headerStyle;
        private Vector2 scrollPosition;

        public GUIStyle styleBarDef;
        public GUIStyle styleBarGreen;
        public GUIStyle styleBarGreen_Back;

        public Texture2D texBarGreen;
        public Texture2D texBarGreen_Back;

        public GUIStyle styleBarGreen_Thin;
        public GUIStyle styleBarText;
        public GUIStyle styleButton;
        public GUIStyle stylePanel;

        public BioVesselMonitoringWindow(FlightController controller, Settings globalSettings, SaveGame gameSettings)
            : base("BioMass Monitoring", 500, 300)
        {
            this.saveGame = gameSettings;

            windowPos.y = 50;

            version = Utilities.GetDllVersion(this);
            LoadTextures();
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.margin.top = 0;
                labelStyle.margin.bottom = 0;
                labelStyle.padding.top = 0;
                labelStyle.padding.bottom = 0;
                labelStyle.normal.textColor = Color.white;
                labelStyle.wordWrap = false;

                warningStyle = new GUIStyle(labelStyle);
                warningStyle.normal.textColor = Color.yellow;

                criticalStyle = new GUIStyle(labelStyle);
                criticalStyle.normal.textColor = Color.red;

                headerStyle = new GUIStyle(labelStyle);
                headerStyle.fontStyle = FontStyle.Bold;

                styleButton = new GUIStyle(GUI.skin.button);
                styleButton.normal.background = HighLogic.Skin.button.normal.background;
                styleButton.hover.background = HighLogic.Skin.button.hover.background;
                styleButton.normal.textColor = new Color(207, 207, 207);
                styleButton.fontStyle = FontStyle.Normal;
                styleButton.contentOffset = new Vector2(0,2);
                styleButton.margin = new RectOffset(0,0,0,0);
                styleButton.fixedHeight = 18;
                styleButton.fixedWidth = 100;

                styleBarDef = new GUIStyle(GUI.skin.box);
                styleBarDef.fixedHeight = 18;
                styleBarDef.border = new RectOffset(2, 2, 2, 2);
                styleBarDef.normal.textColor = Color.white;

                styleBarGreen = new GUIStyle(styleBarDef);
                styleBarGreen.normal.background = texBarGreen;

                styleBarGreen_Back = new GUIStyle(styleBarDef);
                styleBarGreen_Back.normal.background = texBarGreen_Back;

                styleBarGreen_Thin = new GUIStyle(styleBarGreen);
                styleBarGreen_Thin.border = new RectOffset(0, 0, 0, 0);

                styleBarText = new GUIStyle(GUI.skin.label);
                styleBarText.fontSize = 12;
                styleBarText.alignment = TextAnchor.MiddleCenter;
                styleBarText.normal.textColor = new Color(255, 255, 255, 0.8f);
                styleBarText.wordWrap = false;
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            if (FlightGlobals.ready)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.BeginVertical();
                GUILayout.Space(4);

                double currentTime = Planetarium.GetUniversalTime();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Vessels", headerStyle);
                GUILayout.Space(65);
                GUILayout.Label("BioMass", headerStyle);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                // Draw the active vessel first
                Guid activeVesselId = FlightGlobals.ActiveVessel.id;
                if (saveGame.BioVessels.ContainsKey(activeVesselId))
                {
                    GUILayout.BeginHorizontal();
                    DrawVesselInfo(saveGame.BioVessels[activeVesselId], currentTime);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }

                foreach (var entry in saveGame.BioVessels)
                {
                    // The active vessel was already done above
                    if (entry.Key != activeVesselId)
                    {
                        GUILayout.BeginHorizontal();
                        DrawVesselInfo(entry.Value, currentTime);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }

            GUILayout.Space(8);

            //GUI.Label(new Rect(4, windowPos.height - 14, windowPos.width - 20, 12), "TAC Life Support v" + version, labelStyle);
        }

        private void DrawVesselInfo(BioVessel vesselInfo, double currentTime)
        {
            GUIContent contButton;
            contButton = new GUIContent();
            contButton.text = vesselInfo.vesselName;
            if (vesselInfo.vesselName.Length > 15)
                contButton.text = vesselInfo.vesselName.Substring(0, 12) + "...";
            contButton.tooltip = vesselInfo.vesselName + " (Click for Details)";
            

            if (GUILayout.Button(contButton, styleButton))
                PopupDialog.SpawnPopupDialog("Not Implemented", "Someday you will see Details here", "Ok", false, GUI.skin);
            GUILayout.Space(15);
            Rect rectBar;

            float fltBarRemainRatio = (float)vesselInfo.bioMass / (float)vesselInfo.maxBioMass;
            rectBar = DrawBar(1, styleBarGreen_Back, 150);
            if ((rectBar.width * fltBarRemainRatio) > 1)
                DrawBarScaled(rectBar, 1, styleBarGreen, styleBarGreen_Thin, fltBarRemainRatio);
            DrawUsage(rectBar, 1, vesselInfo.bioMass, vesselInfo.maxBioMass);
            GUILayout.BeginVertical();
            GUILayout.Label("  CO2 remaining:        " + FormatTimeRemaining(vesselInfo.estimatedCO2ConsumptionPerDeltaTime,vesselInfo.estimatedCO2Time-currentTime), labelStyle);
            GUILayout.Label("  Water remaining:       " + FormatTimeRemaining(vesselInfo.estimatedWaterConsumptionPerDeltaTime,vesselInfo.estimatedWaterTime-currentTime), labelStyle);
            GUILayout.Label("  Oxygen remaining:      " + FormatTimeRemaining(vesselInfo.estimatedOxygenConsumptionPerDeltaTime,vesselInfo.estimatedOxygenTime-currentTime), labelStyle);
            GUILayout.Label("  WasteWater remaining: " + FormatTimeRemaining(vesselInfo.estimatedWasteWaterConsumptionPerDeltaTime,vesselInfo.estimatedWasteWaterTime-currentTime), labelStyle);
            GUILayout.EndVertical();
        }

        public static string FormatTimeRemaining(double value1,double value2)
        {
            if (value1 <= 0.0)
            {
                return "Infinity";
            }

            double value = value2;

            const double SECONDS_PER_MINUTE = 60.0;
            const double MINUTES_PER_HOUR = 60.0;
            const double HOURS_PER_DAY = 24.0;

            string sign = "";
            if (value < 0.0)
            {
                sign = "-";
                value = -value;
            }

            double seconds = value;

            if (Double.IsInfinity(seconds))
            {
                return "Infinity";
            }

            long minutes = (long)(seconds / SECONDS_PER_MINUTE);
            seconds -= (long)(minutes * SECONDS_PER_MINUTE);

            long hours = (long)(minutes / MINUTES_PER_HOUR);
            minutes -= (long)(hours * MINUTES_PER_HOUR);

            long days = (long)(hours / HOURS_PER_DAY);
            hours -= (long)(days * HOURS_PER_DAY);

            if (days > 0)
            {
                return sign + days.ToString("#0") + "d "
                    + hours.ToString("00") +"h";
            }
            else if (hours > 0)
            {
                return sign + hours.ToString("#0") + "h "
                    + minutes.ToString("00") +"m";
            }
            else
            {
                return sign + minutes.ToString("#0") + "m "
                    + seconds.ToString("00") +"s";
            }
        }

        private Rect DrawBar(int Row, GUIStyle Style, int Width = 0, int Height = 0)
        {
            List<GUILayoutOption> Options = new List<GUILayoutOption>();
            if (Width == 0) Options.Add(GUILayout.ExpandWidth(true));
            else Options.Add(GUILayout.Width(Width));
            if (Height != 0) Options.Add(GUILayout.Height(Height));
            GUILayout.Label("", Style, Options.ToArray());

            return GUILayoutUtility.GetLastRect();
        }

        private void DrawBarScaled(Rect rectStart, int Row, GUIStyle Style, GUIStyle StyleNarrow, float Scale)
        {
            Rect rectTemp = new Rect(rectStart);
            rectTemp.width = (float)Math.Ceiling(rectTemp.width = rectTemp.width * Scale);
            if (rectTemp.width <= 2) Style = StyleNarrow;
            GUI.Label(rectTemp, "", Style);
        }

        private void DrawUsage(Rect rectStart, int Row, double Amount, double MaxAmount)
        {
            Rect rectTemp = new Rect(rectStart);

            rectTemp.y += 2;
            rectTemp.x -= 20;
            rectTemp.width += 40;

            GUI.Label(rectTemp, string.Format("{0} / {1}", DisplayValue(Amount), DisplayValue(MaxAmount)), styleBarText);
        }

        private string DisplayValue(Double Amount)
        {
            String strFormat = "{0:0}";
            if (Amount < 100)
                strFormat = "{0:0.00}";
            return string.Format(strFormat, Amount);
        }

        void LoadTextures()
        {
            LoadImageFromGameDB(ref texBarGreen, "img_BarGreen.png");
            LoadImageFromGameDB(ref texBarGreen_Back, "img_BarGreen_Back.png");
        }

        public Boolean LoadImageFromGameDB(ref Texture2D tex, String FileName, String FolderPath = "")
        {
            //DebugLogFormatted("{0},{1}",FileName, FolderPath);
            Boolean blnReturn = false;

            if (FileName.ToLower().EndsWith(".png")) FileName = FileName.Substring(0, FileName.Length - 4);
            if (FolderPath == "") FolderPath = Settings.PathTextures;

            string imageFilename = String.Format("{0}/{1}", FolderPath, FileName);
            if (GameDatabase.Instance.ExistsTexture(imageFilename))
            {
                this.Log("Load Texture: " + imageFilename);
                tex = GameDatabase.Instance.GetTexture(imageFilename, false);
                blnReturn = true;
            }
            else
            {
                this.Log("Unable to load Texture from GameDB: " + imageFilename);
            }

            return blnReturn;
        }
    }
}
