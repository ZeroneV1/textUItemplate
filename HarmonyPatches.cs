using System;
using HarmonyLib;
using System.Reflection;
using GorillaLocomotion; // For GTPlayer
using GorillaTag;       // For GorillaTagger (used by SphereCastPatch)
using TextUITemplate.Mods; // To access mod states and constants

namespace TextUITemplate
{
    public class TextUITemplatePatcher
    {
        public static Harmony instance;
        public static bool patched { get; private set; }
        public const string id = PluginInfo.GUID;

        public static void ApplyPatches(bool toggle)
        {
            if (toggle)
            {
                if (!patched)
                {
                    if (instance == null)
                    {
                        instance = new Harmony(id);
                    }
                }
                instance.PatchAll(Assembly.GetExecutingAssembly());
                patched = true;
                UnityEngine.Debug.Log($"[{id}] Harmony Patches Applied by TextUITemplatePatcher.");
            }
            else
            {
                if (instance != null)
                {
                    if (patched)
                    {
                        instance.UnpatchSelf();
                        UnityEngine.Debug.Log($"[{id}] Harmony Patches Removed by TextUITemplatePatcher.");
                    }
                }
                patched = false;
            }
        }
    }
}

namespace TextUITemplate.HarmonyPatches
{
    // Combined patch for GTPlayer.LateUpdate to handle SpeedBoost and LongJump
    [HarmonyPatch(typeof(GTPlayer))]
    public class GTPlayer_LateUpdate_Modifications_Patch // Renamed if it's handling multiple mods now
    {
        [HarmonyPatch("LateUpdate")]
        [HarmonyPrefix]
        public static void ExecuteModLogic_Prefix(GTPlayer __instance)
        {
            if (__instance == null) return;

            // SpeedBoost Logic
            if (SpeedBoost.IsActive())
            {
                if (__instance.maxJumpSpeed != SpeedBoost.MODDED_MAX_JUMP_SPEED)
                {
                    __instance.maxJumpSpeed = SpeedBoost.MODDED_MAX_JUMP_SPEED;
                }
                if (__instance.jumpMultiplier != SpeedBoost.MODDED_JUMP_MULTIPLIER)
                {
                    __instance.jumpMultiplier = SpeedBoost.MODDED_JUMP_MULTIPLIER;
                }
            }
            else
            {
                if (__instance.maxJumpSpeed != SpeedBoost.DEFAULT_MAX_JUMP_SPEED)
                {
                    __instance.maxJumpSpeed = SpeedBoost.DEFAULT_MAX_JUMP_SPEED;
                }
                if (__instance.jumpMultiplier != SpeedBoost.DEFAULT_JUMP_MULTIPLIER)
                {
                    __instance.jumpMultiplier = SpeedBoost.DEFAULT_JUMP_MULTIPLIER;
                }
            }

            // LongJump Logic
            if (LongJumpMod.IsModActive())
            {
                LongJumpMod.ExecuteLogic();
            }

            // Fast Snowballs logic removed from here
        }
    }

    // Patch for GorillaTagger.get_sphereCastRadius for TagReachMod
    [HarmonyPatch(typeof(GorillaTagger), "get_sphereCastRadius")]
    public class SphereCastPatch
    {
        public static bool patchEnabled = false;
        public static float overrideRadius = 0.1f;

        public static void Postfix(GorillaTagger __instance, ref float __result)
        {
            if (patchEnabled)
            {
                __result = overrideRadius;
            }
        }
    }
}