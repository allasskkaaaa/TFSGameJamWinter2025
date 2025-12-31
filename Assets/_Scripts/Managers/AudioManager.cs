using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    List<AudioSource> currentAudioSources = new List<AudioSource>();
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup musicGroup;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        currentAudioSources.Add(GetComponent<AudioSource>());
    }

    public void PlayOneShot(AudioClip clip, bool isMusic)
    {
        Debug.Log("Playing " + clip.name);

        foreach (AudioSource source in currentAudioSources)
        {
            if (source.isPlaying)
                continue;

            source.PlayOneShot(clip);
            source.outputAudioMixerGroup = isMusic ? musicGroup : sfxGroup;
            return;
        }

        AudioSource temp = gameObject.AddComponent<AudioSource>();
        currentAudioSources.Add(temp);
        temp.PlayOneShot(clip);
        temp.outputAudioMixerGroup = isMusic ? musicGroup : sfxGroup;
    }

    public void PlayOneShotWithRandomPitch(AudioClip clip, bool isMusic, float minPitch, float maxPitch)
    {
        Debug.Log("Playing " + clip.name + " with random pitch");

        foreach (AudioSource source in currentAudioSources)
        {
            if (source.isPlaying)
                continue;

            float randomPitch = Random.Range(minPitch, maxPitch);
            source.pitch = randomPitch;

            source.PlayOneShot(clip);
            source.outputAudioMixerGroup = isMusic ? musicGroup : sfxGroup;
            return;
        }

        AudioSource temp = gameObject.AddComponent<AudioSource>();
        currentAudioSources.Add(temp);

        float randomPitchTemp = Random.Range(minPitch, maxPitch);
        temp.pitch = randomPitchTemp;

        temp.PlayOneShot(clip);
        temp.outputAudioMixerGroup = isMusic ? musicGroup : sfxGroup;
    }
}