using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ryabomar {

/// <summary>
/// UI element to represent single outfit in catalogue
/// </summary>
public class OutfitButton : MonoBehaviour
{
    /// <summary>Outfit category</summary>
    public Outfit.Category category;

    /// <summary>Outfit body type</summary>
    public AvatarBodyType  bodyType;

    /// <summary>Set thumbnail for button</summary>
    public void SetThumbnail(Texture2D thumbnail){
        RawImage image = this.transform.Find("thumbnail")?.GetComponent<RawImage>();
        if(image != null) {
            image.texture = thumbnail; 
        }
    }
}

} //!namespace ryabomar