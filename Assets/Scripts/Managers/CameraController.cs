using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;       // 카메라 이동 속도
    public float zoomSpeed = 5f;        // 줌 속도
    public float minZoom = 3f;          // 줌인 최대 한계 (작을수록 확대)
    public float maxZoom = 20f;         // 줌아웃 최대 한계

    [Header("Map Limits")]
    // 카메라가 맵 밖으로 너무 멀리 나가지 않게 제한 (50x50 맵 기준)
    public Vector2 minLimit = new Vector2(-5, -5);
    public Vector2 maxLimit = new Vector2(55, 55);

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

  
    void Update()
    {
        Move();
        Zoom();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A, D 또는 좌우 화살표
        float v = Input.GetAxisRaw("Vertical");   // W, S 또는 상하 화살표

        // 이동 방향 계산
        Vector3 direction = new Vector3(h, v, 0).normalized;

        // 현재 위치에 더하기
        Vector3 newPos = transform.position + direction * moveSpeed * Time.deltaTime;

        // 맵 밖으로 나가지 않게 가두기 (Clamp)
        newPos.x = Mathf.Clamp(newPos.x, minLimit.x, maxLimit.x);
        newPos.y = Mathf.Clamp(newPos.y, minLimit.y, maxLimit.y);

        transform.position = newPos;
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // OrthographicSize 조절 (작아지면 확대, 커지면 축소)
            cam.orthographicSize -= scroll * zoomSpeed;

            // 너무 확대되거나 축소되지 않게 제한
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}