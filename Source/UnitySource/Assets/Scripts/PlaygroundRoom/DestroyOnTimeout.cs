using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {


/// <summary>
/// Destroy self then timer runs out
/// </summary>
public class DestroyOnTimeout : MonoBehaviour
{
    /// <summary>
    /// time left
    /// </summary>
    float timeLeft;

    /// <summary>
    /// is timer counting
    /// </summary>
    public bool isCounting {get; private set;} = false;


    /// <summary>
    /// Update timer and destrou self of runs out
    /// </summary>
    void Update()
    {
        if(isCounting){
            timeLeft -= Time.deltaTime;
            if(timeLeft <= 0.0f){
                GameObject.Destroy(this.gameObject);
            }
        }
    }

    /// <summary>
    /// Start timer
    /// </summary>
    /// <param name="timeout">initial time value</param>
    public void StartTimer(float timeout){
        isCounting = true;
        timeLeft = timeout;
    }
}

} //!namespace ryabomar