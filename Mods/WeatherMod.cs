// TextUITemplate/Mods/WeatherMod.cs
using UnityEngine;

namespace TextUITemplate.Mods
{
    public class WeatherMod
    {
        private static bool isModActive = false; // Tracks if the main "Weather Control" button is ON
        private static bool isRainDesired = false;

        public static string[] TimeOptions = { "Night", "Morning", "Day", "Evening" };
        // Default to "Day" (index 2 in TimeOptions array)
        public static int CurrentTimeOptionIndex = 2;

        // Call this when the Weather Control mod is enabled or its settings are first viewed
        public static void InitializeOrRefreshState()
        {
            // Attempt to determine initial rain state from the game
            // This is a simplified check. For a robust solution, you'd ideally
            // query the current actual weather from BetterDayNightManager if possible.
            bool gameIsRaining = false;
            if (BetterDayNightManager.instance != null && BetterDayNightManager.instance.weatherCycle != null && BetterDayNightManager.instance.weatherCycle.Length > 0)
            {
                // A simple assumption: if the first active weather type is rain, consider it raining.
                // You might need a more sophisticated way to check BetterDayNightManager.GetCurrentWeather() or similar.
                for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
                {
                    if (BetterDayNightManager.instance.weatherCycle[i] == BetterDayNightManager.WeatherType.Raining)
                    {
                        gameIsRaining = true;
                        break;
                    }
                    if (BetterDayNightManager.instance.weatherCycle[i] != BetterDayNightManager.WeatherType.None)
                    {
                        // Found an active weather type that isn't rain, so assume not currently forced rain by us.
                        break;
                    }
                }
            }
            isRainDesired = gameIsRaining;

            // Apply current settings if the mod is marked as active
            if (isModActive)
            {
                ApplyCurrentWeatherSettings();
            }
        }

        public static void SetModActive(bool active)
        {
            isModActive = active;
            if (isModActive)
            {
                InitializeOrRefreshState(); // Refresh and apply current settings when activated
            }
            else
            {
                RevertToDefaultWeather(); // Revert when deactivated
            }
        }

        public static bool IsModCurrentlyActive()
        {
            return isModActive;
        }

        // --- Time Control ---
        public static void ApplyTimeFromIndex(int index)
        {
            CurrentTimeOptionIndex = Mathf.Clamp(index, 0, TimeOptions.Length - 1);
            if (!isModActive) return; // Only apply if master toggle is on

            switch (CurrentTimeOptionIndex)
            {
                case 0: // Night
                    BetterDayNightManager.instance.SetTimeOfDay(0);
                    break;
                case 1: // Morning
                    BetterDayNightManager.instance.SetTimeOfDay(1);
                    break;
                case 2: // Day
                    BetterDayNightManager.instance.SetTimeOfDay(3);
                    break;
                case 3: // Evening
                    BetterDayNightManager.instance.SetTimeOfDay(7);
                    break;
            }
        }

        // --- Rain Control ---
        public static void SetRain(bool enabled)
        {
            isRainDesired = enabled;
            if (!isModActive) return; // Only apply if master toggle is on

            if (isRainDesired)
            {
                for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
                {
                    BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.Raining;
                }
            }
            else
            {
                for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
                {
                    BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.None;
                }
            }
            // If BetterDayNightManager needs a manual refresh:
            // BetterDayNightManager.instance.ForceWeatherUpdate(); 
            // BetterDayNightManager.instance.RefreshWeather(); // Or similar
        }

        public static bool IsRainDesired()
        {
            return isRainDesired;
        }

        public static void ApplyCurrentWeatherSettings()
        {
            if (!isModActive) return;
            ApplyTimeFromIndex(CurrentTimeOptionIndex);
            SetRain(isRainDesired);
        }

        public static void RevertToDefaultWeather()
        {
            // Example: Set time to default (e.g., Day) and disable rain
            BetterDayNightManager.instance.SetTimeOfDay(3); // Assuming Day is default time
            CurrentTimeOptionIndex = 2; // Index for "Day"

            for (int i = 0; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
            {
                BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.None; // Or your game's actual default
            }
            isRainDesired = false;

            // If BetterDayNightManager needs a manual refresh:
            // BetterDayNightManager.instance.ForceWeatherUpdate();
            // BetterDayNightManager.instance.RefreshWeather();
        }
    }
}