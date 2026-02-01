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

    [Header("Belt State")]
    public List<ItemOnBelt> items = new List<ItemOnBelt>();

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] beltSprites; // 0: Vertical, 1: Horizontal, 2: Corner... (Simplified)

    private void Update()
    {
        UpdateItemVisuals();
    }

    private void UpdateItemVisuals()
    {
        foreach (var item in items)
        {
            // 1. Instantiate visual if missing
            if (item.visual == null)
            {
                item.visual = new GameObject($"{item.data.displayName}_Visual");
                item.visual.transform.SetParent(transform);
                item.visual.transform.localScale = Vector3.one * 0.5f; // Scale down a bit

                var sr = item.visual.AddComponent<SpriteRenderer>();
                sr.sprite = item.data.icon;
                sr.sortingOrder = 5; // Higher than belt (assuming belt is 0 or low)
            }

            // 2. Update Position
            // Calculate local position based on progress and belt direction logic
            // Assuming straight line from center to output edge?
            // Actually, Belt usually goes from Edge to Edge.
            // Progress 0.0 = Start Edge (Input side center?), 1.0 = End Edge (Output side center)
            // But Belt.cs logic is simplified.
            
            // Vector calculation:
            // Center is (0,0) local.
            // If straight:
            // Start local pos: (0, -0.5) for North direction (Up) ??
            // Let's rely on direction. 
            // If direction is North (Up), movement is along +Y.
            // Local Y goes from -0.5 to 0.5?
            
            // Standard generic way:
            // Start point is "Opposite of Direction" * 0.5
            // End point is "Direction" * 0.5
            // Actually, (0,0) is center of tile.
            // Local moves from -0.5 * DirVector to +0.5 * DirVector?
            // Let's assume progress 0 is Center (0,0) and 1 is Edge?
            // Logic says "moves along belt".
            // Let's use: Start = (0,0), End = DirectionVector (World 1 unit).
            // But we are in Local Space.
            // Belt rotation handles the visual rotation of the *Belt Sprite*.
            // BUT, item movement needs to match that.
            
            // Simplest implementation:
            // Move from (0,0) to (0,1) relative to rotation?
            // Belt rotation is set in UpdateSprite: transform.rotation = ...
            // If belt is rotated, "Up" local is the direction.
            // So we just move along Local Y?
            // If rotation is correct, Local Y is the forward direction.
            // Let's try: Local Pos = Vector3.up * (item.progress - 0.5f);
            // Range: -0.5 to 0.5 along Y axis.
            
            item.visual.transform.localPosition = Vector3.up * (item.progress - 0.5f);
        }
    }

    public override void Place(Vector2Int pos)
    {
        base.Place(pos);
        // Force update neighbors to refresh their sprites too
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

        // Auto-Tiling Logic
        // Determine input directions (where items are coming FROM)
        List<Direction> inputs = new List<Direction>();
        
        Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Direction[] dirs = { Direction.North, Direction.South, Direction.West, Direction.East }; // Matching offsets

        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighborPos = gridPosition + offsets[i];
            Building neighbor = GridManager.Instance.GetBuildingAt(neighborPos);
            
            // Check if neighbor is a Belt (or potentially other outputters) that points to us
            if (neighbor is Belt beltNeighbor)
            {
                Vector2Int neighborOutputVector = beltNeighbor.GetVectorForDirection(beltNeighbor.direction);
                if (neighborPos + neighborOutputVector == gridPosition)
                {
                    inputs.Add(dirs[i]);
                }
            }
            // Add checks for Machines outputting to this belt if necessary, 
            // but usually belts only curve for other belts.
        }

        // Determine Shape
        // Default to Straight
        bool isCorner = false;
        float rotationZ = -90 * (int)direction; // Default rotation based on output direction

        if (inputs.Count == 1)
        {
            Direction inputDir = inputs[0];
            Direction outputDir = direction;

            // Check if orthogonal (Corner)
            if (inputDir != outputDir && inputDir != Opposite(outputDir))
            {
                isCorner = true;
                
                // Determine Corner Rotation
                // Corner sprite assumed to be "Bottom to Right" (North input, East output) or similar reference.
                // Let's assume standard "Corner" sprite connects Bottom -> Right (Input South, Output East).
                
                // Calculate required rotation. 
                // We need to map (Input, Output) pair to a rotation.
                
                if (outputDir == Direction.North)
                {
                    if (inputDir == Direction.West) rotationZ = 0;     // West -> North (Left -> Up)
                    if (inputDir == Direction.East) rotationZ = 90;    // East -> North (Right -> Up)
                }
                else if (outputDir == Direction.East)
                {
                    if (inputDir == Direction.North) rotationZ = -90;  // North -> East (Up -> Right)
                    if (inputDir == Direction.South) rotationZ = 0;    // South -> East (Down -> Right)
                }
                else if (outputDir == Direction.South)
                {
                    if (inputDir == Direction.East) rotationZ = 180;   // East -> South (Right -> Down)
                    if (inputDir == Direction.West) rotationZ = -90;   // West -> South (Left -> Down)
                }
                else if (outputDir == Direction.West)
                {
                    if (inputDir == Direction.South) rotationZ = 90;   // South -> West (Down -> Left)
                    if (inputDir == Direction.North) rotationZ = 180;  // North -> West (Up -> Left)
                }
            }
        }

        // Apply Sprite and Rotation
        if (isCorner && beltSprites.Length > 1)
        {
            spriteRenderer.sprite = beltSprites[1]; // Assume index 1 is Corner
            // Adjust rotation for corner
            // Note: The rotation mapping above relies on a specific base sprite orientation.
            // If the base sprite is "Left -> Up" (West -> North), the angles change.
            // For now, we apply the calculated Z rotation.
             transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        }
        else
        {
            if (beltSprites.Length > 0) spriteRenderer.sprite = beltSprites[0]; // Assume index 0 is Straight
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
        // Item spacing/size. 0.35f ensures good visual separation and stacking behavior.
        float itemSize = 0.35f; 
        float moveAmount = speed * 0.1f;

        // 1. Move and Collide
        // Iterate forwards because item[i] depends on item[i-1]'s position
        for (int i = 0; i < items.Count; i++)
        {
            items[i].progress += moveAmount;

            float limit = 1.0f;
            if (i > 0)
            {
                // Cannot move past the item ahead of us minus the spacing
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
        if (items.Any(i => i.progress < 0.2f))
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
