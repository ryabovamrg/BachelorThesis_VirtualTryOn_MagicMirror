using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {

/// <summary>
/// Simble object generator
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    /// <summary>List of prefabs. Then spawn new object one of prefabs will be selected</summary>
    public List<GameObject> prefabs  = new List<GameObject>();

    /// <summary>List of materials for prefabs. Then spawn new object one of material will be selected</summary>
    public List<Material> materials  = new List<Material>();

    /// <summary>initial velocity of spawned objects</summary>
    public Vector3 newObjectVelocity = new Vector3();
    
    /// <summary>Time to live for spawned objects; Forever if value is 0</summary>
    [Range(0.0f, 30.0f)] public float objectDestroyTimeout = 0.0f;

    /// <summary>interval between spawns</summary>
    [Range(0.1f, 5.0f)]  public float spawningTimeout      = 0.1f;

    // time since activated
    float timeTriggered = 0.0f;

    /// <summary>
    /// Spawn copy of rundom object from prefabs list with random material from materials list
    /// </summary>
    /// <param name="pos">Initial position of new object</param>
    public void SpawnRandomObject(Vector3 pos){
        if(prefabs.Count == 0) return;
        
        int idx = Mathf.FloorToInt(Random.Range(0.0f, prefabs.Count));

        GameObject obj = GameObject.Instantiate(prefabs[idx]);
        obj.transform.position   = pos;
        obj.transform.rotation   = this.transform.rotation;
        obj.transform.parent     = this.transform.parent;

        obj.tag = "PlaygroundPrimitive";


        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if(meshRenderer != null && materials.Count != 0){
            
            int matIdx = Mathf.FloorToInt(Random.Range(0.0f, materials.Count));
            meshRenderer.material = materials[matIdx];
        }


        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if(rb != null){
            rb.velocity = newObjectVelocity;
        }


        if(objectDestroyTimeout > 0.0f){
            var timer = obj.AddComponent<DestroyOnTimeout>();
            timer.StartTimer(objectDestroyTimeout);
        }

        obj.SetActive(true);
    }


    /// <summary>
    /// trigger if hand joint enter collider
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other){
        if(other.tag == "KinectHandJoint"){
            timeTriggered = 0.0f;
        }
    }

    /// <summary>
    /// trigger if hand joint leaves collider
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerLeave(Collider other){
        if(other.tag == "KinectHandJoint"){
            timeTriggered = 0.0f;
        }
    }

    /// <summary>
    /// if hand joint stays in collider for enought time spawns new object
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other){
        if(other.tag == "KinectHandJoint"){
            timeTriggered += Time.deltaTime;
            if(timeTriggered >= spawningTimeout){
                timeTriggered = 0.0f;
                SpawnRandomObject(other.transform.position);
            }
        }
    }
}

} //!namespace ryabomar