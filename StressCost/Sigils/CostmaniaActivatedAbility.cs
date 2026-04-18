using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using EasyFeedback.APIs;
using GBC;
using HarmonyLib;
using InscryptionAPI.Ascension;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Nodes;
using InscryptionAPI.PixelCard;
using InscryptionAPI.Regions;
using InscryptionAPI.Sound;
using InscryptionAPI.Triggers;
using InscryptionCommunityPatch.Card;
using Pixelplacement.TweenSystem;
using Steamworks;
using StressCost.Cost;
using StressCost.Patches;
using StressCost.Sigils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;
using static InscryptionAPI.CardCosts.CardCostManager;

namespace StressCost.Sigils
{

    [HarmonyPatch]
    public abstract class CostmaniaActivatedAbility : ActivatedAbilityBehaviour
    {
        public virtual int StressCost
        {
            get
            {
                return 0;
            }
        }

        public virtual int ValorCost
        {
            get
            {
                return 0;
            }
        }

        public virtual int ValorRankCost
        {
            get
            {
                return 0;
            }
        }

        public virtual int StardustCost
        {
            get
            {
                return 0;
            }
        }

        public virtual int FleshCost
        {
            get
            {
                return 0;
            }
        }

        public virtual int MetalCost
        {
            get
            {
                return 0;
            }
        }

        public virtual int ElixirCost
        {
            get
            {
                return 0;
            }
        }

        public override bool CanActivate()
        {
            return base.CanActivate() && Cost.ValorCost.MaxRank >= this.ValorCost && Card.ValorRank() >= this.ValorRankCost && Cost.StardustCost.stardustCounter >= this.StardustCost
                && AlchemyCounter.fleshCount >= this.FleshCost && AlchemyCounter.metalCount >= this.MetalCost
                && AlchemyCounter.elixirCount >= this.ElixirCost;
        }

        public override IEnumerator Activate()
        {
            yield return true;
        }
    }
}
