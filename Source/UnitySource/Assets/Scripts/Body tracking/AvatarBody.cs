using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JointType = Windows.Kinect.JointType;

namespace ryabomar {

/// <summary>
/// Avatar of user. Uses inverse kinematics animation to simulate user movement
/// </summary>
public class AvatarBody : MonoBehaviour
{

    /// <summary>
    /// body mesh height in meteres
    /// needs for scaling then adjusting body
    /// </summary>
    [Range(0.5f, 2.2f)] public float physicalHeight = 1.50f;

    /// <summary>
    /// reference to KinectBody
    /// </summary>
    public KinectBody kinectBody;
    
    IKChain spineChain;
    IKChain leftHandChain;
    IKChain rightHandChain;
    IKChain leftLegChain;
    IKChain rightLegChain;

    Quaternion pelvesInitGlobalRotation;
    Quaternion ribcageInitGlobalRotation;

    Quaternion headInitGlobalRotation;
    Quaternion headInitLocalRotation;

    /// <summary>
    /// joints of KinectBody. Used as target in IK motion
    /// </summary>
    Dictionary<JointType, Transform> bodyJoints = new Dictionary<JointType, Transform>();

    Dictionary<Bone, Quaternion> boneInitLocalRotations  = new Dictionary<Bone, Quaternion>();
    Dictionary<Bone, Quaternion> boneInitGlobalRotations = new Dictionary<Bone, Quaternion>();

    /// <summary>
    /// bones of the skeleton
    /// </summary>
    Dictionary<Bone, Transform> bones;

    /// <summary>
    /// humanoid bone type. Alternative to HumanBodyBones
    /// </summary>
    enum Bone { 
        PELVIS, 
        SPINE_1, SPINE_2, SPINE_3, 

        NECK, HEAD,

        CLAVICLE_L, UPPER_ARM_L, LOWER_ARM_L, HAND_L,
            THUMB_1_L,  THUMB_2_L,  THUMB_3_L,
            INDEX_1_L,  INDEX_2_L,  INDEX_3_L,
            MIDDLE_1_L, MIDDLE_2_L, MIDDLE_3_L,
            RING_1_L,   RING_2_L,   RING_3_L,
            PINKY_1_L,  PINKY_2_L,  PINKY_3_L,

            
        CLAVICLE_R, UPPER_ARM_R, LOWER_ARM_R, HAND_R,
            THUMB_1_R,  THUMB_2_R,  THUMB_3_R,
            INDEX_1_R,  INDEX_2_R,  INDEX_3_R,
            MIDDLE_1_R, MIDDLE_2_R, MIDDLE_3_R,
            RING_1_R,   RING_2_R,   RING_3_R,
            PINKY_1_R,  PINKY_2_R,  PINKY_3_R,

        THIGH_L, CALF_L, FOOT_L, BALL_L,
        THIGH_R, CALF_R, FOOT_R, BALL_R
    };

