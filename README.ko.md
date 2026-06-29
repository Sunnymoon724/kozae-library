# KoZaeLibrary

[← 개요로 돌아가기](README.md) · [English](README.en.md)

## 개요

**KoZaeLibrary**는 KoZae 게임 프로젝트에서 공통으로 사용하는 C# 라이브러리 및 콘솔 도구 모음입니다.

- **Helper 라이브러리** (`KZData`, `KZUtility`, `KZToolKit`) — Unity 호환을 위해 `netstandard2.1`을 타겟으로 하는 런타임·에디터 공용 유틸리티
- **Console 도구** — 빌드 타임 자동화를 위한 `.NET 9` 독립 실행 파일

핵심 기능은 **Proto 파이프라인**입니다. Excel 스프레드시트에서 강타입 C# 클래스를 생성하고, `KZProto.dll` 플러그인으로 컴파일한 뒤, MemoryPack `.bytes` 바이너리 파일로 직렬화하여 Unity 런타임에서 로드합니다.

## 저장소 구조

```
KoZaeLibrary/
├── Helper/
│   ├── KZData/          # 공용 데이터 타입, 인터페이스, MemoryPack 모델
│   ├── KZUtility/       # 파일 I/O, 암호화, Collection, Foundation, 싱글톤, Lua 매니저
│   └── KZToolKit/       # Excel/MIDI 리더, 데이터 생성기, 바코드
│
└── Console/
    ├── Etc/
    │   └── KZCommon/    # 콘솔 공통 인프라 (AppRunner, 로깅)
    │
    ├── Proto/
    │   ├── KZProtoCommon/    # Proto 상수 및 헬퍼
    │   ├── KZProtoBuilder/   # Excel → C# 코드 생성 → KZProto.dll 빌드
    │   ├── KZProtoExtractor/ # Excel 데이터 → .bytes + .csv 추출
    │   └── KZProtoGenerator/ # 오케스트레이터: Builder → Extractor
    │
    ├── Cipher/
    │   └── KZCipherGenerator/  # RSA + AES 암호화 키 파일 생성
    │
    └── Lua/
        └── KZLuaConverter/     # .lua 파일을 .lua.bytes로 패키징
```

## 요구 사항

| 구성 요소 | 버전 |
|-----------|------|
| .NET SDK (콘솔 도구) | 9.0 이상 |
| 타겟 프레임워크 (라이브러리) | netstandard2.1 |
| 타겟 프레임워크 (콘솔) | net9.0 |

주요 NuGet 의존성: **MemoryPack**, **ClosedXML**, **Newtonsoft.Json**, **YamlDotNet**, **MoonSharp**, **UniTask**, **ZXing.Net**, **Unity3D.SDK**

## 빌드

각 프로젝트는 `dotnet build`로 개별 빌드합니다. 저장소에 솔루션(`.sln`) 파일은 포함되어 있지 않습니다.

```bash
# Helper 라이브러리 빌드
dotnet build Helper/KZData/KZData.csproj -c Release
dotnet build Helper/KZUtility/KZUtility.csproj -c Release
dotnet build Helper/KZToolKit/KZToolKit.csproj -c Release

# 콘솔 도구 빌드
dotnet build Console/Proto/KZProtoBuilder/KZProtoBuilder.csproj -c Release
dotnet build Console/Proto/KZProtoExtractor/KZProtoExtractor.csproj -c Release
dotnet build Console/Proto/KZProtoGenerator/KZProtoGenerator.csproj -c Release
dotnet build Console/Cipher/KZCipherGenerator/KZCipherGenerator.csproj -c Release
dotnet build Console/Lua/KZLuaConverter/KZLuaConverter.csproj -c Release
```

빌드 결과물은 각 프로젝트의 `bin/Release/net9.0/` 폴더에 생성됩니다.

## Proto 파이프라인

Proto 워크플로우는 Excel에 작성된 게임 데이터를 배포 가능한 바이너리 에셋으로 변환합니다.

