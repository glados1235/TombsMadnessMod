using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TombsMadnessMod.Patches;
using UnityEngine;
using TombsMadnessMod.Component;
using Dissonance;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Unity.Netcode;
using System.Reflection;

namespace TombsMadnessMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]

    public class TombsMadnessModBase : BaseUnityPlugin
    {
        private const string modGUID = "TombVali.TombsMadnessMod";
        private const string modName = "Tomb's Madness Mod";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static KeyBinds KeyBindsInstance = new KeyBinds();

        public static TombsMadnessModBase Instance;

        public static ManualLogSource mls;

        public static GameObject assetLoader;

        public static GameObject ModdedUI;

        public static GameObject systemGO;

        void Awake()
        {
            SetupKeybindCallbacks();
            if (Instance == null)
            {
                Instance = this;
            }

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            mls = Logger;

            if (assetLoader == null)
            {
                assetLoader = new GameObject();
                assetLoader.name = "TombsMadnessModAssetLoader";
                DontDestroyOnLoad(assetLoader);
                assetLoader.hideFlags = HideFlags.HideAndDontSave;
                assetLoader.AddComponent<AssetLoader>();
            }




            AssetLoader assetLoaderComp = assetLoader.GetComponent<AssetLoader>();
            if (assetLoaderComp != null)
            {
                if (assetLoaderComp.assetsDictionary.TryGetValue("assets/tombsmadnessmodbundle/moddedui/moddeduicanvas.prefab", out UnityEngine.Object prefab))
                {

                    ModdedUI = (GameObject)prefab;
                }
            }



            mls.LogInfo("TOMB IN DA SHIZOUCE :)");
            harmony.PatchAll(typeof(TombsMadnessModBase));
            //harmony.PatchAll(typeof(PlayerControlerBPatch));
            harmony.PatchAll(typeof(StartOnPlayerControlerBeat));

        }
        public void SetupKeybindCallbacks()
        {
            KeyBindsInstance.SpawnKey.performed += OnSpawnKeyPressed;
        }

        public void OnSpawnKeyPressed(InputAction.CallbackContext explodeContext)
        {
            if (!explodeContext.performed) return;

            if (NetworkManager.Singleton.IsServer)
            {
                Ray ray = new Ray(StartOfRound.Instance.activeCamera.transform.position, StartOfRound.Instance.activeCamera.transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10000, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                {
                    GameObject prefab = assetLoader.GetComponent<AssetLoader>().GetAsset("assets/tombsmadnessmodbundle/beartrap/beartrap.prefab");
                    if (prefab != null)
                    {
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.down, hit.normal) * Quaternion.Euler(90f, 0f, 0f);
                        GameObject spawnedObject = Instantiate(prefab, hit.point, rotation);
                        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();
                        if (networkObject != null)
                        {
                            
                            networkObject.Spawn();   
                        }
                        else
                        {
                            Debug.LogError("The prefab does not have a NetworkObject component!");
                        }
                    }
                    else
                    {
                        Debug.LogError("Prefab not found in AssetLoader!");
                    }
                }
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                mls.LogWarning("your trying to debug spawn on the non host client!");
            }
        }




    }

}
