using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac;

namespace KSPBioMass
{
    public class BioVessel
    {
        public const string ConfigNodeName = "BioVessel";

        public string vesselName;       
        public double lastUpdate;

        public double bioMass;
        public double maxBioMass;

        //Inputs for Biomass
        public double remainingWater;
        public double remainingWasteWater;
        public double remainingOxygen;
        public double remainingCO2;

        //Last 
        public double lastWater;
        public double lastWasteWater;
        public double lastOxygen;
        public double lastCO2;

        //Consumptionrates
        public double estimatedWaterConsumptionPerDeltaTime;
        public double estimatedWasteWaterConsumptionPerDeltaTime;
        public double estimatedOxygenConsumptionPerDeltaTime;
        public double estimatedCO2ConsumptionPerDeltaTime;

        //Time
        public double estimatedWaterTime;
        public double estimatedWasteWaterTime;
        public double estimatedOxygenTime;
        public double estimatedCO2Time;

        public BioVessel(string vesselName, double currentTime)
        {
            this.vesselName = vesselName;
            lastUpdate = currentTime;
        }

        public static BioVessel Load(ConfigNode node)
        {
            string vesselName = Utilities.GetValue(node, "vesselName", "Unknown");
            double lastUpdate = Utilities.GetValue(node, "lastUpdate", 0.0);

            BioVessel info = new BioVessel(vesselName, lastUpdate);
            info.bioMass = Utilities.GetValue(node, "bioMass", 0.0);
            info.maxBioMass = Utilities.GetValue(node, "maxBioMass", 0.0);

            info.remainingWater = Utilities.GetValue(node, "remainingWater", 0.0);
            info.remainingWasteWater = Utilities.GetValue(node, "remainingWasteWater", 0.0);
            info.remainingOxygen = Utilities.GetValue(node, "remainingOxygen", 0.0);
            info.remainingCO2 = Utilities.GetValue(node, "remainingCO2", 0.0);

            info.lastWater = Utilities.GetValue(node, "lastWater", 0.0);
            info.lastWasteWater = Utilities.GetValue(node, "lastWasteWater", 0.0);
            info.lastOxygen = Utilities.GetValue(node, "lastOxygen", 0.0);
            info.lastCO2 = Utilities.GetValue(node, "lastCO2", 0.0);

            info.estimatedWaterConsumptionPerDeltaTime = Utilities.GetValue(node, "estimatedWaterConsumptionPerDeltaTime", 0.0);
            info.estimatedWasteWaterConsumptionPerDeltaTime = Utilities.GetValue(node, "estimatedWasteWaterConsumptionPerDeltaTime", 0.0);
            info.estimatedOxygenConsumptionPerDeltaTime = Utilities.GetValue(node, "estimatedOxygenConsumptionPerDeltaTime", 0.0);
            info.estimatedCO2ConsumptionPerDeltaTime = Utilities.GetValue(node, "estimatedCO2ConsumptionPerDeltaTime", 0.0);

            info.estimatedWaterTime = Utilities.GetValue(node, "estimatedWaterTime", 0.0);
            info.estimatedWasteWaterTime = Utilities.GetValue(node, "estimatedWasteWaterTime", 0.0);
            info.estimatedOxygenTime = Utilities.GetValue(node, "estimatedOxygenTime", 0.0);
            info.estimatedCO2Time = Utilities.GetValue(node, "estimatedCO2Time", 0.0);

            return info;
        }

        public ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselName", vesselName);
            node.AddValue("lastUpdate", lastUpdate);

            node.AddValue("bioMass", bioMass);
            node.AddValue("maxBioMass", maxBioMass);

            node.AddValue("remainingWater", remainingWater);
            node.AddValue("remainingWasteWater", remainingWasteWater);
            node.AddValue("remainingOxygen", remainingOxygen);
            node.AddValue("remainingCO2", remainingCO2);

            node.AddValue("lastWater", lastWater);
            node.AddValue("lastWasteWater", lastWasteWater);
            node.AddValue("lastOxygen", lastOxygen);
            node.AddValue("lastCO2", lastCO2);

            node.AddValue("estimatedWaterConsumptionPerDeltaTime", estimatedWaterConsumptionPerDeltaTime);
            node.AddValue("estimatedWasteWaterConsumptionPerDeltaTime", estimatedWasteWaterConsumptionPerDeltaTime);
            node.AddValue("estimatedOxygenConsumptionPerDeltaTime", estimatedOxygenConsumptionPerDeltaTime);
            node.AddValue("estimatedCO2ConsumptionPerDeltaTime", estimatedCO2ConsumptionPerDeltaTime);

            node.AddValue("estimatedWaterTime", estimatedWaterTime);
            node.AddValue("estimatedWasteWaterTime", estimatedWasteWaterTime);
            node.AddValue("estimatedOxygenTime", estimatedOxygenTime);
            node.AddValue("estimatedCO2Time", estimatedCO2Time);
            return node;
        }

        public void ClearAmounts()
        {
            bioMass = 0.0;
            maxBioMass = 0.0;
            remainingWater = 0.0;
            remainingWasteWater = 0.0;
            remainingOxygen = 0.0;
            remainingCO2 = 0.0;
        }
    }
}
