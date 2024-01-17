using DigitalRuby.ThunderAndLightning;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TombsMadnessMod.ItemScript
{
    public class Grenade : Throwable
    {
        private AudioSource audioSource;

        public AudioClip dudSFX;
        public AudioClip activateSFX;
        public GameObject scanNode;
        public bool shouldDestroyAfterUse;
        public bool hasExploded;
        public float TimeToExplode;
        private float explodeTimer = 0;
        public float killRange;
        public float damageRange;
        public bool hasRandomTimer;
        public float maxExplodeTimer;
        public float minExplodeTimer;

        public bool canDud;
        public bool shouldDud;
        public bool hasRolledDud;
        public int dudRate;

        private Animator animator;

        private float currentTimerNormalized;

        private float randomTimeToExplode;
        private float randomTimer = 0;

         
        public override void Start()
        {
            base.Start();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
        }

        public override void Update()
        { 

            base.Update();

            if (!hasRolledDud)
            {
                hasRolledDud = true; 
                if (UnityEngine.Random.Range(0, 100) <= dudRate)
                {
                    shouldDud = true;
                }
            }

            if (randomTimeToExplode == 0) 
            {
                randomTimeToExplode = UnityEngine.Random.Range(minExplodeTimer, maxExplodeTimer);
            }

            if (isThrown && !hasExploded)
            {
                if (scanNode != null) { scanNode.active = false; }
                if (!animator.GetBool("burning")) { animator.SetBool("burning", true); }
                if (hasRandomTimer)
                {
                    randomTimer += Time.deltaTime;
                    currentTimerNormalized = randomTimer / randomTimeToExplode;
                    if (randomTimer > randomTimeToExplode)
                    {
                        TriggerExplotion();
                    }
                }
                else
                {
                    explodeTimer += Time.deltaTime;
                    currentTimerNormalized = explodeTimer / TimeToExplode; 
                    if (explodeTimer > TimeToExplode)
                    {
                        TriggerExplotion();
                    }
                }
                animator.SetFloat("currentTimerNormalized", currentTimerNormalized);
            }
        }

        public void TriggerDud()
        {
            audioSource.PlayOneShot(dudSFX);
            audioSource.Stop();
            animator.SetBool("burning", false);
        }
        public void TriggerExplotion()
        {
            if (shouldDud)
            {
                TriggerDud();
                return;
            }
            Landmine.SpawnExplosion(this.transform.position, true, killRange, damageRange);
            roundManager.PlayAudibleNoise(transform.position, 30, 5, 1, noiseIsInsideClosedShip: false, 5);
            hasExploded = true;
            audioSource.Stop();
            animator.SetBool("burning", false);
            if (shouldDestroyAfterUse) { Destroy(this.gameObject); }
        }


        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if(!itemUsedUp) { audioSource.PlayOneShot(activateSFX); }
            
        }


    }
}
