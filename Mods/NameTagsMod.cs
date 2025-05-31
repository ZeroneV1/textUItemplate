using GorillaNetworking; // For VRRig, GorillaTagger, GorillaParent, NetPlayer
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using TextUITemplate.Management; // For Settings
using UnityEngine;
using GorillaGameModes; // For GorillaGameManager and GorillaTagManager

namespace TextUITemplate.Mods
{
    public class NameTagsMod
    {
        private static bool isOverallModActive = false;

        // State for individual tag types
        private static bool showStandardNameTags = false;
        private static bool showVelocityTags = false;
        private static bool showFPSTags = false;
        private static bool showTurnTags = false;
        private static bool showTaggedByTags = false;

        // Dictionaries for managing GameObjects
        private static Dictionary<VRRig, GameObject> standardNameTagObjects = new Dictionary<VRRig, GameObject>();
        private static Dictionary<VRRig, GameObject> velocityTagObjects = new Dictionary<VRRig, GameObject>();
        private static Dictionary<VRRig, GameObject> fpsTagObjects = new Dictionary<VRRig, GameObject>();
        private static Dictionary<VRRig, GameObject> turnTagObjects = new Dictionary<VRRig, GameObject>();
        private static Dictionary<VRRig, GameObject> taggedByTagObjects = new Dictionary<VRRig, GameObject>();

        private static Dictionary<VRRig, List<int>> ntDistanceList = new Dictionary<VRRig, List<int>>();

        // Overall Mod Enable/Disable (called by Menu button)
        public static void EnableMod() { isOverallModActive = true; }
        public static void DisableMod()
        {
            isOverallModActive = false;
            if (showStandardNameTags) SetShowStandardNameTags(false);
            if (showVelocityTags) SetShowVelocityTags(false);
            if (showFPSTags) SetShowFPSTags(false);
            if (showTurnTags) SetShowTurnTags(false);
            if (showTaggedByTags) SetShowTaggedByTags(false);
        }
        public static bool IsModActive() { return isOverallModActive; }

        // --- Standard Name Tags ---
        public static void SetShowStandardNameTags(bool enabled)
        {
            showStandardNameTags = enabled;
            if (!enabled) CleanupStandardNameTags();
        }
        public static bool IsStandardNameTagsActive() { return showStandardNameTags; }

        // --- Velocity Tags ---
        public static void SetShowVelocityTags(bool enabled)
        {
            showVelocityTags = enabled;
            if (!enabled) CleanupVelocityTags();
        }
        public static bool IsVelocityTagsActive() { return showVelocityTags; }

        // --- FPS Tags ---
        public static void SetShowFPSTags(bool enabled)
        {
            showFPSTags = enabled;
            if (!enabled) CleanupFPSTags();
        }
        public static bool IsFPSTagsActive() { return showFPSTags; }

        // --- Turn Tags ---
        public static void SetShowTurnTags(bool enabled)
        {
            showTurnTags = enabled;
            if (!enabled) CleanupTurnTags();
        }
        public static bool IsTurnTagsActive() { return showTurnTags; }

        // --- Tagged By Tags ---
        public static void SetShowTaggedByTags(bool enabled)
        {
            showTaggedByTags = enabled;
            if (!enabled) CleanupTaggedByTags();
        }
        public static bool IsTaggedByTagsActive() { return showTaggedByTags; }


        private static float GetTagDistance(VRRig rig)
        {
            if (ntDistanceList.ContainsKey(rig))
            {
                if (ntDistanceList[rig].Count > 0 && ntDistanceList[rig][0] == Time.frameCount)
                {
                    ntDistanceList[rig].Add(Time.frameCount);
                    return 0.25f + (ntDistanceList[rig].Count * 0.15f);
                }
                else
                {
                    ntDistanceList[rig].Clear();
                    ntDistanceList[rig].Add(Time.frameCount);
                    return 0.25f + (ntDistanceList[rig].Count * 0.15f);
                }
            }
            else
            {
                ntDistanceList.Add(rig, new List<int> { Time.frameCount });
                return 0.4f;
            }
        }

