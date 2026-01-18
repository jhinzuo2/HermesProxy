# HermesProxy ![Build](https://github.com/WowLegacyCore/HermesProxy/actions/workflows/Build_Proxy.yml/badge.svg)

This project enables play on existing legacy WoW emulation cores using the modern clients. It serves as a translation layer, converting all network traffic to the appropriate format each side can understand.

There are 4 major components to the application:
- The modern BNetServer to which the client initially logs into.
- The legacy AuthClient which will in turn login to the remote authentication server (realmd).
- The modern WorldServer to which the game client will connect once a realm has been selected.
- The legacy WorldClient which communicates with the remote world server (mangosd).

## Supported Versions

HermesProxy translates between modern WoW Classic clients and legacy private server emulators.

### Modern Client Versions (What You Play With)

These are the Blizzard WoW Classic client versions you can use:

| Version | Expansion      | Build Range       | Notes                    |
|---------|----------------|-------------------|--------------------------|
| 1.14.0  | Classic Era    | 39802 - 40618     | Season of Mastery        |
| 1.14.1  | Classic Era    | 40487 - 42032     |                          |
| 1.14.2  | Classic Era    | 41858 - 42597     |                          |
| 2.5.2   | TBC Classic    | 39570 - 41510     |                          |
| 2.5.3   | TBC Classic    | 41402 - 42598     |                          |

### Legacy Server Versions (What Emulators Run)

These are the private server versions HermesProxy can connect to:

| Version | Expansion | Build | Server Software          |
|---------|-----------|-------|--------------------------|
| 1.12.1  | Vanilla   | 5875  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.2  | Vanilla   | 6005  | CMaNGOS, VMaNGOS, etc.   |
| 1.12.3  | Vanilla   | 6141  | CMaNGOS, VMaNGOS, etc.   |
| 2.4.3   | TBC       | 8606  | CMaNGOS, etc.            |

### Version Mapping

The proxy automatically selects the best legacy version based on your client:

| Modern Client | Connects To    |
|---------------|----------------|
| 1.14.x        | 1.12.x (Vanilla) |
| 2.5.x         | 2.4.3 (TBC)    |

