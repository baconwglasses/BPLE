using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class INInitializer : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_splashes;

	[SerializeField]
	private List<GameObject> m_prefabs;

	[SerializeField]
	private ResourceData m_resourceData;

	private bool m_initialized;

	private bool m_useAlphaAnimation;

	private float m_time;
	private float AnimationInTime = 0f;
	private float AnimationOutTime = 0.5f;

	public bool Initialized => m_initialized;
	
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(System.IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
#endif

	private void Awake()
	{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        bool systemIsDark;

        try
        {
            object value = Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", 1);
            systemIsDark = value is int i && i == 0;
        }
        catch
        {
            systemIsDark = false;
        }

        if (systemIsDark)
		{
			System.IntPtr hwnd = GetActiveWindow();
			int useDark = 1;

			if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int)) != 0)
			{
	            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref useDark, sizeof(int));
	        }
		}
#endif

		m_useAlphaAnimation = true;
		m_time = 15f;
		
		Application.targetFrameRate = FrameRate.GetTargetFPS();
		QualitySettings.vSyncCount = FrameRate.VsyncState();
		StartCoroutine(Initialize());
	}

	private IEnumerator Initialize()
	{
		for (int i = 0; i < m_splashes.Count; i++)
		{
			GameObject splash = Object.Instantiate(m_splashes[i], Vector3.zero, Quaternion.identity);
			yield return PlayAnimation(splash);
			Object.Destroy(splash);
		}
		INUnity.Initialize(m_resourceData);
		foreach (GameObject prefab in m_prefabs)
		{
			Object.Instantiate(prefab);
		}
		while (!INSettings.VersionSelected)
		{
			yield return null;
		}
		m_initialized = true;
		yield return LoadMainMenu();
	}

	private IEnumerator LoadMainMenu()
	{
		while (!SingletonSpawner.SpawnDone)
		{
			yield return null;
		}
		while (!Bundle.initialized || Bundle.checkingBundles || !Singleton<GameConfigurationManager>.Instance.HasData)
		{
			yield return null;
		}
		PostInitialize();
		Singleton<GameManager>.Instance.LoadMainMenu(showLoadingScreen: true);
	}

	private void PostInitialize()
	{
		if (INSettings.GetBool(INFeature.RuntimeGameData))
		{
			Object.Instantiate(INUnity.LoadGameObject("INRuntimeGameData"));
		}
		if (INSettings.GetBool(INFeature.ApplicationInterface))
		{
			Object.Instantiate(INUnity.LoadGameObject("INApplicationInterface"));
		}
	}

	private IEnumerator PlayAnimation(GameObject gameObject)
	{
		if (!m_useAlphaAnimation)
		{
			yield return new WaitForSeconds(m_time);
			yield break;
		}
		CanvasRenderer canvasRenderer = gameObject.GetComponentInChildren<CanvasRenderer>();
		if (canvasRenderer != null)
		{
			yield return canvasRenderer.PlayFadeInAnimation(AnimationInTime);
			yield return new WaitForSeconds(m_time / 3f);
			yield return canvasRenderer.PlayFadeOutAnimation(AnimationOutTime);
		}
	}
}

public class FrameRate
{
	private static readonly int DefaultFramerate = -1; // Unlimited
	private static readonly int DefaultVsyncState = 1; // Every VBlank
	private static readonly string[] Args = Environment.GetCommandLineArgs();

	private void Awake()
	{
		Console.WriteLine("CLI Arguments: {0}", Args);
	}

	public static int GetTargetFPS()
	{

		for (int i = 0; i < Args.Length - 1; i++)
		{
			if (Args[i] == "-fps" && int.TryParse(Args[i + 1], out int fps))
			{
				Debug.Log("Detected -fps argument!");
				try
				{
					return fps;
				}
				catch (Exception)
				{
					string message = String.Format("Passed -fps value isn't a valid integer! Defaulting to {0} FPS.",
						DefaultFramerate);
					Debug.LogWarning(message);
					Console.WriteLine(message);
				}
			}
		}
        
		return DefaultFramerate;
	}

	public static int VsyncState()
	{
		for (int i = 0; i < Args.Length - 1; i++)
		{
			if (Args[i] == "-NoVsync")
			{
				Debug.Log("Detected -NoVsync argument!");
				Console.WriteLine("Disabled Vsync");
				return 0; 
			}
		}

		return DefaultVsyncState;
	}
}