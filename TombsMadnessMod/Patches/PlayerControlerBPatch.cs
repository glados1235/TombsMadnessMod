using HarmonyLib;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombsMadnessMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControlerBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void godModeTestPatch(ref int ___health, ref float ___sprintMeter, ref float ___sinkingSpeedMultiplier, ref float ___sprintMultiplier)
        {
            ___health = 5000;
            ___sprintMeter = 5000;
            ___sinkingSpeedMultiplier = 10;
            ___sprintMultiplier = 5;
        }

    }
}
