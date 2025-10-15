using Spectre.Console;

namespace Mechanic.CLI;

public class SpinnerUtils
{
    private static readonly Random Random = new();
    
    private static readonly Spinner[] KnownSpinners = 
    [
        Spinner.Known.Default,
        Spinner.Known.Ascii,
        Spinner.Known.Dots,
        Spinner.Known.Dots2,
        Spinner.Known.Dots3,
        Spinner.Known.Dots4,
        Spinner.Known.Dots5,
        Spinner.Known.Dots6,
        Spinner.Known.Dots7,
        Spinner.Known.Dots8,
        Spinner.Known.Dots9,
        Spinner.Known.Dots10,
        Spinner.Known.Dots11,
        Spinner.Known.Dots12,
        Spinner.Known.Dots8Bit,
        Spinner.Known.Line,
        Spinner.Known.Line2,
        Spinner.Known.Pipe,
        Spinner.Known.SimpleDots,
        Spinner.Known.SimpleDotsScrolling,
        Spinner.Known.Star,
        Spinner.Known.Star2,
        Spinner.Known.Flip,
        Spinner.Known.Hamburger,
        Spinner.Known.GrowVertical,
        Spinner.Known.GrowHorizontal,
        Spinner.Known.Balloon,
        Spinner.Known.Balloon2,
        Spinner.Known.Noise,
        Spinner.Known.Bounce,
        Spinner.Known.BoxBounce,
        Spinner.Known.BoxBounce2,
        Spinner.Known.Triangle,
        Spinner.Known.Arc,
        Spinner.Known.Circle,
        Spinner.Known.SquareCorners,
        Spinner.Known.CircleQuarters,
        Spinner.Known.CircleHalves,
        Spinner.Known.Squish,
        Spinner.Known.Toggle,
        Spinner.Known.Toggle2,
        Spinner.Known.Toggle3,
        Spinner.Known.Toggle4,
        Spinner.Known.Toggle5,
        Spinner.Known.Toggle6,
        Spinner.Known.Toggle7,
        Spinner.Known.Toggle8,
        Spinner.Known.Toggle9,
        Spinner.Known.Toggle10,
        Spinner.Known.Toggle11,
        Spinner.Known.Toggle12,
        Spinner.Known.Toggle13,
        Spinner.Known.Arrow,
        Spinner.Known.Arrow2,
        Spinner.Known.Arrow3,
        Spinner.Known.BouncingBar,
        Spinner.Known.BouncingBall,
        Spinner.Known.Smiley,
        Spinner.Known.Monkey,
        Spinner.Known.Hearts,
        Spinner.Known.Clock,
        Spinner.Known.Earth,
        Spinner.Known.Material,
        Spinner.Known.Moon,
        Spinner.Known.Runner,
        Spinner.Known.Pong,
        Spinner.Known.Shark,
        Spinner.Known.Dqpb,
        Spinner.Known.Weather,
        Spinner.Known.Christmas,
        Spinner.Known.Grenade,
        Spinner.Known.Point,
        Spinner.Known.Layer,
        Spinner.Known.BetaWave,
        Spinner.Known.Aesthetic
    ];
    
    public static Spinner GetRandomSpinner()
    {
        return KnownSpinners[Random.Next(KnownSpinners.Length)];
    }
}