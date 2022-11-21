using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Microsoft.Kinect;
using JointType = Windows.Kinect.JointType;

namespace ryabomar {

/// <summary>
/// Pseudo cursor for very besic gesture input 
/// </summary>
public class HandCursor : MonoBehaviour
{
    /// <summary>
    /// reference to canvas raycaster
    /// </summary>
    public GraphicRaycaster canvasRaycaster;

    /// <summary>
    /// reference to kinect data source
    /// </summary>
    public KinectDataSource kinectDataSource;

    /// <summary>
    /// which hand
    /// </summary>
    [Serializable] public enum Hand  { LEFT, RIGHT };

    /// <summary>
    /// which hand
    /// </summary>
    public Hand hand;

    /// <summary>
    /// click threshold
    /// </summary>
    [Range(0.0f, 5.0f)] public float thresholdTime = 2.0f;


    /// <summary>
    /// open stae indicator
    /// </summary>
    public GameObject imageOpen;

    /// <summary>
    /// close state indicator
    /// </summary>
    public GameObject imageClosed;

    /// <summary>
    /// red circle indicator
    /// </summary>
    public RectTransform holdingIndicator;

    /// <summary>
    /// accumulated time of holding hand closed
    /// </summary>
    float holdingTime = 0.0f;

    /// <summary>
    /// Hand state
    /// </summary>
    enum State { OPEN, CLOSED, UNTRACKED };

    /// <summary>
    /// Hand state
    /// </summary>
    State state = State.UNTRACKED;

    /// <summary>
    /// Initial cursor position
    /// </summary>
    Vector3 initPosition;

    /// <summary>
    /// screen position horizontal multiplier
    /// </summary>
    float vertMult = 2.0f;

    /// <summary>
    /// screen position horizontal multiplier
    /// </summary>
    float horMult  = 2.0f;


    /// <summary>
    /// Change click threshold time
    /// </summary>
    /// <param name="value"></param>
    public void SetThresholdTime(float value){
        thresholdTime = value;
    }

    /// <summary>
    /// change horizontal position multiplier
    /// </summary>
    /// <param name="value"></param>
    public void SetHorizontalMultiplier(float value){
        horMult = value;
    }


    /// <summary>
    /// change vertical position multiplier
    /// </summary>
    /// <param name="value"></param>
    public void SetVerticalMultiplier(float value){
        vertMult = value;
    }


    /// <summary>
    /// Initialization
    /// </summary>
    void Start(){
        if(canvasRaycaster  == null) throw new Exception("need canvas raycaster");
        if(kinectDataSource == null) throw new Exception("need kinect data source");
        initPosition = transform.position;
    }


    /// <summary>
    /// move cursor and click
    /// </summary>
    void Update(){
        Vector3 newPos = ScreenSpaceKinectHandPosition();

        Vector2 screenCenter = ScreenSpaceKinectSpineMidPosition();
        //new Vector2((float)Screen.width  / 2.0f, (float)Screen.height / 2.0f);

        newPos.x = newPos.x - screenCenter.x;
        newPos.y = newPos.y - screenCenter.y;

        newPos.x *= horMult;
        newPos.y *= vertMult;

        newPos.x = newPos.x + screenCenter.x;
        newPos.y = newPos.y + screenCenter.y;

        transform.position = newPos;

        if(state != State.UNTRACKED && holdingTime >= thresholdTime){
            //Debug.Log("click!");
            SimulateClick();
            holdingTime = 0.0f;
        }
    }

    /// <summary>
    /// simulate click on current cursor position
    /// </summary>
    void SimulateClick(){
        //https://answers.unity.com/questions/1823687/how-would-i-go-about-create-multiple-cursors-contr.html
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = transform.position;
        pointer.button   = PointerEventData.InputButton.Left;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        canvasRaycaster.Raycast(pointer, raycastResults);

        foreach (RaycastResult raycastResult in raycastResults) {
            var ui = raycastResult.gameObject.GetComponent<UIBehaviour>();
            if (ui != null) {
                //if (Input.GetKeyDown(KeyCode.Space)) {
                //Debug.Log($"Clicked on {raycastResult.gameObject}");
                //ExecuteEvents.Execute(raycastResult.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                ExecuteEvents.ExecuteHierarchy(raycastResult.gameObject, pointer, ExecuteEvents.pointerClickHandler); //fix for hierarchical widgets such as toggle and other
                //}
            }
        }
    }

    /// <summary>
    /// map body center coordinate to screen space
    /// </summary>
    /// <returns>body center screen space coordinate</returns>
    Vector3 ScreenSpaceKinectSpineMidPosition(){
        if(kinectDataSource.activeBody == null || !kinectDataSource.activeBody.IsTracked){
            return new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
        } else {
            Vector3 worldPos = utils.jointPosition(kinectDataSource.activeBody.Joints[JointType.SpineMid]);
            return Camera.main.WorldToScreenPoint(worldPos);
        }
    }

    /// <summary>
    /// map position of hand joint to screen space
    /// </summary>
    /// <returns></returns>
    Vector3 ScreenSpaceKinectHandPosition(){
        
        if(kinectDataSource.activeBody == null || !kinectDataSource.activeBody.IsTracked) {
            state = State.UNTRACKED;
            return initPosition;
        } else {
            Vector3 worldPosition;
            Windows.Kinect.HandState kinectState;

            if(hand == Hand.LEFT){
                worldPosition = utils.jointPosition(kinectDataSource.activeBody.Joints[JointType.HandLeft]);
                kinectState   = kinectDataSource.activeBody.HandLeftState;

            } else {
                worldPosition = utils.jointPosition(kinectDataSource.activeBody.Joints[JointType.HandRight]);
                kinectState   = kinectDataSource.activeBody.HandRightState;
            }
            
            switch(kinectState){
                case (Windows.Kinect.HandState.Closed): SetState(State.CLOSED);     break;
                case (Windows.Kinect.HandState.Open)  : SetState(State.OPEN);       break;
                default                               : SetState(State.UNTRACKED);  break;
            }
            return Camera.main.WorldToScreenPoint(worldPosition);
        }
    }


    /// <summary>
    /// change cursor state
    /// </summary>
    /// <param name="newState"></param>
    void SetState(State newState){

        if(state == State.CLOSED && newState == State.CLOSED) {
            holdingTime += Time.deltaTime;
        } else {
            holdingTime = 0.0f;
        }

        

        if(newState != State.UNTRACKED && state != newState){
            imageOpen?.SetActive  (newState == State.OPEN);
            imageClosed?.SetActive(newState == State.CLOSED);
        }

        state = newState;

        if(holdingIndicator != null) {
            
            holdingIndicator.gameObject.SetActive(state == State.CLOSED);
            
            //holdingIndicator.gameObject.SetActive(holdingTime >= 0.0f);

            float scale = 1.0f - (holdingTime / thresholdTime);
            scale = Math.Max(scale, 0.01f);
            scale *= 100;
            holdingIndicator.sizeDelta = new Vector2(scale, scale);
        }
    }

}

} //!namespace ryabomar