using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{

    public AudioMixer audioMixer;

    public void SetVolume(float volume){

        audioMixer.SetFloat("volume", volume);
         
    }

    public void SetAntiAliasing(int antialiasingIndex){

        QualitySettings.antiAliasing = antialiasingIndex;

        //Debug.Log(antialiasingIndex);
    }

    public void SetFullscreen(bool isFullscreen){

        Screen.fullScreen  = isFullscreen;

    }
}