        private static void ResetTagDistancesForFrame()
        {
            List<VRRig> keysToClearFully = new List<VRRig>();
            foreach (var entry in ntDistanceList)
            {
                if (entry.Key == null)
                {
                    keysToClearFully.Add(entry.Key);
                    continue;
                }
                if (entry.Value.Count > 0 && entry.Value[0] != Time.frameCount)
                {
                    entry.Value.Clear();
                }
            }
            foreach (VRRig key in keysToClearFully)
            {
                ntDistanceList.Remove(key);
            }
        }

        public static void UpdateAllNameTagsLogic()
        {
            if (!isOverallModActive || GorillaTagger.Instance == null || GorillaParent.instance == null)
            {
                if (!isOverallModActive)
                {
                    if (showStandardNameTags) CleanupStandardNameTags();
                    if (showVelocityTags) CleanupVelocityTags();
                    if (showFPSTags) CleanupFPSTags();
                    if (showTurnTags) CleanupTurnTags();
                    if (showTaggedByTags) CleanupTaggedByTags();
                }
                return;
            }

            ResetTagDistancesForFrame();

            if (showStandardNameTags) UpdateStandardNameTags(); else CleanupStandardNameTags();
            if (showVelocityTags) UpdateVelocityTags(); else CleanupVelocityTags();
            if (showFPSTags) UpdateFPSTags(); else CleanupFPSTags();
            if (showTurnTags) UpdateTurnTags(); else CleanupTurnTags();
            if (showTaggedByTags) UpdateTaggedByTags(); else CleanupTaggedByTags();
        }

        private static GameObject CreateNameTagGameObject(string prefix)
        {
            GameObject go = new GameObject(prefix + "_Nametag_TextUITemplate");
            go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            TextMesh textMesh = go.AddComponent<TextMesh>();
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontStyle = FontStyle.Bold;

            Renderer renderer = textMesh.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                if (renderer.material.shader != Shader.Find("GUI/Text Shader"))
                {
                    renderer.material.shader = Shader.Find("GUI/Text Shader");
                }
            }
            return go;
        }

        private static void UpdateTag(VRRig vrrig, GameObject nameTagObject, string textContent)
        {
            if (vrrig == null || nameTagObject == null || Camera.main == null) return;
            if (vrrig == GorillaTagger.Instance.offlineVRRig)
            {
                nameTagObject.SetActive(false);
                return;
            }
            nameTagObject.SetActive(true);

            TextMesh textMesh = nameTagObject.GetComponent<TextMesh>();
            textMesh.text = textContent;
            textMesh.color = vrrig.playerColor;

            nameTagObject.transform.position = vrrig.headMesh.transform.position + (Vector3.up * GetTagDistance(vrrig));
            nameTagObject.transform.LookAt(Camera.main.transform.position);
            nameTagObject.transform.Rotate(0f, 180f, 0f);
        }

        private static void CleanupOldRigs(Dictionary<VRRig, GameObject> tagObjects)
        {
            if (GorillaParent.instance == null) return;

            List<VRRig> toRemove = new List<VRRig>();
            foreach (var pair in tagObjects)
            {
                if (pair.Key == null || !GorillaParent.instance.vrrigs.Contains(pair.Key))
                {
                    if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
                    toRemove.Add(pair.Key);
                }
            }
            foreach (VRRig rig in toRemove)
            {
                tagObjects.Remove(rig);
            }
        }