```
Excel 파일 (.xlsx)
       │
       ▼
┌──────────────────┐
│  KZProtoBuilder  │  1. Excel에서 C# Proto 클래스 생성
│                  │  2. 임시 ProtoProject 빌드
│                  │  3. KZProto.dll + .pdb → 플러그인 폴더로 이동
└──────────────────┘
       │
       ▼
┌──────────────────┐
│ KZProtoExtractor │  1. KZProto.dll 로드
│                  │  2. Branch 환경별 행 필터링
│                  │  3. MemoryPack으로 직렬화
└──────────────────┘
       │
       ▼
  ProtoOutput/
  ├── Plugin/   KZProto.dll, KZProto.pdb
  ├── Proto/    *.bytes (바이너리 데이터)
  └── Csv/      *.csv (백업보내기)
```

### KZProtoGenerator (권장 진입점)

Builder와 Extractor를 순서대로 실행합니다.

```bash
KZProtoGenerator <protoFolderRelativePath> <environment> <projectPluginRelativePath>
```

| 인수 | 설명 |
|------|------|
| `protoFolderRelativePath` | Excel Proto 파일이 있는 폴더의 상대 경로 |
| `environment` | `Branch.xlsx`에 정의된 브랜치 이름 (예: `Dev`, `Live`) |
| `projectPluginRelativePath` | `KZProto.dll`이 복사될 상대 경로 |

**사용 예시:**

```bash
cd Console/Proto/KZProtoGenerator/bin/Release/net9.0
KZProtoGenerator ../../../Data/Proto Dev ../../../Output/Plugin
```

### KZProtoBuilder

C# 코드를 생성하고 임시 프로젝트를 빌드한 뒤, 컴파일된 DLL을 배포합니다.

```bash
KZProtoBuilder <protoFolderAbsolutePath> <projectPluginAbsolutePath>
```

**수행 단계:**

1. Proto 폴더에서 모든 `.xlsx` 파일을 스캔 (하위 폴더별 그룹화).
2. 실행 파일 상위 디렉터리에 임시 `ProtoProject` 폴더 생성.
3. 내장 기본 Proto 클래스 복사 (`BuffProto`, `MotionProto`, `ColorProto`, `NetworkErrorProto`).
4. `Enum.xlsx`가 있으면 `Enum.cs` 생성.
5. `+` 접두사 시트가 있는 각 Excel 파일에 대해 `{Name}Proto.cs` 생성.
6. 임시 프로젝트에 `dotnet build` 실행.
7. `KZProto.dll`과 `KZProto.pdb`를 플러그인 폴더로 이동.
8. 임시 프로젝트 폴더 삭제.

### KZProtoExtractor

컴파일된 `KZProto.dll`을 사용하여 Excel 데이터를 읽고 바이너리 출력을 생성합니다.

```bash
KZProtoExtractor <protoFolderAbsolutePath> <environment>
```

**출력 위치:** `{exe상위폴더}/ProtoOutput/`

- `ProtoOutput/Proto/*.bytes` — MemoryPack 직렬화된 Proto 배열
- `ProtoOutput/Csv/*.csv` — 추출된 행의 CSV 백업

Extractor는 자체 디렉터리의 `KZProto.dll`을 로드합니다 (ProtoProject 빌드 시 해당 위치로 출력됨).

## Excel Proto 형식

### 파일 규칙

| 파일 | 용도 |
|------|------|
| `Enum.xlsx` | C# enum 정의 (시트 하나당 enum 하나) |
| `Branch.xlsx` | 브랜치/환경별 필터링 규칙 |
| `{Name}.xlsx` | 데이터 테이블; `{Name}Proto` 클래스 생성 |

`Enum`, `Branch` 이름의 파일은 Proto 클래스 생성 및 바이너리 추출에서 제외됩니다.

### 시트 명명 규칙

- 데이터 시트는 `+` 접두사로 시작해야 합니다 (예: `+Item`, `+Motion`).
- 첫 번째 `+` 시트가 **메인 클래스** (`{Name}Proto`)입니다.
- 추가 `+` 시트는 메인 클래스에서 참조하는 **서브 클래스**를 정의합니다.

### 행 구조 (데이터 시트)

| 행 인덱스 | 내용 | 예시 |
|-----------|------|------|
| 0 | 컬럼 스킴 (필드 이름) | `Num:pk`, `%Branch`, `ItemName` |
| 1 | 컬럼 타입 | `int`, `string`, `string` |
| 2 | 주석 (설명 라벨) | `%번호`, `브랜치`, `아이템 이름` |
| 3+ | 데이터 행 | `1`, `Dev`, `Sword` |

