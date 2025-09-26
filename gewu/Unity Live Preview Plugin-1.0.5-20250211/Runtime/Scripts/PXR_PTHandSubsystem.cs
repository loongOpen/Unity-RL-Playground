using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using System.Runtime.CompilerServices;
using UnityEngine.XR.Management;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;

#if XR_HANDS
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace Unity.XR.PICO.LivePreview
{
    [Preserve]
    /// <summary>
    /// Implement Unity XRHandSubSystem 
    /// Reference: https://docs.unity3d.com/Packages/com.unity.xr.hands@1.4/manual/implement-a-provider.html
    /// </summary>
    public class PXR_PTHandSubsystem : XRHandSubsystem
    {
        XRHandProviderUtility.SubsystemUpdater m_Updater;

        // This method registers the subsystem descriptor with the SubsystemManager
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
            {
                id = "PICO LP Hands",
                providerType = typeof(PXR_PTHandSubsystemProvider),
                subsystemTypeOverride = typeof(PXR_PTHandSubsystem)
            };
            XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Updater = new XRHandProviderUtility.SubsystemUpdater(this);
        }

        protected override void OnStart()
        {
            Debug.Log("PXR_HandSubSystem Start");
            m_Updater.Start();
            base.OnStart();
        }

        protected override void OnStop()
        {
            m_Updater.Stop();
            base.OnStop();
        }

        protected override void OnDestroy()
        {
            m_Updater.Destroy();
            m_Updater = null;
            base.OnDestroy();
        }

        class PXR_PTHandSubsystemProvider : XRHandSubsystemProvider
        {

            HandJointLocations jointLocations = new HandJointLocations();
            readonly HandLocationStatus AllStatus = HandLocationStatus.PositionTracked | HandLocationStatus.PositionValid |
                          HandLocationStatus.OrientationTracked | HandLocationStatus.OrientationValid;

            bool isValid = false;

            public override void Start()
            {
                CreateHands();
            }

            public override void Stop()
            {
                DestroyHands();
            }

            public override void Destroy()
            {

            }

            /// <summary>
            /// Mapping the PICO Joint Index To Unity Joint Index
            /// </summary>
            static int[] pxrJointIndexToUnityJointIndexMapping;

            static void Initialize()
            {
                if (pxrJointIndexToUnityJointIndexMapping == null)
                {
                    pxrJointIndexToUnityJointIndexMapping = new int[(int)HandJoint.JointMax];
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointPalm] = XRHandJointID.Palm.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointWrist] = XRHandJointID.Wrist.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointThumbMetacarpal] = XRHandJointID.ThumbMetacarpal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointThumbProximal] = XRHandJointID.ThumbProximal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointThumbDistal] = XRHandJointID.ThumbDistal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointThumbTip] = XRHandJointID.ThumbTip.ToIndex();

                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointIndexMetacarpal] = XRHandJointID.IndexMetacarpal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointIndexProximal] = XRHandJointID.IndexProximal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointIndexIntermediate] = XRHandJointID.IndexIntermediate.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointIndexDistal] = XRHandJointID.IndexDistal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointIndexTip] = XRHandJointID.IndexTip.ToIndex();


                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointMiddleMetacarpal] = XRHandJointID.MiddleMetacarpal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointMiddleProximal] = XRHandJointID.MiddleProximal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointMiddleIntermediate] = XRHandJointID.MiddleIntermediate.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointMiddleDistal] = XRHandJointID.MiddleDistal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointMiddleTip] = XRHandJointID.MiddleTip.ToIndex();

                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointRingMetacarpal] = XRHandJointID.RingMetacarpal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointRingProximal] = XRHandJointID.RingProximal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointRingIntermediate] = XRHandJointID.RingIntermediate.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointRingDistal] = XRHandJointID.RingDistal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointRingTip] = XRHandJointID.RingTip.ToIndex();

                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointLittleMetacarpal] = XRHandJointID.LittleMetacarpal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointLittleProximal] = XRHandJointID.LittleProximal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointLittleIntermediate] = XRHandJointID.LittleIntermediate.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointLittleDistal] = XRHandJointID.LittleDistal.ToIndex();
                    pxrJointIndexToUnityJointIndexMapping[(int)HandJoint.JointLittleTip] = XRHandJointID.LittleTip.ToIndex();
                }
            }

            /// <summary>
            /// Gets the layout of hand joints for this provider, by having the
            /// provider mark each index corresponding to a <see cref="XRHandJointID"/>
            /// get marked as <see langword="true"/> if the provider attempts to track
            /// that joint.
            /// </summary>
            /// <remarks>
            /// Called once on creation so that before the subsystem is even started,
            /// so the user can immediately create a valid hierarchical structure as
            /// soon as they get a reference to the subsystem without even needing to
            /// start it.
            /// </remarks>
            /// <param name="handJointsInLayout">
            /// Each index corresponds to a <see cref="XRHandJointID"/>. For each
            /// joint that the provider will attempt to track, mark that spot as
            /// <see langword="true"/> by calling <c>.ToIndex()</c> on that ID.
            /// </param>
            public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
            {

                Initialize();
                handJointsInLayout[XRHandJointID.Palm.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.Wrist.ToIndex()] = true;

                handJointsInLayout[XRHandJointID.ThumbMetacarpal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.ThumbProximal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.ThumbDistal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.ThumbTip.ToIndex()] = true;

                handJointsInLayout[XRHandJointID.IndexMetacarpal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.IndexProximal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.IndexIntermediate.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.IndexDistal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.IndexTip.ToIndex()] = true;

                handJointsInLayout[XRHandJointID.MiddleMetacarpal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.MiddleProximal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.MiddleIntermediate.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.MiddleDistal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.MiddleTip.ToIndex()] = true;

                handJointsInLayout[XRHandJointID.RingMetacarpal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.RingProximal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.RingIntermediate.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.RingDistal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.RingTip.ToIndex()] = true;

                handJointsInLayout[XRHandJointID.LittleMetacarpal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.LittleProximal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.LittleIntermediate.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.LittleDistal.ToIndex()] = true;
                handJointsInLayout[XRHandJointID.LittleTip.ToIndex()] = true;

                isValid = true;
            }




            /// <summary>
            /// Attempts to retrieve current hand-tracking data from the provider.
            /// </summary>
            public override UpdateSuccessFlags TryUpdateHands(
                UpdateType updateType,
                ref Pose leftHandRootPose,
                NativeArray<XRHandJoint> leftHandJoints,
                ref Pose rightHandRootPose,
                NativeArray<XRHandJoint> rightHandJoints)
            {
                if (!isValid)
                    return UpdateSuccessFlags.None;

                UpdateSuccessFlags ret = UpdateSuccessFlags.None;

                const int handRootIndex = (int)HandJoint.JointWrist;

                PXR_PTApi.UPxr_GetHandTrackerJointLocations(0, ref jointLocations);
                if (jointLocations.isActive != 0U)
                {
                    for (int index = 0, jointCount = (int)jointLocations.jointCount; index < jointCount; ++index)
                    {
                        ref HandJointLocation joint = ref jointLocations.jointLocations[index];
                        int unityHandJointIndex = pxrJointIndexToUnityJointIndexMapping[index];

                        leftHandJoints[unityHandJointIndex] = CreateXRHandJoint(Handedness.Left, unityHandJointIndex, joint);

                        if (index == handRootIndex)
                        {
                            leftHandRootPose = PXRPosefToUnityPose(joint.pose);
                            ret |= UpdateSuccessFlags.LeftHandRootPose;
                        }
                    }

                    if (PicoAimHand.left.UpdateHand(0, (ret & UpdateSuccessFlags.LeftHandRootPose) != 0))
                    {
                        ret |= UpdateSuccessFlags.LeftHandJoints;
                    }
                }

                PXR_PTApi.UPxr_GetHandTrackerJointLocations(1, ref jointLocations);
                if (jointLocations.isActive != 0U)
                {
                    for (int index = 0, jointCount = (int)jointLocations.jointCount; index < jointCount; ++index)
                    {
                        ref HandJointLocation joint = ref jointLocations.jointLocations[index];
                        int unityHandJointIndex = pxrJointIndexToUnityJointIndexMapping[index];
                        rightHandJoints[unityHandJointIndex] = CreateXRHandJoint(Handedness.Right, unityHandJointIndex, joint);

                        if (index == handRootIndex)
                        {
                            rightHandRootPose = PXRPosefToUnityPose(joint.pose);
                            ret |= UpdateSuccessFlags.RightHandRootPose;
                        }

                    }
                    if (PicoAimHand.right.UpdateHand(1, (ret & UpdateSuccessFlags.RightHandRootPose) != 0))
                    {
                        ret |= UpdateSuccessFlags.RightHandJoints;
                    }
                }

                return ret;
            }

            void CreateHands()
            {
                if (PicoAimHand.left == null)
                    PicoAimHand.left = PicoAimHand.CreateHand(InputDeviceCharacteristics.Left);

                if (PicoAimHand.right == null)
                    PicoAimHand.right = PicoAimHand.CreateHand(InputDeviceCharacteristics.Right);
            }

            void DestroyHands()
            {
                if (PicoAimHand.left != null)
                {
                    InputSystem.RemoveDevice(PicoAimHand.left);
                    PicoAimHand.left = null;
                }

                if (PicoAimHand.right != null)
                {
                    InputSystem.RemoveDevice(PicoAimHand.right);
                    PicoAimHand.right = null;
                }
            }

            /// <summary>
            /// Create Unity XRHandJoint From PXR HandJointLocation
            /// </summary>
            /// <param name="handedness"></param>
            /// <param name="unityHandJointIndex"></param>
            /// <param name="joint"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            XRHandJoint CreateXRHandJoint(Handedness handedness, int unityHandJointIndex, in HandJointLocation joint)
            {

                Pose pose = Pose.identity;
                XRHandJointTrackingState state = XRHandJointTrackingState.None;
                if ((joint.locationStatus & AllStatus) == AllStatus)
                {
                    state = (XRHandJointTrackingState.Pose | XRHandJointTrackingState.Radius);
                    pose = PXRPosefToUnityPose(joint.pose);
                }
                return XRHandProviderUtility.CreateJoint(handedness,
                                        state,
                                        XRHandJointIDUtility.FromIndex(unityHandJointIndex),
                                        pose, joint.radius
                                        );
            }



            /// <summary>
            /// PXR's Posef to Unity'Pose
            /// </summary>
            /// <param name="pxrPose"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            Pose PXRPosefToUnityPose(in Posef pxrPose)
            {
                Vector3 position = pxrPose.Position.ToVector3();
                Quaternion orientation = pxrPose.Orientation.ToQuat();
                return new Pose(position, orientation);
            }

        }
    }

    /// <remarks>
    /// The <see cref="TrackedDevice.devicePosition"/> and
    /// <see cref="TrackedDevice.deviceRotation"/> inherited from <see cref="TrackedDevice"/>
    /// represent the aim pose. You can use these values to discover the target for pinch gestures,
    /// when appropriate. 
    /// 
    /// Use the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) in the scene to position and orient
    /// the device properly. If you are using this data to set the Transform of a GameObject in
    /// the scene hierarchy, you can set the local position and rotation of the Transform and make
    /// it a child of the <c>CameraOffset</c> object below the <c>XROrigin</c>. Otherwise, you can use the
    /// Transform of the <c>CameraOffset</c> to transform the data into world space.
    /// </remarks>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    [Preserve, InputControlLayout(displayName = "Pico Aim Hand", commonUsages = new[] { "LeftHand", "RightHand" })]
    public partial class PicoAimHand : TrackedDevice
    {
        /// <summary>
        /// The left-hand <see cref="InputDevice"/> that contains
        /// <see cref="InputControl"/>s that surface data in the Pico Hand
        /// Tracking Aim extension.
        /// </summary>
        /// <remarks>
        /// It is recommended that you treat this as read-only, and do not set
        /// it yourself. It will be set for you if hand-tracking has been
        /// enabled and if you are running with either the OpenXR or Oculus
        /// plug-in.
        /// </remarks>
        public static PicoAimHand left { get; set; }

        /// <summary>
        /// The right-hand <see cref="InputDevice"/> that contains
        /// <see cref="InputControl"/>s that surface data in the Pico Hand
        /// Tracking Aim extension.
        /// </summary>
        /// <remarks>
        /// It is recommended that you treat this as read-only, and do not set
        /// it yourself. It will be set for you if hand-tracking has been
        /// enabled and if you are running with either the OpenXR or Oculus
        /// plug-in.
        /// </remarks>
        public static PicoAimHand right { get; set; }

        /// <summary>
        /// The pinch amount required to register as being pressed for the
        /// purposes of <see cref="indexPressed"/>, <see cref="middlePressed"/>,
        /// <see cref="ringPressed"/>, and <see cref="littlePressed"/>.
        /// </summary>
        public const float pressThreshold = 0.8f;

        /// <summary>
        /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl)
        /// that represents whether the pinch between the index finger and
        /// the thumb is mostly pressed (greater than a threshold of <c>0.8</c>
        /// contained in <see cref="pressThreshold"/>).
        /// </summary>
        [Preserve, InputControl(offset = 0)]
        public ButtonControl indexPressed { get; private set; }

        /// <summary>
        /// Cast the result of reading this to <see cref="PicoAimFlags"/> to examine the value.
        /// </summary>
        [Preserve, InputControl]
        public IntegerControl aimFlags { get; private set; }

        /// <summary>
        /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl)
        /// that represents the pinch strength between the index finger and
        /// the thumb.
        /// </summary>
        /// <remarks>
        /// A value of <c>0</c> denotes no pinch at all, while a value of
        /// <c>1</c> denotes a full pinch.
        /// </remarks>
        [Preserve, InputControl]
        public AxisControl pinchStrengthIndex { get; private set; }

        /// <summary>
        /// Perform final initialization tasks after the control hierarchy has been put into place.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();

            indexPressed = GetChildControl<ButtonControl>(nameof(indexPressed));
            aimFlags = GetChildControl<IntegerControl>(nameof(aimFlags));
            pinchStrengthIndex = GetChildControl<AxisControl>(nameof(pinchStrengthIndex));

            var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                    InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.LeftHand);
                else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                    InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.RightHand);
            }
        }

        /// <summary>
        /// Creates a <see cref="PicoAimHand"/> and adds it to the Input System.
        /// </summary>
        /// <param name="extraCharacteristics">
        /// Additional characteristics to build the hand device with besides
        /// <see cref="InputDeviceCharacteristics.HandTracking"/> and <see cref="InputDeviceCharacteristics.TrackedDevice"/>.
        /// </param>
        /// <returns>
        /// A <see cref="PicoAimHand"/> retrieved from
        /// <see cref="InputSystem.AddDevice(InputDeviceDescription)"/>.
        /// </returns>
        /// <remarks>
        /// It is recommended that you do not call this yourself. It will be
        /// called for you at the appropriate time if hand-tracking has been
        /// enabled and if you are running with either the OpenXR or Oculus
        /// plug-in.
        /// </remarks>
        public static PicoAimHand CreateHand(InputDeviceCharacteristics extraCharacteristics)
        {
            var desc = new InputDeviceDescription
            {
                product = k_PicoAimHandDeviceProductName,
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | extraCharacteristics,
                    inputFeatures = new List<XRFeatureDescriptor>
                    {
                        new XRFeatureDescriptor
                        {
                            name = "index_pressed",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_flags",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_pose_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_pose_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_strength_index",
                            featureType = FeatureType.Axis1D
                        }
                    }
                }.ToJson()
            };
            return InputSystem.AddDevice(desc) as PicoAimHand;
        }

        /// <summary>
        /// Queues update events in the Input System based on the supplied hand.
        /// It is not recommended that you call this directly. This will be called
        /// for you when appropriate.
        /// </summary>
        /// <param name="isHandRootTracked">
        /// Whether the hand root pose is valid.
        /// </param>
        /// <param name="aimFlags">
        /// The aim flags to update in the Input System.
        /// </param>
        /// <param name="aimPose">
        /// The aim pose to update in the Input System. Used if the hand root is tracked.
        /// </param>
        /// <param name="pinchIndex">
        /// The pinch strength for the index finger to update in the Input System.
        /// </param>
        public void UpdateHand(bool isHandRootTracked, HandAimStatus aimFlags, Posef aimPose, float pinchIndex)
        {
            if (aimFlags != m_PreviousFlags)
            {
                InputSystem.QueueDeltaStateEvent(this.aimFlags, (int)aimFlags);
                m_PreviousFlags = aimFlags;
            }

            bool isIndexPressed = pinchIndex > pressThreshold;
            if (isIndexPressed != m_WasIndexPressed)
            {
                InputSystem.QueueDeltaStateEvent(indexPressed, isIndexPressed);
                m_WasIndexPressed = isIndexPressed;
            }

            InputSystem.QueueDeltaStateEvent(pinchStrengthIndex, pinchIndex);

            if ((aimFlags & HandAimStatus.AimComputed) == 0)
            {
                if (m_WasTracked)
                {
                    InputSystem.QueueDeltaStateEvent(isTracked, false);
                    InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                    m_WasTracked = false;
                }

                return;
            }

            if (isHandRootTracked)
            {
                InputSystem.QueueDeltaStateEvent(devicePosition, aimPose.Position.ToVector3());
                InputSystem.QueueDeltaStateEvent(deviceRotation, aimPose.Orientation.ToQuat());

                if (!m_WasTracked)
                {
                    InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                    InputSystem.QueueDeltaStateEvent(isTracked, true);
                }

                m_WasTracked = true;
            }
            else if (m_WasTracked)
            {
                InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                InputSystem.QueueDeltaStateEvent(isTracked, false);
                m_WasTracked = false;
            }
        }

        internal bool UpdateHand(int handType, bool isHandRootTracked)
        {

            HandAimState handAimState = new HandAimState();
            PXR_PTApi.UPxr_GetHandTrackerAimState(handType, ref handAimState);

            UpdateHand(
                isHandRootTracked,
                handAimState.aimStatus,
                handAimState.aimRayPose,
                handAimState.touchStrengthRay);

            return (handAimState.aimStatus & HandAimStatus.AimComputed) != 0;
        }

#if UNITY_EDITOR
        static PicoAimHand() => RegisterLayout();
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterLayout()
        {
            InputSystem.RegisterLayout<PicoAimHand>(
                    matches: new InputDeviceMatcher()
                    .WithProduct(k_PicoAimHandDeviceProductName));
        }

        const string k_PicoAimHandDeviceProductName = "Pico Aim Hand Tracking";

        HandAimStatus m_PreviousFlags;
        bool m_WasTracked;
        bool m_WasIndexPressed;
    }
}

#endif

