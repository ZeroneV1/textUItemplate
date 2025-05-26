using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using TextUITemplate.Libraries;
using TextUITemplate.Mods;
using TMPro;
using UnityEngine;

namespace TextUITemplate.Management
{
    public class AdjustableMenu
    {
        private static GameObject adjUIParent = null;
        private static TextMeshPro adjUIText = null;

        private static bool isVisible = false;
        private static int selectedUiItemIndex = 0;
        private static float inputCooldown = 0f;
        private const float COOLDOWN_TIME = 0.15f;

        private static Button currentFocusedModButton = null;
        private static List<AdjustableValue> currentModValues = new List<AdjustableValue>();
        private static List<ModPreset> currentModPresets = new List<ModPreset>();
        private static int currentPresetCycleIndex = 0;

        private const string PRESET_ITEM_NAME = "Apply Preset";
        private static Shader guiTextShaderCache = null;

        public static void SetFocusedMod(Button modButton)
        {
            bool modChanged = currentFocusedModButton != modButton;
            currentFocusedModButton = modButton;

            currentModValues.Clear();
            currentModPresets.Clear();

            if (modChanged)
            {
                selectedUiItemIndex = 0;
                currentPresetCycleIndex = 0;
            }

            if (currentFocusedModButton != null && (currentFocusedModButton.HasAdjustableValues || currentFocusedModButton.HasPresets))
            {
                if (currentFocusedModButton.HasAdjustableValues)
                    currentModValues.AddRange(currentFocusedModButton.ModAdjustableValues);
                if (currentFocusedModButton.HasPresets)
                    currentModPresets.AddRange(currentFocusedModButton.Presets);

                isVisible = true;
            }
            else
            {
                isVisible = false;
            }

            if (adjUIParent != null)
            {
                adjUIParent.SetActive(isVisible);
            }
        }

