# [프로젝트: My Factory] 통합 개발 상세 명세서 (Final Ver 7.1 - Revised)

## 1. 프로젝트 개요

- **플랫폼/엔진**: PC / Unity (2D)
    
- **장르**: 공장 자동화 시뮬레이션 (샌드박스)
    
- **핵심 루프**: 자원 설정(Source) → 투입기(능동) → 벨트(수동) → 투입기 → 기계 가공(Recipe) → 납품
    
- **개발 목표**:
    
    - **Tick(Logic)과 Update(Visual)의 분리**: 데이터 연산과 렌더링의 완벽한 분리.
        
    - **능동적 물류**: 모든 아이템의 In/Out은 '투입기'가 주도.
        
    - **확장성**: JSON 기반의 유연한 데이터 구조 및 저장 시스템.
        

## 2. 맵 및 그리드 시스템 (Grid System)

### 2.1. 맵 데이터 구조

- **크기**: 50 x 50 (고정)
    
- **좌표계**: Vector2Int(x, y) 정수 좌표.
    
- **데이터 관리**: `Building[,]` 2차원 배열 + `List<Building>` (활성 객체 관리용).
    
- **[Update] 멀티 타일 처리 (Pivot Rule)**:
    
    - **기준점**: 모든 건물은 **좌측 하단(x, y)**을 기준점(Pivot)으로 함.
        
    - **점유 처리**: 2x2 건물 설치 시 (x, y)에는 실제 객체(Instance)를 저장하고, `(x+1, y)`, `(x, y+1)`, `(x+1, y+1)`에는 **참조(Reference) 혹은 더미(Dummy)** 값을 넣어 다른 건물이 겹쳐 건설되지 않도록 방어 로직 구현.
        

### 2.2. 조작 및 카메라 (Controls)

- **건설/제거**: 좌클릭 건설, 우클릭 제거 (비용 없음/샌드박스 모드).
    
- **회전**: 'R' 키 입력 시 시계 방향 90도 회전 (Ghost 상태에서도 반영).
    
- **설정 변경**: 설치된 건물(Source, Machine) 클릭 시 설정 UI 팝업.
    
- **카메라**: WASD 이동, 휠 줌인/아웃, 맵 밖 이동 제한(Clamp).
    

### 2.3. 시각화 (Visual Feedback)

- **고스트(Ghost)**: 마우스 커서 위치에 반투명 미리보기 + 설치 불가 지역(충돌) 시 붉은색 틴트(Red Tint).
    
- **UI 레이어**: 팝업 UI는 최상단에 위치하며, UI 활성화 시 맵 클릭 방지 (EventSystem 체크).
    

## 3. 핵심 게임 로직 (Core Logic)

### 3.1. 시간 및 이동 시스템 ([Update] Faster Tick)

- **Logic (OnTick)**
    
    - **주기**: **0.1초 (100ms)** (기존 0.2초에서 단축하여 부드러운 시뮬레이션 구현).
        
    - **역할**: 아이템의 논리적 위치 변경(`progress` 증가), 기계 생산 게이지 증가, 투입기 FSM 상태 변경.
        
- **Render (Update)**
    
    - **주기**: 매 프레임 (60 FPS+).
        
    - **보간(Interpolation)**: `Vector3.Lerp(currentPos, targetPos, t)`를 사용하여 0.1초 간격의 틱 사이를 부드럽게 연결.
        
    - **Anti-Teleport**: 한 틱에 아이템은 최대 1칸(혹은 정해진 거리)만 이동.
        

### 3.2. 세이브/로드 시스템 ([Update] Full State Save)

- **형식**: JSON 포맷.
    
- **[중요] 데이터 구조 개선**: 벨트 위 아이템의 위치 손실을 방지하기 위해 `items` 리스트 상세 저장.
    

JSON

```
{
  "buildings": [
    {
      "type": "Belt", "x": 10, "y": 10, "dir": 1,
      "items": [ 
          { "itemId": 101, "progress": 0.5 }, // 벨트 중간에 있는 아이템
          { "itemId": 101, "progress": 0.9 }  // 벨트 끝에 도달한 아이템
      ]
    },
    {
      "type": "Assembler", "x": 12, "y": 12, "dir": 0,
      "recipeId": "R3",
      "inventory": {
         "inputs": [ { "id": 201, "count": 2 } ],
         "outputs": [ { "id": 301, "count": 1 } ]
      }
    }
  ]
}
```

- **로드 프로세스 (Two-Pass)**:
    
    1. **Instantiate**: JSON을 파싱하여 모든 건물을 좌표에 생성 및 배치 (멀티 타일 점유 설정 포함).
        
    2. **Restore & Link**: 생성된 건물의 인벤토리/벨트 아이템 복구, 투입기의 앞/뒤 건물 참조 연결(Link).
        

## 4. 물류 및 생산 시스템 (Logistics)

