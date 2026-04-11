using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GBC;
using InscryptionAPI.Encounters;
using System.Linq;
using InscryptionAPI.Helpers.Extensions;
namespace StressCost.Sigils
{
    public class AbilWatchman : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        private bool textDisplayed = false;
        private int spiedCount = 0;

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            return otherCard.OpponentCard != base.Card.OpponentCard;
        }
        public override bool RespondsToAttackEnded()
        {
            return Card.OpponentCard && spiedCount > 0 && !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("tvflabs.inscryption.MultiplayerMod");
        }
        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return true;
        }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            yield return base.PreSuccessfulTriggerSequence();

            if (!textDisplayed)
            {
                textDisplayed = true;
                yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} spied on incoming {otherCard.Info.displayedName}!", TextBox.Style.Neutral);
            }

            base.Card.Anim.StrongNegationEffect();

            if (Card.OpponentCard)
            {
                spiedCount++;
            } else
            {
                yield return Singleton<CardDrawPiles>.Instance.DrawCardFromDeck(null, null);
            }
                yield return base.LearnAbility(0.1f);
        }

        public override IEnumerator OnAttackEnded()
        {
            if (Card.OpponentCard)
            {
                List<CardSlot> slots = Singleton<BoardManager>.Instance.opponentSlots.Where(slot => Singleton<BoardManager>.Instance.GetCardQueuedForSlot(slot) == null).ToList();
                List<PlayableCard> cards = Singleton<BoardManager>.Instance.GetOpponentCards();

                for (int i = 0; i < spiedCount / 2; i++)
                {
                    CardInfo newCard = CardLoader.GetCardByName(cards[UnityEngine.Random.Range(0, cards.Count)].Info.name);


                    if (slots.Count > 0 && spiedCount > 1)
                    {
                        PlayableCard playableCard = CardSpawner.SpawnPlayableCard(newCard);
                        playableCard.SetIsOpponentCard(true);
                        Singleton<TurnManager>.Instance.Opponent.ModifyQueuedCard(playableCard);

                        Singleton<BoardManager>.Instance.QueueCardForSlot(playableCard, slots[UnityEngine.Random.Range(0, slots.Count)]);
                        Singleton<TurnManager>.Instance.Opponent.Queue.Add(playableCard);
                        spiedCount /= 2;
                    }
                }
                

            }

            return base.OnAttackEnded();
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return base.PreSuccessfulTriggerSequence();
            
            if(!Card.OpponentCard && spiedCount > 0)
            {
                for (int i = 0; i < spiedCount; i++) yield return Singleton<CardDrawPiles>.Instance.DrawCardFromDeck(null, null);
                spiedCount = 0;
            }

            textDisplayed = false;

            yield return base.LearnAbility(0.1f);
        }

        public static void AddWatchman()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Watchman",
                "While [creature] is on the board, it's owner draws a card every time an opponent enters play.",
                typeof(AbilWatchman),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_watchman.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_watchman.png", typeof(CostmaniaPlugin).Assembly));
            AbilWatchman.ability = info.ability;
        }
    }
}
