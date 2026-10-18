# 구름 테마 HUD 리소스

2026-10-13, 기본 제공 imagegen 도구로 제작한 투명 PNG 4종입니다. 정확한 제작 프롬프트는 같은 폴더의 PROMPTS.json에 보관합니다. 원본 픽셀을 수정하지 않고 HudArt에서 투명 여백을 제외한 Sprite를 생성합니다.

| 파일 | 적용 위치 |
| --- | --- |
| CloudMeter.png | 점프 충전 게이지, 스테이지 진행 게이지, 설정 음량 슬라이더 |
| LandingTarget.png | 모든 발판의 정확한 착지 영역 |
| BoostBadge.png | 부스트 상태 아이콘, 콤보 3단계 충전 표시 |
| InfoPanel.png | 스테이지, 닉네임·시간/연습 안내, 콤보, 부스트, 착지 알림, 조작 힌트, 진행 숫자 |

- 점프 게이지: 충전량에 따라 청록색에서 금색으로 변화합니다. 내부 광택 채움은 HudArt.Fill에서 코드로 그리는 캡슐형 그래픽입니다.
- 부스트 준비 완료: 아이콘이 밝아지고 천천히 맥동합니다. 대기 상태는 어둡게 표시합니다.
- 착지 마커: 실제 HalfWidth에 맞춰 크기를 설정합니다. 정확한 착지 때 밝아지고 금색 테두리가 나타납니다. 콜라이더가 꺼지면 숨깁니다. 판정과 물리 수치는 변경하지 않습니다.
- 텍스트는 이미지에 굽지 않았습니다. 기존 Data/UITexts.xlsx에서 계속 관리합니다.
- 기존 로비·메뉴·결과 화면 버튼 리소스는 유지하며, 새 리소스는 그림이 없던 HUD 요소를 중심으로 적용했습니다.
- 위치/크기: Scripts/UI/GameUISkin.cs. 게이지: GameUI.Meter. 착지 표시: PerfectLandingZone.
- PNG는 Resources/HudArt에 있어 별도 수동 연결 없이 빌드에 포함됩니다. 임포트 설정은 Editor/HudArtImporter.cs에서 관리합니다.
- 검증: GameUITests, ComboTests, ComboPresentationTests, ExhibitionUITests, HudSkinTests. 배포 빌드의 실제 기기 검증은 별도입니다.
