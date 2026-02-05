# Deep Fried Headsets - SPT 4.0 Version

A server mod that enhances headset audio properties in SPT (Single Player Tarkov).

## Original Author
- shwng

## Requirements
- SPT 4.0.11 or compatible version
- .NET 9.0 SDK (for building from source)

## Installation (Pre-built)
1. Copy the `DeepFriedHeadsets.dll` file to your `SPT/user/mods/` folder
2. Copy the `config` folder next to the DLL
3. Start the SPT server

## Building from Source
1. Install the [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Open a terminal in the mod folder
3. Run `dotnet build -c Release`
4. The compiled mod will be in `bin/Release/DeepFriedHeadsets/DeepFriedHeadsets/`

## Configuration
Edit `config/config.json` to adjust headset audio settings:

- **AmbientVolume**: Multiplier for ambient noise reduction (default: 1.5)
- **AmbientCompressorSendLevelAdd**: Value added to ambient compressor send level (default: -5)
- **DistortionMultiplier**: Multiplier for audio distortion (default: 0.75)
- **HeadphonesMixerVolume**: Total volume multiplier (default: 2.0)
- **CompressorGain**: Audio compression gain multiplier (default: 1.5)
- **CompressorThreshold**: Compression threshold multiplier (default: 1.2)
- **EQBandGain**: EQ band gain multiplier for all bands (default: 1.75)
- **HighpassFreq**: Highpass filter frequency multiplier (default: 1)
- **DryVolume**: Dry (unprocessed) volume multiplier (default: 1)
- **RolloffMultiplier**: Distance rolloff multiplier (default: 1.00015)
- **PlayerCompressorSendLevel**: Human sounds compressor level (default: 12)
- **EnvCompressorSendLevel**: Environmental sounds compressor level (default: 0)
- **EffectsReturnsCompressorSendLevel**: Effects returns compressor level (default: 0)
- **EffectsReturnsGroupVolume**: Effects returns group volume (default: 0)
- **GunsCompressorSendLevelAdd**: Gun sounds compressor level add (default: 12)

## Changes from 3.x Version
- Rewritten from TypeScript to C# for SPT 4.0 compatibility
- Uses the new SPTarkov.Server.Core NuGet packages
- Config format changed from JSONC to JSON (comments not supported in JSON)

## License
MIT
