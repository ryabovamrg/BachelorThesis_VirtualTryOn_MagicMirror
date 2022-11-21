using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ryabomar {

/// <summary>
/// Body type
/// </summary>
public enum AvatarBodyType {
    FEMALE_FIT, 
    FEMALE_FAT, 
    MALE_FIT, 
    MALE_FAT, 
    CHILD
}

/// <summary>
/// Switches avatars for different body types
/// </summary>
public class AvatarBodyConstoller : MonoBehaviour
{
    /// <summary>
    /// default body type
    /// </summary>
    public AvatarBodyType initialBodyType;

    /// <summary>female medium avatar prefab</summary>
    public GameObject femaleFitBody;

    /// <summary>female large avatar prefab</summary>
    public GameObject femaleFatBody;

    /// <summary>male medium avatar prefab</summary>
    public GameObject maleFitBody;

    /// <summary>male large avatar prefab</summary>
    public GameObject maleFatBody;

    /// <summary>child avatar prefab</summary>
    public GameObject childBody;

    /// <summary>
    /// manager to equip and unequip outfits
    /// </summary>
    public EquipmentManager equipmentManager;

    // more convinient storage for prefabs
    Dictionary<AvatarBodyType, GameObject> avatarPrefabs; 

    /// <summary>
    /// reference to current avatar
    /// </summary>
    [HideInInspector] public GameObject activeAvatarObject;

    /// <summary>
    /// current avatar body type
    /// </summary>
    [HideInInspector] public AvatarBodyType activeAvatarBodyType;

    /// <summary>
    /// visibility of id debug lines
    /// </summary>
    public bool isIkLinesVisible = true;

    /// <summary>switch to female medium body</summary>
    public void ActivateFemaleFit() { ReplaceAvatar(AvatarBodyType.FEMALE_FIT); }

    /// <summary>switch to female large body</summary>
    public void ActivateFemaleFat() { ReplaceAvatar(AvatarBodyType.FEMALE_FAT); }

    /// <summary>switch to male medium body</summary>
    public void ActivateMaleFit()   { ReplaceAvatar(AvatarBodyType.MALE_FIT);   }

    /// <summary>switch to male large body</summary>
    public void ActivateMaleFat()   { ReplaceAvatar(AvatarBodyType.MALE_FAT);   }
    
    /// <summary>switch to child body</summary>
    public void ActivateChild()     { ReplaceAvatar(AvatarBodyType.CHILD);      }


    /// <summary>
    /// Change visibility of ik chain debug lines
    /// </summary>
    /// <param name="value"></param>
    public void SetIKChainLinesVisible(bool value){
        isIkLinesVisible = value;
        activeAvatarObject.GetComponent<AvatarBody>().SetIkLinesVisible(isIkLinesVisible);
    }


    /// <summary>
    /// Adjust current body to better fit user's body
    /// </summary>
    public void AdjustActiveAvatarBody() {
        activeAvatarObject.GetComponent<AvatarBody>().AdjustAvatar();
    }


    /// <summary>
    /// Reset current body
    /// </summary>
    public void ResetActiveBody(){
        ReplaceAvatar(activeAvatarBodyType);
    }


    /// <summary>
    /// initialization
    /// </summary>
    void Awake(){
        { // check prefabs
            if(femaleFitBody == null || femaleFitBody.GetComponent<AvatarBody>() == null)
                throw new System.Exception("need female fit body");
            
            if(femaleFatBody == null || femaleFatBody.GetComponent<AvatarBody>() == null)
                throw new System.Exception("need female fat body");

            if(maleFitBody == null || maleFitBody.GetComponent<AvatarBody>() == null)
                throw new System.Exception("need male fit body");

            if(maleFatBody == null || maleFatBody.GetComponent<AvatarBody>() == null)
                throw new System.Exception("need male fat body");

            if(childBody == null || childBody.GetComponent<AvatarBody>() == null)
                throw new System.Exception("need child body");
        }

        avatarPrefabs = new Dictionary<AvatarBodyType, GameObject>{
            {AvatarBodyType.FEMALE_FIT, femaleFitBody},
            {AvatarBodyType.FEMALE_FAT, femaleFatBody},
            {AvatarBodyType.MALE_FIT,   maleFitBody},
            {AvatarBodyType.MALE_FAT,   maleFatBody},
            {AvatarBodyType.CHILD,      childBody},
        };

        ReplaceAvatar(initialBodyType);
    }


    /// <summary>
    /// initialization
    /// </summary>
    void Start() {
        SetIKChainLinesVisible(isIkLinesVisible);
    }


    /// <summary>
    /// Create new instance of avatar
    /// </summary>
    /// <param name="type">desirable body type</param>
    /// <returns>instance</returns>
    GameObject InstantiateAvatar(AvatarBodyType type) {
        GameObject obj = GameObject.Instantiate(avatarPrefabs[type]);
        obj.name = avatarPrefabs[type].name;

        int layer = LayerMask.NameToLayer("body");
        obj.layer = layer;
        obj.transform.Find(obj.name + "Mesh").gameObject.layer = layer;
        obj.transform.Find("high-polyMesh").gameObject.layer = layer;

        obj.transform.parent = this.transform;
        obj.SetActive(true);

        return obj;
    }


    /// <summary>
    /// Destroy current avatar
    /// </summary>
    void DestroyActiveAvatar(){
        if(activeAvatarObject != null) {
            GameObject.Destroy(activeAvatarObject);
            activeAvatarObject = null;
        }
    }


    /// <summary>
    /// replace current avatar with new avatar of given body type
    /// </summary>
    /// <param name="type"></param>
    void ReplaceAvatar(AvatarBodyType type){
        DestroyActiveAvatar();
        activeAvatarObject = InstantiateAvatar(type);
        activeAvatarBodyType = type;

        SetIKChainLinesVisible(isIkLinesVisible);
        equipmentManager.SetBodyVisible(equipmentManager.isBodyVisible);
    }
}

}// !namespace ryabomar