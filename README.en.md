# KoZaeLibrary

[← Back to overview](README.md) · [한국어](README.ko.md)

## Overview

**KoZaeLibrary** is a shared C# codebase used across KoZae game projects. It consists of:

- **Helper libraries** (`KZData`, `KZUtility`, `KZToolKit`) — reusable runtime and editor utilities targeting `netstandard2.1` for Unity compatibility.
- **Console tools** — standalone executables targeting `.NET 9` for build-time automation.

The centerpiece is the **Proto pipeline**: Excel spreadsheets are converted into strongly-typed C# classes, compiled into a `KZProto.dll` plugin, and then serialized into MemoryPack `.bytes` files for runtime loading in Unity.

## Repository Structure

```
KoZaeLibrary/
├── Helper/
│   ├── KZData/          # Shared data types, interfaces, MemoryPack models
│   ├── KZUtility/       # File I/O, crypto, Collection, Foundation, singletons, Lua manager
│   └── KZToolKit/       # Excel/MIDI readers, data generators, barcode
│
└── Console/
    ├── Etc/
    │   └── KZCommon/    # Shared console infrastructure (AppRunner, logging)
    │
    ├── Proto/
    │   ├── KZProtoCommon/    # Proto constants and helpers
    │   ├── KZProtoBuilder/   # Generate C# code from Excel → build KZProto.dll
    │   ├── KZProtoExtractor/ # Extract Excel data → .bytes + .csv
    │   └── KZProtoGenerator/ # Orchestrator: Builder → Extractor
    │
    ├── Cipher/
    │   └── KZCipherGenerator/  # Generate RSA + AES encryption key files
    │
    └── Lua/
        └── KZLuaConverter/     # Copy .lua files as .lua.bytes for Unity
```

## Requirements

| Component | Version |
|-----------|---------|
| .NET SDK (console tools) | 9.0+ |
| Target framework (libraries) | netstandard2.1 |
| Target framework (console) | net9.0 |

Key NuGet dependencies: **MemoryPack**, **ClosedXML**, **Newtonsoft.Json**, **YamlDotNet**, **MoonSharp**, **UniTask**, **ZXing.Net**, **Unity3D.SDK**.

## Build

Each project is built independently with `dotnet build`. There is no solution (`.sln`) file in the repository.

```bash
# Build all helper libraries
dotnet build Helper/KZData/KZData.csproj -c Release
dotnet build Helper/KZUtility/KZUtility.csproj -c Release
dotnet build Helper/KZToolKit/KZToolKit.csproj -c Release

# Build console tools
dotnet build Console/Proto/KZProtoBuilder/KZProtoBuilder.csproj -c Release
dotnet build Console/Proto/KZProtoExtractor/KZProtoExtractor.csproj -c Release
dotnet build Console/Proto/KZProtoGenerator/KZProtoGenerator.csproj -c Release
dotnet build Console/Cipher/KZCipherGenerator/KZCipherGenerator.csproj -c Release
dotnet build Console/Lua/KZLuaConverter/KZLuaConverter.csproj -c Release
```

Build outputs are placed in each project's `bin/Release/net9.0/` folder.

## Proto Pipeline

The proto workflow converts game design data in Excel into deployable binary assets.

```
Excel files (.xlsx)
       │
       ▼
┌──────────────────┐
│  KZProtoBuilder  │  1. Generate C# proto classes from Excel
│                  │  2. Build temporary ProtoProject
│                  │  3. Output KZProto.dll + .pdb → target plugin folder
└──────────────────┘
       │
       ▼
┌──────────────────┐
│ KZProtoExtractor │  1. Load KZProto.dll
│                  │  2. Filter rows by Branch environment
│                  │  3. Serialize data with MemoryPack
└──────────────────┘
       │
       ▼
  ProtoOutput/
  ├── Plugin/   KZProto.dll, KZProto.pdb
  ├── Proto/    *.bytes (binary data)
  └── Csv/      *.csv (backup exports)
```

### KZProtoGenerator (recommended entry point)

Runs Builder and Extractor in sequence.

```bash
KZProtoGenerator <protoFolderRelativePath> <environment> <projectPluginRelativePath>
```

| Argument | Description |
|----------|-------------|
| `protoFolderRelativePath` | Relative path to the folder containing Excel proto files |
| `environment` | Branch name defined in `Branch.xlsx` (e.g. `Dev`, `Live`) |
| `projectPluginRelativePath` | Relative path where `KZProto.dll` will be copied |

**Example:**

```bash
cd Console/Proto/KZProtoGenerator/bin/Release/net9.0
KZProtoGenerator ../../../Data/Proto Dev ../../../Output/Plugin
```

### KZProtoBuilder

Generates C# code, builds a temporary project, and deploys the compiled DLL.

