using System.Collections.Generic;
using UnityEngine;

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
