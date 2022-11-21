using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ryabomar {

/// <summary>
/// Class for quiting application
/// </summary>
public class QuitApplication : MonoBehaviour
{
    /// <summary>
    /// quit program
    /// </summary>
    public void Quit(){
        //  https://answers.unity.com/questions/899037/applicationquit-not-working-1.html

        // save any game data here
        #if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

} //!namespace ryabomar