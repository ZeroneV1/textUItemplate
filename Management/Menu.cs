using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using TextUITemplate.Libraries;
using TextUITemplate.Mods;
using TMPro;
using UnityEngine;
using GorillaTag;

namespace TextUITemplate.Management
{
    public class Menu
    {
        private static GameObject parent = null;
        private static TextMeshPro text = null;

        private static bool toggled = false;
        public static int page_index = 0;
        public static int index = 0;
        private static float cooldown;

        public static List<Button[]> pages = new List<Button[]>();

        private static int current_display_page = 0;
        private const int MODS_PER_PAGE = 7;

        public static string CurrentSelectedButtonTitle { get; private set; } = string.Empty;
        public static string CurrentSelectedButtonTooltip { get; private set; } = string.Empty;
        public static bool IsMenuToggledOn => toggled;


        public static void Start()
        {
            pages.Clear();

            List<Button> mainPageButtons = new List<Button>
            {
                new Button { title = "Example Page", tooltip = "Navigate to another page.", isToggleable = false, action = () => UpdateCurrentPage(1) },
                new Button { title = "Ping Counter", tooltip = "Shows network ping.", toggled = true, isToggleable = true, action = () => PingCounter.Load(), disableAction = () => PingCounter.Cleanup() },
                new Button { title = "Tool Tips", tooltip = "Displays item descriptions.", toggled = true, isToggleable = true, action = () => ToolTips.Load(), disableAction = () => ToolTips.Cleanup() },
                new Button { title = "Wall Walk", tooltip = "Enables wall walking.", toggled = false, isToggleable = true, action = () => WallWalkMod.Enable(), disableAction = () => WallWalkMod.Disable() },
                new Button {
                    title = "Tag Reach",
                    tooltip = "Extends tag reach (edit with left stick). Active when tagged & right grab held.",
                    toggled = false,
                    isToggleable = true,
                    action = () => TagReachMod.Enable(),
                    disableAction = () => TagReachMod.Disable(),
                    ModAdjustableValues = new List<AdjustableValue>
                    {
                        new AdjustableValue
                        {
                            Name = "Tag Reach Radius", ValType = AdjustableValueType.Float,
                            GetValue = () => TagReachMod.MODDED_TAG_REACH_DISTANCE,
                            SetValue = (val) => { TagReachMod.MODDED_TAG_REACH_DISTANCE = (float)val; },
                            OnValueChanged = (newVal) => TagReachMod.VisualizeReachTemporarily((float)newVal),
                            MinValue = 0.1f, MaxValue = 2.0f, FloatIncrementStep = 0.05f
                        }
                    }
                },
                new Button
                {
                    title = "Speed Boost",
                    tooltip = "Makes you speedy. Edit values or apply presets with left stick.",
                    toggled = false,
                    isToggleable = true,
                    action = () => SpeedBoost.Enable(),
                    disableAction = () => SpeedBoost.Disable(),
                    ModAdjustableValues = new List<AdjustableValue>
                    {
                        new AdjustableValue {
                            Name = "Max Jump Speed", ValType = AdjustableValueType.Float,
                            GetValue = () => SpeedBoost.MODDED_MAX_JUMP_SPEED,
                            SetValue = (val) => { SpeedBoost.MODDED_MAX_JUMP_SPEED = (float)val; },
                            OnValueChanged = (newVal) => { SpeedBoost.RefreshSpeedBoostValues(); },
                            MinValue = 1.0f, MaxValue = 20.0f, FloatIncrementStep = 0.1f
                        },
                        new AdjustableValue {
                            Name = "Jump Multiplier", ValType = AdjustableValueType.Float,
                            GetValue = () => SpeedBoost.MODDED_JUMP_MULTIPLIER,
                            SetValue = (val) => { SpeedBoost.MODDED_JUMP_MULTIPLIER = (float)val; },
                            OnValueChanged = (newVal) => { SpeedBoost.RefreshSpeedBoostValues(); },
                            MinValue = 0.1f, MaxValue = 5.0f, FloatIncrementStep = 0.05f
                        }
                    },
                    Presets = new List<ModPreset>
                    {
                        new ModPreset { Name = "Default Modded", Values = new Dictionary<string, object> { { "Max Jump Speed", 7.5f }, { "Jump Multiplier", 1.4f } } },
                        new ModPreset { Name = "High Jump", Values = new Dictionary<string, object> { { "Max Jump Speed", 10.0f }, { "Jump Multiplier", 2.0f } } }
                    }
                },
                new Button { title = "Long Jump", tooltip = "Hold Right Primary (A/X or E) for a boost.", toggled = false, isToggleable = true, action = () => LongJumpMod.Enable(), disableAction = () => LongJumpMod.Disable() }
            };

            pages.Add(mainPageButtons.ToArray());

            pages.Add(new Button[]
            {
                new Button { title = "Settings Page Example", tooltip = "Navigate to settings.", isToggleable = false, action = () => UpdateCurrentPage(0) },
                new Button { title = "Placeholder 1", tooltip = "Another button.", toggled = false, isToggleable = true },
                new Button { title = "Back to Main Mods", tooltip = "Return to the main list of mods.", isToggleable = false, action = () => UpdateCurrentPage(0) },
            });
        }

