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
using InscryptionAPI.Saves;
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
using System.Diagnostics;
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
using static InscryptionAPI.Slots.SlotModificationManager;
using static System.Net.Mime.MediaTypeNames;
using CustomLine = InscryptionAPI.Dialogue.CustomLine;
using PoeFace = DiskCardGame.P03AnimationController.Face;

namespace StressCost.Patches
{
    internal class DialoguePatches
    {
        private static DialogueSpeaker curSpeaker = new DialogueSpeaker();

        private static DialogueEvent eExplenation = null;
        private static bool tutorialPlaying = false;
        private static int tutorialState = 3;
        private static bool playLeshyRant = false;
        private static bool playP03Rant = false;

        [HarmonyPatch(typeof(DialogueHandler), nameof(DialogueHandler.PlayDialogueEvent))]
        [HarmonyPostfix]
        public static IEnumerator TutorialScene1(IEnumerator enumerator, DialogueHandler __instance, string eventId, TextBox.Style style, DialogueSpeaker speaker)
        {
            UnityEngine.Debug.Log(eventId);
            if (eventId.Contains("ResourceTutorial"))
            {
                
                if (!eventId.Contains("2") && !eventId.Contains("3"))
                {
                    curSpeaker = speaker;
                    if ((!DialogueEventsData.EventIsPlayed("ResourceTutorialBlood") &&
                    !DialogueEventsData.EventIsPlayed("ResourceTutorialBones") && !DialogueEventsData.EventIsPlayed("ResourceTutorialEnergy1") &&
                    !DialogueEventsData.EventIsPlayed("ResourceTutorialGems1"))) tutorialState = 0;
                }
            }

            else if(eventId.Contains("LeshyGBCBossPhase1"))
            {
                curSpeaker = speaker;
                if (DialogueEventsData.GetEventRepeatCount("LeshyGBCBossPhase1") < 2) playLeshyRant = true;
            }

            else if (eventId.Contains("P03BossBattleIntro"))
            {
                curSpeaker = speaker;
                if (DialogueEventsData.GetEventRepeatCount("P03BossBattleIntro") < 2) playP03Rant = true;
            }

            else if(eventId == "MechanicDocks")
            {
                curSpeaker = speaker;

                if (DialogueEventsData.GetEventRepeatCount("MechanicDocks") == 5 && DialogueEventsData.GetEventRepeatCount("MechanicDocksCostmania") < 4)
                {
                    yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(eExplenation.id, TextBox.Style.Neutral, curSpeaker);
                    yield break;
                }
            }

            yield return enumerator;
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

        public static void GenerateRebechaRant()
        {
            var mainLineA = MechanicAlchemyLine();
            var repeatLines = new List<List<CustomLine>>();
            repeatLines.Add(MechanicStressLine());
            repeatLines.Add(MechanicSpaceLine());
            repeatLines.Add(MechanicValorLine());

            eExplenation = DialogueManager.GenerateEvent(CostmaniaPlugin.GUID, "MechanicDocksCostmania",mainLineA, repeatLines);
        }

        //In case you didn't immediately start with required cards in your hand
        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        public static IEnumerator OnUpkeepScenes(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            yield return enumerator;

            if (SaveManager.SaveFile.IsPart2 && __instance.TurnNumber > 1)
            {
                if(tutorialState == 0 && HasTempleInBattle()) yield return PlayTutorialScene1();
                if (playLeshyRant)
                {
                    DialogueEvent eLeshy = DialogueManager.GenerateEvent(CostmaniaPlugin.GUID, "LeshyRant", LeshyRantLines());
                    yield return new WaitForSeconds(0.4f);
                    yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(eLeshy.id, TextBox.Style.Nature, curSpeaker);
                    yield return playLeshyRant = false;
                }

                if (playP03Rant && BoardManager.Instance != null && BoardManager.Instance.opponentSlots.Any(slot =>
                {
                    if (slot.Card != null)
                    {
                        var cond = BoardManager.Instance.GetCardQueuedForSlot(slot);
                        return cond != null && cond.Info.GetModPrefix() != null && cond.Info.GetModPrefix().Contains("Space");
                    }
                    else return false;
                }))
                {
                    DialogueEvent ePoe = DialogueManager.GenerateEvent(CostmaniaPlugin.GUID, "P03Rant", P03RantLines());
                    yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(ePoe.id, TextBox.Style.Tech, curSpeaker);
                    yield return playP03Rant = false;
                }

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
            StoryEventsData.SetEventCompleted(StoryEvent.GBCIntroCompleted, saveToFile: true);
            yield return enumerator;

            if (tutorialState == 2 && card.Info.GetModPrefix() != null && card.Info.GetModPrefix().Contains(NessecaryCustomTemple()))
            {
                yield return new WaitForSeconds(0.6f);
                yield return PlayTutorialScene3();
            }
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPrefix]
        public static void ResetTutorialState(TurnManager __instance)
        {
            tutorialState = 3;
        }

        private static bool HasTempleInBattle()
        {
            return Singleton<PixelPlayerHand>.Instance.cardsInHand.Any(card => card.Info.GetModPrefix() != null && card.Info.GetModPrefix().Contains(NessecaryCustomTemple())) ||
                Singleton<BoardManager>.Instance.playerSlots.Any(slot => slot.Card != null && slot.Card.Info.GetModPrefix() != null && slot.Card.Info.GetModPrefix().Contains(NessecaryCustomTemple()));
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
            yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(e.id, DialogueManager.GetStyleFromAmbition(), curSpeaker);
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
            yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(e2.id, DialogueManager.GetStyleFromAmbition(), curSpeaker);
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
            tutorialState = 3;
            yield return Singleton<DialogueHandler>.Instance.PlayDialogueEvent(e3.id, DialogueManager.GetStyleFromAmbition(), curSpeaker);
        }

        private static List<CustomLine> AlchemyTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("You seem way too interested in these...", emote: Emotion.Neutral));
            ret.Add(GenFromString("\"Creatures\".", emote: Emotion.Anger));
            ret.Add(GenFromString("To think someone wanted these blights upon nature among my proud beasts.", emote: Emotion.Neutral));
            ret.Add(GenFromString("But I digress...", emote: Emotion.Neutral));
            ret.Add(GenFromString("Allow me to show you how they work.", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> AlchemyTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("At the beginning of every turn, you receive one Alchemy Die, it's value is one of three...", emote: Emotion.Neutral));
            ret.Add(GenFromString("Flesh, Metal, and Elixir.", emote: Emotion.Anger));
            ret.Add(GenFromString("The value on the die changes every turn it is not spent.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Though you may consider pressing on a die to Lock it, such that it's value remains still.", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> AlchemyTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("You played the card?", emote: Emotion.Neutral));
            ret.Add(GenFromString("Excellent.", emote: Emotion.Anger));
            ret.Add(GenFromString("Truly Excellent.", emote: Emotion.Neutral));
            ret.Add(GenFromString("I eargerly await your thrashing amidst my cabin walls.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Good luck.", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> StressTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Intriguing! I see you have elected to deploy the modded Paranoia cards!", emote: Emotion.Neutral));
            ret.Add(GenFromString("I'll have you know the Scrybe in charge of these is quite the lovely individual.", emote: Emotion.Neutral));
            ret.Add(GenFromString("I suppose then... That it is time for you...", emote: Emotion.Neutral));
            ret.Add(GenFromString("To learn FEAR!", emote: Emotion.Anger));

            return ret;
        }
        private static List<CustomLine> StressTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("These cards cost \"Stress\", you may play them whenever you wish.", emote: Emotion.Neutral));
            ret.Add(GenFromString("However!", emote: Emotion.Anger));
            ret.Add(GenFromString("Playing one of them will add their \"Cost\" to the dastardly Stress Counter.", emote: Emotion.Anger));
            ret.Add(GenFromString("And when you ring the bell, you will take damage equal to half of the number on the Counter.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Then it will drop by 2!", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> StressTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Notice how the Stress Counter went up?", emote: Emotion.Neutral));
            ret.Add(GenFromString("I believe it is time for you to strike.", emote: Emotion.Neutral));
            ret.Add(GenFromString("For it is a race against your own nerves, beating you from behind your back.", emote: Emotion.Anger));
            ret.Add(GenFromString("Careful not to kill yourself challenger!", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> SpaceTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Wait...", emote: Emotion.Neutral));
            ret.Add(GenFromString("Oh my god you stupid moron you actually WANT these?", emote: Emotion.Anger));
            ret.Add(GenFromString("You have some kind of allergy to decent decks? or are you just that dumb?", emote: Emotion.Anger));
            ret.Add(GenFromString("Whatever.", emote: Emotion.Neutral));
            ret.Add(GenFromString("I'll just show you how they work.", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> SpaceTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Space cards cost \"Stardust\". You gain one every time you place a card.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Then at the end of the turn you lose all the Stardust.", emote: Emotion.Neutral));
            ret.Add(GenFromString("It's RNG HELL if you don't get any good cards.", emote: Emotion.Anger));
            ret.Add(GenFromString("But if it's any consolation prize they're at least not spent playing Stardust cards.", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> SpaceTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Congratulations! You played a card!", emote: Emotion.Neutral));
            ret.Add(GenFromString("I'm so proud of you.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Just leave me to my work already.", emote: Emotion.Anger));

            return ret;
        }

        private static List<CustomLine> ValorTutorialLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Ah, you have invested in Valor I see, a curious set of cards indeed.", emote: Emotion.Neutral));
            ret.Add(GenFromString("You have been modding the game I presume...", emote: Emotion.Anger));
            ret.Add(GenFromString("Regardless...", emote: Emotion.Neutral));
            ret.Add(GenFromString("Allow me to explain to you it's workings.", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> ValorTutorialLines2()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Each card you possess now has a stat studded in grey.", emote: Emotion.Neutral));
            ret.Add(GenFromString("That is their \"Valor Rank\".", emote: Emotion.Neutral));
            ret.Add(GenFromString("When a card posseses a certain rank, cards which cost equal or less Valor may be played.", emote: Emotion.Neutral));

            return ret;
        }
        private static List<CustomLine> ValorTutorialLines3()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Now, keep this card until the end of the turn and you may get the chance to Promote it.", emote: Emotion.Neutral));
            ret.Add(GenFromString("You may choose any sacrificable card to increase it's Valor Rank by 1.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Just like you did last turn.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Prey tell you will take these warriors atop my tower where I lie in wait.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Safe travels.", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> LeshyRantLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("And in case you were asking...", emote: Emotion.Neutral));
            ret.Add(GenFromString("Indeed my deck contains none of those putrid Alchemy cards.", emote: Emotion.Neutral));
            ret.Add(GenFromString("No chimeras of mana, no rancid mutations of flesh.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Only fangs.", emote: Emotion.Anger));
            ret.Add(GenFromString("You degenerate modder.", emote: Emotion.Anger));

            return ret;
        }

        private static List<CustomLine> P03RantLines()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Listen...", emote: Emotion.Neutral));

            if (DialogueManager.GetStyleFromAmbition() == TextBox.Style.Tech)
                ret.Add(GenFromString("I know I said these cards are kinda trash...", emote: Emotion.Neutral));
            else
                ret.Add(GenFromString("Normally I'd say these cards are kinda trash...", emote: Emotion.Neutral));

            ret.Add(GenFromString("But that's only because of Stardust", emote: Emotion.Anger));
            ret.Add(GenFromString("And guess what...", emote: Emotion.Neutral));
            ret.Add(GenFromString("I'm an NPC, dummy!", emote: Emotion.Laughter));
            ret.Add(GenFromString("I don't need your stupid Stardust!", emote: Emotion.Laughter));
            ret.Add(GenFromString("Good Luck!", emote: Emotion.Laughter));

            return ret;
        }

        private static List<CustomLine> MechanicAlchemyLine()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Woof.", emote: Emotion.Neutral));

            ret.Add(GenFromString("You want to learn about the owners of those modded cards you love so much?", emote: Emotion.Neutral));
            ret.Add(GenFromString("Well they're not a known quantity I'll tell you that.", emote: Emotion.Neutral));
            ret.Add(GenFromString("But they seem as savage as the original Scrybes.", emote: Emotion.Neutral));

            ret.Add(GenFromString("Take Niphox for example.", emote: Emotion.Neutral));
            ret.Add(GenFromString("The Scrybe of Alchemy.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Sick freak resides in a faraway island called the Misty Isles.", emote: Emotion.Neutral));
            ret.Add(GenFromString("All he does is run experiments to improve the results of his Stamping Machine.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Maybe Leshy is right not to trust those Alchemy cards.", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> MechanicStressLine()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("Stress?", emote: Emotion.Neutral));
            ret.Add(GenFromString("They belong to Phantomortis.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Or 'Morty'...", emote: Emotion.Neutral));
            ret.Add(GenFromString("I guess the guy wants to come off all soft and cuddly.", emote: Emotion.Neutral));
            ret.Add(GenFromString("At least Grimora seems to take the bait pretty well.", emote: Emotion.Neutral));

            ret.Add(GenFromString("But I've been around the block ever since you added him in...", emote: Emotion.Neutral));
            ret.Add(GenFromString("I know how he makes those cards.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Everyone he operates on is on the verge of suicide afterwards.", emote: Emotion.Neutral));
            ret.Add(GenFromString("We all thank you for that.", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> MechanicSpaceLine()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("And don't get me started on Constal.", emote: Emotion.Neutral));
            ret.Add(GenFromString("I honestly pity him.", emote: Emotion.Neutral));
            ret.Add(GenFromString("He was supposed to be the Scrybe of Technology before P03 showed up.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Then he got scrapped deep underwater.", emote: Emotion.Neutral));

            ret.Add(GenFromString("Perhaps we could've had a shot at a leader who isn't a nutcase.", emote: Emotion.Neutral));
            ret.Add(GenFromString("But I'm sure being considered P03's inferior would likey let loose a bolt or two.", emote: Emotion.Neutral));
            ret.Add(GenFromString("At least he makes the most of it with those Space Cards.", emote: Emotion.Neutral));

            return ret;
        }

        private static List<CustomLine> MechanicValorLine()
        {
            List<CustomLine> ret = new List<CustomLine>();
            ret.Add(GenFromString("But I think the best one might be Herilind.", emote: Emotion.Neutral));
            ret.Add(GenFromString("Might as well be more of a witch than Grimora herself.", emote: Emotion.Neutral));
            ret.Add(GenFromString("She's completely obsessed with you humans from what I've gathered.", emote: Emotion.Neutral));
            ret.Add(GenFromString("And all she does is study them mad and record their fate.", emote: Emotion.Neutral));

            ret.Add(GenFromString("There were actually rumors that Niphox allows her into his study.", emote: Emotion.Neutral));
            ret.Add(GenFromString("He uses her human anatomy to better his creations...", emote: Emotion.Neutral));
            ret.Add(GenFromString("In turn allowing her access to more resources.", emote: Emotion.Neutral));
            ret.Add(GenFromString("But maybe that hadn't been modded in yet.", emote: Emotion.Neutral));

            ret.Add(GenFromString("Maybe wait for an update or something.", emote: Emotion.Neutral));

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
