using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemStack
{
    public ItemData item;
    public int count;
}

[CreateAssetMenu(fileName = "Recipe_", menuName = "Data/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Info")]
    public string id;
    public string displayName;

    [Header("Crafting")]
    public List<ItemStack> ingredients;
    public List<ItemStack> products;
    public float craftingTime = 1f;
}
