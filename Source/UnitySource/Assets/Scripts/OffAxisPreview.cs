using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {

    //[ExecuteInEditMode]

    /// <summary>
    /// manipulate screen and sensor mocks to show their position and orientation
    /// </summary>
    public class OffAxisPreview : MonoBehaviour
    {
        const float INCHES_IN_METER = 39.3701f;

        /// <summary>ref to projection frame</summary>
        public ProjectionFrame projectionFrame;

        /// <summary>ref to sensor mock</summary>
        public GameObject kinectSensorMock;

        /// <summary>ref to screen mock</summary>
        public GameObject screenMock;

        Vector3 InitScreenScale;

        Vector3    initSensorPosition;
        Quaternion initSensorRotation;

        Vector3 sensorOffset = Vector3.zero;
        Vector3 sensorEuler  = Vector3.zero;
        float diagonalInInches = 24.0f;

        /// <summary>reset screen and sensor mocks to their initial states</summary>
        public void Reset() {
            kinectSensorMock.transform.localPosition = initSensorPosition;
            kinectSensorMock.transform.localRotation = initSensorRotation;
            screenMock.transform.localScale          = InitScreenScale;
        }

        void Start() {
            initSensorPosition = kinectSensorMock.transform.localPosition;
            initSensorRotation = kinectSensorMock.transform.localRotation;
            InitScreenScale    = screenMock.transform.localScale;
        }


        void Update() {
            SetScreenDiagonal(diagonalInInches);
            ApplyMotionToProjectionFrame();
        }

        void ApplyMotionToProjectionFrame() {
            // copy screen size
            projectionFrame.transform.localScale = screenMock.transform.localScale;

            // reset position
            projectionFrame.transform.position = Vector3.zero;

            // apply relative rotation
            Quaternion relativeRotation = Quaternion.Inverse(kinectSensorMock.transform.localRotation);
            //Quaternion relativeRotation = kinectSensorMock.transform.localRotation;
            projectionFrame.transform.localRotation = relativeRotation;


            // appy relative position
            Vector3 relativePosition = kinectSensorMock.transform.InverseTransformPoint(screenMock.transform.position);
            relativePosition = screenMock.transform.TransformPoint(relativePosition) - screenMock.transform.position;
            //Vector3 relativePosition = kinectSensorMock.transform.localPosition - screenMock.transform.localPosition;
            projectionFrame.transform.position = -1 * relativePosition;
        }


        /// <summary>move sensor along X axis</summary>
        public void SensorTranslateX(float value) {
            sensorOffset.x = value;
            kinectSensorMock.transform.localPosition = initSensorPosition + sensorOffset;
        }

        /// <summary>move sensor along Y axis</summary>
        public void SensorTranslateY(float value) {
            sensorOffset.y = value;
            kinectSensorMock.transform.localPosition = initSensorPosition + sensorOffset;
        }

        /// <summary>move sensor along Z axis</summary>
        public void SensorTranslateZ(float value) {
            sensorOffset.z = value;
            kinectSensorMock.transform.localPosition = initSensorPosition + sensorOffset;
        }

        /// <summary>rotate sensor X axis</summary>
        public void SensorRotateX(float value) {
            sensorEuler.x = value;
            kinectSensorMock.transform.localRotation = initSensorRotation * Quaternion.Euler(sensorEuler);
        }

        /// <summary>rotate sensor Y axis</summary>
        public void SensorRotateY(float value) {
            sensorEuler.y = value;
            kinectSensorMock.transform.localRotation = initSensorRotation * Quaternion.Euler(sensorEuler);
        }


        /// <summary>change screen diagonal</summary>
        public void SetScreenDiagonal(float valueInInches) {
            diagonalInInches = valueInInches;


            float diagonal = valueInInches / INCHES_IN_METER;

            float ratio = (float)Screen.width / Screen.height; // of physical screen

            float height = Mathf.Sqrt(diagonal * diagonal / (ratio * ratio + 1));
            float width  = height * ratio;

            Vector3 newScale = screenMock.transform.localScale;
            newScale.x = width;
            newScale.y = height;

            screenMock.transform.localScale = newScale;
        }




    }

}