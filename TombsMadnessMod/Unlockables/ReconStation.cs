using System;
using System.Collections.Generic;
using System.Text;
using TombsMadnessMod.ItemScript;
using UnityEngine;
 
namespace TombsMadnessMod.Unlockables
{
    public class ReconStation : MonoBehaviour
    {
        public static readonly List<RangedMicrophone> rangedMicrophones = new List<RangedMicrophone>();
        public int tuneIndex;
        private bool enabled;

        public void PowerButtonPress()
        {
            enabled = !enabled;
            if (!enabled)
            {
                foreach (var rangedMics in rangedMicrophones) { rangedMics.StopRecording(); }
            }
            else
            {
                if (rangedMicrophones.Count > 0) { rangedMicrophones[tuneIndex].StartRecording(); }
                
            }

            TombsMadnessModBase.mls.LogWarning("the Recon Station have been turned on!");
        }

        public void SwitchChannelForwordButton()
        {
            if(enabled)
            {
                if (rangedMicrophones.Count > 1 && tuneIndex < rangedMicrophones.Count - 1)
                {
                    rangedMicrophones[tuneIndex].StopRecording();
                    tuneIndex++;
                    rangedMicrophones[tuneIndex].StartRecording();
                    TombsMadnessModBase.mls.LogWarning("Shifting to the next mic in the list with an index of " + tuneIndex);
                }
                TombsMadnessModBase.mls.LogWarning("looks like the forword button was pressed");
            }
        }
        public void SwitchChannelBackButton()
        {
            if (enabled)
            {
                if (rangedMicrophones.Count > 1 && tuneIndex > 0)
                {
                    rangedMicrophones[tuneIndex].StopRecording();
                    tuneIndex--;
                    rangedMicrophones[tuneIndex].StartRecording();
                    TombsMadnessModBase.mls.LogWarning("Shifting to the last mic in the list with an index of " + tuneIndex);
                }
                TombsMadnessModBase.mls.LogWarning("looks like the back button was pressed");
            }
        }

    }
}

