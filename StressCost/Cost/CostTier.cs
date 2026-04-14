using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Cost
{
    internal class CostTier
    {
        public static int CostTierS(int amount) => Convert.ToInt32(Mathf.Floor(amount / 2.5f));
        public static int CostTierV(int amount) => Convert.ToInt32(Mathf.Floor(amount / 2.25f));
        public static int CostTierA(int amount) => Convert.ToInt32(Mathf.Floor(amount / 1.8f));
        public static int CostTierF(int amount) => Convert.ToInt32(Mathf.Floor(amount / 2.8f));
    }
}
