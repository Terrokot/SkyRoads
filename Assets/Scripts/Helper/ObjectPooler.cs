using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int amountToPool;

    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;


}