    /// <summary>
    /// Find bones by its name and save them in dictionary
    /// </summary>
    void MapBones() {
        bones = new Dictionary<Bone, Transform>{
            { Bone.PELVIS,      FindOrException("Game_engine/Root/pelvis")}, 
            { Bone.SPINE_1,     FindOrException("Game_engine/Root/pelvis/spine_01")}, 
            { Bone.SPINE_2,     FindOrException("Game_engine/Root/pelvis/spine_01/spine_02")},
            { Bone.SPINE_3,     FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03")},
            { Bone.NECK,        FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/neck_01")},
            { Bone.HEAD,        FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/neck_01/head")},
            { Bone.CLAVICLE_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l")},
            { Bone.UPPER_ARM_L, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l")}, 
            { Bone.LOWER_ARM_L, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l")}, 
            { Bone.HAND_L,      FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l")}, 
                { Bone.THUMB_1_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/thumb_01_l")},         
                { Bone.THUMB_2_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/thumb_01_l/thumb_02_l")},
                { Bone.THUMB_3_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/thumb_01_l/thumb_02_l/thumb_03_l")},
            
                { Bone.INDEX_1_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/index_01_l")},         
                { Bone.INDEX_2_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/index_01_l/index_02_l")},
                { Bone.INDEX_3_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/index_01_l/index_02_l/index_03_l")},
            
                { Bone.MIDDLE_1_L, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/middle_01_l")},         
                { Bone.MIDDLE_2_L, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/middle_01_l/middle_02_l")},
                { Bone.MIDDLE_3_L, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/middle_01_l/middle_02_l/middle_03_l")},
            
                { Bone.RING_1_L,   FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/ring_01_l")},         
                { Bone.RING_2_L,   FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/ring_01_l/ring_02_l")},
                { Bone.RING_3_L,   FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/ring_01_l/ring_02_l/ring_03_l")},
            
                { Bone.PINKY_1_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/pinky_01_l")},         
                { Bone.PINKY_2_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/pinky_01_l/pinky_02_l")},
                { Bone.PINKY_3_L,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l/pinky_01_l/pinky_02_l/pinky_03_l")},
                            
            { Bone.CLAVICLE_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r")}, 
            { Bone.UPPER_ARM_R, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r")}, 
            { Bone.LOWER_ARM_R, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r")}, 
            { Bone.HAND_R,      FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r")}, 
            
                { Bone.THUMB_1_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/thumb_01_r")},         
                { Bone.THUMB_2_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/thumb_01_r/thumb_02_r")},
                { Bone.THUMB_3_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/thumb_01_r/thumb_02_r/thumb_03_r")},
            
                { Bone.INDEX_1_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/index_01_r")},         
                { Bone.INDEX_2_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/index_01_r/index_02_r")},
                { Bone.INDEX_3_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/index_01_r/index_02_r/index_03_r")},
            
                { Bone.MIDDLE_1_R, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/middle_01_r")},         
                { Bone.MIDDLE_2_R, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/middle_01_r/middle_02_r")},
                { Bone.MIDDLE_3_R, FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/middle_01_r/middle_02_r/middle_03_r")},
            
                { Bone.RING_1_R,   FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/ring_01_r")},         
                { Bone.RING_2_R,   FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/ring_01_r/ring_02_r")},
                { Bone.RING_3_R,   FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/ring_01_r/ring_02_r/ring_03_r")},
            
                { Bone.PINKY_1_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/pinky_01_r")},         
                { Bone.PINKY_2_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/pinky_01_r/pinky_02_r")},
                { Bone.PINKY_3_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/pinky_01_r/pinky_02_r/pinky_03_r")},
            
            
            //{ Bone.MIDDLE_1_R,  FindOrException("Game_engine/Root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/middle_01_r")},
            { Bone.THIGH_L,     FindOrException("Game_engine/Root/pelvis/thigh_l")}, 
            { Bone.CALF_L,      FindOrException("Game_engine/Root/pelvis/thigh_l/calf_l")}, 
            { Bone.FOOT_L,      FindOrException("Game_engine/Root/pelvis/thigh_l/calf_l/foot_l")}, 
            { Bone.BALL_L,      FindOrException("Game_engine/Root/pelvis/thigh_l/calf_l/foot_l/ball_l")},
            { Bone.THIGH_R,     FindOrException("Game_engine/Root/pelvis/thigh_r")}, 
            { Bone.CALF_R,      FindOrException("Game_engine/Root/pelvis/thigh_r/calf_r")}, 
            { Bone.FOOT_R,      FindOrException("Game_engine/Root/pelvis/thigh_r/calf_r/foot_r")}, 
            { Bone.BALL_R,      FindOrException("Game_engine/Root/pelvis/thigh_r/calf_r/foot_r/ball_r")},        
        };
    }

    /// <summary>
    /// Save bones initial local rotations
    /// </summary>
    /// <param name="list">list of bones</param>
    void SaveBonesInitialLocalRotations(List<Bone> list){
        foreach(Bone bone in list){
            boneInitLocalRotations.Add(bone, bones[bone].localRotation);
        }
    }

    /// <summary>
    /// Save bones initial global rotations
    /// </summary>
    /// <param name="list">list of bones</param>
    void SaveBonesInitialGlobalRotations(List<Bone> list){
        foreach(Bone bone in list){
            boneInitGlobalRotations.Add(bone, bones[bone].rotation);
        }
    }


    /// <summary>
    /// initialisation
    /// </summary>
    void Start(){
        if(kinectBody == null) {
            throw new System.Exception("need KinectBodyVisualization");
        }

        { // map joints (and offsets if presents)
            foreach(var keyValue in kinectBody.joints){
                Transform offset = keyValue.Value.Find("offset");
                if(offset != null){
                    bodyJoints.Add(keyValue.Key, offset);
                } else {
                    bodyJoints.Add(keyValue.Key, keyValue.Value);
                }
            }
        }

        MapBones();

        // save fingers initial rotations
        SaveBonesInitialLocalRotations(new List<Bone>{
            Bone.THUMB_1_L,    Bone.THUMB_1_R,
            Bone.THUMB_2_L,    Bone.THUMB_2_R,
            Bone.THUMB_3_L,    Bone.THUMB_3_R,
            Bone.INDEX_1_L,    Bone.INDEX_1_R,
            Bone.INDEX_2_L,    Bone.INDEX_2_R,
            Bone.INDEX_3_L,    Bone.INDEX_3_R,
            Bone.MIDDLE_1_L,   Bone.MIDDLE_1_R,    
            Bone.MIDDLE_2_L,   Bone.MIDDLE_2_R,    
            Bone.MIDDLE_3_L,   Bone.MIDDLE_3_R,    
            Bone.RING_1_L,     Bone.RING_1_R,
            Bone.RING_2_L,     Bone.RING_2_R,
            Bone.RING_3_L,     Bone.RING_3_R,
            Bone.PINKY_1_L,    Bone.PINKY_1_R,
            Bone.PINKY_2_L,    Bone.PINKY_2_R,
            Bone.PINKY_3_L,    Bone.PINKY_3_R,
            Bone.HEAD
        });


        SaveBonesInitialGlobalRotations(new List<Bone>{

        });

        pelvesInitGlobalRotation  = bones[Bone.PELVIS].rotation;
        ribcageInitGlobalRotation = bones[Bone.SPINE_3].rotation;
        headInitGlobalRotation    = bones[Bone.HEAD].rotation;
        headInitLocalRotation     = bones[Bone.HEAD].localRotation;

        // make it face camera
        transform.Rotate(Vector3.up, 180.0f);

        MakeSpineChain();
        MakeLeftHandChain();
        MakeRightHandChain();
        MakeLeftLegChain();
        MakeRightLegChain();
    }


    /// <summary>
    /// apply motion
    /// </summary>
    void LateUpdate(){
        // Order is important!!!
        MovePelvis();
        RotatePelvis();
        
        //
        spineChain.ManualUpdate();
        RotateRibcage();
        //

        RotateHead();

        leftHandChain.ManualUpdate();
        rightHandChain.ManualUpdate();

        leftLegChain.ManualUpdate();
        rightLegChain.ManualUpdate();

        UpdateHands();
    }

    /// <summary>
    /// move pelvis bone
    /// </summary>
    void MovePelvis() {
        bones[Bone.PELVIS].position = bodyJoints[JointType.SpineBase].position;
        //kinectBody.joints[JointType.SpineBase].position;
    }


    /// <summary>
    /// rotate pelvis bone
    /// </summary>
    void RotatePelvis() {
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;
        
        Vector3 spineBasePos = joints[JointType.SpineBase].position;
        Vector3 leftHipPos   = joints[JointType.HipLeft].position;
        Vector3 rightHipPos  = joints[JointType.HipRight].position;
        Vector3 spineMidPos  = joints[JointType.SpineMid].position;

        Vector3 upDir        = (spineMidPos - spineBasePos).normalized;
        Vector3 rightDir     = ((rightHipPos - spineBasePos) * 2 - (leftHipPos - spineBasePos)).normalized; // average vector
        Vector3 forwardDir   = Vector3.Cross(upDir, rightDir.normalized);
        
        if(!forwardDir.Equals(Vector3.zero)){
            bones[Bone.PELVIS].rotation = Quaternion.LookRotation(forwardDir, upDir) * pelvesInitGlobalRotation;
        }
        
    }


    /// <summary>
    /// Rotate ribcage
    /// </summary>
    void RotateRibcage() {
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;
        
        Vector3 spineShoulderPos = joints[JointType.SpineShoulder].position;
        Vector3 leftShoulderPos  = joints[JointType.ShoulderLeft].position;
        Vector3 rightShoulderPos = joints[JointType.ShoulderRight].position;
        Vector3 spineMidPos      = joints[JointType.SpineMid].position;

        Vector3 upDir        = (spineShoulderPos - spineMidPos).normalized;
        Vector3 rightDir     = ((rightShoulderPos - spineShoulderPos) * 2 - (leftShoulderPos - spineShoulderPos)).normalized;
        Vector3 forwardDir   = -Vector3.Cross(rightDir.normalized, upDir);

        if(!forwardDir.Equals(Vector3.zero)){
            bones[Bone.SPINE_3].rotation = Quaternion.LookRotation(forwardDir, upDir) * ribcageInitGlobalRotation;
        }
        
    }


    /// <summary>
    /// rotate head
    /// </summary>
    void RotateHead() {
        if(kinectBody.isFaceTracked){
            bones[Bone.HEAD].rotation = 
                kinectBody.headMarker.rotation
                * Quaternion.Euler(0.0f, 180.0f, 0.0f) // in kinect space forward is fasing backward.. so thi is the fix
                * headInitGlobalRotation;  
        } else {
            bones[Bone.HEAD].localRotation = headInitLocalRotation;
        }
    }

    /// <summary>
    /// update hands state
    /// </summary>
    void UpdateHands() {
        {// left
            bones[Bone.THUMB_1_L].transform.localRotation  = boneInitLocalRotations[Bone.THUMB_1_L];
            bones[Bone.THUMB_2_L].transform.localRotation  = boneInitLocalRotations[Bone.THUMB_2_L];
            bones[Bone.THUMB_3_L].transform.localRotation  = boneInitLocalRotations[Bone.THUMB_3_L];

            bones[Bone.INDEX_1_L].transform.localRotation  = boneInitLocalRotations[Bone.INDEX_1_L];
            bones[Bone.INDEX_2_L].transform.localRotation  = boneInitLocalRotations[Bone.INDEX_2_L];
            bones[Bone.INDEX_3_L].transform.localRotation  = boneInitLocalRotations[Bone.INDEX_3_L];

            bones[Bone.MIDDLE_1_L].transform.localRotation = boneInitLocalRotations[Bone.MIDDLE_1_L];
            bones[Bone.MIDDLE_2_L].transform.localRotation = boneInitLocalRotations[Bone.MIDDLE_2_L];
            bones[Bone.MIDDLE_3_L].transform.localRotation = boneInitLocalRotations[Bone.MIDDLE_3_L];

            bones[Bone.RING_1_L].transform.localRotation   = boneInitLocalRotations[Bone.RING_1_L];
            bones[Bone.RING_2_L].transform.localRotation   = boneInitLocalRotations[Bone.RING_2_L];
            bones[Bone.RING_3_L].transform.localRotation   = boneInitLocalRotations[Bone.RING_3_L];

            bones[Bone.PINKY_1_L].transform.localRotation  = boneInitLocalRotations[Bone.PINKY_1_L];
            bones[Bone.PINKY_2_L].transform.localRotation  = boneInitLocalRotations[Bone.PINKY_2_L];
            bones[Bone.PINKY_3_L].transform.localRotation  = boneInitLocalRotations[Bone.PINKY_3_L];
            
            if(kinectBody.isRightHandClosed){
                bones[Bone.THUMB_1_L].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.THUMB_2_L].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.THUMB_3_L].transform.Rotate(45,0,0,Space.Self);

                bones[Bone.INDEX_1_L].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.INDEX_2_L].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.INDEX_3_L].transform.Rotate(45,0,0,Space.Self);

                bones[Bone.MIDDLE_1_L].transform.Rotate(45,0,0,Space.Self); ;
                bones[Bone.MIDDLE_2_L].transform.Rotate(45,0,0,Space.Self); ;
                bones[Bone.MIDDLE_3_L].transform.Rotate(45,0,0,Space.Self); ;

                bones[Bone.RING_1_L].transform.Rotate(45,0,0,Space.Self);  
                bones[Bone.RING_2_L].transform.Rotate(45,0,0,Space.Self);  
                bones[Bone.RING_3_L].transform.Rotate(45,0,0,Space.Self);  

                bones[Bone.PINKY_1_L].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.PINKY_2_L].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.PINKY_3_L].transform.Rotate(45,0,0,Space.Self);
            }
        }


        {// right
            bones[Bone.THUMB_1_R].transform.localRotation  = boneInitLocalRotations[Bone.THUMB_1_R];
            bones[Bone.THUMB_2_R].transform.localRotation  = boneInitLocalRotations[Bone.THUMB_2_R];
            bones[Bone.THUMB_3_R].transform.localRotation  = boneInitLocalRotations[Bone.THUMB_3_R];

            bones[Bone.INDEX_1_R].transform.localRotation  = boneInitLocalRotations[Bone.INDEX_1_R];
            bones[Bone.INDEX_2_R].transform.localRotation  = boneInitLocalRotations[Bone.INDEX_2_R];
            bones[Bone.INDEX_3_R].transform.localRotation  = boneInitLocalRotations[Bone.INDEX_3_R];

            bones[Bone.MIDDLE_1_R].transform.localRotation = boneInitLocalRotations[Bone.MIDDLE_1_R];
            bones[Bone.MIDDLE_2_R].transform.localRotation = boneInitLocalRotations[Bone.MIDDLE_2_R];
            bones[Bone.MIDDLE_3_R].transform.localRotation = boneInitLocalRotations[Bone.MIDDLE_3_R];

            bones[Bone.RING_1_R].transform.localRotation   = boneInitLocalRotations[Bone.RING_1_R];
            bones[Bone.RING_2_R].transform.localRotation   = boneInitLocalRotations[Bone.RING_2_R];
            bones[Bone.RING_3_R].transform.localRotation   = boneInitLocalRotations[Bone.RING_3_R];

            bones[Bone.PINKY_1_R].transform.localRotation  = boneInitLocalRotations[Bone.PINKY_1_R];
            bones[Bone.PINKY_2_R].transform.localRotation  = boneInitLocalRotations[Bone.PINKY_2_R];
            bones[Bone.PINKY_3_R].transform.localRotation  = boneInitLocalRotations[Bone.PINKY_3_R];
            
            if(kinectBody.isLeftHandClosed){
                bones[Bone.THUMB_1_R].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.THUMB_2_R].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.THUMB_3_R].transform.Rotate(45,0,0,Space.Self);

                bones[Bone.INDEX_1_R].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.INDEX_2_R].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.INDEX_3_R].transform.Rotate(45,0,0,Space.Self);

                bones[Bone.MIDDLE_1_R].transform.Rotate(45,0,0,Space.Self); ;
                bones[Bone.MIDDLE_2_R].transform.Rotate(45,0,0,Space.Self); ;
                bones[Bone.MIDDLE_3_R].transform.Rotate(45,0,0,Space.Self); ;

                bones[Bone.RING_1_R].transform.Rotate(45,0,0,Space.Self);  
                bones[Bone.RING_2_R].transform.Rotate(45,0,0,Space.Self);  
                bones[Bone.RING_3_R].transform.Rotate(45,0,0,Space.Self);  

                bones[Bone.PINKY_1_R].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.PINKY_2_R].transform.Rotate(45,0,0,Space.Self);
                bones[Bone.PINKY_3_R].transform.Rotate(45,0,0,Space.Self);
            }
        }
    }

    /// <summary>
    /// find child object by name or rise an exaption
    /// </summary>
    /// <param name="objectName">Name of chaild object.</param>
    /// <returns></returns>
    Transform FindOrException(string objectName) {
        Transform result = transform.Find(objectName);
        if(result == null){
            throw new System.Exception("could not find " + objectName);
        } else {
            return result;
        }
    }


    /// <summary>
    /// make IKChain for spine
    /// </summary>
    void MakeSpineChain() {
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;

        // create empty object
        GameObject obj = new GameObject();
        obj.name = "<spine chain>";
        obj.transform.parent = transform;

        // add IKChain component
        spineChain = obj.AddComponent<IKChain>();
        spineChain.MakeChain(
                // target
                joints[JointType.Neck].gameObject,
                // end effector 
                bones[Bone.NECK].gameObject,
                // root
                bones[Bone.SPINE_1].gameObject,
                // hints
                new Dictionary<GameObject, GameObject>{
                    { bones[Bone.SPINE_3].gameObject, joints[JointType.SpineShoulder].gameObject },
                    { bones[Bone.SPINE_2].gameObject, joints[JointType.SpineMid].gameObject },
                }
            );

        // drawing line component only for debugging purpose
        obj.AddComponent<IKChainDrawLines>();
    }


    /// <summary>
    /// make IKChain for left hand
    /// </summary>
    void MakeLeftHandChain() {
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;

        // create empty object
        GameObject obj = new GameObject();
        obj.name = "<left hand chain>";
        obj.transform.parent = transform; 

        // add IKChain component
        leftHandChain = obj.AddComponent<IKChain>();
        leftHandChain.MakeChain(
                // target 
                joints[JointType.HandTipRight].gameObject,
                // end effector
                bones[Bone.MIDDLE_1_L].gameObject,
                // root               
                bones[Bone.CLAVICLE_L].gameObject,
                // hints
                new Dictionary<GameObject, GameObject>{ // swappend right <-> left sides
                    { bones[Bone.HAND_L].gameObject,        joints[JointType.WristRight].gameObject },
                    { bones[Bone.LOWER_ARM_L].gameObject,   joints[JointType.ElbowRight].gameObject },
                    { bones[Bone.CLAVICLE_L].gameObject,    joints[JointType.ShoulderRight].gameObject }
                }
            );

        // drawing line component only for debugging purpose
        obj.AddComponent<IKChainDrawLines>();
    }

    /// <summary>
    /// make IKChain for right hand
    /// </summary>
    void MakeRightHandChain() {
        Dictionary<JointType, Transform> joints = bodyJoints;// kinectBody.joints;

        // create empty object
        GameObject obj = new GameObject();
        obj.name = "<right hand chain>";
        obj.transform.parent = transform; 

        // add IKChain component
        rightHandChain = obj.AddComponent<IKChain>();
        rightHandChain.MakeChain(
                // target
                joints[JointType.HandTipLeft].gameObject, 
                // end effector
                bones[Bone.MIDDLE_1_R].gameObject,  
                // root               
                bones[Bone.CLAVICLE_R].gameObject,
                // hints
                new Dictionary<GameObject, GameObject>{ // swappend right <-> left sides
                    { bones[Bone.HAND_R].gameObject,      joints[JointType.WristLeft].gameObject },
                    { bones[Bone.LOWER_ARM_R].gameObject, joints[JointType.ElbowLeft].gameObject },
                    { bones[Bone.CLAVICLE_R].gameObject,  joints[JointType.ShoulderLeft].gameObject }
                }
            );

        // drawing line component only for debugging purpose
        obj.AddComponent<IKChainDrawLines>();
    }


    /// <summary>
    /// make IKChain for left leg
    /// </summary>
    void MakeLeftLegChain(){
        Dictionary<JointType, Transform> joints = bodyJoints;// kinectBody.joints;

        // create empty object
        GameObject obj = new GameObject();
        obj.name = "<left leg chain>";
        obj.transform.parent = transform; 

        // add IKChain component
        leftLegChain = obj.AddComponent<IKChain>();
        leftLegChain.MakeChain(
                // target
                joints[JointType.FootRight].gameObject, 
                // end effector
                bones[Bone.BALL_L].gameObject,
                // root        
                bones[Bone.PELVIS].gameObject,
                // hints
                new Dictionary<GameObject, GameObject>{ // swappend right <-> left sides
                    { bones[Bone.THIGH_L].gameObject, joints[JointType.HipRight].gameObject },
                    { bones[Bone.CALF_L].gameObject,  joints[JointType.KneeRight].gameObject },
                    { bones[Bone.FOOT_L].gameObject,  joints[JointType.AnkleRight].gameObject },
                    { bones[Bone.BALL_L].gameObject,  joints[JointType.FootRight].gameObject }
                }
            );
        // drawing line component only for debugging purpose
        obj.AddComponent<IKChainDrawLines>();
    }


    /// <summary>
    /// make IKChain for right leg
    /// </summary>
    void MakeRightLegChain(){
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;

        // create empty object
        GameObject obj = new GameObject();
        obj.name = "<right leg chain>";
        obj.transform.parent = transform;

        // add IKChain component
        rightLegChain = obj.AddComponent<IKChain>();
        rightLegChain.MakeChain(
                // target
                kinectBody.joints[JointType.FootLeft].gameObject, 
                // end effector
                bones[Bone.BALL_R].gameObject,  
                // root               
                bones[Bone.PELVIS].gameObject,
                // hints
                new Dictionary<GameObject, GameObject>{ // swappend right <-> left sides
                    { bones[Bone.THIGH_R].gameObject, joints[JointType.HipLeft].gameObject },
                    { bones[Bone.CALF_R].gameObject,  joints[JointType.KneeLeft].gameObject },
                    { bones[Bone.FOOT_R].gameObject,  joints[JointType.AnkleLeft].gameObject },
                    { bones[Bone.BALL_R].gameObject,  joints[JointType.FootLeft].gameObject }
                }
            );

        // drawing line component only for debugging purpose
        obj.AddComponent<IKChainDrawLines>();
    }


    /// <summary>
    /// Adjust avatar to better fit users body
    /// </summary>
    public void AdjustAvatar() {
        AdjustAvatarScale();

        AdjustSpine();
        AdjustArms();
        AdjustLegs();

        // destroy old IKChains
        Destroy(spineChain.gameObject);
        Destroy(leftHandChain.gameObject);
        Destroy(rightHandChain.gameObject);
        Destroy(leftLegChain.gameObject);
        Destroy(rightLegChain.gameObject);

        // reinitialize ik chains
        MakeSpineChain();
        MakeLeftHandChain();
        MakeRightHandChain();
        MakeLeftLegChain();
        MakeRightLegChain();
    }


    /// <summary>
    /// Adjust body mesh scale
    /// </summary>
    void AdjustAvatarScale(){
        // very simple; !!!user have to stand still while adjusting!!!
        float bodyHeight   = kinectBody.MeasureBodyHeight();
        float avatarHeight = (bones[Bone.HEAD].position - (bones[Bone.FOOT_L].position + bones[Bone.FOOT_R].position) / 2.0f).magnitude;
        float proportion = (bodyHeight) / avatarHeight;
        transform.localScale = new Vector3(proportion, proportion, proportion);
    }


    /// <summary>
    /// Adjust spine length
    /// </summary>
    void AdjustSpine(){
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;
        float avatarSpineLength = 0.0f;
        float bodySpineLength   = 0.0f;

        avatarSpineLength += (bones[Bone.SPINE_1].position - bones[Bone.PELVIS].position).magnitude;
        avatarSpineLength += (bones[Bone.SPINE_2].position - bones[Bone.SPINE_1].position).magnitude;
        avatarSpineLength += (bones[Bone.SPINE_3].position - bones[Bone.SPINE_2].position).magnitude;
        avatarSpineLength += (bones[Bone.NECK].position    - bones[Bone.SPINE_3].position).magnitude;
        //avatarSpineLength += (bones[Bone.HEAD].position    - bones[Bone.NECK].position).magnitude;

        bodySpineLength += (joints[JointType.SpineMid].position      - joints[JointType.SpineBase].position).magnitude;
        bodySpineLength += (joints[JointType.SpineShoulder].position - joints[JointType.SpineMid].position).magnitude;
        bodySpineLength += (joints[JointType.Neck].position          - joints[JointType.SpineShoulder].position).magnitude;
        //bodySpineLength += (joints[JointType.Head].position          - joints[JointType.Neck].position).magnitude;

        bones[Bone.PELVIS].localPosition  *= bodySpineLength / avatarSpineLength;         
        bones[Bone.SPINE_1].localPosition *= bodySpineLength / avatarSpineLength;   
        bones[Bone.SPINE_2].localPosition *= bodySpineLength / avatarSpineLength;   
        bones[Bone.SPINE_3].localPosition *= bodySpineLength / avatarSpineLength;     
        bones[Bone.NECK].localPosition    *= bodySpineLength / avatarSpineLength;         
        //bones[Bone.HEAD].localPosition    *= bodySpineLength / avatarSpineLength; 
    }
    

    /// <summary>
    /// Adjust lengths of arms. Arms will keep equal length
    /// </summary>
    void AdjustArms(){
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;
        float avatarArmLength = 0.0f;
        float bodyArmLength   = 0.0f;

        { // left arm
            float avatarLeftClavicleLength = (bones[Bone.UPPER_ARM_L].position - bones[Bone.CLAVICLE_L].position).magnitude;
            float avatarLeftUpperArmLength = (bones[Bone.LOWER_ARM_L].position - bones[Bone.UPPER_ARM_L].position).magnitude;
            float avatarLeftLowerArmLength = (bones[Bone.HAND_L].position      - bones[Bone.LOWER_ARM_L].position ).magnitude;
            avatarArmLength += avatarLeftClavicleLength + avatarLeftUpperArmLength + avatarLeftLowerArmLength;

            float bodyLeftClavicleLength   = (joints[JointType.ShoulderRight].position - joints[JointType.SpineShoulder].position).magnitude;
            float bodyLeftUpperArmLength   = (joints[JointType.ElbowRight].position - joints[JointType.ShoulderRight].position).magnitude;
            float bodyLeftLowerArmLength   = (joints[JointType.WristRight].position - joints[JointType.ElbowRight].position).magnitude;

            bodyArmLength += bodyLeftClavicleLength + bodyLeftUpperArmLength + bodyLeftLowerArmLength;
        }

        { // right arm
            float avatarRightClavicleLength = (bones[Bone.UPPER_ARM_R].position - bones[Bone.CLAVICLE_R].position).magnitude;
            float avatarRightUpperArmLength = (bones[Bone.LOWER_ARM_R].position - bones[Bone.UPPER_ARM_R].position).magnitude;
            float avatarRightLowerArmLength = (bones[Bone.HAND_R].position      - bones[Bone.LOWER_ARM_R].position ).magnitude;

            avatarArmLength += avatarRightClavicleLength + avatarRightUpperArmLength + avatarRightLowerArmLength;

            float bodyRightClavicleLength   = (joints[JointType.ShoulderLeft].position - joints[JointType.SpineShoulder].position).magnitude;
            float bodyRightUpperArmLength   = (joints[JointType.ElbowLeft].position - joints[JointType.ShoulderLeft].position).magnitude;
            float bodyRightLowerArmLength   = (joints[JointType.WristLeft].position - joints[JointType.ElbowLeft].position).magnitude;

            bodyArmLength   += bodyRightClavicleLength + bodyRightUpperArmLength + bodyRightLowerArmLength;
        }

        avatarArmLength /= 2.0f; // arm length (average between left and right)
        bodyArmLength   /= 2.0f;

        // rescale bones
        bones[Bone.UPPER_ARM_L].localPosition *= bodyArmLength / avatarArmLength;
        bones[Bone.LOWER_ARM_L].localPosition *= bodyArmLength / avatarArmLength;
        bones[Bone.HAND_L].localPosition      *= bodyArmLength / avatarArmLength;

        bones[Bone.UPPER_ARM_R].localPosition *= bodyArmLength / avatarArmLength;
        bones[Bone.LOWER_ARM_R].localPosition *= bodyArmLength / avatarArmLength;
        bones[Bone.HAND_R].localPosition      *= bodyArmLength / avatarArmLength;
    }


    /// <summary>
    /// Adjust lengths of legs. Legs will keep equal length
    /// </summary>
    void AdjustLegs(){
        Dictionary<JointType, Transform> joints = bodyJoints;//kinectBody.joints;
        float avatarLegLength = 0.0f;
        float bodyLegLength   = 0.0f;

        {
            float avatarLeftPelvisPartLength = (bones[Bone.THIGH_L].position - bones[Bone.PELVIS].position).magnitude;
            float avatarLeftThighLength      = (bones[Bone.CALF_L].position  - bones[Bone.THIGH_L].position).magnitude;
            float avatarLeftCalfLength       = (bones[Bone.FOOT_L].position  - bones[Bone.CALF_L].position).magnitude;

            avatarLegLength += avatarLeftPelvisPartLength + avatarLeftThighLength + avatarLeftCalfLength;

            float bodyLeftPelvisLength = (joints[JointType.SpineBase].position - joints[JointType.HipRight].position).magnitude;
            float bodyLeftThighLength  = (joints[JointType.HipRight].position  - joints[JointType.KneeRight].position).magnitude;
            float bodyLeftCalfLength   = (joints[JointType.KneeRight].position - joints[JointType.AnkleRight].position).magnitude;

            bodyLegLength += bodyLeftPelvisLength + bodyLeftThighLength + bodyLeftCalfLength;
        }

        {
            float avatarRightPelvisPartLength = (bones[Bone.THIGH_R].position - bones[Bone.PELVIS].position).magnitude;
            float avatarRightThighLength      = (bones[Bone.CALF_R].position  - bones[Bone.THIGH_R].position).magnitude;
            float avatarRightCalfLength       = (bones[Bone.FOOT_R].position  - bones[Bone.CALF_R].position).magnitude;

            avatarLegLength += avatarRightPelvisPartLength + avatarRightThighLength + avatarRightCalfLength;

            float bodyRightPelvisLength = (joints[JointType.SpineBase].position - joints[JointType.HipLeft].position).magnitude;
            float bodyRightThighLength  = (joints[JointType.HipLeft].position   - joints[JointType.KneeLeft].position).magnitude;
            float bodyRightCalfLength   = (joints[JointType.KneeLeft].position  - joints[JointType.AnkleLeft].position).magnitude;

            bodyLegLength += bodyRightPelvisLength + bodyRightThighLength + bodyRightCalfLength;
        }

        avatarLegLength /= 2.0f;
        bodyLegLength   /= 2.0f;

        // rescale bones
        bones[Bone.THIGH_L].localPosition *= bodyLegLength / avatarLegLength;
        bones[Bone.CALF_L].localPosition  *= bodyLegLength / avatarLegLength;
        bones[Bone.FOOT_L].localPosition  *= bodyLegLength / avatarLegLength;

        bones[Bone.THIGH_R].localPosition *= bodyLegLength / avatarLegLength;
        bones[Bone.CALF_R].localPosition  *= bodyLegLength / avatarLegLength;
        bones[Bone.FOOT_R].localPosition  *= bodyLegLength / avatarLegLength;
    }

    /// <summary>
    /// turn on and off visibility of ik debug kines
    /// </summary>
    /// <param name="value"></param>
    public void SetIkLinesVisible(bool value){
        spineChain?.GetComponent<IKChainDrawLines>()?.SetIkLinesVisible(value);
        leftHandChain?.GetComponent<IKChainDrawLines>()?.SetIkLinesVisible(value);
        rightHandChain?.GetComponent<IKChainDrawLines>()?.SetIkLinesVisible(value);
        leftLegChain?.GetComponent<IKChainDrawLines>()?.SetIkLinesVisible(value);
        rightLegChain?.GetComponent<IKChainDrawLines>()?.SetIkLinesVisible(value);
    }


}


}// !namespace ryabomar