        public static void UpdateUI()
        {
            if (!Menu.IsMenuToggledOn)
            {
                if (isVisible) isVisible = false;
                if (adjUIParent != null && adjUIParent.activeSelf)
                    adjUIParent.SetActive(false);
                return;
            }

            bool hasContentToShow = currentModValues.Count > 0 || currentModPresets.Count > 0;
            if (!isVisible || !hasContentToShow)
            {
                if (adjUIParent != null && adjUIParent.activeSelf)
                    adjUIParent.SetActive(false);
                return;
            }

            if (!GorillaTagger.hasInstance) { Cleanup(); return; }

            if (adjUIParent == null)
            {
                Interfaces.Create("AdjustableValuesMenu", ref adjUIParent, ref adjUIText, TextAlignmentOptions.TopLeft);
                RectTransform adjTransform = adjUIParent.GetComponent<RectTransform>();
                adjTransform.sizeDelta = new Vector2(2.2f, 2.2f);
                adjUIText.fontSize = 0.38f;
                adjUIText.lineSpacing = 17f;
            }

            if (isVisible && !adjUIParent.activeSelf) adjUIParent.SetActive(true);

            if (guiTextShaderCache == null)
            {
                guiTextShaderCache = Shader.Find("GUI/Text Shader");
            }
            if (adjUIText.renderer.material.shader != guiTextShaderCache)
                adjUIText.renderer.material.shader = guiTextShaderCache;

            int totalUiItems = currentModValues.Count + (currentModPresets.Count > 0 ? 1 : 0);

            if (totalUiItems == 0) selectedUiItemIndex = 0;
            else if (selectedUiItemIndex >= totalUiItems) selectedUiItemIndex = totalUiItems - 1;
            if (selectedUiItemIndex < 0 && totalUiItems > 0) selectedUiItemIndex = 0;


            if (Time.time >= inputCooldown && totalUiItems > 0)
            {
                if (ControllerInputs.LeftStickUp() || UnityInput.Current.GetKeyDown(KeyCode.PageUp))
                {
                    selectedUiItemIndex--;
                    if (selectedUiItemIndex < 0) selectedUiItemIndex = totalUiItems - 1;
                    inputCooldown = Time.time + COOLDOWN_TIME * 1.5f;
                }
                else if (ControllerInputs.LeftStickDown() || UnityInput.Current.GetKeyDown(KeyCode.PageDown))
                {
                    selectedUiItemIndex++;
                    if (selectedUiItemIndex >= totalUiItems) selectedUiItemIndex = 0;
                    inputCooldown = Time.time + COOLDOWN_TIME * 1.5f;
                }

                bool isPresetRowSelected = currentModPresets.Count > 0 && selectedUiItemIndex == currentModValues.Count;

                if (isPresetRowSelected)
                {
                    if (ControllerInputs.LeftStickRight() || UnityInput.Current.GetKey(KeyCode.KeypadPlus) || UnityInput.Current.GetKey(KeyCode.Equals))
                    {
                        if (currentModPresets.Count > 0)
                        {
                            currentPresetCycleIndex++;
                            if (currentPresetCycleIndex >= currentModPresets.Count) currentPresetCycleIndex = 0;
                            inputCooldown = Time.time + COOLDOWN_TIME;
                        }
                    }
                    else if (ControllerInputs.LeftStickLeft() || UnityInput.Current.GetKey(KeyCode.KeypadMinus) || UnityInput.Current.GetKey(KeyCode.Minus))
                    {
                        if (currentModPresets.Count > 0)
                        {
                            currentPresetCycleIndex--;
                            if (currentPresetCycleIndex < 0) currentPresetCycleIndex = currentModPresets.Count - 1;
                            inputCooldown = Time.time + COOLDOWN_TIME;
                        }
                    }
                    else if (ControllerInputs.leftStick() || UnityInput.Current.GetKeyDown(KeyCode.KeypadEnter) || UnityInput.Current.GetKeyDown(KeyCode.Return))
                    {
                        if (currentModPresets.Count > 0 && currentPresetCycleIndex < currentModPresets.Count)
                        {
                            ApplyPreset(currentModPresets[currentPresetCycleIndex]);
                            inputCooldown = Time.time + COOLDOWN_TIME * 1.5f;
                        }
                    }
                }
                else if (selectedUiItemIndex < currentModValues.Count)
                {
                    AdjustableValue selectedVal = currentModValues[selectedUiItemIndex];
                    object originalValue = selectedVal.GetValue();
                    object newValue = originalValue;
                    bool valueChanged = false;

                    if (ControllerInputs.LeftStickRight() || UnityInput.Current.GetKey(KeyCode.KeypadPlus) || UnityInput.Current.GetKey(KeyCode.Equals))
                    {
                        switch (selectedVal.ValType)
                        {
                            case AdjustableValueType.Float: newValue = Mathf.Clamp((float)originalValue + selectedVal.FloatIncrementStep, selectedVal.MinValue, selectedVal.MaxValue); break;
                            case AdjustableValueType.Int: newValue = Mathf.Clamp((int)originalValue + selectedVal.IntIncrementStep, (int)selectedVal.MinValue, (int)selectedVal.MaxValue); break;
                            case AdjustableValueType.Bool: newValue = !(bool)originalValue; break;
                        }
                        if (!object.Equals(originalValue, newValue)) valueChanged = true;
                        inputCooldown = Time.time + COOLDOWN_TIME;
                    }
                    else if (ControllerInputs.LeftStickLeft() || UnityInput.Current.GetKey(KeyCode.KeypadMinus) || UnityInput.Current.GetKey(KeyCode.Minus))
                    {
                        switch (selectedVal.ValType)
                        {
                            case AdjustableValueType.Float: newValue = Mathf.Clamp((float)originalValue - selectedVal.FloatIncrementStep, selectedVal.MinValue, selectedVal.MaxValue); break;
                            case AdjustableValueType.Int: newValue = Mathf.Clamp((int)originalValue - selectedVal.IntIncrementStep, (int)selectedVal.MinValue, (int)selectedVal.MaxValue); break;
                            case AdjustableValueType.Bool: newValue = !(bool)originalValue; break;
                        }
                        if (!object.Equals(originalValue, newValue)) valueChanged = true;
                        inputCooldown = Time.time + COOLDOWN_TIME;
                    }

                    if (valueChanged)
                    {
                        selectedVal.SetValue(newValue);
                        selectedVal.OnValueChanged?.Invoke(newValue);
                    }
                }
            }

            string displayText = $"<size={adjUIText.fontSize * 1.15f}><color={Menu.Color32ToHTML(Settings.theme)}>{currentFocusedModButton?.title} Settings</color></size>\n";
            if (!hasContentToShow) displayText += "No editable values or presets.";
            else
            {
                for (int i = 0; i < currentModValues.Count; i++)
                {
                    AdjustableValue valToDisplay = currentModValues[i];
                    object currentValueObj = valToDisplay.GetValue();
                    string valueString = "";
                    switch (valToDisplay.ValType)
                    {
                        case AdjustableValueType.Float: valueString = ((float)currentValueObj).ToString("F2"); break;
                        case AdjustableValueType.Int: valueString = ((int)currentValueObj).ToString(); break;
                        case AdjustableValueType.Bool: valueString = (bool)currentValueObj ? $"<color={Menu.Color32ToHTML(Settings.theme)}>[ON]</color>" : "<color=red>[OFF]</color>"; break;
                        default: valueString = currentValueObj?.ToString() ?? "null"; break;
                    }
                    displayText += $"{(i == selectedUiItemIndex ? "-> " : "   ")}{valToDisplay.Name}: {valueString}\n";
                }

                if (currentModPresets.Count > 0)
                {
                    string presetName = (currentPresetCycleIndex < currentModPresets.Count) ? currentModPresets[currentPresetCycleIndex].Name : "N/A";
                    displayText += $"{(selectedUiItemIndex == currentModValues.Count ? "-> " : "   ")}{PRESET_ITEM_NAME}: <color=#FFFF00>{presetName}</color>\n";
                }
            }
            adjUIText.text = displayText;

            Transform headTransform = GorillaTagger.Instance.headCollider.transform;
            float forwardOffset = 2.5f;
            float leftOffset = -0.5f;  // Your specified value
            float desiredVerticalOffset = -0.5f; // Your specified value
            Vector3 calculatedPosition = headTransform.position + (headTransform.forward * forwardOffset) - (headTransform.right * leftOffset) + (headTransform.up * desiredVerticalOffset);
            adjUIParent.transform.position = calculatedPosition;
            adjUIParent.transform.rotation = headTransform.rotation;
        }

