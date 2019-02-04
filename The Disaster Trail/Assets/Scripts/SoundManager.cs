using UnityEngine;
using System.Collections;

// An enum for maintaining the different background music available
public enum BackgroundMusic
{
    None = -1,
    OverworldBGM,
    HurricaneBGM
}

// An enum for maintaining the different sound effects available
public enum SoundEffect
{
    None = -1,
    TimeIndicatorChange,
    GlassBreak
}

// An enum for maintaining the state of playing sound effects and background music
public enum SoundPlayerState
{
    Playing,
    Paused,
    Stopped
}

// An enum for maintaining sound events
public enum SoundEvent
{
    Playing,
    Paused,
    Stopped,
    Destroyed
}

// A class for managing audio sources and event dispatching related to their play.
public class AudioSourceManager
{
    // Our SoundEventCallback, implement a function with this signature and pass to constructor to receive callbacks
    public delegate void SoundEventCallback(SoundEvent soundEvent, float position, AudioClip clip);

    public AudioSource source;
    SoundEventCallback callback;

    public AudioSourceManager(AudioSource source, SoundEventCallback soundEventCallback = null)
    {
        this.source = source;
        this.callback = soundEventCallback;
    }

    // Play the audio source, if there is a callback setup then dispatch the Playing SoundEvent
    public void Play()
    {
        source.Play();
        if (callback != null)
        {
            callback(SoundEvent.Playing, source.time, source.clip);
        }
    }

    // Pause the audio source, if there is a callback setup then dispatch the Paused SoundEvent
    public void Pause()
    {
        source.Pause();
        if (callback != null)
        {
            callback(SoundEvent.Paused, source.time, source.clip);
        }
    }

    // Stop the audio source, if there is a callback setup then dispatch the Stopped SoundEvent
    public void Stop()
    {
        float position = source.time;
        source.Stop();
        if (callback != null)
        {
            callback(SoundEvent.Stopped, position, source.clip);
        }
    }

    // Cleanup the audio source, if there is a callback setup then dispatch the Destroyed SoundEvent
    public void Destroy()
    {
        float position = source.time;
        AudioClip clip = source.clip;
        MonoBehaviour.Destroy(source);
        if (callback != null)
        {
            callback(SoundEvent.Destroyed, position, clip);
        }
    }
}

// Our main sound manager class
public class SoundManager : MonoBehaviour
{
    // Allows other scripts to call functions from SoundManager (part of our singleton pattern).
    public static SoundManager instance = null;
    // The default pitch for any sound in unity
    public float sfxVolume = .5f, minSFXVolume = 0, maxSFXVolume = 1;
    protected ArrayList sfxSources;
    public SoundPlayerState sfxState;
    public float bgmVolume = .5f, minBGMVolume = 0, maxBGMVolume = 1f;
    public BackgroundMusic currentBGM;
    // Drag a reference to the audio source which will play the BGM music.
    public AudioSource bgmSource;
    public SoundPlayerState bgmState = SoundPlayerState.Playing;
    // The lowest a sound effect will be randomly pitched.
    public float lowPitchRange = .95f;
    // The highest a sound effect will be randomly pitched.
    public float highPitchRange = 1.05f;
    //Current track playing 
    public AudioClip currClip;
    //Current tracks sfx
    public AudioSource currSFX;

    public AudioClip[] bgmClips, sfxClips;

    void Start()
    {
        // Initialiaze our array of sound effects.
        sfxSources = new ArrayList();

        // Set our soundplayerstates for sfx and bgm, and set bgm volume
        sfxState = SoundPlayerState.Playing;
        bgmState = bgmSource.isPlaying ? SoundPlayerState.Playing : SoundPlayerState.Stopped;
        bgmSource.volume = bgmVolume;
    }

    void Awake()
    {
        // Check if there is already an instance of SoundManager.
        if (instance == null)
        {
            // If not, set it to this.
            instance = this;
        }
        // If another instance already exists then...
        else if (instance != this)
        {
            // Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);
        }

        // Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
    }