### 4.1. 컨베이어 벨트 (Conveyor Belt)

- **역할**: 수동적 운송. 아이템을 스스로 밀지 않음(벨트 로직에 의해 아이템 좌표가 갱신됨).
    
- **[Update] Auto-Tiling**:
    
    - **비트마스크(Bitmask)**: 인접한 벨트(상하좌우)의 연결 상태를 비트 연산으로 체크.
        
    - **스프라이트 교체**: 상황에 맞춰 직선(Straight), 곡선(Corner), 교차(Cross) 스프라이트로 자동 시각화 변경.
        
    - **곡선 로직**: 시각적으로는 곡선이지만, 논리적으로는 `해당 타일 진입 → 방향 전환 → 다음 타일 진출` 순서로 처리.
        

### 4.2. 투입기 (Inserter) - [Update] 상세 FSM

- **구조**: Source Grid(집는 곳) / Destination Grid(놓는 곳).
    
- **FSM (상태 머신)**:
    
    1. **Idle**: 대기 (집을 물건 없음 / 놓을 곳 꽉 참).
        
    2. **MoveToPick**: 집게가 Source 방향으로 이동.
        
    3. **Pick**: 아이템 1개 획득 (Source 인벤토리 차감).
        
    4. **MoveToDrop**: 아이템을 들고 Destination 방향으로 회전/이동.
        
    5. **Drop**: 아이템 투입 (Destination 인벤토리/벨트에 추가).
        
- **[중요] 필터링 및 슬롯 규칙**:
    
    - **Take (가져오기)**: 기계의 **Output Slot(완성품)**에서만 아이템을 꺼낼 수 있음. (Input Slot 접근 불가)
        
    - **Give (넣기)**: 기계의 **Input Slot(재료)**에만 아이템을 넣을 수 있음. (Output Slot 접근 불가)
        

### 4.3. 생산 기계 (Smelter/Assembler)

- **입력 창고 (Source)**: 아이템 종류 선택, 무한 모드/버퍼 모드 설정.
    
- **조립기/제련기**:
    
    - **레시피 필수**: 레시피 미선택 시 작동 불가.
        
    - **슬롯 구분**: `inputInventory`와 `outputInventory`를 코드 레벨에서 분리하여 관리.
        

## 5. 데이터 명세 (Data Specs)

- **Items**: Iron Ore(101), Copper Ore(102), Iron Ingot(201), Copper Wire(204), Circuit(301) 등.
    
- **Recipes**:
    
    - R1 (Smelter): Iron Ore(1) → 2s → Iron Ingot(1)
        
    - R5 (Assembler): Iron Plate(1) + Copper Wire(2) → 5s → Circuit(1)
        

## 6. 그래픽 및 리소스 가이드 ([Update] Art Guide)

### 6.1. 스프라이트 규격

- **PPU**: 32 or 64.
    
- **레이어 (Sorting Order)**:
    
    - Order 0: 바닥 (Floor)
        
    - Order 1: 벨트 (Belt)
        
    - Order 2: 아이템 (Item)
        
    - Order 3: 건물 본체 (Machine Base), 투입기 바닥
        
    - Order 4: 투입기 팔 (Inserter Arm) **[중요: 아이템보다 위]**
        
    - Order 5: 비행 UI, 고스트
        

### 6.2. 리소스 제작 요청 사항

- **벨트**: 수직, 수평, 코너(4방향) - 총 6개 기본 타일.
    
- **투입기**: Base(고정부)와 Arm/Head(회전부) 이미지 분리.
    
- **아이템**: 벨트 위에서 식별 가능한 아이콘.
    

## 7. QA 및 예외 처리 규칙 (Edge Case Rules)

- **Merge (합류)**: 벨트 합류 시 기존 벨트 위의 아이템이 우선권(Priority)을 가짐. 진입 아이템 대기.
    
- **Full Output**: 기계 Output Slot이 꽉 차면 생산 즉시 중단(Pause).
    
- **Recipe Change**: 레시피 변경 시 내부 재료/생산품 반환 혹은 소멸 처리.
    
- **Disconnect**: 투입기 앞/뒤 건물 삭제 시 투입기는 즉시 Idle 전환 및 작업 중단.
    

## 8. [Action Item] 파트별 작업 우선순위

- **Programmer**:
    
    1. `TickManager` 0.1s 설정 및 최적화.
        
    2. `GridManager` 멀티 타일(Reference) 로직 구현.
        
    3. `Belt` Auto-Tiling 및 JSON 상세 저장(`ItemOnBelt`) 구현.
        
    4. `Inserter` FSM 및 Strict Input/Output 슬롯 접근 제한 구현.
        
- **Designer**:
    
    1. 벨트 연결부위(Corner) 스프라이트 제작.
        
    2. 투입기 분리형(Base/Arm) 리소스 제작.
