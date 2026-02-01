using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Asset Registries")]
    [SerializeField] private List<Building> buildingPrefabs;
    [SerializeField] private List<ItemData> itemDatabase;
    [SerializeField] private List<RecipeData> recipeDatabase;

    private Dictionary<string, Building> buildingPrefabDict;
    private Dictionary<int, ItemData> itemDict;
    private Dictionary<string, RecipeData> recipeDict;
    
    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
        
        // Initialize dictionaries for fast lookups, handling potential duplicates in inspector lists
        buildingPrefabDict = buildingPrefabs
            .Where(p => p != null)
            .GroupBy(p => p.GetType().Name)
            .ToDictionary(g => g.Key, g => g.First());

        itemDict = itemDatabase
            .Where(i => i != null)
            .GroupBy(i => i.id)
            .ToDictionary(g => g.Key, g => g.First());

        recipeDict = recipeDatabase
            .Where(r => r != null)
            .GroupBy(r => r.id)
            .ToDictionary(g => g.Key, g => g.First());
            
        // Log warnings for duplicates to help cleanup
        CheckForDuplicates(buildingPrefabs.Where(p => p != null).Select(p => p.GetType().Name), "Building Prefab");
        CheckForDuplicates(itemDatabase.Where(i => i != null).Select(i => i.id.ToString()), "Item Data");
        CheckForDuplicates(recipeDatabase.Where(r => r != null).Select(r => r.id), "Recipe Data");
    }

    private void CheckForDuplicates(IEnumerable<string> keys, string context)
    {
        var duplicates = keys.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var dup in duplicates)
        {
            Debug.LogWarning($"Duplicate {context} found for key: {dup}. Only the first one will be used.");
        }
    }

    public void Save()
    {
        List<Building> buildings = GridManager.Instance.GetAllActiveBuildings();
        GameSaveData saveData = new GameSaveData { buildings = new List<BuildingSaveData>() };

        foreach (Building building in buildings)
        {
            BuildingSaveData buildingData = new BuildingSaveData();
            building.GetSaveData(buildingData); // Let the building itself populate the data
            saveData.buildings.Add(buildingData);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Game saved to {saveFilePath}");
    }

    public void Load()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Save file not found!");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        GridManager.Instance.ClearGrid(); // Clear the current grid before loading

        // Pass 1: Instantiate all buildings
        foreach (var buildingData in saveData.buildings)
        {
            if (buildingPrefabDict.TryGetValue(buildingData.type, out Building prefab))
            {
                Vector2Int pos = new Vector2Int(buildingData.x, buildingData.y);
                GridManager.Instance.TryPlaceBuilding(prefab, pos);
            }
        }
        
        // Pass 2: Restore state
        foreach (var buildingData in saveData.buildings)
        {
             Vector2Int pos = new Vector2Int(buildingData.x, buildingData.y);
             Building buildingInstance = GridManager.Instance.GetBuildingAt(pos);
             if (buildingInstance != null)
             {
                 buildingInstance.LoadSaveData(buildingData);
             }
        }

        Debug.Log("Game loaded!");
    }

    // Public API for asset lookups
    public ItemData GetItemDataById(int id) => itemDict.TryGetValue(id, out var item) ? item : null;
    public RecipeData GetRecipeDataById(string id) => recipeDict.TryGetValue(id, out var recipe) ? recipe : null;
}