## Ingame Settings
Note: Keep `Optimize Network for Speed` **enabled** (it's under `System` -> `Network`), otherwise you will get kicked every now and then.

## Usage Instructions

- Edit the app's config to specify the exact versions of your game client and the remote server, along with the address.
- Go into your game folder, in the Classic or Classic Era subdirectory, and edit WTF/Config.wtf to set the portal to 127.0.0.1.
- Download [Arctium Launcher](https://github.com/Arctium/WoW-Launcher/releases/tag/latest) into the main game folder, and then run it
with `--staticseed --version=ClassicEra` for vanilla
or `--staticseed --version=Classic` for TBC.
- Start the proxy app and login through the game with your usual credentials.

## Chat Commands

HermesProxy provides some internal chat commands:

| Command                    | Description                                                                  |
|----------------------------|------------------------------------------------------------------------------|
| `!qcomplete <questId>`     | Manually marks a quest as already completed (useful for quest helper addons) |
| `!quncomplete <questId>`   | Unmarks a quest as completed                                                 |

## Command Line Arguments

| Argument                   | Description                                                  |
|----------------------------|--------------------------------------------------------------|
| `--config <filename>`      | Specify a config file (default: `HermesProxy.config`)        |
| `--set <key>=<value>`      | Override a specific config value at runtime                  |
| `--no-version-check`       | Disable the check for newer versions on startup              |

**Examples:**
```bash
# Use a custom config file
HermesProxy --config MyServer.config

# Override server address
HermesProxy --set ServerAddress=logon.example.com

# Override multiple values
HermesProxy --set ServerAddress=logon.example.com --set ServerPort=3725

# Combine config file with overrides
HermesProxy --config Production.config --set DebugOutput=true
```

## Configuration Reference

Configuration is stored in `HermesProxy.config` (XML format). All settings can also be overridden via `--set` command line arguments.

### Connection Settings

| Setting         | Default       | Description                                                              |
|-----------------|---------------|--------------------------------------------------------------------------|
| `ServerAddress` | `127.0.0.1`   | Address of the legacy server (what you'd use in `SET REALMLIST`)         |
| `ServerPort`    | `3724`        | Port of the legacy authentication server                                 |
| `ExternalAddress` | `127.0.0.1` | Your IP address for others to connect (for hosting)                      |

### Version Settings

| Setting       | Default                            | Valid Values                                    |
|---------------|------------------------------------|-------------------------------------------------|
| `ClientBuild` | `40618` (1.14.0)                   | `40618` (1.14.0), `41794` (1.14.1), `42597` (1.14.2), `40892` (2.5.2), `42328` (2.5.3) |
| `ServerBuild` | `auto`                             | `auto`, `5875` (1.12.1), `8606` (2.4.3)         |
| `ClientSeed`  | `179D3DC3235629D07113A9B3867F97A7` | 32-character hex string (16 bytes)              |

### Port Settings

All ports must be in the range `1-65535`.

| Setting        | Default | Description                                                        |
|----------------|---------|--------------------------------------------------------------------|
| `BNetPort`     | `1119`  | Port for BNet/Portal server (use this in your `Config.wtf`)        |
| `RestPort`     | `8081`  | Port for the REST API server                                       |
| `RealmPort`    | `8084`  | Port for the realm server                                          |
| `InstancePort` | `8086`  | Port for the instance server                                       |

### Platform Settings

| Setting            | Default | Valid Values                    | Description                              |
|--------------------|---------|---------------------------------|------------------------------------------|
| `ReportedOS`       | `OSX`   | `OSX`, `Win`, etc.              | OS identifier sent to the legacy server  |
| `ReportedPlatform` | `x86`   | `x86`, `x64`                    | Platform identifier sent to legacy server|

### Logging Settings

| Setting        | Default | Description                                                          |
|----------------|---------|----------------------------------------------------------------------|
| `DebugOutput`  | `false` | Print additional debug information to console                        |
| `PacketsLog`   | `true`  | Save each session's packets to files in `PacketsLog` directory       |
| `SpanStatsLog` | `false` | Log statistics about Span-based packet serialization (for profiling) |

### Example Configuration

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="ClientBuild" value="40618" />
    <add key="ServerBuild" value="auto" />
    <add key="ServerAddress" value="logon.example.com" />
    <add key="ServerPort" value="3724" />
    <add key="BNetPort" value="1119" />
    <add key="DebugOutput" value="false" />
    <add key="PacketsLog" value="true" />
  </appSettings>
</configuration>
```

## Performance Optimizations

HermesProxy has been extensively optimized to minimize latency and memory allocations in packet handling hot paths.

### Span-Based Packet I/O (Zero-Allocation)

The packet serialization system uses `Span<T>` and `ref struct` types for zero-allocation packet writing and reading:

**SpanPacketWriter vs ByteBuffer (Write Operations)**

| Operation    | ByteBuffer | SpanWriter | Speedup | Memory      |
|--------------|------------|------------|---------|-------------|
| WriteInt64   | 93.37 ns   | 0.29 ns    | ~317x   | 80B → 0B    |
| WriteVector3 | 102.99 ns  | 0.68 ns    | ~151x   | 88B → 0B    |
| WriteMixed   | 109.30 ns  | 1.29 ns    | ~85x    | 96B → 0B    |

**SpanPacketReader vs ByteBuffer (Read Operations)**

| Operation    | ByteBuffer | SpanReader | Speedup  | Memory      |
|--------------|------------|------------|----------|-------------|
| ReadInt64    | 157.98 ns  | 0.08 ns    | ~1948x   | 48B → 0B    |
| ReadVector3  | 178.31 ns  | 0.75 ns    | ~238x    | 48B → 0B    |
| ReadCString  | 294.61 ns  | 23.51 ns   | ~12.5x   | 104B → 56B  |

### ByteBuffer Optimizations

The core `ByteBuffer` class has been refactored for improved performance:
- ArrayPool-based buffer management reduces GC pressure
- Direct `BinaryPrimitives` usage eliminates BinaryReader/BinaryWriter overhead
- `MemoryStream.ToArray()` optimization for `GetData()`:

| Buffer Size | Original     | Optimized   | Speedup |
|-------------|--------------|-------------|---------|
| Small       | 46.87 ns     | 10.31 ns    | ~4.5x   |
| Medium      | 649.49 ns    | 70.88 ns    | ~9.2x   |
| Large       | 36,383.19 ns | 4,234.92 ns | ~8.6x   |

### Additional Optimizations

- **Enum Conversions**: Cached name-based mappings replace `Enum.Parse(typeof(T), x.ToString())` pattern (8-25x speedup, 95% memory reduction)
- **Opcode Lookups**: `FrozenDictionary` for O(1) opcode resolution
- **WowGuid**: Refactored to value-type record structs eliminating heap allocations
- **NetworkThread**: O(1) socket removal with `ConcurrentQueue`
- **BnetTcpSession**: Zero-allocation buffer management with `Span<T>`
- **Movement Handlers**: Fixed monster/pet movement zig-zag at tile boundaries

## Acknowledgements

Parts of this project's code are based on [CypherCore](https://github.com/CypherCore/CypherCore) and [BotFarm](https://github.com/jackpoz/BotFarm). I would like to extend my sincere thanks to these projects, as the creation of this app might have never happened without them. And I would also like to expressly thank [Modox](https://github.com/mdx7) for all his work on reverse engineering the classic clients and all the help he has personally given me.

## Download HermesProxy
Stable Downloads: [Releases](https://github.com/WowLegacyCore/HermesProxy/releases)
