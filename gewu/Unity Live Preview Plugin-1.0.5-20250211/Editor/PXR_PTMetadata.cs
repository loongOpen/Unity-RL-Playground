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

#if XR_MGMT_GTE_320

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace Unity.XR.PICO.LivePreview.Editor
{
    internal class PXR_PTMetadata : IXRPackage
    {
        private class PXR_PTPackageMetadata : IXRPackageMetadata
        {
            public string packageName => "PICO Live Preview";
            public string packageId => "com.unity.pico.livepreview";
            public string settingsType => "Unity.XR.PICO.LivePreview.PXR_PTSettings";
            public List<IXRLoaderMetadata> loaderMetadata => lLoaderMetadata;

            private static readonly List<IXRLoaderMetadata> lLoaderMetadata = new List<IXRLoaderMetadata>() { new PXR_PTLoaderMetadata() };
        }

        private class PXR_PTLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName => "PICO Live Preview";
            public string loaderType => "Unity.XR.PICO.LivePreview.PXR_PTLoader";
            public List<BuildTargetGroup> supportedBuildTargets => SupportedBuildTargets;

            private static readonly List<BuildTargetGroup> SupportedBuildTargets = new List<BuildTargetGroup>()
            {
                BuildTargetGroup.Standalone
            };
        }

        private static IXRPackageMetadata Metadata = new PXR_PTPackageMetadata();
        public IXRPackageMetadata metadata => Metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            var settings = obj as PXR_PTSettings;
            if (settings != null)
            {
                return true;
            }
            return false;
        }
    }
}

#endif
