/*******************************************************************************
Copyright © 2015-2022 PICO Technology Co., Ltd.All rights reserved.  

NOTICE：All information contained herein is, and remains the property of 
PICO Technology Co., Ltd. The intellectual and technical concepts 
contained hererin are proprietary to PICO Technology Co., Ltd. and may be 
covered by patents, patents in process, and are protected by trade secret or 
copyright law. Dissemination of this information or reproduction of this 
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd. 
*******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using UnityEngine.XR;

#if UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using Unity.XR.PICO.LivePreview.Input;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if XR_HANDS
using UnityEngine.XR.Hands;
#endif


namespace Unity.XR.PICO.LivePreview
{
#if UNITY_INPUT_SYSTEM
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    static class InputLayoutLoader
    {
        static InputLayoutLoader()
        {
            RegisterInputLayouts();
        }

        public static void RegisterInputLayouts()
        {
            InputSystem.RegisterLayout<PXR_PTHMD>(matches: new InputDeviceMatcher().WithInterface(XRUtilities.InterfaceMatchAnyVersion).WithProduct(@"^(PICO Live Preview HMD)"));
            InputSystem.RegisterLayout<PXR_PTController>(matches: new InputDeviceMatcher().WithInterface(XRUtilities.InterfaceMatchAnyVersion).WithProduct(@"^(PICO Live Preview Controller)"));
        }
    }
#endif

    public class PXR_PTLoader : XRLoaderHelper
#if UNITY_EDITOR
    , IXRLoaderPreInit
#endif
    {
        private static List<XRDisplaySubsystemDescriptor> displaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
        private static List<XRInputSubsystemDescriptor> inputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();
#if XR_HANDS
        private static List<XRHandSubsystemDescriptor> handSubsystemDescriptors = new List<XRHandSubsystemDescriptor>();
#endif

        public XRDisplaySubsystem displaySubsystem
        {
            get
            {
                return GetLoadedSubsystem<XRDisplaySubsystem>();
            }
        }

        public XRInputSubsystem inputSubsystem
        {
            get
            {
                return GetLoadedSubsystem<XRInputSubsystem>();
            }
        }

        public override bool Initialize()
        {
#if UNITY_INPUT_SYSTEM
            InputLayoutLoader.RegisterInputLayouts();
#endif

            PXR_PTApi.UPxr_PTSetSRPState(GraphicsSettings.currentRenderPipeline != null);

            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(displaySubsystemDescriptors, "PICO LP Display");
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(inputSubsystemDescriptors, "PICO LP Input");
#if XR_HANDS
            CreateSubsystem<XRHandSubsystemDescriptor, XRHandSubsystem>(handSubsystemDescriptors, "PICO LP Hands");
#endif

            if (displaySubsystem == null && inputSubsystem == null)
            {
                Debug.LogError("PXRLog Unable to start PICO Plugin.");
            }
            else if (displaySubsystem == null)
            {
                Debug.LogError("PXRLog Failed to load display subsystem.");
            }
            else if (inputSubsystem == null)
            {
                Debug.LogError("PXRLog Failed to load input subsystem.");
            }

#if XR_HANDS
            var handSubSystem = GetLoadedSubsystem<XRHandSubsystem>();
            if (handSubSystem == null)
            {
                Debug.LogError("PXRLog Failed to load XRHandSubsystem.");
            }
#endif

            return displaySubsystem != null;
        }

        public override bool Start()
        {
            PXR_PTApi.UPxr_PTSetSRPState(GraphicsSettings.currentRenderPipeline != null);
            StartSubsystem<XRDisplaySubsystem>();
            StartSubsystem<XRInputSubsystem>();
#if XR_HANDS
            StartSubsystem<XRHandSubsystem>();
#endif

            Application.targetFrameRate = 72;
            return true;
        }

        public override bool Stop()
        {
            StopSubsystem<XRDisplaySubsystem>();
            StopSubsystem<XRInputSubsystem>();
#if XR_HANDS
            StopSubsystem<XRHandSubsystem>();
#endif
            return true;
        }

        public override bool Deinitialize()
        {
            DestroySubsystem<XRDisplaySubsystem>();
            DestroySubsystem<XRInputSubsystem>();
#if XR_HANDS
            DestroySubsystem<XRHandSubsystem>();
#endif
            return true;
        }

#if UNITY_EDITOR
        public string GetPreInitLibraryName(BuildTarget buildTarget, BuildTargetGroup buildTargetGroup)
        {
            return "PxrLivePreview";
        }
#endif
    }
}
