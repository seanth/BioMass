using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac;
using UnityEngine;

namespace KSPBioMass
{
    public class FlightController : MonoBehaviour, ISavable
    {
        private Settings globalSettings;
        private SaveGame saveGame;
        private BioVesselMonitoringWindow monitoringWindow;
        //private RosterWindow rosterWindow;
        private ButtonWrapper button;
        private bool noUpdate = false;

        void Awake()
        {
            this.Log("Awake");
            globalSettings = BioMass.Instance.globalSettings;
            saveGame = BioMass.Instance.saveGame;
            monitoringWindow = new BioVesselMonitoringWindow(this, globalSettings, saveGame);

            button = new ButtonWrapper(new Rect(Screen.width * 0.75f, 0, 32, 32), Settings.PathTextures + "/HerbIcon",
                "BM", "BioMass Monitoring", OnIconClicked, "HerbIcon");
        }

        void Start()
        {
            this.Log("Start");
            button.Visible = true;
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
        }

        private void OnIconClicked()
        {
            monitoringWindow.ToggleVisible();
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            button.Destroy();
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        }

        private void OnGameSceneLoadRequested(GameScenes gameScene)
        {
            this.Log("Game scene load requested: " + gameScene);

            // Disable this instance becuase a new instance will be created after the new scene is loaded
            noUpdate = true;
        }

        void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready || noUpdate)
            {
                return;
            }

            double currentTime = Planetarium.GetUniversalTime();
            var allVessels = FlightGlobals.Vessels;
            var knownVessels = saveGame.BioVessels;

            var vesselsToDelete = new List<Guid>();
            foreach (var entry in knownVessels)
            {
                Guid vesselId = entry.Key;
                BioVessel bioVessel = entry.Value;
                Vessel vessel = allVessels.Find(v => v.id == vesselId);

                if (vessel == null)
                {
                    this.Log("Deleting vessel " + bioVessel.vesselName + " - vessel does not exist anymore");
                    vesselsToDelete.Add(vesselId);
                    continue;
                }

                if (vessel.loaded)
                {
                    double bioMass = UpdateVesselInfo(bioVessel, vessel,currentTime);

                    if (bioMass == 0.0)
                    {
                        this.Log("Deleting vessel " + bioVessel.vesselName + " - no BioMass-Modules onboard");
                        vesselsToDelete.Add(vesselId);
                        continue;
                    }
                }


                if (vessel.loaded)
                {
                    //TODO: Deplenish Ressources
                    ConsumeResources(currentTime, vessel, bioVessel);
                }
            }

            vesselsToDelete.ForEach(id => knownVessels.Remove(id));

            foreach (Vessel vessel in allVessels.Where(v => v.loaded))
            {
                if (!knownVessels.ContainsKey(vessel.id) && IsLaunched(vessel))
                {
                    BioVessel vesselInfo = new BioVessel(vessel.vesselName, currentTime);
                    double bioMass = UpdateVesselInfo(vesselInfo, vessel,currentTime);
                    if (bioMass != 0.0)
                    {
                        this.Log("New vessel: " + vessel.vesselName + " (" + vessel.id + ")");
                        knownVessels[vessel.id] = vesselInfo;
                    }
                }
            }

        }

        private double UpdateVesselInfo(BioVessel bioVessel, Vessel vessel, double currentTime)
        {
            bioVessel.lastWater = bioVessel.remainingWater;
            bioVessel.lastWasteWater = bioVessel.remainingWasteWater;
            bioVessel.lastOxygen = bioVessel.remainingOxygen;
            bioVessel.lastCO2 = bioVessel.remainingCO2;

            bioVessel.ClearAmounts();

            foreach (Part part in vessel.parts)
            {
                foreach (PartResource resource in part.Resources)
                {
                    if (resource.flowState)
                    {
                        if (resource.info.id == globalSettings.BioMassId)
                        {
                            bioVessel.bioMass += resource.amount;
                            bioVessel.maxBioMass += resource.maxAmount;
                        }
                        if (resource.info.id == globalSettings.WaterId)
                        {
                            bioVessel.remainingWater += resource.amount;
                        }
                        if (resource.info.id == globalSettings.WasteWaterId)
                        {
                            bioVessel.remainingWasteWater += resource.amount;
                        }
                        if (resource.info.id == globalSettings.OxygenId)
                        {
                            bioVessel.remainingOxygen += resource.amount;
                        }
                        if (resource.info.id == globalSettings.CO2Id)
                        {
                            bioVessel.remainingCO2 += resource.amount;
                        }
                    }
                }
            }

            //Calculate Estimated Consumption Rates
            double currentdeltaTime = currentTime - bioVessel.lastUpdate;
            bioVessel.estimatedWaterConsumptionPerDeltaTime = bioVessel.lastWater - bioVessel.remainingWater;
            bioVessel.estimatedWasteWaterConsumptionPerDeltaTime = bioVessel.lastWasteWater - bioVessel.remainingWasteWater;
            bioVessel.estimatedOxygenConsumptionPerDeltaTime = bioVessel.lastOxygen - bioVessel.remainingOxygen;
            bioVessel.estimatedCO2ConsumptionPerDeltaTime = bioVessel.lastCO2 - bioVessel.remainingCO2;

            bioVessel.estimatedWaterConsumptionPerDeltaTime =
                (bioVessel.estimatedWaterConsumptionPerDeltaTime / currentdeltaTime);
            bioVessel.estimatedWasteWaterConsumptionPerDeltaTime =
                (bioVessel.estimatedWasteWaterConsumptionPerDeltaTime / currentdeltaTime);
            bioVessel.estimatedOxygenConsumptionPerDeltaTime =
                (bioVessel.estimatedOxygenConsumptionPerDeltaTime / currentdeltaTime);
            bioVessel.estimatedCO2ConsumptionPerDeltaTime =
                (bioVessel.estimatedCO2ConsumptionPerDeltaTime / currentdeltaTime);

		    bioVessel.estimatedWaterTime = bioVessel.lastUpdate + (bioVessel.remainingWater / Math.Abs(bioVessel.estimatedWaterConsumptionPerDeltaTime));
            bioVessel.estimatedOxygenTime = bioVessel.lastUpdate + (bioVessel.remainingOxygen / Math.Abs(bioVessel.estimatedOxygenConsumptionPerDeltaTime));
            bioVessel.estimatedCO2Time = bioVessel.lastUpdate + (bioVessel.remainingCO2 / Math.Abs(bioVessel.estimatedCO2ConsumptionPerDeltaTime));
            bioVessel.estimatedWasteWaterTime = bioVessel.lastUpdate + (bioVessel.remainingWasteWater / Math.Abs(bioVessel.estimatedWasteWaterConsumptionPerDeltaTime));

            return bioVessel.maxBioMass;
        }

        private bool IsLaunched(Vessel vessel)
        {
            return vessel.missionTime > 0.01 || (Time.timeSinceLevelLoad > 5.0f && vessel.srf_velocity.magnitude > 2.0);
        }

        private void ConsumeResources(double currentTime, Vessel vessel, BioVessel bioVessel)
        {
            bioVessel.lastUpdate = currentTime;
            bioVessel.vesselName = vessel.vesselName;
        }

        public void Load(ConfigNode globalNode)
        {
            button.Load(globalNode);
            monitoringWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            button.Save(globalNode);
            monitoringWindow.Save(globalNode);
        }
    }
}
