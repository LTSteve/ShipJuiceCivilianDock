using Smooth.Algebraics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipJuice
{
    public class ModuleImportDock : PartModule
    {
        [KSPField(isPersistant = true)]
        private bool activated;

        [KSPField(isPersistant = true)]
        private double lastTick;

        [KSPField(isPersistant = false, advancedTweakable = false, guiActive = true, guiActiveEditor = false, guiName = "State")]
        private string _inStableOrbit = "Stabilize Orbit";
        
        private string inStableOrbit
        {
            get
            {
                var orbit = vessel.GetCurrentOrbit();

                if (orbit.referenceBody.bodyName == "Kerbin" &&
                    orbit.PeA > 70000 && orbit.ApA < 200000)
                {
                    this._inStableOrbit = "Ready for Trade";
                }
                else
                {
                    this._inStableOrbit = "Orbit of 70k-200k required";
                }

                return this._inStableOrbit;
            }
        }
        
        public override void OnLoad(ConfigNode node)
        {
            Events["ActivateDock"].guiName = activated ? "Deactivate Civilian Dock" : "Activate Civilian Dock";
            
            FlipMenus(false);

            base.OnLoad(node);
        }

        public override void OnUpdate()
        {
            var stable = this.inStableOrbit;

            var sj = ShipJuiceManager.Instance;
            if(sj == null)
            {
                base.OnUpdate();
                return;
            }

            if (activated && stable == "Ready for Trade")
                //TODO: find a better way
            {
                sj.SetDockActivation(part.craftID, true, part.name);
            }
            else
            {
                sj.SetDockActivation(part.craftID, false, part.name);
            }

            var resources = sj.GetResources(part.craftID, part.name);

            foreach (var res in resources)
            {
                if (res.Item2 > 0)
                {
                    part.RequestResource(res.Item1, -res.Item2);
                }
            }
            
            base.OnUpdate();
        }

        [KSPEvent(active = true,
            externalToEVAOnly = false, guiActive = true, guiActiveEditor = false,
            guiActiveUncommand = true, guiActiveUnfocused = false)]
        public void ActivateDock()
        {
            activated = !activated;
            Events["ActivateDock"].guiName = activated ? "Deactivate Civilian Dock" : "Activate Civilian Dock";
            lastTick = Planetarium.GetUniversalTime();

            if (activated)
            {
                //also open menu
                EditImportExport();
            }
        }

        [KSPEvent(active = false,
            externalToEVAOnly = false, guiActive = true, guiActiveEditor = false,
            guiActiveUncommand = true, guiActiveUnfocused = false, guiName = "Edit Import/Export")]
        public void EditImportExport()
        {
            FlipMenus(true);
            ImportExportWindow.ShowGUI(this);
        }

        [KSPEvent(active = false,
           externalToEVAOnly = false, guiActive = true, guiActiveEditor = false,
           guiActiveUncommand = true, guiActiveUnfocused = false, guiName = "Close UI")]
        public void CloseImportExport()
        {
            FlipMenus(false);
            ImportExportWindow.HideGUI();
        }

        public void FlipMenus(bool visible)
        {
            Events["EditImportExport"].active = !visible;
            Events["CloseImportExport"].active = visible;
        }

        public uint GetCraftId()
        {
            return part.craftID;
        }

        public void ToggleResource(string resName)
        {
            var sjm = ShipJuiceManager.Instance;
            if(sjm == null)
            {
                return;
            }

            var resourceTuple = new Tuple<string, uint>(resName,part.craftID);

            if (sjm.disabledResources.Contains(resourceTuple))
            {
                sjm.disabledResources.Remove(resourceTuple);
            }
            else
            {
                sjm.disabledResources.Add(resourceTuple);
            }
        }
    }
}
