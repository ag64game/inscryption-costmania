using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
namespace StressCost.Sigils
{
    public class AbilEndOfTheTunnel : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public static bool dramaqueenTextPlayed = false;

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == !Card.OpponentCard;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return PreSuccessfulTriggerSequence();
            base.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.2f);
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"The merciless forces of gravity push further", (GBC.TextBox.Style)Card.Info.temple);
            try
            {
                foreach (CardSlot target in Singleton<BoardManager>.Instance.allSlots.Where(slot => slot.Card != null && slot.Card.OpponentCard != Card.OpponentCard).OrderBy(slot => Math.Abs(slot.Index - Card.Slot.Index)))
                {
                    Console.WriteLine(target == null);
                    CardSlot destination;

                    if (target.Index > Card.Slot.Index) destination = Singleton<BoardManager>.Instance.GetAdjacent(target, true);
                    else destination = Singleton<BoardManager>.Instance.GetAdjacent(target, false);

                    if (destination != null) yield return Singleton<BoardManager>.Instance.AssignCardToSlot(target.Card, destination, 0.1f, null, true);
                    else yield return target.Card.Die(false, Card);
                }
            }
            finally { }

            yield return LearnAbility(0.2f);
        }

        public static void AddEndOfTheTunnel()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "End Of The Tunnel",
                "After ringing the bell, all opposing creatures move one space towerds [creature], perishing should they be within their grasp.",
                typeof(AbilEndOfTheTunnel),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_endoftheline.png");

            info.SetPowerlevel(5);
            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_endoftheline.png", typeof(CostmaniaPlugin).Assembly));
            AbilEndOfTheTunnel.ability = info.ability;
        }
    }
}
