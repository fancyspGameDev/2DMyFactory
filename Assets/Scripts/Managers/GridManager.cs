using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;

    private Building[,] grid;
    private List<Building> activeBuildings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            InitializeGrid();
        }
    }

    private void InitializeGrid()
    {
        grid = new Building[width, height];
        activeBuildings = new List<Building>();
    }

    public bool TryPlaceBuilding(Building buildingPrefab, Vector2Int position)
    {
        if (!IsAreaAvailable(position, buildingPrefab.size))
        {
            return false;
        }

        Building newBuilding = Instantiate(buildingPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        newBuilding.Place(position);
        
        RegisterBuilding(newBuilding, position);
        return true;
    }

    // Overload for use by GameManager and UIManager with direction
    public void PlaceBuilding(int x, int y, Building prefab, Vector2Int directionVector)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (!IsAreaAvailable(position, prefab.size))
        {
            return;
        }

        Building newBuilding = Instantiate(prefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        newBuilding.Place(position);

        // Apply direction
        Direction dir = directionVector.ToDirection();
        newBuilding.direction = dir;
        newBuilding.transform.rotation = Quaternion.Euler(0, 0, -90 * (int)dir);

        RegisterBuilding(newBuilding, position);
    }

    private void RegisterBuilding(Building newBuilding, Vector2Int position)
    {
        for (int x = 0; x < newBuilding.size.x; x++)
        {
            for (int y = 0; y < newBuilding.size.y; y++)
            {
                Vector2Int tilePos = position + new Vector2Int(x, y);
                // Ensure we don't go out of bounds even if something weird happens
                if (!IsOutOfBounds(tilePos))
                {
                    grid[tilePos.x, tilePos.y] = newBuilding;
                }
            }
        }
        activeBuildings.Add(newBuilding);
    }

    public void RemoveBuilding(Vector2Int position)
    {
        Building buildingToRemove = GetBuildingAt(position);
        if (buildingToRemove == null) return;

        // Clear all cells occupied by the building
        for (int x = 0; x < buildingToRemove.size.x; x++)
        {
            for (int y = 0; y < buildingToRemove.size.y; y++)
            {
                Vector2Int tilePos = buildingToRemove.gridPosition + new Vector2Int(x, y);
                if (!IsOutOfBounds(tilePos))
                {
                    grid[tilePos.x, tilePos.y] = null;
                }
            }
        }

        activeBuildings.Remove(buildingToRemove);
        Destroy(buildingToRemove.gameObject);
    }

    // Overload for UIManager
    public void RemoveBuilding(int x, int y)
    {
        RemoveBuilding(new Vector2Int(x, y));
    }
    
    /// <summary>
    /// Clears all buildings from the grid and destroys their GameObjects.
    /// </summary>
    public void ClearGrid()
    {
        foreach (Building building in activeBuildings)
        {
            if (building != null) // Check if the building still exists, as it might have been destroyed by another process
            {
                Destroy(building.gameObject);
            }
        }
        activeBuildings.Clear();
        InitializeGrid(); // Re-initialize the grid to clear all references
    }

    public Building GetBuildingAt(Vector2Int position)
    {
        if (IsOutOfBounds(position))
        {
            return null;
        }
        return grid[position.x, position.y];
    }

    public bool TryMoveItem(ItemData item, Vector2Int targetPos)
    {
        Building building = GetBuildingAt(targetPos);
        if (building != null)
        {
            return building.AcceptItem(item);
        }
        return false;
    }
    
    public List<Building> GetAllActiveBuildings()
    {
        return activeBuildings;
    }

    private bool IsAreaAvailable(Vector2Int position, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int tilePos = position + new Vector2Int(x, y);
                if (IsOutOfBounds(tilePos) || GetBuildingAt(tilePos) != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsOutOfBounds(Vector2Int position)
    {
        return position.x < 0 || position.x >= width || position.y < 0 || position.y >= height;
    }
}