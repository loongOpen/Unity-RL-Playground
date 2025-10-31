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

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.XR.PICO.LivePreview
{
    public struct BodyTrackingBoneLength
    {
        /// <summary>
        /// The length of the head, which is from the top of the head to the upper area of the neck.
        /// </summary>
        public float headLen;
        /// <summary>
        /// The length of the neck, which is from the upper area of the neck to the lower area of the neck.
        /// </summary>
        public float neckLen;
        /// <summary>
        /// The length of the torso, which is from the lower area of the neck to the navel.
        /// </summary>
        public float torsoLen;
        /// <summary>
        /// The length of the hip, which is from the navel to the center of the upper area of the upper leg.
        /// </summary>
        public float hipLen;
        /// <summary>
        /// The length of the upper leg, which from the hip to the knee-joint.
        /// </summary>
        public float upperLegLen;
        /// <summary>
        /// The length of the lower leg, which is from the knee-joint to the ankle.
        /// </summary>
        public float lowerLegLen;
        /// <summary>
        /// The length of the foot, which is from the ankle to the tiptoe.
        /// </summary>
        public float footLen;
        /// <summary>
        /// The length of the shoulder, which is between the left and right shoulder joints.
        /// </summary>
        public float shoulderLen;
        /// <summary>
        /// The length of the upper arm, which is from the sholder joint to the elbow joint.
        /// </summary>
        public float upperArmLen;
        /// <summary>
        /// The length of the lower arm, which is from the elbow joint to the wrist.
        /// </summary>
        public float lowerArmLen;
        /// <summary>
        /// The length of the hand, which is from the wrist to the finger tip.
        /// </summary>
        public float handLen;
    }

    public struct BodyTrackerResult
    {
        /// <summary>
        /// A fixed-length array, each position transmits the data of one body joint.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public BodyTrackerTransform[] trackingdata;
    }

    /// <summary>
    /// Contains data about the position, velocity, acceleration, and action of a body joint.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BodyTrackerTransform
    {
        /// <summary>
        /// Body joint name. If the value is `NONE_ROLE`, the joint's data will not be calculated.
        /// </summary>
        public BodyTrackerRole bone;
        /// <summary>
        /// The joint's position in the scene. Use `localpose` for your app.
        /// </summary>
        public BodyTrackerTransPose localpose;
        /// <summary>
        /// (do not use `globalpose`)
        /// </summary>
        public BodyTrackerTransPose globalpose;
        /// <summary>
        /// The joint's velocity on the X, Y, and Z axes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] velo;
        /// <summary>
        /// The joint's acceleration on the X, Y, and Z axes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] acce;
        /// <summary>
        /// The joint's angular velocity on the X, Y, and Z axes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] wvelo;
        /// <summary>
        /// The joint's angular acceleration on the X, Y, and Z axes.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] wacce;
        /// <summary>
        /// Multiple actions can be supported at the same time by means of OR
        /// </summary>
        public UInt32 Action;
    }

    /// <summary>
    /// Body joint enumerations.
    /// * For leg tracking mode, joints numbered from 0 to 15 return data.
    /// * For full body tracking mode, all joints return data.
    /// </summary>
    public enum BodyTrackerRole
    {
        Pelvis = 0,
        LEFT_HIP = 1,
        RIGHT_HIP = 2,
        SPINE1 = 3,
        LEFT_KNEE = 4,
        RIGHT_KNEE = 5,
        SPINE2 = 6,
        LEFT_ANKLE = 7,
        RIGHT_ANKLE = 8,
        SPINE3 = 9,
        LEFT_FOOT = 10,
        RIGHT_FOOT = 11,
        NECK = 12,
        LEFT_COLLAR = 13,
        RIGHT_COLLAR = 14,
        HEAD = 15,
        LEFT_SHOULDER = 16,
        RIGHT_SHOULDER = 17,
        LEFT_ELBOW = 18,
        RIGHT_ELBOW = 19,
        LEFT_WRIST = 20,
        RIGHT_WRIST = 21,
        LEFT_HAND = 22,
        RIGHT_HAND = 23,
        NONE_ROLE = 24,                // unvalid
        MIN_ROLE = 0,                 // min value
        MAX_ROLE = 23,                // max value
        ROLE_NUM = 24,
    }

    /// <summary>
    /// Contains data about the position and rotation of a body joint.
    /// </summary>
    public struct BodyTrackerTransPose
    {
        /// <summary>
        /// IMU timestamp.
        /// </summary>
        public Int64 TimeStamp;
        /// <summary>
        /// The joint's position on the X axis.
        /// </summary>
        public double PosX;
        /// <summary>
        /// The joint's position on the Y axis.
        /// </summary>
        public double PosY;
        /// <summary>
        /// The joint's position on the Z axis.
        /// </summary>
        public double PosZ;
        /// <summary>
        /// The joint's rotation on the X component of the Quaternion.
        /// </summary>
        public double RotQx;
        /// <summary>
        /// The joint's rotation on the Y component of the Quaternion.
        /// </summary>
        public double RotQy;
        /// <summary>
        /// The joint's rotation on the Z component of the Quaternion.
        /// </summary>
        public double RotQz;
        /// <summary>
        /// The joint's rotation on the W component of the Quaternion.
        /// </summary>
        public double RotQw;
    }

    /// <summary>
    /// The data about the poses of ray and fingers.
    /// </summary>
    public struct HandAimState
    {
        /// <summary>
        /// The status of hand tracking. If it is not `tracked`, confidence will be `0`.
        /// </summary>
        public HandAimStatus aimStatus;
        /// <summary>
        /// The pose of the ray.
        /// </summary>
        public Posef aimRayPose;
        /// <summary>
        /// The strength of index finger's pinch.
        /// </summary>
        private float pinchStrengthIndex;
        /// <summary>
        /// The strength of middle finger's pinch.
        /// </summary>
        private float pinchStrengthMiddle;
        /// <summary>
        /// The strength of ring finger's pinch.
        /// </summary>
        private float pinchStrengthRing;
        /// <summary>
        /// The strength of little finger's pinch.
        /// </summary>
        private float pinchStrengthLittle;
        /// <summary>
        /// The strength of ray's touch.
        /// </summary>
        public float touchStrengthRay;
    }

    /// <summary>
    /// The status of ray and fingers.
    /// </summary>
    public enum HandAimStatus : ulong
    {
        /// <summary>
        /// Whether the data is valid.
        /// </summary>
        AimComputed = 0x00000001,
        /// <summary>
        /// Whether the ray appears.
        /// </summary>
        AimRayValid = 0x00000002,
        /// <summary>
        /// Whether the index finger pinches.
        /// </summary>
        AimIndexPinching = 0x00000004,
        /// <summary>
        /// Whether the middle finger pinches.
        /// </summary>
        AimMiddlePinching = 0x00000008,
        /// <summary>
        /// Whether the ring finger pinches.
        /// </summary>
        AimRingPinching = 0x00000010,
        /// <summary>
        /// Whether the little finger pinches.
        /// </summary>
        AimLittlePinching = 0x00000020,
        /// <summary>
        /// Whether the ray touches.
        /// </summary>
        AimRayTouched = 0x00000200
    }

    public struct Vector3f
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3() { x = x, y = y, z = -z };
        }
    }

    public struct Quatf
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion ToQuat()
        {
            return new Quaternion() { x = x, y = y, z = -z, w = -w };
        }
    }

    /// <summary>
    /// The location of hand joint.
    /// </summary>
    public struct Posef
    {
        /// <summary>
        /// The orientation of hand joint.
        /// </summary>
        public Quatf Orientation;
        /// <summary>
        /// The position of hand joint.
        /// </summary>
        public Vector3f Position;
        public override string ToString()
        {
            return string.Format("Orientation :{0}, {1}, {2}, {3}  Position: {4}, {5}, {6}",
                Orientation.x, Orientation.y, Orientation.z, Orientation.w,
                Position.x, Position.y, Position.z);
        }
    }

    /// <summary>
    /// The data about the location of hand joint.
    /// </summary>
    public struct HandJointLocation
    {
        /// <summary>
        /// The status of hand joint location.
        /// </summary>
        public HandLocationStatus locationStatus;
        /// <summary>
        /// The orientation and position of hand joint.
        /// </summary>
        public Posef pose;
        /// <summary>
        /// The radius of hand joint.
        /// </summary>
        public float radius;
    }

    /// <summary>
    /// The data about hand tracking.
    /// </summary>
    public struct HandJointLocations
    {
        /// <summary>
        /// The quality level of hand tracking:
        /// `0`: low
        /// `1`: high
        /// </summary>
        public uint isActive;
        /// <summary>
        /// The number of hand joints that the SDK supports. Currenty returns `26`.
        /// </summary>
        public uint jointCount;
        /// <summary>
        /// The scale of the hand.
        /// </summary>
        public float handScale;

        /// <summary>
        /// The locations (orientation and position) of hand joints.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)HandJoint.JointMax)]
        public HandJointLocation[] jointLocations;
    }

    public enum HandJoint
    {
        JointPalm = 0,
        JointWrist = 1,

        JointThumbMetacarpal = 2,
        JointThumbProximal = 3,
        JointThumbDistal = 4,
        JointThumbTip = 5,

        JointIndexMetacarpal = 6,
        JointIndexProximal = 7,
        JointIndexIntermediate = 8,
        JointIndexDistal = 9,
        JointIndexTip = 10,

        JointMiddleMetacarpal = 11,
        JointMiddleProximal = 12,
        JointMiddleIntermediate = 13,
        JointMiddleDistal = 14,
        JointMiddleTip = 15,

        JointRingMetacarpal = 16,
        JointRingProximal = 17,
        JointRingIntermediate = 18,
        JointRingDistal = 19,
        JointRingTip = 20,

        JointLittleMetacarpal = 21,
        JointLittleProximal = 22,
        JointLittleIntermediate = 23,
        JointLittleDistal = 24,
        JointLittleTip = 25,

        JointMax = 26
    }

    /// <summary>
    /// The data about the status of hand joint location.
    /// </summary>
    public enum HandLocationStatus : ulong
    {
        /// <summary>
        /// Whether the joint's orientation is valid.
        /// </summary>
        OrientationValid = 0x00000001,
        /// <summary>
        /// Whether the joint's position is valid.
        /// </summary>
        PositionValid = 0x00000002,
        /// <summary>
        /// Whether the joint's orientation is being tracked.
        /// </summary>
        OrientationTracked = 0x00000004,
        /// <summary>
        /// Whether the joint's position is being tracked.
        /// </summary>
        PositionTracked = 0x00000008
    }


    public static class PXR_PTApi
    {
        private const string PXR_PLATFORM_DLL = "PxrLivePreview";

        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pxr_SetSRPState(bool value);

        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_StartBodyTrackingCalibApp();
        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_StartBodyTracking(int jointSet, ref BodyTrackingBoneLength length,int algmode);
        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_StopBodyTracking();
        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_GetBodyTrackingPose(ref BodyTrackerResult results);
        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_GetFitnessBandCalibState(ref int state);
        
        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_GetHandTrackerActiveState(ref int state);

        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_GetHandTrackerAimState(int hand, ref HandAimState aimState);

        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LP_GetHandTrackerJointLocations(int hand, ref HandJointLocations jointLocations);


        public static void UPxr_PTSetSRPState(bool value)
        {
#if UNITY_EDITOR
            Pxr_SetSRPState(value);
#endif
        }

        public static bool UPxr_GetSettingState()
        {
#if UNITY_EDITOR
            return true;
#endif
        }

        public static void UPxr_GetHandTrackerAimState(int hand, ref HandAimState aimState)
        {
#if UNITY_EDITOR
            LP_GetHandTrackerAimState(hand, ref aimState);
#endif
        }

        public static void UPxr_GetHandTrackerJointLocations(int hand, ref HandJointLocations jointLocations)
        {
#if UNITY_EDITOR
            LP_GetHandTrackerJointLocations(hand, ref jointLocations);
#endif
        }

        public static bool UPxr_GetGetHandTrackerActiveState()
        {
            int state = 0;
#if UNITY_EDITOR
            LP_GetHandTrackerActiveState(ref state);
#endif
            return Convert.ToBoolean(state);
        }

        public static void UPxr_OpenFitnessBandCalibrationAPP()
        {
#if UNITY_EDITOR
            LP_StartBodyTrackingCalibApp();
#endif
        }

        public static void UPxr_GetFitnessBandCalibState(ref int calibrated)
        {
#if UNITY_EDITOR
            LP_GetFitnessBandCalibState(ref calibrated);
#endif
        }

        public static void UPxr_StopBodyTracking()
        {
#if UNITY_EDITOR
            LP_StopBodyTracking();
#endif
        }

        public static void UPxr_StartBodyTracking()
        {
            BodyTrackingBoneLength length = new BodyTrackingBoneLength();
#if UNITY_EDITOR
            LP_StartBodyTracking( 1, ref length,1);
#endif
        }

        public static void UPxr_SetSwiftMode(int mode)
        {
#if UNITY_EDITOR
            BodyTrackingBoneLength length = new BodyTrackingBoneLength();
            LP_StartBodyTracking( 1, ref length,mode);
#endif
        }

        public static void UPxr_SetBodyTrackingBoneLength(BodyTrackingBoneLength length)
        {
#if UNITY_EDITOR
            LP_StartBodyTracking(1, ref length,2);
#endif
        }

        public static void UPxr_GetBodyTrackingPose(ref BodyTrackerResult bodyTrackerResult)
        {
#if UNITY_EDITOR
            LP_GetBodyTrackingPose(ref bodyTrackerResult);
#endif
        }

    }
}