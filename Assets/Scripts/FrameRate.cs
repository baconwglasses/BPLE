using System;
using UnityEngine;

public class FrameRate : MonoBehaviour
{
    private static readonly int DefaultFramerate = -1; // Unlimited
    private static readonly int DefaultVsyncState = 1; // Every VBlank
    private static readonly string[] Args = Environment.GetCommandLineArgs();

    //fixed bugs in this method
    private void Awake()
    {
        /*_ <- */Console.WriteLine("CLI Arguments:" + string.Join("\n", Args));
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