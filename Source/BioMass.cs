using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;
using KSP.IO;

namespace KSPBioMass
{
    /*
     * Load Persistent Game
     * 
     */

#if DEBUG
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Debug_AutoLoadPersistentSaveOnStartup : MonoBehaviour
    {
        public static bool first = true;
        public void Start()
        {
            if (first)
            {
                first = false;
                HighLogic.SaveFolder = "default";
                var game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);
                if (game != null && game.flightState != null && game.compatible)
                {
                    FlightDriver.StartAndFocusVessel(game, 6);
                }
                //CheatOptions.InfiniteFuel = true;
            }
        }
    }
#endif


    /*
     * HookMethod to get BioMass Scenario load at Spacecenter
     * Credits to TAC Life Support
    */
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AddBioMassToScenarioModules : MonoBehaviour
    {
        void Start()
        {
            var game = HighLogic.CurrentGame;

            ProtoScenarioModule psm = game.scenarios.Find(s => s.moduleName == typeof(BioMass).Name);
            if (psm == null)
            {
                this.Log_DebugOnly("Adding BioMass to Scenarios");
                psm = game.AddProtoScenarioModule(typeof(BioMass), GameScenes.SPACECENTER,GameScenes.FLIGHT);
            }
            else
            {
                if (!psm.targetScenes.Any(s => s == GameScenes.SPACECENTER))
                {
                    psm.targetScenes.Add(GameScenes.SPACECENTER);
                }
                if (!psm.targetScenes.Any(s => s == GameScenes.FLIGHT))
                {
                    psm.targetScenes.Add(GameScenes.FLIGHT);
                } 
            }
        }
    }

    public class BioMass : ScenarioModule
    {
        public static BioMass Instance { get; private set; }
        public Settings globalSettings { get; private set; }
        public SaveGame saveGame { get; private set; }
        private readonly List<Component> controller = new List<Component>();

        public BioMass()
        {
            this.Log_DebugOnly("Constructor BioMass");
            Instance = this;
            globalSettings = new Settings();
            saveGame = new SaveGame();
        }

        public override void OnAwake()
        {

            this.Log_DebugOnly("Wakeup in " + HighLogic.LoadedScene);
            base.OnAwake();

            globalSettings.Load();

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                this.Log_DebugOnly("Adding SpaceCenter Controller");
                var c = gameObject.AddComponent<SpaceCenterController>();
                controller.Add(c);
                globalSettings.controller.Add(c);
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                this.Log_DebugOnly("Adding Flight Controller");
                var c = gameObject.AddComponent<FlightController>();
                controller.Add(c);
                globalSettings.controller.Add(c);
            }
 
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            saveGame.Load(gameNode);
            globalSettings.Load();
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            saveGame.Save(gameNode);

            globalSettings.Save();
        }

        void OnDestroy()
        {
            this.Log("OnDestroy");
            foreach (Component c in controller)
            {
                Destroy(c);
            }
            controller.Clear();
        }
    }
}
