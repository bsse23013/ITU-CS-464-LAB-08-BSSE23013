using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

// Menu item  Lab07 > Build Menu Scene : builds the Canvas and saves Assets/Scenes/MenuSystem.unity
// (Lab 08: also adds the AudioListener and the Audio object. Build the audio mixer and the Gameplay scene first, or use Lab08 > Build Everything.)
public static class Lab07Menu
{
    const string ScenePath = "Assets/Scenes/MenuSystem.unity";

    [MenuItem("Lab07/Build Menu Scene")]
    public static void BuildScene()
    {
        AssetDatabase.ImportAsset("Assets/Lab07/Resources/Lab07/Sprites", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        MakeFontAsset("LilitaOne-Regular");
        MakeFontAsset("Poppins-Regular");
        MakeFontAsset("Poppins-SemiBold");

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.BgDeep;
        camGo.AddComponent<AudioListener>();      // the one listener for the whole game (Lab 08)

        MenuBuilder.Build();

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/Gameplay.unity", true)
        };
        Debug.Log("[Lab07] Menu scene saved to " + ScenePath);
    }

    // TextMeshPro needs a font asset for each font (same as Window > TextMeshPro > Font Asset Creator)
    static void MakeFontAsset(string fontName)
    {
        string dir = "Assets/Lab07/Resources/Lab07/Fonts/";
        string assetPath = dir + fontName + " SDF.asset";
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null) return;

        Font font = AssetDatabase.LoadAssetAtPath<Font>(dir + fontName + ".ttf");
        TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        fa.name = fontName + " SDF";
        AssetDatabase.CreateAsset(fa, assetPath);
        fa.material.name = fontName + " Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
        foreach (Texture2D tex in fa.atlasTextures) { tex.name = fontName + " Atlas"; AssetDatabase.AddObjectToAsset(tex, fa); }
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
    }
}
