using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public List<BuildingSaveData> buildings;
}

[Serializable]
public class BuildingSaveData
{
    public string type;
    public int x;
    public int y;
    public int dir;

    // Belt-specific data
    public List<ItemOnBeltSaveData> items;

    // Machine-specific data
    public string recipeId;
    public List<InventoryItemSaveData> inputInventory;
    public List<InventoryItemSaveData> outputInventory;
    
    // Inserter-specific data
    public InventoryItemSaveData heldItem;
}

[Serializable]
public class ItemOnBeltSaveData
{
    public int itemId;
    public float progress;
}

[Serializable]
public class InventoryItemSaveData
{
    public int id;
    public int count;
}
