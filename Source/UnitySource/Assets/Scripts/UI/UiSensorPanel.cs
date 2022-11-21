using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ryabomar {

/// <summary>
/// Shows kinect data in textures and FPS meter
/// </summary>
public class UiSensorPanel : MonoBehaviour
{
    /// <summary>reference to kinect data source</summary>
    public KinectDataSource kinectDataSouce;

    /// <summary>reference to ui image for color camera view</summary>
    public RawImage colorCameraImage;

    /// <summary>reference to ui image for depth data</summary>
    public RawImage depthCameraView;

    /// <summary>reference to ui image for body index data</summary>
    public RawImage bodyIndexView;

    /// <summary>reference to ui image for point cloud positions data</summary>
    public RawImage bakedPositions;

    /// <summary>reference to ui image for point cloud UVs data</summary>
    public RawImage bakedUVs;

    /// <summary>reference to ui text for FPS</summary>
    public Text fpsText;


    /// <summary>
    /// Initialization
    /// </summary>
    void Start() {
        if(kinectDataSouce == null) throw new Exception("need KinectDataSouce");
        
        colorCameraImage.texture = kinectDataSouce.colorTexture;
        depthCameraView.texture = kinectDataSouce.depthTexture;
        bodyIndexView.texture   = kinectDataSouce.bodyIndexTexture;
        bakedPositions.texture  = kinectDataSouce.bakedPositionsTexture;
        bakedUVs.texture        = kinectDataSouce.bakedUVsTexture;
    }


    /// <summary>
    /// update fps indicator
    /// </summary>
    void LateUpdate() {

        fpsText.text = "FPS: " + (1.0f / Time.smoothDeltaTime).ToString("0.0");
    }
}

} //!namespace ryabomar