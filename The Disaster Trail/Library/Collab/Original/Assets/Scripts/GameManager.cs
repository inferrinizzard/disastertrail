using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Scenes
{
	MainMenu = 0,
	House,
	Overworld,
	FloodGame,
	Hurricane,
	WinterStormGame,
	Shop = 10
}

public class GameManager : MonoBehaviour
{
	// Allows other scripts to call functions from GameManager (part of our singleton pattern).
	public static GameManager instance = null;

	//Tracks which mode the player is playing
	public bool storyMode = true;

	//Order of levels 
	private int[] levels = { 0, 1, 2, 3, 2, 4, 2, 5, 2 };

	//Current level
	private int levelPointer = 0;

	//Tracks active scene through levels
	public int currentScene = 0;

	//Tracks previous scene 
	private int previousScene = 0;

	//Tracks postion in trail array
	private int trailPointer = 0;


	// The inventory stats class that manages global inventory for the player
	public InventoryStats inventoryStats = new InventoryStats();

	#region Fading Logic
	// Our FadeEventCallback, implement a function with this signature and pass to fade function to receive fade complete callbacks
	public delegate void FadeEventCallback();
	// The image that fades over the entire screen for transitions
	[SerializeField] private Image transitionImage;
	// The speed that the image fades for over the entire screen for transitions
	public float FadeTransitionSpeedPerFrame;
	// The speed that the image fades for over the entire screen for transitions
	// Positive fades in, negative fades out
	private float fadeAmount;
	// Are we fading in?
	[SerializeField] private bool fadeIn;
	// Are we fading out?
	private bool fadeOut;
	// The callback function when a fade is complete
	private FadeEventCallback fadeComplete;
	// The scene to load after a fade out is complete
	private Scenes sceneToLoad;
	private int step = 0;
	#endregion

	void Awake()
	{
		// Check if there is already an instance of GameManager.
		if (instance == null)
		{
			// If not, set it to this.
			instance = this;
		}
		// If another instance already exists then...
		else if (instance != this)
		{
			// Destroy this, this enforces our singleton pattern so there can only be one instance of GameManager.
			Destroy(gameObject);
		}

		// Set GameManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
	}

	void Update()
	{
		// Allow quitting the game with escape!
		if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

		// Transition fade in logic
		if (fadeIn)
		{
			float newAlpha = transitionImage.color.a - fadeAmount;
			Debug.Log(transitionImage.color.a + " - " + fadeAmount + " = " + newAlpha + " {Before}");
			transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, Mathf.Max(0, newAlpha));
			// Check to see if we are done fading in
			if (transitionImage.color.a == 0)
			{
				fadeIn = false;
				transitionImage.gameObject.SetActive(false);
				// Handle callback if applicable
				fadeComplete?.Invoke();
			}
			Debug.Log(transitionImage.color.a);
		}
		// Transition fade out logic
		if (fadeOut)
		{
			float newAlpha = transitionImage.color.a + fadeAmount * ++step;
			Debug.Log(transitionImage.color.a + " + " + fadeAmount + " = " + newAlpha + " {Before}");
			transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, Mathf.Min(1, newAlpha));

