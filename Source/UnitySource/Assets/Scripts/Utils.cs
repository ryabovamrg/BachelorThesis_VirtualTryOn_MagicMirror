using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kinect = Windows.Kinect;
using Face = Microsoft.Kinect.Face;

namespace ryabomar { 


/// <summary>
/// collection of utilities and maps(dictionaries) for wide use in project
/// </summary>
public static class utils
{

    /// <summary>
    /// convert Kinect.Joint position to Vector3
    /// </summary>
    /// <param name="joint">joint</param>
    /// <returns>position as Vector3</returns>
    public static Vector3 jointPosition(Kinect.Joint joint) {
        return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
    }


    /// <summary>
    /// convert Kinect.JointOrientation orientation to quaternion
    /// </summary>
    /// <param name="joint">joint</param>
    /// <returns>orientation as quaternion</returns>
    public static Quaternion jointRotation(Kinect.JointOrientation joint) {
        return new Quaternion(joint.Orientation.X, joint.Orientation.Y, joint.Orientation.Z, joint.Orientation.W);
    }



    /// <summary>
    /// map of joint types as begin and end of bone (for ex. knee -> hip)
    /// </summary>
    public static Dictionary<Kinect.JointType, Kinect.JointType> BONE_MAP = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft,       Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft,      Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft,       Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft,        Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight,      Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight,     Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight,      Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight,       Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft,    Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft,      Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft,       Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft,      Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft,      Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft,   Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight,   Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight,     Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight,      Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight,     Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight,     Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight,  Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase,      Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid,       Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder,  Kinect.JointType.Neck },
        { Kinect.JointType.Neck,           Kinect.JointType.Head },
    };


    /// <summary>
    /// map of joint type to unity human bone type
    /// </summary>
    public static Dictionary<Kinect.JointType, HumanBodyBones> JOINT_TO_HUMAN_BONE = new Dictionary<Kinect.JointType, HumanBodyBones>() {
        { Kinect.JointType.SpineBase,       HumanBodyBones.Hips },
        { Kinect.JointType.SpineMid,        HumanBodyBones.Spine },
        { Kinect.JointType.SpineShoulder,   HumanBodyBones.Chest },

        { Kinect.JointType.Neck,            HumanBodyBones.Neck },
        { Kinect.JointType.Head,            HumanBodyBones.Head },

        { Kinect.JointType.ShoulderLeft,    HumanBodyBones.LeftShoulder},
        { Kinect.JointType.ElbowLeft,       HumanBodyBones.LeftUpperArm},
        { Kinect.JointType.WristLeft,       HumanBodyBones.LeftLowerArm },
        { Kinect.JointType.HandLeft,        HumanBodyBones.LeftHand },


        { Kinect.JointType.ShoulderRight,    HumanBodyBones.RightShoulder},
        { Kinect.JointType.ElbowRight,       HumanBodyBones.RightUpperArm},
        { Kinect.JointType.WristRight,       HumanBodyBones.RightLowerArm },
        { Kinect.JointType.HandRight,        HumanBodyBones.RightHand },

        { Kinect.JointType.HipLeft,          HumanBodyBones.LeftUpperLeg },
        { Kinect.JointType.KneeLeft,         HumanBodyBones.LeftLowerLeg },
        { Kinect.JointType.AnkleLeft,        HumanBodyBones.LeftFoot },
        { Kinect.JointType.FootLeft,         HumanBodyBones.LeftToes },


        { Kinect.JointType.HipRight,          HumanBodyBones.RightUpperLeg },
        { Kinect.JointType.KneeRight,         HumanBodyBones.RightLowerLeg },
        { Kinect.JointType.AnkleRight,        HumanBodyBones.RightFoot },
        { Kinect.JointType.FootRight,         HumanBodyBones.RightToes },
    };


}


}// !namespace ryabomar
