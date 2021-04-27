using Rufus31415.WebXR;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UInput = UnityEngine.Input;

namespace Rufus31415.MixedReality.Toolkit.WebXR.Input
{

    public class OnScreenHelper : MonoBehaviour
    {
        private GUIStyle _style;

        void Start()
        {
            _style = new GUIStyle();
            _style.richText = true;
            _style.alignment = TextAnchor.UpperLeft;
        }

        private float _lastScrollTime;

        private void OnGUI()
        {
            if (SimpleWebXR.InSession) return;

            if (UInput.mouseScrollDelta.y != 0) _lastScrollTime = Time.time;

            var wheelScroll = (Time.time - _lastScrollTime) < 1;


            var txt = new StringBuilder();

            txt.Append("    <b>Help</b>\r\nMove camera with arrows : ");
            Append(txt, "LEFT", UInput.GetKey(KeyCode.LeftArrow));
            txt.Append(", ");
            Append(txt, "RIGHT", UInput.GetKey(KeyCode.RightArrow));
            txt.Append(", ");
            Append(txt, "UP", UInput.GetKey(KeyCode.UpArrow));
            txt.Append(", ");
            Append(txt, "DOWN", UInput.GetKey(KeyCode.DownArrow));

            txt.Append("\r\nRotate camera : ");
            Append(txt, "Right Mouse Button", UInput.GetMouseButton(1));

            txt.Append("\r\nMove hands : ");
            Append(txt, "Left Shift", UInput.GetKey(KeyCode.LeftShift));
            txt.Append(", ");
            Append(txt, "Space Bar", UInput.GetKey(KeyCode.Space));

            txt.Append("\r\nToggle hands : ");
            Append(txt, "T ", UInput.GetKey(KeyCode.T));
            txt.Append(", ");
            Append(txt, "Y", UInput.GetKey(KeyCode.Y));

            txt.Append("\r\nMove hand : ");
            Append(txt, "Left Shift + Wheel", UInput.GetKey(KeyCode.LeftShift) && wheelScroll);

            txt.Append("\r\nRotate hand : ");
            Append(txt, "Left Shift + Left CTRL + Wheel", UInput.GetKey(KeyCode.LeftShift) && UInput.GetKey(KeyCode.LeftControl) && wheelScroll);

            txt.Append("\r\nPinch / Select : ");
            Append(txt, "Left Mouse Button", UInput.GetMouseButton(0));

            GUI.Label(new Rect(0,0,Screen.width, Screen.height), txt.ToString());
        }

        private void Append(StringBuilder txt, string text, bool pressed)
        {
            if (pressed)
            {
                txt.Append("<color=#03fcbe><size=18><b>");
                txt.Append(text);
                txt.Append("</b></size></color>");
            }
            else
            {
                txt.Append(text);
            }
        }
    }
}