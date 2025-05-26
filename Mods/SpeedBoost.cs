using GorillaLocomotion;
using UnityEngine;

namespace TextUITemplate.Mods
{
    public class SpeedBoost
    {
        // Default game values can remain const
        public const float DEFAULT_MAX_JUMP_SPEED = 6.5f;
        public const float DEFAULT_JUMP_MULTIPLIER = 1.1f;

        // Modded values - CHANGED FROM CONST TO STATIC PUBLIC
        public static float MODDED_MAX_JUMP_SPEED = 7.5f;
        public static float MODDED_JUMP_MULTIPLIER = 1.4f;

        private static bool isActive = false;

        public static void Enable()
        {
            isActive = true;
            RefreshSpeedBoostValues();
            Debug.Log("SpeedBoost: Mod Enabled. Values will be continuously applied by Harmony patch.");
        }

        public static void Disable()
        {
            isActive = false;
            RefreshSpeedBoostValues();
            Debug.Log("SpeedBoost: Mod Disabled. Default values restored.");
        }

        public static bool IsActive()
        {
            return isActive;
        }

        public static void RefreshSpeedBoostValues()
        {
            if (GTPlayer.Instance != null)
            {
                if (isActive)
                {
                    GTPlayer.Instance.maxJumpSpeed = MODDED_MAX_JUMP_SPEED;
                    GTPlayer.Instance.jumpMultiplier = MODDED_JUMP_MULTIPLIER;
                }
                else
                {
                    GTPlayer.Instance.maxJumpSpeed = DEFAULT_MAX_JUMP_SPEED;
                    GTPlayer.Instance.jumpMultiplier = DEFAULT_JUMP_MULTIPLIER;
                }
            }
            else
            {
                Debug.LogError("SpeedBoost: GTPlayer.Instance is null during RefreshSpeedBoostValues. Values not applied yet.");
            }
        }
    }
}