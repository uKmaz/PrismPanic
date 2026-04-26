#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using PrismPanic.ScriptableObjects;
using System.Collections.Generic;

namespace PrismPanic.Editor
{
    public static class LevelLayoutGenerator
    {
        [MenuItem("PrismPanic/Generate ALL Maps (0 to 8)")]
        public static void GenerateAll()
        {
            GenerateMap0(); // Tutorial Menu
            GenerateMap1();
            GenerateMap2();
            GenerateMap3();
            GenerateMap4();
            GenerateMap5();
            GenerateMap6();
            GenerateMap7();
            GenerateMap8();
            Debug.Log("[PrismPanic] All 9 maps generated (including Tutorial)!");
        }

        [MenuItem("PrismPanic/Maps/Generate Map0 (Tutorial Menu)")]
        public static void GenerateMap0()
        {
            var layout = GetOrCreateLayout("Map0", "map0_tutorial");
            var baseAngel = FindEnemyData("BaseAngel"); // User will rename or create a "TutorialAngel" if they want, but base works since it takes 999 dmg

            layout.wallPositions = BuildPerimeterWalls(5);
            layout.floorPositions = BuildFloorGrid(5);

            // 3 Mirrors for the Purple Beam puzzle
            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(3f, 1f, -3f), rotationY = -45f },  // Bounces UP (+Z)
                new MirrorPlacement { position = new Vector3(3f, 1f, 3f), rotationY = -135f },  // Bounces LEFT (-X)
                new MirrorPlacement { position = new Vector3(0f, 1f, 3f), rotationY = 135f }    // Bounces DOWN (-Z) to Angel
            };

