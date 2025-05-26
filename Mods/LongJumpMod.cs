using UnityEngine;
using GorillaLocomotion;         // For GTPlayer
using TextUITemplate.Libraries; // For ControllerInputs

namespace TextUITemplate.Mods
{
    public class LongJumpMod
    {
        private static bool isActive = false; // Is the mod toggled on in the menu?

        // This static field will hold the calculated jump power, similar to iiMenu's Movement.longJumpPower
        private static Vector3 currentLongJumpPower = Vector3.zero;

        // Called by the UI Button when toggled ON
        public static void Enable()
        {
            isActive = true;
            currentLongJumpPower = Vector3.zero; // Reset power when enabling
            Debug.Log("LongJumpMod: Enabled.");
        }

        // Called by the UI Button when toggled OFF
        public static void Disable()
        {
            isActive = false;
            currentLongJumpPower = Vector3.zero; // Ensure power is zeroed out when mod is disabled
            Debug.Log("LongJumpMod: Disabled.");
        }

        public static bool IsModActive()
        {
            return isActive;
        }

        // This method will be called every frame from Plugin.Update()
        public static void ExecuteLogic()
        {
            if (!isActive)
            {
                // If the mod is toggled off, ensure the jump power is reset
                if (currentLongJumpPower != Vector3.zero)
                {
                    currentLongJumpPower = Vector3.zero;
                }
                return;
            }

            // Ensure GTPlayer.Instance and its components are available
            if (GTPlayer.Instance == null ||
                GTPlayer.Instance.bodyCollider == null ||
                GTPlayer.Instance.bodyCollider.attachedRigidbody == null)
            {
                // If an essential component is missing, reset power and exit to prevent errors
                currentLongJumpPower = Vector3.zero;
                return;
            }

            bool rightPrimaryHeld = ControllerInputs.rightPrimary();

            if (rightPrimaryHeld)
            {
                // If it's the first frame the button is held in this activation sequence
                // (i.e., currentLongJumpPower is zero), calculate the power.
                if (currentLongJumpPower == Vector3.zero)
                {
                    currentLongJumpPower = GTPlayer.Instance.bodyCollider.attachedRigidbody.velocity / 125f;
                    currentLongJumpPower.y = 0f; // Zero out vertical component for a horizontal jump
                }
                // Continuously apply the calculated power directly to the position
                // NO Time.deltaTime, to match iiMenu's FPS-dependent behavior
                GTPlayer.Instance.transform.position += currentLongJumpPower;
            }
            else
            {
                // Reset power when the button is released
                currentLongJumpPower = Vector3.zero;
            }
        }
    }
}