### 컬럼 규칙

- **기본 키:** 스킴 이름에 `:pk` 접미사 추가 (예: `Num:pk`). 시트 내에서 유일해야 합니다.
- **브랜치 필터:** `%Branch` 컬럼을 포함합니다. 환경 인수에 따라 행이 필터링됩니다.
- **주석:** `%`로 시작하는 스킴 이름은 코드 생성 시 무시됩니다.
- **병합 셀:** 스킴/타입 행 및 Branch 시트에서 행·열 방향 병합 셀을 지원합니다.

### 지원 타입

`int`, `float`, `double`, `bool`, `string`, `Vector2`, `Vector3`, 사용자 정의 enum, 배열 타입 (`Type[]`), 서브 클래스 참조

### 서브 시트 연결

메인 시트 컬럼 타입이 서브 시트 클래스 이름과 일치하면, Extractor가 기본 키로 참조를 해석합니다. 서브 시트 행은 JSON으로 직렬화되어 추출 시 부모 행에 삽입됩니다.

### 기본 내장 Proto

`KZProtoBuilder`에 포함되어 있으며 Excel에서 재생성되지 않는 클래스:

- `BuffProto` — 버프/디버프 정의
- `MotionProto` — 애니메이션 상태 및 이벤트 데이터
- `ColorProto` — 색상 팔레트 데이터
- `NetworkErrorProto` — 네트워크 에러 코드

## Branch 시스템

`Branch.xlsx`의 `Branch` 시트는 환경 이름과 활성 브랜치 태그를 매핑합니다.

추출 시 지정된 환경(environment)에서 활성화된 `%Branch` 값을 가진 행만 포함됩니다. 동일한 Excel 파일로 개발·스테이징·운영 등 여러 배포 대상을 데이터 파일 복제 없이 관리할 수 있습니다.

## 콘솔 도구

### KZCipherGenerator

보안 통신 또는 에셋 보호를 위한 RSA 및 AES 암호화 키 파일을 생성합니다.

```bash
KZCipherGenerator <resultFolderRelativePath>
```

**생성 파일:**

| 파일 | 설명 |
|------|------|
| `Encryption.key` | 개인 키 복호화용 AES 키 (Base64) |
| `PublicKey.pem` | RSA 공개 키 (PEM 형식) |
| `EncryptedPrivateKey.pem` | AES로 암호화된 RSA 개인 키 (PEM 형식) |

`KZUtility`의 `KZCryptoKit`을 사용하여 키를 생성하고 암호화합니다.

### KZLuaConverter

`.lua` 스크립트 파일을 복사하고 `.lua.bytes`로 이름을 변경하여 Unity `TextAsset` 로딩에 사용합니다.

```bash
KZLuaConverter <luaFolderRelativePath> <resultFolderRelativePath>
```

상대 디렉터리 구조를 유지합니다. `.lua`가 아닌 파일은 건너뜁니다.

## Helper 라이브러리

### KZData

Unity 런타임과 콘솔 도구 모두에서 사용하는 공용 데이터 계층입니다. Proto 계약, 설정용 값 타입, MemoryPack 모델을 포함합니다.

#### Scripts 폴더 구조

```
KZData/Scripts/
├── DataConstant.cs      # 핵심 인터페이스, enum
├── CommonData.cs        # Proto 인터페이스
├── BuffEntry.cs         # 버프 스탯 엔트리
├── MotionEntry.cs       # 모션 이펙트 엔트리
├── ScreenResolution.cs  # 해상도 + 전체화면 값 타입
├── SoundVolume.cs       # 채널 볼륨 (0.0–1.0) + 음소거
├── SoundProfile.cs      # 마스터/음악/효과음 프로필
└── Global.cs            # netstandard2.1용 IsExternalInit 폴리필
```

#### 핵심 인터페이스 · enum (`DataConstant.cs`)

| 심볼 | 설명 |
|------|------|
| `IProto` | 모든 Proto의 기본 키 (`Num`) |
| `IConfig` | 설정 데이터 마커 인터페이스 |
| `IAffix` | Affix 초기화/갱신 계약 |
| `ICluster` | 클러스터 데이터 마커 인터페이스 |
| `EffectType` | `VisualEffect`, `SoundEffect` |
| `NetworkErrorResultType` | `None`, `Popup`, `Toast`, `Title` |

