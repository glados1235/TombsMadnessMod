using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine.InputSystem;

namespace TombsMadnessMod
{
    public class KeyBinds : LcInputActions
    {
        [InputAction("<Keyboard>/v", Name = "Spawn")]
        public InputAction SpawnKey { get; set; }
    }
}
