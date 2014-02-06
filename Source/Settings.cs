using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using UnityEngine;
using Tac;
using KSP.IO;

namespace KSPBioMass
{
    public class Settings
    {
        //Static
        public static String AddonName = "BioMass+Science";
        public static String PathKSP;        
        public static String PathPlugin;
        public static String PathPluginData;
        public static String PathTextures;
        public static String GlobalConfigFile;
        
        public ConfigNode globalSettingsNode = new ConfigNode();
        public List<Component> controller = new List<Component>();


        private const string configNodeName = "GlobalSettings";
        private const int SECONDS_PER_MINUTE = 60;
        private const int SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE;
        private const int SECONDS_PER_DAY = 24 * SECONDS_PER_HOUR;

        public int MaxDeltaTime { get; set; }
        public int ElectricityMaxDeltaTime { get; set; }

        public string Food { get; private set; }
        public string Water { get; private set; }
        public string Oxygen { get; private set; }
        public string Electricity { get { return "ElectricCharge"; } }
        public string CO2 { get; private set; }
        public string Waste { get; private set; }
        public string WasteWater { get; private set; }

        public string Seeds { get; private set; }
        public string BioMass { get; private set; }
        public string Light { get; private set; }
        public string BioCake { get; private set; }
        public string CompressedCO2 { get; private set; }
        public string Hydrogen { get; private set; }

        public double WasteWaterConsumptionRate { get; set; }
        public double WaterConsumptionRate { get; set; }
        public double OxygenConsumptionRate { get; set; }
        public double CO2ConsumptionRate { get; set; }

        public double CO2ProductionRate { get; set; }
        public double OxygenProductionRate { get; set; }
        public double WaterProductionRate { get; set; }

        public DifficultySetting DifficultyBioMass;

