using UnityEngine;
using GorillaLocomotion;
using GorillaTag;       // For VRRig and potentially SnowballThrowable if it's in this namespace
using System.Collections.Generic; // For List
using System.Linq; // For ToList()

// Ensure you have the correct 'using' statement for SnowballThrowable if it's not global.
// This might be something like 'using YourGameProject.Projectiles;' or it might be part of GorillaTag.
// For this example, I'm assuming SnowballThrowable is a known type.
// If you get a "type or namespace name 'SnowballThrowable' could not be found" error,
// you'll need to add the correct 'using' statement for it or ensure it's defined.

namespace TextUITemplate.Management
{
    public static class Main
    {
        // This list will hold snowballs considered "yours" (held or in holdable slots)
        // It will be updated by RefreshHeldSnowballs().
        public static List<SnowballThrowable> HELD_SNOWBALLS = new List<SnowballThrowable>();

        // Cache for already processed snowballs in a frame to avoid redundant GetFullPath calls
        // when using the parent path checking method.
        private static HashSet<SnowballThrowable> processedThisFrameForPathChecking = new HashSet<SnowballThrowable>();


        // --- TrueLeftHand and TrueRightHand methods ---
        // These are used by WallWalkMod and potentially others.
        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            if (GorillaTagger.Instance == null || GTPlayer.Instance == null)
            {
                // It's better to log an error or warning if this happens frequently during gameplay
                // Debug.LogError("Main.TrueLeftHand: GorillaTagger.Instance or GTPlayer.Instance is null!");
                return (Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero, Vector3.zero);
            }

            Quaternion handRotation = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.leftHandRotOffset;
            Vector3 handPosition = GorillaTagger.Instance.leftHandTransform.position +
                                   (GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.leftHandOffset);

            return (
                position: handPosition,
                rotation: handRotation,
                up: handRotation * Vector3.up,
                forward: handRotation * Vector3.forward,
                right: handRotation * Vector3.right
            );
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            if (GorillaTagger.Instance == null || GTPlayer.Instance == null)
            {
                // Debug.LogError("Main.TrueRightHand: GorillaTagger.Instance or GTPlayer.Instance is null!");
                return (Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero, Vector3.zero);
            }

            Quaternion handRotation = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.rightHandRotOffset;
            Vector3 handPosition = GorillaTagger.Instance.rightHandTransform.position +
                                   (GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.rightHandOffset);

            return (
                position: handPosition,
                rotation: handRotation,
                up: handRotation * Vector3.up,
                forward: handRotation * Vector3.forward,
                right: handRotation * Vector3.right
            );
        }

        /// <summary>
        /// Finds SnowballThrowable instances that are children of the player's hand/holdable slots
        /// by checking their parent transform paths.
        /// This should be called once per frame before any mod logic that needs the HELD_SNOWBALLS list.
        /// </summary>
        public static void RefreshHeldSnowballs()
        {
            HELD_SNOWBALLS.Clear();
            processedThisFrameForPathChecking.Clear();

            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null) return;

            // Define the expected parent paths (these need to be exact for your game version)
            // It's often better to find these parent Transforms by a more robust method if possible,
            // e.g., by reference from GorillaTagger.Instance.leftHandTransform / rightHandTransform if they have direct children.
            string holdablesPathGeneric = "player objects/local vrrig/local gorilla player/holdables"; // Generic holdables
            string leftHandItemPathSpecific = "player objects/local vrrig/local gorilla player/riganchor/rig/body/shoulder.l/upper_arm.l/forearm.l/hand.l/palm.01.l/transferrableitemlefthand";
            string rightHandItemPathSpecific = "player objects/local vrrig/local gorilla player/riganchor/rig/body/shoulder.r/upper_arm.r/forearm.r/hand.r/palm.01.r/transferrableitemrighthand";

            // Alternative: Get direct references to hand hold points if available
            // Transform leftHandHoldPoint = GorillaTagger.Instance.leftHandTransform.Find("TransferrableItemLeftHand"); // Example, path might differ
            // Transform rightHandHoldPoint = GorillaTagger.Instance.rightHandTransform.Find("TransferrableItemRightHand"); // Example

            SnowballThrowable[] allPotentialSnowballs = GameObject.FindObjectsOfType<SnowballThrowable>(true); // Include inactive

            foreach (SnowballThrowable snowball in allPotentialSnowballs)
            {
                if (snowball == null || snowball.transform.parent == null) continue;
                if (processedThisFrameForPathChecking.Contains(snowball)) continue;

                string parentPath = GetFullPath(snowball.transform.parent).ToLower();

                // Check against known holdable paths
                // Using EndsWith for specific hand paths because the snowball itself might add to the path.
                if (parentPath == holdablesPathGeneric ||
                    parentPath.EndsWith(leftHandItemPathSpecific) ||
                    parentPath.EndsWith(rightHandItemPathSpecific))
                {
                    HELD_SNOWBALLS.Add(snowball);
                    processedThisFrameForPathChecking.Add(snowball);
                }
                // Add more specific checks if needed, e.g., if snowballs are direct children of hand transforms
                else if (snowball.transform.parent == GorillaTagger.Instance.leftHandTransform ||
                         snowball.transform.parent == GorillaTagger.Instance.rightHandTransform)
                {
                    HELD_SNOWBALLS.Add(snowball);
                    processedThisFrameForPathChecking.Add(snowball);
                }
                // Example using direct hold point references (if you can get them reliably):
                // else if (leftHandHoldPoint != null && snowball.transform.parent == leftHandHoldPoint) { ... }
                // else if (rightHandHoldPoint != null && snowball.transform.parent == rightHandHoldPoint) { ... }
            }
        }

        // Helper to get the full hierarchy path of a Transform (from iiMenu)
        private static string GetFullPath(Transform current)
        {
            if (current == null) return "";
            if (current.parent == null) return "/" + current.name;
            return GetFullPath(current.parent) + "/" + current.name;
        }
    }
}
