using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spawnable Entity Library")]
public class SpawnableEntityLibrary : ScriptableObject
{
    [SerializeField]
    private GameObject[] spawnableEntities;

    public GameObject GetEntity(uint id)
    {
        return spawnableEntities[id];
    }
}
