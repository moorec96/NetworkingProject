/**
 * Class: 
 * Purpose: 
 * Author: 
 * Date: 
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
    /**
     * Method: 
     * Purpose: 
     * Parameters: 
     * Return Val: 
     * */
    public void loadScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    } 
}
