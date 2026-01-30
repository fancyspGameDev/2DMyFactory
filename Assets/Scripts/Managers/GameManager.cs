using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings")]
    public Building[] buildingPrefabs; // [0]:Belt, [1]:Source, [2]:Sink, [3]:Smelter

    [Header("Game State")]
    public int currentBuildingIndex = 0; // 현재 선택된 건물
    public int score = 0;

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
    }

    // ★ [수정됨] 꺾인 경로(Snake Path) 테스트 레이아웃 생성
    public void GenerateTestLayout()
    {
        // 프리팹 확인
        if (buildingPrefabs == null || buildingPrefabs.Length < 4)
        {
            Debug.LogError("Building Prefabs에 최소 4개의 건물이 등록되어야 합니다. (0:Belt, 1:Source, 2:Sink, 3:Smelter)");
            return;
        }

        // 인덱스 매핑: [0]:Belt, [1]:Source, [2]:Sink, [3]:Smelter
        int idxBelt = 0;
        int idxSource = 1;
        int idxSink = 2;
        int idxSmelter = 3;

        int startX = 20;
        int startY = 20;

        // 1. Source (입력 창고) - 위치: (20, 20), 방향: 오른쪽
        PlaceTestBuilding(startX, startY, idxSource, Vector2Int.right);

        // 2. Belt (오른쪽 이동) - 위치: (21, 20)
        PlaceTestBuilding(startX + 1, startY, idxBelt, Vector2Int.right);

        // 3. Belt (위로 꺾기!) - 위치: (22, 20), 방향: 위(Up)
        // 아이템이 (21,20)에서 들어와서 위(22,21)로 나갑니다.
        PlaceTestBuilding(startX + 2, startY, idxBelt, Vector2Int.up);

        // 4. Belt (위로 이동) - 위치: (22, 21), 방향: 위(Up)
        PlaceTestBuilding(startX + 2, startY + 1, idxBelt, Vector2Int.up);

        // 5. Belt (오른쪽으로 다시 꺾기!) - 위치: (22, 22), 방향: 오른쪽(Right)
        // 아이템이 아래(22,21)에서 들어와서 오른쪽(23,22)으로 나갑니다.
        PlaceTestBuilding(startX + 2, startY + 2, idxBelt, Vector2Int.right);

        // 6. Smelter (제련기) - 위치: (23, 22), 방향: 오른쪽
        PlaceTestBuilding(startX + 3, startY + 2, idxSmelter, Vector2Int.right);

        // 7. Belt (오른쪽 이동) - 위치: (24, 22)
        PlaceTestBuilding(startX + 4, startY + 2, idxBelt, Vector2Int.right);

        // 8. Belt (아래로 꺾기!) - 위치: (25, 22), 방향: 아래(Down)
        PlaceTestBuilding(startX + 5, startY + 2, idxBelt, Vector2Int.down);

        // 9. Belt (아래로 이동) - 위치: (25, 21), 방향: 아래(Down)
        PlaceTestBuilding(startX + 5, startY + 1, idxBelt, Vector2Int.down);

        // 10. Belt (오른쪽으로 마지막 꺾기) - 위치: (25, 20), 방향: 오른쪽(Right)
        PlaceTestBuilding(startX + 5, startY, idxBelt, Vector2Int.right);

        // 11. Sink (출고 창고) - 위치: (26, 20)
        PlaceTestBuilding(startX + 6, startY, idxSink, Vector2Int.right);

        Debug.Log("S자 굴곡 테스트 공장 배치 완료! (시작위치: 20, 20)");
    }

    // GridManager를 호출하여 건물을 짓는 내부 함수
    private void PlaceTestBuilding(int x, int y, int prefabIndex, Vector2Int dir)
    {
        Building prefab = buildingPrefabs[prefabIndex];
        GridManager.Instance.PlaceBuilding(x, y, prefab, dir);
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