        public int FoodId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Food).id;
            }
        }
        public int WaterId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Water).id;
            }
        }
        public int OxygenId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Oxygen).id;
            }
        }
        public int ElectricityId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Electricity).id;
            }
        }
        public int CO2Id
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(CO2).id;
            }
        }
        public int WasteId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Waste).id;
            }
        }
        public int WasteWaterId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(WasteWater).id;
            }
        }

        public int SeedsId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Seeds).id;
            }
        }
        public int BioMassId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(BioMass).id;
            }
        }
        public int LightId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Light).id;
            }
        }
        public int BioCakeId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(BioCake).id;
            }
        }
        public int CompressedCO2Id
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(CompressedCO2).id;
            }
        }
        public int HydrogenId
        {
            get
            {
                return PartResourceLibrary.Instance.GetDefinition(Hydrogen).id;
            }
        }

        private string GetKSPPath(string strSource)
        {
            int Start, End;
            if (strSource.Contains(AddonName))
            {
                Start = 0;
                End = strSource.IndexOf(AddonName, 0) - 1;
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public Settings()
        {
            this.Log_DebugOnly("Constructor Settings");
            
            //string filepath = IOUtils.GetFilePathFor(this.GetType(), "XYZ");
            //PathKSP = GetKSPPath(filepath);
            PathPlugin = string.Format("{0}",AddonName);
            PathPluginData = string.Format("{0}/PluginData", PathPlugin);
            PathTextures = string.Format("{0}/Textures", PathPlugin);
            GlobalConfigFile = IOUtils.GetFilePathFor(this.GetType(), "BioMass.cfg");

            this.Log_DebugOnly("ConfigFile: " + GlobalConfigFile);
            
            this.globalSettingsNode = new ConfigNode();

            MaxDeltaTime = SECONDS_PER_DAY; // max 1 day (24 hour) per physics update, or 50 days (4,320,000 seconds) per second
            ElectricityMaxDeltaTime = 1; // max 1 second per physics update

            Food = "Food";
            Water = "Water";
            Oxygen = "Oxygen";
            CO2 = "CarbonDioxide";
            Waste = "Waste";
            WasteWater = "WasteWater";
            Seeds = "Seeds";
            BioMass = "BioMass";
            Light = "Light";
            BioCake = "BioCake";
            CompressedCO2 = "CompressedCO2";
            Hydrogen = "Hydrogen";

            WasteWaterConsumptionRate = 0.00000826f;
            WaterConsumptionRate = 0.00000826f;
            OxygenConsumptionRate = 0.00000445;
            CO2ConsumptionRate = 0.00002018f;

            CO2ProductionRate = 0.00000612f;
            OxygenProductionRate = 0.00001468f;
            WaterProductionRate = 0.00000250f;
            DifficultyBioMass = DifficultySetting.Normal;
        }
        
        public Boolean FileExists(string FileName)
        {
            return (System.IO.File.Exists(FileName));
        }

        public Boolean Load()
        {
            return this.Load(GlobalConfigFile);
        }       

        public Boolean Load(String fileFullName)
        {
            Boolean blnReturn = false;
            try
            {
                if (FileExists(fileFullName))
                {
                    //Load the file into a config node
                    globalSettingsNode = ConfigNode.Load(fileFullName);
                    this.LoadFromNode(globalSettingsNode);
                    foreach (ISavable s in controller.Where(c => c is ISavable))
                    {
                        s.Load(globalSettingsNode);
                    }
                    this.LogWarning("Load GlobalSettings");
                    blnReturn = true;
                }
                else
                {
                    this.Log_DebugOnly(String.Format("File could not be found to load({0})", fileFullName));
                    blnReturn = false;
                }
            }
            catch (Exception ex)
            {
                this.LogError(String.Format("Failed to Load Configfile({0})-Error:{1}", fileFullName, ex.Message));
                blnReturn = false;
            }
            return blnReturn;
        }

        private void LoadFromNode(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);

                MaxDeltaTime = Utilities.GetValue(settingsNode, "MaxDeltaTime", MaxDeltaTime);
                ElectricityMaxDeltaTime = Utilities.GetValue(settingsNode, "ElectricityMaxDeltaTime", ElectricityMaxDeltaTime);

                Food = Utilities.GetValue(settingsNode, "FoodResource", Food);
                Water = Utilities.GetValue(settingsNode, "WaterRes4600urce", Water);
                Oxygen = Utilities.GetValue(settingsNode, "OxygenResource", Oxygen);
                CO2 = Utilities.GetValue(settingsNode, "CarbonDioxideResource", CO2);
                Waste = Utilities.GetValue(settingsNode, "WasteResource", Waste);
                WasteWater = Utilities.GetValue(settingsNode, "WasteWaterResource", WasteWater);

                Seeds = Utilities.GetValue(settingsNode, "SeedsResource", Seeds);
                BioMass = Utilities.GetValue(settingsNode, "BioMassResource", BioMass);
                Light = Utilities.GetValue(settingsNode, "LightResource", Light);
                BioCake = Utilities.GetValue(settingsNode, "BioCakeResource", BioCake);
                CompressedCO2 = Utilities.GetValue(settingsNode, "CompressedCO2Resource", CompressedCO2);
                Hydrogen = Utilities.GetValue(settingsNode, "HydrogenResource", Hydrogen);

                WasteWaterConsumptionRate = Utilities.GetValue(settingsNode, "WasteWaterConsumptionRate", WasteWaterConsumptionRate) / MaxDeltaTime;
                WaterConsumptionRate = Utilities.GetValue(settingsNode, "WaterConsumptionRate", WaterConsumptionRate) / MaxDeltaTime;
                OxygenConsumptionRate = Utilities.GetValue(settingsNode, "OxygenConsumptionRate", OxygenConsumptionRate) / MaxDeltaTime;
                CO2ConsumptionRate = Utilities.GetValue(settingsNode, "CO2ConsumptionRate", CO2ConsumptionRate) / MaxDeltaTime;

                CO2ProductionRate = Utilities.GetValue(settingsNode, "CO2ProductionRate", CO2ProductionRate) / MaxDeltaTime;
                OxygenProductionRate = Utilities.GetValue(settingsNode, "OxygenProductionRate", OxygenProductionRate) / MaxDeltaTime;
                WaterProductionRate = Utilities.GetValue(settingsNode, "WaterProductionRate", WaterProductionRate) / MaxDeltaTime;
                DifficultyBioMass = Utilities.GetValue(settingsNode, "DifficultySettings",DifficultyBioMass);
                this.LogWarning("Difficulty read "+DifficultyBioMass.ToString());
            }
        }

        public Boolean Save()
        {
            return this.Save(GlobalConfigFile);
        }

        public Boolean Save(String fileFullName)
        {
            Boolean blnReturn = false;
            try
            {
                //Encode the current object
                SaveToNode(globalSettingsNode);
                foreach (ISavable s in controller.Where(c => c is ISavable))
                {
                    s.Save(globalSettingsNode);
                }
                globalSettingsNode.Save(GlobalConfigFile);
                this.LogWarning("Save GlobalSettings");
                blnReturn = true;
            }
            catch (Exception ex)
            {
                this.LogError(String.Format("Failed to Save ConfigNode to file({0})-Error:{1}", fileFullName, ex.Message));
                blnReturn = false;
            }
            return blnReturn;
        }

        private void SaveToNode(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
                settingsNode.ClearData();
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }

            settingsNode.AddValue("MaxDeltaTime", MaxDeltaTime);
            settingsNode.AddValue("ElectricityMaxDeltaTime", ElectricityMaxDeltaTime);

            settingsNode.AddValue("FoodResource", Food);
            settingsNode.AddValue("WaterResource", Water);
            settingsNode.AddValue("OxygenResource", Oxygen);
            settingsNode.AddValue("CarbonDioxideResource", CO2);
            settingsNode.AddValue("WasteResource", Waste);
            settingsNode.AddValue("WasteWaterResource", WasteWater);

            settingsNode.AddValue("SeedsResource", Seeds);
            settingsNode.AddValue("BioMassResource", BioMass);
            settingsNode.AddValue("LightResource", Light);
            settingsNode.AddValue("BioCakeResource", BioCake);
            settingsNode.AddValue("CompressedCO2Resource", CompressedCO2);
            settingsNode.AddValue("HydrogenResource", Hydrogen);

            settingsNode.AddValue("WasteWaterConsumptionRate", WasteWaterConsumptionRate * MaxDeltaTime);
            settingsNode.AddValue("WaterConsumptionRate", WaterConsumptionRate * MaxDeltaTime);
            settingsNode.AddValue("OxygenConsumptionRate", OxygenConsumptionRate * MaxDeltaTime);
            settingsNode.AddValue("CO2ConsumptionRate", CO2ConsumptionRate * MaxDeltaTime);

            settingsNode.AddValue("CO2ProductionRate", CO2ProductionRate * MaxDeltaTime);
            settingsNode.AddValue("OxygenProductionRate", OxygenProductionRate * MaxDeltaTime);
            settingsNode.AddValue("WaterProductionRate", WaterProductionRate * MaxDeltaTime);

            settingsNode.AddValue("DifficultySettings", DifficultyBioMass);
        }

        public void setDifficultySettings(DifficultySetting difficulty)
        {
            
            if (difficulty == DifficultyBioMass)
            {
                return;
            }

            DifficultyBioMass = difficulty;
            switch (difficulty)
            {
                case DifficultySetting.Easy:
                    WasteWaterConsumptionRate = 0.00000826f;
                    WaterConsumptionRate = 0.00000826f;
                    OxygenConsumptionRate = 0.00000445;
                    CO2ConsumptionRate = 0.00002018f;

                    CO2ProductionRate = 0.00001612f;
                    OxygenProductionRate = 0.00002468f;
                    WaterProductionRate = 0.00001250f;
                    break;
                case DifficultySetting.Normal:
                    WasteWaterConsumptionRate = 0.00000826f;
                    WaterConsumptionRate = 0.00000826f;
                    OxygenConsumptionRate = 0.00000445;
                    CO2ConsumptionRate = 0.00002018f;

                    CO2ProductionRate = 0.00000612f;
                    OxygenProductionRate = 0.00001468f;
                    WaterProductionRate = 0.00000250f;
                    break;
                case DifficultySetting.Hard:
                    WasteWaterConsumptionRate = 0.00000526f;
                    WaterConsumptionRate = 0.00001326f;
                    OxygenConsumptionRate = 0.00000945;
                    CO2ConsumptionRate = 0.00004018f;

                    CO2ProductionRate = 0.00000912f;
                    OxygenProductionRate = 0.00000868f;
                    WaterProductionRate = 0.00000150f;
                    break;
                case DifficultySetting.Custom:
                    WasteWaterConsumptionRate = 0.00000826f;
                    WaterConsumptionRate = 0.00000826f;
                    OxygenConsumptionRate = 0.00000445;
                    CO2ConsumptionRate = 0.00002018f;

                    CO2ProductionRate = 0.00000612f;
                    OxygenProductionRate = 0.00001468f;
                    WaterProductionRate = 0.00000250f;
                    break;
                default:
                    break;
            }
        }
    }
}
