using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Machine : Building, IItemReceiver, IItemSource
{
    [Header("Machine State")]
    public RecipeData currentRecipe;
    public float productionProgress;

    [Header("Inventory")]
    public List<ItemStack> inputInventory = new List<ItemStack>();
    public List<ItemStack> outputInventory = new List<ItemStack>();
    
    // Abstract property to define inventory capacity
    protected abstract int InputCapacity { get; }
    protected abstract int OutputCapacity { get; }

    public override void OnTick()
    {
        if (currentRecipe == null) return;

        if (CanProduce())
        {
            productionProgress += 0.1f; // Corresponds to TickManager interval
            if (productionProgress >= currentRecipe.craftingTime)
            {
                Produce();
                productionProgress = 0f;
            }
        }
    }
    
    public override void GetSaveData(BuildingSaveData data)
    {
        base.GetSaveData(data);
        data.recipeId = currentRecipe != null ? currentRecipe.id : null;
        
        if (inputInventory != null)
        {
            data.inputInventory = inputInventory
                .Where(s => s.item != null)
                .Select(s => new InventoryItemSaveData { id = s.item.id, count = s.count })
                .ToList();
        }
        
        if (outputInventory != null)
        {
            data.outputInventory = outputInventory
                .Where(s => s.item != null)
                .Select(s => new InventoryItemSaveData { id = s.item.id, count = s.count })
                .ToList();
        }
    }

    public override void LoadSaveData(BuildingSaveData data)
    {
        base.LoadSaveData(data);
        currentRecipe = !string.IsNullOrEmpty(data.recipeId) ? SaveManager.Instance.GetRecipeDataById(data.recipeId) : null;
        
        if (data.inputInventory != null)
        {
            inputInventory = data.inputInventory
                .Select(s => new ItemStack { item = SaveManager.Instance.GetItemDataById(s.id), count = s.count })
                .Where(s => s.item != null)
                .ToList();
        }
        
        if (data.outputInventory != null)
        {
            outputInventory = data.outputInventory
                .Select(s => new ItemStack { item = SaveManager.Instance.GetItemDataById(s.id), count = s.count })
                .Where(s => s.item != null)
                .ToList();
        }
    }

    private bool CanProduce()
    {
        if (currentRecipe == null) return false;
        
        // Check if output is full
        int currentOutputCount = outputInventory.Sum(itemStack => itemStack.count);
        int recipeOutputCount = currentRecipe.products.Sum(itemStack => itemStack.count);
        if (currentOutputCount + recipeOutputCount > OutputCapacity)
        {
            return false;
        }

        // Check for required ingredients
        foreach (var required in currentRecipe.ingredients)
        {
            var found = inputInventory.FirstOrDefault(i => i.item == required.item);
            if (found.item == null || found.count < required.count)
            {
                return false;
            }
        }

        return true;
    }

    private void Produce()
    {
        // Consume ingredients
        foreach (var required in currentRecipe.ingredients)
        {
            int index = inputInventory.FindIndex(i => i.item == required.item);
            if (index != -1)
            {
                ItemStack stack = inputInventory[index];
                stack.count -= required.count;
                
                if (stack.count <= 0)
                {
                    inputInventory.RemoveAt(index);
                }
                else
                {
                    inputInventory[index] = stack; // Update struct in list
                }
            }
        }

        // Add products to output
        foreach (var product in currentRecipe.products)
        {
            int index = outputInventory.FindIndex(i => i.item == product.item);
            if (index != -1)
            {
                ItemStack stack = outputInventory[index];
                stack.count += product.count;
                outputInventory[index] = stack;
            }
            else
            {
                outputInventory.Add(new ItemStack { item = product.item, count = product.count });
            }
        }
    }

    public bool TryReceiveItem(ItemStack item)
    {
        // Only accept items that are part of the current recipe's ingredients
        if (currentRecipe == null || !currentRecipe.ingredients.Any(ing => ing.item == item.item))
        {
            return false;
        }
        
        int currentInputCount = inputInventory.Sum(itemStack => itemStack.count);
        if (currentInputCount + item.count > InputCapacity)
        {
            return false;
        }
        
        // Stack if possible
        int index = inputInventory.FindIndex(i => i.item == item.item);
        if (index != -1)
        {
            ItemStack stack = inputInventory[index];
            stack.count += item.count;
            inputInventory[index] = stack;
        }
        else
        {
            inputInventory.Add(item);
        }

        return true;
    }

    public ItemStack TakeItem()
    {
        // Only allow taking items from the output inventory
        if (outputInventory.Count > 0)
        {
            ItemStack itemToTake = outputInventory[0];
            outputInventory.RemoveAt(0);
            return itemToTake;
        }
        return default;
    }
}