#### Proto 인터페이스 (`CommonData.cs`)

| 인터페이스 | 주요 멤버 |
|------------|-----------|
| `IBuffProto` | `BuffName`, `Duration`, `MaxStackCount`, `BuffEntryArray` |
| `IMotionProto` | `StateName`, `MotionEntryArray` |
| `IColorProto` | `ColorArray` |
| `INetworkErrorProto` | `Description`, `ResultMainType`, `ResultSubType` |

#### MemoryPack 엔트리

| 타입 | 필드 |
|------|------|
| `BuffEntry` | `Id`, `StatName`, `Value`, `IsPercent` |
| `MotionEntry` | `Order`, `EffectPath`, `PositionOffset`, `StartBone` |

#### 설정 값 타입

| 타입 | 설명 |
|------|------|
| `ScreenResolution` | `width`, `height`, `fullscreen`. 프리셋: `sd`, `hd`, `fhd`, `qhd`, `uhd` (기본 `fullscreen: true`). `ToString` / `Parse` / `TryParse` 지원 |
| `SoundVolume` | `level` (0.0–1.0, 클램프), `mute`. 프리셋: `zero`, `min`, `max`. 음소거 시 `level` 유지. `+`/`-`/`*`/`/` 연산자, `Toggle()` |
| `SoundProfile` | `master`, `music`, `effect` 채널. `maxSoundProfile` 프리셋. `outputMusic` / `outputEffect` = 마스터 × 채널. `WithMaster` / `WithMusic` / `WithEffect` 불변 업데이트 |

**문자열 형식 예시**

```
resolution : 1920x1080, fullscreen : True
level : 0.80, mute : False
master : level : 1.00, mute : False, music : level : 0.80, mute : False, effect : level : 1.00, mute : False
```

`Parse` / `TryParse`는 `ReadOnlySpan<char>` 오버로드를 제공하며, 할당 없이 Span 기반으로 파싱합니다.

### KZUtility

Unity 및 콘솔 환경을 위한 범용 유틸리티입니다.

#### Scripts 폴더 구조

```
KZUtility/Scripts/
├── Collection/          # 순수 컨테이너 (큐, 힙, 트리, 집합)
├── Foundation/
│   ├── Index/           # handle, 공간, 연결 그룹 추적
│   ├── Storage/         # 캐시, 폰/레인 풀
│   └── Pattern/         # 설계 패턴, smart enum, 난수
├── Utility/Kit/         # KZFileKit, KZCryptoKit, KZRandomKit
├── Singleton/
├── Manager/
├── Converter/
└── Log/
```

#### Kit (`Utility/Kit/`)

| 모듈 | 설명 |
|------|------|
| `KZFileKit` | 파일/폴더 생성, 읽기, 쓰기, 복사, 이동, 삭제, 압축, 검색 |
| `KZCryptoKit` | AES/RSA 암호화, 키 생성, PEM 포맷팅 |
| `KZRandomKit` | 가중치 선택, 구간 샘플링 등 난수 헬퍼 (`Randomizer` 기반) |

#### Collections (`Collection/`)

| 타입 | 설명 |
|------|------|
| `BinaryHeap` / `MinHeap` / `MaxHeap` | 스레드 세이프 배열 기반 이진 힙. 우선순위 큐·Top-N 추출에 사용 |
| `CircularQueue` | **고정 크기** ring buffer FIFO. 가득 차면 **가장 오래된 항목을 덮어씀** (최근 N개 유지) |
| `Deque` | **확장 가능** 양방향 큐. 앞·뒤 O(1) 삽입/제거. `Enqueue`/`Dequeue`/`Peek` Queue API 포함 |
| `Trie` | 문자열 prefix 트리. `Contains`, prefix 검색, 자동완성 |
| `SparseSet` | live integer index만 O(1) 추가/제거·순회. `SlotMap` slot index + parallel array와 함께 사용 |

#### Foundation — Index (`Foundation/Index/`)

| 타입 | 설명 |
|------|------|
| `SlotMap` | generational handle 발급 + 값 저장. slot index(하위 16비트)와 generation(상위 16비트) 패킹 |
| `FastTree` | 2D 균일 격자 공간 인덱스. 영역 내 포인트 등록·범위 조회 |
| `UnionFind` | disjoint-set. `TryFind` / `Union` / `Connected`로 index 그룹 연결 여부 추적 |

