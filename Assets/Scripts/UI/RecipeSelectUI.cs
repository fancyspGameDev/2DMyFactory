using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecipeSelectUI : MonoBehaviour
{
    public static RecipeSelectUI Instance;

    [Header("UI References")]
    public GameObject uiPanel; // The entire UI panel to show/hide
    public Transform buttonContainer; // The parent object for recipe buttons
    public Button closeButton;
    public GameObject buttonPrefab; // A simple prefab with a Button and Text/Image

    private Smelter currentSmelter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (uiPanel != null) uiPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    public void Open(Smelter smelter)
    {
        currentSmelter = smelter;
        if (uiPanel != null) uiPanel.SetActive(true);
        GenerateButtons();
    }

    public void Close()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        currentSmelter = null;
    }

    private void GenerateButtons()
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Get recipes from GameManager
        if (GameManager.Instance.testRecipes == null || GameManager.Instance.testRecipes.Length == 0)
        {
            Debug.LogWarning("RecipeSelectUI: No recipes found in GameManager.testRecipes!");
            return;
        }

        Debug.Log($"RecipeSelectUI: Generating buttons for {GameManager.Instance.testRecipes.Length} recipes.");

        foreach (var recipe in GameManager.Instance.testRecipes)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            
            if (btn == null)
            {
                Debug.LogError("RecipeSelectUI: Button component missing on prefab!");
                continue;
            }

            // Set Text if available (assumed prefab structure)
            // Try finding Text (Legacy) or TextMeshProUGUI
            var legacyText = btnObj.GetComponentInChildren<Text>();
            if (legacyText != null) 
            {
                legacyText.text = recipe.displayName;
            }
            else
            {
                // Reflection or simple search for TMPro if legacy text isn't found, 
                // but since I can't import TMPro namespace easily without knowing if it's in asmdef,
                // I'll rely on the user having set up the right text component or check for "Text" name.
                var tmpros = btnObj.GetComponentsInChildren<Component>(true);
                foreach(var c in tmpros)
                {
                    if (c.GetType().Name == "TextMeshProUGUI")
                    {
                        // Use reflection to set text property to avoid compile errors if TMPro is missing from context
                        var prop = c.GetType().GetProperty("text");
                        if (prop != null) prop.SetValue(c, recipe.displayName);
                        break;
                    }
                }
            }

            // Add Click Listener
            btn.onClick.AddListener(() => OnRecipeSelected(recipe));
        }
    }

    private void OnRecipeSelected(RecipeData recipe)
    {
        if (currentSmelter != null)
        {
            currentSmelter.currentRecipe = recipe;
            
            // Clear inventory to prevent jamming
            currentSmelter.inputInventory.Clear();
            currentSmelter.outputInventory.Clear();
            currentSmelter.productionProgress = 0f;

            Debug.Log($"Recipe changed to {recipe.displayName} for Smelter at {currentSmelter.gridPosition}");
        }
        Close();
    }
}
