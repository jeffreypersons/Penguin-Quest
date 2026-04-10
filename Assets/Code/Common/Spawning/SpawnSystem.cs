using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace PQ.Common.Spawning
{
    /* Spawning system with basic pooling. */
    public class SpawnSystem
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Transform _poolParent;

        public SpawnSystem()
        {
            GameObject poolContainer = new GameObject("SpawnSystem_Pool");
            _poolParent = poolContainer.transform;
            Object.DontDestroyOnLoad(poolContainer);
        }

        /* Spawn a prefab at the first GameObject with the specified tag, using pooling when available. */
        public GameObject Spawn(GameObject prefab, string tag)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(tag);
            if (spawnPoints.Length == 0)
                throw new MissingReferenceException($"Cannot spawn - no GameObject found with tag '{tag}'");

            Transform spawnPoint = spawnPoints[0].transform;
            return SpawnAt(prefab, spawnPoint.position, spawnPoint.rotation);
        }

        /* Spawn a prefab at the specified position and rotation, using pooling when available. */
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return SpawnAt(prefab, position, rotation);
        }

        /* Return an instance to the pool for reuse. */
        public void Despawn(GameObject instance, GameObject prefab)
        {
            if (instance == null)
                throw new NullReferenceException("Object to despawn cannot be null");

            instance.SetActive(false);
            instance.transform.SetParent(_poolParent);

            if (!_pools.ContainsKey(prefab))
                _pools[prefab] = new Queue<GameObject>();

            _pools[prefab].Enqueue(instance);
        }
        

        private GameObject SpawnAt(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                throw new NullReferenceException("Prefab to spawn cannot be null");
            
            GameObject instance = GetFromPool(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);
            return instance;
        }
        
        private GameObject GetFromPool(GameObject prefab)
        {
            if (_pools.ContainsKey(prefab) && _pools[prefab].Count > 0)
                return _pools[prefab].Dequeue();
            
            return Object.Instantiate(prefab);
        }
    }
}
