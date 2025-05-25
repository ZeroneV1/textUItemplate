using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace TextUITemplate.Libraries
{
    public class ControllerInputs
    {
        // Define a common threshold for joystick direction sensitivity
        private const float joystickDirectionThreshold = 0.7f;

        public static bool leftStick()
        {
            bool output;
            if (GameObject.Find("[SteamVR]") != null)
                output = SteamVR_Actions.gorillaTag_LeftJoystickClick.GetState(SteamVR_Input_Sources.LeftHand);
            else
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out output);
            return output;
        }

        public static bool leftGrip()
        {
            return ControllerInputPoller.instance.leftGrab;
        }

        public static bool leftTrigger()
        {
            return ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f;
        }

        public static bool leftPrimary()
        {
            return ControllerInputPoller.instance.leftControllerPrimaryButton;
        }

        public static bool leftSecondary()
        {
            return ControllerInputPoller.instance.leftControllerSecondaryButton;
        }

        public static bool rightStick()
        {
            bool output;
            if (GameObject.Find("[SteamVR]") != null)
                output = SteamVR_Actions.gorillaTag_RightJoystickClick.GetState(SteamVR_Input_Sources.RightHand);
            else
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out output);
            return output;
        }

        public static bool rightGrip()
        {
            return ControllerInputPoller.instance.rightGrab;
        }

        public static bool rightTrigger()
        {
            return ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
        }

        public static bool rightPrimary()
        {
            return ControllerInputPoller.instance.rightControllerPrimaryButton;
        }

        public static bool rightSecondary()
        {
            return ControllerInputPoller.instance.rightControllerSecondaryButton;
        }

        // Joystick Direction Methods with updated threshold

        public static bool LeftStickUp()
        {
            return ControllerInputPoller.instance.leftControllerPrimary2DAxis.y >= joystickDirectionThreshold;
        }

        public static bool LeftStickDown()
        {
            return ControllerInputPoller.instance.leftControllerPrimary2DAxis.y <= -joystickDirectionThreshold;
        }

        public static bool LeftStickLeft()
        {
            return ControllerInputPoller.instance.leftControllerPrimary2DAxis.x <= -joystickDirectionThreshold;
        }

        public static bool LeftStickRight()
        {
            return ControllerInputPoller.instance.leftControllerPrimary2DAxis.x >= joystickDirectionThreshold;
        }

        public static bool RightStickUp()
        {
            return ControllerInputPoller.instance.rightControllerPrimary2DAxis.y >= joystickDirectionThreshold;
        }

        public static bool RightStickDown()
        {
            return ControllerInputPoller.instance.rightControllerPrimary2DAxis.y <= -joystickDirectionThreshold;
        }

        public static bool RightStickLeft()
        {
            return ControllerInputPoller.instance.rightControllerPrimary2DAxis.x <= -joystickDirectionThreshold;
        }

        public static bool RightStickRight()
        {
            return ControllerInputPoller.instance.rightControllerPrimary2DAxis.x >= joystickDirectionThreshold;
        }
    }
}