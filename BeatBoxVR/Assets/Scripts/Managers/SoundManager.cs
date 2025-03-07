using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static EnvironmentManager;
using static SoundManager;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer; // Assign this in the Inspector

    // Serializable class to hold sound data for drums and cymbals
    [System.Serializable]
    public class PercussionSound
    {
        public string tag;           // Tag to identify the percussion instrument
        public AudioClip sound;      // Sound for the percussion hit
    }

    [System.Serializable]
    public class DrumKit
    {
        public string KitName;           // Tag to identify the percussion instrument
        public Sprite KitImage;
        public PercussionSound[] drumKit;
    }

    // Lists to hold the sounds for drums and cymbals
    public List<PercussionSound> percussionSounds;
    public List<DrumKit> drumKitList;
    private int currDrumKitIndex = 0;
    public TextMeshProUGUI currDrumKitTMP;
    public Image currDrumKitSprite;

    // Play a sound based on the tag, position, and velocity
    public void PlaySound(string tag, Vector3 position, float velocity)
    {
        AudioClip clip = GetClipForTag(tag);
        if (clip != null)
        {
            GameObject soundObject = new GameObject("TemporaryAudio");
            soundObject.transform.position = position;
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1.0f; // Sets the sound to be 3D/ Spatial

            // Adjusts volume and pitch based on the hit velocity 
            audioSource.volume = Mathf.Clamp(velocity, 0.0f, 1.0f);
            audioSource.pitch = 1.0f + velocity * 0.1f;

            audioSource.Play();
            Destroy(soundObject, clip.length);
        }
        else
        {
            Debug.LogWarning("No sound found for tag: " + tag);
        }
    }

    // Helper method to get the appropriate AudioClip based on the tag
    private AudioClip GetClipForTag(string tag)
    {
        PercussionSound percussionSound = percussionSounds.Find(ps => ps.tag == tag);
        return percussionSound?.sound;
    }

    // Method to adjust the master volume
    public void SetMasterVolume(float sliderValue)
    {
        // Ensure sliderValue is never 0 to avoid log10(0) which is undefined
        sliderValue = Mathf.Max(sliderValue, 0.0001f);
        float volumeDb = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat("MasterVolume", volumeDb);
    }

    // Method to adjust the drum volume
    public void SetDrumVolume(float volume)
    {
        // Convert the volume to a logarithmic scale and set it
        audioMixer.SetFloat("DrumVolume", Mathf.Log10(volume) * 20);
    }

    public void loadDrumKit(int kitIndex)
    {
        Debug.Log(kitIndex);
        Debug.Log(drumKitList[kitIndex].KitName);
        currDrumKitTMP.text = drumKitList[kitIndex].KitName;
        currDrumKitSprite.sprite = drumKitList[kitIndex].KitImage;

        for (int i = 0; i < percussionSounds.Count; i++)
        {
            percussionSounds[i] = drumKitList[kitIndex].drumKit[i]; 
        }
    }

    public void incrementDrumKit()
    {
        currDrumKitIndex = (currDrumKitIndex + 1) % drumKitList.Count;
        loadDrumKit(currDrumKitIndex);
    }

    public void decrementDrumKit()
    {
        currDrumKitIndex = (currDrumKitIndex - 1 + drumKitList.Count) % drumKitList.Count;
        loadDrumKit(currDrumKitIndex);
    }

}
