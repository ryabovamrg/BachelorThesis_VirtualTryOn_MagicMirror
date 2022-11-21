using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace ryabomar {

/// <summary>
/// Popup window with timeout. Performs avatar adjustment then timer runs out
/// </summary>
public class UiDialogAdjustingWithDelay : MonoBehaviour
{
    /// <summary>
    /// reference to avatar switcher
    /// </summary>
    public AvatarBodyConstoller avatarsManager;

    /// <summary>
    /// UI text element to display left time
    /// </summary>
    public Text timeoutText;

    /// <summary>
    /// Initial amount if time
    /// </summary>
    [Range(0.0f, 30.0f)] public float timeout;

    /// <summary>
    /// time left
    /// </summary>
    private float timeLeft;


    /// <summary>
    /// Initialization
    /// </summary>
    void OnEnable() {
        timeLeft = timeout;
    }

    /// <summary>
    /// Countdown
    /// </summary>
    void Update() {
        timeLeft -= Time.deltaTime;
        timeoutText.text = timeLeft.ToString("0.0");

        if(timeLeft <= 0.0) {
            avatarsManager.AdjustActiveAvatarBody();
            gameObject.SetActive(false);
        }
    }
}

} //!namespace ryabomar