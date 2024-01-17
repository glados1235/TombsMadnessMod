using HarmonyLib;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using TombsMadnessMod.Component;
using Unity.Netcode;

namespace TombsMadnessMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class StartOnPlayerControlerBeat
    {
        public static GameObject throwable;
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void throwableSpawnPatch(PlayerControllerB __instance)
        {

            //UnityEngine.Object.Instantiate(AssetLoader.Instance.light,
                //__instance.transform.position, Quaternion.identity, __instance.transform);
        }

    }
}
