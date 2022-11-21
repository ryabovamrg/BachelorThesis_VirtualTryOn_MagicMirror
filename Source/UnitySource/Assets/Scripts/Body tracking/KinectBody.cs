using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

namespace ryabomar {

/// <summary>
/// Representation of kinect body joints
/// </summary>
public class KinectBody : MonoBehaviour
{
    /// <summary>reference to kinect data source</summary>
    public KinectDataSource kinectDataSource;

    /// <summary>joint prefab</summary>
    public GameObject jointPrefab;

    /// <summary>head joint prefab</summary>
    public GameObject headPrefab;

    /// <summary>hand marker prefab</summary>
    public GameObject handStateMarkerPrefab;

    /// <summary>is joint visible</summary>
    public bool drawJoints           = true;

    /// <summary>is lines between joints visible</summary>
    public bool drawLines            = true;

    /// <summary>is hand markers visible</summary>
    public bool drawHandStateMarkers = true;

    /// <summary>is head visible</summary>
    public bool drawHeadMarker       = true;

    /// <summary>is left hand is in close state</summary>
    public bool isLeftHandClosed  { get; private set; }

    /// <summary>is right hand is in close state</summary>
    public bool isRightHandClosed { get; private set; }

    /// <summary>joint position lerping coefficient</summary>
    [Range(0.0f, 1.0f)] public float lerpPositions = 0.0f;

    /// <summary>body vertical shift</summary>
    [Range(0.0f, 2.0f)] public float verticalOffset = 0.0f;

    /// <summary>is fase tracked</summary>
    public bool isFaceTracked = false;

    /// <summary>height if the body</summary>
    public float bodyHeight;

    /// <summary>head marker reference</summary>
    public Transform headMarker;

    /// <summary>joint objects</summary>
    public Dictionary<JointType, Transform> joints = new Dictionary<JointType, Transform>();

    /// <summary>hand markers</summary>
    SpriteRenderer leftHandStateMarker, rightHandStateMarker;

    /// <summary>is update stopped</summary>
    public bool isFreezed = false;

    /// <summary>stop update</summary>
    public void SetFreezed(bool value){
        isFreezed = value;
    }

    /// <summary>set position lerp coefficient</summary>
    public void SetLerpPositions(float value){
        lerpPositions = value;
    }

    /// <summary>set body vertical offset</summary>
    public void SetVerticalOffset(float value){
        verticalOffset = value;
    }


    /// <summary>turn lines visibility</summary>
    public void SetDrawingLines(bool value){
        drawLines = value;
        foreach (var entry in joints) {
            LineRenderer lineRenderer = entry.Value.GetComponent<LineRenderer>();
            if(lineRenderer != null)
                lineRenderer.enabled = value;
        }
    }

    /// <summary>turn joints visibility</summary>
    public void SetDrawingJoints(bool value){
        drawJoints = value;
        foreach (var entry in joints) {
            MeshRenderer meshRenderer = entry.Value.GetComponent<MeshRenderer>();
            if(meshRenderer != null)
                meshRenderer.enabled = value;
        }
        
        foreach(Transform headPart in headMarker.transform){
            headPart.gameObject.SetActive(value);
        }
    }

    /// <summary>turn hand markers visibility</summary>
    public void SetDrawingHandStateMarkers(bool value){
        drawHandStateMarkers = value;
        leftHandStateMarker.enabled  = value;
        rightHandStateMarker.enabled = value;
    }


    /// <summary>turn head visibility</summary>
    public void SetDrawingHeadMarker(bool value){
        drawHeadMarker = value;
        headMarker.gameObject.SetActive(value);
    }


    /// <summary>
    /// initialization
    /// </summary>
    void Awake(){
        // check kinect data source
        if(kinectDataSource == null) throw new Exception("need KinectDataSource");

        InstantiateJoints();
        InstantiateHeadMarker();
        InstantiateHandStateMarkers();

        SetDrawingJoints(drawJoints);
        SetDrawingLines(drawLines);
        SetDrawingHandStateMarkers(drawHandStateMarkers);
        SetDrawingHeadMarker(drawHeadMarker);
    }

