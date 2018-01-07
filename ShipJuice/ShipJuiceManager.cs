using Smooth.Algebraics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShipJuice
{
    public class ShipJuiceManager
    {
        public static ShipJuiceManager Instance;

        public static void Init()
        {
            Instance = new ShipJuiceManager();
        }
        
        public static float shipJuiceTickTime = 7f;
        public static float kerbinResourceCostMultiplier = 10f;
        public static float amountShipped = 1f;
        public string data = "";

        public List<Tuple<string, uint>> disabledResources = new List<Tuple<string, uint>>();
        public Dictionary<uint, Tuple<string, double>> importDocks = new Dictionary<uint, Tuple<string, double>>();

        public Dictionary<string, PartResourceDefinition> includedResources = new Dictionary<string, PartResourceDefinition>();
        
        public Dictionary<uint, List<Tuple<string, double>>> outgoingResources = new Dictionary<uint, List<Tuple<string, double>>>();
        
        public ShipJuiceManager()
        {
            var prl = PartResourceLibrary.Instance;

            //cache some resources
            foreach (var def in prl.resourceDefinitions)
            {
                includedResources[def.name] = def;
            }

            //Remove some dumb things
            includedResources.Remove("ElectricCharge");
            includedResources.Remove("IntakeAir");
            includedResources.Remove("EVA Propellant");
            includedResources.Remove("Albator");
        }

        internal void Save(ConfigNode dockManager)
        {
            dockManager.SetValue("Data", data, true);
        }

        internal void Load(ConfigNode dockManager)
        {
            data = dockManager.HasValue("Data") ?
                dockManager.GetValue("Data") : "";

            importDocks = DataToDictionary(data);
            disabledResources = DataToDisabledList(data);
        }

        public void Update()
        {
            var currentTimestamp = Planetarium.GetUniversalTime();

            var keys = importDocks.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];

                var dockPart = importDocks[key].Item1;

                var dockLastTick = importDocks[key].Item2;

                if (dockLastTick == 0)
                {
                    importDocks[key] = new Tuple<string, double>(dockPart, dockLastTick = Planetarium.GetUniversalTime());
                }

                var delta = (((currentTimestamp - dockLastTick) / 3600) / 5);

                if (delta >= shipJuiceTickTime)
                {
                    var divtimes = (int)(delta / shipJuiceTickTime);

                    importDocks[key] = new Tuple<string, double>(dockPart, dockLastTick + divtimes * shipJuiceTickTime * 5 * 3600);

                    foreach (var resName in includedResources.Keys)
                    {
                        if(disabledResources.Contains(new Tuple<string, uint>(resName, key)))
                        {
                            continue;
                        }

                        var res = includedResources[resName];

                        var requested = divtimes * (res.volume / res.density) * amountShipped;

                        if (dockPart == null)
                        {
                            Debug.LogError("dockPart is null!");
                            break;
                        }

                        AddResources(key, res.name, requested);

                        Funding.Instance.AddFunds(-requested * res.unitCost * kerbinResourceCostMultiplier, TransactionReasons.Cheating);
                    }
                }
            }
        }

        internal void AddResources(uint craftId, string resourceName, double amount)
        {
            if (!outgoingResources.ContainsKey(craftId))
            {
                outgoingResources[craftId] = new List<Tuple<string, double>>();
            }

            outgoingResources[craftId].Add(new Tuple<string, double>(resourceName, amount));
        }

        internal List<Tuple<string, double>> GetResources(uint craftID, string name)
        {
            if (outgoingResources.ContainsKey(craftID))
            {
                var toReturn = outgoingResources[craftID];

                outgoingResources.Remove(craftID);

                return toReturn;
            }
            return new List<Tuple<string, double>>();
        }

        internal void SetDockActivation(uint id, bool active, string part)
        {
            if (!active)
            {
                importDocks.Remove(id);
            }
            else
            {
                if (!importDocks.ContainsKey(id))
                {
                    foreach (var res in includedResources.Keys)
                    {
                        var resTuple = new Tuple<string, uint>(res, id);
                        if (!disabledResources.Contains(resTuple))
                        {
                            disabledResources.Add(resTuple);
                        }
                    }
                }

                importDocks[id] = new Tuple<string, double>(part, Planetarium.GetUniversalTime());
                
                UpdateData();
            }
        }

        private Dictionary<uint, Tuple<string, double>> DataToDictionary(string data)
        {
            var output = new Dictionary<uint, Tuple<string, double>>();

            if (string.IsNullOrEmpty(data))
                return output;

            data = data.Split('-')[0];

            if (string.IsNullOrEmpty(data))
                return output;

            var splitData = data.Split(';');

            foreach (var d in splitData)
            {
                var splitDatum = d.Split(',');

                output.Add(UInt32.Parse(splitDatum[0]), new Tuple<string, double>(splitDatum[1], Double.Parse(splitDatum[2])));
            }

            return output;
        }

        private List<Tuple<string, uint>> DataToDisabledList(string data)
        {
            var output = new List<Tuple<string, uint>>();

            if (string.IsNullOrEmpty(data))
                return output;

            data = data.Split('-')[1];

            if (string.IsNullOrEmpty(data))
                return output;

            var splitData = data.Split(';');

            foreach (var d in splitData)
            {
                var splitDatum = d.Split(',');

                output.Add(new Tuple<string, uint>(splitDatum[0], uint.Parse(splitDatum[1])));
            }

            return output;
        }

        private string DictionaryToData(Dictionary<uint, Tuple<string, double>> data, List<Tuple<string, uint>> list)
        {
            var output = "";

            foreach (var key in data.Keys)
            {
                if (!string.IsNullOrEmpty(output))
                {
                    output += ";";
                }

                var tuple = data[key];

                output += key + "," + tuple.Item1 + "," + tuple.Item2;
            }

            output += "-";
            var output2 = "";

            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(output2))
                {
                    output2 += ";";
                }
                
                output2 += item.Item1 + "," + item.Item2;
            }

            return output + output2;
        }

        private void UpdateData()
        {
            data = DictionaryToData(importDocks, disabledResources);
        }

    }
}
