using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace TombsMadnessMod.ItemScript
{
    public class DistractionDevice : Throwable
    { 
        public AudioClip beepAudio;

        public AudioSource audioSource;

        public float timer;

        private float timerCount;

        private bool isUsed;

        private Animator animator; 

        private int timesPlayedWithoutTurningOff;

        private float noiseInterval;

        public override void Start()
        {
            base.Start();
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            animator = GetComponent<Animator>();
        }
        public override void Update()
        {
            base.Update(); 
            if (isThrown && !isUsed)
            {
                timerCount += Time.deltaTime;
                if (timerCount > timer)
                {
                    isBeingUsed = true;
                    animator.SetBool("IsActive", true);
                    if (!audioSource.isPlaying) { audioSource.Play(); }
                    if (insertedBattery.charge < 0.08f)
                    {
                        audioSource.pitch = 1f - (0.05f - insertedBattery.charge) * 4f;
                        if (animator.GetFloat("FlashSpeed") == 0f) { animator.SetFloat("FlashSpeed", 0); }
                        else { animator.SetFloat("FlashSpeed", -0.05f); }

                    }
                    if (noiseInterval <= 0f)
                    {  
                        noiseInterval = 1f;
                        timesPlayedWithoutTurningOff++;
                        roundManager.PlayAudibleNoise(transform.position, 16f, 0.9f, timesPlayedWithoutTurningOff, noiseIsInsideClosedShip: false, 5);
                    }
                    else
                    { 
                        noiseInterval -= Time.deltaTime;
                    }
                }
            }
        }



        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            audioSource.Stop();
            animator.SetBool("IsActive", false);
            isUsed = true;
            isBeingUsed = false;
        }

    }
}
