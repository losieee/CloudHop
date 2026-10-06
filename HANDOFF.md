# CLOUD HOP 작업 인계 — 2026-10-06

## 다른 컴퓨터에서 시작
1. Fork에서 `https://github.com/losieee/CloudHop.git`을 Clone합니다. 기존 복제본이면 변경 사항을 보존한 뒤 `main`을 Pull합니다.
2. Unity Hub에서 **Unity 6000.3.16f1**을 설치합니다. Windows 실행 파일 제작 시 Windows Build Support도 설치합니다.
3. Hub에 복제한 저장소 루트를 추가합니다. `Assets`, `Packages`, `ProjectSettings`가 들어 있는 폴더가 프로젝트 루트입니다. `Assets` 폴더 자체를 열지 않습니다.
4. 첫 패키지 복원과 리소스 임포트를 기다립니다. 개발 환경의 첫 패키지 다운로드에는 인터넷 연결이 필요할 수 있습니다.
5. **Assets/_Project/Scenes/CloudHop.unity**를 열어 Play합니다. 기존 SampleScene/실험 씬과 구분합니다. 원본 다운로드 폴더나 이전 컴퓨터의 임시 검증 프로젝트를 복사할 필요는 없습니다.
6. Codex에서 이 저장소를 프로젝트로 열고 이 문서를 읽도록 요청합니다.

주의: 현재 `ProjectSettings/EditorBuildSettings.asset`의 기본 씬 목록에는 `Assets/Scenes/SampleScene.unity`가 들어 있습니다. 실제 배포 전 Build Profiles의 Scene List에서 `Assets/_Project/Scenes/CloudHop.unity`를 첫 실행 씬으로 지정하고 SampleScene을 제외해야 합니다. 이번 인계에서는 게임 빌드를 새로 만들지 않았습니다.

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
이번 버전은 핵심 조작과 1스테이지 시험 구간까지입니다. 2·3스테이지 경로 재설계나 신규 기믹 확대까지 완료한 상태는 아닙니다. 세 캐릭터의 물리 조건은 동일합니다.

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
4. Build Profiles 씬 목록 정리 후 Windows 빌드에서 한글 IME, 전체 3스테이지 완주, 재실행 후 로컬 기록 유지 확인.
새 Windows 배포 빌드와 사람의 전체 코스 완주 테스트는 이번 검증에 포함하지 않았습니다.

## 데이터와 Git
- Assets(메타 포함), Packages, ProjectSettings가 Git으로 이동합니다. Library/Temp/Logs는 재생성되며 커밋하지 않습니다.
- 닉네임 랭킹은 기기/사용자별 PlayerPrefs에 저장됩니다. Git Clone/Pull로 기존 랭킹 기록이 다른 컴퓨터에 옮겨지지 않습니다.
- 랭킹 키 `CloudHop.TimeRanking.v1` 및 기존 기록을 임의로 삭제하지 않습니다. 부스트 도입 전 기록과의 비교 조건은 달라질 수 있습니다.
- 이전 컴퓨터 경로를 코드에 하드코딩하지 말고 현재 복제한 저장소를 기준으로 작업합니다.

## 새 Codex 대화에 붙여 넣을 요청
이 저장소의 HANDOFF.md와 Assets/_Project/COMBO_GUIDE.md를 읽고 현재 구현 상태부터 이어서 작업해줘. 콤보·공중 부스트와 1스테이지 시험 지름길은 구현되어 있어. 먼저 현재 브랜치와 변경 사항, Unity 컴파일 및 관련 테스트를 확인하고, 실제 조작감을 기준으로 다음 밸런싱 작업을 진행해줘. 기존 한글 UI Excel 관리와 로컬 랭킹을 유지하고, 2·3스테이지 경로 확장은 아직 남은 작업으로 봐줘.
