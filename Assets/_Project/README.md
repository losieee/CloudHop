# CLOUD HOP — 1차 플레이 프로토타입

> 2차 고정 스테이지 레벨링은 `Scenes/CloudHopStages.unity`에서 실행한다.
> 쉬움 40 / 보통 48 / 어려움 56점프의 3개 맵과 선택·클리어·다음 스테이지를 추가했다.
> 상세 구간과 편집 방법은 [STAGES.md](STAGES.md)를 참고한다. 아래 설명은 기존 1차 씬 기준이다.

## 프로젝트 분석 및 구현 방침

- 확인한 버전: Unity 6000.3.16f1, URP 17.3.0의 2D Renderer, Input System 1.19.0, uGUI 2.0.0.
- Active Input Handling = Input System, 기본 편집 모드 = 2D, Physics2D 중력 = (0, -9.81).
- 기존 씬: `Assets/Scenes/SampleScene.unity`, `Assets/Settings/Scenes/URP2DSceneTemplate.unity`.
- 기존 게임 C# 스크립트 없음. 기본 InputSystem_Actions와 렌더링 설정 및 SampleScene 보존.
- 외부 패키지 추가 없음. 새 기능은 `_Project` 및 `CloudHop` 네임스페이스에 배치.
- 입력 → 플레이어 → 게임 이벤트 → 점수/UI 구조. Singleton 없음.

## 실행

1. `Assets/_Project/Scenes/CloudHopPrototype.unity`를 연다.
2. Play를 누르고 Game 뷰를 클릭한다.
3. Space 또는 마우스 왼쪽 버튼을 누른 채 충전한 후 놓는다. 첫 발판은 약 0.5초 충전으로 도달한다.
4. 새 발판에 착지하면 SCORE가 1 증가한다. 착지 후에는 다음 입력을 기다린다.
5. 점프를 짧게/길게 조절해 발판을 놓치고 Y < -6까지 떨어지면 결과 패널을 확인한다.
6. RETRY를 클릭한다. 점수, 위치, 카메라, 충전, 게임 상태가 초기화되며 BEST는 유지된다.

두 입력을 동시에 누르면 둘 다 놓아야 점프한다. 공중 입력은 무시한다. 포커스 상실은 충전을 취소한다.
마지막 발판 이후에는 맵이 없으므로 추락한다. 현재 12개 발판은 유한한 테스트 코스다.

## Inspector

씬과 프리팹의 참조는 생성 시 모두 연결된다. 수동 연결은 필요 없다.

| 대상 | 주요 설정 |
| --- | --- |
| Player / PlayerController | Minimum/Maximum Charge Time: 0.05 / 1초, Minimum/Maximum Jump Speed: 5 / 11, Horizontal Speed: 5, Fall Threshold: -6 |
| Player / Rigidbody2D | Gravity Scale: 2, Continuous 충돌, Interpolate, Z 회전 고정 |
| Player / BoxCollider2D | 0.6 × 0.9. 스킨과 독립적으로 유지 |
| Player / Visual | CharacterVisual의 Character에 CharacterData 할당. SpriteRenderer와 Animator는 자식에만 존재 |
| Platform 프리팹 | Transform Scale로 너비와 높이 조절. BoxCollider2D가 함께 변화. 시작 발판 Awards Score만 false |
| Main Camera / FollowCamera | Target = Player, Look Ahead = 3, Fixed Y = 1, Smooth Time = 0.2 |
| Game Systems / GameManager | Player, Scores, Spawn Point, Follow Camera |
| Prototype UI / PrototypeHUD | Game, Scores, Player, Score/Result/Charge Text, Game Over Panel, Retry Button |

점프 값은 힘(뉴턴)이 아닌 초기 속도다. 질량 변화가 점프 감각에 영향을 주지 않도록 `linearVelocity`를 설정한다.
카메라는 Y를 고정하고 X만 따라간다. 충전 최댓값에 도달해도 자동 점프하지 않는다.

## 파일과 책임

