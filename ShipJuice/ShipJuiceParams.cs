using System;
using UnityEngine;

namespace ShipJuice
{
    public class ShipJuiceParams : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomFloatParameterUI("Dock Delivery Time", asPercentage = false, 
            autoPersistance = true, gameMode = GameParameters.GameMode.ANY, 
            maxValue = 365, minValue = .01f, newGameOnly = false, displayFormat = "N2",
            title = "Dock Delivery Time", toolTip = "Time in Days")]
        public float ShipJuiceTickTime = 7f;

        [GameParameters.CustomFloatParameterUI("Kerbin Resource Cost Multiplier", asPercentage = false, 
            autoPersistance = true, gameMode = GameParameters.GameMode.ANY, 
            maxValue = 200f, minValue = 0f, newGameOnly = false, displayFormat = "N2",
            title = "Kerbin Resource Cost Multiplier", toolTip = "How much markup civs get for shipping resources to you")]
        public float KerbinResourceCostMultiplier = 10f;

        [GameParameters.CustomFloatParameterUI("Amount Shipped", asPercentage = true,
            autoPersistance = true, gameMode = GameParameters.GameMode.ANY,
            maxValue = 10f, minValue = 0f, newGameOnly = false, displayFormat = "N2",
            title = "Amount Shipped", toolTip = "Mass per shipment")]
        public float AmountShipped = 1f;

        public override string DisplaySection
        {
            get
            {
                return "ShipJuice";
            }
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return true;
            }
        }

        public override string Section
        {
            get
            {
                return "ShipJuice";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 0;
            }
        }

        public override string Title
        {
            get
            {
                return "Civilian Dock";
            }
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    ShipJuiceTickTime = 7;
                    KerbinResourceCostMultiplier = 2;
                    AmountShipped = 10f;
                    break;
                case GameParameters.Preset.Normal:
                case GameParameters.Preset.Custom:
                    ShipJuiceTickTime = 7;
                    KerbinResourceCostMultiplier = 10;
                    AmountShipped = 10f;
                    break;
                case GameParameters.Preset.Moderate:
                    ShipJuiceTickTime = 7;
                    KerbinResourceCostMultiplier = 15;
                    AmountShipped = 10f;
                    break;
                case GameParameters.Preset.Hard:
                    ShipJuiceTickTime = 14;
                    KerbinResourceCostMultiplier = 25;
                    AmountShipped = 10f;
                    break;
            }
        }
    }
}
