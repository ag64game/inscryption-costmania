using BepInEx;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Logging;
using DiskCardGame;
using DiskCardGame;
using EasyFeedback.APIs;
using EasyFeedback.APIs;
using GBC;
using GBC;
using HarmonyLib;
using HarmonyLib;
using InscryptionAPI.Ascension;
using InscryptionAPI.Ascension;
using InscryptionAPI.Boons;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using InscryptionAPI.Card;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Encounters;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Nodes;
using InscryptionAPI.Nodes;
using InscryptionAPI.PixelCard;
using InscryptionAPI.PixelCard;
using InscryptionAPI.Regions;
using InscryptionAPI.Regions;
using InscryptionAPI.Sound;
using InscryptionAPI.Sound;
using InscryptionAPI.Triggers;
using InscryptionAPI.Triggers;
using InscryptionCommunityPatch.Card;
using InscryptionCommunityPatch.Card;
using InscryptionCommunityPatch.PixelTutor;
using InscryptionCommunityPatch.PixelTutor;
using Pixelplacement;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using Pixelplacement.TweenSystem;
using Sirenix.Serialization.Utilities;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;
using Steamworks;
using Steamworks;
using StressCost.Cost;
using StressCost.Cost;
using StressCost.Sigils;
using StressCost.Sigils;
using StressCost.Sigils.VariableStats;
using StressCost.Sigils.VariableStats;
using System;
using System;
using System.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
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
using CustomLine = InscryptionAPI.Dialogue.CustomLine;
using PoeFace = DiskCardGame.P03AnimationController.Face;

namespace StressCost.Patches
{
    internal class DialoguePatches
    {
        private static DialogueSpeaker curSpeaker;
        private static int tutorialState = 0;

        [HarmonyPatch(typeof(DialogueHandler), nameof(DialogueHandler.PlayDialogueEvent))]
        [HarmonyPostfix]
        public static IEnumerator TutorialScene1(IEnumerator enumerator, DialogueHandler __instance, string eventId, TextBox.Style style, DialogueSpeaker speaker)
        {
            Debug.Log(eventId);
            if (eventId.Contains("ResourceTutorial"))
            {
                curSpeaker = speaker;
                if (Singleton<PixelPlayerHand>.Instance.cardsInHand.Any(card => card.Info.GetModPrefix() != null && card.Info.GetModPrefix().Contains(NessecaryCustomTemple())))
                {
                    if (tutorialState == 0 || eventId.Contains("2") || eventId.Contains("3")) yield break;
                    yield return PlayTutorialScene1();

                    yield break;
                }
                else yield return enumerator;
            }
            else
            {
                yield return enumerator;
            } 
        }

        private static string NessecaryCustomTemple()
        {
            switch (DialogueManager.GetStyleFromAmbition())
            {
                case TextBox.Style.Nature:
                    return CostmaniaPlugin.NEW_TEMPLES[0];

                case TextBox.Style.Undead:
                    return CostmaniaPlugin.NEW_TEMPLES[1];

                case TextBox.Style.Tech:
                    return CostmaniaPlugin.NEW_TEMPLES[2];

                case TextBox.Style.Magic:
                    return CostmaniaPlugin.NEW_TEMPLES[3];

                default:
                    throw new ArgumentException("No temple was selected");

            }
        }

