using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void PlayGame(){

        SceneManager.LoadScene("SailGame");
    }

    public void OpenBehindTheScenes()
    {
        SceneManager.LoadScene("BehindTheScenes");
    }

    public void QuitGame(){
        
        Debug.Log("QUIT");
        Application.Quit();

    }


}
