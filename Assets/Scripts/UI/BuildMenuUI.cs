using UnityEngine;
using UnityEngine.UI;

public class BuildMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentTransform; // Scroll View의 Content
    public GameObject buttonPrefab;    // 버튼 프리팹

    private bool isExpanded = false;
    private GameObject toggleButton;

    private void Start()
    {
        // 1. Ensure UI is attached to a Canvas
        Canvas mainCanvas = null;
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        foreach (var c in canvases)
        {
            mainCanvas = c;
            break;
        }

        if (mainCanvas != null)
        {
            // 2. Set Render Mode to Screen Space - Camera
            mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            mainCanvas.worldCamera = Camera.main;
            mainCanvas.planeDistance = 5f; 
            
            // 3. Setup Canvas Scaler
            CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            mainCanvas.sortingLayerName = "UI"; 
            mainCanvas.sortingOrder = 100;

            if (transform.parent != mainCanvas.transform)
            {
                transform.SetParent(mainCanvas.transform, false);
            }
        }

        // 3. Force Bottom-Center Alignment for the LIST
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0); 
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 60); 
            rect.sizeDelta = new Vector2(600, 60); 
            rect.localScale = Vector3.one;
        }

        CreateToggleButton();
        RefreshMenu();
        
        // Initialize hidden
        SetMenuState(false);
    }

    private void CreateToggleButton()
    {
        if (buttonPrefab == null) return;

        // Create a toggle button outside the content list (as a sibling of this panel or child)
        // Since 'this' object is the panel with HorizontalLayout, we should probably put the toggle button 
        // as a child but ignore layout, OR create it on the parent. 
        // For simplicity, let's create it as a child but manage its position manually or put it in a separate container?
        // Actually, if we hide 'contentTransform' (which is usually 'this.transform' or a child), we hide the list.
        
        // Let's assume 'contentTransform' IS the list container. 
        // If 'contentTransform' == transform, then hiding it hides the script too? No, script is on GameObject.
        // Hiding GameObject stops Update/Coroutines usually, but button callbacks might work? 
        // Safer to separate: BuildMenuUI (Manager) -> [ToggleButton] , [BuildingListPanel]
        
        // Current setup: BuildMenuUI IS the panel.
        // Let's create the Toggle Button as a child of the Canvas directly to be independent, or child of this rect but anchored differently.
        
        toggleButton = Instantiate(buttonPrefab, transform.parent);
        toggleButton.name = "BuildMenu_Toggle";
        
        RectTransform btnRect = toggleButton.GetComponent<RectTransform>();
        if (btnRect != null)
        {
            btnRect.anchorMin = new Vector2(0.5f, 0); 
            btnRect.anchorMax = new Vector2(0.5f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 10); // Very bottom
            btnRect.sizeDelta = new Vector2(120, 40);
            btnRect.localScale = Vector3.one;
        }

        var btnText = toggleButton.GetComponentInChildren<Text>();
        if (btnText != null) btnText.text = "BUILD";
        else
        {
            var tmpText = toggleButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmpText != null) tmpText.text = "BUILD";
        }

        Button btn = toggleButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(ToggleMenu);
        }
    }

    public void ToggleMenu()
    {
        SetMenuState(!isExpanded);
    }

    private void SetMenuState(bool open)
    {
        isExpanded = open;
        
        // Show/Hide the list content
        // If contentTransform is assigned, use it. If it's this transform, be careful not to hide the toggle button if it was a child.
        // We parented ToggleButton to transform.parent (Canvas), so it's safe to hide 'gameObject' (this panel).
        
        // Visual toggle:
        // We can just enable/disable the Image and Loop children, or SetActive the whole object.
        // But if we SetActive(false) on 'this', the script might stop? 
        // Unity scripts on disabled GameObjects do NOT run Update, but public methods can be called? 
        // Actually, clicking the button calls ToggleMenu on THIS instance. If THIS gameObject is inactive, can we call it?
        // Yes, if the reference exists. BUT usually it's safer to just hide the visual components or scale to 0.
        // Let's just use CanvasGroup if available, or SetActive(false) on contentTransform if it is a child.
        
        if (contentTransform != null && contentTransform != transform)
        {
            contentTransform.gameObject.SetActive(open);
        }
        else
        {
            // Fallback: If contentTransform IS this object, we need a wrapper.
            // Assuming the user set contentTransform to 'this' or a child 'Content'.
            // Let's toggle the Image and LayoutGroup?
            
            // Simplest: Set gameObject.SetActive(open).
            // NOTE: If the toggle button calls this script, and this script is on the disabled object, it works as long as the EventSystem can target the toggle button (which is separate).
            gameObject.SetActive(open);
        }
    }

    public void RefreshMenu()
    {
        if (GameManager.Instance == null || contentTransform == null || buttonPrefab == null)
        {
            return; 
        }

        // Layout Group이 없으면 자동 추가
        VerticalLayoutGroup layout = contentTransform.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 10;
            layout.childControlWidth = false; 
            layout.childControlHeight = false;
        }

        // Content Size Fitter가 없으면 자동 추가
        ContentSizeFitter fitter = contentTransform.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // 기존 버튼 초기화
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // 버튼 생성
        Building[] buildings = GameManager.Instance.buildingPrefabs;
        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] == null) continue;

            int index = i;
            GameObject btnObj = Instantiate(buttonPrefab, contentTransform);
            
            // Support both standard Text and TextMeshPro
            string buildingName = buildings[i].GetType().Name;
            var btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = $"{index + 1}. {buildingName}";
                btnText.fontSize = 18;
            }
            else
            {
                var tmpText = btnObj.GetComponentInChildren<TMPro.TMP_Text>();
                if (tmpText != null)
                {
                    tmpText.text = $"{index + 1}. {buildingName}";
                    tmpText.fontSize = 14;
                }
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnBuildingClicked(index));
            }
        }
    }

    private void OnBuildingClicked(int index)
    {
        GameManager.Instance.SetBuildingIndex(index);
    }
}