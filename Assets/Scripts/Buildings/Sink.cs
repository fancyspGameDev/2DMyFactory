using UnityEngine;

public class Sink : Building, IItemReceiver
{
    // A sink can receive any item and simply destroys it.
    public bool TryReceiveItem(ItemStack item)
    {
        // Always returns true, as it has infinite capacity.
        Debug.Log($"Sunk: {item.count} of {item.item.displayName}");
        return true;
    }
}
