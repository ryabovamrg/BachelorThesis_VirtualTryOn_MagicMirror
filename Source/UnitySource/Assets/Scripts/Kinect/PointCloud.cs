using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

namespace ryabomar {

/// <summary>
/// Environment visualization using lots of points
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PointCloud : MonoBehaviour {

    /// <summary>Kinect data source reference</summary>
    public KinectDataSource kinectDataSouce; 

    /// <summary>Size of one point</summary>
    [Range(0.001f, 0.05f)] public float squareSize = 0.001f;

    /// <summary>Is background to be removed</summary>
    public bool removeBackground;

    /// <summary>Maximum amount of points in point cloud</summary>
    const uint CAPACITY = 512 * 424 * 16;

    /// <summary>Number of points. Default is IR camera resolution</summary>
    [Range(512 * 424 / 8, CAPACITY)] public uint nPoints = 512 * 424; // 

    /// <summary>number of points by vertical</summary>
    public int nPointsVertical;

    /// <summary>number of points by horizontal</summary>
    public int nPointsHorizontal;


    Material material;


    /// <summary>
    /// Initialization
    /// </summary>
    void Start() {
        if(kinectDataSouce == null) throw new Exception("need KinectDataSource"); // check kinect
        

        { // mesh and material
            GetComponent<MeshFilter>().mesh = makeMeshOfDisconnectedVertices(CAPACITY);

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            material = renderer.material;

            material.SetInt("depthFrameWidth",  kinectDataSouce.depthFrameDesc.Width);
            material.SetInt("depthFrameHeight", kinectDataSouce.depthFrameDesc.Height);

            material.SetInt("colorFrameWidth",  kinectDataSouce.colorFrameDesc.Width);
            material.SetInt("colorFrameHeight", kinectDataSouce.colorFrameDesc.Height);

            material.SetTexture("_MainTex", kinectDataSouce.colorTexture);

            material.SetInt("nPointsHorizontal", 512);
            material.SetInt("nPointsVertical", 424);

            material.SetTexture("_bakedPositions", kinectDataSouce.bakedPositionsTexture);
            material.SetTexture("_bakedUVs",       kinectDataSouce.bakedUVsTexture);
            material.SetTexture("_bakedBodyIndexes", kinectDataSouce.bodyIndexTexture);

            UnityEngine.Vector4 positionOffset = new UnityEngine.Vector4(0.0f,0.0f,-6.0f,0.0f);
            material.SetVector("positionOffset", positionOffset);
        }
    }


    /// <summary>
    /// Update points using most resent sensor data
    /// </summary>
    void Update() {
        material.SetInt("removeBackground", removeBackground ? 1 : 0);
        material.SetFloat("squareSize", squareSize);

        double ratio = 512.0f / 424.0f; // width to height ratio
        
        double height = Math.Sqrt((double)nPoints / ratio);
        double width  = height * ratio;

        nPointsHorizontal = (int)Math.Floor(width);
        nPointsVertical = (int)Math.Floor(height);

        material.SetInt("nPointsHorizontal", nPointsHorizontal);
        material.SetInt("nPointsVertical", nPointsVertical);
    }


    /// <summary>
    /// Generate mesh of disconnected verticies
    /// </summary>
    /// <param name="nVertices">number of verticies</param>
    /// <returns></returns>
    static private Mesh makeMeshOfDisconnectedVertices(uint nVertices) {
        // create new mesh
        Mesh mesh = new Mesh();

        if(nVertices >= 65535) {
            // to make mesh of more then 65535 vertices
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
        }

        Vector3[] vertices = new Vector3[nVertices];
        Vector2[] uv       = new Vector2[nVertices];
        int[]     indices  = new int    [nVertices];

        for(int i =0; i < nVertices; ++i) {
            indices [i] = i;
            vertices[i] = Vector3.zero;
            uv      [i] = Vector2.zero;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

            // large bounds prevents mesh culling
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10.0f);

        return mesh;
    }

    /// <summary>
    /// Change single point size
    /// </summary>
    /// <param name="value">new size</param>
    public void SetPointSize(float value) {
        squareSize = value;
    }


    /// <summary>
    /// Change number of moints
    /// </summary>
    /// <param name="value">new number of points</param>
    public void SetPointCount(uint value) {
        nPoints = value;
    }


    /// <summary>
    /// Change number of points
    /// </summary>
    /// <param name="value">new number of points</param>
    public void SetPointCount(float value) {
        nPoints = (uint)value;
    }

    /// <summary>
    /// Turn background removal
    /// </summary>
    /// <param name="value"></param>
    public void RemoveBackground(bool value) {
        removeBackground = value;
    }
}

}// !namespace ryabomar