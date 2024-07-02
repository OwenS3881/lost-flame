using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance { get; private set; }

    private List<Sound> songs = new List<Sound>();

    public string activeSong = "";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            s.source.loop = s.loop;
            s.source.playOnAwake = s.playOnAwake;

            if (s.isSong)
            {
                songs.Add(s);
            }

            if (s.playOnAwake)
            {
                if (!s.isSong)
                {
                    s.source.Play();
                }
                else
                {
                    PlaySong(s.name);
                }
            }
        }

        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);
    }

    private Sound GetSound(string name)
    {
        foreach (Sound s in sounds)
        {
            if (s.name.Equals(name))
            {
                return s;
            }
        }

        Debug.LogError("No sound found with name " + name);
        return null;
    }

    private Sound GetSong(string name)
    {
        foreach (Sound s in songs)
        {
            if (s.name.Equals(name))
            {
                return s;
            }
        }

        Debug.LogError("No song found with name " + name);
        return null;
    }

    public void Play(string name)
    {
        Sound sound = GetSound(name);

        if (sound == null) return;

        sound.source.Play();
    }

    public void PlayOneShot(string name)
    {
        Sound sound = GetSound(name);

        if (sound == null) return;

        sound.source.PlayOneShot(sound.clip);
    }

    public void StopSound(string name)
    {
        Sound sound = GetSound(name);

        if (sound == null) return;

        sound.source.Stop();

        if (sound.isSong && sound.name.Equals(activeSong))
        {
            activeSong = "";
        }
    }

    public void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
        activeSong = "";
    }

    public void PlaySong(string name)
    {
        Sound song = GetSong(name);

        if (song == null) return;

        StopAllSongs();

        song.source.Play();
        activeSong = song.name;
    }

    public void StopAllSongs()
    {
        foreach (Sound s in songs)
        {
            s.source.Stop();
        }
        activeSong = "";
    }
}
