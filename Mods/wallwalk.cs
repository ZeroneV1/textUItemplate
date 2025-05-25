using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GorillaLocomotion;
using UnityEngine;
using TextUITemplate.Management; // Needed for Main.TrueLeftHand/TrueRightHand
using TextUITemplate.Libraries; // Needed for Interfaces.Create
using TMPro; // Needed for TextMeshPro and TextAlignmentOptions
using GorillaTag; // Needed for GorillaTagger.Instance

namespace TextUITemplate.Mods
{
    public class WallWalkMod
    {
        // A static boolean to control whether wall walk is active.
        private static bool isActive = false;

        // UI elements for the Wall Walk status display
        private static GameObject parent = null;
        private static TextMeshPro text = null;

        // Method to activate (or 'load') the wall walk functionality
        public static void Load()
        {
            if (!isActive)
            {
                isActive = true;
                Debug.Log("WallWalkMod: Activated.");
            }

            // UI Creation and Update logic (similar to PingCounter.Load())
            if (parent == null)
            {
                // Create the UI display for Wall Walk status
                Interfaces.Create("Wall Walk Status", ref parent, ref text, TextAlignmentOptions.TopRight);
            }
            else
            {
                // Ensure the UI is active if it was previously disabled
                if (!parent.activeSelf)
                    parent.SetActive(true);

                // Update the UI text to show the status (ON)
                string statusText = $"<size=0.7>Wall Walk: <color={Menu.Color32ToHTML(Settings.theme)}>[ON]</color></size>";
                if (text.text != statusText)
                    text.text = statusText;

                // Ensure the shader is correct
                if (text.renderer.material.shader != Shader.Find("GUI/Text Shader"))
                    text.renderer.material.shader = Shader.Find("GUI/Text Shader");

                // Position the UI (adjust as needed, perhaps different from main menu or ping counter)
                // For demonstration, placing it near the head like others, but you might want it elsewhere.
                if (GorillaTagger.hasInstance && GorillaTagger.Instance.headCollider != null)
                {
                    parent.transform.position = GorillaTagger.Instance.headCollider.transform.position + GorillaTagger.Instance.headCollider.transform.forward * 2.75f;
                    parent.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
                }
            }
        }

        // Method to deactivate (or 'cleanup') the wall walk functionality
        public static void Cleanup()
        {
            if (isActive)
            {
                isActive = false;
                Debug.Log("WallWalkMod: Deactivated.");
            }

            // UI Cleanup logic (similar to PingCounter.Cleanup())
            if (parent != null)
            {
                // Deactivate the UI display
                if (parent.activeSelf)
                    parent.SetActive(false);

                // Optionally, destroy the GameObject entirely if you want to reclaim memory completely
                // GameObject.Destroy(parent);
                // parent = null;
                // text = null;
            }
        }

        // This method will perform the actual wall walking logic
        // It should be called from an Update loop or similar, only when isActive is true.
        public static void DoWallWalkLogic()
        {
            // IMPORTANT: This check ensures the actual wall walk mechanics only run when the mod is active.
            if (isActive)
            {
                float num = 0.2f;
                float num2 = -2f;

                // Add checks for GorillaLocomotion.GTPlayer.Instance to avoid NullReferenceExceptions
                if (GorillaLocomotion.GTPlayer.Instance == null || GorillaLocomotion.GTPlayer.Instance.bodyCollider == null)
                {
                    // Debug.LogWarning("WallWalkMod: GTPlayer.Instance or bodyCollider is null. Cannot perform wall walk logic.");
                    return;
                }

                // Reflection access for lastHitInfoHand
                // It's generally safer to cache the FieldInfo if this is called every frame,
                // but for readability here, it's kept as is.
                FieldInfo lastHitInfoHandField = typeof(GTPlayer).GetField("lastHitInfoHand", BindingFlags.Instance | BindingFlags.NonPublic);
                if (lastHitInfoHandField == null)
                {
                    // Debug.LogError("WallWalkMod: 'lastHitInfoHand' field not found via reflection.");
                    return;
                }

                try
                {
                    RaycastHit raycastHitLeft = (RaycastHit)lastHitInfoHandField.GetValue(GorillaLocomotion.GTPlayer.Instance);
                    RaycastHit raycastHitRight = (RaycastHit)lastHitInfoHandField.GetValue(GorillaLocomotion.GTPlayer.Instance); // Assuming this field holds info for both hands, or you need two distinct fields if they're separate.

                    // Corrected: Unpack the value tuple and use the 'position' component
                    // This assumes Main.TrueLeftHand() returns a tuple like (Vector3 position, Quaternion rotation, ...)
                    var leftHandData = Main.TrueLeftHand(); // Unpacks the tuple into local variables
                    if (leftHandData.position != null) // Check the position component for null
                    {
                        RaycastHit raycastHit2;
                        if (Physics.Raycast(leftHandData.position, -raycastHitLeft.normal, out raycastHit2, num, GorillaLocomotion.GTPlayer.Instance.locomotionEnabledLayers))
                        {
                            GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(raycastHit2.normal * num2, ForceMode.Acceleration);
                        }
                    }

                    var rightHandData = Main.TrueRightHand(); // Unpacks the tuple into local variables
                    if (rightHandData.position != null) // Check the position component for null
                    {
                        RaycastHit raycastHit4;
                        if (Physics.Raycast(rightHandData.position, -raycastHitRight.normal, out raycastHit4, num, GorillaLocomotion.GTPlayer.Instance.locomotionEnabledLayers))
                        {
                            GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(raycastHit4.normal * num2, ForceMode.Acceleration);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Debug.LogError($"WallWalkMod: Error during reflection or physics calculation: {ex.Message}");
                }
            }
        }
    }
}