    // Update our sfx array and remove any sound effects that are done playing.
    void Update()
    {
        // Iterate through our sound effects and see if they are done yet... Then remove them.
        // Don't do this if the sfxstate is paused since the sfx won't be playing.
        if (sfxState != SoundPlayerState.Paused)
        {
            for (int i = sfxSources.Count - 1; i >= 0; i--)
            {
                AudioSourceManager sfx = (AudioSourceManager)sfxSources[i];
                if (!sfx.source.isPlaying)
                {
                    sfxSources.Remove(sfx);
                    sfx.Destroy();
                }
            }
        }

        // If the bgm is not on loop and finishes, update bgmState to match
        if (bgmState == SoundPlayerState.Playing && !bgmSource.isPlaying)
        {
            bgmState = SoundPlayerState.Stopped;
        }
    }

    // Get the SoundEffect enum that corresponds to the specified AudioSource
    public SoundEffect AudioSourceToSoundEffect(AudioSource source)
    {
        return AudioClipToSoundEffect(source.clip);
    }

    // Get the SoundEffect enum that corresponds to the specified AudioClip
    public SoundEffect AudioClipToSoundEffect(AudioClip clip)
    {
        for(int i = 0; i < sfxClips.Length; i++)
        {
            if (sfxClips[i] == clip)
            {
                return (SoundEffect)i;
            }
        }

        return SoundEffect.None;
    }

    // Get the AudioSourceManager that handles the specified AudioSource. Returns null if it isn't in the sfxSources.
    public AudioSourceManager AudioSourceToAudioSourceManager(AudioSource source)
    {
        for (int i = 0; i < sfxSources.Count; i++)
        {
            AudioSourceManager sfx = (AudioSourceManager)sfxSources[i];
            if (sfx.source == source)
            {
                return sfx;
            }
        }

        return null;
    }

    // Dynamically creates a new AudioSource with specified clip, volume, and panning.
    protected AudioSource createAudioSource(AudioClip clip, float volume, bool loop = false, float pitch = 1, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0)
    {
        // Create a new AudioSource to play the clip on.
        AudioSource audio = gameObject.AddComponent<AudioSource>();

        // Set the clip of our AudioSource to the clip passed in as a parameter.
        audio.clip = clip;

        // Set the volume of the sound.
        audio.volume = volume;

        // Is the sound going to loop indefinitely.
        audio.loop = loop;

        // Set the pitch of the sound.
        audio.pitch = pitch;

        // What time offset do we want the sound to start at.
        audio.time = positionOffset;

        // Are we panning the sound?
        audio.panStereo = pan;

        // Spatial blend is 0, meaning 2D.
        // Can add this as a global setting but probably a bad idea for now since 3D spatial sounds require setting positions and other data to work properly.
        audio.spatialBlend = 0;

        // Add sfx to arraylist.
        sfxSources.Add(new AudioSourceManager(audio, soundEventCallback));

        // Return a reference to the AudioSource for later use.
        return audio;
    }

    // Used to play single sound clips.
    public AudioSource PlaySFX(SoundEffect sfx, bool loop = false, float pitch = 1, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        return PlaySFX(sfxClips[(int)sfx], loop, pitch, positionOffset, soundEventCallback, pan, delay);
    }

    // Used to play single sound clips.
    public AudioSource PlaySFX(AudioClip clip, bool loop = false, float pitch = 1, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        // Create a new sfx.
        AudioSource sfx = createAudioSource(clip, sfxVolume, loop, pitch, positionOffset, soundEventCallback, pan);

        // Play the clip if we are in sfx playing or stopped state.
        // Change sfx state to playing if it was stopped before
        if (sfxState != SoundPlayerState.Paused)
        {
            sfx.Play(delay);
            sfxState = SoundPlayerState.Playing;
            currClip = clip;
            currSFX = sfx;
        }


        // Return a reference to the AudioSource for later use.

        return sfx;
    }