#### Foundation — Storage (`Foundation/Storage/`)

| 타입 | 설명 |
|------|------|
| `PawnPool` | `Queue` 기반 폰 풀. `GetOrCreate` / `Put`으로 idle 폰 재사용. checkout 상한 없음, checkout 중인 폰은 추적하지 않음 |
| `LanePool` | 동시 실행 슬롯(레인) 풀. `TryAcquire` / `Return` / `Tick`. 꽉 차면 `false`; finished active 레인은 `Tick`에서 `Return`으로 자동 반납 |
| `LazyRegistry` | key별 lazy resolve. 첫 `Fetch`에서 provider 호출, 이후 캐시된 값 반환 |
| `CacheResolver` | string key당 **FIFO TTL 큐**. `TryGetCache`는 소비형 dequeue. `updatePeriod == 0`이면 타이머 없이 접근 시 purge만 |
| `TransientStore` | 프로세스 전역 **1칸** handoff. `Set` → `Consume` 1회 소비 |

##### PawnPool

**idle 폰만** 큐에 보관합니다. `LanePool`처럼 active 인스턴스를 추적하지 않으므로, `Clear`/`Dispose` 전에 사용 중인 폰은 `Put`으로 반환하거나 호출자가 직접 정리해야 합니다. checkout 상한은 없으며, 큐가 비어 있으면 `GetOrCreate`는 ctor에 넘긴 `onCopy`로 pivot에서 새 폰을 복제합니다. `capacity`는 `autoFill` 시 미리 채울 idle 개수, purge 후 유지할 최소 idle 개수이며, `capacity > 1`이면 ctor에서 `onCopy`가 서로 다른 인스턴스를 반환하는지 검증합니다.

| API | 설명 |
|-----|------|
| `GetOrCreate()` | idle 폰 dequeue → 없으면 `onCopy(pivot)`으로 복제 |
| `Put(pawn)` | idle 큐에 enqueue |
| `PurgeForce()` | idle 폰이 `capacity` 초과분이면 `onDestroy`로 즉시 정리 |
| `Clear()` | 큐에 있는 idle 폰만 `onDestroy` (풀은 재사용 가능) |
| `Dispose()` | `_Clear` 후 disposed 표시 |

**`IPawn`**

- 풀 대상 pawn **마커** 인터페이스 (멤버 없음)

**ctor 콜백**

- `pivot` — 복제 템플릿. 풀이 destroy하지 않음
- `onCopy` — pivot에서 새 pawn 복제 (`capacity > 1`이면 매번 서로 다른 인스턴스)
- `onDestroy` — idle 큐에서 제거될 때 호출 (`Clear` / `PurgeForce` / `Dispose`)

**확장 (선택, consumer / UMP)**

- `PawnPool` 서브클래스 — `onCopy`/`onDestroy`를 private static 메서드 method group으로 넘기거나, `GetOrCreate` / `Put` override로 checkout·반환 lifecycle 추가

```csharp
public sealed class ComponentPawn : IPawn
{
    public ComponentPawn(Component component) => Component = component;
    public Component Component { get; }
}

var pool = new PawnPool<ComponentPawn>(
    new ComponentPawn(templateComponent),
    capacity: 16,
    onCopy: pivot => new ComponentPawn(pivot.Component.CopyObject() as Component),
    onDestroy: pawn =>
    {
        if (pawn.Component && pawn.Component.gameObject)
            pawn.Component.DestroyObject();
    });

var pawn = pool.GetOrCreate();
// pawn.Component ...
pool.Put(pawn);
```

```csharp
// UMP: copy/destroy를 서브클래스에 두는 경우
public sealed class ComponentPawnPool : PawnPool<ComponentPawn>
{
    public ComponentPawnPool(ComponentPawn pivot, int capacity)
        : base(pivot, capacity, _Copy, _Destroy) { }

    private static ComponentPawn _Copy(ComponentPawn pivot)
        => new ComponentPawn(pivot.Component.CopyObject() as Component);

    private static void _Destroy(ComponentPawn pawn)
    {
        if (pawn.Component && pawn.Component.gameObject)
            pawn.Component.DestroyObject();
    }
}
```

