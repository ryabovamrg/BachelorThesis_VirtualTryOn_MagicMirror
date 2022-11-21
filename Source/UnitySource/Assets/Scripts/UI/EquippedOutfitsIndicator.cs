using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ryabomar {

using Category = Outfit.Category;

/// <summary>
/// indicator for equipped slots
/// </summary>
public class EquippedOutfitsIndicator : MonoBehaviour
{
    /// <summary>
    /// reference to equipment manager
    /// </summary>
    public EquipmentManager equipmentManager;

    /// <summary>headgear slot indicator</summary>
    public Image headgear;

    /// <summary>feetwear slot indicator</summary>
    public Image feetwear;

    /// <summary>gloves slot indicator</summary>
    public Image gloves;

    /// <summary>underwear bottom slot indicator</summary>
    public Image underwearBottom;

    /// <summary>underwear top slot indicator</summary>
    public Image underwearTop;

    /// <summary>topwear bottom slot indicator</summary>
    public Image topwearBottom;

    /// <summary>topwear top slot indicator</summary>
    public Image topwearTop;

    /// <summary>
    /// reference to equipment manager slots
    /// </summary>
    Dictionary<Category, Outfit> equippedOutfits;


    /// <summary>
    /// initialization
    /// </summary>
    void Start(){
        equippedOutfits = equipmentManager.equipped;
    }


    /// <summary>
    /// update state of indicators
    /// </summary>
    void Update()
    {
        headgear.enabled        = equippedOutfits[Category.HEADGEAR]         != null;
        feetwear.enabled        = equippedOutfits[Category.FEETWEAR]         != null;
        gloves.enabled          = equippedOutfits[Category.GLOVES]           != null;
        underwearBottom.enabled = equippedOutfits[Category.UNDERWEAR_BOTTOM] != null;
        underwearTop.enabled    = equippedOutfits[Category.UNDERWEAR_TOP]    != null;
        topwearBottom.enabled   = equippedOutfits[Category.TOPWEAR_BOTTOM]   != null;
        topwearTop.enabled      = equippedOutfits[Category.TOPWEAR_TOP]      != null;
    }
}

} //!namespace ryabomar