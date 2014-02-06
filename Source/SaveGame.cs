using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPBioMass
{
    public class SaveGame
    {
        private const string configNodeName = "SavedGame";
        public Dictionary<Guid, BioVessel> BioVessels { get; private set; }

        public SaveGame()
        {
            BioVessels = new Dictionary<Guid, BioVessel>();
        }

        public void Load(ConfigNode node)
        {
            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);

                BioVessels.Clear();
                var vesselNodes = settingsNode.GetNodes(BioVessel.ConfigNodeName);
                foreach (ConfigNode vesselNode in vesselNodes)
                {
                    if (vesselNode.HasValue("Guid"))
                    {
                        Guid id = new Guid(vesselNode.GetValue("Guid"));
                        BioVessel bioVessel = BioVessel.Load(vesselNode);
                        BioVessels[id] = bioVessel;
                    }
                }
            }
        }

        public void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }

            foreach (var bioVessel in BioVessels)
            {
                ConfigNode vesselNode = bioVessel.Value.Save(settingsNode);
                vesselNode.AddValue("Guid", bioVessel.Key);
            }

        }
    }
}
