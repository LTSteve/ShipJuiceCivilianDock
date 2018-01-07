using KSPDev.ConfigUtils;
using KSPDev.LogUtils;
using Smooth.Algebraics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShipJuice
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class ShipJuiceAddonConfig : MonoBehaviour
    {
        private ShipJuiceParams settings;

        private static bool once = true;

        public void Awake()
        {
            settings = HighLogic.CurrentGame.Parameters.CustomParams<ShipJuiceParams>();

            ShipJuiceManager.shipJuiceTickTime = settings != null ? settings.ShipJuiceTickTime : 7f;
            ShipJuiceManager.kerbinResourceCostMultiplier = settings != null ? settings.KerbinResourceCostMultiplier : 10f;
            ShipJuiceManager.amountShipped = settings != null ? settings.AmountShipped : 1f;

            if (once)
            {
                GameEvents.onGameStateCreated.Add(GameSaveLoaded);
                GameEvents.onGameStateSave.Add(Save);
                GameEvents.onGameStateLoad.Add(Load);
                once = false;
            }

            if (ShipJuiceManager.Instance != null)
            {
                return;
            }

            ShipJuiceManager.Init();
        }

        public void Update()
        {
            if(ShipJuiceManager.Instance == null)
            {
                return;
            }

            ShipJuiceManager.Instance.Update();
        }

        private void Save(ConfigNode data)
        {
            if (ShipJuiceManager.Instance == null)
            {
                return;
            }

            var shipJuice = data.HasNode("ShipJuice") ? 
                data.GetNode("ShipJuice") : new ConfigNode("ShipJuice");

            data.SetNode("ShipJuice", shipJuice, true);

            shipJuice = data.GetNode("ShipJuice");

            var dockManager = shipJuice.HasNode("DockManager") ?
                shipJuice.GetNode("DockManager") : new ConfigNode("DockManager");

            shipJuice.SetNode("DockManager", dockManager, true);

            dockManager = shipJuice.GetNode("DockManager");
            
            ShipJuiceManager.Instance.Save(dockManager);
        }

        private void Load(ConfigNode data)
        {
            var shipJuice = data.HasNode("ShipJuice") ? 
                data.GetNode("ShipJuice") : new ConfigNode("ShipJuice");

            var dockManager = shipJuice.HasNode("DockManager") ?
                shipJuice.GetNode("DockManager") : new ConfigNode("DockManager");

            ShipJuiceManager.Instance.Load(dockManager);
        }

        private void GameSaveLoaded(Game game)
        {
            Load(game.config);
        }
    }
}
