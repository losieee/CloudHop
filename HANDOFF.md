# CLOUD HOP 작업 인계 — 2026-10-06

## 다른 컴퓨터에서 시작
1. Fork에서 `https://github.com/losieee/CloudHop.git`을 Clone합니다. 기존 복제본이면 변경 사항을 보존한 뒤 `main`을 Pull합니다.
2. Unity Hub에서 **Unity 6000.3.16f1**을 설치합니다. Windows 실행 파일 제작 시 Windows Build Support도 설치합니다.
3. Hub에 복제한 저장소 루트를 추가합니다. `Assets`, `Packages`, `ProjectSettings`가 들어 있는 폴더가 프로젝트 루트입니다. `Assets` 폴더 자체를 열지 않습니다.
4. 첫 패키지 복원과 리소스 임포트를 기다립니다. 개발 환경의 첫 패키지 다운로드에는 인터넷 연결이 필요할 수 있습니다.
5. **Assets/_Project/Scenes/CloudHop.unity**를 열어 Play합니다. 기존 SampleScene/실험 씬과 구분합니다. 원본 다운로드 폴더나 이전 컴퓨터의 임시 검증 프로젝트를 복사할 필요는 없습니다.
6. Codex에서 이 저장소를 프로젝트로 열고 이 문서를 읽도록 요청합니다.

`ProjectSettings/EditorBuildSettings.asset`의 기본 씬 목록은 `Assets/_Project/Scenes/CloudHop.unity`만 활성화하도록 정리했습니다. 별도 Build Profile에서 씬 목록을 재정의한다면 같은 설정인지 확인합니다. 게임 빌드는 아직 새로 만들지 않았습니다.

## 현재 구현 상태
게임 변경 기준 커밋: `06c2427` (`[Feat] 재미요소 추가`).
- 배경 이미지의 레이어 배치, 업로드된 기믹 이미지와 상승기류/번개 연출.
- 로비, 캐릭터 선택, 한글 UI 및 한글 폰트. 메인/소녀/여우 캐릭터와 상태별 포즈.
- 3개 스테이지와 기존 이동 발판/붕괴/상승기류/번개/새 기믹.
- 계정 없이 닉네임을 등록하고 1~3스테이지 전체 완주 시간을 겨루는 로컬 TOP 10. 연습 모드는 기록하지 않음.
- 일시정지 화면 전체 암막. 통합 게임 UI 문구 69개를 Excel에서 관리.
- 새 발판 중앙에 3연속 정확히 착지하면 공중 부스트 1회 충전. 공중에서 스페이스바를 다시 누르면 짧게 앞으로 가속.
- 착지 표시/효과음, 부스트 잔상/효과음, 난이도별 중앙 판정 폭.
- 1스테이지 초반 3번째 발판에서 4번째를 건너뛰어 5번째로 향하는 시험 지름길과 안전 경로 안내.

## 콤보 규칙과 범위
부스트는 최대 1회 저장, 한 점프에 한 번 사용합니다. 일반 착지·재방문·피격은 콤보를 끊지만 저장된 부스트는 유지합니다. 재시도/스테이지 전환은 초기화합니다. 기본 점프 속도는 유지하며 자동 가속하지 않습니다.
현재 통합 씬에는 1스테이지 시험 구간과 2·3스테이지 각각 2개의 선택 경로가 있습니다. 기존 기믹을 재배치했으며 세 캐릭터의 물리 조건은 동일합니다.

