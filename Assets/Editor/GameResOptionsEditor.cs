using UnityEditor;
using UnityEngine;

public class GameResOptionsEditor : EditorWindow
{
    private static GameResOptionsEditor Instance;

    [MenuItem("GameRes/GameRes Options", false, 100)]
    private static void Init()
    {
        if (Instance == null)
        {
            Instance = GameResOptionsEditor.GetWindow<GameResOptionsEditor>(true, "GameRes Options");
            Instance.minSize = new Vector2(440, 285);
            Instance.Show();
        }
        else
        {
            Instance.Close();
            Instance = null;
        }
    }

    private string _gameMainPath;
    private string _arenaPath;
    private string _uiTestPath;

    private void OnEnable()
    {
        _gameMainPath = PlayerPrefs.GetString("GameMainScene", "Assets/Scenes/GameMain.unity");
        _arenaPath = PlayerPrefs.GetString("ArenaTestScene", "Assets/Scenes/ArenaTest3D.unity");
        _uiTestPath = PlayerPrefs.GetString("UITestScene", "Assets/Scenes/UITest.unity");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("== ScenePath ==");
        _gameMainPath = EditorGUILayout.TextField("GameMain:", _gameMainPath);
        _arenaPath = EditorGUILayout.TextField("ArenaTest:", _arenaPath);
        _uiTestPath = EditorGUILayout.TextField("UITest:", _uiTestPath);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("保存配置", "LargeButton", GUILayout.Height(50f)))
        {
            PlayerPrefs.SetString("GameMainScene", _gameMainPath);
            PlayerPrefs.SetString("ArenaTestScene", _arenaPath);
            PlayerPrefs.SetString("UITestScene", _uiTestPath);
            Debug.Log("Save GameRes Options!");
        }
        EditorGUILayout.EndHorizontal();
    }
}