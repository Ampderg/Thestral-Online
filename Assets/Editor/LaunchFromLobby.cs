using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LaunchFromLobby : MonoBehaviour
{
    [MenuItem("AmpUtil/Play From Title Screen %l")]
    public static void PlayFromPrelaunchScene()
    {
        //#if UNITY_EDITOR

        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        EditorApplication.isPlaying = true;
        //#endif
    }
}
