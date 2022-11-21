using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {

/// <summary>
/// Draw lines in order to visualize relations between IKChain elements and targets
/// </summary>
[RequireComponent(typeof(IKChain))] 
public class IKChainDrawLines : MonoBehaviour
{
    /// <summary>
    /// material for dashed line
    /// </summary>
    public Material lineMaterial;

    List<IKChain.Element> chainElements;
    List<LineRenderer> lineRenderers = new List<LineRenderer>();

    /// <summary>
    /// initialisation
    /// </summary>
    void Start() {
        IKChain iKChain = GetComponent<IKChain>();
        chainElements = iKChain.elements;

        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        obj.name = "lines";


        if(lineMaterial == null){
            // load material from "Resource" folder
            lineMaterial = Resources.Load("DashedLineMaterial") as Material;
        }


        // line for end effector AND lines for other nodes
        for(int i = 0; i < chainElements.Count; i++){
            GameObject o = new GameObject();
            o.name = "line";
            o.transform.parent = obj.transform;
            o.layer = LayerMask.NameToLayer("debug"); 

            var lineRenderer = o.AddComponent<LineRenderer>();
            lineRenderer.material       = lineMaterial;
            lineRenderer.positionCount  = 2;
            lineRenderer.startWidth     = 0.01f;
            lineRenderer.endWidth       = 0.01f;
            lineRenderer.textureMode    = LineTextureMode.Tile;
            lineRenderers.Add(lineRenderer);
        }

        SetIkLinesVisible(false);
    }

    /// <summary>
    /// Update
    /// </summary>
    void Update(){
        // lines for other nodes
        for(int i = 0; i < chainElements.Count; i++){
            if(chainElements[i].node != null && chainElements[i].target != null) {
                lineRenderers[i].enabled = true;
                lineRenderers[i].SetPosition(0, chainElements[i].node.transform.position);
                lineRenderers[i].SetPosition(1, chainElements[i].target.transform.position);
            } else {
                lineRenderers[i].enabled = false;
            }

        }
    }


    /// <summary>
    /// Change visibility of dashed lines
    /// </summary>
    public void SetIkLinesVisible(bool value) {
        foreach(LineRenderer lineRenderer in lineRenderers){
            lineRenderer.gameObject.SetActive(value);
        }
    }
}

}// !namespace ryabomar