    // The number of ms to play a sfx for.
    // Will automatically loop the sfx until the duration is over.
    public AudioSource PlaySFXFor(SoundEffect sfx, int durationMS, float pitch = 1, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        return PlaySFXFor(sfxClips[(int)sfx], durationMS, pitch, positionOffset, soundEventCallback, pan, delay);
    }

    // The number of ms to play a sfx for.
    // Will automatically loop the sfx until the duration is over.
    public AudioSource PlaySFXFor(AudioClip clip, int durationMS, float pitch = 1, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        // Start our sound effect.
        AudioSource sfx = PlaySFX(clip, true, pitch, positionOffset, soundEventCallback, pan, delay);

        // Then start our coroutine which will stop it
        StartCoroutine(StopSFXInMS(sfx, durationMS));

        // Return a reference to our AudioSource
        return sfx;
    }

    // The IEnumerator that handles pausing a sound after a specified duration.
    IEnumerator StopSFXInMS(AudioSource sfx, int durationMS)
    {
        // Wait for the specified duration.
        yield return new WaitForSeconds(durationMS / 1000.0f);

        // Pause our sound effect if the source still exists.
        // I.e., since we waited, something could have happened to it in the meantime.
        if (sfx != null)
        {
            StopSFX(sfx);
        }
    }

    // Used to play single sound clips and randomly slightly changes the pitch.
    public AudioSource PlayRandomizedSFX(SoundEffect sfx, bool loop = false, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        return PlayRandomizedSFX(sfxClips[(int)sfx], loop, positionOffset, soundEventCallback, pan, delay);
    }

    // Used to play single sound clips and randomly slightly changes the pitch.
    public AudioSource PlayRandomizedSFX(AudioClip clip, bool loop = false, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        // Choose a random pitch to play back our clip at between our high and low pitch ranges.
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        // Create a new sfx
        AudioSource sfx = PlaySFX(clip, loop, randomPitch, positionOffset, soundEventCallback, pan, delay);

        // Return a reference to the AudioSource for later use.
        return sfx;
    }

    // PlayRandomSFX chooses a passed AudioClip randomly and plays it.
    // params allows us to pass in a variable number of AudioClips without needing to explicitly create an AudioClip[].
    public AudioSource PlayRandomSFX(params AudioClip[] clips)
    {
        // Generate a random number between 0 and the length of our array of clips passed in.
        int randomIndex = Random.Range(0, clips.Length);

        // Play a single randomized clip randomly selected from the passed in args.
        AudioSource sfx = PlaySFX(clips[randomIndex]);

        // Return a reference to the AudioSource for later use.
        return sfx;
    }

    // PlayRandomSFX chooses a passed AudioClip randomly and plays it.
    // Must create an explicit AudioClip[], but allows for customization of parameters.
    public AudioSource PlayRandomSFX(AudioClip[] clips, bool loop = false, float pitch = 1, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        // Generate a random number between 0 and the length of our array of clips passed in.
        int randomIndex = Random.Range(0, clips.Length);

        // Play a single randomized clip randomly selected from the passed in args.
        AudioSource sfx = PlaySFX(clips[randomIndex], loop, pitch, positionOffset, soundEventCallback, pan, delay);

        // Return a reference to the AudioSource for later use.
        return sfx;
    }

    // PlayRandomRandomizedSFX chooses a passed AudioClip randomly and plays it with a randomized pitch.
    // params allows us to pass in a variable number of AudioClips without needing to explicitly create an AudioClip[].
    public AudioSource PlayRandomRandomizedSFX(params AudioClip[] clips)
    {
        // Generate a random number between 0 and the length of our array of clips passed in.
        int randomIndex = Random.Range(0, clips.Length);

        // Play a single randomized clip randomly selected from the passed in args.
        AudioSource sfx = PlayRandomizedSFX(clips[randomIndex]);

        // Return a reference to the AudioSource for later use.
        return sfx;
    }

