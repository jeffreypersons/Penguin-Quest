using UnityEngine;


namespace PQ.Common.Spawning
{
    public static class SpawnSystem
    {
        /// <summary>
        /// Spawns a prefab at the first GameObject with the specified tag.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="tag">The tag to search for spawn point</param>
        /// <returns>The instantiated GameObject</returns>
        /// <exception cref="MissingReferenceException">Thrown when no GameObject with the specified tag is found</exception>
        public static GameObject Spawn(GameObject prefab, string tag)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(tag);
            
            if (spawnPoints.Length == 0)
                throw new MissingReferenceException($"Cannot spawn - no GameObject found with tag '{tag}'");
            
            Transform spawnPoint = spawnPoints[0].transform;
            return Object.Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