## 수정 위치
- 문구 원본: `Assets/_Project/Data/UITexts.xlsx` → `UI텍스트` 시트의 `한국어` 열. Key, 변수 자리표시자, 시트 이름 유지.
- 자동 산출물: `Assets/_Project/Resources/UITexts.json`. JSON을 직접 수정하지 말고 Excel을 저장한 뒤 `Cloud Hop > UI 텍스트 > 엑셀 다시 반영` 사용. 재생 중이면 다시 시작.
- 게임 입력/물리: `Assets/_Project/Scripts/Player/PlayerController.cs`.
- 콤보 규칙/효과: 같은 폴더의 `ComboChain.cs`, `ComboEffects.cs`.
- 중앙 표시/경로: `Scripts/Platform/PerfectLandingZone.cs`, `ComboShortcutGuide.cs`.
- 통합 UI/부스트 안내: `Scripts/UI/GameUI.cs`, `GameUICombo.cs`, `GameUIRanking.cs`.
- 스테이지 데이터: `Assets/_Project/ScriptableObjects/Stages/`.
- 자세한 설명: `Assets/_Project/COMBO_GUIDE.md`, `UI_TEXT_GUIDE.md`, `EXHIBITION_GUIDE.md`, `CHARACTER_ANIMATION_GUIDE.md`.

## 확인한 것 / 아직 남은 것
직전 작업에서 콤보 물리·지름길 착지·잔상·피격·초기화, 기존 UI 및 랭킹 흐름, Excel 문구를 검증했습니다. 관련 11개 테스트가 최종 수정별 실행에서 통과했고 실제 프로젝트의 Unity 컴파일/69개 문구 임포트도 성공했습니다. 테스트 로그와 캡처는 이전 컴퓨터의 임시 폴더에 있으며 저장소에 포함하지 않았습니다.
새 컴퓨터에서는 Test Runner의 EditMode에서 `ComboTests`, `ComboPresentationTests`, `GameUITests`, `ExhibitionUITests`, `UITextTests`를 실행해 환경을 다시 확인할 수 있습니다. 물리 시간 검사는 `Time.fixedTime` 경과를 사용합니다. EditMode 테스트에서 `WaitForSeconds`만으로 실제 물리 시간이 지났다고 가정하지 않습니다.

다음 권장 순서:
1. 실제 조작으로 1스테이지의 콤보 획득 난도, 부스트 타이밍, 지름길 시간 이득을 플레이테스트.
2. 결과에 따라 판정 폭/충전 횟수/부스트 지속 시간과 경로 간격 조절.
3. 2·3스테이지에 기존 바람·이동 발판·번개와 선택 경로를 단계적으로 결합.
4. Windows 빌드에서 한글 IME, 전체 3스테이지 완주, 재실행 후 로컬 기록 유지 확인. 기본 씬 목록 정리는 완료했으며 별도 Build Profile의 재정의 여부는 빌드 시 확인.
새 Windows 배포 빌드와 사람의 전체 코스 완주 테스트는 이번 검증에 포함하지 않았습니다.

## 새 컴퓨터에서 이어서 확인한 결과 — 2026-10-06
- 시작 상태: `main`, `1c4df23`, 작업 트리 변경 없음.
- Unity 6000.3.16f1에서 패키지 복원과 스크립트 컴파일 성공.
- 위의 5개 테스트 클래스 실행 결과 **11개 통과, 실패 0개**. 자동 물리/화면/UI 흐름 검증이며 사람의 실제 조작감 검증은 아님.
- 로컬 결과: `Logs/handoff-tests.xml`, 실행 로그: `Logs/handoff-tests.log`, 화면 캡처: `Logs/handoff-preview/`. Logs는 Git에 포함하지 않음.
- 기본 빌드 씬을 CloudHop으로 수정. 테스트에서 자동 생성된 폰트 아틀라스와 설정 직렬화 변경은 원복함.
- 부스트 수치와 스테이지 배치는 유지. 실제 플레이에 따른 밸런싱, 2·3스테이지 경로 확장, Windows 배포 빌드 검증은 아직 남아 있음.
- 다음 플레이에서는 1스테이지 초반 3연속 중앙 착지 성공률, 부스트 발동 시점, 안전 경로 대비 지름길의 도착 시간과 실패 횟수를 같은 시작 위치에서 비교할 것. 자동 테스트 통과만으로 난이도가 적절하다고 판단하지 않음.

