/*
* Copyright (c) 2012-2018 AssimpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

/* Note 2019
* 
* This file is modified by Dongho Kang to distributed as a Unity package 2019.
*/ 

using Assimp.Unmanaged;
using System.IO;
using UnityEngine;

namespace Assimp
{
    /// <summary>
    /// AssimpNet Unity integration. This handles one-time initialization (before scene load) of the AssimpLibrary instance, setting DLL probing paths to load the correct native
    /// dependencies, if the current platform is supported.
    /// </summary>
    public class AssimpUnity
    {
        private const string packageName = "com.unity.robotics.urdf-importer";
        
        private static bool s_triedLoading = false;
        private static bool s_assimpAvailable = false;

        /// <summary>
        /// Gets if the assimp library is available on this platform (e.g. the library can load native dependencies).
        /// </summary>
        public static bool IsAssimpAvailable
        {
            get
            {
                return s_assimpAvailable;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializePlugin()
        {
            //Only try once during runtime
            if (s_triedLoading)
                return;

            UnmanagedLibrary libInstance = AssimpLibrary.Instance;

            //If already initialized, set flags and return
            if (libInstance.IsLibraryLoaded)
            {
                s_assimpAvailable = true;
                s_triedLoading = true;
                return;
            }

            //First time initialization, need to set a probing path (at least in editor) to resolve the native dependencies
            string pluginsFolder = Path.Combine(Application.dataPath, "Plugins");
            string editorPluginNativeFolder = Path.Combine(Path.GetFullPath(string.Format($"Packages/{packageName}")), "Runtime", "UnityMeshImporter", "Plugins", "AssimpNet", "Native");
            string native64LibPath = null;
            string native32LibPath = null;

            //Set if any platform needs to tweak the default name AssimpNet uses for the platform, null clears using an override at all
            string override64LibName = null;
            string override32LibName = null;

            //Setup DLL paths based on platforms. When run inside the editor, the path will be to the AssimpNet plugin folder structure. When in standalone,
            //Unity copies the native DLLs for the specific target architecture into a single Plugin folder.
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    native64LibPath = Path.Combine(editorPluginNativeFolder, "win", "x86_64");
                    native32LibPath = Path.Combine(editorPluginNativeFolder, "win", "x86");
                    break;
                case RuntimePlatform.WindowsPlayer:
                    native64LibPath = pluginsFolder + "/x86_64";
                    native32LibPath = pluginsFolder + "/x86";
                    break;
                case RuntimePlatform.LinuxEditor:
                    native64LibPath = Path.Combine(editorPluginNativeFolder, "linux", "x86_64");
                    native32LibPath = Path.Combine(editorPluginNativeFolder, "linux", "x86");
                    break;
                case RuntimePlatform.LinuxPlayer:
                    //Linux also drop package plugins inside Plugins folder
                    native64LibPath = pluginsFolder;
                    native32LibPath = pluginsFolder;
                    break;
                case RuntimePlatform.OSXEditor:
                    native64LibPath = Path.Combine(editorPluginNativeFolder, "osx", "x86_64");
                    native32LibPath = Path.Combine(editorPluginNativeFolder, "osx", "x86");

                    //In order to get unity to accept the dylib, had to rename it as *.bundle. Set an override name so we try and load that file. Seems to load
                    //fine.
                {
                    string bundlelibName = Path.ChangeExtension(libInstance.DefaultLibraryName, ".bundle");
                    override64LibName = bundlelibName;
                    override32LibName = bundlelibName;
                }
                    break;
                case RuntimePlatform.OSXPlayer:
                    native64LibPath = pluginsFolder;
                    native32LibPath = pluginsFolder;

                    //In order to get unity to accept the dylib, had to rename it as *.bundle. Set an override name so we try and load that file. Seems to load
                    //fine.
                {
                    string bundlelibName = Path.ChangeExtension(libInstance.DefaultLibraryName, ".bundle");
                    override64LibName = bundlelibName;
                    override32LibName = bundlelibName;
                }
                    break;
                //TODO: Add more platforms if you have binaries that can run on it
            }

            //If both null, then we do not support the platform
            if (native64LibPath == null && native32LibPath == null)
            {
                Debug.Log(string.Format("Assimp does not support platform: {0}", Application.platform.ToString()));
                s_assimpAvailable = false;
                s_triedLoading = true;
                return;
            }

            // Check if library paths exist (especially important for Linux Editor)
            if (Application.platform == RuntimePlatform.LinuxEditor)
            {
                if (native64LibPath != null && !Directory.Exists(native64LibPath))
                {
                    Debug.LogWarning($"Assimp: Native library path does not exist: {native64LibPath}. Assimp features will be unavailable.");
                    s_assimpAvailable = false;
                    s_triedLoading = true;
                    return;
                }
            }

            //Set resolver properties, null will clear the property
            libInstance.Resolver.SetOverrideLibraryName64(override64LibName);
            libInstance.Resolver.SetOverrideLibraryName32(override32LibName);
            libInstance.Resolver.SetProbingPaths64(native64LibPath);
            libInstance.Resolver.SetProbingPaths32(native32LibPath);
            libInstance.ThrowOnLoadFailure = false;

            //Try and load the native library, if failed we won't get an exception
            bool success = false;
            try
            {
                success = libInstance.LoadLibrary();
            }
            catch (System.DllNotFoundException ex)
            {
                // Silently handle DllNotFoundException (e.g., libdl.so not found on Linux)
                // This can happen if system libraries are not available or if the native library
                // dependencies cannot be resolved
                Debug.LogWarning($"Assimp: Failed to load native library (DllNotFoundException: {ex.Message}). Assimp features will be unavailable. This is normal if system libraries (e.g., libdl.so) are not accessible.");
                success = false;
            }
            catch (System.Exception ex)
            {
                // Log other exceptions but don't crash
                Debug.LogWarning($"Assimp: Error loading native library: {ex.GetType().Name}: {ex.Message}. Assimp features will be unavailable.");
                success = false;
            }
            
            s_assimpAvailable = success;
            s_triedLoading = true;

            //Turn exceptions back on
            libInstance.ThrowOnLoadFailure = true;
        }
    }
}