        private static void ChangeDisplayPage(int direction)
        {
            current_display_page += direction;
            Button[] allButtonsOnCurrentLogicalPage = (pages.Count > page_index && page_index >= 0) ? pages[page_index] : new Button[0];
            int totalModsOnLogicalPage = allButtonsOnCurrentLogicalPage.Length;
            int itemsActuallyDisplayingPerPage = MODS_PER_PAGE;

            int totalDisplayPagesRequired = (totalModsOnLogicalPage > 0) ? (int)Math.Ceiling((double)totalModsOnLogicalPage / itemsActuallyDisplayingPerPage) : 1;

            if (current_display_page < 0) current_display_page = 0;
            if (current_display_page >= totalDisplayPagesRequired) current_display_page = totalDisplayPagesRequired - 1;

            index = 0;
            cooldown = Time.time + 0.2f;
            UpdateAdjustableMenuFocus(null);
        }

        public static void Load()
        {
            if (!GorillaTagger.hasInstance)
            {
                if (parent != null && parent.activeSelf) parent.SetActive(false);
                ToolTips.Cleanup();
                PingCounter.Cleanup();
                AdjustableMenu.Cleanup();
                if (typeof(WallWalkMod).GetMethod("UpdateStatusUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null) WallWalkMod.UpdateStatusUI();
                return;
            }

            if (parent == null)
            {
                Interfaces.Create("PrimaryMenu", ref parent, ref text, TextAlignmentOptions.TopRight);
                parent.GetComponent<RectTransform>().sizeDelta = new Vector2(2.2f, 2.5f);
                text.fontSize = 0.5f;
            }

            Shader menuShader = Shader.Find("GUI/Text Shader");
            if (text.renderer.material.shader != menuShader)
                text.renderer.material.shader = menuShader;

            if (ControllerInputs.leftStick() || UnityInput.Current.GetKeyDown(KeyCode.Tab))
            {
                if (Time.time >= cooldown)
                {
                    toggled = !toggled;
                    cooldown = Time.time + 0.25f;
                    UpdateAdjustableMenuFocus(null);
                    if (!toggled)
                    {
                        ToolTips.Cleanup();
                        PingCounter.Cleanup();
                        if (typeof(WallWalkMod).GetMethod("UpdateStatusUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null) WallWalkMod.UpdateStatusUI();
                    }
                }
            }

            parent.SetActive(toggled);

            if (!toggled)
            {
                AdjustableMenu.UpdateUI();
                return;
            }

            Button[] allButtonsOnCurrentLogicalPage = (pages.Count > page_index && page_index >= 0) ? pages[page_index] : new Button[0];
            List<Button> buttonsToDisplayThisFrameList = new List<Button>();
            int totalButtonsOnLogicalPage = allButtonsOnCurrentLogicalPage.Length;
            int itemsPerPageToConsiderForPagination = MODS_PER_PAGE;
            int totalDisplayPagesRequired = (totalButtonsOnLogicalPage > 0) ? (int)Math.Ceiling((double)totalButtonsOnLogicalPage / itemsPerPageToConsiderForPagination) : 1;

            if (current_display_page < 0) current_display_page = 0;
            if (current_display_page >= totalDisplayPagesRequired) current_display_page = totalDisplayPagesRequired - 1;

            if (current_display_page > 0)
            {
                buttonsToDisplayThisFrameList.Add(new Button { title = "< Page", tooltip = "Previous Page", isToggleable = false, action = () => ChangeDisplayPage(-1) });
            }

            int startIndexForMods = current_display_page * itemsPerPageToConsiderForPagination;
            for (int i = 0; i < itemsPerPageToConsiderForPagination; i++)
            {
                int actualButtonIndex = startIndexForMods + i;
                if (actualButtonIndex < totalButtonsOnLogicalPage)
                {
                    buttonsToDisplayThisFrameList.Add(allButtonsOnCurrentLogicalPage[actualButtonIndex]);
                }
                else break;
            }

            if (current_display_page < totalDisplayPagesRequired - 1)
            {
                buttonsToDisplayThisFrameList.Add(new Button { title = "Page >", tooltip = "Next Page", isToggleable = false, action = () => ChangeDisplayPage(1) });
            }

            Button[] currentFrameDisplayableButtons_local = buttonsToDisplayThisFrameList.ToArray();

            if (index >= currentFrameDisplayableButtons_local.Length && currentFrameDisplayableButtons_local.Length > 0)
                index = currentFrameDisplayableButtons_local.Length - 1;
            else if (currentFrameDisplayableButtons_local.Length == 0 && index != 0)
                index = 0;
            else if (index < 0 && currentFrameDisplayableButtons_local.Length > 0)
                index = 0;

            UpdateAdjustableMenuFocus(currentFrameDisplayableButtons_local);

            if (Time.time >= cooldown && currentFrameDisplayableButtons_local.Length > 0)
            {
                if (ControllerInputs.RightStickUp() || UnityInput.Current.GetKeyDown(KeyCode.UpArrow))
                {
                    if (index > 0) index--; else index = currentFrameDisplayableButtons_local.Length - 1;
                    cooldown = Time.time + 0.20f;
                    UpdateAdjustableMenuFocus(currentFrameDisplayableButtons_local);
                }
                if (ControllerInputs.RightStickDown() || UnityInput.Current.GetKeyDown(KeyCode.DownArrow))
                {
                    if (index < currentFrameDisplayableButtons_local.Length - 1) index++; else index = 0;
                    cooldown = Time.time + 0.20f;
                    UpdateAdjustableMenuFocus(currentFrameDisplayableButtons_local);
                }
                if (ControllerInputs.rightStick() || UnityInput.Current.GetKeyDown(KeyCode.Return) || UnityInput.Current.GetKeyDown(KeyCode.RightArrow))
                {
                    if (index >= 0 && index < currentFrameDisplayableButtons_local.Length)
                    {
                        Button selectedButton = currentFrameDisplayableButtons_local[index];

                        if (selectedButton.action != null)
                        {
                            selectedButton.action();
                        }

                        if (selectedButton.isToggleable)
                        {
                            selectedButton.toggled = !selectedButton.toggled;
                            if (!selectedButton.toggled && selectedButton.disableAction != null)
                            {
                                selectedButton.disableAction();
                            }
                        }
                        cooldown = Time.time + 0.20f;
                        UpdateAdjustableMenuFocus(currentFrameDisplayableButtons_local);
                    }
                }
            }

            string display = $"<size={text.fontSize * 1.2f}><color={Color32ToHTML(Settings.theme)}>{Settings.title} (DP {current_display_page + 1}/{Math.Max(1, totalDisplayPagesRequired)})</color></size>\n";
            if (currentFrameDisplayableButtons_local.Length == 0)
            {
                display += (totalButtonsOnLogicalPage == 0 && current_display_page == 0) ? "No items on this page." : " (Empty View)";
            }
            else
            {
                for (int i = 0; i < currentFrameDisplayableButtons_local.Length; i++)
                {
                    display += $"{(i == index ? "-> " : "   ")}{currentFrameDisplayableButtons_local[i].title} ";
                    if (currentFrameDisplayableButtons_local[i].isToggleable)
                        display += currentFrameDisplayableButtons_local[i].toggled ? $"<color={Color32ToHTML(Settings.theme)}>[ON]</color>" : "<color=red>[OFF]</color>";
                    display += "\n";
                }
            }
            text.text = display;

            Transform headTransform = GorillaTagger.Instance.headCollider.transform;
            float mainVerticalOffset = -0.35f; // Your specified positioning
            parent.transform.position = headTransform.position + (headTransform.forward * 2.75f) + (headTransform.up * mainVerticalOffset);
            parent.transform.rotation = headTransform.rotation;

            Button toolTipsButton = FindButtonByTitle("Tool Tips");
            if (toolTipsButton != null) { if (toolTipsButton.toggled && toggled) ToolTips.Load(); else ToolTips.Cleanup(); }

            Button pingCounterButton = FindButtonByTitle("Ping Counter");
            if (pingCounterButton != null) { if (pingCounterButton.toggled && toggled) PingCounter.Load(); else PingCounter.Cleanup(); }

            Button wallWalkButton = FindButtonByTitle("Wall Walk");
            if (wallWalkButton != null && typeof(WallWalkMod).GetMethod("UpdateStatusUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
            {
                WallWalkMod.UpdateStatusUI();
            }

            AdjustableMenu.UpdateUI();
        }

        // Reverted UpdateAdjustableMenuFocus
        private static void UpdateAdjustableMenuFocus(Button[] currentFrameButtons = null)
        {
            Button currentlyHoveredButtonForAdjustableMenu = null;
            Button buttonForTooltipAndTitle = null;

            if (toggled && currentFrameButtons != null && currentFrameButtons.Length > 0 && index >= 0 && index < currentFrameButtons.Length)
            {
                buttonForTooltipAndTitle = currentFrameButtons[index];

                if (!buttonForTooltipAndTitle.title.Contains("Page"))
                {
                    currentlyHoveredButtonForAdjustableMenu = buttonForTooltipAndTitle;
                }
            }

            if (buttonForTooltipAndTitle != null)
            {
                CurrentSelectedButtonTitle = buttonForTooltipAndTitle.title;
                CurrentSelectedButtonTooltip = buttonForTooltipAndTitle.tooltip;
            }
            else
            {
                CurrentSelectedButtonTitle = string.Empty;
                CurrentSelectedButtonTooltip = string.Empty;
            }
            AdjustableMenu.SetFocusedMod(currentlyHoveredButtonForAdjustableMenu);
        }


        private static void UpdateCurrentPage(int target_page_index)
        {
            if (target_page_index >= 0 && target_page_index < pages.Count)
            {
                page_index = target_page_index;
                current_display_page = 0;
                index = 0;
                cooldown = Time.time + 0.2f;
                UpdateAdjustableMenuFocus(null);
            }
        }

        private static Button FindButtonByTitle(string title)
        {
            foreach (var page in pages)
            {
                foreach (var button in page)
                {
                    if (button.title == title)
                        return button;
                }
            }
            return null;
        }

        public static string Color32ToHTML(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }

        public static void FullCleanup()
        {
            if (parent != null) GameObject.Destroy(parent);
            parent = null;
            text = null;
            ToolTips.Cleanup();
            PingCounter.Cleanup();
            AdjustableMenu.Cleanup();
            if (typeof(WallWalkMod).GetMethod("UpdateStatusUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
            {
                WallWalkMod.UpdateStatusUI();
            }
            pages.Clear();
            toggled = false;
        }
    }
}