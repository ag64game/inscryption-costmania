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
    public class AbilImpart : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
        {
            return true;
        }

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            yield return base.PreSuccessfulTriggerSequence();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} shares his wishom.", (GBC.TextBox.Style)Card.Info.temple);
            yield return new WaitForSeconds(0.2f);

            try { Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true).Card.AddValorRank(Card.ValorRank()); } catch { }
            try { Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false).Card.AddValorRank(Card.ValorRank()); } catch { }

            yield return LearnAbility(0.2f);
        }

        public static void AddImpart()
        {

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Impart",
                "When [creature] perishes, it adds it's Valor Rank to it's adjacent allies.",
                typeof(AbilImpart),
                "3d_impart.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_impart.png", typeof(CostmaniaPlugin).Assembly));
            info.SetPowerlevel(3);
            AbilImpart.ability = info.ability;
        }
    }
}