##### LanePool

제한된 수의 **동시 active 슬롯**을 관리합니다. `PawnPool`이 idle 인스턴스만 재활용하는 것과 달리, 생성된 모든 레인을 리스트로 소유합니다. `maxCount`에 도달하고 전부 active이면 `TryAcquire`는 `false`를 반환합니다.

| API | 설명 |
|-----|------|
| `Prepare()` | `prepareCount`까지 레인 미리 생성 |
| `TryAcquire(param, out lane)` | idle 레인 빌림 → `Initialize` → 없으면 신규 생성 → 꽉 차면 `false` |
| `Tick()` | active 레인 중 `IsActive && !IsPlaying`이면 `Return`으로 자동 반납 |
| `Return(lane)` | 락 안에서 `IsActive` 확인 후 `ILane.Release` 호출 (권장 진입점) |
| `TryFind` / `TryFindActive` / `ForEachActive` | 스냅샷 기반 조회·순회 (lock 밖에서 predicate/action 실행) |
| `TryReleaseActive` / `TryReleaseAllActive` | 조건에 맞는 active 레인을 `Return` |
| `ReleaseAll()` | 현재 active 레인 전부 `Return` |
| `Clear()` / `Dispose()` | active 레인 `Return` → 전체 `Destroy` 후 비우기 (`Clear` 후 `Prepare` 선택) |

**`ILane<TPayload>` 계약**

- `Create(payload, storage)` — 레인을 payload·부모 Transform에 바인딩 (보통 `_Create` override에서 호출)
- `Initialize(param)` — acquire 시 `IsActive = true`, 작업 시작 (`IsPlaying = true`)
- `IsPlaying` — 실행 중 여부를 반영하는 속성 (예: `AudioSource.isPlaying`). 풀이 `Tick`에서 읽음
- `Release()` — `IsActive = false`, **멱등**. `Return`이 호출하거나 외부에서 직접 호출 가능
- `Destroy()` — `Clear`/`Dispose` 시 풀이 호출

```csharp
var pool = new LanePool<MyAudioLane, AudioSource>(storageTransform, prepareCount: 8, maxCount: 32);
pool.Prepare();

if (pool.TryAcquire(myParam, out var lane))
{
    // Initialize에서 재생 시작 → IsPlaying = true
}

pool.Tick(); // 매 프레임/틱: finished면 Return → ILane.Release
pool.Return(lane); // 명시적 반납
```

**주의:** `Initialize`에서 즉시 시작하지 않으면 다음 `Tick`에서 `IsActive && !IsPlaying` 상태로 자동 반납될 수 있음. finished 레인 회수는 `Tick`에만 의존하므로 주기적으로 호출할 것. `Clear()`/`Dispose()` 중 다른 스레드 acquire는 피할 것.

##### CacheResolver

string key마다 **FIFO 큐**로 TTL entry를 보관합니다. `LanePool`/`PawnPool`과 달리 checkout 추적은 없고, `TryGetCache`는 peek이 아니라 **소비형 dequeue** — 가장 오래된 유효 entry를 꺼낸 뒤 큐에서 제거합니다. 만료된 entry는 dequeue·타이머 purge 시 버려지며, `TCache` 리소스 정리는 호출자 책임입니다.

| API | 설명 |
|-----|------|
| `CacheResolver(deleteTime, updatePeriod)` | entry TTL(초, 기본 60)과 백그라운드 purge 주기(초, 기본 30). `updatePeriod == 0`이면 타이머 비활성 |
| `StoreCache(key, cache, isUpdate)` | entry enqueue. `isUpdate: true`이면 같은 key의 **pending entry TTL을 모두 연장**한 뒤 신규 entry 추가 |
| `TryGetCache(key, out cache)` | 큐 앞에서 만료분을 건너뛰고, 가장 오래된 유효 entry dequeue. key 없음·전부 만료면 `false` |
| `Dispose()` | purge 타이머 정지 → in-flight 콜백 대기 → 전체 큐 비우기 |

**주의**

- `key`는 null 불가. `deleteTime` / `updatePeriod` 음수는 ctor에서 예외
- `sealed` 타입이라 확장은 래퍼/컴포지션으로 처리 (`protected virtual _Dispose` 없음)

