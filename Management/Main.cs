using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTag.Cosmetics;
using GorillaTagScripts;
using GorillaTagScripts.ObstacleCourse;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextUITemplate.Management // Kept the original namespace
{
    // Token: 0x0200008B RID: 139
    [HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    public partial class Main : MonoBehaviour
    {
        // Token: 0x06000631 RID: 1585 RVA: 0x000515C4 File Offset: 0x0004F7C4
        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            Quaternion quaternion = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.leftHandRotOffset;

            return (
                position: GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.leftHandOffset,
                rotation: quaternion,
                up: quaternion * Vector3.up,
                forward: quaternion * Vector3.forward,
                right: quaternion * Vector3.right
            );
        }

        // Token: 0x06000632 RID: 1586 RVA: 0x00051650 File Offset: 0x0004F850
        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion quaternion = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.rightHandRotOffset;

            return (
                position: GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.rightHandOffset,
                rotation: quaternion,
                up: quaternion * Vector3.up,
                forward: quaternion * Vector3.forward,
                right: quaternion * Vector3.right
            );
        }
    }
}