using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {

using Category = Outfit.Category;

/// <summary>
/// Manager to equip and unequip outfits
/// </summary>
public class EquipmentManager : MonoBehaviour
{   
    /// <summary>Reference to cataloge</summary>
    public OutfitCatalogue outfitCatalogue;

    /// <summary>is body visible</summary>
    public bool isBodyVisible = false;

    /// <summary>reference to avatar switcher</summary>
    public AvatarBodyConstoller avatarBodyConstoller;

    /// <summary>outfits equipped in slots</summary>
    public Dictionary<Category, Outfit> equipped = new Dictionary<Category, Outfit>{
        { Category.HEADGEAR,         null},
        { Category.FEETWEAR,         null},
        { Category.GLOVES,           null},
        { Category.UNDERWEAR_TOP,    null},
        { Category.UNDERWEAR_BOTTOM, null},
        { Category.TOPWEAR_TOP,      null},
        { Category.TOPWEAR_BOTTOM,   null}
    };

    /// <summary>list of equipped combinations</summary>
    List<OutfitCombination> equippedCombinations = new List<OutfitCombination>();

    /// <summary>root object to group outfits together</summary>
    GameObject equippedOutfitsRoot;

    /// <summary>root object to group outfit combinations together</summary>
    GameObject equippedCombinationsRoot;

    /// <summary>
    /// Initializations
    /// </summary>
    void Start(){
        if(avatarBodyConstoller == null) throw new System.Exception("need AvatarManager");

        equippedOutfitsRoot = transform.Find("equipped outfits")?.gameObject;
        if(equippedOutfitsRoot == null){
            equippedOutfitsRoot = new GameObject("equipped outfits");
            equippedOutfitsRoot.transform.parent = this.transform;
        }

        equippedCombinationsRoot = transform.Find("equipped combinations")?.gameObject;
        if(equippedCombinationsRoot == null){
            equippedCombinationsRoot = new GameObject("equipped combinations");
            equippedCombinationsRoot.transform.parent = this.transform;
        }
    }

    /// <summary>
    /// Equip outfit
    /// </summary>
    /// <param name="bodyType">body type</param>
    /// <param name="id">index in OutfitCatalogue</param>
    public void Equip(AvatarBodyType bodyType, int id) {
        //outfitCatalogue.collections[bodyType][id]
        GameObject catalogueObj = outfitCatalogue.GetOutfit(bodyType, id)?.gameObject;
        if(catalogueObj == null) { throw new System.Exception("invalid outfit id"); }

        GameObject outfitObj = GameObject.Instantiate(catalogueObj);
        outfitObj.transform.parent = equippedOutfitsRoot.transform;

        outfitObj.transform.localPosition = Vector3.zero;
        outfitObj.name = catalogueObj.name;

        Outfit outfit = outfitObj.GetComponent<Outfit>();

        // unequip conflicting outfits
        if(hasCategory(outfit, Category.HEADGEAR))         { Unequip(Category.HEADGEAR);         }
        if(hasCategory(outfit, Category.FEETWEAR))         { Unequip(Category.FEETWEAR);         }
        if(hasCategory(outfit, Category.GLOVES))           { Unequip(Category.GLOVES);           }
        if(hasCategory(outfit, Category.UNDERWEAR_TOP))    { Unequip(Category.UNDERWEAR_TOP);    }
        if(hasCategory(outfit, Category.UNDERWEAR_BOTTOM)) { Unequip(Category.UNDERWEAR_BOTTOM); }
        if(hasCategory(outfit, Category.TOPWEAR_TOP))      { Unequip(Category.TOPWEAR_TOP);      }
        if(hasCategory(outfit, Category.TOPWEAR_BOTTOM))   { Unequip(Category.TOPWEAR_BOTTOM);   }

        // occupy equipped categoties
        if(hasCategory(outfit, Category.HEADGEAR))         { equipped[Category.HEADGEAR]         = outfit; }
        if(hasCategory(outfit, Category.FEETWEAR))         { equipped[Category.FEETWEAR]         = outfit; }
        if(hasCategory(outfit, Category.GLOVES))           { equipped[Category.GLOVES]           = outfit; }
        if(hasCategory(outfit, Category.UNDERWEAR_TOP))    { equipped[Category.UNDERWEAR_TOP]    = outfit; }
        if(hasCategory(outfit, Category.UNDERWEAR_BOTTOM)) { equipped[Category.UNDERWEAR_BOTTOM] = outfit; }
        if(hasCategory(outfit, Category.TOPWEAR_TOP))      { equipped[Category.TOPWEAR_TOP]      = outfit; }
        if(hasCategory(outfit, Category.TOPWEAR_BOTTOM))   { equipped[Category.TOPWEAR_BOTTOM]   = outfit; }

        outfit.gameObject.SetActive(true);
        outfit.CopyBones(GetAvatarBodyMeshObject());

        TryReplaceOutfitsWithCombinations();
    }

    /// <summary>
    /// Unequip all outfits
    /// </summary>
    public void UnequipEverything() { 
        UnequipHeadgear       (); 
        UnequipGloves         (); 
        UnequipUnderwearTop   (); 
        UnequipUnderwearBottom(); 
        UnequipFeetwear       (); 
        UnequipTopwearTop     (); 
        UnequipTopwearBottom  ();        
    }

    /// <summary>Unequip hat</summary>
    public void UnequipHeadgear       () { Unequip(Outfit.Category.HEADGEAR);         TryReplaceOutfitsWithCombinations(); }

    /// <summary>Unequip gloves</summary>
    public void UnequipGloves         () { Unequip(Outfit.Category.GLOVES);           TryReplaceOutfitsWithCombinations(); }

    /// <summary>Unequip top of underwear</summary>
    public void UnequipUnderwearTop   () { Unequip(Outfit.Category.UNDERWEAR_TOP);    TryReplaceOutfitsWithCombinations(); }

    /// <summary>Unequip bottom of underwear</summary>
    public void UnequipUnderwearBottom() { Unequip(Outfit.Category.UNDERWEAR_BOTTOM); TryReplaceOutfitsWithCombinations(); }

    /// <summary>Unequip boots</summary>
    public void UnequipFeetwear       () { Unequip(Outfit.Category.FEETWEAR);         TryReplaceOutfitsWithCombinations(); }

    /// <summary>Unequip top of topwear</summary>
    public void UnequipTopwearTop     () { Unequip(Outfit.Category.TOPWEAR_TOP);      TryReplaceOutfitsWithCombinations(); }

    /// <summary>Unequip bottom of topwear</summary>
    public void UnequipTopwearBottom  () { Unequip(Outfit.Category.TOPWEAR_BOTTOM);   TryReplaceOutfitsWithCombinations(); }


    /// <summary>
    /// Unequip outfit of given category
    /// </summary>
    /// <param name="category">category</param>
    public void Unequip(Outfit.Category category) {
        if(equipped[category] == null) return;

        Outfit outfitToUnequip = equipped[category];

        List<Category> slotsToUnequip = new List<Category>();

        foreach(var entry in equipped){
            if(entry.Value != null && entry.Value.gameObject == outfitToUnequip.gameObject){
                slotsToUnequip.Add(entry.Key);
            }
        }

        foreach(var slot in slotsToUnequip){
            equipped[slot].gameObject.SetActive(false);
            GameObject.Destroy(equipped[slot].gameObject);
            equipped[slot] = null;
        }
    }

    /// <summary>
    /// Turn body visibility
    /// </summary>
    /// <param name="value"></param>
    public void SetBodyVisible(bool value) {
        isBodyVisible = value;

        // set visibility/invisibility of body and eyes meshes
        GetAvatarBodyMeshObject().SetActive(isBodyVisible);
        GetAvatarEyesMeshObject().SetActive(isBodyVisible);
    }

    /// <summary>
    /// Check if given outfit has category using bitset operations
    /// </summary>
    /// <param name="outfit">outfit to check</param>
    /// <param name="category">category</param>
    /// <returns></returns>
    static bool hasCategory(Outfit outfit, Outfit.Category category){
        return (outfit.category & category) == category; //!= Outfit.Category.NONE;
    }

    /// <summary>
    /// Find object with body mesh of current avatar
    /// </summary>
    /// <returns>reference to found object</returns>
    GameObject GetAvatarBodyMeshObject(){
        foreach(Transform child in avatarBodyConstoller.activeAvatarObject.transform){
            if(isBodyMesh(child.gameObject, avatarBodyConstoller.activeAvatarObject.name)){
                return child.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Find object with eyes mesh of current avatar
    /// </summary>
    /// <returns>reference to found object</returns>
    GameObject GetAvatarEyesMeshObject(){
        foreach(Transform child in avatarBodyConstoller.activeAvatarObject.transform){
            if(isEyesMesh(child.gameObject)){
                return child.gameObject;
            }
        }
        return null;
    }


    /// <summary>
    /// Check if given object is a body mesh
    /// </summary>
    /// <param name="obj">object</param>
    /// <param name="outfitName">Outfit name. Body mesh should have name as "outfitName" + "Mesh"</param>
    /// <returns>result</returns>
    static bool isBodyMesh(GameObject obj, string outfitName) {
        return obj != null && obj.name == (outfitName + "Mesh") 
            && obj.GetComponent<SkinnedMeshRenderer>() != null;
    }


    /// <summary>
    /// Check if given object is a body eyes mesh
    /// </summary>
    /// <param name="obj">object</param>
    /// <returns>result</returns>
    static bool isEyesMesh(GameObject obj) {
        return obj != null && (obj.name == "high-polyMesh" || obj.name == "low-polyMesh")
            && obj.GetComponent<SkinnedMeshRenderer>() != null;
    }

    /// <summary>
    /// Dispose all equipped combinations
    /// </summary>
    void RemoveAllCombinations(){
        {// destroy combinations
            foreach(OutfitCombination combination in equippedCombinations){
                combination.gameObject.SetActive(false);
                GameObject.Destroy(combination.gameObject);
            }
            equippedCombinations = new List<OutfitCombination>();
        }

        // enable all regular outfits
        foreach(var entry in equipped){
            Outfit outfit = entry.Value;
            outfit?.gameObject.SetActive(true);
        }
    }


    /// <summary>
    /// Equip outfit combination
    /// </summary>
    /// <param name="combination"></param>
    void EquipCombination(OutfitCombination combination){
        foreach(var slot in combination.slots){
            // disable regular outfit
            if(slot.Value != null){
                equipped[slot.Key].gameObject.SetActive(false);
            }
        }

        // instantiate combination
        GameObject newCombinationObject = GameObject.Instantiate(combination.gameObject);
        newCombinationObject.transform.parent = equippedCombinationsRoot.transform;
        newCombinationObject.name = combination.gameObject.name;
        newCombinationObject.SetActive(true);
        newCombinationObject.GetComponent<Outfit>().CopyBones(GetAvatarBodyMeshObject());

        // add combination into list
        equippedCombinations.Add(newCombinationObject.GetComponent<OutfitCombination>());
    }
    

    /// <summary>
    /// Seqrch in catalogue for most matching outfit combinations and replace equipped outfits with best suitable combinations if possible
    /// </summary>
    void TryReplaceOutfitsWithCombinations(){

        RemoveAllCombinations();

        List<KeyValuePair<int,OutfitCombination>> combinationsMatches = new List<KeyValuePair<int, OutfitCombination>>();
        
        { // find matches combinations
            List<OutfitCombination> combinationsAvailable = outfitCatalogue.combinations[avatarBodyConstoller.activeAvatarBodyType];
            foreach(OutfitCombination combination in combinationsAvailable){
                int matches = combination.matches(equipped);
                if(matches > 0){
                    combinationsMatches.Add(new KeyValuePair<int, OutfitCombination>(matches, combination));
                    //Debug.Log($"matches: {matches} with combination {combination.gameObject.name}");
                }
                
            }
        }
        
        combinationsMatches.Sort( (lhr, rhr) => { return -lhr.Key.CompareTo(rhr.Key); });
        List<OutfitCombination> combinationsToEquip = new List<OutfitCombination>();

        { // deside which to equip
            Category covered = Category.EVERYTHING;

            foreach(var entry in combinationsMatches){
                OutfitCombination combination = entry.Value;
                if((covered & combination.category) == combination.category){
                    covered &= ~combination.category;
                    combinationsToEquip.Add(combination);
                }
            }
        }
        
        {// equip combinations
            foreach(var combination in combinationsToEquip){
                Debug.Log($"combination: {combination.gameObject.name}");
                EquipCombination(combination);
            }
        }
    }   
}

}// !namespace ryabomar