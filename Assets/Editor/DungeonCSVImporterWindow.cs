using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// CSV로부터 DungeonData ScriptableObject를 생성/저장하는 에디터 임포터 윈도우
/// 메뉴: Tools/Dungeon/Import DungeonData from CSV
/// </summary>
public class DungeonCSVImporterWindow : EditorWindow
{
    private string dungeonCSVPath = "Assets/Resources/DungeonData/DungeonFloors.csv";
    private string enemySpawnsCSVPath = "Assets/Resources/DungeonData/EnemySpawns.csv";
    private string rewardsCSVPath = "Assets/Resources/DungeonData/Rewards.csv";

    private string outputAssetPath = "Assets/DungeonData.asset";

    private string dungeonName = "CSV 던전";
    private int totalFloors = 10;
    private float difficultyMultiplier = 1.0f;

    [MenuItem("Tools/Dungeon/Import DungeonData from CSV")] 
    public static void Open()
    {
        var win = GetWindow<DungeonCSVImporterWindow>(true, "Dungeon CSV Importer", true);
        win.minSize = new Vector2(520, 360);
        win.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("CSV 파일 경로", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        DrawCSVField(ref dungeonCSVPath, "DungeonFloors.csv");
        DrawCSVField(ref enemySpawnsCSVPath, "EnemySpawns.csv");
        DrawCSVField(ref rewardsCSVPath, "Rewards.csv");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("던전 기본 설정", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        dungeonName = EditorGUILayout.TextField("Dungeon Name", dungeonName);
        totalFloors = EditorGUILayout.IntField("Total Floors", totalFloors);
        difficultyMultiplier = EditorGUILayout.FloatField("Difficulty Multiplier", difficultyMultiplier);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("출력 에셋 경로", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        outputAssetPath = EditorGUILayout.TextField(outputAssetPath);
        if (GUILayout.Button("Select", GUILayout.Width(80)))
        {
            string selected = EditorUtility.SaveFilePanelInProject(
                "Save DungeonData", "DungeonData", "asset", "Select output path for DungeonData.asset");
            if (!string.IsNullOrEmpty(selected))
            {
                outputAssetPath = selected;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate CSV"))
        {
            ValidateCSVFiles();
        }

        GUI.enabled = File.Exists(dungeonCSVPath) && File.Exists(enemySpawnsCSVPath) && File.Exists(rewardsCSVPath);
        if (GUILayout.Button("Import & Save"))
        {
            ImportAndSave();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawCSVField(ref string path, string label)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(140));
        path = EditorGUILayout.TextField(path);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string picked = EditorUtility.OpenFilePanel("Select " + label, Application.dataPath, "csv");
            if (!string.IsNullOrEmpty(picked))
            {
                if (picked.StartsWith(Application.dataPath))
                {
                    // absolute -> relative Assets path
                    path = "Assets" + picked.Substring(Application.dataPath.Length);
                }
                else
                {
                    path = picked; // allow absolute
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ValidateCSVFiles()
    {
        bool ok = true;
        ok &= File.Exists(dungeonCSVPath);
        ok &= File.Exists(enemySpawnsCSVPath);
        ok &= File.Exists(rewardsCSVPath);
        if (!ok)
        {
            EditorUtility.DisplayDialog("Validate CSV", "일부 CSV 파일을 찾을 수 없습니다.", "OK");
            return;
        }

        var floors = CSVParser.ParseCSV(GetReadablePath(dungeonCSVPath));
        var spawns = CSVParser.ParseCSV(GetReadablePath(enemySpawnsCSVPath));
        var rewards = CSVParser.ParseCSV(GetReadablePath(rewardsCSVPath));

        if (floors.Length == 0 || spawns.Length == 0 || rewards.Length == 0)
        {
            EditorUtility.DisplayDialog("Validate CSV", "CSV 데이터가 비어있거나 읽기에 실패했습니다.", "OK");
            return;
        }

        EditorUtility.DisplayDialog("Validate CSV", "CSV 파일 검증 성공", "OK");
    }

    private void ImportAndSave()
    {
        try
        {
            var floors = CSVParser.ParseCSV(GetReadablePath(dungeonCSVPath));
            var spawns = CSVParser.ParseCSV(GetReadablePath(enemySpawnsCSVPath));
            var rewards = CSVParser.ParseCSV(GetReadablePath(rewardsCSVPath));

            if (floors.Length == 0 || spawns.Length == 0 || rewards.Length == 0)
            {
                EditorUtility.DisplayDialog("Import", "CSV를 읽을 수 없습니다.", "OK");
                return;
            }

            // Build cache
            var floorCache = new Dictionary<int, DungeonFloorData>();

            // Floors: FloorNumber,FloorName,FloorDescription,IsBossFloor,BossName
            for (int i = 1; i < floors.Length; i++)
            {
                if (floors[i].Length < 5) continue;
                DungeonFloorData fd = new DungeonFloorData();
                fd.floorNumber = SafeInt(floors[i][0]);
                fd.floorName = floors[i][1];
                fd.floorDescription = floors[i][2];
                fd.isBossFloor = SafeBool(floors[i][3]);
                fd.bossName = floors[i][4];
                fd.enemySpawns = new List<EnemySpawnData>();
                fd.itemRewards = new List<ItemReward>();
                floorCache[fd.floorNumber] = fd;
            }

            // Spawns: FloorNumber,EnemyType,SpawnCount,SpawnX,SpawnY,SpawnDelay,IsBoss
            for (int i = 1; i < spawns.Length; i++)
            {
                if (spawns[i].Length < 7) continue;
                int floor = SafeInt(spawns[i][0]);
                if (!floorCache.ContainsKey(floor)) continue;

                EnemySpawnData es = new EnemySpawnData();
                es.enemyType = SafeEnemyType(spawns[i][1]);
                es.spawnCount = SafeInt(spawns[i][2]);
                float sx = SafeFloat(spawns[i][3]);
                float sy = SafeFloat(spawns[i][4]);
                es.spawnPosition = new Vector2(sx, sy);
                es.spawnDelay = SafeFloat(spawns[i][5]);
                es.isBoss = SafeBool(spawns[i][6]);

                var fd = floorCache[floor];
                if (fd.enemySpawns == null) fd.enemySpawns = new List<EnemySpawnData>();
                fd.enemySpawns.Add(es);
                floorCache[floor] = fd;
            }

            // Rewards: FloorNumber,ExperienceReward,GoldReward,ItemName,ItemCount,DropChance
            for (int i = 1; i < rewards.Length; i++)
            {
                if (rewards[i].Length < 6) continue;
                int floor = SafeInt(rewards[i][0]);
                if (!floorCache.ContainsKey(floor)) continue;

                var fd = floorCache[floor];
                fd.experienceReward = SafeInt(rewards[i][1]);
                fd.goldReward = SafeInt(rewards[i][2]);

                string itemName = rewards[i][3];
                if (!string.IsNullOrEmpty(itemName))
                {
                    ItemReward ir = new ItemReward();
                    ir.itemName = itemName;
                    ir.itemCount = SafeInt(rewards[i][4]);
                    ir.dropChance = SafeFloat(rewards[i][5]);
                    if (fd.itemRewards == null) fd.itemRewards = new List<ItemReward>();
                    fd.itemRewards.Add(ir);
                }
                floorCache[floor] = fd;
            }

            // Create SO
            DungeonData so = ScriptableObject.CreateInstance<DungeonData>();
            so.dungeonName = dungeonName;
            so.totalFloors = totalFloors;
            so.difficultyMultiplier = difficultyMultiplier;
            so.floorData = new List<DungeonFloorData>();

            for (int i = 1; i <= totalFloors; i++)
            {
                if (floorCache.ContainsKey(i))
                {
                    so.floorData.Add(floorCache[i]);
                }
            }

            // Ensure directory exists
            string dir = Path.GetDirectoryName(outputAssetPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(so, outputAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Import", "DungeonData 에셋 생성 완료:\n" + outputAssetPath, "OK");
            Selection.activeObject = so;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            EditorUtility.DisplayDialog("Import Failed", ex.Message, "OK");
        }
    }

    private string GetReadablePath(string path)
    {
        // If path is under Assets, convert to absolute path for File IO
        if (path.StartsWith("Assets"))
        {
            return Path.GetFullPath(path);
        }
        return path;
    }

    private int SafeInt(string s)
    {
        int v; if (int.TryParse(s, out v)) return v; return 0;
    }
    private float SafeFloat(string s)
    {
        float v; if (float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v)) return v;
        if (float.TryParse(s, out v)) return v; return 0f;
    }
    private bool SafeBool(string s)
    {
        bool v; if (bool.TryParse(s, out v)) return v; return false;
    }
    private EnemyType SafeEnemyType(string s)
    {
        try { return (EnemyType)System.Enum.Parse(typeof(EnemyType), s, true); }
        catch { return EnemyType.Normal; }
    }
}


