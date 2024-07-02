using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerPlayer : MonoBehaviour
{
    public void Play(string name)
    {
        AudioManager.instance.Play(name);
    }

    public void PlayOneShot(string name)
    {
        AudioManager.instance.PlayOneShot(name);
    }

    public void StopSound(string name)
    {
        AudioManager.instance.StopSound(name);
    }

    public void StopAllSounds()
    {
        AudioManager.instance.StopAllSounds();
    }

    public void PlaySong(string name)
    {
        AudioManager.instance.PlaySong(name);
    }

    public void StopAllSongs()
    {
        AudioManager.instance.StopAllSongs();
    }
}
