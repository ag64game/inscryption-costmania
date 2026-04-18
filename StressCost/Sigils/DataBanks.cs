using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GBC;
namespace StressCost.Sigils
{
    public class AbilDataBanks : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public static void AddDataBanks()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Data Banks",
                "While [creature] is on the board, it's owner loses 2 Stardust instead of all of them at the start of their turn.",
                typeof(AbilDataBanks),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_databanks.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_databanks.png", typeof(CostmaniaPlugin).Assembly));
            info.SetPowerlevel(4);
            AbilDataBanks.ability = info.ability;
        }
    }
}
