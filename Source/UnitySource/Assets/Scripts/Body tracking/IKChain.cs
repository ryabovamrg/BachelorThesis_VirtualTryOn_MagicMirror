using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ryabomar {

/// <summary>
/// Chain of elements to apply inverse kinematic
/// </summary>
public class IKChain : MonoBehaviour
{
    /// <summary>
    /// element of chain
    /// </summary>
    [Serializable] public struct Element {
        public Transform node;              // link to node
        public Transform target;            // link to node target

        public float   length;           // distance between object and child node
        public Vector3 directionToChild; // (local, normalized) direction to the position of the child

        internal Quaternion initialRotation;
    }

    // ------------------------------ pablic properties to be set in editor ---v
    /// <summary>
    /// Root of chain. will not move
    /// </summary>
    public GameObject root;

    /// <summary>
    /// Last element of chain. Will strive to reach target
    /// </summary>
    public GameObject effector;

    /// <summary>
    /// target to reach
    /// </summary>
    public GameObject target;

    /// <summary>
    /// list of nodes there effector is first [0] element
    /// </summary>
    public List<Element> elements = new List<Element>();
    // ------------------------------------------------------------------------^
    float totalLength = 0.0f; // total chain length


    /// <summary>
    /// Initialize chain
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="endEffector">end effector; aka last chain element</param>
    /// <param name="root">root chain element</param>
    /// <param name="intermediateTargets">hints for intermediate chain elements</param>
    public void MakeChain(GameObject target, GameObject endEffector, GameObject root, Dictionary<GameObject, GameObject> intermediateTargets) {
        this.effector = endEffector;
        this.root     = root;
        this.target   = target;
 
        Element endEffectorElement = new Element();
            endEffectorElement.node   = endEffector.transform;
            endEffectorElement.target = target.transform;
            endEffectorElement.length = 0.0f;
            endEffectorElement.directionToChild = Vector3.zero;

        elements.Add(endEffectorElement);

        Transform node = endEffector.transform.parent;
        Transform childNode = endEffector.transform;
        
        totalLength = 0.0f;

        while(node != null && node.parent != null && node.gameObject != root) {
            Element element = new Element();

            // construct element
            element.node   = node;
            element.initialRotation = node.localRotation;

            element.length = (childNode.position - node.position).magnitude;
            element.directionToChild = childNode.localPosition.normalized;

            totalLength += element.length;

            // hint for node
            if(intermediateTargets.ContainsKey(node.gameObject)){
                element.target = intermediateTargets[node.gameObject].transform;
            } else {
                element.target = null;
            }

            // store element in list
            elements.Add(element);

            // shift to child
            childNode = node;
            node      = node.parent;
        } //!while

        // add last element length
        totalLength += (elements[elements.Count - 1].node.position - root.transform.position).magnitude;

        // check if effector is an offspring of root in objects hierarchy
        if(node == null || node.gameObject != root.gameObject) { 
            root = null;
            throw new UnityException("[root] - [affector] hierarchy missmatch. Affector should be offspring of the root");
        }
    }

    
    /// <summary>
    /// replacement of Update; to be invoken in other class
    /// </summary>
    public void ManualUpdate(){
        if(root == null || effector == null) return;
        if(elements.Count == 0) return;
        
        Vector3 fromRootToTarget = target.transform.position - root.transform.position;
        if(fromRootToTarget.sqrMagnitude >= totalLength * totalLength) {
            StratchTo(elements[0].target.position);
        } else {
            FABRIK(10, 0.001f);
        }
    }


    /// <summary>
    /// IK algorithm implementation
    /// </summary>
    /// <param name="maxItarations">maximum namber of allowed iterations</param>
    /// <param name="precision">precision threshold</param>
    /// <returns>number of executed iterations</returns>
    uint FABRIK(uint maxItarations, float precision){
        Vector3[] nodePositions = new Vector3[elements.Count];

        // copy node positions
        for(int i = 0; i < elements.Count; i++){
            nodePositions[i] = elements[i].node.position;
        }

        uint iteration = 1;
        for(; iteration <= maxItarations; iteration++){
            {// forward part
                // move effector to its target
                nodePositions[0] = elements[0].target.position;

                for(int i = 1; i < elements.Count; i++){
                    Vector3 d;

                    if(iteration == 1 && elements[i].target != null) {
                        // use hints on first iteration
                        d = nodePositions[i - 1] - elements[i].target.position;
                    } else {
                        d = nodePositions[i - 1] - nodePositions[i];
                    }

                    nodePositions[i] = nodePositions[i - 1] - d.normalized * elements[i].length;
                }
            }
            
            {// backward part
                nodePositions[elements.Count - 1] = elements[elements.Count - 1].node.position;
                
                for(int i = elements.Count - 2; i >= 0; i--){
                    Vector3 d = nodePositions[i] - nodePositions[i + 1];
                    nodePositions[i] = nodePositions[i + 1] + d.normalized * elements[i + 1].length;

                }
            }

            // check distance to target
            if((nodePositions[0] - elements[0].target.position).sqrMagnitude <= (precision * precision)) {
                break; // close enought
            }
        }

        // apply positions
        for(int i = elements.Count - 1; i >= 1; i--){
            // restor initial rotation
            elements[i].node.localRotation = elements[i].initialRotation;

            // figure aout how to rotate 
            Vector3 dirToChild    = elements[i].directionToChild;
            Vector3 newDirToChild = elements[i].node.InverseTransformDirection(nodePositions[i-1] - nodePositions[i]); // transform to local space
            Quaternion deltaRotation = Quaternion.FromToRotation(dirToChild, newDirToChild);

            // apply rotation
            elements[i].node.localRotation *= deltaRotation;
        }
        
        return iteration - 1; // negitiate one extra increment from for loop
    }


    /// <summary>
    /// Stratch whole chain along line
    /// </summary>
    /// <param name="target">target to reach</param>
    void StratchTo(Vector3 target){
        for(int i = elements.Count - 1; i >= 1; i--){
            elements[i].node.localRotation = elements[i].initialRotation;
            
            Vector3 dirToChild = elements[i].directionToChild;
            Vector3 DirTarget  = elements[i].node.InverseTransformDirection(target - elements[i].node.position);
            Quaternion deltaRotation = Quaternion.FromToRotation(dirToChild, DirTarget);
            
            elements[i].node.localRotation *= deltaRotation;
        }
    }
}

}// !namespace ryabomar