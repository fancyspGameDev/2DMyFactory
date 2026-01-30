using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform ghostObject; // 미리보기용 오브젝트 (Scene에 있는 빈 오브젝트 연결)
    private SpriteRenderer ghostRenderer;

    // 현재 건설하려는 건물의 회전 방향
    private Vector2Int currentDirection = Vector2Int.right;

    private void Start()
    {
        // 고스트 오브젝트에서 렌더러 가져오기
        if (ghostObject != null)
        {
            ghostRenderer = ghostObject.GetComponent<SpriteRenderer>();
            // 반투명하게 설정 (알파값 0.5)
            ghostRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

    void Update()
    {
        HandleInput();
        UpdateGhost(); // 매 프레임 미리보기 갱신
    }

    void HandleInput()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(mouseWorldPos.x);
        int y = Mathf.RoundToInt(mouseWorldPos.y);

        // 1. 건설 (좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            Building prefab = GameManager.Instance.GetCurrentBuildingPrefab();
            if (prefab != null)
            {
                GridManager.Instance.PlaceBuilding(x, y, prefab, currentDirection);
            }
        }

        // 2. 제거 (우클릭)
        if (Input.GetMouseButtonDown(1))
        {
            GridManager.Instance.RemoveBuilding(x, y);
        }

        // 3. 회전 (R키)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateDirection();
        }

        // 4. 건물 선택 (숫자키) - 선택 시 바로 미리보기 갱신됨
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

    // ★ 핵심: 미리보기(Ghost) 위치 및 모양 업데이트
    private void UpdateGhost()
    {
        if (ghostObject == null || ghostRenderer == null) return;

        // 1. 마우스 위치 계산 (Grid 스냅)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(mousePos.x);
        int y = Mathf.RoundToInt(mousePos.y);

        // 2. 위치 이동
        ghostObject.position = new Vector3(x, y, 0);

        // 3. 현재 선택된 건물의 스프라이트 가져오기
        Building currentPrefab = GameManager.Instance.GetCurrentBuildingPrefab();
        if (currentPrefab != null)
        {
            // 프리팹 본체 혹은 자식에 있는 스프라이트 렌더러 찾기
            SpriteRenderer prefabRenderer = currentPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabRenderer != null)
            {
                ghostRenderer.sprite = prefabRenderer.sprite;
            }
        }

        // 4. 회전 적용
        float rotZ = 0;
        if (currentDirection == Vector2Int.right) rotZ = 0;
        else if (currentDirection == Vector2Int.down) rotZ = -90;
        else if (currentDirection == Vector2Int.left) rotZ = 180;
        else if (currentDirection == Vector2Int.up) rotZ = 90;

        ghostObject.rotation = Quaternion.Euler(0, 0, rotZ);
    }
}