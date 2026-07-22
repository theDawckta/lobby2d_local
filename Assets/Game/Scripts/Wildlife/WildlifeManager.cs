using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Wildlife
{
    [Serializable]
    public class WildlifeSpawnEntry
    {
        public GameObject prefab;
        public int count = 1;
    }

    // Spawns and manages wildlife (deer, rabbits, birds, etc.) within a bounded area of the
    // lobby. Each spawned instance gets a WildlifeAgent driving wander/flee behaviour; if the
    // spawned prefab also carries a GlbCharacterAnimator (the CoreSystems 3D-creature runtime
    // component, wired onto the prefab by a separate "Apply <creature>" design ticket per the
    // prefab-split convention), its locomotion animation is driven automatically as a side effect
    // of WildlifeAgent calling SetSpeed().
    //
    // Player tracking is decoupled from any specific multiplayer transport: RegisterPlayer /
    // UnregisterPlayer are called externally (e.g. from the local player controller, and from a
    // WorldPresence.OnPlayerJoined/OnPlayerLeft hookup in a future Main Scene assembly ticket), so
    // this component has no network dependency of its own and is fully testable offline.
    public class WildlifeManager : MonoBehaviour
    {
        [SerializeField] private List<WildlifeSpawnEntry> spawnEntries = new();
        [SerializeField] private Vector3 areaCenter = Vector3.zero;
        [SerializeField] private Vector3 areaSize = new(20f, 0f, 20f);
        [SerializeField] private bool spawnOnStart = true;

        private readonly List<WildlifeAgent> _spawned = new();
        private readonly List<Transform> _players = new();

        public IReadOnlyList<WildlifeAgent> SpawnedWildlife => _spawned;
        public IReadOnlyList<Transform> Players => _players;
        public Vector3 AreaCenter { get => areaCenter; set => areaCenter = value; }
        public Vector3 AreaSize { get => areaSize; set => areaSize = value; }
        public List<WildlifeSpawnEntry> SpawnEntries => spawnEntries;
        public bool SpawnOnStart { get => spawnOnStart; set => spawnOnStart = value; }

        private void Start()
        {
            if (spawnOnStart) SpawnAll();
        }

        public void SpawnAll()
        {
            foreach (var entry in spawnEntries)
            {
                if (entry?.prefab == null) continue;
                for (var i = 0; i < entry.count; i++)
                {
                    SpawnOne(entry.prefab);
                }
            }
        }

        public WildlifeAgent SpawnOne(GameObject prefab)
        {
            if (prefab == null) return null;

            var spawnPosition = WildlifeMovement.PickWanderTarget(areaCenter, areaSize);
            var instance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            var agent = instance.GetComponent<WildlifeAgent>();
            if (agent == null) agent = instance.AddComponent<WildlifeAgent>();
            agent.Initialize(areaCenter, areaSize, _players);
            _spawned.Add(agent);
            return agent;
        }

        public void RegisterPlayer(Transform player)
        {
            if (player == null || _players.Contains(player)) return;
            _players.Add(player);
        }

        public void UnregisterPlayer(Transform player)
        {
            _players.Remove(player);
        }
    }
}
