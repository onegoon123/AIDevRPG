# 게임 로그 시스템

게임 플레이 분석을 위한 자동 로그 데이터 수집 및 저장 시스템입니다.

## 주요 기능

### 1. 자동 데이터 수집
- **플레이어 행동**: 경험치 획득, 레벨업, 스킬 업그레이드, 힐링 사용
- **전투 데이터**: 공격, 데미지, 크리티컬 히트, HP 변화
- **던전 진행**: 층 클리어, 적 처치, 스폰 정보
- **적 스폰**: 적 타입, 위치, 레벨, 보스 여부

### 2. 데이터 저장
- JSON 형식으로 자동 저장
- 세션별 고유 ID 생성
- 자동 저장 (30초 간격)
- 수동 저장 기능

### 3. 데이터 분석
- 플레이 패턴 분석
- 전투 통계
- 진행도 분석
- 세션별 상세 분석

## 사용법

### 1. 기본 설정
1. `GameLogManager`를 씬에 추가
2. `LogAnalyzer`를 씬에 추가 (선택사항)
3. `LogSystemUI`를 씬에 추가 (선택사항)

### 2. 자동 로깅
로깅은 자동으로 시작되며, 다음 이벤트들이 자동으로 기록됩니다:
- 플레이어의 모든 행동
- 전투 이벤트
- 던전 진행
- 적 스폰

### 3. 수동 제어
```csharp
// 로깅 활성화/비활성화
GameLogManager.Instance.SetLoggingEnabled(true/false);

// 수동 저장
GameLogManager.Instance.SaveLogData();

// 로그 데이터 가져오기
GameLogData logData = GameLogManager.Instance.GetLogData();
```

### 4. 분석 기능
```csharp
// 모든 로그 분석
LogAnalyzer analyzer = FindObjectOfType<LogAnalyzer>();
analyzer.AnalyzeAllLogs();

// 특정 세션 분석
SessionAnalysisResult result = analyzer.AnalyzeSession(sessionId);
```

## 저장 위치

로그 파일은 다음 위치에 저장됩니다:
- **Windows**: `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\GameLogs\`
- **Mac**: `~/Library/Application Support/<CompanyName>/<ProductName>/GameLogs/`
- **Linux**: `~/.config/unity3d/<CompanyName>/<ProductName>/GameLogs/`

## 로그 파일 형식

각 로그 파일은 JSON 형식으로 저장되며, 다음 정보를 포함합니다:

```json
{
  "sessionData": {
    "sessionId": "고유 세션 ID",
    "startTime": "시작 시간",
    "endTime": "종료 시간",
    "totalPlayTime": "총 플레이 시간",
    "totalFloorsCleared": "클리어한 층수",
    "isCompleted": "완료 여부",
    "completionReason": "완료 사유"
  },
  "playerActions": [...],
  "combatEvents": [...],
  "dungeonProgress": [...],
  "enemySpawns": [...],
  "playerStats": [...]
}
```

## 분석 가능한 데이터

### 플레이어 행동
- 총 행동 횟수
- 가장 많이 사용된 스킬
- 총 획득 경험치
- 힐링 사용 횟수

### 전투 통계
- 총 전투 이벤트 수
- 총 입힌 데미지
- 크리티컬 확률
- 평균 데미지

### 던전 진행
- 시도한 층수
- 성공한 층수
- 평균 층당 시간
- 가장 어려운 층

### 적 정보
- 총 스폰된 적 수
- 보스 적 수
- 가장 흔한 적 타입

## 성능 고려사항

- 로그 데이터는 메모리에 누적됩니다
- 자동 저장 간격을 조정하여 성능을 최적화할 수 있습니다
- 최대 로그 엔트리 수를 제한할 수 있습니다
- 필요시 로깅을 비활성화할 수 있습니다

## 문제 해결

### 로그가 저장되지 않는 경우
1. `Application.persistentDataPath` 권한 확인
2. 디스크 공간 확인
3. 로그 디렉토리 생성 권한 확인

### 메모리 사용량이 높은 경우
1. 자동 저장 간격을 줄입니다
2. 최대 로그 엔트리 수를 제한합니다
3. 불필요한 로그를 정기적으로 삭제합니다

### 분석 결과가 부정확한 경우
1. 로그 파일이 올바르게 저장되었는지 확인
2. JSON 형식이 올바른지 확인
3. 로그 데이터의 타임스탬프가 정확한지 확인
