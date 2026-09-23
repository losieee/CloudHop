# CLOUD HOP — 고정 스테이지 레벨링 1차

`Assets/_Project/Scenes/CloudHopStages.unity`를 열어 Play한다. 기존 `CloudHopPrototype` 씬은 비교용으로 유지했다.

## 길이와 난도

| 스테이지 | 점프 / 발판 | 총 길이 (중심 기준) | 일반 발판 폭 | 의도 |
| --- | --- | --- | --- | --- |
| 1 · SKY MEADOW · 쉬움 | 40 / 41 | 153.2 units | 2.6–3.2 | 넓은 발판, 완만한 높이 변화, 충전 감각 학습 |
| 2 · WIND RIDGE · 보통 | 48 / 49 | 194.3 units | 1.8–2.5 | 장거리와 정밀 착지 혼합, 높낮이 전환 |
| 3 · STORM SUMMIT · 어려움 | 56 / 57 | 228.2 units | 1.1–1.7 | 좁은 착지, 큰 높이 차, 충전 길이의 잦은 전환 |

한 스테이지 첫 성공까지 긴 호흡으로 플레이하도록 구성했다. 관찰 시간을 포함한 성공 1회의 목표는 대략 2–4분이며,
실제 사람의 플레이 시간·실패율을 측정한 수치는 아니다. 제한 시간이나 자동 진행은 없다.
난도 표기는 최초 튜닝 가설이며 사용자 플레이테스트 후 조정한다.

각 구간은 8점프. 8번째는 초록색의 넓은 휴식 발판, 마지막은 금색 도착 발판이다.
휴식 발판은 체크포인트가 아니다. 추락하면 현재 스테이지 처음부터 다시 시작한다.

## 구간 구성

- **1:** WARM UP → GENTLE STEPS → VALLEY HOPS → LONG AND SHORT → FIRST SUMMIT.
  기본 수평 점프 → 완만한 오르내림 → 골짜기 → 짧음/김 전환 → 종합.
- **2:** CHANGING RHYTHM → CLOUD STAIRS → WIDE CROSSINGS → SMALL ISLANDS → HIGH AND LOW → RIDGE FINALE.
  충전 변화 → 계단 → 긴 간격 → 좁은 착지 → 큰 높낮이 전환 → 종합.
- **3:** PRECISION → HIGH STEPS → WIDE GAPS → DOWNHILL CONTROL → RHYTHM REVERSALS → NEEDLE RIDGE → FINAL EXAM.
  정밀 착지 → 높은 계단 → 긴 간격 → 하강 거리 제어 → 짧음/김 교대 → 최소 폭 → 종합.

점프 방향과 능력치는 그대로다. 높이·거리·크기를 읽는 난도를 우선 검증하기 위해 움직이는 발판이나 함정은 넣지 않았다.
좌우 자유 이동, 랜덤 생성, 체크포인트, 스테이지별 기록 저장은 이번 범위에 포함하지 않았다.

## 플레이 흐름과 UI

- 화면 위: 현재 스테이지와 난도, 현재 점수.
- 아래: 스테이지 선택 버튼 3개, 충전 안내, 현재 착지 번호/전체와 구간 이름.
- 선택 버튼은 레벨링 테스트용으로 항상 해금되어 있다. 누르면 해당 스테이지 처음부터 시작한다.
- 마지막 발판 착지 → STAGE CLEAR → NEXT STAGE.
- 3스테이지 종료 → ALL STAGES CLEAR → RESTART ALL로 1스테이지.
- RETRY STAGE는 현재 스테이지를 다시 시작한다. 씬 리로드는 하지 않는다.
- 점수는 **현재 스테이지별**로 초기화하며, BEST는 기존 방식대로 어느 스테이지에서든 기록한 최고 점수다.
  난도별 랭킹·누적 캠페인 점수는 별도 기능으로 분리할 예정이다.

## 편집 구조

- `ScriptableObjects/Stages/Stage01_Easy.asset`, `Stage02_Normal.asset`, `Stage03_Hard.asset`:
  좌표, 폭, 구간명과 스테이지 색상 원본.
- `StageBuilder.cs`: 원본 좌표로 Scene에 실제 Platform 프리팹 인스턴스를 배치한다. 랜덤하지 않다.
  기존 스테이지 씬이 있으면 덮어쓰지 않는다.
- `StageCourse.cs`: 스테이지의 Spawn, 순서대로 정렬된 Platform 참조, Definition.
- `StageDirector.cs`: 활성 스테이지, 진행률, 도착 판정. 선택되지 않은 두 맵은 비활성화한다.
- `GameManager.cs`: 선택/재시도/다음 스테이지 및 StageClear 상태.
- `PrototypeHUD.cs`: 기존 프로토타입 호환을 유지하며 선택적으로 스테이지 UI를 표시한다.

모든 Inspector 참조는 연결되어 있다. `Game Systems > StageDirector > Starting Stage Index`의
0/1/2로 시작 스테이지를 지정할 수도 있다. Scene 뷰에서는 원하는 스테이지 루트를 활성화해서 편집한다.

현재 맵은 에디터에서 구워진 배치다. Definition의 좌표를 바꿔도 기존 씬의 Transform은 자동 변경하지 않는다.
실제 플레이 맵을 미세 조정할 때는 해당 Platform 인스턴스의 Transform을 조절하고,
재생성 원본과 일치하도록 Definition의 같은 항목도 갱신한다. 발판 추가/삭제 시 StageCourse의 순서 배열도 갱신한다.
기존 편집을 지우지 않도록 자동 재생성/덮어쓰기는 제공하지 않는다.

## 검증

검증 결과: Unity 6000.3.16f1 별도 프로젝트 사본에서 컴파일 성공, 테스트 **6개 통과 / 0개 실패**.
세 맵의 **144회 연속 점프**와 전환까지 완료했다. 검증된 씬/데이터와 원본에 복사한 파일의 해시도 확인했다.

- `StageLayoutTests`: 현재 플레이어 프리팹의 실제 점프 속도/중력을 읽어, 발판 중앙 및 캐릭터 전체가 올라갈 수 있는
  양쪽 가장자리에서 다음 발판에 도달 가능한 충전값이 있는지 검사한다. 발판 사이 빈 간격도 확인한다.
- `StagePlayTests`: 실제 Play Mode에서 일반 BeginCharge/ReleaseCharge 경로로 각 발판 중앙을 목표로 연속 점프한다.
  40 + 48 + 56 = 144번 착지, 점수, 스테이지 클리어/전환/전체 재시작을 검증한다.
- 추가 통합 테스트: 3스테이지 직접 선택 → 추락 → 현재 스테이지 재시도.
- 기존 `PrototypePlayTests` 3개도 실행하여 Space/Mouse 입력과 기존 씬의 동작을 확인한다.

수학적 도달성과 자동 중앙 조준 완주는 사람에게 적당한 난도라는 보장이 아니다.
좁은 발판의 끝에 일부만 걸친 착지는 가장자리 수학 검증의 보장 범위 밖이다.
플레이어 점프 수치·중력·Collider를 바꾸면 레벨링 검증도 다시 실행해야 한다.
Windows/WebGL 배포 빌드와 사람의 체감 난도 검수는 별도다.
