using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac;
using UnityEngine;

namespace KSPBioMass
{
    public class SpaceCenterController : MonoBehaviour, ISavable
    {
        private Settings globalSettings;
        private SaveGame saveGame;
        private ButtonWrapper button;
        private SavedGameConfigWindow configWindow;

        public SpaceCenterController()
        {
            this.Log_DebugOnly("Constructor SCC");
            globalSettings = BioMass.Instance.globalSettings;
            saveGame = BioMass.Instance.saveGame;
            button = new ButtonWrapper(new Rect(Screen.width * 0.75f, 0, 32, 32), Settings.PathTextures+"/HerbIcon",
                "BM", "BioMass Configuration Window", OnIconClicked, "HerbIcon");
            configWindow = new SavedGameConfigWindow(globalSettings, saveGame);
            this.Log(Settings.PathTextures + "/HerbIcon");
        }

        public void Load(ConfigNode globalNode)
        {
            button.Load(globalNode);
            configWindow.Load(globalNode);
        }

        public void Save(ConfigNode globalNode)
        {
            button.Save(globalNode);
            configWindow.Save(globalNode);
        }

        void OnDestroy()
        {
            this.Log_DebugOnly("Destroy SCC");
            button.Destroy();
        }

        private void OnIconClicked()
        {
            configWindow.ToggleVisible();
        }
    }
}
