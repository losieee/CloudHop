# CLOUD HOP — 기믹 프로토타입

## 실행

- 먼저 `Assets/_Project/Scenes/CloudHopGimmickLab.unity`에서 Play.
  - **1 BIRD / WIND:** 2번째 점프에 새, 5번째 점프에 바람.
  - **2 CLOUD / MOVE:** 2번째 착지에 사라지는 구름, 5번째 착지에 이동 발판.
  - **3 LIGHTNING:** 2번째·6번째 점프에 번개.
  - 각 짧은 코스는 8점프. 하단 버튼으로 바로 전환 가능.
- 전체 코스: `Assets/_Project/Scenes/CloudHopGimmicks.unity`.
  기존 40 / 48 / 56점프 배치는 유지하고 기믹 30개를 추가했다.
- `CloudHopStages.unity`와 `CloudHopPrototype.unity`는 기존 비교용 씬으로 보존했다.
- `GIMMICKS: ON/OFF` 버튼은 모든 기믹을 켜거나 끄고 **현재 스테이지를 재시작**한다.

## 표시와 동작

| 표시 | 동작 | 테스트할 것 |
| --- | --- | --- |
| 분홍 BIRD | 일정한 범위를 왕복하는 새. 공중에서 닿으면 왼쪽 위로 밀림 | 위치/진행 방향을 보고 점프, 피격 후 연속 넉백 방지 |
| 청록 WIND UP ^ | 5초 ON → 5초 OFF 반복. 접촉 시 위로 상승, 적용 중 LIFTING과 흰색 표시 | 상승과 남은 시간 확인, OFF에는 효과 없음 |
| 주황 CRUMBLE | 정상 착지 후 카운트다운 → Collider 소멸 → 일정 시간 뒤 복구 | 오래 머물면 추락, 떠나도 카운트다운 지속, Retry 즉시 복구 |
| 초록 MOVE UP/DOWN | 수직 왕복 발판. 탑승자를 함께 이동시킴 | 상승·하강 중 충전 유지, 정상 점프, 중복 착지 점수 없음 |
| 노랑/빨강 BOLT | SAFE → WARNING → ACTIVE. 빨간 ACTIVE 때만 공중 피격 | 예고를 보고 통과, 기다리거나 머물러도 발판 위에서는 피격되지 않음 |

새·번개는 즉사 대신 넉백을 준다. 피해 후 1초 동안 추가 넉백을 무시한다. 상승 기류는 보호 중에도 적용된다.
충전 중 피격되면 충전을 취소한다. 화면 하단에 HIT / PROTECTED 남은 시간을 표시한다.
실제 사망은 기존처럼 Fall Threshold 아래 추락할 때 처리한다.

최종 아트·애니메이션·효과음은 추가하지 않았다. 기존 사각 Sprite, 반투명 영역, TextMesh와 색상만 사용했다.
표시 텍스트는 임시 폰트 호환을 위해 영문이다.

## 스테이지 배치

- **쉬움:** 새를 13·21·29·37번째 점프에 배치. 다른 기믹 없음.
- **보통:** 새 5·19·33, 바람 11·27, 구름 15·23·39, 이동 발판 35·43.
- **어려움:** 새 3·17·35·47, 바람 11·31·53, 구름 13·27·43, 이동 발판 19·45,
  번개 7·23·39·51.

숫자는 도착 발판 번호다. 구름과 이동 발판은 해당 발판 자체에 붙고, 새·바람·번개는 직전 발판과의 사이에 있다.
초록 휴식 발판과 움직이는 초록 발판은 MOVE 텍스트로 구별한다.
구름 다음 점프에는 즉시 기다려야 하는 시간제 위험을 겹치지 않았다.
움직임은 원래 발판 높이에서 ±0.25(보통), ±0.2(어려움)로 제한했다.

## Inspector와 파일

