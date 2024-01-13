using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static LethalLib.Modules.Levels;

namespace TombsMadnessMod.Tags
{
    public class MapItem : MonoBehaviour
    {
        public LevelTypes levelTypes;
        public int rarity;
        public SpawnableMapObject spawnableMapObject;
    }
}
