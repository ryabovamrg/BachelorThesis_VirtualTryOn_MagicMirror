using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Category = ryabomar.Outfit.Category;

namespace ryabomar {

/// <summary>
/// Catalog to organize outfits
/// </summary>
public class OutfitCatalogue : MonoBehaviour
{
    /// <summary>Reference to equipment manager</summary>
    public EquipmentManager equipmentManager;

    /// <summary>Collection of outfits for female medium body</summary>
    public GameObject femaleFitOutfitCollection;

    /// <summary>Collection of outfits for female large body</summary>
    public GameObject femaleFatOutfitCollection;

    /// <summary>Collection of outfits for male medium body</summary>
    public GameObject maleFitOutfitCollection;

    /// <summary>Collection of outfits for male large body</summary>
    public GameObject maleFatOutfitCollection;

    /// <summary>Collection of outfits for child body</summary>
    public GameObject childOutfitCollection;

    /// <summary>UI element which contains outfit buttons</summary>
    public GameObject uiButtonsContainer;

    /// <summary>UI button prefab</summary>
    public GameObject uiEquipOutfitButtonPrefab;

    /// <summary>Outfits per body type</summary>
    public Dictionary<AvatarBodyType, List<Outfit>>            outfits      = new Dictionary<AvatarBodyType, List<Outfit>>();

    /// <summary>Outfit combinations per body type</summary>
    public Dictionary<AvatarBodyType, List<OutfitCombination>> combinations = new Dictionary<AvatarBodyType, List<OutfitCombination>>();

    /// <summary>Reference to the showroom. There thumbnails are made</summary>
    public Showroom showroom;

    /// <summary>
    /// Initialization
    /// </summary>
    void Start(){
        if(showroom == null){ throw new Exception("missing showroom"); }

        // discover outfit collections per body type
        if( femaleFitOutfitCollection == null || femaleFatOutfitCollection == null
         || maleFitOutfitCollection   == null || maleFatOutfitCollection   == null
         || childOutfitCollection     == null) {
             throw new Exception("missing some of outfit collections");
        }

        if(uiEquipOutfitButtonPrefab == null) throw new Exception("need button prefab");
        if(uiButtonsContainer        == null) throw new Exception("need buttons container");

        // discover outfits
        outfits[AvatarBodyType.FEMALE_FIT] = DiscoverOutfits(AvatarBodyType.FEMALE_FIT, femaleFitOutfitCollection.transform.Find("outfits").gameObject);
        outfits[AvatarBodyType.FEMALE_FAT] = DiscoverOutfits(AvatarBodyType.FEMALE_FAT, femaleFatOutfitCollection.transform.Find("outfits").gameObject);
        outfits[AvatarBodyType.MALE_FIT]   = DiscoverOutfits(AvatarBodyType.MALE_FIT,   maleFitOutfitCollection.transform.Find("outfits").gameObject);
        outfits[AvatarBodyType.MALE_FAT]   = DiscoverOutfits(AvatarBodyType.MALE_FAT,   maleFatOutfitCollection.transform.Find("outfits").gameObject);
        outfits[AvatarBodyType.CHILD]      = DiscoverOutfits(AvatarBodyType.CHILD,      childOutfitCollection.transform.Find("outfits").gameObject);


        // make buttons
        MakeButtons(AvatarBodyType.FEMALE_FIT, outfits[AvatarBodyType.FEMALE_FIT]);
        MakeButtons(AvatarBodyType.FEMALE_FAT, outfits[AvatarBodyType.FEMALE_FAT]);
        MakeButtons(AvatarBodyType.MALE_FIT  , outfits[AvatarBodyType.MALE_FIT]);
        MakeButtons(AvatarBodyType.MALE_FAT  , outfits[AvatarBodyType.MALE_FAT]);
        MakeButtons(AvatarBodyType.CHILD     , outfits[AvatarBodyType.CHILD]);


        // discover combinations
        combinations[AvatarBodyType.FEMALE_FIT] = DiscoverOutfitCombinations(AvatarBodyType.FEMALE_FIT, femaleFitOutfitCollection.transform.Find("combinations").gameObject);
        combinations[AvatarBodyType.FEMALE_FAT] = DiscoverOutfitCombinations(AvatarBodyType.FEMALE_FAT, femaleFatOutfitCollection.transform.Find("combinations").gameObject);
        combinations[AvatarBodyType.MALE_FIT]   = DiscoverOutfitCombinations(AvatarBodyType.MALE_FIT,   maleFitOutfitCollection.transform.Find("combinations").gameObject);
        combinations[AvatarBodyType.MALE_FAT]   = DiscoverOutfitCombinations(AvatarBodyType.MALE_FAT,   maleFatOutfitCollection.transform.Find("combinations").gameObject);
        combinations[AvatarBodyType.CHILD]      = DiscoverOutfitCombinations(AvatarBodyType.CHILD,      childOutfitCollection.transform.Find("combinations").gameObject);


        UiOutfitButtonsContainer buttonContainer = uiButtonsContainer.GetComponent<UiOutfitButtonsContainer>();
        buttonContainer.EnableButtonsForBodyType(AvatarBodyType.FEMALE_FIT);

        // no needs anymore
        showroom.gameObject.SetActive(false);
    }


