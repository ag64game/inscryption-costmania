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
using InscryptionCommunityPatch.PixelTutor;
using Newtonsoft.Json.Bson;
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
    internal class EncounterPatches
    {
        private static bool isP03 = false, isLeshy = false;
        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPrefix]
        public static void ReplaceAIEncounters(TurnManager __instance, ref EncounterData encounterData)
        {
            if (isP03)
            {
                encounterData = P03EncounterMod(encounterData);
                isP03 = false;
            }

            if (isLeshy)
            {
                encounterData = LeshyEncounterMod(encounterData);
                isLeshy = false;
            }

            try
            {
                var grimora = Singleton<GrimoraBossOpponent>.Instance;
                grimora.TurnPlan = GrimoraEncounterMod(grimora.TurnPlan);
            } catch { Debug.Log("Not Grimora"); }
        }

        [HarmonyPatch(typeof(CardBattleNPC), nameof(CardBattleNPC.CreateEncounterData))]
        [HarmonyPostfix]
        public static void AddNewCards(ref CardBattleNPC __instance, ref EncounterData __result)
        {
            EncounterData enc;

            switch(__instance.greetingDialogueId)
            {
                case ("ProspectorNPCGreeting"):
                    enc = ProspectorEncounterMod(__result);
                    break;
                case ("AnglerNPCGreeting"):
                    enc = AnglerEncounterMod(__result);
                    break;
                case ("TrapperNPCGreeting"):
                    enc = TrapperEncounterMod(__result);
                    break;
                case ("LeshyGBCIntro"):
                    isLeshy = true;
                    enc = __result;
                    break;
                case ("GhoulBriarGreeting"):
                    enc = KayceeEncounterMod(__result);
                    break;
                case ("GhoulSawyerGreeting"):
                    enc = SawyerEncounterMod(__result);
                    break;
                case ("GhoulRoyalGreeting"):
                    enc = RoyalEncounterMod(__result);
                    break;
                case ("InspectorGreeting"):
                    enc = InspectorEncounterMod(__result);
                    break;
                case ("SmelterGreeting"):
                    enc = MelterEncounterMod(__result);
                    break;
                case ("DredgerGreeting"):
                    enc = DredgerEncounterMod(__result);
                    break;
                case ("P03GBCGreeting"):
                    isP03 = true;
                    enc = __result;
                    break;
                case ("WizardGreenGreeting"):
                    enc = GoobertEncounterMod(__result);
                    break;
                case ("WizardOrangeGreeting"):
                    enc = AmberEncounterMod(__result);
                    break;
                case ("WizardBlueGreeting"):
                    enc = LonelyEncounterMod(__result);
                    break;
                case ("MagnificusGreeting"):
                    enc = MagnificusEncounterMod(__result);
                    break;
                default:
                    enc = __result;
                    break;
            }

            __result = enc;
        }

        private static void PrintEncounter(EncounterData enc)
        {
            int aga = 0;

            enc.opponentTurnPlan.ForEach(turn =>
            {
                aga++;
                turn.ForEach(data => Debug.Log($"{aga}: {data.name}"));
            });
        }
        private static void PrintEncounter(EncounterBlueprintData enc)
        {
            int aga = 0;

            enc.turns.ForEach(turn =>
            {
                aga++;
                turn.ForEach(data => Debug.Log($"{aga}: {data.card.name}"));
            });
        }

        private static EncounterData ProspectorEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[0][0] = CardLoader.GetCardByName("Alchemy_Zeppeloid");
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Alchemy_Cyborg"));
            enc.opponentTurnPlan[4].Add(CardLoader.GetCardByName("Alchemy_Zeppeloid"));
            enc.opponentTurnPlan[6].Add(CardLoader.GetCardByName("Alchemy_Biborg"));

            enc.opponentTurnPlan.Add(new List<CardInfo>());

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Squirrel"));
            newTurn.Add(CardLoader.GetCardByName("Alchemy_CyborgPrime"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData AnglerEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[1].Add(CardLoader.GetCardByName("Alchemy_Spite"));
            enc.opponentTurnPlan[0][0] = CardLoader.GetCardByName("Alchemy_DeepOne");

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Salmon"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData TrapperEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[1][0] = CardLoader.GetCardByName("Alchemy_GreaterHomonculus");
            enc.opponentTurnPlan[3].Add(CardLoader.GetCardByName("Alchemy_Homonculus"));
            enc.opponentTurnPlan[6][0] = CardLoader.GetCardByName("Alchemy_Mistwalker");

            return enc;
        }

        private static EncounterData LeshyEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[1].Add(CardLoader.GetCardByName("Alpha"));
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Stoat"));
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Stoat"));
            enc.opponentTurnPlan[4][0] = CardLoader.GetCardByName("MantisGod");
            enc.opponentTurnPlan[5].Add(CardLoader.GetCardByName("WolfCub"));

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("MantisGod"));
            newTurn.Add(CardLoader.GetCardByName("Coyote"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData KayceeEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[0][1] = CardLoader.GetCardByName("Stress_Aero");
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Stress_Myso"));

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Stress_Aero"));
            newTurn.Add(CardLoader.GetCardByName("Banshee"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData SawyerEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Stress_Claustro"));
            enc.opponentTurnPlan[3][0] = CardLoader.GetCardByName("Stress_Myso");

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Stress_Trypano"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData RoyalEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[0].Add(CardLoader.GetCardByName("Stress_Nycto"));
            enc.opponentTurnPlan[1].Add(CardLoader.GetCardByName("Stress_Micro"));

            enc.opponentTurnPlan[3].Add(CardLoader.GetCardByName("Stress_Micro"));
            enc.opponentTurnPlan[3][0] = CardLoader.GetCardByName("Stress_Arachno");

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Stress_Obeso"));
            newTurn.Add(CardLoader.GetCardByName("Stress_Ekrixi"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        public static List<List<CardInfo>> GrimoraEncounterMod(List<List<CardInfo>> enc)
        {
            enc.Add(new List<CardInfo>());
            enc.Add(new List<CardInfo>());

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Stress_Ekrixi"));
            enc.Add(newTurn);

            enc.Add(new List<CardInfo>());
            enc.Add(new List<CardInfo>());
            enc.Add(new List<CardInfo>());

            List<CardInfo> newTurn2 = new List<CardInfo>();
            newTurn2.Add(CardLoader.GetCardByName("Stress_Dystychi"));
            enc.Add(newTurn2);

            enc.Add(new List<CardInfo>());
            enc.Add(new List<CardInfo>());
            enc.Add(new List<CardInfo>());
            enc.Add(new List<CardInfo>());

            List<CardInfo> gameOver = new List<CardInfo>();
            gameOver.Add(CardLoader.GetCardByName("Stress_Thanato"));
            enc.Add(gameOver);

            return enc;
        }

        private static EncounterData InspectorEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[1][0] = CardLoader.GetCardByName("Space_CookingQuarter");

            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Space_ShootingStar"));
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Space_Alien"));

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Space_CookingQuarter"));
            newTurn.Add(CardLoader.GetCardByName("Space_ShootingStar"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData MelterEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[0].Add(CardLoader.GetCardByName("Space_KillerComet"));
            enc.opponentTurnPlan[1].Add(CardLoader.GetCardByName("Space_MutatedMeteorite"));

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Space_KillerComet"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData DredgerEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[0].Add(CardLoader.GetCardByName("Space_Railgun"));
            enc.opponentTurnPlan[0].Add(CardLoader.GetCardByName("Space_Railgun"));
            enc.opponentTurnPlan[0].Add(CardLoader.GetCardByName("Space_Railgun"));
            enc.opponentTurnPlan[0].Add(CardLoader.GetCardByName("Space_Railgun"));

            return enc;
        }

        private static EncounterData P03EncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[2][1] = CardLoader.GetCardByName("Space_BlackholeBehemoth");

            enc.opponentTurnPlan[3].Add(CardLoader.GetCardByName("Space_CookingQuarter"));

            enc.opponentTurnPlan[4][0] = CardLoader.GetCardByName("Space_WormHole");

            enc.opponentTurnPlan[6].Add(CardLoader.GetCardByName("Space_CookingQuarter"));

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Space_BlackholeBehemoth"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData GoobertEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[0][2] = CardLoader.GetCardByName("Valor_ReconScout");

            enc.opponentTurnPlan[1].Add(CardLoader.GetCardByName("Valor_SentryScout"));
            enc.opponentTurnPlan[1].Add(CardLoader.GetCardByName("Valor_ReconScout"));

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Valor_ReconScout"));
            newTurn.Add(CardLoader.GetCardByName("Valor_SentryScout"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData AmberEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[1].Add(CardLoader.GetCardByName("Valor_Vestal"));
            enc.opponentTurnPlan[2][0] = CardLoader.GetCardByName("Valor_BloodLancer");
            enc.opponentTurnPlan[3].Add(CardLoader.GetCardByName("Valor_BloodLancer"));
            enc.opponentTurnPlan[4][0] = CardLoader.GetCardByName("Valor_Vestal");
            return enc;
        }

        private static EncounterData LonelyEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Valor_CorsairPirate"));
            enc.opponentTurnPlan[3][0] = CardLoader.GetCardByName("Valor_Blunderbuss");

            enc.opponentTurnPlan.Add(new List<CardInfo>());

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("Valor_CorsairPirate"));
            newTurn.Add(CardLoader.GetCardByName("Valor_Blunderbuss"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }

        private static EncounterData MagnificusEncounterMod(EncounterData enc)
        {
            enc.opponentTurnPlan[0].Add(CardLoader.GetCardByName("Valor_NascentSquire"));
            enc.opponentTurnPlan[2].Add(CardLoader.GetCardByName("Valor_NascentSquire"));
            enc.opponentTurnPlan[4].Add(CardLoader.GetCardByName("Valor_SentryScout"));
            enc.opponentTurnPlan[4][0] = CardLoader.GetCardByName("Valor_Commandant");
            enc.opponentTurnPlan[7][0] = CardLoader.GetCardByName("Valor_Blunderbuss");
            enc.opponentTurnPlan[7].Add(CardLoader.GetCardByName("Valor_SentryScout"));

            enc.opponentTurnPlan.Add(new List<CardInfo>());

            List<CardInfo> newTurn = new List<CardInfo>();
            newTurn.Add(CardLoader.GetCardByName("MasterGoranj"));
            newTurn.Add(CardLoader.GetCardByName("Valor_Blunderbuss"));
            enc.opponentTurnPlan.Add(newTurn);

            return enc;
        }
    }
}
