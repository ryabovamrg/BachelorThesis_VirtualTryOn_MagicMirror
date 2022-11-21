using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;

namespace ryabomar {
/// <summary>
/// Flat screen to display kinect color camera view
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FlatScreen : MonoBehaviour {

    /// <summary>reference to kinect data source</summary>
    public KinectDataSource kinectDataSouce;

    /// <summary>
    /// initialization
    /// </summary>
    void Start() {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.mainTexture = kinectDataSouce.colorTexture;

    }
}

}// !namespace ryabomar