using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace TombsMadnessMod
{
    public class Tools : MonoBehaviour
    {
        public static bool CheckIsServer()
        {
            if (NetworkManager.Singleton.IsServer) { return true; }
            return false;
        }


    }

}