    /// <summary>
    /// joints instantiation
    /// </summary>
    void InstantiateJoints(){
        if(jointPrefab == null || headPrefab == null) {
            throw new Exception("need prefabs");
        }
        
        gameObject.layer = LayerMask.NameToLayer("debug"); 

        for(JointType jointType = JointType.SpineBase; jointType <= JointType.ThumbRight; jointType++) {

            GameObject joint = transform.Find(jointType.ToString())?.gameObject;

            if(joint == null) {
                joint = Instantiate(jointPrefab);
                joint.name = jointType.ToString();
            }

            joint.SetActive(true); // in case if prefab disabled
            joint.layer = LayerMask.NameToLayer("debug"); 
            joint.transform.parent        = transform;      // joint will be a child
            joint.transform.localPosition = Vector3.zero;
            joints[jointType] = joint.transform;

            if(utils.BONE_MAP.ContainsKey(jointType)) {
                // will drow the line if joint has other joint on the end
                LineRenderer lineRenderer = joint.GetComponent<LineRenderer>();
                if(lineRenderer == null) {
                    lineRenderer = joint.AddComponent<LineRenderer>();
                }
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.material.color = UnityEngine.Color.white;
                lineRenderer.positionCount  = 2;
                lineRenderer.startWidth     = 0.01f;
                lineRenderer.endWidth       = 0.01f;
            }
        }
    }

