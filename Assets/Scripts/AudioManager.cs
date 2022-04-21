using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Audio Manager and Sound code used with permission from Alex Robert.

[System.Serializable]
public class Sound
{
    public string name;
    [Tooltip("Drag the audiofile into this field to have the game find the clip correctly.\nA sound with no clip won't throw errors AND won't play!")]
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;
    public bool loop;
    ///<summary>
    ///SoundOverlap used for sounds that might be called quickly enough for overlap.
    ///</summary>
    [Tooltip("SoundOverlap used for sounds that might be called quickly enough for overlap.")]
    public bool SoundOverlap;
    [HideInInspector]
    public bool isPaused;
    [HideInInspector]
    public AudioSource source;
    public AudioMixerGroup group;
}

public class AudioManager : MonoBehaviour
{
    [SerializeField] Sound[] sounds;
    private static AudioManager _instance;
    public static AudioManager Instance //Singleton Stuff
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("AudioManager is null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        foreach (Sound s in sounds)
        {
            //go through each sound in our sound array
            //and assign each field to an audio source component tied to this gameObject
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.outputAudioMixerGroup = s.group;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.isPaused = false;
        }
    }
    private Sound GetSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " was not found");
            return null;
        }
        return s;
    }
    public void Play(string name)
    {
        Sound s = GetSound(name);
        if (s == null) return;

        //TODO: see if this logic holds true for every situation
        //for now the loop logic is a workaround for primitive scene management
        //see ChangedActiveScene() for more details
        if (s.source.loop && s.source.isPlaying) return; //don't play something that's already looping

        if (s.SoundOverlap)
        {
            s.source.PlayOneShot(s.source.clip);
            return;
        };

        s.source.Play(); //if no special preconditions are met, play the sound. 
    }
    public void Stop(string name)
    {
        Sound s = GetSound(name);
        if (s == null) return;
        s.source.Stop();
    }

    public void UnPause(string name)
    {
        Sound s = GetSound(name);
        if (s == null) return;

        //I know this isn't the default behavior of UnPause() for audio sources
        //but this logic handled internally makes it much easier for outside scripts
        //to simply unpause rather than handle the current state of the audio source
        if (s.isPaused)
        {
            s.source.UnPause();
            s.isPaused = false;
        }
        else
            Play(name);
    }
    public void Pause(string name)
    {
        Sound s = GetSound(name);
        if (s == null) return;
        s.isPaused = true;
        s.source.Pause();
    }
}
