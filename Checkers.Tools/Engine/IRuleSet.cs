using Checkers.Tools.Models;

namespace Checkers.Tools.Engine;

/// <summary>Variant-specific rule parameters used by <see cref="CheckersGame"/>.</summary>
public interface IRuleSet
{
    Variant Variant { get; }
    int BoardSize { get; }
    int RowsPerSide { get; }
    int DrawPlyLimit { get; }
    bool MenCaptureBackwards { get; }
    bool FlyingKings { get; }
    bool MustTakeLongestCapture { get; }
    bool CaptureEndsOnCrowning { get; }
}

public static class RuleSets
{
    public static IRuleSet For(Variant variant) => variant == Variant.American ? American : European;

    public static readonly IRuleSet American = new Rules(Variant.American, 8, 3, 80, false, false, false, true);

    public static readonly IRuleSet European = new Rules(Variant.European, 10, 4, 50, true, true, true, false);

    private sealed record Rules(
        Variant Variant,
        int BoardSize,
        int RowsPerSide,
        int DrawPlyLimit,
        bool MenCaptureBackwards,
        bool FlyingKings,
        bool MustTakeLongestCapture,
        bool CaptureEndsOnCrowning) : IRuleSet;
}
