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
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // [추가] 직접 키 입력을 통한 보강 (WSDF/WASD 모두 지원)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.F)) h = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1;

        if (h != 0 || v != 0)
        {
            // 이동 방향 계산
            Vector3 direction = new Vector3(h, v, 0).normalized;

            // 현재 위치에 더하기
            Vector3 newPos = transform.position + direction * moveSpeed * Time.deltaTime;

            // 맵 밖으로 나가지 않게 가두기 (Clamp)
            newPos.x = Mathf.Clamp(newPos.x, minLimit.x, maxLimit.x);
            newPos.y = Mathf.Clamp(newPos.y, minLimit.y, maxLimit.y);

            if (transform.position != newPos)
            {
                transform.position = newPos;
                //Debug.Log($"Camera Moving: {transform.position}, Input: h={h}, v={v}");
            }
        }
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