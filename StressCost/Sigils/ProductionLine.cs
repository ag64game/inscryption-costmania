using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace StressCost.Sigils
{
    public class AbilProductionLine : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        private string[] cards =
        {
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_Homonculus",
            "Alchemy_GreaterHomonculus"
        };

        public override bool RespondsToTurnEnd(bool playerTurnEnd)
        {
            return playerTurnEnd && 
                (Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true).Card == null || Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false).Card == null);
        }

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            CardSlot old = Card.Slot;
            yield return base.PreSuccessfulTriggerSequence();
            
            CardSlot left = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true);
            CardSlot right = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false);

            if (right.Card == null) yield return DoMovement(right, false);
            else if (left.Card == null) yield return DoMovement(left, true);

            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName}'s pipeline triggers", (GBC.TextBox.Style)Card.Info.temple);
            yield return old.CreateCardInSlot(CardLoader.GetCardByName(cards[UnityEngine.Random.Range(0, cards.Length)]));

            yield return base.LearnAbility(0.1f);
        }

        private IEnumerator DoMovement(CardSlot destination, bool isLeft)
        {
            base.Card.RenderInfo.flippedPortrait = (isLeft && base.Card.Info.flipPortraitForStrafe);
            base.Card.RenderCard();

            yield return Singleton<BoardManager>.Instance.AssignCardToSlot(base.Card, destination, 0.1f, null, true);

            yield return new WaitForSeconds(0.25f);
        }

        public static void AddProductionLine()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Production Line",
                "At the end of the turn, [creature] will move in the direction inscrybed in the sigil, and will drop a Homonculus in it's place.",
                typeof(AbilProductionLine),
                "3d_productionline.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_productionline.png", typeof(CostmaniaPlugin).Assembly));
            info.SetPowerlevel(5);
            AbilProductionLine.ability = info.ability;
        }
    }
}
