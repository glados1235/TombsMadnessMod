using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TombsMadnessMod.MapItems
{
    public class NoiseHazard : NetworkBehaviour
    {
        private RoundManager roundManager;
        private Animator animator;
        private AudioSource audioSource;
        private bool triggerd;
        public bool oneUse;
        public AudioClip[] triggerSFX;

        public void Start()
        {
            roundManager = FindObjectOfType<RoundManager>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            TombsMadnessModBase.mls.LogError("Watch your toes! could be some broken glass around");
        }

        public void OnTriggerEnter(Collider collider)
        {
            if(oneUse && triggerd) { return; }
            if (collider.CompareTag("Player"))
            {
                PlayerControllerB component = collider.gameObject.GetComponent<PlayerControllerB>();
                if (!(component != GameNetworkManager.Instance.localPlayerController) && component != null && !component.isPlayerDead)
                {
                    TriggerSFXServerRpc();
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TriggerSFXServerRpc(ServerRpcParams rpcParams = default)
        {
            foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                TriggerSFXClientRpc();
            }
        }
        [ClientRpc]
        public void TriggerSFXClientRpc(ClientRpcParams rpcParams = default)
        {
            audioSource.PlayOneShot(triggerSFX[Random.Range(0, triggerSFX.Length)]);
            roundManager.PlayAudibleNoise(transform.position, 16f, 1f, 1, noiseIsInsideClosedShip: false, 5);
            if(animator != null) { animator.SetBool("Triggerd", true); }
            triggerd = true;
        }

    }
}
