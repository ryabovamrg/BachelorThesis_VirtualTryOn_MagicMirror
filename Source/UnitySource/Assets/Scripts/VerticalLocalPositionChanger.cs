using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// changes its local position along Y axis
/// </summary>
public class VerticalLocalPositionChanger : MonoBehaviour
{

    Vector3 initLocalPosition;

    void Awake()
    {
        initLocalPosition = transform.localPosition;
    }

    void Update()
    {
        
    }


    public void VerticalOffset(float value) {
        Vector3 newLocalPos = initLocalPosition;
        newLocalPos.y += value;
        transform.localPosition = newLocalPos;
    }
}
