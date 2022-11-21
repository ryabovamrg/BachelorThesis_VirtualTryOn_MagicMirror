using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using System.Runtime.InteropServices;

namespace ryabomar {

/// <summary>
/// Source of the Kinect sensor data
/// </summary>
public class KinectDataSource : MonoBehaviour
{
    // ----------------------------------------------------------- textures ---v
    ///<summary>color camera as texture (updates continually)</summary>             
    public Texture2D colorTexture               { get; private set; }

    ///<summary>depth data in texture</summary>
    public RenderTexture depthTexture           { get; private set; }

    ///<summary>body indexes as masks</summary>
    public RenderTexture bodyIndexTexture       { get; private set; }

    ///<summary>color space points in dexrure (RenderTextureFormat.ARGBFloat)</summary>
    public RenderTexture bakedPositionsTexture  { get; private set; }

    ///<summary>color space points in dexrure (RenderTextureFormat.RGFloat)</summary>
    public RenderTexture bakedUVsTexture        { get; private set; }

    // ------------------------------------------------- frame descriptions ---x

    ///<summary>description of color frame</summary>
    public FrameDescription colorFrameDesc      { get; private set; }

    ///<summary>description of depth frame</summary>
    public FrameDescription depthFrameDesc      { get; private set; }

    // ------------------------------------------------------- data buffers ---x

    ///<summary>color data for last frame</summary>
    public byte  [] colorBuffer                 { get; private set; }

    ///<summary>depth data for last frame</summary>
    public ushort[] depthDataBuffer             { get; private set; }

    ///<summary>body indices in depth frame </summary>
    public byte  [] bodyIndexBuffer             { get; private set; }

    ///<summary>bodies in last frame</summary>
    public Body            [] bodyData          { get; private set; }

    ///<summary>points in IR camera space (X,Y)</summary>
    public DepthSpacePoint [] depthSpacePoints  { get; private set; }

    ///<summary>points in color camera space (X,Y)</summary>
    public ColorSpacePoint [] colorSpacePoints  { get; private set; }

    ///<summary>points in camera(world) space (X,Y,Z)</summary>
    public CameraSpacePoint[] cameraSpacePoints { get; private set; }

    // ------------------------------------------------------ miscellaneous ---x
    ///<summary>to convert depth data into points and points from one space to another</summary>
    public CoordinateMapper coordinateMapper    { get; private set; }

    ///<summary>minimum reliable depth (should be multiplied by 0.0001f to become world space coordinate)</summary>
    public ushort depthMin                      { get; private set; }

    ///<summary>maximum reliable depth (should be multiplied by 0.0001f to become world space coordinate)</summary>
    public ushort depthMax                      { get; private set; }

    // --------------------------------------------------- active body data ---x
    ///<summary>First tracked body</summary>
    public Body            activeBody           { get; private set; }

    ///<summary>face of first tracked body</summary>
    public FaceFrameResult activeBodyFace       { get; private set; }


    // ---------------------------------------------------- compute buffers ---X
    /// <summary>depth data uploaded to gpu</summary>
    public ComputeBuffer depthData_computeBuffer         { get; private set; }

    /// <summary>depth space points data uploaded to gpu</summary>
    public ComputeBuffer depthSpacePoints_computeBuffer  { get; private set; }

    /// <summary>color space points data uploaded to gpu</summary>
    public ComputeBuffer colorSpacePoints_computeBuffer  { get; private set; }

    /// <summary>camera space points data uploaded to gpu</summary>
    public ComputeBuffer cameraSpacePoints_computeBuffer { get; private set; }

    /// <summary>body indecies data uploaded to gpu</summary>
    public ComputeBuffer bodyIndex_computeBuffer         { get; private set; }

    /// <summary>shader for data processing on gpu</summary>
    public ComputeShader computeShader;                                         
    // ------------------------------------------------------------------------^

    // ---------------------------------------------------- private members ---v
    KinectSensor            sensor;

    ColorFrameReader        colorFrameReader;
    DepthFrameReader        depthFrameReader;
    InfraredFrameReader     irFrameReader;
    BodyFrameReader         bodyFrameReader;
    BodyIndexFrameReader    bodyIndexFrameReader;

    FaceFrameReader activeBodyFaceReader;
    FaceFrameSource activeBodyFaceSource;
    // --------------------------------------------------- !private members ---^
    
