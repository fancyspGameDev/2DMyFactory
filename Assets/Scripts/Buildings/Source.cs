using UnityEngine;

public class Source : Building, IItemSource
{
    [Header("Source Settings")]
    public ItemData itemToProduce;
    public float spawnInterval = 1.0f;

    private float spawnTimer = 0f;

    public override void OnTick()
    {
        if (spawnTimer < spawnInterval)
        {
            spawnTimer += 0.1f;
        }
    }

    // In "infinite" mode, a source can always provide its designated item.
    public ItemStack TakeItem()
    {
        if (itemToProduce == null)
        {
            return default;
        }

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            return new ItemStack { item = itemToProduce, count = 1 };
        }

        return default;
    }
}
