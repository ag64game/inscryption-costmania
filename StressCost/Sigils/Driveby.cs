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
    public class AbilDriveby : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public static void AddDriveby()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Driveby",
                "[creature] deals 1 damage to the creature opposing it when moving.",
                typeof(AbilDriveby),
                "3d_driveby.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_driveby.png", typeof(CostmaniaPlugin).Assembly));
            info.SetPowerlevel(1);
            AbilDriveby.ability = info.ability;
        }
    }
}