```bash
KZProtoBuilder <protoFolderAbsolutePath> <projectPluginAbsolutePath>
```

**Steps performed:**

1. Scan the proto folder for all `.xlsx` files (grouped by subfolder).
2. Create a temporary `ProtoProject` folder next to the executable's parent directory.
3. Copy embedded default proto classes (`BuffProto`, `MotionProto`, `ColorProto`, `NetworkErrorProto`).
4. Generate `Enum.cs` from `Enum.xlsx` (if present).
5. Generate `{Name}Proto.cs` for each Excel file with `+` prefixed sheets.
6. Run `dotnet build` on the temporary project.
7. Move `KZProto.dll` and `KZProto.pdb` to the plugin folder.
8. Delete the temporary project folder.

### KZProtoExtractor

Reads Excel data using the compiled `KZProto.dll` and writes binary output.

```bash
KZProtoExtractor <protoFolderAbsolutePath> <environment>
```

**Output location:** `{exeParent}/ProtoOutput/`

- `ProtoOutput/Proto/*.bytes` — MemoryPack-serialized proto arrays
- `ProtoOutput/Csv/*.csv` — CSV backup of extracted rows

The extractor loads `KZProto.dll` from its own directory (copied there during the build step of `ProtoProject`).

## Excel Proto Format

### File conventions

| File | Purpose |
|------|---------|
| `Enum.xlsx` | Defines C# enums (one sheet per enum) |
| `Branch.xlsx` | Defines branch/environment filtering rules |
| `{Name}.xlsx` | Data table; generates `{Name}Proto` class |

Files named `Enum` and `Branch` are excluded from proto class generation and binary extraction.

### Sheet naming

- Data sheets must start with the `+` prefix (e.g. `+Item`, `+Motion`).
- The first `+` sheet is the **main class** (`{Name}Proto`).
- Additional `+` sheets define **sub-classes** referenced by the main class.

### Row layout (data sheets)

| Row index | Content | Example |
|-----------|---------|---------|
| 0 | Column scheme (field names) | `Num:pk`, `%Branch`, `ItemName` |
| 1 | Column types | `int`, `string`, `string` |
| 2 | Comments (descriptive labels) | `%번호`, `브랜치`, `아이템 이름` |
| 3+ | Data rows | `1`, `Dev`, `Sword` |

### Column rules

- **Primary key:** Append `:pk` to the scheme name (e.g. `Num:pk`). Must be unique per sheet.
- **Branch filter:** Include a `%Branch` column. Rows are filtered by the environment argument.
- **Comments:** Scheme names starting with `%` are ignored during code generation.
- **Merged cells:** Supported in both row and column directions for scheme/type rows and Branch sheets.

### Supported types

`int`, `float`, `double`, `bool`, `string`, `Vector2`, `Vector3`, custom enum types, array types (`Type[]`), and sub-class references.

### Sub-sheet linking

When a main sheet column type matches a sub-sheet class name, the extractor resolves the reference by primary key. Sub-sheet rows are serialized as JSON and embedded into the parent row during extraction.

### Default embedded protos

These classes ship with `KZProtoBuilder` and are not regenerated from Excel:

- `BuffProto` — buff/debuff definitions
- `MotionProto` — animation state and event data
- `ColorProto` — color palette data
- `NetworkErrorProto` — network error codes

## Branch System

`Branch.xlsx` contains a `Branch` sheet that maps environment names to active branch tags.

When extracting, only rows whose `%Branch` value is enabled for the given environment are included. This allows the same Excel files to serve multiple deployment targets (development, staging, production) without duplicating data files.

## Console Tools

### KZCipherGenerator

Generates RSA and AES encryption key files for secure communication or asset protection.

```bash
KZCipherGenerator <resultFolderRelativePath>
```

**Generated files:**

| File | Description |
|------|-------------|
| `Encryption.key` | AES key (Base64) for decrypting the private key |
| `PublicKey.pem` | RSA public key (PEM format) |
| `EncryptedPrivateKey.pem` | AES-encrypted RSA private key (PEM format) |

Uses `KZCryptoKit` from `KZUtility` for key generation and encryption.

### KZLuaConverter

Copies `.lua` script files and renames them to `.lua.bytes` for Unity `TextAsset` loading.

```bash
KZLuaConverter <luaFolderRelativePath> <resultFolderRelativePath>
```

Preserves the relative directory structure. Non-`.lua` files are skipped.

## Helper Libraries

### KZData

Shared data layer used by both Unity runtime and console tools. Covers proto contracts, settings value types, and MemoryPack models.

#### Scripts folder layout

