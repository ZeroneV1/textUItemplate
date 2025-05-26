using BepInEx;
using TextUITemplate.Management;
using TextUITemplate.Mods; // Required for static mod classes
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
                var visualizer = TagReachVisualizer.Instance;
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
            TextUITemplatePatcher.ApplyPatches(false);

            if (TagReachMod.IsModActive()) TagReachMod.Disable();
            if (SpeedBoost.IsActive()) SpeedBoost.Disable();
            if (WallWalkMod.IsCurrentlyActive()) WallWalkMod.Disable();
            if (LongJumpMod.IsModActive()) LongJumpMod.Disable();
            // FastSnowballsMod.Disable() removed as it's not part of this reverted state

            Menu.FullCleanup();

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
                Menu.Start();

                if (typeof(WallWalkMod).GetMethod("Initialize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
                {
                    WallWalkMod.Initialize();
                }

                Logger.LogInfo($"[{PluginInfo.Name}] Menu.Start() and Mod Initializations executed successfully.");
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
                Menu.Load();

                // MOD LOGIC EXECUTION
                // SpeedBoost and LongJump logic are handled by the GTPlayer.LateUpdate Prefix patch.
                // WallWalk and TagReach have logic that needs to run based on input or continuous checks here.

                if (WallWalkMod.IsCurrentlyActive())
                {
                    WallWalkMod.ExecuteLogic();
                }

                TagReachMod.UpdateLogic(); // Manages enabling/disabling its SphereCastPatch based on input.

                // Main.RefreshHeldSnowballs() and FastSnowballsMod.ExecuteLogic() removed.
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[{PluginInfo.Name}][Plugin.Update] Error: {e.ToString()}");
            }
        }
    }
}