    // ------------------------------------------------------ public methods
    /// <summary>
    /// Set first tracked body as active body
    /// </summary>
    void SetFirstTrackedBodyAsActiveBody() {
        for(int i = 0; i < bodyData.Length; i++) {
            if(bodyData[i] != null && bodyData[i].IsTracked) {
                activeBody = bodyData[i];
            }  
        }
    }

    // ===================================================================================== MonoBehaviour methods ====v
    /// <summary>kinect sensor initialization</summary>
    void Awake() {
        sensor = KinectSensor.GetDefault(); 
            if(sensor == null) throw new Exception("Could not get kinect sensor");

        { // initialize frame reader
            bodyIndexFrameReader = sensor.BodyIndexFrameSource.OpenReader();
            bodyFrameReader      = sensor.BodyFrameSource.OpenReader();
            colorFrameReader     = sensor.ColorFrameSource.OpenReader();
            depthFrameReader     = sensor.DepthFrameSource.OpenReader();
            irFrameReader        = sensor.InfraredFrameSource.OpenReader();
        }

        { // frame descriptions, constants, etc
            colorFrameDesc      = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            depthFrameDesc      = sensor.DepthFrameSource.FrameDescription;

            depthMin = sensor.DepthFrameSource.DepthMinReliableDistance;
            depthMax = sensor.DepthFrameSource.DepthMaxReliableDistance;

            coordinateMapper = sensor.CoordinateMapper;
        }

        { // initialize buffers
            bodyIndexBuffer = new byte  [depthFrameDesc.Width * depthFrameDesc.Height];
            colorBuffer     = new byte  [colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];
            depthDataBuffer = new ushort[depthFrameDesc.LengthInPixels];

            bodyData        = new Body[sensor.BodyFrameSource.BodyCount];

            colorTexture    = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);

            depthSpacePoints  = new DepthSpacePoint [colorFrameDesc.Width * colorFrameDesc.Height];
            colorSpacePoints  = new ColorSpacePoint [depthFrameDesc.LengthInPixels];
            cameraSpacePoints = new CameraSpacePoint[depthFrameDesc.LengthInPixels];
        }

        if(!sensor.IsOpen) sensor.Open();

        InitializeFaceTracking();

        InitializeComputeBuffers();
        InitializeComputeSaders();
    }


    /// <summary>
    /// Initialize face tracking
    /// </summary>
    void InitializeFaceTracking(){
        if(sensor == null) return;

        activeBodyFaceSource = FaceFrameSource.Create(sensor, 0,
                // FaceFrameFeatures.BoundingBoxInColorSpace   |
                // FaceFrameFeatures.PointsInColorSpace        |
                // FaceFrameFeatures.PointsInInfraredSpace     |
                // FaceFrameFeatures.FaceEngagement            |
                // FaceFrameFeatures.Happy                     |
                // FaceFrameFeatures.MouthOpen                 |
                // FaceFrameFeatures.Glasses                   |
                // FaceFrameFeatures.LeftEyeClosed             |
                // FaceFrameFeatures.RightEyeClosed            |
                FaceFrameFeatures.RotationOrientation
            );

        if(activeBodyFaceSource == null) {
            Debug.Log("could not create face source");
            //throw new Exception("face source is null");
        }

        activeBodyFaceReader = activeBodyFaceSource?.OpenReader();
    }

    /// <summary>
    /// Update face tracking
    /// </summary>
    void UpdateFaceTracking(){
        if(activeBodyFaceSource != null && activeBodyFaceReader != null){
            // setup fase source for first tracked body (only)
            if(activeBody != null){
                activeBodyFaceSource.TrackingId = activeBody.TrackingId;

                using(var faceFrame = activeBodyFaceReader.AcquireLatestFrame()){
                    if(faceFrame != null) {
                        activeBodyFace = faceFrame.FaceFrameResult;
                    } else {
                        activeBodyFace = null;
                    }
                
                }
            }
        } else {
            InitializeFaceTracking();
        }

    }


