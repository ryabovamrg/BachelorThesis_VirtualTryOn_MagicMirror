using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ryabomar {


/// <summary>
/// Popup with timer. Will freeze kinect body then time runs out
/// </summary>
public class DialogueFreezeBody : MonoBehaviour
{
    /// <summary>
    /// reference to kinect body
    /// </summary>
    public KinectBody kinectBody;

    /// <summary>
    /// reference to text ui element
    /// </summary>
    public Text timeoutText;


    /// <summary>
    /// time left
    /// </summary>
    [Range(0.0f, 30.0f)] public float timeout;
    private float timeLeft;

    /// <summary>
    /// initialization
    /// </summary>
    void OnEnable() {
        timeLeft = timeout;
    }


    /// <summary>
    /// countdown
    /// </summary>
    void Update() {
        timeLeft -= Time.deltaTime;
        timeoutText.text = timeLeft.ToString("0.0");

        if(timeLeft <= 0.0) {
            kinectBody.SetFreezed(true);
            this.gameObject.SetActive(false);
        }
    }
}

} //!namespace ryabomar