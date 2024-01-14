using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace TombsMadnessMod.MapItems
{
   internal class BearTrap : NetworkBehaviour
   {
       private bool hasTriggered;
       private Animator animator;
       private AudioSource audioSource;

       public AudioClip trapCloseSFX;

       public void Start()
       {
           animator = GetComponent<Animator>();
           audioSource = GetComponent<AudioSource>();
       }
       
       public void OnTriggerEnter(Collider other)
       {
           if (hasTriggered)
           {
               return;
           }

           if (other.CompareTag("Player"))
           {
               PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
               if (!(component != GameNetworkManager.Instance.localPlayerController) && component != null && !component.isPlayerDead)
               {
                  component.DamagePlayer(80, true, true, CauseOfDeath.Unknown);
                  component.movementSpeed = 0.4f;
                  TriggerTrapServerRpc();
               }
           }
       }

       public void OnTriggerExit(Collider other)
       {
           if (other.CompareTag("Player"))
           {
               PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
               if (!(component != GameNetworkManager.Instance.localPlayerController) && component != null && !component.isPlayerDead)
               {
                  animator.SetBool("IsTriggerd", false);
                  component.movementSpeed = 4.6f;
               }
           }
       }

       [ServerRpc(RequireOwnership = false)]
       public void TriggerTrapServerRpc(ServerRpcParams rpcParams = default)
       {
           foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
           {
               TriggerTrapClientRpc();
           }
       }
         
       [ClientRpc]
       public void TriggerTrapClientRpc(ClientRpcParams rpcParams = default)
       {
           audioSource.PlayOneShot(trapCloseSFX);
           animator.SetBool("IsTriggerd", true);
           hasTriggered = true;
       }
   }
}
