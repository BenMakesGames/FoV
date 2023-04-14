using System.Runtime.CompilerServices;

namespace BenMakesGames.FoV.Algorithms;

public static class FoVHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int X, int Y) TranslateLocalToMap(int x, int y, (int X, int Y) origin, int octant) => octant switch
    {
        0 => (origin.X + x, origin.Y - y),
        1 => (origin.X + y, origin.Y - x),
        2 => (origin.X - y, origin.Y - x),
        3 => (origin.X - x, origin.Y - y),
        4 => (origin.X - x, origin.Y + y),
        5 => (origin.X - y, origin.Y + x),
        6 => (origin.X + y, origin.Y + x),
        7 => (origin.X + x, origin.Y + y),
        _ => throw new ArgumentOutOfRangeException(nameof(octant), octant, "Octant must be between 0 and 7 inclusive.")
    };
}