```
KZData/Scripts/
├── DataConstant.cs      # Core interfaces and enums
├── CommonData.cs        # Proto interfaces
├── BuffEntry.cs         # Buff stat entry
├── MotionEntry.cs       # Motion effect entry
├── ScreenResolution.cs  # Resolution + fullscreen value type
├── SoundVolume.cs       # Channel volume (0.0–1.0) + mute
├── SoundProfile.cs      # Master / music / effect profile
└── Global.cs            # IsExternalInit polyfill for netstandard2.1
```

#### Core interfaces and enums (`DataConstant.cs`)

| Symbol | Description |
|--------|-------------|
| `IProto` | Base primary key (`Num`) for all protos |
| `IConfig` | Marker interface for config data |
| `IAffix` | Affix initialize/update contract |
| `ICluster` | Marker interface for cluster data |
| `EffectType` | `VisualEffect`, `SoundEffect` |
| `NetworkErrorResultType` | `None`, `Popup`, `Toast`, `Title` |

#### Proto interfaces (`CommonData.cs`)

| Interface | Key members |
|-----------|-------------|
| `IBuffProto` | `BuffName`, `Duration`, `MaxStackCount`, `BuffEntryArray` |
| `IMotionProto` | `StateName`, `MotionEntryArray` |
| `IColorProto` | `ColorArray` |
| `INetworkErrorProto` | `Description`, `ResultMainType`, `ResultSubType` |

#### MemoryPack entries

| Type | Fields |
|------|--------|
| `BuffEntry` | `Id`, `StatName`, `Value`, `IsPercent` |
| `MotionEntry` | `Order`, `EffectPath`, `PositionOffset`, `StartBone` |

#### Settings value types

| Type | Description |
|------|-------------|
| `ScreenResolution` | `width`, `height`, `fullscreen`. Presets: `sd`, `hd`, `fhd`, `qhd`, `uhd` (default `fullscreen: true`). `ToString` / `Parse` / `TryParse` |
| `SoundVolume` | `level` (0.0–1.0, clamped), `mute`. Presets: `zero`, `min`, `max`. Preserves `level` while muted. `+`/`-`/`*`/`/` operators, `Toggle()` |
| `SoundProfile` | `master`, `music`, `effect` channels. `DefaultProfile` preset. `OutputMusic` / `OutputEffect` = master × channel. Immutable updates via `WithMaster` / `WithMusic` / `WithEffect` |

**String format examples**

```
resolution : 1920x1080, fullscreen : True
level : 0.80, mute : False
master : level : 1.00, mute : False, music : level : 0.80, mute : False, effect : level : 1.00, mute : False
```

`Parse` / `TryParse` expose `ReadOnlySpan<char>` overloads and parse without allocating via `ToString()`.

### KZUtility

General-purpose utilities for Unity and console environments.

#### Scripts folder layout

```
KZUtility/Scripts/
├── Collection/          # Pure containers (queues, heap, trie, set)
├── Foundation/
│   ├── Index/           # Handles, spatial lookup, connected groups
│   ├── Storage/         # Cache, object pool
│   └── Pattern/         # Design patterns, smart enum, random
├── Utility/Kit/         # KZFileKit, KZCryptoKit, KZRandomKit
├── Singleton/
├── Manager/
├── Converter/
└── Log/
```

#### Kit (`Utility/Kit/`)

| Module | Description |
|--------|-------------|
| `KZFileKit` | File/folder create, read, write, copy, move, delete, compress, search |
| `KZCryptoKit` | AES/RSA encryption, key generation, PEM formatting |
| `KZRandomKit` | Weighted pick, range sampling, and other random helpers (built on `Randomizer`) |

#### Collections (`Collection/`)

| Type | Description |
|------|-------------|
| `BinaryHeap` / `MinHeap` / `MaxHeap` | Thread-safe array-backed binary heap for priority queues and top-N extraction |
| `CircularQueue` | **Fixed-size** ring-buffer FIFO. When full, **overwrites the oldest** entry (keep last N items) |
| `Deque` | **Growable** double-ended queue. O(1) push/pop at both ends. Includes `Enqueue` / `Dequeue` / `Peek` queue API |
| `Trie` | String prefix tree. `Contains`, prefix search, autocomplete |
| `SparseSet` | O(1) add/remove/iterate over live integer indices. Pairs with `SlotMap` slot indices and parallel arrays |

#### Foundation — Index (`Foundation/Index/`)

| Type | Description |
|------|-------------|
| `SlotMap` | Generational handle allocator + value storage. Packs slot index (lower 16 bits) and generation (upper 16 bits) |
| `FastTree` | Uniform 2D grid spatial index. Register points and query by region |
| `UnionFind` | Disjoint-set with `TryFind` / `Union` / `Connected` for tracking connected index groups |

#### Foundation — Storage (`Foundation/Storage/`)

