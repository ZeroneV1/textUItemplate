using System;
using System.Reflection;
using GorillaLocomotion;
using UnityEngine;
using TextUITemplate.Management; // For Menu, Settings, Main classes
using TextUITemplate.Libraries; // For Interfaces.Create
using TMPro;                   // For TextMeshPro
using GorillaTag;              // For GorillaTagger

namespace TextUITemplate.Mods
{
    public class WallWalkMod
    {
        private static bool isActive = false;
        private static GameObject uiStatusParent = null;
        private static TextMeshPro uiStatusText = null;

        // Cached FieldInfo for performance, initialized once
        private static FieldInfo lastHitInfoHandField = null;

        // Call this once, perhaps in your Plugin.Start() or when WallWalkMod is first accessed.
        public static void Initialize()
        {
            if (lastHitInfoHandField == null)
            {
                lastHitInfoHandField = typeof(GTPlayer).GetField("lastHitInfoHand", BindingFlags.Instance | BindingFlags.NonPublic);
                if (lastHitInfoHandField == null)
                {
                    Debug.LogError("WallWalkMod: Critical - 'lastHitInfoHand' field not found via reflection. Wall Walk will not function.");
                }
            }
        }

        // Called by the UI Button when toggled ON
        public static void Enable() // Renamed from Load to Enable for consistency
        {
            isActive = true;
            // UI update will be handled by Menu.cs calling UpdateStatusUI()
            Debug.Log("WallWalkMod: Enabled.");
        }

        // Called by the UI Button when toggled OFF
        public static void Disable() // Renamed from Cleanup to Disable
        {
            isActive = false;
            // UI update will be handled by Menu.cs calling UpdateStatusUI()
            if (uiStatusParent != null)
            {
                uiStatusParent.SetActive(false); // Explicitly hide UI when mod is disabled
            }
            Debug.Log("WallWalkMod: Disabled.");
        }

        public static bool IsCurrentlyActive()
        {
            return isActive;
        }

        // This method is called by Menu.Load() to update the UI status display
        public static void UpdateStatusUI()
        {
            // Determine if the UI should be visible: Main menu is on AND this mod is enabled
            bool shouldBeVisible = Menu.IsMenuToggledOn && isActive;

            if (uiStatusParent == null && shouldBeVisible)
            {
                // Create UI only if it's supposed to be visible and doesn't exist yet
                Interfaces.Create("WallWalk Status", ref uiStatusParent, ref uiStatusText, TextAlignmentOptions.BottomLeft); // Example: BottomLeft
                if (uiStatusText != null) uiStatusText.fontSize = 0.35f; // Adjust as needed
            }

            if (uiStatusParent != null)
            {
                if (uiStatusParent.activeSelf != shouldBeVisible)
                {
                    uiStatusParent.SetActive(shouldBeVisible);
                }

                if (shouldBeVisible && uiStatusText != null) // Only update text if visible
                {
                    uiStatusText.text = $"<size=0.7>Wall Walk: <color={Menu.Color32ToHTML(Settings.theme)}>[ON]</color></size>";
                    if (uiStatusText.renderer.material.shader != Shader.Find("GUI/Text Shader"))
                        uiStatusText.renderer.material.shader = Shader.Find("GUI/Text Shader");

                    if (GorillaTagger.hasInstance && GorillaTagger.Instance.headCollider != null)
                    {
                        Transform headTransform = GorillaTagger.Instance.headCollider.transform;
                        // Example distinct position for Wall Walk UI (e.g., bottom-left of view)
                        float forwardOffset = 1.8f;
                        float rightOffset = -1.0f; // Negative to go left
                        float verticalOffset = -0.6f; // Negative to go down
                        uiStatusParent.transform.position = headTransform.position +
                                                            (headTransform.forward * forwardOffset) +
                                                            (headTransform.right * rightOffset) +
                                                            (headTransform.up * verticalOffset);
                        uiStatusParent.transform.rotation = headTransform.rotation;
                    }
                }
            }
        }

        // This is the core logic. Call this from Plugin.Update() if isActive is true.
        public static void ExecuteLogic() // Renamed from DoWallWalkLogic for clarity
        {
            if (!isActive || GTPlayer.Instance == null || GTPlayer.Instance.bodyCollider == null || lastHitInfoHandField == null)
            {
                return;
            }

            float rayLength = 0.2f;
            float forceStrength = -2f;

            try
            {
                RaycastHit lastHitFromGame = (RaycastHit)lastHitInfoHandField.GetValue(GTPlayer.Instance);
                Vector3 normalToUseForRayDirection = lastHitFromGame.normal;

                if (normalToUseForRayDirection == Vector3.zero) // If normal is invalid, can't determine good ray direction
                {
                    // Debug.LogWarning("WallWalkMod: lastHitInfoHand.normal is zero. Skipping force application.");
                    return;
                }

                // Left Hand
                var leftHandData = Main.TrueLeftHand(); // Assuming this is from your TextUITemplate.Management.Main
                if (leftHandData.position != Vector3.zero) // Check if hand data is valid
                {
                    RaycastHit hitOnWall;
                    // Raycast from hand position, in the direction opposite to the surface normal game detected
                    if (Physics.Raycast(leftHandData.position, -normalToUseForRayDirection, out hitOnWall, rayLength, GTPlayer.Instance.locomotionEnabledLayers))
                    {
                        // Apply force along the normal of the wall *we just hit with our raycast*
                        GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(hitOnWall.normal * forceStrength, ForceMode.Acceleration);
                    }
                }

                // Right Hand
                var rightHandData = Main.TrueRightHand();
                if (rightHandData.position != Vector3.zero)
                {
                    RaycastHit hitOnWall;
                    if (Physics.Raycast(rightHandData.position, -normalToUseForRayDirection, out hitOnWall, rayLength, GTPlayer.Instance.locomotionEnabledLayers))
                    {
                        GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(hitOnWall.normal * forceStrength, ForceMode.Acceleration);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"WallWalkMod ExecuteLogic Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}