			// Check to see if we are done fading in
			if (transitionImage.color.a == 1)
			{
				step =0;
				fadeOut = false;
				// Handle callback if applicable
				fadeComplete?.Invoke();
			}
			Debug.Log(transitionImage.color.a);
		}

		if (previousScene != currentScene)
		{
			previousScene = currentScene;
			FadeFromBlack(.1f);
			if (storyMode)
			{
				levelPointer++;
				trailPointer = levels[levelPointer] == 2 ? trailPointer++ : trailPointer;

			}

		}

	}


	//sets new loaded Scene by name, returns true if completed properly
	public bool TrackScene(string scene)
	{
		//Check if string is valid enum constant
		// and sets current scene to value of that constant
		currentScene = Scenes.IsDefined(typeof(Scenes), scene) ? (int)Scenes.Parse(typeof(Scenes), scene, true) : currentScene;
		return Scenes.IsDefined(typeof(Scenes), scene);
	}
	//sets new loaded Scene by index, returns true if completed properly
	public bool TrackScene(int scene)
	{
		//Check if int is valid enum value
		// and sets current scene to that value
		currentScene = Scenes.IsDefined(typeof(Scenes), scene) ? scene : currentScene;
		return Scenes.IsDefined(typeof(Scenes), scene);
	}

	//sets trail for overworld car
	public int getNextTrail()
	{
		return trailPointer;
	}

	public int getLevel()
	{
		return levels[levelPointer];
	}

	#region Scene Loading Functionality
	// Loads a specified scene after a specified amount of time
	public void LoadScene(string scene, float waitToLoadSeconds = 0)
	{
		StartCoroutine(loadSceneAfterSeconds(scene, waitToLoadSeconds));
	}

	public void LoadScene(int scene, float waitToLoadSeconds = 0)
	{
		StartCoroutine(loadSceneAfterSeconds(scene, waitToLoadSeconds));
	}

	public void LoadScene(Scenes scene, float waitToLoadSeconds = 0)
	{
		LoadScene((int)scene, waitToLoadSeconds);
	}

	private IEnumerator loadSceneAfterSeconds(string scene, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		SceneManager.LoadScene(scene);
		TrackScene(scene);
	}

	private IEnumerator loadSceneAfterSeconds(int scene, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		SceneManager.LoadScene(scene);
		TrackScene(scene);
	}

	// For internal scene loading functionality only
	private void loadStoredScene()
	{

		LoadScene(sceneToLoad);
	}
	#endregion

	#region Transition Fade Functionality
	// Fade in from white helper
	public void FadeFromWhite(float fadeAmountPerFrame, FadeEventCallback fadeCompleteCallback = null)
	{
		FadeFrom(Color.white, fadeAmountPerFrame, fadeCompleteCallback);
	}

	// Fade in from black helper
	public void FadeFromBlack(float fadeAmountPerFrame, FadeEventCallback fadeCompleteCallback = null)
	{
		FadeFrom(Color.black, fadeAmountPerFrame, fadeCompleteCallback);
	}

	// Fade in from any color at specified speed with callback when complete
	// Note: can fade in from partially transparent color as well
	public void FadeFrom(Color transitionColor, float fadeAmountPerFrame, FadeEventCallback fadeCompleteCallback = null)
	{
		// If we are already fading out then stop that to fade in
		if (fadeOut)
		{
			fadeOut = false;
		}

		// Setup fade in parameters
		transitionImage.gameObject.SetActive(true);
		transitionImage.color = transitionColor;
		fadeAmount = fadeAmountPerFrame;
		fadeComplete = fadeCompleteCallback;
		fadeIn = true;
	}

	// Fade in out to white helper
	public void FadeToWhite(float fadeAmountPerFrame, FadeEventCallback fadeCompleteCallback = null)
	{
		FadeTo(Color.white, fadeAmountPerFrame, fadeCompleteCallback);
	}

	// Fade out to black helper
	public void FadeToBlack(float fadeAmountPerFrame, FadeEventCallback fadeCompleteCallback = null)
	{
		FadeTo(Color.black, fadeAmountPerFrame, fadeCompleteCallback);
	}

	// Fade out to any color at specified speed with callback when complete
	// Note: can fade in from partially transparent color as well
	public void FadeTo(Color transitionColor, float fadeAmountPerFrame, FadeEventCallback fadeCompleteCallback = null)
	{
		// If we are already fading in then stop that to fade out
		if (fadeIn)
		{
			fadeIn = false;
		}

		// Setup fade out parameters
		transitionImage.gameObject.SetActive(true);
		transitionImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0);
		fadeAmount = fadeAmountPerFrame;
		fadeComplete = fadeCompleteCallback;
		fadeOut = true;
	}

	// Fade out to black and load helpers
	public void FadeToBlackAndLoad(float fadeAmountPerFrame, Scenes scene)
	{
		FadeToAndLoad(Color.black, fadeAmountPerFrame, scene);
	}

	public void FadeToBlackAndLoad(float fadeAmountPerFrame, int scene)
	{
		FadeToAndLoad(Color.black, fadeAmountPerFrame, scene);
	}

	// Fade out to white and load helpers
	public void FadeToWhiteAndLoad(float fadeAmountPerFrame, Scenes scene)
	{
		FadeToAndLoad(Color.white, fadeAmountPerFrame, scene);
	}

	public void FadeToWhiteAndLoad(float fadeAmountPerFrame, int scene)
	{
		FadeToAndLoad(Color.white, fadeAmountPerFrame, scene);
	}

	// Fade out to any color at specified speed and load specified scene when complete
	public void FadeToAndLoad(Color transitionColor, float fadeAmountPerFrame, Scenes scene)
	{
		sceneToLoad = scene;
		FadeTo(transitionColor, fadeAmountPerFrame, loadStoredScene);

	}

	public void FadeToAndLoad(Color transitionColor, float fadeAmountPerFrame, int scene)
	{
		FadeToAndLoad(transitionColor, fadeAmountPerFrame, (Scenes)scene);
	}


	#endregion
}