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
            var layout = GetOrCreateLayout("Map1", "map1_tutorial");

            layout.layoutID = "map1_tutorial";

            // --- WALLS (perimeter of 10x10 room) ---
            // Room: X from -5 to 5, Z from -5 to 5
            // Wall cube = 1x3x1, center at Y=1.5
            List<Vector3> walls = new List<Vector3>();

            // Bottom wall (Z = -5)
            for (int x = -5; x <= 5; x++)
                walls.Add(new Vector3(x, 0f, -5));

            // Top wall (Z = 5)
            for (int x = -5; x <= 5; x++)
                walls.Add(new Vector3(x, 0f, 5));

            // Left wall (X = -5), skip corners
            for (int z = -4; z <= 4; z++)
                walls.Add(new Vector3(-5, 0f, z));

            // Right wall (X = 5), skip corners
            for (int z = -4; z <= 4; z++)
                walls.Add(new Vector3(5, 0f, z));

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

            SaveLayout(layout, "Map1");
        }

        [MenuItem("PrismPanic/Generate Map2 (Intermediate)")]
        public static void GenerateMap2()
        {
            var layout = GetOrCreateLayout("Map2", "map2_intermediate");
            var baseAngel = FindEnemyData("BaseAngel");

            // 12x12 room
            layout.wallPositions = BuildPerimeterWalls(6);
            layout.floorPositions = BuildFloorGrid(6);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(3f, 1f, 2f), rotationY = 50f },
                new MirrorPlacement { position = new Vector3(-3f, 1f, -1f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 4f), rotationY = 20f }
            };

            layout.pillarPositions = new Vector3[]
            {
                new Vector3(2f, 1.5f, -2f),
                new Vector3(-2f, 1.5f, 2f)
            };

            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);

            layout.enemySpawnPoints = new Vector3[]
            {
                new Vector3(-4f, 0.5f, 4f),
                new Vector3(4f, 0.5f, 4f),
                new Vector3(0f, 0.5f, 3f)
            };

            layout.doorSpawnPoints = new Vector3[]
            {
                new Vector3(0f, 1.5f, 5f),
                new Vector3(-3f, 1.5f, 5f),
                new Vector3(3f, 1.5f, 5f)
            };

            layout.waves = new EnemyWaveData[]
            {
                new EnemyWaveData { enemyData = baseAngel, count = 3 }
            };

            SaveLayout(layout, "Map2");
        }

        [MenuItem("PrismPanic/Generate Map3 (Advanced)")]
        public static void GenerateMap3()
        {
            var layout = GetOrCreateLayout("Map3", "map3_advanced");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel");

            // 14x12 room (wider)
            layout.wallPositions = BuildPerimeterWallsRect(7, 6);
            layout.floorPositions = BuildFloorGridRect(7, 6);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(4f, 1f, 0f), rotationY = 45f },
                new MirrorPlacement { position = new Vector3(-4f, 1f, 3f), rotationY = -60f },
                new MirrorPlacement { position = new Vector3(0f, 1f, -2f), rotationY = 30f },
                new MirrorPlacement { position = new Vector3(2f, 1f, 4f), rotationY = -45f }
            };

            layout.pillarPositions = new Vector3[]
            {
                new Vector3(3f, 1.5f, 2f),
                new Vector3(-3f, 1.5f, -1f),
                new Vector3(0f, 1.5f, 1f)
            };

            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);

            layout.enemySpawnPoints = new Vector3[]
            {
                new Vector3(-5f, 0.5f, 4f),
                new Vector3(5f, 0.5f, 4f),
                new Vector3(-2f, 0.5f, 2f),
                new Vector3(2f, 0.5f, 2f)
            };

            layout.doorSpawnPoints = new Vector3[]
            {
                new Vector3(0f, 1.5f, 5f),
                new Vector3(-4f, 1.5f, 5f),
                new Vector3(4f, 1.5f, 5f)
            };

            layout.waves = new EnemyWaveData[]
            {
                new EnemyWaveData { enemyData = baseAngel, count = 2 },
                new EnemyWaveData { enemyData = fastAngel != null ? fastAngel : baseAngel, count = 2 }
            };

            SaveLayout(layout, "Map3");
        }

        [MenuItem("PrismPanic/Generate Map4 (Hard)")]
        public static void GenerateMap4()
        {
            var layout = GetOrCreateLayout("Map4", "map4_hard");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel");

            // 14x14 room
            layout.wallPositions = BuildPerimeterWalls(7);
            layout.floorPositions = BuildFloorGrid(7);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(4f, 1f, 3f), rotationY = 40f },
                new MirrorPlacement { position = new Vector3(-4f, 1f, -3f), rotationY = -55f },
                new MirrorPlacement { position = new Vector3(2f, 1f, -4f), rotationY = 70f },
                new MirrorPlacement { position = new Vector3(-2f, 1f, 4f), rotationY = -25f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 0f), rotationY = 45f }
            };

            layout.pillarPositions = new Vector3[]
            {
                new Vector3(3f, 1.5f, 0f),
                new Vector3(-3f, 1.5f, 0f),
                new Vector3(0f, 1.5f, 3f),
                new Vector3(0f, 1.5f, -3f)
            };

            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -5f);

            layout.enemySpawnPoints = new Vector3[]
            {
                new Vector3(-5f, 0.5f, 5f),
                new Vector3(5f, 0.5f, 5f),
                new Vector3(-3f, 0.5f, 3f),
                new Vector3(3f, 0.5f, 3f),
                new Vector3(0f, 0.5f, 5f),
                new Vector3(0f, 0.5f, 0f)
            };

            layout.doorSpawnPoints = new Vector3[]
            {
                new Vector3(0f, 1.5f, 6f),
                new Vector3(-4f, 1.5f, 6f),
                new Vector3(4f, 1.5f, 6f)
            };

            layout.waves = new EnemyWaveData[]
            {
                new EnemyWaveData { enemyData = baseAngel, count = 2 },
                new EnemyWaveData { enemyData = fastAngel != null ? fastAngel : baseAngel, count = 4 }
            };

            SaveLayout(layout, "Map4");
        }

        [MenuItem("PrismPanic/Generate ALL Maps")]
        public static void GenerateAll()
        {
            GenerateMap1();
            GenerateMap2();
            GenerateMap3();
            GenerateMap4();
            Debug.Log("[PrismPanic] All 4 maps generated!");
        }

        // --- Helpers ---

        private static LevelLayoutSO GetOrCreateLayout(string name, string id)
        {
            string folder = "Assets/_Game/Scripts/ScriptableObjects/LevelLayouts";
            string path = $"{folder}/{name}.asset";
            var layout = AssetDatabase.LoadAssetAtPath<LevelLayoutSO>(path);

            if (layout == null)
            {
                layout = ScriptableObject.CreateInstance<LevelLayoutSO>();
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.CreateFolder("Assets/_Game/Scripts/ScriptableObjects", "LevelLayouts");
                }
                AssetDatabase.CreateAsset(layout, path);
            }

            layout.layoutID = id;
            return layout;
        }

        private static void SaveLayout(LevelLayoutSO layout, string name)
        {
            EditorUtility.SetDirty(layout);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PrismPanic] {name} generated!");
            Selection.activeObject = layout;
        }

        private static Vector3[] BuildPerimeterWalls(int halfSize)
        {
            return BuildPerimeterWallsRect(halfSize, halfSize);
        }

        private static Vector3[] BuildPerimeterWallsRect(int halfX, int halfZ)
        {
            List<Vector3> walls = new List<Vector3>();
            for (int x = -halfX; x <= halfX; x++)
            {
                walls.Add(new Vector3(x, 0f, -halfZ));
                walls.Add(new Vector3(x, 0f, halfZ));
            }
            for (int z = -halfZ + 1; z <= halfZ - 1; z++)
            {
                walls.Add(new Vector3(-halfX, 0f, z));
                walls.Add(new Vector3(halfX, 0f, z));
            }
            return walls.ToArray();
        }

        private static Vector3[] BuildFloorGrid(int halfSize)
        {
            return BuildFloorGridRect(halfSize, halfSize);
        }

        private static Vector3[] BuildFloorGridRect(int halfX, int halfZ)
        {
            List<Vector3> floors = new List<Vector3>();
            for (int x = -halfX; x <= halfX; x++)
            {
                for (int z = -halfZ; z <= halfZ; z++)
                {
                    floors.Add(new Vector3(x, 0f, z));
                }
            }
            return floors.ToArray();
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