| Type | Description |
|------|-------------|
| `ObjectPool` | Queue-backed object pool. Reuse instances via `GetOrCreate` / `Put` |
| `LanePool` | Concurrent execution lane pool. `TryAcquire` / `ReleaseLane` / `Tick`. Reclaims the oldest active lane when full |
| `LazyRegistry` | Per-key lazy resolve. First `Fetch` invokes a provider; later calls return the cached value |
| `CacheResolver` | **TTL (time-based)** string-key cache. FIFO entries per key, background expiry purge |
| `TransientStore` | Process-wide **single slot** handoff. `Set` then one-shot `Consume` |

##### LanePool

Manages a bounded number of **concurrently active lanes**. Unlike `ObjectPool`, which recycles idle instances, multiple lanes may be active at once; at `maxCount` the pool reclaims the front-of-list (oldest) active lane.

| API | Description |
|-----|-------------|
| `Prepare()` | Pre-creates lanes up to `prepareCount` |
| `TryAcquire(out lane)` | Borrows a lane: idle → grow → steal |
| `Tick()` | Calls `Tick` on active lanes; auto-releases when `!IsPlaying` |
| `ReleaseLane(lane)` | Explicit release |
| `ReleaseAll()` / `Clear()` | Release all / destroy and empty (`Prepare` again after `Clear`) |

**`ILane` contract**

- `IsActive` — pool sets `true` on acquire; `Release()` must set `false`
- `IsPlaying` — `true` while running; **start immediately after acquire**
- `Tick()` / `Release()` / `Destroy()` — may run outside the pool lock

```csharp
var pool = new LanePool<MyLane>(index => new MyLane(index), prepareCount: 8, maxCount: 32);
pool.Prepare();

if (pool.TryAcquire(out var lane))
{
    lane.StartWork(); // IsPlaying = true
}

pool.Tick(); // each frame/tick
```

**Notes:** Failing to start right after acquire may auto-release (`IsActive && !IsPlaying`). Avoid acquiring on other threads during `Clear()`.

#### Foundation — Pattern (`Foundation/Pattern/`)

| Type | Description |
|------|-------------|
| `StrategyCatalog` | Enum-keyed map of strategy implementations. Strategy pattern base |
| `CustomTag` | **Smart enum** base using static fields instead of C# `enum`. Extend tags via derived types |
| `Randomizer` | Thread-safe `Random` wrapper. Int/float ranges, weighted pick, unique group sampling |

#### Singleton · Manager · Converter · Log

| Module | Description |
|--------|-------------|
| `Singleton` / `SingletonMB` / `SingletonSO` | Singleton bases for plain class, `MonoBehaviour`, and `ScriptableObject` |
| `LuaManager` | MoonSharp Lua script execution |
| `TimeManager` | Time and schedule management |
| `YamlConverter` | YAML serialize/deserialize via YamlDotNet. Supports `SoundVolume`, `ScreenResolution`, `SoundProfile` (nested channels), and more |
| `LogBridge` | `OnInfo` / `OnWarning` / `OnError` delegates for Unity and console log wiring |

**KZData YAML examples (`YamlConverter`)**

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

Editor and data-processing toolkits.

| Module | Description |
|--------|-------------|
| `ExcelReader` | Read and deserialize Excel spreadsheets (ClosedXML) |
| `MidiReader` | Parse MIDI files (tracks, notes, meta/system events) |
| `PlayerPrefsReader` | Read Unity PlayerPrefs from the Windows registry |
| `ProtoGenerator` | Generate Excel template files (e.g. Motion proto) |
| `ConfigGenerator` | Config code generation (partially implemented) |
| `LingoGenerator` | Localization data generation |
| `BarcodeGenerator` | QR/barcode image generation (ZXing.Net) |

## Console Infrastructure

All console tools use `AppRunner` from `KZCommon`:

- Validates argument count and prints usage on failure.
- Sets culture to `en-US` for consistent parsing.
- Catches unhandled exceptions and exits with code `-1`.
- Waits for Enter key on completion (unless suppressed).

**Suppress pause on exit:**

```bash
set SKIP_PAUSE=1
```

Or set the `SKIP_PAUSE` environment variable to `true`.

## Unity Integration

1. Reference `KZData`, `KZUtility`, and `KZToolKit` assemblies in your Unity project.
2. Copy `KZProto.dll` (from the plugin output folder) into your Unity `Plugins` folder.
3. Load `.bytes` files from `ProtoOutput/Proto/` at runtime using MemoryPack deserialization.
4. Use `.lua.bytes` files produced by `KZLuaConverter` as Unity `TextAsset` resources.

Helper libraries target `netstandard2.1` with `UNITY_EDITOR` defined in `KZUtility`, ensuring compatibility with Unity's scripting runtime.
