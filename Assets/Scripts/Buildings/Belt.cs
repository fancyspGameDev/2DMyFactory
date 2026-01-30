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
        // Visual Update Loop
        // 1. Interpolate item positions
        // Since we don't have separate GameObjects for items in this script yet, 
        // this is where we would update their transforms if we did.
        // For the prototype, we assume an 'ItemVisualManager' might handle this, 
        // or we would iterate through instantiated item prefabs here.
        
        // Example logic if we had visual instances:
        /*
        foreach (var item in items)
        {
            if (item.visualInstance != null)
            {
                Vector3 start = transform.position; // Local 0,0
                Vector3 end = transform.position + (Vector3)(Vector2)GetVectorForDirection(direction); 
                // Note: This logic assumes straight line. 
                // For corners, we'd need Bezier or arc math based on 'progress'.
                item.visualInstance.transform.position = Vector3.Lerp(start, end, item.progress);
            }
        }
        */
    }

    public override void Place(Vector2Int pos)
    {
        base.Place(pos);
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        // Auto-Tiling Logic (Simplified)
        // In a real implementation, we would check neighbors using GridManager
        // and set spriteRenderer.sprite based on connections.
        
        // Example:
        // Building neighbor = GridManager.Instance.GetBuildingAt(gridPosition + Vector2Int.up);
        // ... determine connectivity ...
        
        // For now, just rotate based on direction
        if (spriteRenderer != null)
        {
            // Reset rotation because Base.Place might have set it
            // transform.rotation = Quaternion.Euler(0, 0, -90 * (int)direction);
            // Actually, if we use different sprites for corners, we might handle rotation differently.
        }
    }

    public override void OnTick()
    {
        // Move items along the belt
        float moveAmount = speed * 0.1f; // speed * tickInterval
        for (int i = items.Count - 1; i >= 0; i--)
        {
            items[i].progress += moveAmount;

            // If item reaches the end of the belt, try to move it to the next one
            if (items[i].progress >= 1.0f)
            {
                Building nextBuilding = GridManager.Instance.GetBuildingAt(gridPosition + GetVectorForDirection(direction));
                if (nextBuilding is IItemReceiver receiver)
                {
                    ItemStack itemStack = new ItemStack { item = items[i].data, count = 1 };
                    if (receiver.TryReceiveItem(itemStack))
                    {
                        items.RemoveAt(i);
                    }
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
        
        items.Remove(itemToTake);
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
