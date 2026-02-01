using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform ghostObject; // Ghost Object for placement preview
    private SpriteRenderer ghostRenderer;

    // Current rotation direction
    private Vector2Int currentDirection = Vector2Int.right;

    private void Start()
    {
        // Init Ghost
        if (ghostObject != null)
        {
            ghostRenderer = ghostObject.GetComponent<SpriteRenderer>();
            // Semi-transparent
            ghostRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

    void Update()
    {
        HandleInput();
        UpdateGhost();
    }

    void HandleInput()
    {
        // Prevent interaction if pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(mouseWorldPos.x);
        int y = Mathf.RoundToInt(mouseWorldPos.y);

        // 1. Left Click (Place or Select)
        if (Input.GetMouseButtonDown(0))
        {
            Building existingBuilding = GridManager.Instance.GetBuildingAt(new Vector2Int(x, y));
            
            if (existingBuilding != null)
            {
                // Interaction with existing building
                if (existingBuilding is Smelter smelter)
                {
                    if (RecipeSelectUI.Instance != null)
                    {
                        RecipeSelectUI.Instance.Open(smelter);
                    }
                }
                // Add other interactions here
            }
            else
            {
                // Place new building
                Building prefab = GameManager.Instance.GetCurrentBuildingPrefab();
                if (prefab != null)
                {
                    GridManager.Instance.PlaceBuilding(x, y, prefab, currentDirection);
                }
            }
        }

        // 2. Right Click (Remove)
        if (Input.GetMouseButtonDown(1))
        {
            GridManager.Instance.RemoveBuilding(x, y);
        }

        // 3. Rotate (R Key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateDirection();
        }

        // 4. Hotkeys for building selection
        if (Input.GetKeyDown(KeyCode.Alpha1)) GameManager.Instance.SetBuildingIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) GameManager.Instance.SetBuildingIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) GameManager.Instance.SetBuildingIndex(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) GameManager.Instance.SetBuildingIndex(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) GameManager.Instance.SetBuildingIndex(4);
    }

    private void RotateDirection()
    {
        currentDirection = new Vector2Int(currentDirection.y, -currentDirection.x);
    }

    // Update Ghost Visuals
    private void UpdateGhost()
    {
        if (ghostObject == null || ghostRenderer == null) return;

        // 1. Position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(mousePos.x);
        int y = Mathf.RoundToInt(mousePos.y);

        ghostObject.position = new Vector3(x, y, 0);

        // 2. Sprite
        Building currentPrefab = GameManager.Instance.GetCurrentBuildingPrefab();
        if (currentPrefab != null)
        {
            SpriteRenderer prefabRenderer = currentPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabRenderer != null)
            {
                ghostRenderer.sprite = prefabRenderer.sprite;
            }
        }

        // 3. Rotation
        float rotZ = 0;
        if (currentDirection == Vector2Int.right) rotZ = 0;
        else if (currentDirection == Vector2Int.down) rotZ = -90;
        else if (currentDirection == Vector2Int.left) rotZ = 180;
        else if (currentDirection == Vector2Int.up) rotZ = 90;

        ghostObject.rotation = Quaternion.Euler(0, 0, rotZ);
    }
}
