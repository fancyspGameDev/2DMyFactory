using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Belt : Building, IItemReceiver, IItemSource
{
    // A class to represent an item's state on the belt
    public class ItemOnBelt
    {
        public ItemData data;
        public float progress; // 0.0 at the start of the belt, 1.0 at the end
        public GameObject visual;
    }

    [Header("Belt Settings")]
    [SerializeField] private float speed = 1f; // Tiles per second
    [SerializeField] private float itemSize = 0.6f; // Min distance between items

    [Header("Belt State")]
    public List<ItemOnBelt> items = new List<ItemOnBelt>();

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] beltSprites;

    private int currentSpriteIndex = 0;
    private bool currentIsCorner = false;

    private void Update()
    {
        UpdateItemVisuals();
    }

    private void UpdateItemVisuals()
    {
        foreach (var item in items)
        {
            if (item.visual == null)
            {
                item.visual = new GameObject($"{item.data.displayName}_Visual");
                item.visual.transform.SetParent(transform);
                item.visual.transform.localScale = Vector3.one * 0.5f;

                var sr = item.visual.AddComponent<SpriteRenderer>();
                sr.sprite = item.data.icon;
                sr.sortingOrder = 5;
            }

            Vector3 startPos;
            Vector3 endPos;

            if (currentIsCorner)
            {
                // Mapping based on spriteIndex 1-8
                // 1: N->E, 2: E->N, 3: E->S, 4: S->E, 5: S->W, 6: W->S, 7: W->N, 8: N->W
                switch (currentSpriteIndex)
                {
                    case 1: startPos = new Vector3(0, 0.5f, 0); endPos = new Vector3(0.5f, 0, 0); break;
                    case 2: startPos = new Vector3(0.5f, 0, 0); endPos = new Vector3(0, 0.5f, 0); break;
                    case 3: startPos = new Vector3(0.5f, 0, 0); endPos = new Vector3(0, -0.5f, 0); break;
                    case 4: startPos = new Vector3(0, -0.5f, 0); endPos = new Vector3(0.5f, 0, 0); break;
                    case 5: startPos = new Vector3(0, -0.5f, 0); endPos = new Vector3(-0.5f, 0, 0); break;
                    case 6: startPos = new Vector3(-0.5f, 0, 0); endPos = new Vector3(0, -0.5f, 0); break;
                    case 7: startPos = new Vector3(-0.5f, 0, 0); endPos = new Vector3(0, 0.5f, 0); break;
                    case 8: startPos = new Vector3(0, 0.5f, 0); endPos = new Vector3(-0.5f, 0, 0); break;
                    default: startPos = new Vector3(0, -0.5f, 0); endPos = new Vector3(0, 0.5f, 0); break;
                }
            }
            else
            {
                // Straight: Always move from back edge to front edge relative to rotation
                // Since parent is rotated, local -0.5 to 0.5 on Y axis is correct.
                startPos = new Vector3(0, -0.5f, 0);
                endPos = new Vector3(0, 0.5f, 0);
            }
            
            item.visual.transform.localPosition = Vector3.Lerp(startPos, endPos, item.progress);
        }
    }

    public override void Place(Vector2Int pos)
    {
        base.Place(pos);
        Debug.Log($"[Belt] Placing at {pos}, Direction: {direction}");
        UpdateSprite();
        UpdateNeighborSprites();
    }

    private void UpdateNeighborSprites()
    {
        Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var offset in offsets)
        {
            var neighbor = GridManager.Instance.GetBuildingAt(gridPosition + offset) as Belt;
            if (neighbor != null)
            {
                neighbor.UpdateSprite();
            }
        }
    }

    public void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        List<Direction> inputs = new List<Direction>();
        Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Direction[] dirs = { Direction.North, Direction.South, Direction.West, Direction.East };

        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighborPos = gridPosition + offsets[i];
            Building neighbor = GridManager.Instance.GetBuildingAt(neighborPos);
            if (neighbor is Belt beltNeighbor)
            {
                if (neighborPos + beltNeighbor.GetVectorForDirection(beltNeighbor.direction) == gridPosition)
                {
                    inputs.Add(dirs[i]);
                }
            }
        }

        Direction outputDir = direction;
        Direction backDir = Opposite(outputDir);

        currentSpriteIndex = 0;
        currentIsCorner = false;

        if (!inputs.Contains(backDir) && inputs.Count > 0)
        {
            foreach (var inputDir in inputs)
            {
                if ((inputDir == Direction.North && outputDir == Direction.East))
                {
                    currentSpriteIndex = 1; currentIsCorner = true; break;
                }
                if ((inputDir == Direction.East && outputDir == Direction.North))
                {
                    currentSpriteIndex = 2; currentIsCorner = true; break;
                }
                if ((inputDir == Direction.East && outputDir == Direction.South))
                {
                    currentSpriteIndex = 3; currentIsCorner = true; break;
                }
                if ((inputDir == Direction.South && outputDir == Direction.East))
                {
                    currentSpriteIndex = 4; currentIsCorner = true; break;
                }
                if ((inputDir == Direction.South && outputDir == Direction.West))
                {
                    currentSpriteIndex = 5; currentIsCorner = true; break;
                }
                if ((inputDir == Direction.West && outputDir == Direction.South))
                {
                    currentSpriteIndex = 6; currentIsCorner = true; break;
                }
                if ((inputDir == Direction.West && outputDir == Direction.North))
                {
                    currentSpriteIndex = 7; currentIsCorner = true; break;
                }
                if ((inputDir == Direction.North && outputDir == Direction.West))
                {
                    currentSpriteIndex = 8; currentIsCorner = true; break;
                }
            }
        }

        string inputLog = string.Join(", ", inputs);
        Debug.Log($"[Belt at {gridPosition}] Output: {outputDir}, Receiving from: [{inputLog}], isCorner: {currentIsCorner}, SpriteIndex: {currentSpriteIndex}");

        if (currentIsCorner && currentSpriteIndex < beltSprites.Length)
        {
            spriteRenderer.sprite = beltSprites[currentSpriteIndex];
            transform.rotation = Quaternion.identity;
        }
        else
        {
            if (beltSprites.Length > 0) spriteRenderer.sprite = beltSprites[0];
            transform.rotation = Quaternion.Euler(0, 0, -90 * (int)direction);
        }
    }
    
    private Direction Opposite(Direction d)
    {
        if (d == Direction.North) return Direction.South;
        if (d == Direction.South) return Direction.North;
        if (d == Direction.East) return Direction.West;
        return Direction.East;
    }

    public override void OnTick()
    {
        // moveAmount is speed * tickInterval (0.1s)
        float moveAmount = speed * 0.1f;

        // 1. Move and Collide
        // Iterate forwards because item[i] depends on item[i-1]'s position
        for (int i = 0; i < items.Count; i++)
        {
            items[i].progress += moveAmount;

            float limit = 1.0f;
            if (i > 0)
            {
                // Cannot move past the item ahead of us minus the spacing (itemSize)
                limit = items[i - 1].progress - itemSize;
            }

            if (items[i].progress > limit)
            {
                items[i].progress = limit;
            }
        }

        // 2. Eject the head item if it reached the end
        if (items.Count > 0 && items[0].progress >= 1.0f)
        {
            Building nextBuilding = GridManager.Instance.GetBuildingAt(gridPosition + GetVectorForDirection(direction));
            
            if (nextBuilding is IItemReceiver receiver)
            {
                ItemStack itemStack = new ItemStack { item = items[0].data, count = 1 };
                if (receiver.TryReceiveItem(itemStack))
                {
                    if (items[0].visual != null) Destroy(items[0].visual);
                    items.RemoveAt(0);
                }
            }
        }
    }

    public override void GetSaveData(BuildingSaveData data)
    {
        base.GetSaveData(data);
        data.items = new List<ItemOnBeltSaveData>();
        foreach (var item in items)
        {
            data.items.Add(new ItemOnBeltSaveData
            {
                itemId = item.data.id,
                progress = item.progress
            });
        }
    }

    public override void LoadSaveData(BuildingSaveData data)
    {
        base.LoadSaveData(data);
        items.Clear();
        foreach (var itemData in data.items)
        {
            items.Add(new ItemOnBelt
            {
                data = SaveManager.Instance.GetItemDataById(itemData.itemId),
                progress = itemData.progress
            });
        }
    }

    /// <summary>
    /// An inserter places an item onto the beginning of the belt.
    /// </summary>
    public bool TryReceiveItem(ItemStack item)
    {
        // Prevent item collision at the start of the belt
        if (items.Any(i => i.progress < itemSize))
        {
            return false;
        }

        items.Add(new ItemOnBelt { data = item.item, progress = 0.0f });
        return true;
    }

    /// <summary>
    /// An inserter takes an item from the belt. Let's make it take the one nearest the end.
    /// </summary>
    public ItemStack TakeItem()
    {
        if (items.Count == 0) return default;

        // Find the item with the highest progress
        ItemOnBelt itemToTake = items.OrderByDescending(i => i.progress).First();
        
        // Only allow taking items that have reached at least the middle of the belt
        if (itemToTake.progress < 0.5f)
        {
            return default;
        }

        // Ensure visual is destroyed immediately
        if (itemToTake.visual != null) 
        {
            DestroyImmediate(itemToTake.visual);
        }
        
        items.Remove(itemToTake);
        // Debug.Log($"[Belt {gridPosition}] Item taken. Remaining count: {items.Count}");
        return new ItemStack { item = itemToTake.data, count = 1 };
    }
    
    private Vector2Int GetVectorForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Vector2Int.up;
            case Direction.East:  return Vector2Int.right;
            case Direction.South: return Vector2Int.down;
            case Direction.West:  return Vector2Int.left;
        }
        return Vector2Int.zero;
    }
}
