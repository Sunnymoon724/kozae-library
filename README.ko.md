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
│   ├── KZUtility/       # 파일 I/O, 암호화, 컬렉션, 싱글톤, Lua 매니저
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

Unity 런타임과 콘솔 도구 모두에서 사용하는 공용 데이터 계층입니다.

- Proto 인터페이스: `IProto`, `IBuffProto`, `IMotionProto`, `IColorProto`, `INetworkErrorProto`
- 엔트리 타입: `BuffEntry`, `MotionEntry`
- 게임 상수: `DataConstant`, `ScreenResolution`, `SoundProfile`, `SoundVolume`
- MemoryPack 직렬화 지원

### KZUtility

Unity 및 콘솔 환경을 위한 범용 유틸리티입니다.

| 모듈 | 설명 |
|------|------|
| `KZFileKit` | 파일/폴더 생성, 읽기, 쓰기, 복사, 이동, 삭제, 압축, 검색 |
| `KZCryptoKit` | AES/RSA 암호화, 키 생성, PEM 포맷팅 |
| `KZRandomKit` | 난수 유틸리티 |
| Collections | `BinaryHeap`, `CircularQueue`, `FastTree`, `Trie` |
| Patterns | `Singleton`, `SingletonMB`, `SingletonSO`, `ObjectPool`, `LazyRegistry`, `StrategyCatalog`, `TransientStore` |
| Managers | `LuaManager` (MoonSharp), `TimeManager` |
| Converters | `YamlConverter` (YamlDotNet) |
| Diagnostics | `LogBridge` |

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