    /// <summary>Update data from the Kinect sensor</summary>
    void Update() {
        if (sensor == null) return; // kinect not initialized
        
        ReadColorFrame();
        ReadDepthFrame();
        ReadBodyFrame();
        ReadBodyIndexFrame();

        UpdateComputeBuffers();
        InvokeComputeShaders();

        SetFirstTrackedBodyAsActiveBody();

        UpdateFaceTracking();
    }

    
    /// <summary> 
    /// Dispose kinect sensor on quit
    ///</summary>
    void OnApplicationQuit() {
        colorFrameReader?.Dispose();
        depthFrameReader?.Dispose();
        irFrameReader?.Dispose();
        bodyFrameReader?.Dispose();
        bodyIndexFrameReader?.Dispose();

        activeBody = null;
        activeBodyFace = null;

        activeBodyFaceReader?.Dispose();
        //activeBodyFaceSource // ???
        sensor?.Close();

        sensor = null;

        colorFrameReader = null;
        depthFrameReader = null;
        irFrameReader    = null;
        bodyFrameReader  = null;
        bodyIndexFrameReader = null;

        bodyIndexBuffer = null;
        depthDataBuffer = null;

        activeBodyFaceReader = null;

        ReleaseComputeBuffers();
        ReleaseComputeSaders();
    }
    // ====================================================================================! MonoBehaviour methods ====^



    // ====================================================================================== internal use methods ====v

    /// <summary>
    /// Read color data
    /// </summary>
    void ReadColorFrame() {
        using (var frame = colorFrameReader.AcquireLatestFrame()){
            if (frame != null) {
                frame.CopyConvertedFrameDataToArray(colorBuffer, ColorImageFormat.Rgba);
                colorTexture.LoadRawTextureData(colorBuffer);
                colorTexture.Apply();
            }
        }
    }

    /// <summary>
    /// Read depth data
    /// </summary>
    void ReadDepthFrame() {
        using (var frame = depthFrameReader.AcquireLatestFrame()){
            if (frame != null) {
                frame.CopyFrameDataToArray(depthDataBuffer);
                coordinateMapper.MapColorFrameToDepthSpace (depthDataBuffer, depthSpacePoints);
                coordinateMapper.MapDepthFrameToColorSpace (depthDataBuffer, colorSpacePoints);
                coordinateMapper.MapDepthFrameToCameraSpace(depthDataBuffer, cameraSpacePoints);
            }
        }
    }


    /// <summary>
    /// Read body data
    /// </summary>
    void ReadBodyFrame(){
        using (var bodyFrame = bodyFrameReader.AcquireLatestFrame()){
            if (bodyFrame != null) {
                bodyFrame.GetAndRefreshBodyData(bodyData);
            }
        }
    }

    /// <summary>
    /// Read body index data
    /// </summary>
    void ReadBodyIndexFrame(){
        using (var frame = bodyIndexFrameReader.AcquireLatestFrame()){
            if (frame != null) {
                frame.CopyFrameDataToArray(bodyIndexBuffer);
            }
        }
    }


    /// <summary>
    /// Initialize compute buffers
    /// </summary>
    private void InitializeComputeBuffers() {
        ReleaseComputeBuffers();

        depthData_computeBuffer = new ComputeBuffer(depthDataBuffer.Length, sizeof(float));

        depthSpacePoints_computeBuffer  = new ComputeBuffer(depthSpacePoints.Length,  sizeof(float) * 2);
        colorSpacePoints_computeBuffer  = new ComputeBuffer(colorSpacePoints.Length,  sizeof(float) * 2);
        cameraSpacePoints_computeBuffer = new ComputeBuffer(cameraSpacePoints.Length, sizeof(float) * 3);

        bodyIndex_computeBuffer = new ComputeBuffer(bodyIndexBuffer.Length, sizeof(float));
    }


    /// <summary>
    /// Dispose compute buffers
    /// </summary>
    private void ReleaseComputeBuffers(){
        depthData_computeBuffer?.Dispose();
        depthSpacePoints_computeBuffer?.Dispose();
        colorSpacePoints_computeBuffer?.Dispose();
        cameraSpacePoints_computeBuffer?.Dispose();
        bodyIndex_computeBuffer?.Dispose();

        depthData_computeBuffer         = null;
        depthSpacePoints_computeBuffer  = null;
        colorSpacePoints_computeBuffer  = null;
        cameraSpacePoints_computeBuffer = null;
        bodyIndex_computeBuffer         = null;
    }


