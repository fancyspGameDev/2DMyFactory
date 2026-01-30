using UnityEngine;

public interface IItemSource
{
    ItemStack TakeItem();
}

public class Source : Building, IItemSource
{
    [Header("Source Settings")]
    public ItemData itemToProduce;

    // In "infinite" mode, a source can always provide its designated item.
    public ItemStack TakeItem()
    {
        if (itemToProduce == null)
        {
            return default;
        }
        return new ItemStack { item = itemToProduce, count = 1 };
    }
}
