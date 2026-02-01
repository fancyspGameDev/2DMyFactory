using UnityEngine;

public interface IItemReceiver
{
    bool TryReceiveItem(ItemStack item);
}
