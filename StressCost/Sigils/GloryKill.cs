using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using StressCost.Cost;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Sigils
{
    public class AbilGloryKill : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => killer == Card;

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            yield return base.PreSuccessfulTriggerSequence();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName}'s vigor rises with gusto!", (GBC.TextBox.Style)Card.Info.temple);
            yield return new WaitForSeconds(0.2f);

            Card.AddValorRank();

            Card.Anim.StrongNegationEffect();
            yield return LearnAbility(0.2f);
        }

        public static void AddGloryKill()
        {

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Glory Kill",
                "When [creature] strikes an opposing creature and it perishes, this card's Valor Rank goes up by 1.",
                typeof(AbilGloryKill),
                "3d_glorykill.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_glorykill.png", typeof(CostmaniaPlugin).Assembly));
            info.SetPowerlevel(2);
            AbilGloryKill.ability = info.ability;
        }
    }
}