    // PlayRandomRandomizedSFX chooses a passed AudioClip randomly and plays it with a randomized pitch.
    // Must create an explicit AudioClip[], but allows for customization of parameters.
    public AudioSource PlayRandomRandomizedSFX(AudioClip[] clips, bool loop = false, float positionOffset = 0, AudioSourceManager.SoundEventCallback soundEventCallback = null, float pan = 0, ulong delay = 0)
    {
        // Generate a random number between 0 and the length of our array of clips passed in.
        int randomIndex = Random.Range(0, clips.Length);

        // Play a single randomized clip randomly selected from the passed in args.
        AudioSource sfx = PlayRandomizedSFX(clips[randomIndex], loop, positionOffset, soundEventCallback, pan, delay);

        // Return a reference to the AudioSource for later use.
        return sfx;
    }

    // Play all sound effects that were paused (if any).
    // Sets the sfx state to playing.
    public void PlayAllSFX()
    {
        if (sfxState != SoundPlayerState.Playing)
        {
            for (int i = 0; i < sfxSources.Count; i++)
            {
                ((AudioSourceManager)sfxSources[i]).Play();
            }

            sfxState = SoundPlayerState.Playing;
        }
    }

    // Pauses a given sound effect.
    public void PauseSFX(AudioSource sfx)
    {
        PauseSFX(AudioSourceToAudioSourceManager(sfx));
    }

    // Pauses a given sound effect.
    // Note: this will fire a paused sound effect event if that was setup for the sound effect.
    public void PauseSFX(AudioSourceManager sfx)
    {
        if (sfx != null)
        {
            sfx.Pause();
        }
    }

    // Pauses all sound effects currently playing (if any).
    // Sets the sfx state to paused.
    // Note: this will queue up paused sound effects when they are added to the sound manager (e.g., PlaySFX()).
    // These won't play until PlayAllSFX() is called.
    public void PauseAllSFX()
    {
        if (sfxState != SoundPlayerState.Paused)
        {
            for (int i = 0; i < sfxSources.Count; i++)
            {
                PauseSFX((AudioSourceManager)sfxSources[i]);
            }

            sfxState = SoundPlayerState.Paused;
        }
    }

    // Stops a given sound effect.
    public void StopSFX(AudioSource sfx)
    {
        StopSFX(AudioSourceToAudioSourceManager(sfx));
    }

    // Stops a given sound effect.
    // Note: this will fire a stopped sound effect event if that was setup for the sound effect.
    public void StopSFX(AudioSourceManager sfx)
    {
        if (sfx != null)
        {
            sfx.Stop();
        }
    }

    // Stops all sounds effects currently playing (if any).
    // Sets the sfx state to stopped.
    // Note: this will also result in all stopped sfx being removed.
    public void StopAllSFX()
    {
        for (int i = 0; i < sfxSources.Count; i++)
        {
            StopSFX((AudioSourceManager)sfxSources[i]);
        }

        sfxState = SoundPlayerState.Stopped;
    }

    // Set the global volume of all future sound effects.
    // Updates the volume of currently playing sound effects as well.
    public void SetSFXVolume(float newVolume)
    {
        // Set the new sfx volume and ensure that it is clamped to the sfx volume boundaries.
        sfxVolume = Mathf.Clamp(newVolume, minSFXVolume, maxSFXVolume);

        // Go through each currently playing sound effect and update its volume.
        for (int i = 0; i < sfxSources.Count; i++)
        {
            ((AudioSourceManager)sfxSources[i]).source.volume = sfxVolume;
        }
    }