            layout.pillarPositions = new Vector3[]
            {
                new Vector3(0f, 1.5f, -1.5f) // Blocks direct shot from player to angel
            };

            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -3f);
            
            layout.enemySpawnPoints = new Vector3[]
            {
                new Vector3(0f, 0.5f, 0f) // The target angel
            };

            layout.doorSpawnPoints = new Vector3[]
            {
                new Vector3(0f, 1.5f, 4f),
                new Vector3(-2f, 1.5f, 4f),
                new Vector3(2f, 1.5f, 4f)
            };

            layout.waves = new EnemyWaveData[]
            {
                new EnemyWaveData { enemyData = baseAngel, count = 1 }
            };

            SaveLayout(layout, "Map0");
        }

        [MenuItem("PrismPanic/Maps/Generate Map1")]
        public static void GenerateMap1()
        {
            var layout = GetOrCreateLayout("Map1", "map1");
            var baseAngel = FindEnemyData("BaseAngel");

            layout.wallPositions = BuildPerimeterWalls(6);
            layout.floorPositions = BuildFloorGrid(6);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(2f, 1f, 0f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-2f, 1f, 3f), rotationY = -120f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(0f, 1.5f, -2f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);
            
            layout.enemySpawnPoints = new Vector3[] { new Vector3(-3f, 0.5f, 3f), new Vector3(3f, 0.5f, 3f) };
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 5f), new Vector3(-3f, 1.5f, 5f), new Vector3(3f, 1.5f, 5f) };
            layout.waves = new EnemyWaveData[] { new EnemyWaveData { enemyData = baseAngel, count = 2 } };
            SaveLayout(layout, "Map1");
        }

        [MenuItem("PrismPanic/Maps/Generate Map2")]
        public static void GenerateMap2()
        {
            var layout = GetOrCreateLayout("Map2", "map2");
            var baseAngel = FindEnemyData("BaseAngel");

            layout.wallPositions = BuildPerimeterWallsRect(7, 6);
            layout.floorPositions = BuildFloorGridRect(7, 6);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(3f, 1f, 2f), rotationY = -40f },
                new MirrorPlacement { position = new Vector3(-3f, 1f, -1f), rotationY = -135f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 4f), rotationY = -70f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(2f, 1.5f, -2f), new Vector3(-2f, 1.5f, 2f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);
            layout.enemySpawnPoints = new Vector3[] { new Vector3(-4f, 0.5f, 4f), new Vector3(4f, 0.5f, 4f), new Vector3(0f, 0.5f, 3f) };
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 5f), new Vector3(-3f, 1.5f, 5f), new Vector3(3f, 1.5f, 5f) };
            layout.waves = new EnemyWaveData[] { new EnemyWaveData { enemyData = baseAngel, count = 3 } };
            SaveLayout(layout, "Map2");
        }

        [MenuItem("PrismPanic/Maps/Generate Map3")]
        public static void GenerateMap3()
        {
            var layout = GetOrCreateLayout("Map3", "map3");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel") ?? baseAngel;

            layout.wallPositions = BuildPerimeterWallsRect(7, 7);
            layout.floorPositions = BuildFloorGridRect(7, 7);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(4f, 1f, 0f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-4f, 1f, 3f), rotationY = -150f },
                new MirrorPlacement { position = new Vector3(0f, 1f, -2f), rotationY = -60f },
                new MirrorPlacement { position = new Vector3(2f, 1f, 4f), rotationY = -135f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(3f, 1.5f, 2f), new Vector3(-3f, 1.5f, -1f), new Vector3(0f, 1.5f, 1f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -5f);
            layout.enemySpawnPoints = new Vector3[] { new Vector3(-5f, 0.5f, 4f), new Vector3(5f, 0.5f, 4f), new Vector3(-2f, 0.5f, 2f), new Vector3(2f, 0.5f, 2f) };
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 6f), new Vector3(-4f, 1.5f, 6f), new Vector3(4f, 1.5f, 6f) };
            layout.waves = new EnemyWaveData[] { new EnemyWaveData { enemyData = baseAngel, count = 2 }, new EnemyWaveData { enemyData = fastAngel, count = 2 } };
            SaveLayout(layout, "Map3");
        }

        [MenuItem("PrismPanic/Maps/Generate Map4")]
        public static void GenerateMap4()
        {
            var layout = GetOrCreateLayout("Map4", "map4");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel") ?? baseAngel;

            layout.wallPositions = BuildPerimeterWalls(8);
            layout.floorPositions = BuildFloorGrid(8);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(4f, 1f, 3f), rotationY = -50f },
                new MirrorPlacement { position = new Vector3(-4f, 1f, -3f), rotationY = -145f },
                new MirrorPlacement { position = new Vector3(2f, 1f, -4f), rotationY = -20f },
                new MirrorPlacement { position = new Vector3(-2f, 1f, 4f), rotationY = -115f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 0f), rotationY = -45f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(3f, 1.5f, 0f), new Vector3(-3f, 1.5f, 0f), new Vector3(0f, 1.5f, 3f), new Vector3(0f, 1.5f, -3f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -6f);
            layout.enemySpawnPoints = new Vector3[] { new Vector3(-5f, 0.5f, 5f), new Vector3(5f, 0.5f, 5f), new Vector3(-3f, 0.5f, 3f), new Vector3(3f, 0.5f, 3f), new Vector3(0f, 0.5f, 5f) };
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 7f), new Vector3(-4f, 1.5f, 7f), new Vector3(4f, 1.5f, 7f) };
            layout.waves = new EnemyWaveData[] { new EnemyWaveData { enemyData = baseAngel, count = 2 }, new EnemyWaveData { enemyData = fastAngel, count = 3 } };
            SaveLayout(layout, "Map4");
        }

        [MenuItem("PrismPanic/Maps/Generate Map5")]
        public static void GenerateMap5()
        {
            var layout = GetOrCreateLayout("Map5", "map5");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel") ?? baseAngel;

            layout.wallPositions = BuildPerimeterWallsRect(9, 7);
            layout.floorPositions = BuildFloorGridRect(9, 7);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(6f, 1f, 4f), rotationY = -120f },
                new MirrorPlacement { position = new Vector3(-6f, 1f, 4f), rotationY = -60f },
                new MirrorPlacement { position = new Vector3(4f, 1f, -2f), rotationY = -30f },
                new MirrorPlacement { position = new Vector3(-4f, 1f, -2f), rotationY = -150f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 2f), rotationY = 0f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(2f, 1.5f, 2f), new Vector3(-2f, 1.5f, 2f), new Vector3(0f, 1.5f, -1f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);
            layout.enemySpawnPoints = new Vector3[] { new Vector3(-7f, 0.5f, 5f), new Vector3(7f, 0.5f, 5f), new Vector3(-4f, 0.5f, 2f), new Vector3(4f, 0.5f, 2f), new Vector3(0f, 0.5f, 5f) };
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 6f), new Vector3(-5f, 1.5f, 6f), new Vector3(5f, 1.5f, 6f) };
            layout.waves = new EnemyWaveData[] { new EnemyWaveData { enemyData = baseAngel, count = 3 }, new EnemyWaveData { enemyData = fastAngel, count = 3 } };
            SaveLayout(layout, "Map5");
        }

        [MenuItem("PrismPanic/Maps/Generate Map6")]
        public static void GenerateMap6()
        {
            var layout = GetOrCreateLayout("Map6", "map6");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel") ?? baseAngel;

            layout.wallPositions = BuildPerimeterWalls(9);
            layout.floorPositions = BuildFloorGrid(9);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(5f, 1f, 5f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-5f, 1f, -5f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-5f, 1f, 5f), rotationY = -135f },
                new MirrorPlacement { position = new Vector3(5f, 1f, -5f), rotationY = -135f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 0f), rotationY = -67.5f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(3f, 1.5f, 3f), new Vector3(-3f, 1.5f, -3f), new Vector3(-3f, 1.5f, 3f), new Vector3(3f, 1.5f, -3f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);
            layout.enemySpawnPoints = new Vector3[] { new Vector3(-6f, 0.5f, 6f), new Vector3(6f, 0.5f, 6f), new Vector3(-2f, 0.5f, 4f), new Vector3(2f, 0.5f, 4f), new Vector3(-6f, 0.5f, 0f), new Vector3(6f, 0.5f, 0f) };
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 8f), new Vector3(-4f, 1.5f, 8f), new Vector3(4f, 1.5f, 8f) };
            layout.waves = new EnemyWaveData[] { new EnemyWaveData { enemyData = baseAngel, count = 4 }, new EnemyWaveData { enemyData = fastAngel, count = 3 } };
            SaveLayout(layout, "Map6");
        }

        [MenuItem("PrismPanic/Maps/Generate Map7")]
        public static void GenerateMap7()
        {
            var layout = GetOrCreateLayout("Map7", "map7");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel") ?? baseAngel;

            layout.wallPositions = BuildPerimeterWallsRect(10, 8);
            layout.floorPositions = BuildFloorGridRect(10, 8);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(7f, 1f, 4f), rotationY = -75f },
                new MirrorPlacement { position = new Vector3(-7f, 1f, 4f), rotationY = -105f },
                new MirrorPlacement { position = new Vector3(3f, 1f, -2f), rotationY = -15f },
                new MirrorPlacement { position = new Vector3(-3f, 1f, -2f), rotationY = -165f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 5f), rotationY = 0f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 2f), rotationY = -90f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(5f, 1.5f, 0f), new Vector3(-5f, 1.5f, 0f), new Vector3(2f, 1.5f, 3f), new Vector3(-2f, 1.5f, 3f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);
            layout.enemySpawnPoints = new Vector3[] { new Vector3(-8f, 0.5f, 6f), new Vector3(8f, 0.5f, 6f), new Vector3(-4f, 0.5f, 6f), new Vector3(4f, 0.5f, 6f), new Vector3(-8f, 0.5f, -2f), new Vector3(8f, 0.5f, -2f) };
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 7f), new Vector3(-5f, 1.5f, 7f), new Vector3(5f, 1.5f, 7f) };
            layout.waves = new EnemyWaveData[] { new EnemyWaveData { enemyData = baseAngel, count = 3 }, new EnemyWaveData { enemyData = fastAngel, count = 5 } };
            SaveLayout(layout, "Map7");
        }

        [MenuItem("PrismPanic/Maps/Generate Map8")]
        public static void GenerateMap8()
        {
            var layout = GetOrCreateLayout("Map8", "map8");
            var baseAngel = FindEnemyData("BaseAngel");
            var fastAngel = FindEnemyData("FastAngel") ?? baseAngel;

            // Huge boss-like room
            layout.wallPositions = BuildPerimeterWalls(11);
            layout.floorPositions = BuildFloorGrid(11);

            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(8f, 1f, 8f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-8f, 1f, -8f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-8f, 1f, 8f), rotationY = -135f },
                new MirrorPlacement { position = new Vector3(8f, 1f, -8f), rotationY = -135f },
                new MirrorPlacement { position = new Vector3(4f, 1f, 4f), rotationY = -75f },
                new MirrorPlacement { position = new Vector3(-4f, 1f, 4f), rotationY = -105f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 0f), rotationY = 0f },
                new MirrorPlacement { position = new Vector3(0f, 1f, 7f), rotationY = -90f }
            };

            layout.pillarPositions = new Vector3[] { new Vector3(6f, 1.5f, 0f), new Vector3(-6f, 1.5f, 0f), new Vector3(0f, 1.5f, 4f), new Vector3(4f, 1.5f, -4f), new Vector3(-4f, 1.5f, -4f) };
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -4f);
            
            layout.enemySpawnPoints = new Vector3[] 
            { 
                new Vector3(-9f, 0.5f, 9f), new Vector3(9f, 0.5f, 9f), 
                new Vector3(-5f, 0.5f, 8f), new Vector3(5f, 0.5f, 8f), 
                new Vector3(-9f, 0.5f, 0f), new Vector3(9f, 0.5f, 0f),
                new Vector3(-4f, 0.5f, 2f), new Vector3(4f, 0.5f, 2f) 
            };
            
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 10f), new Vector3(-6f, 1.5f, 10f), new Vector3(6f, 1.5f, 10f) };
            
            // 3 Waves for the final map
            layout.waves = new EnemyWaveData[] 
            { 
                new EnemyWaveData { enemyData = baseAngel, count = 4 }, 
                new EnemyWaveData { enemyData = fastAngel, count = 6 },
                new EnemyWaveData { enemyData = fastAngel, count = 4 }
            };
            SaveLayout(layout, "Map8");
        }

        [MenuItem("PrismPanic/Maps/Generate Map9 (Boss)")]
        public static void GenerateMap9()
        {
            var layout = GetOrCreateLayout("Map9", "map9");

            // Circular/Square large map for boss
            layout.wallPositions = BuildPerimeterWalls(12);
            layout.floorPositions = BuildFloorGrid(12);

            // Dynamic mirrors will be handled by the layout or prefabs later,
            // but we can spawn initial mirrors here
            layout.mirrorPlacements = new MirrorPlacement[]
            {
                new MirrorPlacement { position = new Vector3(8f, 1f, 8f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-8f, 1f, -8f), rotationY = -45f },
                new MirrorPlacement { position = new Vector3(-8f, 1f, 8f), rotationY = -135f },
                new MirrorPlacement { position = new Vector3(8f, 1f, -8f), rotationY = -135f }
            };

            // Pillars to hide behind from the boss laser
            layout.pillarPositions = new Vector3[] 
            { 
                new Vector3(4f, 1.5f, 4f), new Vector3(-4f, 1.5f, 4f), 
                new Vector3(4f, 1.5f, -4f), new Vector3(-4f, 1.5f, -4f),
                new Vector3(0f, 1.5f, 6f), new Vector3(0f, 1.5f, -6f),
                new Vector3(6f, 1.5f, 0f), new Vector3(-6f, 1.5f, 0f)
            };
            
            layout.playerSpawnPoint = new Vector3(0f, 0.5f, -9f);
            
            // Set Boss properties
            layout.hasBoss = true;
            layout.bossSpawnPoint = new Vector3(0f, 1.5f, 0f); // Center of the room

            // No regular waves for boss fight, or maybe 1 initial wave
            layout.waves = new EnemyWaveData[0];
            layout.enemySpawnPoints = new Vector3[0];
            
            // Doors appear after boss is defeated
            layout.doorSpawnPoints = new Vector3[] { new Vector3(0f, 1.5f, 10f) };
            
            SaveLayout(layout, "Map9");
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
