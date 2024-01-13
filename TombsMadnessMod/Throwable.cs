using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TombsMadnessMod.Resources
{
    public class Throwable : GrabbableObject
    { 
        private Ray grenadeThrowRay;

        private RaycastHit hit;
        public bool canThrowWhenNotCharged;

        [HideInInspector]
        public bool isThrown = false;

        public AnimationCurve fallCurve;

        public AnimationCurve verticalFallCurve; 

        public AnimationCurve verticalFallCurveNoBounce;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if(!canThrowWhenNotCharged && insertedBattery.charge <= 0)
            {
                return;
            }
            if (isThrown)
            {
                playerHeldBy.DiscardHeldObject(placeObject: true, null, GetThrowDestination());
                itemUsedUp = true;
            }

            isThrown = true;

        }


        public Vector3 GetThrowDestination() 
        { 
            Vector3 position = transform.position;

            grenadeThrowRay = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);

            position = (!Physics.Raycast(grenadeThrowRay, out hit, 12f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                ? grenadeThrowRay.GetPoint(10f) : grenadeThrowRay.GetPoint((hit.distance - 0.05f));

            grenadeThrowRay = new Ray(position, Vector3.down);

            if (Physics.Raycast(grenadeThrowRay, out hit, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                return hit.point + Vector3.up * 0.05f;
            }

            return grenadeThrowRay.GetPoint(30f);
        }


        public override void FallWithCurve()
        {
            float magnitude = (startFallingPosition - targetFloorPosition).magnitude;

            base.transform.rotation = Quaternion.Lerp(base.transform.rotation, 
                Quaternion.Euler(itemProperties.restingRotation.x, base.transform.eulerAngles.y, itemProperties.restingRotation.z), 14f * Time.deltaTime / magnitude);

            base.transform.localPosition = Vector3.Lerp(startFallingPosition, targetFloorPosition, fallCurve.Evaluate(fallTime));

            if (magnitude > 5f)
            {
                base.transform.localPosition = Vector3.Lerp(new Vector3(base.transform.localPosition.x, startFallingPosition.y, base.transform.localPosition.z), new Vector3(base.transform.localPosition.x, targetFloorPosition.y, base.transform.localPosition.z), verticalFallCurveNoBounce.Evaluate(fallTime));
            }
            else
            { 
                base.transform.localPosition = Vector3.Lerp(new Vector3(base.transform.localPosition.x, startFallingPosition.y, base.transform.localPosition.z), new Vector3(base.transform.localPosition.x, targetFloorPosition.y, base.transform.localPosition.z), verticalFallCurve.Evaluate(fallTime));
            }

            fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
        }

    }
}
