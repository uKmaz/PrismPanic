using UnityEngine;

namespace PrismPanic.ScriptableObjects
{
    /// <summary>
    /// Handcrafted level layout. Each asset = one map configuration.
    /// Defines exact positions for walls, mirrors, pillars, enemies, doors.
    /// NO procedural generation — everything is authored.
    /// </summary>
    [CreateAssetMenu(menuName = "PrismPanic/LevelLayout")]
    public class LevelLayoutSO : ScriptableObject
    {
        public string layoutID;

        [Header("Room Geometry")]
        [Tooltip("Positions for individual wall cubes")]
        public Vector3[] wallPositions;

        [Tooltip("Positions for floor tiles (or use a single scaled plane)")]
        public Vector3[] floorPositions;

        [Header("Interactables")]
        public MirrorPlacement[] mirrorPlacements;
        public Vector3[] pillarPositions;

        [Header("Spawns")]
        public Vector3 playerSpawnPoint;
        public Vector3[] enemySpawnPoints;

        [Tooltip("Exactly 3 door positions")]
        public Vector3[] doorSpawnPoints;

        [Header("Waves")]
        public EnemyWaveData[] waves;

        [Header("Boss Setup")]
        public bool hasBoss;
        public Vector3 bossSpawnPoint;
    }

    [System.Serializable]
    public struct MirrorPlacement
    {
        public Vector3 position;
        [Tooltip("Rotation around Y axis in degrees")]
        public float rotationY;
    }

    [System.Serializable]
    public struct EnemyWaveData
    {
        public EnemyDataSO enemyData;
        public int count;
        [Tooltip("If true, spawns from the ShadowAngel pool (invisible in darkness)")]
        public bool isShadowAngel;
    }
}
