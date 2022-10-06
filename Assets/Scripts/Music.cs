using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour {
    public Object[] BGM;
    public Object[] Effect;
    private AudioSource bgmAudioSource;
    private AudioSource scoreAudioSource;
    private bool tryingChange = false;
    public static int nextMusic;

    void Awake() {
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.volume = 0.1f;
        scoreAudioSource = gameObject.AddComponent<AudioSource>();
        DontDestroyOnLoad(this); // to no restart music on a new game
        randomInitialization();
        bgmAudioSource.Play();
    }

    // get the first music by random number
    void randomInitialization() {
        nextMusic = Random.Range(0, BGM.Length);
        bgmAudioSource.clip = BGM[nextMusic] as AudioClip;
    }

    // select next music and increment nextMusic by circular reference
    void selectScorePointMusic(){
        scoreAudioSource.clip = Effect[1] as AudioClip;
    }

    // Create a circular reference: 0 1 2 0 1 2 0 1 2 ...
    void nextCircularPlaylist() {
        nextMusic = Utils.Mod(nextMusic + 1, BGM.Length);
    }

    // Select the next music and play it
    public void playScorePointMusic() {
        selectScorePointMusic();
        scoreAudioSource.Play();
    }

    public void playOpenMusic()
    {
        randomInitialization();
        bgmAudioSource.Play();
    }

    public void pauseMusic()
    {
        bgmAudioSource.Pause();
    }

    public void unpauseMusic()
    {
        bgmAudioSource.UnPause();
    }

    void selectGameOverMusic()
    {
        bgmAudioSource.clip = Effect[0] as AudioClip;
    }

    public void playGameOverMusic()
    {
        selectGameOverMusic();
        bgmAudioSource.Play();
    }

    public void stopMusic()
    {
        bgmAudioSource.Stop();
    }
   
     //this avoid the behavior of start a new music
     //when the unity stop by desfocusing
     //So we wait for 1 second before change for new music
     //after isPLaying is false
    IEnumerator tryChange()
    {
        tryingChange = true;
        yield return new WaitForSeconds(1);
        if (!bgmAudioSource.isPlaying)
        {
            randomInitialization();
            bgmAudioSource.Play();
        }
        tryingChange = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!tryingChange && !bgmAudioSource.isPlaying)
        {
            StartCoroutine(tryChange());
        }
    }
}