```csharp
using var cache = new CacheResolver<DownloadResult>(deleteTime: 60f, updatePeriod: 30f);

cache.StoreCache("asset:logo", result, isUpdate: false);

// 같은 key에 연속 저장 + pending TTL 연장
cache.StoreCache("asset:logo", newerResult, isUpdate: true);

if (cache.TryGetCache("asset:logo", out var item))
{
    // item은 캐시에서 제거됨 (재조회 불가)
}
```

#### Foundation — Pattern (`Foundation/Pattern/`)

| 타입 | 설명 |
|------|------|
| `StrategyCatalog` | enum key → strategy 구현체 맵. Strategy 패턴 베이스 |
| `CustomTag` | C# enum 대신 static 필드 기반 **smart enum**. 파생 타입으로 태그 확장 |
| `Randomizer` | 스레드 세이프 `Random` 래퍼. 정수/실수 구간, 가중치, unique 그룹 샘플링 |

#### Singleton · Manager · Converter · Log

| 모듈 | 설명 |
|------|------|
| `Singleton` / `SingletonMB` | 일반 클래스 / `MonoBehaviour` 싱글톤 베이스. `SingletonMB`는 `SingletonMBConfigAttribute`로 자동 생성·DontDestroy·프리팹 경로 설정 |
| `LuaManager` | MoonSharp Lua 스크립트 실행 |
| `TimeManager` | 시간·스케줄 관리 |
| `YamlConverter` | YamlDotNet 기반 YAML 직렬화/역직렬화. `SoundVolume`, `ScreenResolution`, `SoundProfile`(중첩 채널) 등 지원 |
| `LogBridge` | `OnInfo` / `OnWarning` / `OnError` delegate. Unity·콘솔 로그 연동용 브릿지 |

**KZData 타입 YAML 예시 (`YamlConverter`)**

```yaml
# ScreenResolution
width: 1920
height: 1080
fullscreen: true

# SoundVolume
level: 0.8
mute: false

# SoundProfile
master:
  level: 1.0
  mute: false
music:
  level: 0.8
  mute: false
effect:
  level: 1.0
  mute: false
```

### KZToolKit

에디터 및 데이터 처리 툴킷입니다.

| 모듈 | 설명 |
|------|------|
| `ExcelReader` | Excel 스프레드시트 읽기 및 역직렬화 (ClosedXML) |
| `MidiReader` | MIDI 파일 파싱 (트랙, 노트, 메타/시스템 이벤트) |
| `PlayerPrefsReader` | Windows 레지스트리에서 Unity PlayerPrefs 읽기 |
| `ProtoGenerator` | Excel 템플릿 파일 생성 (예: Motion proto) |
| `ConfigGenerator` | Config 코드 생성 (부분 구현) |
| `LingoGenerator` | 로컬라이제이션 데이터 생성 |
| `BarcodeGenerator` | QR/바코드 이미지 생성 (ZXing.Net) |

## 콘솔 인프라

모든 콘솔 도구는 `KZCommon`의 `AppRunner`를 사용합니다:

- 인수 개수를 검증하고, 실패 시 사용법을 출력합니다.
- 일관된 파싱을 위해 culture를 `en-US`로 설정합니다.
- 처리되지 않은 예외를 잡아 종료 코드 `-1`로 종료합니다.
- 완료 후 Enter 키 입력을 대기합니다 (비활성화하지 않은 경우).

**종료 대기 비활성화:**

```bash
set SKIP_PAUSE=1
```

또는 `SKIP_PAUSE` 환경 변수를 `true`로 설정합니다.

## Unity 연동

1. Unity 프로젝트에서 `KZData`, `KZUtility`, `KZToolKit` 어셈블리를 참조합니다.
2. `KZProto.dll`(플러그인 출력 폴더)을 Unity `Plugins` 폴더에 복사합니다.
3. `ProtoOutput/Proto/`의 `.bytes` 파일을 런타임에 MemoryPack 역직렬화로 로드합니다.
4. `KZLuaConverter`가 생성한 `.lua.bytes` 파일을 Unity `TextAsset` 리소스로 사용합니다.

Helper 라이브러리는 `netstandard2.1`을 타겟으로 하며, `KZUtility`에 `UNITY_EDITOR`가 정의되어 있어 Unity 스크립팅 런타임과 호환됩니다.