- `Scripts/Core/GameManager.cs`: Ready → Playing → GameOver, 이벤트 연결, 재시도 조정.
- `Scripts/Core/ScoreManager.cs`: 방문 발판 중복 방지, 현재/최고 점수, 점수 변경 이벤트.
- `Scripts/Core/FollowCamera.cs`: 수평 추적과 재시도 위치 복원.
- `Scripts/Player/ChargeInput.cs`: 기존 Input System을 이용한 키보드/마우스 입력, UI 클릭 차단, 포커스/입력 초기화.
- `Scripts/Player/PlayerController.cs`: 충전 보간, FixedUpdate 점프, 상향 접촉 법선 기반 착지, 추락 감지.
- `Scripts/Player/CharacterVisual.cs`: 스킨의 Sprite/Animator/색상 적용. 물리 성능에 관여하지 않음.
- `Scripts/Platform/Platform.cs`: 발판 식별 및 점수 부여 여부.
- `Scripts/Data/CharacterData.cs`: 능력치가 없는 외형 전용 ScriptableObject.
- `Scripts/Save/LocalScoreStore.cs`: PlayerPrefs 기반 최고 점수 영속 저장. 키 `CloudHop.BestScore.v1`.
- `Scripts/UI/PrototypeHUD.cs`: 이벤트 기반 점수/결과 표시, 충전 안내, Retry 버튼.
- `Editor/PrototypeBuilder.cs`: 씬/리소스 생성 도구. 기존 프로토타입 씬이 있으면 덮어쓰지 않음.
- `Tests/Editor/PrototypePlayTests.cs`: Editor Test Runner에서 실제 Play Mode에 진입하는 통합 테스트.
- asmdef 3개: 런타임, Editor 도구, 테스트 의존성 분리. Editor 코드/테스트는 게임 빌드에 포함되지 않음.

생성 리소스: `Scenes/CloudHopPrototype.unity`, `Prefabs/Characters/Player.prefab`,
`Prefabs/Platforms/Platform.prefab`, `ScriptableObjects/PrototypeCharacter.asset`,
`Art/PrototypeSquare.png`, `Art/PrototypeUnlit.mat`, 각 파일의 Unity `.meta`.

## 검증 및 빌드

Window > General > Test Runner > EditMode에서 CloudHop.Tests 실행.
테스트는 실제 씬을 열고 Play Mode에서 물리, Space/Mouse 입력, 점수, 게임 오버, 재시도 두 번째 실행을 검증한다.
테스트 실행 전 현재 씬의 편집 내용을 저장한다. 테스트는 최고 점수 저장 키를 실행 전 값으로 복원한다.

2026-09-22 검증: Unity 6000.3.16f1 배치 컴파일 성공. 실제 Play Mode 진입 통합 테스트 3개 통과.
Space 충전 → 착지/점수 → 자연 추락 → Game Over → 마우스 Retry 클릭 → 두 번째 착지,
마우스 짧은/최대 충전 및 공중 입력 무시, 두 입력 동시 사용 및 충전 취소를 확인했다.
배치 실행은 화면 포커스가 없으므로 테스트에서만 InputSettings 복사본을 사용하며 원본 설정은 복원한다.
렌더링 화면의 시각적 검수와 배포 빌드 테스트는 수행하지 않았다.

기존 Build Settings의 SampleScene 등록은 보존했다. Windows/Web 빌드 시 Build Profiles의 Scene List에
CloudHopPrototype을 추가하고 첫 실행 씬으로 배치한다. 다른 씬에서 실행하면 프로토타입은 자동 시작되지 않는다.

## 범위와 다음 단계

미구현: 이름 등록/Local TOP 10, 선택 UI와 실제 캐릭터 3종, 무한/랜덤 맵, 최종 아트/사운드, 온라인 기능.
다음 단계: 타깃 해상도에서 점프 감각 조정 → 도달 가능한 발판 규칙/생성 → JSON 로컬 TOP 10 → 스킨 선택 UI.

주의: WebGL PlayerPrefs는 브라우저/사이트별 로컬 저장이며 사이트 데이터 삭제 시 소실된다.
이번 착지 로직은 정적인 BoxCollider2D 발판 기준이다. 이동/회전/일방통행 발판을 도입할 때 접촉 처리를 확장한다.
WebGL/Windows 실제 플레이어 빌드와 Chrome 브라우저 수동 검증은 별도로 필요하다.

## 기존 파일 변경

기존 게임 씬/입력 액션/패키지/Build Settings는 수정하지 않았다.
Unity 실행 중 `ProjectSettings/SceneTemplateSettings.json`이 자동 생성되었으며 Library 캐시도 갱신됐다.
이 프로젝트에는 Git 저장소가 없으므로 다음 작업 전에 버전 관리 시작을 권장한다.
