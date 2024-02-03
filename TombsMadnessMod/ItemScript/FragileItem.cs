﻿using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TombsMadnessMod.ItemScript
{
    public class FragileItem : Throwable
    {
        public AudioClip[] breakAudios;

        public GameObject itemFragments;

        public float noiseScale;

        public float noiseLoudness;

        public int itemDurability;

        [HideInInspector] public bool wasPlacedOnSellPlatform;



        private AudioSource audioSource;

        private float animationFraction;

        private float animationFractionLifetime;

        private int initialValue;

        private int initialDurability;

        private Animator animator;

        private bool itemBroken;

        public override void Start()
        {
            base.Start(); 
            audioSource = gameObject.GetComponent<AudioSource>();
            animator = gameObject.GetComponent<Animator>();
            animationFraction = 1f / itemDurability;

            initialDurability = itemDurability;
            initialValue = scrapValue;
        }



        public override void OnHitGround()
        {
            base.OnHitGround();

            if (!wasPlacedOnSellPlatform)
            {
                if (Tools.CheckIsServer()) { DamageItemClientRpc(); if (isThrown) { BreakItemClientRpc(); } }
                else { DamageRequestServerRpc(); if (isThrown) { BreakRequestServerRpc(); } }
            }
           

            if (itemDurability <= 0 && !wasPlacedOnSellPlatform && !itemBroken)
            {
                if (Tools.CheckIsServer())
                {
                    BreakItemClientRpc();
                }
                else { BreakRequestServerRpc(); }
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void DamageRequestServerRpc()
        {
            DamageItemClientRpc();
        }

        [ClientRpc]
        public void DamageItemClientRpc()
        {
            itemDurability -= 1;
            scrapValue = initialValue * (itemDurability / initialDurability);
            animationFractionLifetime += animationFraction;
            animator.SetFloat("animationFraction", animationFractionLifetime);
            NetworkLog.LogInfo("The Scrap value is " + scrapValue + "!");
        }




        [ServerRpc(RequireOwnership = false)]
        public void BreakRequestServerRpc()
        {
            BreakItemClientRpc();
        }

        [ClientRpc]
        public void BreakItemClientRpc()
        {
            itemBroken = true;
            mainObjectRenderer.enabled = false;
            itemFragments.gameObject.SetActive(true);
            audioSource.PlayOneShot(breakAudios[Random.Range(0, breakAudios.Length)]);
            roundManager.PlayAudibleNoise(transform.position, noiseScale, noiseLoudness, 1, noiseIsInsideClosedShip: false, 5);
            gameObject.GetComponent<BoxCollider>().enabled = false;
        }
    }

    public class PathedObject : MonoBehaviour
    {
        public AnimationCurve arcCurve;
        public float lerpDuration = 3f;
        public bool enableCollision = false;
        public LayerMask groundLayer;
        private float timeElapsed;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private bool isFrozen = false;
        void OnEnable()
        {
            startPosition = transform.position;
            endPosition = new Vector3(startPosition.x, arcCurve.Evaluate(1f), startPosition.z);
            timeElapsed = 0f;
        }
        void Update()
        {
            if (!isFrozen)
            {
                if (timeElapsed < lerpDuration)
                {
                    timeElapsed += Time.deltaTime;
                    float t = timeElapsed / lerpDuration;
                    Vector3 horizontalPosition = Vector3.Lerp(startPosition, endPosition, t);
                    float height = startPosition.y + arcCurve.Evaluate(t) * (endPosition.y - startPosition.y);
                    transform.position = new Vector3(horizontalPosition.x, height + 0.5f, horizontalPosition.z);
                    if (enableCollision)
                    {
                        CheckCollision();
                    }
                }
                else
                {
                    isFrozen = true;
                    transform.position = startPosition;
                }
            }
        }
        private void CheckCollision()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.025f, groundLayer))
            {
                isFrozen = true;
                transform.position = hit.point;
            }
        }
    }
}
