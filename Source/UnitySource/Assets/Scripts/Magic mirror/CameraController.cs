using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JointType = Windows.Kinect.JointType;

namespace ryabomar {

/// <summary>
/// Controlls cameras and compute off-axis projection matrices
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>Kinect data source reference</summary>
    public KinectDataSource kinectDataSource;

    /// <summary>projection frame reference</summary>
    public ProjectionFrame projectionFrame;

    /// <summary>Camera mode</summary>
    enum MODE { OnAxis, OffAxis };

    /// <summary>Camera mode</summary>
    MODE mode = MODE.OnAxis;

    /// <summary>Field of view</summary>
    float FOV = 48.0f;

    /// <summary>Cameras to manage</summary>
    List<Camera> cameras = new List<Camera>();

    /// Initial transform properties
    Vector3 initPosition, initScale;
    Quaternion initRotation;

    bool trackUserHead = true;

    /// <summary>
    /// Iinitialization
    /// </summary>
    void Awake() {
        if(kinectDataSource == null) throw new Exception("need kinect data source");
        if(projectionFrame  == null) throw new Exception("need projection frame");

        initPosition = transform.localPosition;
        initScale    = transform.localPosition;
        initRotation = transform.localRotation;

        // find cameras in child gameobjects
        foreach (Transform child in transform) {
            child.localPosition = Vector3.zero;

            Camera camera = child.gameObject.GetComponent<Camera>();
            if(camera != null) {
                cameras.Add(camera);
            }
        }

    }

    /// <summary>
    /// Initialization
    /// </summary>
    void Start() {
        SwitchMode(mode);
    }


    public void SetTrackUserHead(bool value) {
        trackUserHead = value;
    }


    /// <summary>
    /// Update cameras
    /// </summary>
    void LateUpdate() {
        if(mode == MODE.OffAxis) { /* dynamic camera */
            // move to head position
            if(trackUserHead) {
                transform.position = CameraDynamicPosition();
            } else {
                transform.position = new Vector3(0.0f, 0.0f, -2.0f);
            }

            Vector3 eyePosition = transform.position;

            Vector3 vA = projectionFrame.pA - eyePosition;
            Vector3 vB = projectionFrame.pB - eyePosition;
            Vector3 vC = projectionFrame.pC - eyePosition;
            
            //distance from eye to projection screen plane
            float dist = -Vector3.Dot(vA, projectionFrame.vN);

            float near, far, l, r, b, t;
            near = Camera.main.nearClipPlane;
            far  = Camera.main.farClipPlane;

            l = Vector3.Dot(projectionFrame.vR, vA) * (near / dist);
            r = Vector3.Dot(projectionFrame.vR, vB) * (near / dist);
            b = Vector3.Dot(projectionFrame.vU, vA) * (near / dist);
            t = Vector3.Dot(projectionFrame.vU, vC) * (near / dist);

            Matrix4x4 projection = Matrix4x4.Frustum(l, r, b, t, near, far);

            Matrix4x4 transition = Matrix4x4.Translate(-eyePosition);
            Matrix4x4 rotation   = Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation));

            foreach(Camera camera in cameras) {
                camera.worldToCameraMatrix = projectionFrame.matrix * rotation * transition;
                camera.projectionMatrix    = projection;
            }
        }

        if(mode == MODE.OnAxis) { /* ctatic camera */
            // nothing
        }
    }


    /// <summary>
    /// Change cameras field of view
    /// </summary>
    /// <param name="value">new FOV</param>
    public void SetFOV(float value){
        FOV = value;
        if(mode != MODE.OffAxis) { // has sense only with on axis projection
            for(int i = 0; i < cameras.Count; i++) {
                cameras[i].fieldOfView = value;
            }
        }
    }

    /// <summary>
    /// Change mode to on-axis
    /// </summary>
    public void SwitchToOnAxis () { SwitchMode(MODE.OnAxis);  }
    
    
    /// <summary>
    /// Change mode to off-axis
    /// </summary>
    public void SwitchToOffAxis() { SwitchMode(MODE.OffAxis); }


    /// <summary>
    /// Change cameras mode
    /// </summary>
    /// <param name="newMode">new mode</param>
    private void SwitchMode(MODE newMode) {
        if(newMode == MODE.OnAxis) {
            // restore initial transform parameteres
            transform.localPosition = initPosition;
            transform.localPosition = initScale;
            transform.localRotation = initRotation;

            foreach(Camera camera in cameras) {
                camera.ResetProjectionMatrix();
                camera.ResetWorldToCameraMatrix();
                camera.fieldOfView = FOV;
            }
        }

        if(newMode == MODE.OffAxis) {
            foreach(Camera camera in cameras) {
                camera.ResetProjectionMatrix();
            }
        }

        mode = newMode;
    }


    /// <summary>
    /// Reflect user's head position by projection frame and use result as new cameras position
    /// </summary>
    /// <returns>calculated position</returns>
    private Vector3 CameraDynamicPosition() {
        
        var body = kinectDataSource.activeBody;
        if(body != null && body.IsTracked) {
            /* if have tracked body returns head joint position mirrored from projection frame */

            // get head position
            Vector3 headPos = utils.jointPosition(body.Joints[Windows.Kinect.JointType.Head]);

            // head position in local coordinates of projection frame
            Vector3 headPos_projectionFrameLocal = projectionFrame.transform.InverseTransformPoint(headPos);
            headPos_projectionFrameLocal.z = -headPos_projectionFrameLocal.z; // "mirroring" it by inversing Z axis sign

            // converting back into global space
            return projectionFrame.transform.TransformPoint(headPos_projectionFrameLocal);
        } else {
            /* if no body -> return point behind of projection frame */
            
            // average depth
            float avgDepth = ((float)kinectDataSource.depthMax + kinectDataSource.depthMin) / 2.0f;

            // some point befind projection frame
            Vector3 headPos_projectionFrameLocal = new Vector3(0,0, -avgDepth / 1000);

            // converting point position into global space
            return projectionFrame.transform.TransformPoint(headPos_projectionFrameLocal);
        }
    }
}

}// !namespace ryabomar