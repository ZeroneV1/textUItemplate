using BepInEx;
using TextUITemplate.Management; // Needed for Menu

// Make sure your PluginInfo class is correctly defined in this namespace
// or referenced correctly if it's in a different assembly/namespace.
// namespace TextUITemplate { public static class PluginInfo { /* ... */ } }

namespace TextUITemplate
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public void OnEnable()
        {
            Logger.LogInfo($"[{PluginInfo.Name}] OnEnable called. Attempting to apply patches.");
            try
            {
                // --- CRITICAL TEST ---
                // Temporarily comment out the line below to see if Harmony patching is causing the plugin to disable.
                // If OnDisable is NOT called immediately after this, the issue is likely in your HarmonyPatches.Patch method.
                HarmonyPatches.Patch(true);
                Logger.LogInfo($"[{PluginInfo.Name}] HarmonyPatches.Patch(true) executed (or was skipped if commented out).");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[{PluginInfo.Name}] Error during HarmonyPatches.Patch(true): {e}");
            }
        }

        public void OnDisable()
        {
            Logger.LogInfo($"[{PluginInfo.Name}] OnDisable called. Attempting to remove patches.");
            try
            {
                // --- CRITICAL TEST ---
                // Temporarily comment out the line below if you commented out the one in OnEnable.
                // HarmonyPatches.Patch(false);
                Logger.LogInfo($"[{PluginInfo.Name}] HarmonyPatches.Patch(false) executed (or was skipped if commented out).");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[{PluginInfo.Name}] Error during HarmonyPatches.Patch(false): {e}");
            }
        }

        public void Start() // Unity's Start method
        {
            Logger.LogInfo($"[{PluginInfo.Name}] Unity Start() method called. Attempting to call Menu.Start().");
            try
            {
                Menu.Start();
                Logger.LogInfo($"[{PluginInfo.Name}] Menu.Start() executed successfully.");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[{PluginInfo.Name}] Error during Menu.Start(): {e}");
            }
        }

        public void Update() // Unity's Update method
        {
            // This log is essential to confirm Update() is running.
            Logger.LogInfo($"[{PluginInfo.Name}][Plugin.Update] Update() method entered. Attempting to call Menu.Load().");
            try
            {
                Menu.Load();
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[{PluginInfo.Name}][Plugin.Update] Error during Menu.Load(): {e.ToString()}");
            }
        }
    }

    // Ensure your PluginInfo class is defined correctly and accessible.
    // This is just a reminder of its structure; use your actual PluginInfo.cs file.
    // public static class PluginInfo
    // {
    // public const string GUID = "com.finn.gorillatag.textuitemplate";
    // public const string Name = "TextUITemplate";
    // public const string Version = "1.0.0";
    // }

    // Ensure your HarmonyPatches class is defined correctly.
    // This is just a structural reminder.
    // public static class HarmonyPatches
    // {
    // public static void Patch(bool enable) { /* Your actual Harmony logic */ }
    // }
}