/**
 * Thunder Aerospace Corporation's Life Support for Kerbal Space Program.
 * Written by Taranis Elsu.
 * 
 * (C) Copyright 2013, Taranis Elsu
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Thunder Aerospace Corporation is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Tac;

namespace KSPBioMass
{
    class SavedGameConfigWindow : Window<SavedGameConfigWindow>
    {
        private Settings globalSettings;
        private SaveGame saveGame;
        private GUIStyle labelStyle;
        private GUIStyle editStyle;
        private GUIStyle headerStyle;
        private GUIStyle headerStyle2;
        private GUIStyle warningStyle;
        private GUIStyle buttonStyle;


        private bool showBioMassConsumption = false;


        private DifficultySetting BioMassDifficulty;
        private bool EasySettings = false;
        private bool NormalSettings = false;
        private bool HardSettings = false;
        private bool CustomSettings = false;

 
        private readonly string version;

        public SavedGameConfigWindow(Settings globalSettings, SaveGame saveGame)
            : base("BioMass Settings", 400, 300)
        {
            base.Resizable = false;
            this.globalSettings = globalSettings;
            this.saveGame = saveGame;
            this.changeSettings(globalSettings.DifficultyBioMass, true);
            version = Utilities.GetDllVersion(this);
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.wordWrap = false;

                editStyle = new GUIStyle(GUI.skin.textField);
                editStyle.alignment = TextAnchor.MiddleRight;

                headerStyle = new GUIStyle(labelStyle);
                headerStyle.fontStyle = FontStyle.Bold;

                headerStyle2 = new GUIStyle(headerStyle);
                headerStyle2.wordWrap = true;

                buttonStyle = new GUIStyle(GUI.skin.button);

                warningStyle = new GUIStyle(headerStyle2);
                warningStyle.normal.textColor = new Color(0.88f, 0.20f, 0.20f, 1.0f);
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            GUILayout.Label("Version: " + version, labelStyle);
            GUILayout.Space(3);
            DifficultySettings();
            GUILayout.Space(10);
            BioMassConsumptionRates();

            if (GUI.changed)
            {
                SetSize(150, 20);
            }
        }

        private void DifficultySettings()
        {
            GUILayout.Label("Difficulty Settings", headerStyle);
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(EasySettings, "Easy", buttonStyle))
            {
                changeSettings(DifficultySetting.Easy,false);
            }
            if (GUILayout.Toggle(NormalSettings, "Normal", buttonStyle))
            {
                changeSettings(DifficultySetting.Normal,false);
            }
            if (GUILayout.Toggle(HardSettings, "Hard", buttonStyle))
            {
                changeSettings(DifficultySetting.Hard,false);
            }
            if (GUILayout.Toggle(CustomSettings, "Custom", buttonStyle))
            {
                changeSettings(DifficultySetting.Custom,false);
            }
            GUILayout.EndHorizontal();
        }

        private void BioMassConsumptionRates()
        {
            showBioMassConsumption = GUILayout.Toggle(showBioMassConsumption, "Consumption/Production Rates", buttonStyle);

            if (showBioMassConsumption)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("These settings affect all saves. Restart KSP for changes to take effect.", warningStyle);
                GUILayout.Label("The following values are in units per day (24 hours).", headerStyle);

                GUILayout.Label("Consumption", headerStyle2);
                globalSettings.WaterConsumptionRate = Utilities.ShowTextField("Water Consumption Rate", labelStyle,
                    globalSettings.WaterConsumptionRate * globalSettings.MaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150)) / globalSettings.MaxDeltaTime;
                globalSettings.WasteWaterConsumptionRate = Utilities.ShowTextField("Waste Water Consumption Rate", labelStyle,
                    globalSettings.WasteWaterConsumptionRate * globalSettings.MaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150)) / globalSettings.MaxDeltaTime;
                globalSettings.OxygenConsumptionRate = Utilities.ShowTextField("Oxygen Consumption Rate", labelStyle,
                    globalSettings.OxygenConsumptionRate * globalSettings.MaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150)) / globalSettings.MaxDeltaTime;
                globalSettings.CO2ConsumptionRate = Utilities.ShowTextField("CarbonDioxide Consumption Rate", labelStyle,
                    globalSettings.CO2ConsumptionRate * globalSettings.MaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150)) / globalSettings.MaxDeltaTime;

                GUILayout.Space(15);
                GUILayout.Label("Production", headerStyle2);
                globalSettings.WaterProductionRate = Utilities.ShowTextField("Water Production Rate", labelStyle,
                    globalSettings.WaterProductionRate * globalSettings.MaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150)) / globalSettings.MaxDeltaTime;
                globalSettings.CO2ProductionRate = Utilities.ShowTextField("CarbonDioxide Production Rate", labelStyle,
                    globalSettings.CO2ProductionRate * globalSettings.MaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150)) / globalSettings.MaxDeltaTime;               
                globalSettings.OxygenProductionRate = Utilities.ShowTextField("Oxygen Production Rate", labelStyle,
                    globalSettings.OxygenProductionRate * globalSettings.MaxDeltaTime, 30, editStyle, GUILayout.MinWidth(150)) / globalSettings.MaxDeltaTime;

                GUILayout.Space(15);
                GUILayout.Label("Misc", headerStyle);
                globalSettings.MaxDeltaTime = (int)Utilities.ShowTextField("Max Delta Time", labelStyle, globalSettings.MaxDeltaTime,
                    30, editStyle, GUILayout.MinWidth(150));

                GUILayout.EndVertical();
            }
        }


        private void changeSettings(DifficultySetting diff,bool initialSet)
        {
            if (diff == BioMassDifficulty)
            {
                return;
            }

            this.Log_DebugOnly("Change Difficulty FROM " + BioMassDifficulty.ToString() + " TO "+diff);
            EasySettings = false;
            NormalSettings = false;
            HardSettings = false;
            CustomSettings = false;

            BioMassDifficulty = diff;

            switch (BioMassDifficulty)
            {
                case DifficultySetting.Easy:
                    EasySettings = true;
                    break;
                case DifficultySetting.Normal:
                    NormalSettings = true;
                    break;
                case DifficultySetting.Hard:
                    HardSettings = true;
                    break;
                case DifficultySetting.Custom:
                    CustomSettings = true;
                    break;
                default:
                    break;
            }

            if (!initialSet)
              globalSettings.setDifficultySettings(BioMassDifficulty);

        }
    }

    public enum DifficultySetting
    {
        Easy,
        Normal,
        Hard,
        Custom
    }
}