    /// <summary>
    /// Update compute buffers
    /// </summary>
    private void UpdateComputeBuffers() {
        depthSpacePoints_computeBuffer?.SetData(depthSpacePoints);
        colorSpacePoints_computeBuffer?.SetData(colorSpacePoints);
        cameraSpacePoints_computeBuffer?.SetData(cameraSpacePoints);

        // require to convert into float since ComputeBuffer doesn't supports other data types
        // depth data
        if(depthDataBuffer != null && depthData_computeBuffer != null) {
            float[] tmpBuffer = new float[depthDataBuffer.Length]; 

            for(int i = 0; i < depthDataBuffer.Length; i++){
                tmpBuffer[i] = (float)depthDataBuffer[i];
            }

            depthData_computeBuffer.SetData(tmpBuffer);
        }

        // body index data
        if(bodyIndexBuffer != null && bodyIndex_computeBuffer != null) {
            float[] tmpBuffer = new float[bodyIndexBuffer.Length];

            for (int i = 0; i < bodyIndexBuffer.Length; i++) {
                tmpBuffer[i] = (float)bodyIndexBuffer[i];
            }
            
            bodyIndex_computeBuffer.SetData(tmpBuffer);
        }
    }

    /// <summary>
    /// initialize compute shaders
    /// </summary>
    private void InitializeComputeSaders() {
        {
            depthTexture = new RenderTexture(512,424,0, RenderTextureFormat.RFloat);
            depthTexture.enableRandomWrite = true;
            depthTexture.Create();

            int MakeDepthTexture_kernelId = computeShader.FindKernel("MakeDepthTexture");
            computeShader.SetBuffer (MakeDepthTexture_kernelId, "depthData_buffer", depthData_computeBuffer);
            computeShader.SetTexture(MakeDepthTexture_kernelId, "depthData_RWTexture2d", depthTexture, 0);
        }

        {
            bodyIndexTexture = new RenderTexture(512,424,0);
            bodyIndexTexture.enableRandomWrite = true;
            bodyIndexTexture.Create();

            int MakeBodyIndexTexture_kernelId = computeShader.FindKernel("MakeBodyIndexTexture");
            computeShader.SetBuffer (MakeBodyIndexTexture_kernelId, "bodyIndex_buffer", bodyIndex_computeBuffer);
            computeShader.SetTexture(MakeBodyIndexTexture_kernelId, "bodyIndex_RWTexture2d", bodyIndexTexture, 0);
        }


        { 
            bakedPositionsTexture = new RenderTexture(512,424,0, RenderTextureFormat.ARGBFloat);
            bakedPositionsTexture.enableRandomWrite = true;
            bakedPositionsTexture.Create();

            bakedUVsTexture = new RenderTexture(512,424,0, RenderTextureFormat.RGFloat);
            bakedUVsTexture.enableRandomWrite = true;
            bakedUVsTexture.Create();

            int MakeBodyIndexTexture_kernelId = computeShader.FindKernel("PositionBaker");

            computeShader.SetBuffer (MakeBodyIndexTexture_kernelId, "cameraSpacePositions_buffer", cameraSpacePoints_computeBuffer);
            computeShader.SetBuffer (MakeBodyIndexTexture_kernelId, "colorSpaceCoordinates_buffer", colorSpacePoints_computeBuffer);
            
            computeShader.SetTexture(MakeBodyIndexTexture_kernelId, "pointPositionsBaked_RWTexture2d", bakedPositionsTexture, 0);
            computeShader.SetTexture(MakeBodyIndexTexture_kernelId, "pointUvBaked_RWTexture2d", bakedUVsTexture, 0);
        }

        computeShader.SetInt("depthFrameWidth",  depthFrameDesc.Width);
        computeShader.SetInt("depthFrameHeight", depthFrameDesc.Height);

        computeShader.SetInt("depthMinReliable", depthMin);
        computeShader.SetInt("depthMaxReliable", depthMax);
    }

    /// <summary>
    /// Dispose compute shaders
    /// </summary>
    private void ReleaseComputeSaders() {
        depthTexture.Release();
        bodyIndexTexture.Release();
        bakedPositionsTexture.Release();
        bakedUVsTexture.Release();
    }


    /// <summary>
    /// Evaluate compute shaders
    /// </summary>
    private void InvokeComputeShaders(){
		computeShader.Dispatch(computeShader.FindKernel("MakeDepthTexture"),     64, 1, 1);
        computeShader.Dispatch(computeShader.FindKernel("MakeBodyIndexTexture"), 64, 1, 1);
        computeShader.Dispatch(computeShader.FindKernel("PositionBaker"),        64, 1, 1);
    }

    // =====================================================================================! internal use methods ====^
}

}// !namespace ryabomar