using UnityEngine;
using UnityEditor;

public class ScreenshotWindow : EditorWindow
{
    string path;
    string fileName;

    [MenuItem("Window/Screenshot/Create Window")]
    public static void ShowWindow()
    {
        GetWindow<ScreenshotWindow>("Screenshot Window");
    }

    [MenuItem("Window/Screenshot/Take Screenshot")]
    public static void TakeScreenshot()
    {
        if (PlayerPrefs.GetString("ScreenshotFileName") != "" && PlayerPrefs.GetString("ScreenshotPath") != "")
        {
            ScreenCapture.CaptureScreenshot(PlayerPrefs.GetString("ScreenshotPath") + PlayerPrefs.GetString("ScreenshotFileName"));
        }
    }


    private void OnGUI()
    {
        GUILayout.Label("Screenshot Window", EditorStyles.boldLabel);

        path = EditorGUILayout.TextField("Path", path);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (path == null || path.Equals(""))
        {
            path = PlayerPrefs.GetString("ScreenshotPath");
        }
        else
        {
            PlayerPrefs.SetString("ScreenshotPath", path);
        }

        if (fileName == null || fileName.Equals(""))
        {
            fileName = PlayerPrefs.GetString("ScreenshotFileName");
        }
        else
        {
            PlayerPrefs.SetString("ScreenshotFileName", fileName);
        }

        GUILayout.Space(20f);
        if (GUILayout.Button("Take Screenshot"))
        {
            TakeScreenshot();
        }
    }
}
