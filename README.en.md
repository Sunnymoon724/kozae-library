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
│   ├── KZUtility/       # File I/O, crypto, collections, singletons, Lua manager
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

Shared data layer used by both Unity runtime and console tools.

- Proto interfaces: `IProto`, `IBuffProto`, `IMotionProto`, `IColorProto`, `INetworkErrorProto`
- Entry types: `BuffEntry`, `MotionEntry`
- Game constants: `DataConstant`, `ScreenResolution`, `SoundProfile`, `SoundVolume`
- MemoryPack serialization support

### KZUtility

General-purpose utilities for Unity and console environments.

| Module | Description |
|--------|-------------|
| `KZFileKit` | File/folder create, read, write, copy, move, delete, compress, search |
| `KZCryptoKit` | AES/RSA encryption, key generation, PEM formatting |
| `KZRandomKit` | Random number utilities |
| Collections | `BinaryHeap`, `CircularQueue`, `FastTree`, `Trie` |
| Patterns | `Singleton`, `SingletonMB`, `SingletonSO`, `ObjectPool`, `LazyRegistry`, `StrategyCatalog`, `TransientStore` |
| Managers | `LuaManager` (MoonSharp), `TimeManager` |
| Converters | `YamlConverter` (YamlDotNet) |
| Diagnostics | `LogBridge` |

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