## 데이터와 Git
- Assets(메타 포함), Packages, ProjectSettings가 Git으로 이동합니다. Library/Temp/Logs는 재생성되며 커밋하지 않습니다.
- 닉네임 랭킹은 기기/사용자별 PlayerPrefs에 저장됩니다. Git Clone/Pull로 기존 랭킹 기록이 다른 컴퓨터에 옮겨지지 않습니다.
- 랭킹 키 `CloudHop.TimeRanking.v1` 및 기존 기록을 임의로 삭제하지 않습니다. 부스트 도입 전 기록과의 비교 조건은 달라질 수 있습니다.
- 이전 컴퓨터 경로를 코드에 하드코딩하지 말고 현재 복제한 저장소를 기준으로 작업합니다.

## 새 Codex 대화에 붙여 넣을 요청
이 저장소의 HANDOFF.md와 Assets/_Project/COMBO_GUIDE.md를 읽고 현재 구현 상태부터 이어서 작업해줘. 콤보·공중 부스트와 1스테이지 시험 지름길은 구현되어 있어. 먼저 현재 브랜치와 변경 사항, Unity 컴파일 및 관련 테스트를 확인하고, 실제 조작감을 기준으로 다음 밸런싱 작업을 진행해줘. 기존 한글 UI Excel 관리와 로컬 랭킹을 유지하고, 추가된 2·3스테이지 선택 경로의 실제 조작감과 난이도를 확인해줘.

## 선택 경로 확장 — 2026-10-13
- 통합 CloudHop.unity에 2스테이지 8→10, 24→26, 3스테이지 10→12, 19→21 지름길을 추가했습니다. 각 중간 발판은 부스트 없는 기본 경로로 남아 있습니다.
- 2스테이지 중간 발판을 낮추고 넓혔습니다. 3스테이지는 기존 상승기류/이동 발판과 결합하고 Jump 23 번개를 19→21 구간으로 옮겼습니다. 상승기류 합류 발판과 다음 발판 높이도 조정했습니다.
- StageDefinition.choiceRoutes로 안내 구간을 관리합니다. GameUICombo는 기존 Excel의 combo.* 키를 사용합니다. 재설치 도구: Cloud Hop > Install Stage Choice Routes.
- 관련 13개 테스트의 최종 수정별 실행에서 모두 통과했습니다. ChoiceRouteTests는 실제 씬에서 네 구간의 기본 경로·부스트 지름길·재시도를 검증하고, StageLayoutTests는 전체 발판의 착지 가능 범위를 확인합니다. 기존 UI·랭킹·콤보·문구 검사도 통과했습니다.
- Windows 배포 빌드 및 사람의 전체 코스 완주/밸런스 검증은 별도입니다. 이전 날짜의 미완료 항목 중 '2·3스테이지 경로 확장'은 이번 작업으로 갱신되었습니다.

## HUD 그래픽 적용 — 2026-10-13
- Resources/HudArt의 투명 PNG 4종을 기본 제공 imagegen으로 제작했습니다. README.md와 PROMPTS.json에 사용처·관리 방법·프롬프트를 기록했습니다.
- 점프/진행/음량 게이지, 콤보·부스트 아이콘, 스테이지·시간·힌트 정보 패널, 정확한 착지 목표 마커에 적용했습니다. 문구는 기존 Excel 데이터 유지.
- GameUISkin/HudArt가 표시를 담당하며, 착지 마커 폭은 PerfectLandingZone.HalfWidth와 일치합니다. 기존 게임 물리와 판정 수치는 유지합니다.
- 관련 UI·랭킹·콤보·HUD 테스트 6개가 최종 수정별 실행에서 통과했습니다. HUD 테스트에는 반/최대 충전, 취소, 착지 표시 폭·강조·숨김을 포함합니다. 1920×1080과 1024×768 캡처를 확인했습니다.
- 로컬 캡처: Logs/HudPreview-20261013. Windows 배포 빌드 검증은 별도입니다.

