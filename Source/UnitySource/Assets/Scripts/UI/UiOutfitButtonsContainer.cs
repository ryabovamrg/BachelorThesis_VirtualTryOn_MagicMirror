using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {


/// <summary>
/// Container for outfit buttons
/// </summary>
public class UiOutfitButtonsContainer : MonoBehaviour
{
    /// <summary>
    /// enabled categories
    /// </summary>
    public Outfit.Category enabledMask = Outfit.Category.EVERYTHING;

    /// <summary>
    /// body type
    /// </summary>
    public AvatarBodyType activeBodyType;

    /// <summary>
    /// turn buttons if their category match mask
    /// </summary>
    /// <param name="mask">mask</param>
    /// <param name="value">on or off</param>
    public void SetButtonsEnabled(Outfit.Category mask, bool value){
        if(value){
            enabledMask |= mask;
        } else {
            enabledMask &= ~mask;
        }

        foreach(Transform button in transform){
            OutfitButton outfitButton = button.GetComponent<OutfitButton>();
            if(outfitButton != null){
                if(outfitButton.bodyType == activeBodyType
                 && (outfitButton.category & enabledMask) != Outfit.Category.NONE) 
                {
                    button.gameObject.SetActive(true);
                } else {
                    button.gameObject.SetActive(false);
                }
            }
        }

    }

    /// <summary>Turn buttons for headgear category</summary><param name="value">value</param>
    public void SetButtonsEnabledHeadgear        (bool value) { SetButtonsEnabled(Outfit.Category.HEADGEAR, value);         }

    /// <summary>Turn buttons for gloves category</summary><param name="value">value</param>
    public void SetButtonsEnabledGloves          (bool value) { SetButtonsEnabled(Outfit.Category.GLOVES, value);           }

    /// <summary>Turn buttons for underwear top category</summary><param name="value">value</param>
    public void SetButtonsEnabledUnderwearTop    (bool value) { SetButtonsEnabled(Outfit.Category.UNDERWEAR_TOP, value);    }

    /// <summary>Turn buttons for underwear bottom category</summary><param name="value">value</param>
    public void SetButtonsEnabledUnderwearBottom (bool value) { SetButtonsEnabled(Outfit.Category.UNDERWEAR_BOTTOM, value); }

    /// <summary>Turn buttons for feetgear category</summary><param name="value">value</param>
    public void SetButtonsEnabledFeetwear        (bool value) { SetButtonsEnabled(Outfit.Category.FEETWEAR, value);         }

    /// <summary>Turn buttons for topwear top category</summary><param name="value">value</param>
    public void SetButtonsEnabledTopwearTop      (bool value) { SetButtonsEnabled(Outfit.Category.TOPWEAR_TOP, value);      }

    /// <summary>Turn buttons for topwear bottom category</summary><param name="value">value</param>
    public void SetButtonsEnabledTopwearBottom   (bool value) { SetButtonsEnabled(Outfit.Category.TOPWEAR_BOTTOM, value);   }


    /// <summary>
    /// turn bottons for body type
    /// </summary>
    /// <param name="bodyType">body type</param>
    public void EnableButtonsForBodyType (AvatarBodyType bodyType) {
        this.activeBodyType = bodyType;

        foreach(Transform button in transform){
            OutfitButton outfitButton = button.GetComponent<OutfitButton>();
            if(outfitButton != null){
                if(outfitButton.bodyType == activeBodyType) {
                    button.gameObject.SetActive(true);
                } else {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>enable buttons for female medium body type</summary>
    public void EnableButtonsForFemaleFit() { EnableButtonsForBodyType(AvatarBodyType.FEMALE_FIT); }

    /// <summary>enable buttons for female large body type</summary>
    public void EnableButtonsForFemaleFat() { EnableButtonsForBodyType(AvatarBodyType.FEMALE_FAT); }

    /// <summary>enable buttons for male medium body type</summary>
    public void EnableButtonsForMaleFit  () { EnableButtonsForBodyType(AvatarBodyType.MALE_FIT);   }

    /// <summary>enable buttons for male large body type</summary>
    public void EnableButtonsForMaleFat  () { EnableButtonsForBodyType(AvatarBodyType.MALE_FAT);   }

    /// <summary>enable buttons for child body type</summary>
    public void EnableButtonsForChild    () { EnableButtonsForBodyType(AvatarBodyType.CHILD);      }
}

} //!namespace ryabomar