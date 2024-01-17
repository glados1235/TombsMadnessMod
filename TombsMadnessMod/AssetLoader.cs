using HarmonyLib;
using LethalLib.Modules;
using System.Collections.Generic;
using TombsMadnessMod.Tags;
using Unity.Netcode;
using UnityEngine;
using static LethalLib.Modules.Items;
using static LethalLib.Modules.Levels;
using ScrapItem = TombsMadnessMod.Tags.ScrapItem;

namespace TombsMadnessMod.Component
{
    public class AssetLoader : MonoBehaviour
    {
        
        public StartOfRound SoR; 
          

        public Dictionary<string, UnityEngine.Object> assetsDictionary = new Dictionary<string, UnityEngine.Object>();

         
        public void Awake()   
        {
            
            var bundle = AssetBundle.LoadFromMemory(TombsMadnessMod.Properties.Resources.tombsmadnessmodbundle);
            foreach (var assetName in bundle.GetAllAssetNames())
            {
                GameObject asset = bundle.LoadAsset<GameObject>(assetName); 
                if (asset != null)
                { 
                    assetsDictionary[assetName] = asset;
                    if(asset.GetComponent<ScrapItem>() is ScrapItem i && i != null)
                    {
                        if (i.regScrap) { RegisterScrap(i.itemRef, i.rarity, i.levelTypes); }
                        if (i.regShop) { RegisterShopItem(i.itemRef, i.cost); }
                        Destroy(i);  
                    }

                    if (asset.GetComponent<ShipItem>() is ShipItem u && u != null)
                    {
                        LethalLib.Modules.Unlockables.RegisterUnlockable(u.unlockableRef, u.cost, u.storeType);
                        Destroy(u);
                    }
                    if (asset.GetComponent<MapItem>() is MapItem m && m != null)
                    { 
                        LethalLib.Modules.MapObjects.RegisterMapObject(m.spawnableMapObject, m.levelTypes, null);
                        Destroy(m);
                    }
                    if (asset.GetComponent<NetworkObject>() is NetworkObject obj && obj != null) 
                    {
                        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(asset);
                    }
                }
                
            }
            foreach (var item in assetsDictionary)
            {
                TombsMadnessModBase.mls.LogFatal($"Key: {item.Key}, Value: {item.Value}");
            }
        }

        public GameObject GetAsset(string assetName)
        {
            if (assetsDictionary.TryGetValue(assetName, out UnityEngine.Object asset))
            {
                return asset as GameObject;
            }
            else
            {
                TombsMadnessModBase.mls.LogFatal($"Asset '{assetName}' not found in AssetLoader.");
                return null;
            }
        }

    }


}