    /// <summary>
    /// hand markers instantiation
    /// </summary>
    void InstantiateHandStateMarkers(){
            if(handStateMarkerPrefab == null) {
                throw new Exception("need prefabs");
            }
                    // GameObject leftHandMarker_obj = transform.Find("left hand state marker")?.gameObject;
            // if(leftHandMarker_obj == null){
            //     leftHandMarker_obj = GameObject.Instantiate(handStateMarkerPrefab);
            //     leftHandMarker_obj.name = "left hand state marker";
            // }
            
            GameObject leftHandMarker_obj = GameObject.Instantiate(handStateMarkerPrefab);
            leftHandMarker_obj.name = "left hand state marker";
            leftHandMarker_obj.transform.localPosition = Vector3.zero;
            leftHandMarker_obj.transform.parent = transform;
            leftHandMarker_obj.layer = LayerMask.NameToLayer("debug"); 
            leftHandMarker_obj.SetActive(true);

            leftHandStateMarker = leftHandMarker_obj.GetComponent<SpriteRenderer>();


            // GameObject rightHandMarker_obj = transform.Find("right hand state marker")?.gameObject;
            // if(rightHandMarker_obj == null){
            //     rightHandMarker_obj = GameObject.Instantiate(handStateMarkerPrefab);
            //     rightHandMarker_obj.name = "right hand state marker";
            // }

            GameObject rightHandMarker_obj = GameObject.Instantiate(handStateMarkerPrefab);
            rightHandMarker_obj.name = "right hand state marker";
            rightHandMarker_obj.transform.localPosition = Vector3.zero;
            rightHandMarker_obj.transform.parent = transform;
            rightHandMarker_obj.layer = LayerMask.NameToLayer("debug"); 
            rightHandMarker_obj.SetActive(true);

            rightHandStateMarker = rightHandMarker_obj.GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// head instantiation
    /// </summary>
    void InstantiateHeadMarker(){
        GameObject obj = transform.Find("head marker")?.gameObject;

        if(obj == null){
            obj = GameObject.Instantiate(headPrefab);
            obj.transform.parent = transform;
            obj.name = "head marker";
        }

        obj.layer = LayerMask.NameToLayer("debug"); 
        foreach(Transform child in obj.transform){
            child.gameObject.layer = LayerMask.NameToLayer("debug"); 
        }

        headMarker = obj.transform;
    }

    /// <summary>
    /// refresh all parts
    /// </summary>
    void Update(){
        // no updates then freezed
        if(isFreezed) return;

        UpdateJoints();
        UpdateHeadMarker();
        UpdateHandStateMarkers();
        MeasureBodyHeight();

        if(kinectDataSource.activeBody != null && kinectDataSource.activeBody.IsTracked){
            isLeftHandClosed  = kinectDataSource.activeBody.HandLeftState  == HandState.Closed;
            isRightHandClosed = kinectDataSource.activeBody.HandRightState == HandState.Closed;
        } else {
            isLeftHandClosed  = false;
            isRightHandClosed = false;
        }

    }

    /// <summary>
    /// update joints potions
    /// </summary>
    void UpdateJoints() {
        Body body = kinectDataSource.activeBody;
        if(body == null) return;  

        foreach (var entry in joints) {

            JointType jointType = entry.Key;
            Transform joint     = entry.Value;

            Vector3 jointPosition = utils.jointPosition(body.Joints[jointType]);
            jointPosition.y += verticalOffset;
            
			joint.localPosition = Vector3.Lerp(jointPosition, joint.localPosition, lerpPositions);
			joint.localRotation = utils.jointRotation(body.JointOrientations[jointType]);


            { // draw line
                // skip if joint is last in hierarchy
                if(!utils.BONE_MAP.ContainsKey(jointType)) continue;

                // draw line between joints
                LineRenderer lineRenderer = joint.GetComponent<LineRenderer>();

                // line color depends of state
                UnityEngine.Color color;
                switch(body.Joints[jointType].TrackingState){
                    case TrackingState.Tracked:     color = UnityEngine.Color.green;    break;
                    case TrackingState.Inferred:    color = UnityEngine.Color.red;      break;
                    default:                        color = UnityEngine.Color.black;    break;
                }
                
                lineRenderer.startColor = color;
                lineRenderer.endColor   = color;

                Vector3 endPos = utils.jointPosition(body.Joints[utils.BONE_MAP[jointType]]);

                lineRenderer.SetPosition(0, joint.position);
                lineRenderer.SetPosition(1, transform.TransformPoint(endPos));
            }
        }
    }

    /// <summary>
    /// update hand markers
    /// </summary>
    void UpdateHandStateMarkers(){
        Body body = kinectDataSource.activeBody;
        if(body == null) return;  

        leftHandStateMarker.transform.localPosition  = joints[JointType.HandLeft].localPosition;
        rightHandStateMarker.transform.localPosition = joints[JointType.HandRight].localPosition;

        UnityEngine.Color colorClosed = UnityEngine.Color.red;
        UnityEngine.Color colorOpen   = UnityEngine.Color.green;
        UnityEngine.Color colorOther  = UnityEngine.Color.grey;

        switch (body.HandLeftState) {
            case HandState.Open  : leftHandStateMarker.color = colorOpen;   break;
            case HandState.Closed: leftHandStateMarker.color = colorClosed; break;
            default              : leftHandStateMarker.color = colorOther;  break;
        }

        switch (body.HandRightState) {
            case HandState.Open  : rightHandStateMarker.color = colorOpen;   break;
            case HandState.Closed: rightHandStateMarker.color = colorClosed; break;
            default              : rightHandStateMarker.color = colorOther;  break;
        }
    }


    /// <summary>
    /// update head
    /// </summary>
    void UpdateHeadMarker() {
        if(kinectDataSource.activeBody == null || kinectDataSource.activeBodyFace == null) {
            return;
        }

        // apply position
        headMarker.position = joints[JointType.Head].position;

        // try apply rotation from Kinect.Face
        if(kinectDataSource.activeBodyFace != null) {
            isFaceTracked = true;
            // traking fase ==>> use its rotation
            var quat = kinectDataSource.activeBodyFace.FaceRotationQuaternion;
            headMarker.rotation = Quaternion.Lerp(
                    headMarker.rotation,
                    new Quaternion(quat.X, quat.Y, quat.Z, quat.W),
                    0.25f
                ); // smooth rotating using Lerp
        } else {
            isFaceTracked = false;
            // no tracking face but still tracking body ==>> use rotation of head joint
            headMarker.rotation = joints[JointType.Head].rotation;
        }

    }

    /// <summary>
    /// measure body height
    /// </summary>
    /// <returns>approximation of the user's body height</returns>
    public float MeasureBodyHeight() {
        // very simple implementation: works only if user stands still
        float lowestBodyPoint = Math.Min(
            joints[JointType.FootLeft].position.y,
            joints[JointType.FootRight].position.y
        );
        float hightestBodyPoint = joints[JointType.Head].position.y;
        bodyHeight = hightestBodyPoint - lowestBodyPoint;
        //bodyHeight += 0.1f; // head joint is in the center of the head; not at the top... so this should fix it
        return bodyHeight;
    }
}

}// !namespace ryabomar