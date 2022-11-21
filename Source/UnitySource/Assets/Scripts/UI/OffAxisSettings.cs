using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ryabomar {

    public class OffAxisSettings : MonoBehaviour
    {
        // ui elements ------------------------------
        public Slider xOffsetSlider;
        public Slider yOffsetSlider;
        public Slider zOffsetSlider;
        public Slider xRotationSlider;
        public Slider yRotationSlider;
        public Slider diagonalSlider;

        public InputField diagonalInputField;
        // -------------------------------------------

        public ProjectionFrame projectionFrame;


        // ui element init values
        Vector3 initOffsetSlidersValues;
        Vector3 initRotationSlidersValues;
        float initDiagonalValue;

        void Start() {
            // backup init values of ui elements
            initOffsetSlidersValues.x = xOffsetSlider.value;
            initOffsetSlidersValues.y = yOffsetSlider.value;
            initOffsetSlidersValues.z = zOffsetSlider.value;

            initRotationSlidersValues.x = xRotationSlider.value;
            initRotationSlidersValues.y = yRotationSlider.value;

            initDiagonalValue = diagonalSlider.value;
            SetDiagonalInputField(initDiagonalValue);

            InvokeCallbacks();
        }


        /// <summary>
        /// reset ui elements
        /// </summary>
        public void Reset() {
            xOffsetSlider.value = initOffsetSlidersValues.x;
            yOffsetSlider.value = initOffsetSlidersValues.y;
            zOffsetSlider.value = initOffsetSlidersValues.z;

            xRotationSlider.value = initRotationSlidersValues.x;
            yRotationSlider.value = initRotationSlidersValues.y;

            diagonalSlider.value = initDiagonalValue;

            SetDiagonalInputField(initDiagonalValue);

            InvokeCallbacks();
        }


        /// <summary>
        /// invoke function assotiated with ui elements
        /// </summary>
        void InvokeCallbacks() {
            xOffsetSlider.onValueChanged.Invoke(xOffsetSlider.value);
            yOffsetSlider.onValueChanged.Invoke(yOffsetSlider.value);
            zOffsetSlider.onValueChanged.Invoke(zOffsetSlider.value);

            xRotationSlider.onValueChanged.Invoke(xRotationSlider.value);
            yRotationSlider.onValueChanged.Invoke(yRotationSlider.value);

            diagonalSlider.onValueChanged.Invoke(diagonalSlider.value);
        }



        /// <summary>
        /// set slider position
        /// </summary>
        /// <param name="stringValue">string(!) value</param>
        public void SetDiagonalSlider(string stringValue) {
            float parsedValue;

            if(float.TryParse(stringValue, out parsedValue)) {
                diagonalSlider.value = parsedValue; // set value AND invoke callback
            }
        }


        /// <summary>
        /// set input field text
        /// </summary>
        /// <param name="value">float(!) value</param>
        public void SetDiagonalInputField(float value) {
            diagonalInputField.SetTextWithoutNotify(value.ToString("0.00")); // set text WITHOUT calling callback
        }


    }
}