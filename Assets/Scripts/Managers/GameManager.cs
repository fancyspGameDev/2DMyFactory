using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings")]
    public Building[] buildingPrefabs; // [0]:Belt, [1] inserter, [2]:Source, [3]:Sink, [4]:Smelter
    public ItemData testItemToProduce; // 테스트용 아이템 (Source에서 생성할 아이템)
    public RecipeData testRecipe; // 초기 테스트용 레시피
    public RecipeData[] testRecipes; // 런타임 교체용 레시피 목록

    [Header("Game State")]
    public int currentBuildingIndex = 0; // 현재 선택된 건물
    public int score = 0;
    private Smelter trackedSmelter; // 테스트용으로 생성된 Smelter 참조

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 게임 시작 시 자동으로 테스트 공장을 짓습니다.
        GenerateTestLayout();
    }

    private void Update()
    {
        // 게임 도중 'T' 키를 누르면 테스트 맵을 다시 시도합니다.
        if (Input.GetKeyDown(KeyCode.T))
        {
            GenerateTestLayout();
        }

        // 'R' 키를 누르면 Smelter의 레시피를 다음 것으로 변경합니다.
        if (Input.GetKeyDown(KeyCode.R))
        {
            SwapSmelterRecipe();
        }
    }

    private void SwapSmelterRecipe()
    {
        if (trackedSmelter == null || testRecipes == null || testRecipes.Length == 0)
        {
            Debug.LogWarning("교체할 Smelter가 없거나 레시피 목록이 비어있습니다.");
            return;
        }

        // 현재 레시피의 인덱스를 찾습니다.
        int currentIndex = -1;
        for (int i = 0; i < testRecipes.Length; i++)
        {
            if (trackedSmelter.currentRecipe == testRecipes[i])
            {
                currentIndex = i;
                break;
            }
        }

        // 다음 인덱스로 이동 (순환)
        int nextIndex = (currentIndex + 1) % testRecipes.Length;
        RecipeData newRecipe = testRecipes[nextIndex];

        // 레시피 변경
        trackedSmelter.currentRecipe = newRecipe;
        
        // 중요: 레시피가 바뀌면 기존 재료가 맞지 않아 막힐 수 있으므로 인벤토리를 초기화해줍니다.
        trackedSmelter.inputInventory.Clear();
        trackedSmelter.outputInventory.Clear();
        trackedSmelter.productionProgress = 0f;

        Debug.Log($"Smelter 레시피 변경됨: {newRecipe.name} (인벤토리 초기화됨)");
    }

    // ★ [수정됨] Inserter 포함 테스트 레이아웃 생성
    public void GenerateTestLayout()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length < 5) return;
        GridManager.Instance.ClearGrid();

        int idxBelt = 0;
        int idxInserter = 1;
        int idxSource = 2;
        int idxSink = 3;
        int idxSmelter = 4;

        // --- Segment 1: Source to Smelter ---
        // Source -> Inserter -> Belt (W->E) 3개
        Building src = PlaceTestBuilding(10, 10, idxSource, Vector2Int.right);
        if (src is Source source) source.itemToProduce = testItemToProduce;
        
        PlaceTestBuilding(11, 10, idxInserter, Vector2Int.right);
        for (int i = 0; i < 3; i++)
        {
            PlaceTestBuilding(12 + i, 10, idxBelt, Vector2Int.right);
        }

        // Belt (E->N) 3개
        for (int i = 0; i < 3; i++)
        {
            PlaceTestBuilding(15, 10 + i, idxBelt, Vector2Int.up);
        }

        // Inserter -> Smelter -> Inserter
        PlaceTestBuilding(15, 13, idxInserter, Vector2Int.up);
        Building sml = PlaceTestBuilding(15, 14, idxSmelter, Vector2Int.up);
        if (sml is Smelter smelter)
        {
            smelter.currentRecipe = testRecipe;
            trackedSmelter = smelter;
        }
        PlaceTestBuilding(15, 15, idxInserter, Vector2Int.up);

        // --- Segment 2: Smelter to Sink ---
        // Belt (S->N) 2개
        for (int i = 0; i < 2; i++)
        {
            PlaceTestBuilding(15, 16 + i, idxBelt, Vector2Int.up);
        }

        // Belt (W->E) 8개
        for (int i = 0; i < 8; i++)
        {
            PlaceTestBuilding(15 + i, 18, idxBelt, Vector2Int.right);
        }

        // Belt (E->S) 3개
        for (int i = 0; i < 3; i++)
        {
            PlaceTestBuilding(23, 18 - i, idxBelt, Vector2Int.down);
        }

        // Belt (S->W) 3개
        for (int i = 0; i < 3; i++)
        {
            PlaceTestBuilding(23 - i, 15, idxBelt, Vector2Int.left);
        }

        // Inserter -> Sink
        PlaceTestBuilding(20, 15, idxInserter, Vector2Int.left);
        PlaceTestBuilding(19, 15, idxSink, Vector2Int.left);

        Debug.Log("Full Production Line Generated: Source -> Belts -> Smelter -> North Belts -> Long East Belts -> South Belts -> West Belts -> Sink.");
    }

    // GridManager를 호출하여 건물을 짓는 내부 함수
    private Building PlaceTestBuilding(int x, int y, int prefabIndex, Vector2Int dir)
    {
        if (prefabIndex < 0 || prefabIndex >= buildingPrefabs.Length)
        {
            Debug.LogError($"GameManager: Invalid prefab index {prefabIndex}. Array length is {buildingPrefabs.Length}.");
            return null;
        }

        Building prefab = buildingPrefabs[prefabIndex];
        if (prefab == null)
        {
            Debug.LogError($"GameManager: Building prefab at index {prefabIndex} is null! Check inspector.");
            return null;
        }

        return GridManager.Instance.PlaceBuilding(x, y, prefab, dir);
    }

    // 건물 선택 변경
    public void SetBuildingIndex(int index)
    {
        if (index >= 0 && index < buildingPrefabs.Length)
        {
            currentBuildingIndex = index;
            Debug.Log($"Selected Building: {buildingPrefabs[index].name}");
        }
    }

    // 현재 선택된 프리팹 가져오기
    public Building GetCurrentBuildingPrefab()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return null;
        return buildingPrefabs[currentBuildingIndex];
    }

    // 점수 추가
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"Current Score: {score}");
    }
}