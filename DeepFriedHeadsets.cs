using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using System.Reflection;

namespace DeepFriedHeadsets;

/// <summary>
/// Mod metadata for Deep Fried Headsets - replaces the former package.json
/// </summary>
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.shwng.deepfriedheadsets";
    public override string Name { get; init; } = "Deep Fried Headsets";
    public override string Author { get; init; } = "shwng";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("4.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

/// <summary>
/// Configuration class matching the config.json structure
/// All values are multipliers unless noted with "Add" (additive) or "Set" (direct set)
/// </summary>
public class HeadsetConfig
{
    // Volume controls (multipliers)
    public double AmbientVolume { get; set; } = 1.5;
    public double HeadphonesMixerVolume { get; set; } = 2.0;
    public double DryVolume { get; set; } = 1.0;
    public double EffectsReturnsGroupVolumeSet { get; set; } = 0;

    // Compressor settings (multipliers unless noted)
    public double CompressorGain { get; set; } = 1.5;
    public double CompressorThreshold { get; set; } = 1.2;
    public double CompressorAttack { get; set; } = 1.0;
    public double CompressorRelease { get; set; } = 1.0;

    // Compressor send levels
    public double AmbientCompressorSendLevelAdd { get; set; } = -5;
    public double ClientPlayerCompressorSendLevelSet { get; set; } = 6;
    public double PlayerCompressorSendLevel { get; set; } = 12;
    public double GunsCompressorSendLevelAdd { get; set; } = 12;
    public double EffectsReturnsCompressorSendLevelSet { get; set; } = 0;

    // EQ settings (multipliers)
    public double EQBandGain { get; set; } = 1.75;
    public double EQBandFrequency { get; set; } = 1.0;
    public double EQBandQ { get; set; } = 1.0;

    // Filter settings
    public double HighpassFreq { get; set; } = 1.0;
    public double HighpassResonance { get; set; } = 1.0;
    public double LowpassFreq { get; set; } = 1.0;

    // Distortion
    public double DistortionMultiplier { get; set; } = 0.75;

    // Spatial
    public double RolloffMultiplier { get; set; } = 1.00015;
}

/// <summary>
/// Main mod class that modifies headset audio properties
/// Load after PostDBModLoader so database is ready
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class DeepFriedHeadsetsMod(
    ISptLogger<DeepFriedHeadsetsMod> logger,
    DatabaseService databaseService,
    ModHelper modHelper) : IOnLoad
{
    public Task OnLoad()
    {
        // Load config
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var cfg = modHelper.GetJsonDataFromFile<HeadsetConfig>(pathToMod, "config/config.json");

        // Get items database
        var items = databaseService.GetItems();

        int modifiedCount = 0;

        foreach (var (id, item) in items)
        {
            if (item?.Properties == null) continue;
            var p = item.Properties;

            // Check if this is a headset item (has headset-specific properties)
            if (p.AmbientVolume.HasValue &&
                p.HeadphonesMixerVolume.HasValue &&
                p.CompressorGain.HasValue &&
                p.CompressorThreshold.HasValue &&
                p.DryVolume.HasValue)
            {
                // === VOLUME CONTROLS ===
                p.AmbientVolume = ApplyMultiplier(p.AmbientVolume.Value, cfg.AmbientVolume, -50, 50);
                p.HeadphonesMixerVolume = ApplyMultiplier(p.HeadphonesMixerVolume.Value, cfg.HeadphonesMixerVolume, -20, 10);
                p.DryVolume = ApplyMultiplier(p.DryVolume.Value, cfg.DryVolume, -60, 0);

                if (p.EffectsReturnsGroupVolume.HasValue)
                {
                    p.EffectsReturnsGroupVolume = cfg.EffectsReturnsGroupVolumeSet;
                }

                // === COMPRESSOR SETTINGS ===
                p.CompressorGain = ApplyMultiplier(p.CompressorGain.Value, cfg.CompressorGain, 0, 30);
                p.CompressorThreshold = ApplyMultiplier(p.CompressorThreshold.Value, cfg.CompressorThreshold, -80, -5);

                if (p.CompressorAttack.HasValue)
                {
                    p.CompressorAttack = ApplyMultiplier(p.CompressorAttack.Value, cfg.CompressorAttack, 1, 200);
                }
                if (p.CompressorRelease.HasValue)
                {
                    p.CompressorRelease = ApplyMultiplier(p.CompressorRelease.Value, cfg.CompressorRelease, 10, 1000);
                }

                // === COMPRESSOR SEND LEVELS ===
                if (p.ClientPlayerCompressorSendLevel.HasValue)
                {
                    p.ClientPlayerCompressorSendLevel = cfg.ClientPlayerCompressorSendLevelSet;
                }
                if (p.AmbientCompressorSendLevel.HasValue)
                {
                    p.AmbientCompressorSendLevel = p.AmbientCompressorSendLevel.Value + cfg.AmbientCompressorSendLevelAdd;
                }
                if (p.GunsCompressorSendLevel.HasValue)
                {
                    p.GunsCompressorSendLevel = p.GunsCompressorSendLevel.Value + cfg.GunsCompressorSendLevelAdd;
                }
                if (p.EffectsReturnsCompressorSendLevel.HasValue)
                {
                    p.EffectsReturnsCompressorSendLevel = cfg.EffectsReturnsCompressorSendLevelSet;
                }

                // NPC/Observed Player compressor send levels
                if (p.NpcCompressorSendLevel.HasValue)
                {
                    p.NpcCompressorSendLevel = p.NpcCompressorSendLevel.Value == 0
                        ? cfg.PlayerCompressorSendLevel
                        : p.NpcCompressorSendLevel.Value + cfg.PlayerCompressorSendLevel;
                }
                if (p.ObservedPlayerCompressorSendLevel.HasValue)
                {
                    p.ObservedPlayerCompressorSendLevel = p.ObservedPlayerCompressorSendLevel.Value == 0
                        ? cfg.PlayerCompressorSendLevel
                        : p.ObservedPlayerCompressorSendLevel.Value + cfg.PlayerCompressorSendLevel;
                }

                // === EQ SETTINGS ===
                if (p.EQBand1Gain.HasValue)
                    p.EQBand1Gain = ApplyMultiplier(p.EQBand1Gain.Value, cfg.EQBandGain, -10, 10);
                if (p.EQBand2Gain.HasValue)
                    p.EQBand2Gain = ApplyMultiplier(p.EQBand2Gain.Value, cfg.EQBandGain, -10, 10);
                if (p.EQBand3Gain.HasValue)
                    p.EQBand3Gain = ApplyMultiplier(p.EQBand3Gain.Value, cfg.EQBandGain, -10, 10);

                if (p.EQBand1Frequency.HasValue)
                    p.EQBand1Frequency = ApplyMultiplier(p.EQBand1Frequency.Value, cfg.EQBandFrequency, 20, 500);
                if (p.EQBand2Frequency.HasValue)
                    p.EQBand2Frequency = ApplyMultiplier(p.EQBand2Frequency.Value, cfg.EQBandFrequency, 200, 5000);
                if (p.EQBand3Frequency.HasValue)
                    p.EQBand3Frequency = ApplyMultiplier(p.EQBand3Frequency.Value, cfg.EQBandFrequency, 2000, 20000);

                if (p.EQBand1Q.HasValue)
                    p.EQBand1Q = ApplyMultiplier(p.EQBand1Q.Value, cfg.EQBandQ, 0.1, 10);
                if (p.EQBand2Q.HasValue)
                    p.EQBand2Q = ApplyMultiplier(p.EQBand2Q.Value, cfg.EQBandQ, 0.1, 10);
                if (p.EQBand3Q.HasValue)
                    p.EQBand3Q = ApplyMultiplier(p.EQBand3Q.Value, cfg.EQBandQ, 0.1, 10);

                // === FILTER SETTINGS ===
                if (p.HighpassFreq.HasValue)
                    p.HighpassFreq = ApplyMultiplier(p.HighpassFreq.Value, cfg.HighpassFreq, 20, 2000);
                if (p.HighpassResonance.HasValue)
                    p.HighpassResonance = ApplyMultiplier(p.HighpassResonance.Value, cfg.HighpassResonance, 0.5, 10);
                if (p.LowpassFreq.HasValue)
                    p.LowpassFreq = ApplyMultiplier(p.LowpassFreq.Value, cfg.LowpassFreq, 1000, 22000);

                // === DISTORTION ===
                if (p.Distortion.HasValue)
                {
                    p.Distortion = Math.Min(p.Distortion.Value * cfg.DistortionMultiplier, 1.0);
                }

                // === SPATIAL ===
                if (p.RolloffMultiplier.HasValue)
                {
                    p.RolloffMultiplier = Math.Min(Math.Max(cfg.RolloffMultiplier, p.RolloffMultiplier.Value), 1.35);
                }

                modifiedCount++;
            }
        }

        logger.Success($"Deep Fried Headsets: Modified {modifiedCount} headset items!");

        return Task.CompletedTask;
    }

    private static double ApplyMultiplier(double val, double multiplier, double clampMin, double clampMax)
    {
        double result = val * multiplier;
        return Math.Clamp(result, clampMin, clampMax);
    }
}
