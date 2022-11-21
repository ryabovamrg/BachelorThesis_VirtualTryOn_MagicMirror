using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {


/// <summary>
/// Grabs other object on scene based on hand state
/// </summary>
public class ObjectHandGrabber : MonoBehaviour
{
    /// <summary>Reference to kinect body</summary>
    public KinectBody kinectBody;

    /// <summary>left or right hand</summary>
    [System.Serializable] public enum Hand { LEFT, RIGHT }

    /// <summary>hand side</summary>
    public Hand hand;

    /// <summary>red material to show then hand is closed</summary>
    public Material redMaterial;

    /// <summary>default materal to show then hand is not closed</summary>
    Material defaultMaterial;

    // renderer component
    MeshRenderer meshRenderer;

    // object to grab
    Collider objToGrab = null;
    
    // position from previous frame
    Vector3 prevPosition;

    // hand state
    public bool isClosed;

    /// <summary>
    /// Initializations
    /// </summary>
    void Start(){
        if(kinectBody == null) throw new System.Exception("need KinectBodyVisualization");
        prevPosition = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.material;
    }

    /// <summary>
    /// Update
    /// </summary>
    void Update(){
        if(hand == Hand.LEFT){
            transform.position = kinectBody.joints[Windows.Kinect.JointType.HandLeft].position;
            isClosed = kinectBody.isLeftHandClosed;
        } else {
            transform.position = kinectBody.joints[Windows.Kinect.JointType.HandRight].position;
            isClosed = kinectBody.isRightHandClosed;
        }

        if(isClosed){
            meshRenderer.material = redMaterial;
        } else {
            meshRenderer.material = defaultMaterial;
        }
    }

    /// <summary>
    /// trigger on grabbing object
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other){
        if(other.tag != "PlaygroundPrimitive") return;

        if(!isClosed && objToGrab == null){
            //Debug.Log("ready to grab: " + other.name);
            objToGrab = other;
            objToGrab.attachedRigidbody.useGravity = false;
            //other.attachedRigidbody.isKinematic = true;
        }
    }

    /// <summary>
    /// trigger on grabbing object
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other){
        if(other.tag != "PlaygroundPrimitive") return;

        if(objToGrab == other){
            //Debug.Log("releasing: " + objToGrab.name);
            objToGrab.attachedRigidbody.useGravity = true;
            objToGrab = null;
        }

        if(!isClosed && objToGrab == other){
            
            //other.attachedRigidbody.isKinematic = false;
            
        }
    }

    /// <summary>
    /// move grabbed object
    /// </summary>
    void FixedUpdate(){
        if(objToGrab != null && isClosed){
            //Debug.Log("moving grabbed object: " + objToGrab.name);
            Vector3 deltaPos = transform.position - this.prevPosition;
            objToGrab.attachedRigidbody.velocity = Vector3.zero;
            objToGrab.attachedRigidbody.MovePosition(objToGrab.transform.position + deltaPos);
        }
        this.prevPosition = transform.position;
    }
}

} //!namespace ryabomar