    // Get the BackgroundMusic enum that corresponds to the specified AudioClip.
    // Returns None if the AudioClip is not part of the BGM.
    public BackgroundMusic AudioClipToBackgroundMusic(AudioClip clip)
    {
        for (int i = 0; i < bgmClips.Length; i++)
        {
            if (bgmClips[i] == clip)
            {
                return (BackgroundMusic)i;
            }
        }

        return BackgroundMusic.None;
    }

    // Switch the background music.
    public void SwitchBGM(BackgroundMusic bgm)
    {
        SwitchBGM(bgmClips[(int)bgm]);
    }

    // Switch the background music.
    public void SwitchBGM(AudioClip clip)
    {
        // Don't do anything if it is the same audioclip
        if (clip == bgmSource.clip)
        {
            return;
        }

        // Save bgm state before loading new sound
        SoundPlayerState originalBGMState = bgmState;

        // Stop background music if it is currently playing or paused before switching bgm
        if (bgmState != SoundPlayerState.Stopped) {
            StopBGM();
        }

        // Set bgmSource clip to the new clip
        bgmSource.clip = clip;

        // Restore bgm state
        if (originalBGMState == SoundPlayerState.Paused)
        {
            PauseBGM();
        }
        else if (originalBGMState == SoundPlayerState.Playing)
        {
            PlayBGM();
        }
    }

    // Play the background music if it isn't playing already.
    public void PlayBGM(bool loop = true, float pan = 0, ulong delay = 0)
    {
        // If we aren't already playing something then start the background music.
        if (bgmState != SoundPlayerState.Playing)
        {
            bgmSource.loop = loop;
            bgmSource.panStereo = pan;
            bgmSource.Play(delay);
            bgmState = SoundPlayerState.Playing;
        }
    }

    // The number of ms to play a bgm for.
    // Will automatically loop the sfx until the duration is over.
    public void PlayBGMFor(BackgroundMusic bgm, int durationMS, float pan = 0, ulong delay = 0)
    {
        PlayBGMFor(bgmClips[(int)bgm], durationMS, pan, delay);
    }

    // The number of ms to play a bgm for.
    // Will automatically loop the sfx until the duration is over.
    public void PlayBGMFor(AudioClip clip, int durationMS, float pan = 0, ulong delay = 0)
    {
        StartCoroutine(PlayBGMForMS(clip, durationMS, pan, delay));
    }

    // The IEnumerator that handles the nitty gritty of playing and stoping a sound.
    IEnumerator PlayBGMForMS(AudioClip clip, int durationMS, float pan, ulong delay = 0)
    {
        // Switch to our new bgm.
        SwitchBGM(clip);

        // Start our new bgm.
        PlayBGM(true, pan, delay);

        // Wait for the specified duration.
        yield return new WaitForSeconds(durationMS / 1000.0f);

        // Stop our bgm.
        StopBGM();
    }

    // Pause the background music only if it is currently playing.
    public void PauseBGM()
    {
        // If we are playing something then pause it!
        if (bgmState == SoundPlayerState.Playing) {
            bgmSource.Pause();
        }

        // No matter what, our state is now paused.
        bgmState = SoundPlayerState.Paused;
    }

    // Stop the background music.
    public void StopBGM()
    {
        bgmSource.Stop();
        bgmState = SoundPlayerState.Stopped;
    }

    // Set the global volume of the BGM.
    public void SetBGMVolume(float newVolume)
    {
        // Set the new bgm volume and ensure that it is clamped to the bgm volume boundaries.
        bgmVolume = Mathf.Clamp(newVolume, minBGMVolume, maxBGMVolume);

        // Update the volume of the BGM.
        bgmSource.volume = bgmVolume;
    }

    // Plays everything (i.e., BGM and SFX).
    public void Play()
    {
        PlayAllSFX();
        PlayBGM();
    }

    // Pauses everything (i.e., BGM and SFX).
    public void Pause()
    {
        PauseBGM();
        PauseAllSFX();
    }

    // Stops everything (i.e., BGM and SFX).
    public void Stop()
    {
        StopBGM();
        StopAllSFX();
    }
}
