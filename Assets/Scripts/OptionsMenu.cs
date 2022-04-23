using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{

    public AudioMixerGroup audioMixerGroup;
    public void SetMasterVolume(float volume)
    {
        var vol = volume;
        if (volume == 0)
        {
            vol = 0.000001f;
        }
        audioMixerGroup.audioMixer.SetFloat("Master", Mathf.Log10(vol) * 20);
    }
    public void SetMusicVolume(float volume)
    {
        var vol = volume;
        if (volume == 0)
        {
            vol = 0.000001f;
        }
        audioMixerGroup.audioMixer.SetFloat("Music", Mathf.Log10(vol) * 20);
    }
    public void SetSoundVolume(float volume)
    {
        var vol = volume;
        if (volume == 0)
        {
            vol = 0.000001f;
        }
        audioMixerGroup.audioMixer.SetFloat("Sound", Mathf.Log10(vol) * 20);
    }
}
