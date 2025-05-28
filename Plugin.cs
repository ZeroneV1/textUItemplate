using BepInEx;
using TextUITemplate.Management; // Ensure this is here for AutoConfigManager
using TextUITemplate.Mods;
using UnityEngine;

namespace TextUITemplate
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private static GameObject tagReachVisualizerObject;

        public void OnEnable()
        {
            Logger.LogInfo($"[{PluginInfo.Name}] OnEnable called.");
            TextUITemplatePatcher.ApplyPatches(true);

            if (tagReachVisualizerObject == null)
            {
                var visualizer = TagReachVisualizer.Instance; // TagReachVisualizer.cs
                if (visualizer != null)
                {
                    tagReachVisualizerObject = visualizer.gameObject;
                    DontDestroyOnLoad(tagReachVisualizerObject);
                    Logger.LogInfo($"[{PluginInfo.Name}] TagReachVisualizer component host GameObject created/retrieved.");
                }
            }
            else
            {
                if (tagReachVisualizerObject != null) tagReachVisualizerObject.SetActive(true);
            }
        }

        public void OnDisable()
        {
            Logger.LogInfo($"[{PluginInfo.Name}] OnDisable called.");

            // Optional: Force a save on disable/quit if the menu is open or always.
            // AutoConfigManager.ForceSaveOnExit(); //

            TextUITemplatePatcher.ApplyPatches(false); // HarmonyPatches.cs

            // Ensure mods are disabled before full menu cleanup
            if (TagReachMod.IsModActive()) TagReachMod.Disable(); // TagReachMod.cs
            if (SpeedBoost.IsActive()) SpeedBoost.Disable(); // SpeedBoost.cs
            if (WallWalkMod.IsCurrentlyActive()) WallWalkMod.Disable(); // wallwalk.cs
            if (LongJumpMod.IsModActive()) LongJumpMod.Disable(); // LongJumpMod.cs

            Menu.FullCleanup(); // Management/Menu.cs

            if (tagReachVisualizerObject != null)
            {
                Destroy(tagReachVisualizerObject);
                tagReachVisualizerObject = null;
            }
        }

        public void Start()
        {
            Logger.LogInfo($"[{PluginInfo.Name}] Unity Start() method called. Initializing Menu and Mods.");
            try
            {
                Menu.Start(); // Management/Menu.cs - Ensures pages and buttons are initialized

                if (typeof(WallWalkMod).GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
                {
                    WallWalkMod.Initialize(); // wallwalk.cs
                }

                AutoConfigManager.Initialize(); // Initialize and load config AFTER Menu.Start()

                Logger.LogInfo($"[{PluginInfo.Name}] Menu.Start(), Mod Initializations, and AutoConfigManager.Initialize() executed successfully.");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[{PluginInfo.Name}] Error during Start(): {e}");
            }
        }

        public void Update()
        {
            try
            {
                Menu.Load(); // Management/Menu.cs - Handles menu display and input
                AutoConfigManager.Update(); // Handles timed auto-saving

                // MOD LOGIC EXECUTION
                // SpeedBoost and LongJump logic are handled by the GTPlayer.LateUpdate Prefix patch.
                // WallWalk and TagReach have logic that needs to run based on input or continuous checks here.

                if (WallWalkMod.IsCurrentlyActive()) // wallwalk.cs
                {
                    WallWalkMod.ExecuteLogic(); // wallwalk.cs
                }

                TagReachMod.UpdateLogic(); // TagReachMod.cs - Manages enabling/disabling its SphereCastPatch based on input.

            }
            catch (System.Exception e)
            {
                Logger.LogError($"[{PluginInfo.Name}][Plugin.Update] Error: {e.ToString()}");
            }
        }
    }
}