        private static void ApplyPreset(ModPreset presetToApply)
        {
            if (presetToApply == null || currentFocusedModButton == null || !currentFocusedModButton.HasAdjustableValues)
                return;

            foreach (var presetValueEntry in presetToApply.Values)
            {
                AdjustableValue targetAdjustableValue = currentModValues.Find(adjVal => adjVal.Name == presetValueEntry.Key);
                if (targetAdjustableValue != null)
                {
                    try
                    {
                        object valueToSet = presetValueEntry.Value;
                        if (targetAdjustableValue.ValType == AdjustableValueType.Float && !(valueToSet is float))
                            valueToSet = Convert.ToSingle(valueToSet);
                        else if (targetAdjustableValue.ValType == AdjustableValueType.Int && !(valueToSet is int))
                            valueToSet = Convert.ToInt32(valueToSet);
                        else if (targetAdjustableValue.ValType == AdjustableValueType.Bool && !(valueToSet is bool))
                            valueToSet = Convert.ToBoolean(valueToSet);

                        targetAdjustableValue.SetValue(valueToSet);
                        targetAdjustableValue.OnValueChanged?.Invoke(valueToSet);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error applying preset value for {presetValueEntry.Key}: {ex.Message}");
                    }
                }
            }
            Debug.Log($"Applied preset: {presetToApply.Name} for mod {currentFocusedModButton.title}");
        }

        public static void Cleanup()
        {
            if (adjUIParent != null)
            {
                GameObject.Destroy(adjUIParent);
                adjUIParent = null;
                adjUIText = null;
            }
            isVisible = false;
            currentModValues.Clear();
            currentModPresets.Clear();
            currentFocusedModButton = null;
        }
    }
}