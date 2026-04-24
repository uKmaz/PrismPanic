#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using PrismPanic.ScriptableObjects;
using System.Collections.Generic;

namespace PrismPanic.Editor
{
    /// <summary>
    /// Editor utility to auto-generate LevelLayoutSO data.
    /// Use from menu: PrismPanic → Generate Map1
    /// </summary>
    public static class LevelLayoutGenerator
    {
        [MenuItem("PrismPanic/Generate Map1 (Tutorial)")]
        public static void GenerateMap1()
        {
            // Find or create the asset
            string path = "Assets/_Game/Scripts/ScriptableObjects/LevelLayouts/Map1.asset";
            LevelLayoutSO layout = AssetDatabase.LoadAssetAtPath<LevelLayoutSO>(path);

            if (layout == null)
            {
                layout = ScriptableObject.CreateInstance<LevelLayoutSO>();
                // Ensure folder exists
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Scripts/ScriptableObjects/LevelLayouts"))
                {
                    AssetDatabase.CreateFolder("Assets/_Game/Scripts/ScriptableObjects", "LevelLayouts");
                }
                AssetDatabase.CreateAsset(layout, path);
            }

            layout.layoutID = "map1_tutorial";

            // --- WALLS (perimeter of 10x10 room) ---
            // Room: X from -5 to 5, Z from -5 to 5
            // Wall cube = 1x3x1, center at Y=1.5
            List<Vector3> walls = new List<Vector3>();

            // Bottom wall (Z = -5)
            for (int x = -5; x <= 5; x++)
                walls.Add(new Vector3(x, 1.5f, -5));

            // Top wall (Z = 5)
            for (int x = -5; x <= 5; x++)
                walls.Add(new Vector3(x, 1.5f, 5));

            // Left wall (X = -5), skip corners
            for (int z = -4; z <= 4; z++)
                walls.Add(new Vector3(-5, 1.5f, z));

            // Right wall (X = 5), skip corners
            for (int z = -4; z <= 4; z++)
                walls.Add(new Vector3(5, 1.5f, z));

            layout.wallPositions = walls.ToArray();

            // --- FLOOR (10x10 grid inside walls) ---
            List<Vector3> floors = new List<Vector3>();
            for (int x = -4; x <= 4; x++)
            {
                for (int z = -4; z <= 4; z++)
                {
                    floors.Add(new Vector3(x, 0f, z));
                }
            }
            // Fill under walls too for seamless look
            for (int x = -5; x <= 5; x++)
            {
                floors.Add(new Vector3(x, 0f, -5));
                floors.Add(new Vector3(x, 0f, 5));
            }
            for (int z = -4; z <= 4; z++)
            {
                floors.Add(new Vector3(-5, 0f, z));
                floors.Add(new Vector3(5, 0f, z));
            }
            layout.floorPositions = floors.ToArray();

            // --- MIRRORS (2 mirrors for easy 1-bounce path) ---
            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement
                {
                    position = new Vector3(2f, 1f, 0f),
                    rotationY = 45f  // angled to bounce beam from player toward enemy area
                },
                new MirrorPlacement
                {
                    position = new Vector3(-2f, 1f, 3f),
                    rotationY = -30f  // second bounce option
                }
            };

            // --- PILLARS (1 pillar for cover) ---
            layout.pillarPositions = new Vector3[]
            {
                new Vector3(0f, 1.5f, -2f)
            };

            // --- PLAYER SPAWN ---
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -3f);

            // --- ENEMY SPAWNS (2 angels) ---
            layout.enemySpawnPoints = new Vector3[]
            {
                new Vector3(-3f, 0.5f, 3f),
                new Vector3(3f, 0.5f, 3f)
            };

            // --- DOOR SPAWNS (3 doors along top wall) ---
            layout.doorSpawnPoints = new Vector3[]
            {
                new Vector3(0f, 1.5f, 4f),
                new Vector3(-3f, 1.5f, 4f),
                new Vector3(3f, 1.5f, 4f)
            };

            // --- WAVES ---
            // You need to assign the EnemyDataSO reference manually after generation
            // because we can't guarantee the BaseAngel asset exists yet
            EnemyDataSO baseAngel = FindEnemyData("BaseAngel");
            layout.waves = new EnemyWaveData[]
            {
                new EnemyWaveData
                {
                    enemyData = baseAngel, // may be null — assign manually if so
                    count = 2
                }
            };

            EditorUtility.SetDirty(layout);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PrismPanic] Map1 generated! Walls: {layout.wallPositions.Length}, " +
                      $"Floors: {layout.floorPositions.Length}, Mirrors: {layout.mirrorPlacements.Length}");

            if (baseAngel == null)
            {
                Debug.LogWarning("[PrismPanic] BaseAngel EnemyDataSO not found. " +
                                 "Create it at ScriptableObjects/EnemyData/BaseAngel and assign manually in Map1 waves.");
            }

            // Select it in inspector
            Selection.activeObject = layout;
        }

        private static EnemyDataSO FindEnemyData(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"t:EnemyDataSO {name}");
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<EnemyDataSO>(assetPath);
            }
            return null;
        }
    }
}
#endif
