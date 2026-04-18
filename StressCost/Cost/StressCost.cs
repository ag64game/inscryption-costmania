using DiskCardGame;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using InscryptionCommunityPatch.Card;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using static InscryptionAPI.CardCosts.CardCostManager;

namespace StressCost.Cost
{
    internal class StressCost : CustomCardCost
    {
        private static int __prev = 0;
        private static int __counter = 0;
        public static int stressCounter
        {
            get
            {
                return __counter;
            }

            set
            {
                __counter = value;
                try { Patches.CostGraphicPatches.disStressCounter.DisplayValue(Cost.StressCost.stressCounter); } catch { }
            }
        }

        private static int secondPlayer = 0;

        public override string CostName => "StressCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return true;
        }

        public static void SwitchPlayer() => (secondPlayer, stressCounter) = (stressCounter, secondPlayer);
        public static void ResetPlayerTwo() => secondPlayer = 0;

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"StressCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_stress.png", typeof(CostmaniaPlugin).Assembly));
        }
    }

    public static class CardStressExpansion
    {
        public static int StressCost(this CardInfo card)
        {
            int? baseVal = card.GetExtendedPropertyAsInt("StressCost");
            if (baseVal == null) baseVal = 0;

            return baseVal.Value;
        }
    }
}