        // --- Standard Name Tags Logic ---
        private static void UpdateStandardNameTags()
        {
            CleanupOldRigs(standardNameTagObjects);
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig == null || vrrig.Creator == null) continue;
                if (vrrig == GorillaTagger.Instance.offlineVRRig) continue;
                if (!standardNameTagObjects.ContainsKey(vrrig))
                {
                    standardNameTagObjects.Add(vrrig, CreateNameTagGameObject("StandardName"));
                }
                UpdateTag(vrrig, standardNameTagObjects[vrrig], vrrig.Creator.NickName.ToString());
            }
        }
        private static void CleanupStandardNameTags()
        {
            foreach (var pair in standardNameTagObjects) if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            standardNameTagObjects.Clear();
        }

        // --- Velocity Tags Logic ---
        private static void UpdateVelocityTags()
        {
            CleanupOldRigs(velocityTagObjects);
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig == null) continue;
                if (vrrig == GorillaTagger.Instance.offlineVRRig) continue;
                if (!velocityTagObjects.ContainsKey(vrrig))
                {
                    velocityTagObjects.Add(vrrig, CreateNameTagGameObject("Velocity"));
                }
                UpdateTag(vrrig, velocityTagObjects[vrrig], string.Format("{0:F1}m/s", vrrig.LatestVelocity().magnitude));
            }
        }
        private static void CleanupVelocityTags()
        {
            foreach (var pair in velocityTagObjects) if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            velocityTagObjects.Clear();
        }

        // --- FPS Tags Logic ---
        private static void UpdateFPSTags()
        {
            CleanupOldRigs(fpsTagObjects);
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig == null) continue;
                if (vrrig == GorillaTagger.Instance.offlineVRRig) continue;
                if (!fpsTagObjects.ContainsKey(vrrig))
                {
                    fpsTagObjects.Add(vrrig, CreateNameTagGameObject("FPS"));
                }
                UpdateTag(vrrig, fpsTagObjects[vrrig], "FPS: N/A");
            }
        }
        private static void CleanupFPSTags()
        {
            foreach (var pair in fpsTagObjects) if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            fpsTagObjects.Clear();
        }

        // --- Turn Tags Logic ---
        private static void UpdateTurnTags()
        {
            CleanupOldRigs(turnTagObjects);
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig == null) continue;
                if (vrrig == GorillaTagger.Instance.offlineVRRig) continue;
                if (!turnTagObjects.ContainsKey(vrrig))
                {
                    turnTagObjects.Add(vrrig, CreateNameTagGameObject("Turn"));
                }
                UpdateTag(vrrig, turnTagObjects[vrrig], "Turn: N/A");
            }
        }
        private static void CleanupTurnTags()
        {
            foreach (var pair in turnTagObjects) if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            turnTagObjects.Clear();
        }

        // --- Tagged By Tags Logic ---
        private static void UpdateTaggedByTags()
        {
            CleanupOldRigs(taggedByTagObjects);
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig == null || vrrig.Creator == null) continue;
                if (vrrig == GorillaTagger.Instance.offlineVRRig) continue;

                if (!taggedByTagObjects.ContainsKey(vrrig))
                {
                    taggedByTagObjects.Add(vrrig, CreateNameTagGameObject("TaggedBy"));
                }

                string taggerText = ""; // Corrected variable name
                if (PlayerIsTagged(vrrig))
                {
                    // Default to "Tagged" if specific tagger info isn't available or needed
                    taggerText = "Tagged";

                    if (GorillaGameManager.instance != null && GorillaGameManager.instance is GorillaTagManager)
                    {
                        GorillaTagManager tagManager = GorillaGameManager.instance as GorillaTagManager;
                        if (tagManager != null && tagManager.currentIt == vrrig.Creator)
                        {
                            taggerText = "IT";
                        }
                        // The 'TryGetPlayerHitBy' and 'infectedPlayers' logic from original is removed 
                        // as those are not standard. We rely on currentIt and material checks in PlayerIsTagged.
                    }
                }
                UpdateTag(vrrig, taggedByTagObjects[vrrig], taggerText);
            }
        }
        private static void CleanupTaggedByTags()
        {
            foreach (var pair in taggedByTagObjects) if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            taggedByTagObjects.Clear();
        }

        private static string ToTitleCase(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        private static bool PlayerIsTagged(VRRig rig)
        {
            if (rig == null || rig.mainSkin == null || rig.mainSkin.material == null)
                return false;

            string materialName = rig.mainSkin.material.name.ToLower();
            if (materialName.Contains("fected") || materialName.Contains("it"))
                return true;

            if (PhotonNetwork.InRoom && GorillaGameManager.instance != null)
            {
                if (GorillaGameManager.instance is GorillaTagManager)
                {
                    GorillaTagManager tagManager = (GorillaTagManager)GorillaGameManager.instance;
                    // Check if this rig's player is the current "it"
                    if (tagManager.currentIt != null && rig.Creator != null && tagManager.currentIt == rig.Creator)
                        return true;
                    // Removed check for tagManager.infectedPlayers as it's not standard
                }
            }
            return false;
        }
    }
}