using UnityEngine;
using GorillaLocomotion;
using GorillaTag;
using TextUITemplate.Libraries;
using TextUITemplate.HarmonyPatches;
using TextUITemplate.Management; // For Settings.theme for color

namespace TextUITemplate.Mods
{
    public class TagReachMod
    {
        public static float MODDED_TAG_REACH_DISTANCE = 0.5f;
        public const float DEFAULT_TAG_REACH_DISTANCE = 0.25f;
        private static bool isActive = false;
        private static bool isCurrentlyExtendingReach = false;

        public static void Enable()
        {
            isActive = true;
            UpdateSphereCastPatchState(); // Initial state update for the patch
            Debug.Log("TagReachMod: Enabled.");
        }

        public static void Disable()
        {
            isActive = false;
            UpdateSphereCastPatchState(); // Ensure patch is disabled
            Debug.Log("TagReachMod: Disabled.");
        }

        public static bool IsModActive()
        {
            return isActive;
        }

        public static void UpdateLogic() // Called by Plugin.Update()
        {
            if (!isActive)
            {
                if (TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled)
                {
                    TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled = false;
                }
                return;
            }

            if (GTPlayer.Instance == null || GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
            {
                if (TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled)
                {
                    TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled = false;
                }
                return;
            }

            bool useRightGrab = ControllerInputs.rightGrip();

            if (PlayerIsTagged(GorillaTagger.Instance.offlineVRRig) && useRightGrab)
            {
                if (!TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled ||
                    TextUITemplate.HarmonyPatches.SphereCastPatch.overrideRadius != MODDED_TAG_REACH_DISTANCE)
                {
                    TextUITemplate.HarmonyPatches.SphereCastPatch.overrideRadius = MODDED_TAG_REACH_DISTANCE;
                    TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled = true;
                }
            }
            else
            {
                if (TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled)
                {
                    TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled = false;
                }
            }
        }

        private static void UpdateSphereCastPatchState()
        {
            if (!isActive)
            {
                TextUITemplate.HarmonyPatches.SphereCastPatch.patchEnabled = false;
            }
            // If active, UpdateLogic will handle enabling based on conditions
        }

        // This is called by the AdjustableValue's OnValueChanged callback
        public static void VisualizeReachTemporarily(float newRadius)
        {
            if (GorillaTagger.Instance != null && GorillaTagger.Instance.leftHandTransform != null && GorillaTagger.Instance.rightHandTransform != null)
            {
                // Use your theme color or a specific color for visualization
                Color vizColor = Settings.theme; // Assuming Settings.theme is your menu color

                // Ensure the TagReachVisualizer instance exists
                TagReachVisualizer visualizer = TagReachVisualizer.Instance;
                if (visualizer != null)
                {
                    visualizer.ShowVisualization(GorillaTagger.Instance.leftHandTransform.position, newRadius, vizColor, true);
                    visualizer.ShowVisualization(GorillaTagger.Instance.rightHandTransform.position, newRadius, vizColor, false);
                }
            }
        }

        private static bool PlayerIsTagged(VRRig who)
        {
            if (who == null || who.mainSkin == null || who.mainSkin.material == null || who.nameTagAnchor == null)
                return false;

            string materialName = who.mainSkin.material.name.ToLower();
            return materialName.Contains("fected") || materialName.Contains("it") ||
                   materialName.Contains("stealth") || materialName.Contains("ice") ||
                   !who.nameTagAnchor.activeSelf;
        }
    }
}