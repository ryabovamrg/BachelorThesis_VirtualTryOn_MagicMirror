using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {

using Category = Outfit.Category;

/// <summary>
/// Combination of several Outfits
/// </summary>
public class OutfitCombination : MonoBehaviour
{
    /// <summary>
    /// References to outfit which this combination can replace
    /// </summary>
    public List<Outfit> outfits = new List<Outfit>();


    /// <summary>
    /// Overal category for outfit
    /// </summary>
    [HideInInspector] public Outfit.Category category;

    /// <summary>
    /// Outfits sorted byt its slots
    /// </summary>
    public Dictionary<Category, Outfit> slots = new Dictionary<Category, Outfit>{
        { Category.HEADGEAR,         null},
        { Category.FEETWEAR,         null},
        { Category.GLOVES,           null},
        { Category.UNDERWEAR_TOP,    null},
        { Category.UNDERWEAR_BOTTOM, null},
        { Category.TOPWEAR_TOP,      null},
        { Category.TOPWEAR_BOTTOM,   null}
    };


    /// <summary>
    /// Initialization
    /// </summary>
    void Awake(){
        category = Outfit.Category.NONE;

        foreach(Outfit outfit in outfits){
            if((category & outfit.category) != Outfit.Category.NONE) {
                throw new System.Exception("Contradicting outfit categories in " + this.gameObject.name);
            }

            category |= outfit.category;

            if((Category.HEADGEAR         & outfit.category) != Category.NONE){ slots[Category.HEADGEAR]         = outfit; }
            if((Category.FEETWEAR         & outfit.category) != Category.NONE){ slots[Category.FEETWEAR]         = outfit; }
            if((Category.GLOVES           & outfit.category) != Category.NONE){ slots[Category.GLOVES]           = outfit; }
            if((Category.UNDERWEAR_TOP    & outfit.category) != Category.NONE){ slots[Category.UNDERWEAR_TOP]    = outfit; }    
            if((Category.UNDERWEAR_BOTTOM & outfit.category) != Category.NONE){ slots[Category.UNDERWEAR_BOTTOM] = outfit; }        
            if((Category.TOPWEAR_TOP      & outfit.category) != Category.NONE){ slots[Category.TOPWEAR_TOP]      = outfit; }    
            if((Category.TOPWEAR_BOTTOM   & outfit.category) != Category.NONE){ slots[Category.TOPWEAR_BOTTOM]   = outfit; }    
        }

        Outfit thisOutfit = gameObject.AddComponent<Outfit>();
        thisOutfit.category = category;
        thisOutfit.catalogueId = -1;
        thisOutfit.isCombination = true;
    }


    /// <summary>
    /// Compare self with set of outfit and count how namy outfits are the same
    /// </summary>
    /// <param name="combinationToMatch">Set of outfits to compare with</param>
    /// <returns>how many items in combination matches with given combination</returns>
    public int matches(Dictionary<Category, Outfit> combinationToMatch){
        int result = 0;

        foreach(var slot in slots){
            if(slot.Value == null) continue;

            if(combinationToMatch[slot.Key] == null) return 0;

            if(slot.Value.catalogueId == combinationToMatch[slot.Key].catalogueId){
                result++;
            } else {
                return 0;
            }
            
        }

        return result;
    }
}

}// !namespace ryabomar