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
        // 프리팹 확인
        if (buildingPrefabs == null || buildingPrefabs.Length < 5)
        {
            Debug.LogError("Building Prefabs에 최소 5개의 건물이 등록되어야 합니다. (0:Belt, 1:Inserter, 2:Source, 3:Sink, 4:Smelter)");
            return;
        }

        // 인덱스 매핑: [0]:Belt, [1]:Inserter, [2]:Source, [3]:Sink, [4]:Smelter
        int idxBelt = 0;
        int idxInserter = 1;
        int idxSource = 2;
        int idxSink = 3;
        int idxSmelter = 4;

        int startX = 20;
        int startY = 20;
        int currentX = startX;

        // 배치 시나리오: Source -> Inserter -> Belt (4개) -> Inserter -> Smelter -> Inserter -> Belt (4개) -> Inserter -> Sink
        
        // 1. Source (입력 창고)
        Building sourceBuilding = PlaceTestBuilding(currentX, startY, idxSource, Vector2Int.right);
        if (sourceBuilding is Source source)
        {
            source.itemToProduce = testItemToProduce;
        }
        currentX++;

        // 2. Inserter (Source -> Belt Start)
        PlaceTestBuilding(currentX, startY, idxInserter, Vector2Int.right);
        currentX++;

        // 3. Belt 4개
        for (int i = 0; i < 4; i++)
        {
            PlaceTestBuilding(currentX, startY, idxBelt, Vector2Int.right);
            currentX++;
        }

        // 4. Inserter (Belt End -> Smelter)
        PlaceTestBuilding(currentX, startY, idxInserter, Vector2Int.right);
        currentX++;

        // 5. Smelter (가공)
        Building smelterBuilding = PlaceTestBuilding(currentX, startY, idxSmelter, Vector2Int.right);
        if (smelterBuilding is Smelter smelter)
        {
            smelter.currentRecipe = testRecipe;
            trackedSmelter = smelter; // 참조 저장
        }
        currentX++;

        // 6. Inserter (Smelter -> Belt Start)
        PlaceTestBuilding(currentX, startY, idxInserter, Vector2Int.right);
        currentX++;

        // 7. Belt 4개 (생산물 이동)
        for (int i = 0; i < 4; i++)
        {
            PlaceTestBuilding(currentX, startY, idxBelt, Vector2Int.right);
            currentX++;
        }

        // 8. Inserter (Belt End -> Sink)
        PlaceTestBuilding(currentX, startY, idxInserter, Vector2Int.right);
        currentX++;

        // 9. Sink (소모)
        PlaceTestBuilding(currentX, startY, idxSink, Vector2Int.right);

        Debug.Log("확장된 테스트 공장 배치 완료! (Source -> Inserter -> Belt(4) -> Inserter -> Smelter -> Inserter -> Belt(4) -> Inserter -> Sink)");
    }

    // GridManager를 호출하여 건물을 짓는 내부 함수
    private Building PlaceTestBuilding(int x, int y, int prefabIndex, Vector2Int dir)
    {
        Building prefab = buildingPrefabs[prefabIndex];
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