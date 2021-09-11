using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SerializeSceneColliders : MonoBehaviour
{
    public const string DIRECTORY = "RoomColliderData";

    [MenuItem("AmpUtil/Serialize Colliders")]
    static void SerializeColliders()
    {
        BoxCollider2D[] cols = FindObjectsOfType<BoxCollider2D>();
        int linesWritten = 0;
        Scene scene = SceneManager.GetActiveScene();
        string dirpath = Application.persistentDataPath + '/' + DIRECTORY;
        if (!Directory.Exists(dirpath))
            Directory.CreateDirectory(dirpath);
        string path = dirpath + '/' + scene.name + "-index" + scene.buildIndex + ".collision";
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(FindObjectOfType<ServerInstanceInfo>().GetRoomId());
            for (int i = 0; i < cols.Length; i++)
            {
                //Only static colliders should be serialized.
                if (cols[i].gameObject.isStatic)
                {
                    Vector2 offset = (Vector2)cols[i].transform.position + cols[i].offset;
                    sw.WriteLine($"{cols[i].gameObject.name}|{(int)(offset.x * Game.PixelsPerUnit)}|{(int)(offset.y * Game.PixelsPerUnit)}|{(int)(cols[i].size.x * Game.PixelsPerUnit)}|{(int)(cols[i].size.y * Game.PixelsPerUnit)}");
                    linesWritten++;
                }
            }
        }
        Debug.Log($"Wrote {linesWritten} lines to {path}");
    }
}
