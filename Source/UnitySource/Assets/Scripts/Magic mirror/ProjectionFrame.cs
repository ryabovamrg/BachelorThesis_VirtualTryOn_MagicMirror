using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {

    /// <summary>
    /// Projection frame for off-axis projection
    /// </summary>
    [ExecuteInEditMode] public class ProjectionFrame : MonoBehaviour
    {
        /// <summary>to draw lines between corners or not</summary>
        public bool isVisible = true;

        /// <summary>border color</summary>
        public Color color = Color.white;

        // global positions of the frame corners
        /// <summary>corner A global position</summary>
        public Vector3 pA { get; private set; }

        /// <summary>corner B global position</summary>
        public Vector3 pB { get; private set; }

        /// <summary>corner C global position</summary>
        public Vector3 pC { get; private set; }

        /// <summary>corner D global position</summary>
        public Vector3 pD { get; private set; }

        // frame basis
        /// <summary>Basis vector U</summary>
        public Vector3 vU { get; private set; }

        /// <summary>Basis vector R</summary>
        public Vector3 vR { get; private set; }

        /// <summary>Basis vector N</summary>
        public Vector3 vN { get; private set; }


        /// <summary>Calculated matrix</summary>
        public Matrix4x4 matrix { get; private set; }


        /*
        C._______.D
         |       |
         |       |
        A._______.B
        */

        Vector3 cornerA = new Vector3(-1, -1, 0);  // bottom left
        Vector3 cornerB = new Vector3( 1, -1, 0);  // bottom right
        Vector3 cornerC = new Vector3(-1,  1, 0);  // top left
        Vector3 cornerD = new Vector3( 1,  1, 0);  // top right


        Vector3 initGlobalPosition;
        Vector3 initScale;
        Quaternion initRotation;

        Vector3 offset = Vector3.zero;
        Vector3 euler_rotation = Vector3.zero;

        // for inch <=> meter(unity unit) conversion
        const float INCHES_IN_METER = 39.3701f;

        void Awake() {
            initGlobalPosition = transform.position;
            initScale = transform.localScale;
            initRotation = transform.localRotation;
        }

        /// <summary>
        /// Update values
        /// </summary>
        void Update() {
            {// keep width/height ratio same as screen
                Vector3 newScale = transform.localScale;

                float ratio = (float)Screen.width / Screen.height;
                newScale.x = ratio * transform.localScale.y;

                transform.localScale = newScale;
            }

            {// corner global positions
                pA = transform.TransformPoint(cornerA);
                pB = transform.TransformPoint(cornerB);
                pC = transform.TransformPoint(cornerC);
                pD = transform.TransformPoint(cornerD);
            }

            {// frame basis
                vR = (pB - pA).normalized;
                vU = (pC - pA).normalized;
                vN = -Vector3.Cross(vR, vU).normalized;
            }
        
            {// matrix
                Matrix4x4 m = Matrix4x4.zero;

                m[0, 0] = vR.x;
                m[0, 1] = vR.y;
                m[0, 2] = vR.z;
                m[1, 0] = vU.x;
                m[1, 1] = vU.y;
                m[1, 2] = vU.z;
                m[2, 0] = vN.x;
                m[2, 1] = vN.y;
                m[2, 2] = vN.z;
                m[3, 3] = 1.0f;

                matrix = m;
            }
        }

        /// <summary>
        /// Draw border
        /// </summary>
        void OnDrawGizmos(){
            if(isVisible) {
                Gizmos.color = color;
                Gizmos.DrawLine(pA, pB);
                Gizmos.DrawLine(pA, pC);
                Gizmos.DrawLine(pD, pB);
                Gizmos.DrawLine(pC, pD);
            }
        }


        /// <summary>
        /// Horizontal translation
        /// </summary>
        /// <param name="value">offset</param>
        public void TranslateX(float value) {
            offset.x = value;
            
            transform.localPosition = initGlobalPosition + offset;
        }

        /// <summary>
        /// Vertical translation
        /// </summary>
        /// <param name="value">offset</param>
        public void TranslateY(float value) {
            offset.y = value;
            transform.localPosition = initGlobalPosition + offset;
        }


        /// <summary>
        /// Forward translation
        /// </summary>
        /// <param name="value">offset</param>
        public void TranslateZ(float value) {
            offset.z = value;
            transform.localPosition = initGlobalPosition + offset;
        }


        /// <summary>
        /// Rotation around Up direction
        /// </summary>
        /// <param name="value">rotation</param>
        public void RotateY(float value) {
            euler_rotation.y = value;
            transform.localRotation = initRotation * Quaternion.Euler(euler_rotation);
            //Quaternion.AngleAxis(value, Vector3.up);
        }


        /// <summary>
        /// Rotation around right direction
        /// </summary>
        /// <param name="value">rotation</param>
        public void RotateX(float value) {
            euler_rotation.x = value;
            transform.localRotation = initRotation * Quaternion.Euler(euler_rotation);
            //transform.localRotation = initRotation * Quaternion.AngleAxis(value, Vector3.right);
        }


        /// <summary>
        /// change scale
        /// </summary>
        /// <param name="value">scale</param>
        public void Scale(float value) {
            Vector3 newScale = initScale;
            newScale *= value;
            transform.localScale = newScale;
        }


        /// <summary>
        /// set screen diagonal
        /// </summary>
        /// <param name="value"></param>
        public void SetDiagonal(float value) {
            float width  = transform.localScale.x;
            float height = transform.localScale.y;

            float diagonal = Mathf.Sqrt(width*width + height*height);
            
            float mult = value / diagonal;

            transform.localScale = transform.localScale * mult;
        }


        /// <summary>
        /// set screen diagonal in inches
        /// </summary>
        /// <param name="value"></param>
        public void SetDiagonal_inches(float value) {
            SetDiagonal(value / INCHES_IN_METER);
        }


        /// <summary>
        /// reset transform to its initial state
        /// </summary>
        public void ResetTransform() {
            transform.localPosition = initGlobalPosition;
            transform.localScale    = initScale;
            transform.localRotation = initRotation;
        }
    }

}// !namespace ryabomar