    /// <summary>
    /// Discover outfits
    /// </summary>
    /// <param name="bodyType">body type</param>
    /// <param name="catalogueRootObject">child of this object will be picked as outfits</param>
    /// <returns></returns>
    List<Outfit> DiscoverOutfits(AvatarBodyType bodyType, GameObject catalogueRootObject){
        List<Outfit> outfitList = new List<Outfit>();

        foreach(Transform catalogueItem in catalogueRootObject.transform){
            catalogueItem.gameObject.SetActive(true);

            Outfit outfit = catalogueItem.GetComponent<Outfit>();

            if(outfit == null) {
                throw new Exception("object + " + catalogueItem.gameObject.name + " has no Outfit component!");
            }

            if(outfit.bodyMeshObject == null) { throw new Exception("no body mesh in "    + outfit.gameObject.name); }   
            if(outfit.eyesMeshObject == null) { throw new Exception("no eyes mesh in "    + outfit.gameObject.name); }
            if(outfit.meshObjects.Count == 0) { throw new Exception("no outfit items in " + outfit.gameObject.name); }
            
            outfit.catalogueId = outfitList.Count;
            outfitList.Add(outfit);

            catalogueItem.gameObject.SetActive(false);
        }
        return outfitList;
    }


    /// <summary>
    /// Discover outfit combinations
    /// </summary>
    /// <param name="bodyType">body type</param>
    /// <param name="rootObject">child of this object will be picked as outfit combinations</param>
    /// <returns></returns>
    List<OutfitCombination> DiscoverOutfitCombinations(AvatarBodyType bodyType, GameObject rootObject){
        List<OutfitCombination> combinationList = new List<OutfitCombination>();

        foreach(Transform child in rootObject.transform){
            child.gameObject.SetActive(true);
            OutfitCombination combination = child.GetComponent<OutfitCombination>();
            
            if(combination == null)
                continue;

            combinationList.Add(combination);
            child.gameObject.SetActive(false);
        }

        return combinationList;
    }


    /// <summary>
    /// Create UI buttons for given outfit collection
    /// </summary>
    /// <param name="bodyType">body type</param>
    /// <param name="outfitList">list of outfits</param>
    void MakeButtons(AvatarBodyType bodyType, List<Outfit> outfitList){
        for(int i = 0; i < outfitList.Count; i++){
            // instantiate button
            GameObject button = GameObject.Instantiate(uiEquipOutfitButtonPrefab);
            button.transform.SetParent(uiButtonsContainer.transform);

            // outfit categoy and body type
            OutfitButton outfitButtonComponent = button.AddComponent<OutfitButton>();
            outfitButtonComponent.bodyType  = bodyType;
            outfitButtonComponent.category  = outfitList[i].category;

            // bind onclick action
            int id = i;
            //AvatarBodyType bodyType = outfitList[i].bodyType;
            button.GetComponent<Button>().onClick.AddListener(() => { 
                    equipmentManager.Equip(bodyType, id); 
                });

            outfitButtonComponent.SetThumbnail(showroom.MakeThumbnail(outfitList[i]));
        }
    }

    /// <summary>
    /// Get outfit by given index
    /// </summary>
    /// <param name="bodyType">body type</param>
    /// <param name="id">index</param>
    /// <returns>reference to outfit</returns>
    public Outfit GetOutfit(AvatarBodyType bodyType, int id) {
        return outfits[bodyType][id];
    }
}

} //!namespace ryabomar