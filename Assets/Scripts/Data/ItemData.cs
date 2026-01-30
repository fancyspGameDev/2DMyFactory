using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public int id;
    public string displayName;

    [Header("Visuals")]
    public Sprite icon;
}
