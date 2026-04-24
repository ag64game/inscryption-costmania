using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Sigils
{
    public class AbilRelaxant : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();

            yield return new WaitForSeconds(0.2f);
            ViewManager.Instance.SwitchToView(View.Default);

            Card.Anim.StrongNegationEffect();
            if (Cost.StressCost.stressCounter > 0)
            {
                Cost.StressCost.stressCounter -= 1;
                AudioController.Instance.PlaySound2D("plainBlip6", volume: 0.6f);
            }

            yield return LearnAbility(0.2f);
        }

        public static void AddRelaxant()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Relaxant",
                "[creature] lowers the Stress Counter by 1 when placed.",
                typeof(AbilRelaxant),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_relaxant.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_relaxant.png", typeof(CostmaniaPlugin).Assembly));
            info.SetPowerlevel(1);
            AbilRelaxant.ability = info.ability;
        }
    }
}
