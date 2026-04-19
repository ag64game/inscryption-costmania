using APIPlugin;
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
using InscryptionAPI.Dialogue;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Nodes;
using InscryptionAPI.PixelCard;
using InscryptionAPI.Regions;
using InscryptionAPI.Sound;
using InscryptionAPI.Triggers;
using InscryptionCommunityPatch.Card;
using InscryptionCommunityPatch.PixelTutor;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using Sirenix.Serialization.Utilities;
using Steamworks;
using StressCost.Cost;
using StressCost.Sigils;
using StressCost.Sigils.VariableStats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;
using static InscryptionAPI.CardCosts.CardCostManager;
using static System.Net.Mime.MediaTypeNames;

namespace StressCost.Patches
{
    internal class SporePatches
    {
        [HarmonyPatch(typeof(MycologistsDialogueNPC), "Start")]
        public class GBCMycosPatch
        {
            [HarmonyPrefix]
            public static void MycosPrestart(MycologistsDialogueNPC __instance)
            {
                List<CardInfo> victims = __instance.requiredCards;
                List<CardInfo> results = __instance.fusedCards;

                victims.Add(CardLoader.GetCardByName("Alchemy_Golem"));
                results.Add(CardLoader.GetCardByName("Alchemy_Golem_Fused"));

                victims.Add(CardLoader.GetCardByName("Stress_Trypano"));
                results.Add(CardLoader.GetCardByName("Stress_Trypano_Fused"));

                victims.Add(CardLoader.GetCardByName("Space_WormHole"));
                results.Add(CardLoader.GetCardByName("Space_WormHole_Fused"));

                victims.Add(CardLoader.GetCardByName("Valor_Oracle"));
                results.Add(CardLoader.GetCardByName("Valor_Oracle_Fused"));

                __instance.requiredCards = victims;
                __instance.fusedCards = results;
            }    
        }
}

    public class SporeBackground : PixelAppearanceBehaviour
    {
        public override Sprite OverrideBackground()
        {
            Texture2D texture = TextureHelper.GetImageAsTexture($"spore_cards_decal.png", typeof(CostmaniaPlugin).Assembly);
            return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                    );
        }
    }
}
