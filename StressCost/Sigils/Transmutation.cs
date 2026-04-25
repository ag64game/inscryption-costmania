using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using StressCost.Cost;
using StressCost.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StressCost.Sigils
{
    public class AbilTransmutation : ActivatedAbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override int EnergyCost => 2;
        private bool writeText = true;

        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return playerUpkeep;
        }

        public override System.Collections.IEnumerator Activate()
        {
            yield return PreSuccessfulTriggerSequence();

            yield return AlchemyCounter.WaitUntilClick(Reroll());

            yield return LearnAbility(0.2f);
            yield break;
        }

        public IEnumerator Reroll()
        {
            if(writeText) yield return Singleton<TextBox>.Instance.ShowUntilInput($"TRANSMUTATION!", (GBC.TextBox.Style)Card.Info.temple, shake: true);
            base.Card.Anim.StrongNegationEffect();
            AlchemyCounter.RollDies(AlchemyCounter.lastClicked);
            yield return writeText = false;
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return writeText = true;
            yield return base.OnUpkeep(playerUpkeep);
        }

        public static void AddTransmutation()
        {
            const string rulebookDescription = "Pay 2 Energy to reroll a chosen Alchemy Die, provided it is not locked.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Transmutation",
                rulebookDescription,
                typeof(AbilTransmutation),
                "3d_transmutation.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_transmutation.png", typeof(CostmaniaPlugin).Assembly));
            info.SetActivated(true);
            info.SetPowerlevel(2);
            AbilTransmutation.ability = info.ability;
        }
    }
}