        //In case you didn't immediately start with required cards in your hand
        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        public static IEnumerator TutorialScene1Later(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            yield return enumerator;

            if (SaveManager.SaveFile.IsPart2 && tutorialState == 0 &&
                Singleton<PixelPlayerHand>.Instance.cardsInHand.Any(card => card.Info.GetModPrefix() != null && card.Info.GetModPrefix().Contains(NessecaryCustomTemple())))
            {
                yield return PlayTutorialScene1();
            }
        }

        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.AddEnergy))]
        [HarmonyPostfix]
        public static IEnumerator TutorialScene2(IEnumerator enumerator, ResourcesManager __instance, int amount)
        {
            yield return enumerator;

            if (SaveManager.SaveFile.IsPart2 && tutorialState == 1)
            {
                yield return new WaitForSeconds(0.85f);
                yield return PlayTutorialScene2();
            }
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        public static IEnumerator TutorialScene3(IEnumerator enumerator, BoardManager __instance, PlayableCard card, CardSlot slot)
        {
            yield return enumerator;

            if (tutorialState == 2 && card.Info.GetModPrefix() != null && card.Info.GetModPrefix().Contains(NessecaryCustomTemple()))
            {
                yield return new WaitForSeconds(0.6f);
                yield return PlayTutorialScene3();
            }
        }

        public static IEnumerator PlayTutorialScene1()
        {
            List<CustomLine> lines = new List<CustomLine>();
            switch (DialogueManager.GetStyleFromAmbition())
            {
                case (TextBox.Style.Nature):
                    lines = AlchemyTutorialLines();
                    break;
                case (TextBox.Style.Undead):
                    lines = StressTutorialLines();
                    break;
                case (TextBox.Style.Tech):
                    lines = SpaceTutorialLines();
                    break;
                case (TextBox.Style.Magic):
                    lines = ValorTutorialLines();
                    break;
            }

            DialogueEvent e = DialogueManager.GenerateEvent(CostmaniaPlugin.GUID, "introduceNewCost1", lines);
            yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(e.id, TextBox.Style.Nature, curSpeaker);
            tutorialState = 1;
        }
        public static IEnumerator PlayTutorialScene2()
        {
            List<CustomLine> lines2 = new List<CustomLine>();
            switch (DialogueManager.GetStyleFromAmbition())
            {
                case (TextBox.Style.Nature):
                    lines2 = AlchemyTutorialLines2();
                    break;
                case (TextBox.Style.Undead):
                    lines2 = StressTutorialLines2();
                    break;
                case (TextBox.Style.Tech):
                    lines2 = SpaceTutorialLines2();
                    break;
                case (TextBox.Style.Magic):
                    lines2 = ValorTutorialLines2();
                    break;
            }

            DialogueEvent e2 = DialogueManager.GenerateEvent(CostmaniaPlugin.GUID, "introduceNewCost2", lines2);
            yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(e2.id, TextBox.Style.Nature, curSpeaker);
            tutorialState = 2;
        }
        public static IEnumerator PlayTutorialScene3()
        {
            List<CustomLine> lines3 = new List<CustomLine>();
            switch (DialogueManager.GetStyleFromAmbition())
            {
                case (TextBox.Style.Nature):
                    lines3 = AlchemyTutorialLines3();
                    break;
                case (TextBox.Style.Undead):
                    lines3 = StressTutorialLines3();
                    break;
                case (TextBox.Style.Tech):
                    lines3 = SpaceTutorialLines3();
                    break;
                case (TextBox.Style.Magic):
                    lines3 = ValorTutorialLines3();
                    break;
            }

            DialogueEvent e3 = DialogueManager.GenerateEvent(CostmaniaPlugin.GUID, "introduceNewCost3", lines3);
            yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(e3.id, TextBox.Style.Nature, curSpeaker);
            Singleton<GBCEncounterManager>.Instance.SetCardBattleMechanicsLearned();
            tutorialState = 3;
        }

        private static List<CustomLine> AlchemyTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("You seem way too interested in these", emote: Emotion.Neutral));
            ret.Add(GenFromString("\"Creatures\"", emote: Emotion.Anger));
            ret.Add(GenFromString("To think someone wanted these blights upon nature among my proud beasts", emote: Emotion.Neutral));
            ret.Add(GenFromString("But I digress", emote: Emotion.Neutral));
            ret.Add(GenFromString("Allow me to show you how they work", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> AlchemyTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("At the beginning of every turn, you receive one Alchemy Die, it's value is one of three...", emote: Emotion.Neutral));
            ret.Add(GenFromString("Flesh, Metal, and Elixir", emote: Emotion.Anger));
            ret.Add(GenFromString("The value on the die changes every turn it is not spent", emote: Emotion.Neutral));
            ret.Add(GenFromString("Though you may consider pressing on a die to Lock it, such that it's value remains still", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> AlchemyTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("You played the card?", emote: Emotion.Neutral));
            ret.Add(GenFromString("Excellent", emote: Emotion.Anger));
            ret.Add(GenFromString("Truly Excellent", emote: Emotion.Neutral));
            ret.Add(GenFromString("I eargerly await your thrashing amidst my cabin walls", emote: Emotion.Neutral));
            ret.Add(GenFromString("Good luck", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> StressTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Intriguing! I see you have elected to deploy the modded Paranoia cards!", emote: Emotion.Neutral));
            ret.Add(GenFromString("I'll have you know the Scrybe in charge of these is quite the lovely individual", emote: Emotion.Neutral));
            ret.Add(GenFromString("I suppose then... That it is time for you...", emote: Emotion.Neutral));
            ret.Add(GenFromString("To learn FEAR", emote: Emotion.Anger));

            return ret;
        }
        private static List<CustomLine> StressTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("These cards cost \"Stress\", you may play them whenever you wish", emote: Emotion.Neutral));
            ret.Add(GenFromString("However!", emote: Emotion.Anger));
            ret.Add(GenFromString("Playing one of them will add their \"Cost\" to the dastardly Stress Counter", emote: Emotion.Anger));
            ret.Add(GenFromString("And when you ring the bell, you will take damage equal to half of the number shown on the Stress Counter", emote: Emotion.Neutral));
            ret.Add(GenFromString("Then it will drop by 2!", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> StressTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Notice how the Stress Counter went up?", emote: Emotion.Neutral));
            ret.Add(GenFromString("I believe it is time for you to strike hard", emote: Emotion.Neutral));
            ret.Add(GenFromString("For it is a race against your own nerves, beating you from behind your back", emote: Emotion.Anger));
            ret.Add(GenFromString("Careful not to kill yourself challenger!", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> SpaceTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Oh my god you stupid moron you actually WANT these?", emote: Emotion.Anger));
            ret.Add(GenFromString("You have some kind of allergy to decent decks? or are you just that dumb?", emote: Emotion.Anger));
            ret.Add(GenFromString("Whatever. I'll just show you how they work", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> SpaceTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Space cards cost \"Stardust\". You gain one every time you place a card", emote: Emotion.Neutral));
            ret.Add(GenFromString("Then at the end of the turn you lose all the Stardust", emote: Emotion.Neutral));
            ret.Add(GenFromString("It's RNG HELL if you don't get any good cards", emote: Emotion.Anger));
            ret.Add(GenFromString("But if it's any consolation prize they're at least not spent when a Space card is played", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> SpaceTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Congratulations! You played a card", emote: Emotion.Neutral));
            ret.Add(GenFromString("I'm so proud of you", emote: Emotion.Neutral));
            ret.Add(GenFromString("Just leave me to my work already", emote: Emotion.Anger));

            return ret;
        }

        private static List<CustomLine> ValorTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Ah, you have invested in Valor I see, a curious set of cards indeed", emote: Emotion.Neutral));
            ret.Add(GenFromString("You have been modding the game I presume...", emote: Emotion.Anger));
            ret.Add(GenFromString("Regardless...", emote: Emotion.Neutral));
            ret.Add(GenFromString("Allow me to explain to you it's workings", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> ValorTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("You may not see it now, but each cards you play now bears a \"Valor Rank\"", emote: Emotion.Neutral));
            ret.Add(GenFromString("When a card posseses a certain rank, cards which cost equal or less Valor may be played", emote: Emotion.Neutral));
            ret.Add(GenFromString("The grey stat on a card, such as those on the War Banners, represents their Rank", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> ValorTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Now, keep this card until the end of the turn and you may get the chance to Promote it", emote: Emotion.Neutral));
            ret.Add(GenFromString("One can promote any card they control that is not a Terrain card, increasing their Valor Rank by 1", emote: Emotion.Neutral));
            ret.Add(GenFromString("Prey tell you will take these warriors atop my tower where I lie in wait", emote: Emotion.Neutral));
            ret.Add(GenFromString("Safe travels", emote: Emotion.Neutral));

            return ret;
        }

        private static CustomLine GenFromString(string text, DialogueEvent.Speaker speaker = DialogueEvent.Speaker.Single, Emotion emote = Emotion.Neutral, PoeFace face = PoeFace.Default)
        {
            CustomLine ret = text;
            ret.speaker = speaker;
            ret.emotion = emote;

            return ret;
        }
    }
}
