using LethalLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace TombsMadnessMod.ItemScript
{
    public class Throwable : GrabbableObject
    {
        // Public variables
        public AudioClip primeSFX;
        public AnimationCurve fallCurve;
        public AnimationCurve verticalFallCurve;
        public AnimationCurve verticalFallCurveNoBounce;
        public bool instaThrow;
        public bool canThrowWhenNotCharged;
        [HideInInspector]
        public bool isThrown = false;
        [HideInInspector]
        public bool isPrimed = false;
        [HideInInspector]
        public RoundManager roundManager;
        [HideInInspector]
        public AudioSource audioSource;

        // Private variables for internal use
        private Ray ThrowRay;
        private RaycastHit hit;

        public override void Start()
        {
            base.Start();
            audioSource = GetComponent<AudioSource>();
            roundManager = FindObjectOfType<RoundManager>();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            // Check if the item can be thrown without being charged and if the battery charge is 0. If so, exit the function
            if (!canThrowWhenNotCharged && insertedBattery.charge <= 0) { return; }
            if (!IsOwner) { return; }
            // Check if the item is primed or if it should be thrown instantly
            if (isPrimed || instaThrow)
            {
                // Discard the held object and place it at the calculated throw destination
                playerHeldBy.DiscardHeldObject(placeObject: true, null, GetThrowDestination(), false);

                // Mark the item as used up and set it as thrown
                itemUsedUp = true;
                isThrown = true;
            }

            // If the item is not thrown instantly and has not been primed yet, check if the game is running on the server or client
            // Then, call the appropriate RPC to prime the throw
            if (!instaThrow && !isPrimed)
            {
                // Set the item as primed and play the prime sound effect
                isPrimed = true;
                audioSource.PlayOneShot(primeSFX);
            }
        }

        public Vector3 GetThrowDestination()
        {
            // Get the current position of the object
            Vector3 position = transform.position;

            // Define the ray from the camera through the object
            ThrowRay = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);

            // Check if the ray hits something within a distance of 12 units
            // If it doesn't, set the position to a point 10 units away along the ray
            // Otherwise, set the position to a point just before the hit point
            position = !Physics.Raycast(ThrowRay, out hit, 12f, StartOfRound.Instance.collidersAndRoomMaskAndDefault)
                ? ThrowRay.GetPoint(10f) : ThrowRay.GetPoint(hit.distance - 0.05f);

            // Define a new ray from the calculated position downwards
            ThrowRay = new Ray(position, Vector3.down);

            // Check if the new ray hits something within a distance of 30 units
            // If it does, return the hit point slightly above the ground
            // Otherwise, return a point 30 units down along the ray
            if (Physics.Raycast(ThrowRay, out hit, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                return hit.point + Vector3.up * 0.05f;
            }

            return ThrowRay.GetPoint(30f);
        }

        public override void FallWithCurve()
        {
            // Calculate the distance between the starting and target positions
            float magnitude = (startFallingPosition - targetFloorPosition).magnitude;

            // Interpolate the rotation of the object towards the resting rotation over time
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(itemProperties.restingRotation.x, transform.eulerAngles.y, itemProperties.restingRotation.z), 14f * Time.deltaTime / magnitude);

            // Interpolate the local position of the object towards the target floor position over time
            transform.localPosition = Vector3.Lerp(startFallingPosition, targetFloorPosition, fallCurve.Evaluate(fallTime));

            // If the distance is greater than 5 units, interpolate the y-coordinate separately
            if (magnitude > 5f)
            {
                transform.localPosition = Vector3.Lerp(new Vector3(transform.localPosition.x, startFallingPosition.y, transform.localPosition.z), new Vector3(transform.localPosition.x, targetFloorPosition.y, transform.localPosition.z), verticalFallCurveNoBounce.Evaluate(fallTime));
            }
            else
            {
                transform.localPosition = Vector3.Lerp(new Vector3(transform.localPosition.x, startFallingPosition.y, transform.localPosition.z), new Vector3(transform.localPosition.x, targetFloorPosition.y, transform.localPosition.z), verticalFallCurve.Evaluate(fallTime));
            }

            // Update the fall time
            fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
        }


    }
}
