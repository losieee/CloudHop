# PC / Android 버전 작업 안내

하나의 Unity 프로젝트에서 게임 로직, 스테이지, 캐릭터, 아트, UI 문구를 함께 관리합니다.

## 시작 씬

- PC: `Assets/_Project/Scenes/CloudHop_PC.unity`
- Android 휴대폰·태블릿: `Assets/_Project/Scenes/CloudHop_Android.unity`
- 공통 게임: `Assets/_Project/Scenes/CloudHop.unity`

시작 씬은 플랫폼을 선택한 뒤 공통 게임 씬을 불러옵니다. 공통 게임 수정은 두 버전에 함께 적용됩니다. 공통 씬을 직접 실행하면 Android 기기에서는 모바일, PC 에디터에서는 PC 조작을 사용합니다.

Unity 메뉴 `Cloud Hop > 플랫폼 > PC 시작 씬 열기` 또는 `Android 시작 씬 열기`를 선택하고 Play를 누르세요. Android 미리보기는 가로 Game View에서 확인합니다. 씬이 없으면 `시작 씬 준비` 메뉴로 생성할 수 있습니다.

## 조작

| 항목 | PC | Android |
| --- | --- | --- |
| 점프 충전 | 스페이스바 또는 마우스 왼쪽 누르기 | 우측 하단 점프 버튼 누르기 |
| 점프 | 누른 입력 모두 놓기 | 누른 손가락 떼기 |
| 공중 부스트 | 부스트 보유 시 다시 누르기 | 부스트 보유 시 점프 버튼 다시 누르기 |
| 일시정지 | Esc 또는 일시정지 버튼 | 일시정지 버튼 |

모바일은 첫 손가락이 시작한 입력만 처리합니다. 다른 손가락으로 떼는 동작은 점프를 실행하지 않습니다. 일시정지·앱 전환·포커스 상실은 충전을 취소합니다. 앱이 백그라운드로 이동하면 일시정지 화면을 표시합니다.

Android는 가로 화면 두 방향을 지원합니다. UI 내용은 화면 안전 영역에 맞추고, 일시정지·결과 화면의 어두운 배경은 전체 화면에 유지합니다. 모바일 전용 문구도 `Data/UITexts.xlsx`의 `mobile.*` 항목에서 수정합니다.

## 빌드

- `Cloud Hop > 빌드 > PC Windows 빌드`: `Builds/PC/CloudHop.exe`
- `Cloud Hop > 빌드 > Android APK 빌드`: `Builds/Android/CloudHop.apk`

각 빌드는 해당 시작 씬과 공통 씬만 포함합니다. Android는 ARM64 / IL2CPP / 가로 화면 / 패키지 ID `com.cloudhop.game`으로 생성합니다. 빌드에 한시적으로 사용한 Player Settings는 완료 또는 실패 후 복원합니다. 스토어 배포용 서명·등록 작업은 포함하지 않습니다.

현재 설치된 Unity 6000.3.16f1에는 Android Build Support가 없습니다. Unity Hub의 해당 에디터 `Add modules`에서 Android Build Support와 하위 Android SDK & NDK Tools, OpenJDK를 설치한 후 APK 빌드를 실행하세요. 모듈이 없으면 빌드 메뉴가 설치 안내를 표시하고 중단합니다.

PC 배포 시 exe만 옮기지 말고 `Builds/PC` 폴더 전체를 복사하세요. Android는 APK를 기기에 설치합니다. 랭킹·닉네임·설정은 각 기기 로컬에 저장되며 PC와 휴대폰 사이에 자동 동기화되지 않습니다.

## 검증 범위

에디터 테스트는 시작 씬 로딩, PC UI 유지, 터치 충전·점프·부스트, 다중 터치 소유권, 일시정지/앱 전환 취소, UI 문구 및 전체 화면 오버레이를 확인합니다. Android 실기기 성능·터치 키보드·노치/시스템 내비게이션 바는 APK 생성 후 별도 실기기 점검이 필요합니다.
