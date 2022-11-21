using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// makes object togglable
/// enable or disable by calling single method
/// </summary>
public class Togglable : MonoBehaviour
{
    /// <summary>
    /// enable or disable its GameObject
    /// </summary>
    public void toggle() {


        if(gameObject.activeInHierarchy) gameObject.SetActive(false);
        else gameObject.SetActive(true);
    }
}