## PC / Android 분리 (2026-10-14)
- 시작 씬: CloudHop_PC.unity / CloudHop_Android.unity. 두 씬 모두 기존 CloudHop.unity를 로드하며 게임 로직·스테이지·리소스를 공유한다.
- Cloud Hop > 플랫폼에서 각 시작 씬 열기, Cloud Hop > 빌드에서 Windows exe / Android APK를 각각 생성한다.
- Android 전용 터치 점프·충전·부스트, 가로 화면, UI 안전 영역, 앱 전환 시 일시정지 추가. 모바일 문구 3개는 UITexts.xlsx의 mobile.* 키로 관리한다.
- PC 키보드/마우스 동작 유지. 로컬 기록은 각 기기 내부에 저장되며 자동 동기화 없음.
- Android Build Support 모듈이 현재 없어 APK 생성 및 실기기 검증은 미실행. 설치 조건 및 전체 절차는 Assets/_Project/PLATFORM_GUIDE.md 참고.

## 공통 1스테이지 맵 확장 (2026-10-14)
- 기존 8~16번 발판을 상승 → 위/아래 갈림길 → 합류 → 하강 구간으로 변경.
- 상단 추가 발판 2개는 BranchLanding으로 진행률을 연결하며, 동일 CloudHop.unity를 PC/Android가 공유한다.
- Stage01_Easy.coins에 코인 80개(일반 1점, 황금 5점, 총 92점). StageCollectibles가 수집과 재시작 복원을 관리한다. 기존 착지 점수/시간 랭킹과 분리.
- 기존 13번 점프 구간 새를 18번으로 옮겨 첫 갈림길의 하단 경로를 안전하게 유지.
- 1스테이지 높은 발판에서 카메라가 위아래로 부드럽게 이동. 낮은 경로가 하단 HUD에 가려지지 않도록 높이 여유를 둠.
- 모바일 결과 화면에서 Safe Area Content 아래로 이동한 Retry 버튼을 찾지 못하던 참조도 수정.
- 맵 관리 방법: Assets/_Project/MEADOW_MAP_GUIDE.md.

## 공통 2스테이지 바람 능선 확장 (2026-10-17)
- RidgeMapInstaller로 공통 씬/Stage02_Normal에 능선 상승, 상하단 경로, 후반 작은 섬과 정상 하강을 적용.
- Golden Ridge Route 상단 3개 발판은 16번에서 출발해 20번에 합류. 하단 점프가 상단에 걸리지 않도록 높이/간격 조정.
- 코인 136개(총 152점), 기존 StageCollectibles로 재시작 복원. 기존 8/24 콤보 지름길 및 상승기류 유지.
- StageLayoutTests, ChoiceRouteTests 통과. RidgeMapTests는 변경 구간 12~48의 기본 점프, 상단 경로, 활성 이동 발판 진입/이탈, 수집 합산/중복 방지/재시작을 검증해 통과.
- 세부 배치/편집: Assets/_Project/RIDGE_MAP_GUIDE.md.

## 2스테이지 미끄럼 언덕 (2026-10-18)
- 29번 발판: 평지 진입 → 얼음 비탈, 30번 평지에서 정지. 31~32번으로 다시 상승.
- SlidingSlope의 표면 속도를 PlayerController에서 적용. 미끄럼 중에도 충전 점프 가능. 슬라이딩 전용 자세는 세 캐릭터의 chargeSprite에 기울기/흔들림을 합성하고 이탈/재시작 시 복원.
- SlideTests(세 스킨, 28→29 진입, 자동 이동/정지, 도중 점프, 자세 복원, 출구 점프), StageLayoutTests, RidgeMapTests, ChoiceRouteTests 통과.
- 아트/충돌은 SlideMapInstaller에서 생성. 재적용 순서와 편집 주의사항: Assets/_Project/SLIDE_GUIDE.md.
- 현재 2스테이지에만 도입. 3스테이지에는 아직 미끄럼 지형을 배치하지 않음.
