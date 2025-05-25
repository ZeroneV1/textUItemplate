using Photon.Pun;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextUITemplate.Libraries;
using TextUITemplate.Management;
using TMPro;
using UnityEngine;

namespace TextUITemplate.Mods
{
    public class PingCounter
    {
        private static GameObject parent = null;
        private static TextMeshPro text = null;

        public static void Load()
        {
            if (parent == null)
                Interfaces.Create("Ping Counter", ref parent, ref text, TextAlignmentOptions.TopLeft);
            else
            {
                if (!parent.activeSelf)
                    parent.SetActive(true);
                else
                {
                    string ping = $"<size=0.7>{Mathf.RoundToInt(PhotonNetwork.GetPing())} <color={Menu.Color32ToHTML(Settings.theme)}>ms</color></size>";

                    if (PlayFabClientAPI.IsClientLoggedIn())
                        if (text.text != ping)
                            text.text = ping;

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
