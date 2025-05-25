using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private static bool toggled = true;
        public static int page_index = 0;
        public static int index = 0;
        private static float cooldown;

        public static List<Button[]> pages = new List<Button[]>();

        private static int current_display_page = 0;
        private const int MODS_PER_PAGE = 8;

        // Properties for ToolTips.cs to access currently selected item info
        public static string CurrentSelectedButtonTitle { get; private set; } = string.Empty;
        public static string CurrentSelectedButtonTooltip { get; private set; } = string.Empty;
        public static bool IsMenuToggledOn => toggled; // Expose menu toggle state


        public static void Start()
        {
            List<Button> mainPageButtons = new List<Button>
            {
                new Button { title = "Example Page", tooltip = "Navigate to a secondary example page of buttons.", isToggleable = false, action = () => UpdateCurrentPage(1) },
                new Button { title = "Ping Counter", tooltip = "Shows your current network ping in the top-left.", toggled = true, isToggleable = true, action = () => PingCounter.Load(), disableAction = () => PingCounter.Cleanup() },
                new Button { title = "Tool Tips", tooltip = "Toggle the display of descriptions for selected menu items at the bottom-left.", toggled = true, isToggleable = true, action = () => ToolTips.Load(), disableAction = () => ToolTips.Cleanup() },
                new Button { title = "Wall Walk", tooltip = "Enables wall walking. Be careful, might be detectable!", toggled = false, isToggleable = true, action = () => WallWalkMod.Load(), disableAction = () => WallWalkMod.Cleanup() }
            };

            for (int i = 1; i <= 10; i++)
            {
                int buttonNum = i;
                mainPageButtons.Add(new Button { title = $"Mod Example {buttonNum}", tooltip = $"This is the description for Mod Example number {buttonNum}.", isToggleable = true, toggled = false, action = () => Debug.Log($"Mod {buttonNum} Action"), disableAction = () => Debug.Log($"Mod {buttonNum} Disable") });
            }
            pages.Add(mainPageButtons.ToArray());

            pages.Add(new Button[]
            {
                new Button { title = "Example Toggle", tooltip = "A toggleable option on page 2.", toggled = false, isToggleable = true, },
                new Button { title = "Example Button", tooltip = "A simple button on page 2.", toggled = false, isToggleable = false, },
                new Button { title = "Back", tooltip = "Return to the main list of mods.", isToggleable = false, action = () => UpdateCurrentPage(0) },
            });
        }

        private static void ChangeDisplayPage(int direction)
        {
            current_display_page += direction;
            index = 0;
            cooldown = Time.time + 0.2f;
        }

        public static void Load()
        {
            if (GorillaTagger.hasInstance)
            {
                if (parent == null)
                {
                    Interfaces.Create("Menu", ref parent, ref text, TextAlignmentOptions.TopRight);
                }

                WallWalkMod.DoWallWalkLogic(); // Assuming this handles its own active state

                if (text.renderer.material.shader != Shader.Find("GUI/Text Shader"))
                    text.renderer.material.shader = Shader.Find("GUI/Text Shader");

                Button[] allButtonsForCurrentMenuScreen = pages[page_index];
                List<Button> buttonsToDisplayThisFrame = new List<Button>();
                int totalButtonsInMenuScreen = allButtonsForCurrentMenuScreen.Length;
                int totalDisplayPagesRequired = (int)Math.Ceiling((double)totalButtonsInMenuScreen / MODS_PER_PAGE);

                if (current_display_page > 0)
                {
                    buttonsToDisplayThisFrame.Add(new Button { title = "< Page", tooltip = "Previous Page", isToggleable = false, action = () => ChangeDisplayPage(-1) });
                }
                int startIndexForMods = current_display_page * MODS_PER_PAGE;
                for (int i = 0; i < MODS_PER_PAGE; i++)
                {
                    int actualButtonIndex = startIndexForMods + i;
                    if (actualButtonIndex < totalButtonsInMenuScreen)
                    {
                        buttonsToDisplayThisFrame.Add(allButtonsForCurrentMenuScreen[actualButtonIndex]);
                    }
                    else break;
                }
                if (current_display_page < totalDisplayPagesRequired - 1)
                {
                    buttonsToDisplayThisFrame.Add(new Button { title = "Page >", tooltip = "Next Page", isToggleable = false, action = () => ChangeDisplayPage(1) });
                }
                Button[] currentFrameDisplayableButtons = buttonsToDisplayThisFrame.ToArray();

                // Update static properties for external use (e.g., ToolTips.cs)
                if (toggled && currentFrameDisplayableButtons.Length > 0 && index < currentFrameDisplayableButtons.Length)
                {
                    CurrentSelectedButtonTitle = currentFrameDisplayableButtons[index].title;
                    CurrentSelectedButtonTooltip = currentFrameDisplayableButtons[index].tooltip;
                }
                else
                {
                    CurrentSelectedButtonTitle = string.Empty;
                    CurrentSelectedButtonTooltip = string.Empty;
                }


                if (toggled)
                {
                    if (!parent.activeSelf) parent.SetActive(true);

                    if (index >= currentFrameDisplayableButtons.Length && currentFrameDisplayableButtons.Length > 0)
                        index = currentFrameDisplayableButtons.Length - 1;
                    else if (currentFrameDisplayableButtons.Length == 0)
                        index = 0;

                    if (ControllerInputs.LeftStickUp() || UnityInput.Current.GetKey(KeyCode.UpArrow))
                    {
                        if (Time.time >= cooldown && currentFrameDisplayableButtons.Length > 0)
                        {
                            if (index > 0) index--; else index = currentFrameDisplayableButtons.Length - 1;
                            cooldown = Time.time + 0.25f;
                        }
                    }
                    if (ControllerInputs.LeftStickDown() || UnityInput.Current.GetKey(KeyCode.DownArrow))
                    {
                        if (Time.time >= cooldown && currentFrameDisplayableButtons.Length > 0)
                        {
                            if (index + 1 < currentFrameDisplayableButtons.Length) index++; else index = 0;
                            cooldown = Time.time + 0.25f;
                        }
                    }
                    if (ControllerInputs.leftStick() || UnityInput.Current.GetKey(KeyCode.RightArrow))
                    {
                        if (Time.time >= cooldown && currentFrameDisplayableButtons.Length > 0 && index < currentFrameDisplayableButtons.Length)
                        {
                            Button selectedButton = currentFrameDisplayableButtons[index];
                            bool originalToggleState = selectedButton.toggled;
                            if (selectedButton.isToggleable)
                            {
                                selectedButton.toggled = !selectedButton.toggled;
                            }

                            // Always call action if it exists.
                            // For non-toggleable, it's the primary action.
                            // For toggleable, it can be an enabling action or just a click action.
                            if (selectedButton.action != null)
                            {
                                // If it's toggleable and had a specific disable action,
                                // only call action if it's being turned ON or is not toggleable.
                                if (!selectedButton.isToggleable || (selectedButton.isToggleable && selectedButton.toggled))
                                {
                                    selectedButton.action();
                                }
                            }
                            // If it was toggled OFF, and has a disableAction, call it.
                            if (selectedButton.isToggleable && !selectedButton.toggled && originalToggleState)
                            {
                                if (selectedButton.disableAction != null) selectedButton.disableAction();
                            }
                            cooldown = Time.time + 0.25f;
                        }
                    }
                }
                else
                {
                    if (parent.activeSelf) parent.SetActive(false);
                }

                if (ControllerInputs.rightStick() || UnityInput.Current.GetKey(KeyCode.Tab))
                {
                    if (Time.time >= cooldown) { toggled = !toggled; cooldown = Time.time + 0.25f; }
                }

                string display = $"<size={text.fontSize * 1.5f}><color={Color32ToHTML(Settings.theme)}>{Settings.title} (Page {current_display_page + 1}/{Math.Max(1, totalDisplayPagesRequired)})</color></size>\n";
                if (currentFrameDisplayableButtons.Length == 0)
                {
                    display += (totalButtonsInMenuScreen == 0) ? "No items in this menu." : "Error: No items for this page.";
                }
                else
                {
                    for (int i = 0; i < currentFrameDisplayableButtons.Length; i++)
                    {
                        display += $"{((i == index) ? "-> " : string.Empty)}{currentFrameDisplayableButtons[i].title} ";
                        if (currentFrameDisplayableButtons[i].isToggleable)
                            display += currentFrameDisplayableButtons[i].toggled ? $"<color={Color32ToHTML(Settings.theme)}>[ON]</color>" : "<color=red>[OFF]</color>";
                        display += "\n";
                    }
                }
                text.text = display;
                parent.transform.position = GorillaTagger.Instance.headCollider.transform.position + GorillaTagger.Instance.headCollider.transform.forward * 2.75f;
                parent.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                // Handle continuous mods like ToolTips and PingCounter
                // Assuming "Tool Tips" and "Ping Counter" buttons are on the first page (pages[0])
                if (pages.Count > 0 && pages[0] != null)
                {
                    Button toolTipsButton = Array.Find(pages[0], btn => btn.title == "Tool Tips");
                    if (toolTipsButton != null)
                    {
                        if (toolTipsButton.toggled && toggled) // Also check if main menu is toggled
                        {
                            ToolTips.Load();
                        }
                        else
                        {
                            ToolTips.Cleanup();
                        }
                    }

                    Button pingCounterButton = Array.Find(pages[0], btn => btn.title == "Ping Counter");
                    if (pingCounterButton != null)
                    {
                        if (pingCounterButton.toggled && toggled) // Also check if main menu is toggled
                        {
                            PingCounter.Load();
                        }
                        else
                        {
                            PingCounter.Cleanup();
                        }
                    }
                }
            }
            else // GorillaTagger not found, ensure UIs are off
            {
                if (parent != null && parent.activeSelf) parent.SetActive(false);
                ToolTips.Cleanup();
                PingCounter.Cleanup();
            }
        }

        private static void UpdateCurrentPage(int target_page_index)
        {
            if (target_page_index >= 0 && target_page_index < pages.Count)
            {
                page_index = target_page_index;
                current_display_page = 0;
                index = 0;
            }
        }

        public static string Color32ToHTML(Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }
    }
}