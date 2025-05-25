using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TextUITemplate.Libraries
{
    public class Interfaces
    {
        public static void Create(string name, ref GameObject parent, ref TextMeshPro text, TextAlignmentOptions alignment)
        {
            parent = new GameObject(name);
            text = parent.AddComponent<TextMeshPro>();

            RectTransform transform = parent.GetComponent<RectTransform>();
            transform.sizeDelta = new Vector2(1.75f, 1.75f);

            text.lineSpacing = 25f;
            text.font = GameObject.Find("motdtext").GetComponent<TextMeshPro>().font;
            text.alignment = alignment;
            text.fontSize = 0.5f;

            parent.transform.LookAt(Camera.main.transform);
            parent.transform.Rotate(0f, 180f, 0f);
        }
    }
}
