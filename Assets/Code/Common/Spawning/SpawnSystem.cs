using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace PQ.Common.Spawning
{
    /* Direction used to align a spawned entity to the nearest surface after resolving overlaps. */
    public enum SnapDirection { None, Up, Down, Left, Right }

    /* Options for resolving collisions at spawn time to avoid the entity visibly "dropping in". */
    public readonly struct SpawnCollisionOptions
    {
        public readonly SnapDirection SnapDirection;
        public readonly LayerMask     CollisionMask;
        public readonly int           MaxIterations;

        /*
        Resolve overlaps and optionally snap to nearest surface in given direction.
        Pass collisionMask=0 to use Physics2D.DefaultRaycastLayers.
        */
        public SpawnCollisionOptions(SnapDirection snapDirection, LayerMask collisionMask = default, int maxIterations = 8)
        {
            SnapDirection = snapDirection;
            CollisionMask = collisionMask;
            MaxIterations = maxIterations;
        }
    }

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
        public GameObject Spawn(GameObject prefab, string tag, SpawnCollisionOptions? collisionOptions = null)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(tag);
            if (spawnPoints.Length == 0)
                throw new MissingReferenceException($"Cannot spawn - no GameObject found with tag '{tag}'");

            Transform spawnPoint = spawnPoints[0].transform;
            return SpawnAt(prefab, spawnPoint.position, spawnPoint.rotation, collisionOptions);
        }

        /* Spawn a prefab at the specified position and rotation, using pooling when available. */
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, SpawnCollisionOptions? collisionOptions = null)
        {
            return SpawnAt(prefab, position, rotation, collisionOptions);
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


        private GameObject SpawnAt(GameObject prefab, Vector3 position, Quaternion rotation, SpawnCollisionOptions? collisionOptions)
        {
            if (prefab == null)
                throw new NullReferenceException("Prefab to spawn cannot be null");

            GameObject instance = GetFromPool(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);
            if (collisionOptions.HasValue)
                ResolveSpawnCollisions(instance, collisionOptions.Value);
            return instance;
        }

        /*
        Resolve spawn-time collisions in two phases:
        1. De-penetrate any overlaps using minimum separation (shortest path out of geometry).
        2. If a snap direction is given, cast the shape in that direction and align flush to the nearest surface.

        This prevents the "drop in" effect where an entity spawns slightly inside or above geometry
        and then visibly pops/slides into place on the first physics frame.

        Falls back to sprite bounds when no Collider2D is present. In that case only the directional
        snap is applied — iterative overlap resolution requires a collider and is skipped.
        */
        private static void ResolveSpawnCollisions(GameObject instance, SpawnCollisionOptions options)
        {
            LayerMask mask = options.CollisionMask == 0 ? Physics2D.DefaultRaycastLayers : options.CollisionMask;
            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask    = mask;
            filter.useTriggers  = false;

            // Ensure physics engine sees the freshly-set transform position.
            // Required because Physics2D.autoSyncTransforms is disabled in this project.
            Physics2D.SyncTransforms();

            if (instance.TryGetComponent(out Collider2D collider))
            {
                Vector2 escapeDir = options.SnapDirection != SnapDirection.None ? -ToVector(options.SnapDirection) : Vector2.zero;
                ResolveOverlaps(collider, filter, options.MaxIterations, escapeDir);
                if (options.SnapDirection != SnapDirection.None)
                    SnapToSurface(collider, filter, ToVector(options.SnapDirection));
            }
            else if (options.SnapDirection != SnapDirection.None &&
                     instance.TryGetComponent(out SpriteRenderer sprite))
            {
                SnapToSurface(sprite, filter, ToVector(options.SnapDirection));
            }
        }

        /*
        Iteratively push the collider out of any overlapping geometry using minimum separation.
        Handles the case where the spawn point is placed inside a collider.

        When escapeDirection is non-zero, guards against ambiguous normals from one-sided colliders
        (EdgeCollider2D) by reflecting any resolution component that points against the escape axis.
        This preserves lateral correction while ensuring the entity isn't pushed through the surface.
        */
        private static void ResolveOverlaps(Collider2D collider, ContactFilter2D filter, int maxIterations, Vector2 escapeDirection)
        {
            Collider2D[] overlaps = new Collider2D[8];
            for (int iter = 0; iter < maxIterations; iter++)
            {
                int count = collider.Overlap(filter, overlaps);
                if (count == 0)
                    break;

                bool anyResolved = false;
                for (int i = 0; i < count; i++)
                {
                    ColliderDistance2D dist = collider.Distance(overlaps[i]);
                    if (!dist.isValid || !dist.isOverlapped)
                        continue;

                    Vector2 resolve = dist.distance * dist.normal;
                    if (escapeDirection != Vector2.zero)
                    {
                        float escapeComponent = Vector2.Dot(resolve, escapeDirection);
                        if (escapeComponent < 0f)
                            resolve -= 2f * escapeComponent * escapeDirection;
                    }
                    collider.transform.position += (Vector3)resolve;
                    anyResolved = true;
                }
                if (!anyResolved)
                    break;

                Physics2D.SyncTransforms();
            }
        }

        /*
        Cast the collider in the snap direction and move flush to the nearest surface found.
        Only moves if a surface exists at a positive distance (no-op if already touching or nothing found).
        */
        private static void SnapToSurface(Collider2D collider, ContactFilter2D filter, Vector2 direction)
        {
            Physics2D.SyncTransforms();
            RaycastHit2D[] hits = new RaycastHit2D[1];
            int hitCount = collider.Cast(direction, filter, hits, Mathf.Infinity, ignoreSiblingColliders: true);
            if (hitCount > 0 && hits[0].distance > 0f)
                collider.transform.position += (Vector3)(direction * hits[0].distance);
        }

        /*
        Sprite bounds fallback for snap when no Collider2D is present.
        Uses SpriteRenderer.bounds (world-space, updates with transform) for the BoxCast.
        Overlap depenetration is not supported on this path.
        */
        private static void SnapToSurface(SpriteRenderer sprite, ContactFilter2D filter, Vector2 direction)
        {
            Bounds b = sprite.bounds;
            RaycastHit2D[] hits = new RaycastHit2D[1];
            int hitCount = Physics2D.BoxCast(b.center, b.size, 0f, direction, filter, hits, Mathf.Infinity);
            if (hitCount > 0 && hits[0].distance > 0f)
                sprite.transform.position += (Vector3)(direction * hits[0].distance);
        }

        private static Vector2 ToVector(SnapDirection direction) => direction switch
        {
            SnapDirection.Up    => Vector2.up,
            SnapDirection.Down  => Vector2.down,
            SnapDirection.Left  => Vector2.left,
            SnapDirection.Right => Vector2.right,
            _                   => Vector2.zero,
        };

        private GameObject GetFromPool(GameObject prefab)
        {
            if (_pools.ContainsKey(prefab) && _pools[prefab].Count > 0)
                return _pools[prefab].Dequeue();

            return Object.Instantiate(prefab);
        }
    }
}
