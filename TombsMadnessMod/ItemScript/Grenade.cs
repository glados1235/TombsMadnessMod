using DigitalRuby.ThunderAndLightning;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Unity.Netcode;
using UnityEngine;

namespace TombsMadnessMod.ItemScript
{
    public class Grenade : Throwable
    {

        // Public variables
        public AudioClip dudSFX;
        public GameObject scanNode;
        public bool shouldDestroyAfterUse;
        public bool hasExploded;
        public float TimeToExplode;
        public float killRange;
        public float damageRange;
        public bool hasRandomTimer;
        public float maxExplodeTimer;
        public float minExplodeTimer;
        public bool canDud;
        public bool shouldDud;
        public bool hasRolledDud;
        public int dudRate;

        // Private variables for internal use
        private Animator animator;
        private float explodeTimer = 0;
        private float currentTimerNormalized;
        private float randomTimeToExplode = 0;
        private float randomTimer = 0;

        public override void Start()
        {
            base.Start();
            animator = GetComponent<Animator>();
        }

        public override void Update()
        {
            base.Update();
            // Check if the dud chance has been rolled yet
            if (!hasRolledDud)
            {
                // If not, check if the game is running on the server
                // If it is, roll the dud chance
                if (Tools.CheckIsServer()) { RollDudClientRpc(UnityEngine.Random.Range(0, 100)); TombsMadnessModBase.mls.LogError($"Rolled a dud on the server with a value of: {shouldDud}"); }
            }

            // Check if the random time to explode has been set
            if (randomTimeToExplode == 0)
            {
                // If not, check if the game is running on the server
                // If it is, set the random time to explode
                if (Tools.CheckIsServer()) { InitiateRandomClientRpc(UnityEngine.Random.Range(minExplodeTimer, maxExplodeTimer)); TombsMadnessModBase.mls.LogError($"Set the Random Time to explode on the server to {randomTimeToExplode}"); }
            }

            // Check if the item is primed and has not exploded yet
            if (isPrimed && !hasExploded)
            {
                // If the scan node is active, deactivate it
                if (scanNode.activeSelf) { scanNode.SetActive(false); TombsMadnessModBase.mls.LogError("turned off the Scan Node"); }

                // Check if the "burning" animator parameter is false
                // If it is, set it to true
                if (!animator.GetBool("burning")) { animator.SetBool("burning", true); TombsMadnessModBase.mls.LogError($"Set the animator bool burning to true and it has a value of: {animator.GetBool("burning")}"); }

                // Check if the "hasRandomTimer" inspector setting is true
                if (hasRandomTimer)
                {
                    // Start a random countdown to explode
                    randomTimer += Time.deltaTime;

                    // Normalize the current timer to a 0 to 1 range for animation purposes
                    currentTimerNormalized = randomTimer / randomTimeToExplode;

                    //TombsMadnessModBase.mls.LogError($"randomTimer time passed: {randomTimer} || the currentTimerNormalized is: {currentTimerNormalized}");

                    // If the random timer exceeds the random time to explode, trigger a random timed explosion
                    if (randomTimer > randomTimeToExplode)
                    {
                        TombsMadnessModBase.mls.LogError("Triggering a random timed explosion!");

                        // Check if the game is running on the server
                        // If it is, trigger the explosion client RPC
                        // Otherwise, request the server to trigger the explosion
                        if (Tools.CheckIsServer()) { TriggerExplosionClientRpc(); } else { RequestTriggerExplosionServerRpc(); }
                    }
                }
                else
                {
                    // Start a fixed countdown to explode
                    explodeTimer += Time.deltaTime;

                    // Normalize the current timer to a 0 to 1 range based on the time to explode
                    currentTimerNormalized = explodeTimer / TimeToExplode;

                    // Log the amount of time passed and the normalized current timer
                    //TombsMadnessModBase.mls.LogError($"explodeTimer time passed: {explodeTimer} || the currentTimerNormalized is: {currentTimerNormalized}");

                    // If the explode timer exceeds the time to explode, trigger a fixed timed explosion
                    if (explodeTimer > TimeToExplode)
                    {
                        TombsMadnessModBase.mls.LogError("Triggering a fixed timed explosion!");

                        // Check if the game is running on the server
                        // If it is, trigger the explosion client RPC
                        // Otherwise, request the server to trigger the explosion
                        if (Tools.CheckIsServer()) { TriggerExplosionClientRpc(); } else { RequestTriggerExplosionServerRpc(); }
                    }
                }

                // Set the "currentTimerNormalized" animator parameter to the current timer normalized
                animator.SetFloat("currentTimerNormalized", currentTimerNormalized);
            }
        }

        public void TriggerDud()
        {

            TombsMadnessModBase.mls.LogError("Dud triggered!");

            // Play the dud sound effect
            audioSource.PlayOneShot(dudSFX);

            // Turn off the "burning" animator parameter
            animator.SetBool("burning", false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestTriggerExplosionServerRpc()
        {

            NetworkLog.LogError("ServerRequestTriggerExplosionRpc method called");

            // Call the TriggerExplosionClientRpc() method from the server
            TriggerExplosionClientRpc();
        }

        [ClientRpc]
        public void TriggerExplosionClientRpc()
        {

            TombsMadnessModBase.mls.LogError($"TriggerExplosionRpc method called, shouldDud: {shouldDud}");

            // Check if the "shouldDud" parameter is true
            if (shouldDud)
            {
                // If it is, trigger the dud and return without exploding
                TriggerDud();
                return;
            }

            // Create an explosion at the object's current position with the specified kill and damage ranges
            Landmine.SpawnExplosion(this.transform.position, true, killRange, damageRange);

            // Create a long-range audible noise
            roundManager.PlayAudibleNoise(transform.position, 25, 4.5f, 1, noiseIsInsideClosedShip: false, 5);

            // Mark the object as having exploded
            hasExploded = true;

            // Stop any audio playing on the audioSource
            audioSource.Stop();

            // Turn off the "burning" animator parameter
            animator.SetBool("burning", false);

            // Check if the object should be destroyed after use
            // If it should, destroy the object
            if (shouldDestroyAfterUse)
            {
                if (Tools.CheckIsServer()) { Destroy(this.gameObject); } else { RequestObjectDestroyServerRpc(); }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestObjectDestroyServerRpc()
        {
            // Destroy the object
            Destroy(this.gameObject);
        }

        [ClientRpc]
        public void InitiateRandomClientRpc(float time)
        {

            TombsMadnessModBase.mls.LogError($"InitiateRandomClientRpc method called, time: {time}");

            // Set the random time to explode
            randomTimeToExplode = time;
        }

        [ClientRpc]
        public void RollDudClientRpc(int dudRoll)
        {

            TombsMadnessModBase.mls.LogError($"RollDudClientRpc method called, dudRoll: {dudRoll}");

            // Check if the local int dudRoll is less or equal to the inspector set int dudRate
            // If it is, set shouldDud to true
            if (dudRoll <= dudRate) { shouldDud = true; }

            // Mark that the dud has been rolled
            hasRolledDud = true;
        }
    }
}
