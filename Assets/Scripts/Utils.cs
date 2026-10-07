using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 Random(Vector3 min, Vector3 max)
    {
        return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
    }

    public static GameObject[] FindGameObjectsWithLayer(int layer)
    {
        GameObject[] goArray = GameObject.FindObjectsByType<GameObject>();
        List<GameObject> goList = new();
        for (var i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == layer) 
            {
                goList.Add(goArray[i]); 
            }
        }
        if (goList.Count == 0) 
        { 
            return null; 
        }
            return goList.ToArray();
        }
}
