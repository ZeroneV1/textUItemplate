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
            try
            {
                if (parent == null)
                {
                    Interfaces.Create("Ping Counter", ref parent, ref text, TextAlignmentOptions.TopLeft);
                }

                if (parent != null && !parent.activeSelf)
                {
                    parent.SetActive(true);
                }

                if (text == null && parent != null)
                {
                    text = parent.GetComponent<TextMeshPro>();
                    if (text == null)
                    {
                        Debug.LogError("[PingCounter.Load] TextMeshPro component is null even after trying to get it.");
                        if (parent != null && parent.activeSelf) parent.SetActive(false);
                        return;
                    }
                }
                else if (text == null && parent == null)
                {
                    Debug.LogError("[PingCounter.Load] Parent and TextMeshPro component are null.");
                    return;
                }

                string pingDisplayText;
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) // Check if in a room for more reliable ping
                {
                    pingDisplayText = $"<size=0.7>{Mathf.RoundToInt(PhotonNetwork.GetPing())} <color={Menu.Color32ToHTML(Settings.theme)}>ms</color></size>";
                }
                else if (PhotonNetwork.IsConnected) // Connected to master or nameserver, but not in a room
                {
                    pingDisplayText = "<size=0.7>Ping: Connecting...</size>";
                }
                else
                {
                    pingDisplayText = "<size=0.7>Ping: Offline</size>";
                }

                if (text.text != pingDisplayText)
                {
                    text.text = pingDisplayText;
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
                Debug.LogError($"[PingCounter.Load] Error: {e.Message}\n{e.StackTrace}");
                if (parent != null && parent.activeSelf) parent.SetActive(false);
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