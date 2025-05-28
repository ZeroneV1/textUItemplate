using BepInEx; // For Paths
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TextUITemplate.Management
{
    public static class AutoConfigManager
    {
        private static string configFilePath;
        private const float SAVE_INTERVAL = 60f; // 60 seconds
        private static float timeSinceLastSave = 0f;
        private const string CONFIG_FILE_NAME = "TextUITemplate.AutoSave.json";

        public static void Initialize()
        {
            configFilePath = Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME);
            LoadConfig();
        }

        public static void Update()
        {
            if (!Menu.IsMenuToggledOn)
            {
                timeSinceLastSave = 0f; // Reset timer if menu is closed
                return;
            }

            timeSinceLastSave += Time.deltaTime;

            if (timeSinceLastSave >= SAVE_INTERVAL)
            {
                SaveConfig();
                timeSinceLastSave = 0f;
            }
        }

        public static void SaveConfig()
        {
            if (!Menu.IsMenuToggledOn)
            {
                Debug.Log("[AutoConfigManager] Menu is closed, skipping auto-save.");
                return;
            }

            AutoConfigData currentConfig = new AutoConfigData();

            foreach (var page in Menu.pages)
            {
                foreach (var button in page)
                {
                    // Only save toggleable mods. Navigation/utility buttons are typically not toggleable.
                    if (!button.isToggleable)
                        continue;

                    // Skip buttons without a valid title, though this should ideally not happen for mods.
                    if (string.IsNullOrEmpty(button.title))
                        continue;

                    ModConfigState modState = new ModConfigState
                    {
                        ModTitle = button.title,
                        IsEnabled = button.toggled
                    };

                    if (button.HasAdjustableValues)
                    {
                        foreach (var adjValue in button.ModAdjustableValues)
                        {
                            modState.AdjustableValues[adjValue.Name] = adjValue.GetValue();
                        }
                    }
                    currentConfig.ModStates.Add(modState);
                }
            }

            try
            {
                string json = JsonConvert.SerializeObject(currentConfig, Formatting.Indented);
                File.WriteAllText(configFilePath, json);
                Debug.Log($"[AutoConfigManager] Configuration saved to {configFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AutoConfigManager] Error saving config: {ex}");
            }
        }

        public static void LoadConfig()
        {
            if (!File.Exists(configFilePath))
            {
                Debug.Log($"[AutoConfigManager] No auto-save file found at {configFilePath}. Loading defaults.");
                return;
            }

            try
            {
                string json = File.ReadAllText(configFilePath);
                AutoConfigData savedConfig = JsonConvert.DeserializeObject<AutoConfigData>(json);

                if (savedConfig == null || savedConfig.ModStates == null)
                {
                    Debug.LogError("[AutoConfigManager] Failed to deserialize config or config is empty.");
                    return;
                }

                foreach (var savedModState in savedConfig.ModStates)
                {
                    Button targetButton = FindButtonByTitleRecursive(savedModState.ModTitle);
                    if (targetButton != null)
                    {
                        if (targetButton.isToggleable) // Ensure it's a mod button that can be toggled
                        {
                            // Apply toggled state only if different from current, to avoid redundant action calls
                            if (targetButton.toggled != savedModState.IsEnabled)
                            {
                                targetButton.toggled = savedModState.IsEnabled;
                                if (targetButton.toggled && targetButton.action != null)
                                {
                                    targetButton.action.Invoke();
                                }
                                else if (!targetButton.toggled && targetButton.disableAction != null)
                                {
                                    targetButton.disableAction.Invoke();
                                }
                            }
                        }

                        if (targetButton.HasAdjustableValues && savedModState.AdjustableValues != null)
                        {
                            foreach (var savedValueEntry in savedModState.AdjustableValues)
                            {
                                AdjustableValue targetAdjValue = targetButton.ModAdjustableValues.Find(adjVal => adjVal.Name == savedValueEntry.Key);
                                if (targetAdjValue != null)
                                {
                                    try
                                    {
                                        object convertedValue = ConvertValueToType(savedValueEntry.Value, targetAdjValue.ValType);
                                        targetAdjValue.SetValue(convertedValue);
                                        targetAdjValue.OnValueChanged?.Invoke(convertedValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"[AutoConfigManager] Error applying saved value for {targetButton.title} - {savedValueEntry.Key}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
                Debug.Log($"[AutoConfigManager] Configuration loaded from {configFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AutoConfigManager] Error loading config: {ex}");
            }
        }

        private static object ConvertValueToType(object value, AdjustableValueType targetType)
        {
            if (value == null) return null;

            // Newtonsoft.Json might deserialize integers as Int64 and floats as Double.
            // We need to ensure they are converted to the specific types (Int32, Single) our mods expect.
            switch (targetType)
            {
                case AdjustableValueType.Float:
                    return Convert.ToSingle(value); // Explicitly convert to float (Single)
                case AdjustableValueType.Int:
                    return Convert.ToInt32(value);  // Explicitly convert to int (Int32)
                case AdjustableValueType.Bool:
                    return Convert.ToBoolean(value);
                default:
                    return value; // Should not happen for known types
            }
        }

        private static Button FindButtonByTitleRecursive(string title)
        {
            foreach (var page in Menu.pages)
            {
                foreach (var button in page)
                {
                    if (button.title == title)
                        return button;
                }
            }
            return null;
        }

        public static void ForceSaveOnExit()
        {
            // Only save on exit if the menu is currently open, or adjust as needed.
            if (Menu.IsMenuToggledOn)
            {
                SaveConfig();
            }
        }
    }
}