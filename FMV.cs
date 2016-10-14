using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(AudioSource))]

public class FMV : MonoBehaviour
{
	// The movie to play
	public MovieTexture movie;

	// Level loading
	public int playLevelAfter = -1;
	private AsyncOperation levelLoad;
	
	// References to own components
	private RawImage rawImg;
	private AudioSource audioSource;
	
	// Default framerate settings
	private int savedVSyncCount;
	private int savedTargetFrameRate;
	
	// Working variables
	private bool playingMovie = false;

	
	// Use this for initialization
	void Start ()
	{
		// Save default frame rate settings
		this.SaveFrameRateSettings ();
		
		// Get components
		this.rawImg = GetComponent<RawImage> ();
		this.audioSource = GetComponent<AudioSource> ();
		
		// Set up video and audio components
		this.rawImg.texture = this.movie;
		this.audioSource.clip = this.movie.audioClip;

		// Set up scene async
		if(this.playLevelAfter != -1) {
			this.levelLoad = Application.LoadLevelAsync(this.playLevelAfter);
			this.levelLoad.allowSceneActivation = false;
		}
		
		// Play the movie
		StartCoroutine (this.PlayMovie ());
	}

	// Begins movie playback
	public IEnumerator PlayMovie ()
	{
		if (!this.playingMovie) {
			Debug.Log ("Movie started");
			this.UnlockFramrate ();		// Unlock the application framerate
			this.movie.Play ();			// Play the movie
			this.audioSource.Play ();	// and the audio source
			if(PlayerPrefsExtensions.GetBool("subtitles"))
				SubtitleSystem.PlaySubtitles(this.gameObject, this.audioSource.clip);

			playingMovie = true;		// Enable flag
		} else if (this.movie.isPlaying == false) {
			Debug.LogError ("Movie is already playing!");
			yield break;
		}
		
		// Wait until the movie has finished
		while (this.movie.isPlaying) {
			yield return new WaitForEndOfFrame ();
		}
		
		this.LoadFrameRateSettings ();
		this.playingMovie = false;
		Debug.Log ("Movie finished");

		// Load the next level
		if(this.levelLoad != null) {
			this.levelLoad.allowSceneActivation = true;
		}
	}


	// -------- FRAMERATE SETTINGS -------- //
	
	// Sets the default framerate settings
	protected void SaveFrameRateSettings ()
	{
		this.savedTargetFrameRate = Application.targetFrameRate;
		this.savedVSyncCount = QualitySettings.vSyncCount;
	}
	
	
	// Loads the saved framerate settings
	protected void LoadFrameRateSettings ()
	{
		Application.targetFrameRate = this.savedTargetFrameRate;
		QualitySettings.vSyncCount = this.savedVSyncCount;
	}
	
	
	// Changes the framerate settings to a new value
	protected void UnlockFramrate ()
	{
		QualitySettings.vSyncCount = 0;		// Disable v-sync
		Application.targetFrameRate = -1;	// Set framerate to -1
	}
	
}
