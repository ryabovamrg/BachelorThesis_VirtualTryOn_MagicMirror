using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Category = ryabomar.Outfit.Category;

namespace ryabomar {
/// <summary>
/// Makes thumbnails for outfits
/// </summary>
public class Showroom : MonoBehaviour
{
    /// <summary>
    /// Camera and outfit category pair
    /// </summary>
    [System.Serializable] public struct CameraEntry {
        public Category category;
        public Camera camera;
    }

    /// <summary>
    /// list of cameras with categories for making thumbnails
    /// </summary>
    public List<CameraEntry> showroomCameras = new List<CameraEntry>();

    /// <summary>
    /// Camera which will be used if no camera fits given outfit category
    /// </summary>
    public Camera fallbackCamera;

    /// <summary>
    /// Make thumbnail for given outfit with respect of its category
    /// </summary>
    /// <param name="outfit">outfit</param>
    /// <returns>thumbnail as texture</returns>
    public Texture2D MakeThumbnail(Outfit outfit) {
        { // show outfit
            outfit.gameObject.SetActive(true);
            
            // disable body and eyes
            outfit.bodyMeshObject.SetActive(false); 
            outfit.eyesMeshObject.gameObject.SetActive(false); 
        }

        Camera camera = SchooseCamera(outfit.category);

        int width  = camera.targetTexture.width;
        int height = camera.targetTexture.height;

        Texture2D thumbnail = new Texture2D(width, height, TextureFormat.RGBA32, false);
        {// make photo
            var backupActiveTexture = RenderTexture.active; // backup active render texture
            RenderTexture.active    = camera.targetTexture;
            camera.Render();

            // copy pixels into texture
            thumbnail.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            thumbnail.Apply();

            RenderTexture.active = backupActiveTexture;  // restore previous active texture
        }

        // enable body and eyes back
        outfit.bodyMeshObject.SetActive(true);
        outfit.eyesMeshObject.SetActive(true);  

        // hide outfit
        outfit.gameObject.SetActive(false);

        return thumbnail;
    }


    /// <summary>
    /// Choose camera with respect of category
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    Camera SchooseCamera(Category category){
        foreach(var entry in showroomCameras){
            if((entry.category & category) == category){
                return entry.camera;
            }
        }
        return fallbackCamera;
    }
}

}// !namespace ryabomar