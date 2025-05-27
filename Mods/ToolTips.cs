using Photon.Pun;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TextUITemplate.Libraries;
using TextUITemplate.Management; // Required for Menu.CurrentSelectedButtonTitle, etc.
using TMPro;
using UnityEngine;

namespace TextUITemplate.Mods
{
    public class ToolTips
    {
        private static GameObject parent = null;
        private static TextMeshPro text = null;

        public static void Load()
        {
            try // Added try-catch for robust error logging
            {
                if (parent == null)
                {
                    Interfaces.Create("Tool Tips", ref parent, ref text, TextAlignmentOptions.BottomLeft);
                }

                if (parent != null && !parent.activeSelf)
                {
                    parent.SetActive(true);
                }

                if (text == null && parent != null) // Attempt to get TextMeshPro if it's null but parent exists
                {
                    text = parent.GetComponent<TextMeshPro>();
                    if (text == null)
                    {
                        Debug.LogError("[ToolTips.Load] TextMeshPro component is null even after trying to get it.");
                        if (parent != null && parent.activeSelf) parent.SetActive(false); // Hide if unusable
                        return;
                    }
                }
                else if (text == null && parent == null) // If both are null, can't proceed
                {
                    Debug.LogError("[ToolTips.Load] Parent and TextMeshPro component are null.");
                    return;
                }


                if (!string.IsNullOrEmpty(Menu.CurrentSelectedButtonTooltip))
                {
                    string titleToShow = !string.IsNullOrEmpty(Menu.CurrentSelectedButtonTitle) ? Menu.CurrentSelectedButtonTitle : "Info";
                    string tooltipDisplay = $"<size=0.7><color={Menu.Color32ToHTML(Settings.theme)}>{titleToShow}</color></size>\n{Menu.CurrentSelectedButtonTooltip}";

                    if (text.text != tooltipDisplay)
                        text.text = tooltipDisplay;
                }
                else
                {
                    if (text.text != "")
                        text.text = "";
                }

                if (text.renderer != null && text.renderer.material != null && text.renderer.material.shader != Shader.Find("GUI/Text Shader"))
                {
                    text.renderer.material.shader = Shader.Find("GUI/Text Shader");
                }

                if (GorillaTagger.Instance != null && GorillaTagger.Instance.headCollider != null && parent != null)
                {
                    parent.transform.position = GorillaTagger.Instance.headCollider.transform.position + GorillaTagger.Instance.headCollider.transform.forward * 2.75f;
                    parent.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ToolTips.Load] Error: {e.Message}\n{e.StackTrace}");
                if (parent != null && parent.activeSelf) parent.SetActive(false); // Hide on error
            }
        }

        public static void Cleanup()
        {
            if (parent != null)
            {
                if (parent.activeSelf)
                    parent.SetActive(false);
            }
        }
    }
}