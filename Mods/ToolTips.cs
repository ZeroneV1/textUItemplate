using Photon.Pun;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TextUITemplate.Libraries;
using TextUITemplate.Management;
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
            if (parent == null)
                Interfaces.Create("Tool Tips", ref parent, ref text, TextAlignmentOptions.BottomLeft);
            else
            {
                if (!parent.activeSelf)
                    parent.SetActive(true);
                else
                {
                    string tooltip = $"<size=0.7><color={Menu.Color32ToHTML(Settings.theme)}>{Menu.pages[Menu.page_index][Menu.index].title}</color></size>\n{Menu.pages[Menu.page_index][Menu.index].tooltip}";

                    if (text.text != tooltip)
                        text.text = tooltip;

                    if (text.renderer.material.shader != Shader.Find("GUI/Text Shader"))
                        text.renderer.material.shader = Shader.Find("GUI/Text Shader");

                    parent.transform.position = GorillaTagger.Instance.headCollider.transform.position + GorillaTagger.Instance.headCollider.transform.forward * 2.75f;
                    parent.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
                }
            }
        }

        public static void Cleanup()
        {
            if (parent != null)
                if (parent.activeSelf)
                    parent.SetActive(false);
        }
    }
}
