using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ryabomar {

/// <summary>
/// Outfit with meshes for cloth and specific body
/// </summary>
public class Outfit : MonoBehaviour
{
    /// <summary>
    /// Category bitset
    /// </summary>
    [Flags] public enum Category {
        NONE             = 0, 
        HEADGEAR         = 1, 
        FEETWEAR         = 2, 
        GLOVES           = 4,
        UNDERWEAR_BOTTOM = 8, 
        UNDERWEAR_TOP    = 16, 
        TOPWEAR_BOTTOM   = 32, 
        TOPWEAR_TOP      = 64,
        EVERYTHING       = HEADGEAR | FEETWEAR | GLOVES | UNDERWEAR_BOTTOM | UNDERWEAR_TOP | TOPWEAR_BOTTOM | TOPWEAR_TOP
    }

    /// <summary>
    /// category; aka occupied slots
    /// </summary>
    public Category category;

    /// <summary>Reference to body mesh</summary>
    [HideInInspector] public GameObject       bodyMeshObject;

    /// <summary>Reference to eyes mesh</summary>
    [HideInInspector] public GameObject       eyesMeshObject;

    /// <summary>outfit meshes</summary>
    [HideInInspector] public List<GameObject> meshObjects;

    /// <summary>copy of outfit meshes but with masking stencil shader</summary>
    [HideInInspector] public List<GameObject> maskObjects;

    /// <summary>Index in catalogue</summary>
    [HideInInspector] public int catalogueId;

    /// <summary>Is this a single outfit or a combination</summary>
    [HideInInspector] public bool isCombination = false;

    /// <summary>Initialization</summary>
    public void Awake(){

        meshObjects = new List<GameObject>();
        foreach(Transform child in this.transform){
            if(isBodyMesh(child.gameObject)){
                bodyMeshObject = child.gameObject;
                continue;
            }

            if(isEyesMesh(child.gameObject)){
                eyesMeshObject = child.gameObject;
                continue;
            }

            // consider all other objects with SkinnedMeshRenderer as outfit meshes
            if(child.gameObject.GetComponent<SkinnedMeshRenderer>() != null){
                meshObjects.Add(child.gameObject);
            }
        }

        if(bodyMeshObject == null){ Debug.LogWarning("Outfit " + this.gameObject.name + " has no body mesh");}
        if(eyesMeshObject == null){ Debug.LogWarning("Outfit " + this.gameObject.name + " has no eyes mesh");}
        if(meshObjects.Count == 0){ Debug.LogWarning("Outfit " + this.gameObject.name + " has no outfit mesh(es)");}

        MakeMaskObjects();
        ReplaceShaders();

        SetLayer(LayerMask.NameToLayer("outfit"));
        SetUpdateThenOffscreen(true);
    }


    /// <summary>Initialization</summary>
    void Start(){
        if(category == Category.NONE) throw new Exception("no category set");

    }


    /// <summary>Change rendering layer</summary>
    void SetLayer(int layer){
        bodyMeshObject.layer = layer;
        eyesMeshObject.layer = layer;

        foreach(GameObject obj in meshObjects){
            obj.layer = layer;
        }

        foreach(GameObject obj in maskObjects){
            obj.layer = layer;
        }
    }


    /// <summary>
    /// Turn offscreen update for mesh renderer
    /// </summary>
    /// <param name="value"></param>
    void SetUpdateThenOffscreen(bool value){
        bodyMeshObject.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = value;
        eyesMeshObject.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = value;

        foreach(GameObject obj in meshObjects){
            obj.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = value;
        }

        foreach(GameObject obj in maskObjects){
            obj.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = value;
        }

    /// 
    /// 
    /// 
    }

    /// <summary>
    /// Copy bone structure from given object to all child objects with SkinnedMeshRenderer
    /// </summary>
    /// <param name="obj">bone source</param>
    public void CopyBones(GameObject obj){
        CopySkeletonBones(obj, bodyMeshObject);
        CopySkeletonBones(obj, eyesMeshObject);

        foreach(GameObject meshObj in meshObjects){
            CopySkeletonBones(obj, meshObj);
        }
        
        foreach(GameObject maskObj in maskObjects){
            CopySkeletonBones(obj, maskObj);
        }
    }

    /// <summary>
    /// for each outfit mesh create its copy
    /// </summary>
    void MakeMaskObjects(){
        maskObjects = new List<GameObject>();

        GameObject masksRoot = this.transform.Find("masks")?.gameObject;
        if(masksRoot == null){
            masksRoot = new GameObject("masks");
            masksRoot.transform.parent = this.transform;

            foreach(GameObject meshObj in meshObjects){
                GameObject maskObj = GameObject.Instantiate(meshObj, masksRoot.transform);
                maskObj.name = "mask_" + meshObj.name;
                maskObj.SetActive(true);
            }
        } else {
            // reuse old mask objects
            foreach(Transform mask in masksRoot.transform){
                maskObjects.Add(mask.gameObject);
            }
        }
    }

    /// <summary>
    /// replace shaders in materials
    /// body, eyes    --> stencil body shader
    /// outfit meshes --> stencil outfit shader
    /// outfit masks  --> stencil mask shader 
    /// </summary>
    void ReplaceShaders(){
        ReplaceShader(bodyMeshObject.GetComponent<SkinnedMeshRenderer>(), Shader.Find("stencil/_2_BodyShader"));
        ReplaceShader(eyesMeshObject.GetComponent<SkinnedMeshRenderer>(), Shader.Find("stencil/_2_BodyShader"));

        foreach(GameObject meshObj in meshObjects){
            ReplaceShader(meshObj.GetComponent<SkinnedMeshRenderer>(), Shader.Find("stencil/_3_OutfitLitShader"));
        }

        foreach(GameObject maskObj in maskObjects){
            ReplaceShader(maskObj.GetComponent<SkinnedMeshRenderer>(), Shader.Find("stencil/_1_MaskingShader"));
        }
    }


    /// <summary> 
    /// replace shader in given renderer
    /// !!!!
    ///     in order to avoid conflicts with other instances of the same object
    ///     instead of just replacing shader in material creates a new material 
    ///     with given shader and all values of previous material
    /// !!!!
    /// </summary>
    /// <param name="renderer">renderer in which shader will be changed</param>
    /// <param name="shader">source shader</param>
    void ReplaceShader(Renderer renderer, Shader shader){
        Material newMaterial = new Material(shader);
        newMaterial.CopyPropertiesFromMaterial(renderer.material);
        renderer.material = newMaterial;
        renderer.material.shader = shader;
        renderer.material.SetInt("_StencilMask", (int)category);
    }

    /// <summary>
    /// check if given object is a body mesh
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    bool isBodyMesh(GameObject obj) {
        return obj != null && 
            ( obj.name == (this.gameObject.name + "Mesh") ||
              obj.name == (this.gameObject.name.ToLower() + "Mesh")) &&
            obj.GetComponent<SkinnedMeshRenderer>() != null;
    }

    /// <summary>
    /// check if given object is a eyes mesh
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    bool isEyesMesh(GameObject obj) {
        return obj != null && (obj.name == "high-polyMesh" || obj.name == "low-polyMesh")
            && obj.GetComponent<SkinnedMeshRenderer>() != null;
    }

    /// <summary>
    /// copies bones from one SkinnedMeshRenderer to another
    /// bone structures have to be the same
    /// </summary>
    /// <param name="sourceObject"></param>
    /// <param name="destObject"></param>
    static void CopySkeletonBones(GameObject sourceObject, GameObject destObject){
        //https://forum.unity.com/threads/transfer-the-rig-of-a-skinned-mesh-renderer-to-a-new-smr-with-code.499008/
        SkinnedMeshRenderer source = sourceObject.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer dest   = destObject.GetComponent<SkinnedMeshRenderer>();

        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        
        foreach (Transform bone in source.bones)
        {
            boneMap[bone.name] = bone;
        }
 
        Transform[] boneArray = dest.bones;
        for (int idx = 0; idx < boneArray.Length; ++idx)
        {
            string boneName = boneArray[idx].name;
            if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
            {
                Debug.LogError("failed to get bone: " + boneName);
                Debug.Break();
            }
        }
        dest.bones = boneArray;
    }
}

}// !namespace ryabomar