씬의 참조는 모두 연결되어 있다. 새 기믹은 각 StageCourse 루트 아래에 둔다.
Platform 계열은 기존 발판 오브젝트에 컴포넌트를 붙이고, 새·바람·번개는 별도 Trigger 오브젝트다.

| 스크립트 | 책임 / 조절값 |
| --- | --- |
| StageMechanics | 스테이지별 시계, Game Over/Clear 정지, 전체 Reset. Gimmicks Enabled |
| Gimmick | 공통 디버그 라벨과 색상. Visual / Debug Label |
| PeriodicGimmick | Safe / Warning / Active Duration, Phase Offset. 공통 주기 계산 |
| PatrolBird | Half Range, Period, Knockback |
| WindZone | Active Duration 5초 / Inactive Duration 5초, Lift Speed 7. 수평 속도 유지, 접촉 시 최소 상승 속도 보장 |
| LightningZone | 주기와 Knockback |
| CrumblingPlatform | Crumble Delay, Respawn Delay |
| MovingPlatform | Amplitude, Period. 수직 Kinematic Rigidbody2D 이동 |
| GimmickTestControls | ON/OFF 버튼 및 현재 스테이지 재시작 |
| GimmickBuilder (Editor) | 전체/테스트 씬과 테스트 맵 데이터 생성. 기존 씬 덮어쓰기 방지 |
| GimmickPlayTests (Editor) | 실제 충돌·주기·탑승·복구·토글 테스트 |
| GimmickCaptureTests (Editor) | 그래픽 장치를 사용한 테스트 화면 캡처 |

변경한 기존 코드:
- PlayerController: 움직이는 발판 속도 기준 접촉 판정과 탑승, 넉백·보호 시간·공중 바람 처리.
- GameManager: Retry 시 기믹 시계와 상태 초기화.
- PrototypeHUD: 피격 보호 시간 안내.

기본값: 새 왕복 3.4–4.5초, 상승 기류 속도 7 / ON 5초 + OFF 5초,
번개 주기 SAFE 3.5초 + WARNING 1.2초 + ACTIVE 0.4초,
구름 붕괴 2.4초(보통) / 1.8초(어려움), 복구 2.5초.
Ready에서도 주기는 흐르므로 안전한 발판에서 관찰할 수 있다. Game Over와 Clear에서는 멈춘다.
Retry/스테이지 선택 때는 시간과 위치, Collider, 피격 보호 시간이 초기화된다.

## 검증과 남은 작업

- Unity 6000.3.16f1 컴파일.
- 기능 테스트 12개 통과(기믹 6 + 기존 회귀 6), 별도 그래픽 캡처 테스트 통과.
  새/바람, 구름/이동 발판, 번개 예고/작동의 실제 렌더 이미지 4장을 확인했다.
- 기믹별 Play Mode 기능 테스트 6개: 새 피격/보호/리셋, 바람 상태, 번개 예고/피격/정지,
  구름 소멸/복귀, 이동 발판 전체 주기 탑승/점프, ON/OFF.
- 기존 입력·점수·재시도·맵 도달성 테스트 6개와 기존 3스테이지 144회 연속 점프 회귀 검증.
- 기믹이 켜진 전체 3스테이지의 인간 플레이 난도와 완주율은 아직 측정하지 않았다.
  테스트 씬에서 기믹 감각 확인 → 전체 씬에서 등장 주기/넉백/붕괴 시간 조정을 권장한다.
- 실제 Windows/WebGL 빌드·Chrome 실행은 별도 검증이 필요하다.
- 현재 MovingPlatform은 수직 이동 전용. 수평 이동·회전·일방통행 발판은 미구현이다.
- 그래픽을 교체할 때 플레이 판정 Collider와 디버그 Visual을 독립적으로 유지한다.

상승 기류는 접촉한 플레이어를 위로 띄우며 충전을 취소한다. 영역 이탈 또는 OFF 후에는 중력에 따라 자연스럽게 하강한다. Retry는 ON 5초부터 다시 시작한다.
