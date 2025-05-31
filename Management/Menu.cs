using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using TextUITemplate.Libraries;
using TextUITemplate.Mods; // Make sure this is present
using TMPro;
using UnityEngine;
using GorillaTag;
using Photon.Pun;

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
        private const int MODS_PER_PAGE = 7; // Number of mods to show before needing pagination within a logical page

        public static string CurrentSelectedButtonTitle { get; private set; } = string.Empty;
        public static string CurrentSelectedButtonTooltip { get; private set; } = string.Empty;
        public static bool IsMenuToggledOn => toggled;

        private const string DISCONNECT_BUTTON_TITLE = "Disconnect";
        private const string DISCONNECT_BUTTON_TOOLTIP = "Disconnects from the current Photon room.";

        public static void Start()
        {
            pages.Clear();

            // Initialize mods that need it before menu setup, if any (e.g., WeatherMod)
            // if (typeof(WeatherMod).GetMethod("InitializeOrRefreshState", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
            // {
            // WeatherMod.InitializeOrRefreshState();
            // }


            // Page 0: Main Mods
            List<Button> mainPageButtons = new List<Button>
            {
                new Button {
                    title = "Weather Control",
                    tooltip = "Toggle custom weather. Settings via Left Stick.",
                    isToggleable = true,
                    toggled = WeatherMod.IsModCurrentlyActive(),
                    action = () => WeatherMod.SetModActive(true),
                    disableAction = () => WeatherMod.SetModActive(false),
                    ModAdjustableValues = new List<AdjustableValue>
                    {
                        new AdjustableValue
                        {
                            Name = "Time of Day",
                            ValType = AdjustableValueType.Int,
                            GetValue = () => WeatherMod.CurrentTimeOptionIndex,
                            SetValue = (val) => WeatherMod.ApplyTimeFromIndex((int)val),
                            OnValueChanged = (newVal) => WeatherMod.ApplyTimeFromIndex((int)newVal),
                            MinValue = 0,
                            MaxValue = WeatherMod.TimeOptions.Length - 1,
                            IntIncrementStep = 1
                        },
                        new AdjustableValue
                        {
                            Name = "Enable Rain",
                            ValType = AdjustableValueType.Bool,
                            GetValue = () => WeatherMod.IsRainDesired(),
                            SetValue = (val) => WeatherMod.SetRain((bool)val),
                            OnValueChanged = (val) => WeatherMod.SetRain((bool)val)
                        }
                    }
                },
                new Button { // NAMETAGS MOD BUTTON
                    title = "Nametags",
                    tooltip = "Toggle and configure player nametags.",
                    isToggleable = true,
                    toggled = NameTagsMod.IsModActive(), // Get initial state
                    action = () => NameTagsMod.EnableMod(),
                    disableAction = () => NameTagsMod.DisableMod(),
                    ModAdjustableValues = new List<AdjustableValue>
                    {
                        new AdjustableValue
                        {
                            Name = "Show Standard Tags",
                            ValType = AdjustableValueType.Bool,
                            GetValue = () => NameTagsMod.IsStandardNameTagsActive(),
                            SetValue = (val) => NameTagsMod.SetShowStandardNameTags((bool)val),
                            OnValueChanged = (val) => NameTagsMod.SetShowStandardNameTags((bool)val)
                        },
                        new AdjustableValue
                        {
                            Name = "Show Velocity Tags",
                            ValType = AdjustableValueType.Bool,
                            GetValue = () => NameTagsMod.IsVelocityTagsActive(),
                            SetValue = (val) => NameTagsMod.SetShowVelocityTags((bool)val),
                            OnValueChanged = (val) => NameTagsMod.SetShowVelocityTags((bool)val)
                        },
                        new AdjustableValue
                        {
                            Name = "Show FPS Tags",
                            ValType = AdjustableValueType.Bool,
                            GetValue = () => NameTagsMod.IsFPSTagsActive(),
                            SetValue = (val) => NameTagsMod.SetShowFPSTags((bool)val),
                            OnValueChanged = (val) => NameTagsMod.SetShowFPSTags((bool)val)
                        },
                        new AdjustableValue
                        {
                            Name = "Show Turn Tags",
                            ValType = AdjustableValueType.Bool,
                            GetValue = () => NameTagsMod.IsTurnTagsActive(),
                            SetValue = (val) => NameTagsMod.SetShowTurnTags((bool)val),
                            OnValueChanged = (val) => NameTagsMod.SetShowTurnTags((bool)val)
                        },
                        new AdjustableValue
                        {
                            Name = "Show Tagged By Tags",
                            ValType = AdjustableValueType.Bool,
                            GetValue = () => NameTagsMod.IsTaggedByTagsActive(),
                            SetValue = (val) => NameTagsMod.SetShowTaggedByTags((bool)val),
                            OnValueChanged = (val) => NameTagsMod.SetShowTaggedByTags((bool)val)
                        }
                    }
                },
                new Button { title = "Example Page", tooltip = "Navigate to another page.", isToggleable = false, action = () => UpdateCurrentPage(1) },
                new Button { title = "Ping Counter", tooltip = "Shows network ping.", toggled = true, isToggleable = true, action = () => PingCounter.Load(), disableAction = () => PingCounter.Cleanup() },
                new Button { title = "Tool Tips", tooltip = "Displays item descriptions.", toggled = true, isToggleable = true, action = () => ToolTips.Load(), disableAction = () => ToolTips.Cleanup() },
                new Button { title = "Wall Walk", tooltip = "Enables wall walking.", toggled = false, isToggleable = true, action = () => WallWalkMod.Enable(), disableAction = () => WallWalkMod.Disable() },
                new Button {
                    title = "Tag Reach",
                    tooltip = "Extends tag reach (edit with left stick). Active when tagged & right grab held.",
                    toggled = false, isToggleable = true, action = () => TagReachMod.Enable(), disableAction = () => TagReachMod.Disable(),
                    ModAdjustableValues = new List<AdjustableValue>
                    {
                        new AdjustableValue { Name = "Tag Reach Radius", ValType = AdjustableValueType.Float, GetValue = () => TagReachMod.MODDED_TAG_REACH_DISTANCE, SetValue = (val) => { TagReachMod.MODDED_TAG_REACH_DISTANCE = (float)val; }, OnValueChanged = (newVal) => TagReachMod.VisualizeReachTemporarily((float)newVal), MinValue = 0.1f, MaxValue = 2.0f, FloatIncrementStep = 0.05f }
                    }
                },
                new Button {
                    title = "Speed Boost",
                    tooltip = "Makes you speedy. Edit values or apply presets with left stick.",
                    toggled = false, isToggleable = true, action = () => SpeedBoost.Enable(), disableAction = () => SpeedBoost.Disable(),
                    ModAdjustableValues = new List<AdjustableValue> {
                        new AdjustableValue { Name = "Max Jump Speed", ValType = AdjustableValueType.Float, GetValue = () => SpeedBoost.MODDED_MAX_JUMP_SPEED, SetValue = (val) => { SpeedBoost.MODDED_MAX_JUMP_SPEED = (float)val; }, OnValueChanged = (newVal) => { SpeedBoost.RefreshSpeedBoostValues(); }, MinValue = 1.0f, MaxValue = 20.0f, FloatIncrementStep = 0.1f },
                        new AdjustableValue { Name = "Jump Multiplier", ValType = AdjustableValueType.Float, GetValue = () => SpeedBoost.MODDED_JUMP_MULTIPLIER, SetValue = (val) => { SpeedBoost.MODDED_JUMP_MULTIPLIER = (float)val; }, OnValueChanged = (newVal) => { SpeedBoost.RefreshSpeedBoostValues(); }, MinValue = 0.1f, MaxValue = 5.0f, FloatIncrementStep = 0.05f }
                    },
                    Presets = new List<ModPreset> {
                        new ModPreset { Name = "Default Modded", Values = new Dictionary<string, object> { { "Max Jump Speed", 7.5f }, { "Jump Multiplier", 1.4f } } },
                        new ModPreset { Name = "High Jump", Values = new Dictionary<string, object> { { "Max Jump Speed", 10.0f }, { "Jump Multiplier", 2.0f } } }
                    }
                },
                new Button { title = "Long Jump", tooltip = "Hold Right Primary (A/X or E) for a boost.", toggled = false, isToggleable = true, action = () => LongJumpMod.Enable(), disableAction = () => LongJumpMod.Disable() }
            };
            pages.Add(mainPageButtons.ToArray());

            // Page 1: Example Page (or your second page of mods)
            pages.Add(new Button[]
            {
                new Button { title = "Placeholder Mod 3", tooltip = "Another button.", toggled = false, isToggleable = true },
                new Button { title = "Back to Main Mods", tooltip = "Return to the main list of mods.", isToggleable = false, action = () => UpdateCurrentPage(0) },
            });
        }

        private static void UpdateCurrentPage(int target_page_index)
        {
            if (target_page_index >= 0 && target_page_index < pages.Count)
            {
                page_index = target_page_index;
                current_display_page = 0;
                index = 0;
                cooldown = Time.time + 0.2f;
                Button[] currentFrameButtons = GetCurrentFrameDisplayableButtons();
                UpdateAdjustableMenuFocus(currentFrameButtons, index);
            }
            else
            {
                Debug.LogError($"Menu.UpdateCurrentPage: Attempted to navigate to invalid page index {target_page_index}. Max pages: {pages.Count}");
            }
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
            if (current_display_page < 0) current_display_page = 0; // Ensure it's not negative if totalDisplayPagesRequired is 0


            index = 0;
            cooldown = Time.time + 0.2f;
            Button[] currentFrameDisplayableButtons = GetCurrentFrameDisplayableButtons();
            UpdateAdjustableMenuFocus(currentFrameDisplayableButtons, index);
        }

        private static Button[] GetCurrentFrameDisplayableButtons()
        {
            Button[] allButtonsOnCurrentLogicalPage = (pages.Count > page_index && page_index >= 0) ? pages[page_index] : new Button[0];
            List<Button> buttonsToDisplayThisFrameList = new List<Button>();
            int totalButtonsOnLogicalPage = allButtonsOnCurrentLogicalPage.Length;
            int itemsPerPageToConsiderForPagination = MODS_PER_PAGE;
            int totalDisplayPagesRequired = (totalButtonsOnLogicalPage > 0) ? (int)Math.Ceiling((double)totalButtonsOnLogicalPage / itemsPerPageToConsiderForPagination) : 1;
            if (totalDisplayPagesRequired == 0 && totalButtonsOnLogicalPage == 0) totalDisplayPagesRequired = 1; // Handle empty logical page

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
            return buttonsToDisplayThisFrameList.ToArray();
        }

        private static bool GetActualModState(string buttonTitle)
        {
            if (buttonTitle == "Weather Control") return WeatherMod.IsModCurrentlyActive();
            if (buttonTitle == "Wall Walk") return WallWalkMod.IsCurrentlyActive();
            if (buttonTitle == "Tag Reach") return TagReachMod.IsModActive();
            if (buttonTitle == "Speed Boost") return SpeedBoost.IsActive();
            if (buttonTitle == "Long Jump") return LongJumpMod.IsModActive();
            if (buttonTitle == "Nametags") return NameTagsMod.IsModActive();

            Button btn = FindButtonByTitle(buttonTitle);
            return btn != null ? btn.toggled : false;
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
                parent.GetComponent<RectTransform>().sizeDelta = new Vector2(2.2f, 2.8f); // Increased height
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

                    Button[] currentFrameButtons = GetCurrentFrameDisplayableButtons();
                    UpdateAdjustableMenuFocus(currentFrameButtons, index);
                    if (!toggled)
                    {
                        ToolTips.Cleanup();
                        PingCounter.Cleanup();
                        AdjustableMenu.Cleanup();
                        if (typeof(WallWalkMod).GetMethod("UpdateStatusUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null) WallWalkMod.UpdateStatusUI();
                    }
                }
            }

            parent.SetActive(toggled);
            AdjustableMenu.UpdateUI();

            if (!toggled)
            {
                return;
            }

            Button[] currentFrameDisplayableButtons_local = GetCurrentFrameDisplayableButtons();
            int numRegularButtons = currentFrameDisplayableButtons_local.Length;
            int totalSelectableUIItems = numRegularButtons + 1;

            if (totalSelectableUIItems > 0)
            {
                if (index >= totalSelectableUIItems) index = totalSelectableUIItems - 1;
                if (index < 0) index = 0;
            }
            else
            {
                index = 0;
            }


            if (Time.time >= cooldown && totalSelectableUIItems > 0)
            {
                bool navigated = false;
                if (ControllerInputs.RightStickUp() || UnityInput.Current.GetKeyDown(KeyCode.UpArrow))
                {
                    if (index > 0) index--; else index = totalSelectableUIItems - 1;
                    navigated = true;
                }
                else if (ControllerInputs.RightStickDown() || UnityInput.Current.GetKeyDown(KeyCode.DownArrow))
                {
                    if (index < totalSelectableUIItems - 1) index++; else index = 0;
                    navigated = true;
                }

                if (navigated)
                {
                    cooldown = Time.time + 0.20f;
                    UpdateAdjustableMenuFocus(currentFrameDisplayableButtons_local, index);
                }
                else if (ControllerInputs.rightStick() || UnityInput.Current.GetKeyDown(KeyCode.Return) || UnityInput.Current.GetKeyDown(KeyCode.RightArrow))
                {
                    if (index >= 0 && index < totalSelectableUIItems)
                    {
                        if (index < numRegularButtons)
                        {
                            Button selectedButton = currentFrameDisplayableButtons_local[index];
                            if (selectedButton.isToggleable)
                            {
                                bool currentActualModState = GetActualModState(selectedButton.title);
                                if (currentActualModState)
                                {
                                    selectedButton.disableAction?.Invoke();
                                }
                                else
                                {
                                    selectedButton.action?.Invoke();
                                }
                            }
                            else
                            {
                                selectedButton.action?.Invoke();
                            }
                        }
                        else
                        {
                            if (PhotonNetwork.IsConnected)
                            {
                                PhotonNetwork.Disconnect();
                            }
                            toggled = false;
                            ToolTips.Cleanup();
                            PingCounter.Cleanup();
                            AdjustableMenu.Cleanup();
                            if (typeof(WallWalkMod).GetMethod("UpdateStatusUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null) WallWalkMod.UpdateStatusUI();
                        }
                        cooldown = Time.time + 0.20f;
                        UpdateAdjustableMenuFocus(currentFrameDisplayableButtons_local, index);
                    }
                }
            }

            Button[] allButtonsOnCurrentLogicalPage_display = (pages.Count > page_index && page_index >= 0) ? pages[page_index] : new Button[0];
            int totalButtonsOnLogicalPage_display = allButtonsOnCurrentLogicalPage_display.Length;
            int itemsPerPageToConsiderForPagination_display = MODS_PER_PAGE;
            int totalDisplayPagesRequired_display = (totalButtonsOnLogicalPage_display > 0) ? (int)Math.Ceiling((double)totalButtonsOnLogicalPage_display / itemsPerPageToConsiderForPagination_display) : 1;
            if (totalDisplayPagesRequired_display == 0 && totalButtonsOnLogicalPage_display == 0) totalDisplayPagesRequired_display = 1;


            string display = $"<size={text.fontSize * 1.2f}><color={Color32ToHTML(Settings.theme)}>{Settings.title} (Page {current_display_page + 1}/{Math.Max(1, totalDisplayPagesRequired_display)})</color></size>\n";

            if (numRegularButtons == 0)
            {
                if (totalButtonsOnLogicalPage_display == 0 && current_display_page == 0) display += "No items on this page.\n";
                else display += " (Empty View)\n";
            }

            for (int i = 0; i < numRegularButtons; i++)
            {
                Button currentButtonToDisplay = currentFrameDisplayableButtons_local[i];
                bool actualModStateForDisplay = currentButtonToDisplay.toggled;
                if (currentButtonToDisplay.isToggleable)
                {
                    actualModStateForDisplay = GetActualModState(currentButtonToDisplay.title);
                    currentButtonToDisplay.toggled = actualModStateForDisplay;
                }

                display += $"{(i == index ? "-> " : "   ")}{currentButtonToDisplay.title} ";
                if (currentButtonToDisplay.isToggleable)
                    display += actualModStateForDisplay ? $"<color={Color32ToHTML(Settings.theme)}>[ON]</color>" : "<color=red>[OFF]</color>";
                display += "\n";
            }

            display += $"{(index == numRegularButtons ? "-> " : "   ")}{DISCONNECT_BUTTON_TITLE}\n";

            text.text = display;

            Transform headTransform = GorillaTagger.Instance.headCollider.transform;
            float mainVerticalOffset = -0.45f;
            parent.transform.position = headTransform.position + (headTransform.forward * 2.75f) + (headTransform.up * mainVerticalOffset);
            parent.transform.rotation = headTransform.rotation;

            Button toolTipsButton = FindButtonByTitle("Tool Tips");
            if (toolTipsButton != null) { if (GetActualModState("Tool Tips") && toggled) ToolTips.Load(); else ToolTips.Cleanup(); }

            Button pingCounterButton = FindButtonByTitle("Ping Counter");
            if (pingCounterButton != null) { if (GetActualModState("Ping Counter") && toggled) PingCounter.Load(); else PingCounter.Cleanup(); }

            Button wallWalkButton = FindButtonByTitle("Wall Walk");
            if (wallWalkButton != null && typeof(WallWalkMod).GetMethod("UpdateStatusUI", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static) != null)
            {
                WallWalkMod.UpdateStatusUI();
            }
        }

        private static void UpdateAdjustableMenuFocus(Button[] modButtonsOnCurrentFrame, int currentSelectionIndex)
        {
            Button currentlyHoveredButtonForAdjustableMenu = null;
            string selectedTitle = string.Empty;
            string selectedTooltip = string.Empty;

            if (toggled)
            {
                if (modButtonsOnCurrentFrame != null && currentSelectionIndex >= 0 && currentSelectionIndex < modButtonsOnCurrentFrame.Length)
                {
                    Button buttonForTooltipAndTitle = modButtonsOnCurrentFrame[currentSelectionIndex];
                    if (buttonForTooltipAndTitle != null)
                    {
                        selectedTitle = buttonForTooltipAndTitle.title;
                        selectedTooltip = buttonForTooltipAndTitle.tooltip;

                        if (!buttonForTooltipAndTitle.title.Contains("< Page") && !buttonForTooltipAndTitle.title.Contains("Page >") &&
                            (buttonForTooltipAndTitle.HasAdjustableValues || buttonForTooltipAndTitle.HasPresets))
                        {
                            currentlyHoveredButtonForAdjustableMenu = buttonForTooltipAndTitle;
                        }
                    }
                }
                else if (currentSelectionIndex == (modButtonsOnCurrentFrame?.Length ?? 0)) // If Disconnect is selected
                {
                    selectedTitle = DISCONNECT_BUTTON_TITLE;
                    selectedTooltip = DISCONNECT_BUTTON_TOOLTIP;
                    currentlyHoveredButtonForAdjustableMenu = null;
                }
            }

            CurrentSelectedButtonTitle = selectedTitle;
            CurrentSelectedButtonTooltip = selectedTooltip;
            AdjustableMenu.SetFocusedMod(currentlyHoveredButtonForAdjustableMenu);
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
                if (WallWalkMod.IsCurrentlyActive()) WallWalkMod.Disable();
                WallWalkMod.UpdateStatusUI();
            }
            if (NameTagsMod.IsModActive()) NameTagsMod.DisableMod(); // Cleanup Nametags mod
            toggled = false;
            index = 0;
            current_display_page = 0;
        }
    }
}