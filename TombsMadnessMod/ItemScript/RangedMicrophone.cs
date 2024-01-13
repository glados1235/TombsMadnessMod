using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TombsMadnessMod.Unlockables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TombsMadnessMod.ItemScript
{
    public class RangedMicrophone : GrabbableObject
    {
        private ModdedUI m_UI;
        public bool isPowerd;
        public bool tunedInto;
        public float micRange;

        private AudioSource audioSource;

        private AudioListener playerAudioListener;
        private AudioListener micAudioListener;

        public AudioClip[] stopTransmissionSFX;
         
        public AudioClip[] startTransmissionSFX;
         
        public AudioClip clickON;


        public void Awake()
        {
            TombsMadnessModBase.mls.LogInfo("RangedMicrophone Awake");
            m_UI = TombsMadnessModBase.ModdedUI.GetComponent<ModdedUI>();
            ReconStation.rangedMicrophones.Add(this);
        }
        public override void Start()
        {
            base.Start();
            playerAudioListener = StartOfRound.Instance.localPlayerController.activeAudioListener;
            audioSource = GetComponent<AudioSource>();
            micAudioListener = this.GetComponent<AudioListener>();
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            ReconStation.rangedMicrophones.Remove(this);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            SwitchOn(used);
            audioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
            audioSource.PlayOneShot(clickON);
        }

        public void SwitchOn(bool enabled)
        {
            isBeingUsed = enabled;
        }

        public void StartRecording()
        {
            TombsMadnessModBase.mls.LogWarning(this.name + " has started recording!");
            playerAudioListener.enabled = false;
            micAudioListener.enabled = true;
        }

        public void StopRecording()
        {
            TombsMadnessModBase.mls.LogWarning(this.name + " has Stopped recording!");
            playerAudioListener.enabled = true;
            micAudioListener.enabled = false;
        }

    }
}
