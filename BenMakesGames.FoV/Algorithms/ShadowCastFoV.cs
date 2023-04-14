namespace BenMakesGames.FoV.Algorithms;

/// <summary>
/// Adapted from http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html#shadowcode
/// </summary>
public sealed class ShadowCastFoV : IFoVAlgorithm
{
    /// <summary>
    /// Computes field of view (FoV) using the shadow casting algorithm.
    ///
    /// Compared to other algorithms, shadow casting:
    /// * Is very fast (low CPU usage), especially for large, open maps and players with large sight radiuses
    /// * Reveals fewer tiles
    ///
    /// If speed is important, or you just want a more clausterphobic feeling, shadow casting is a good pick.
    /// </summary>
    public static HashSet<(int X, int Y)> Compute(IFoVMap map, (int X, int Y) origin, int radius)
    {
        var visible = new HashSet<(int X, int Y)>();
        visible.Add(origin);

        for (var i = 0; i < 8; i++)
            ComputeOctant(map, visible, i, origin, radius, 1, new Slope(1, 1), new Slope(0, 1));

        return visible;
    }

    private static void ComputeOctant(
        IFoVMap map, HashSet<(int X, int Y)> visible, int octant, (int X, int Y) origin,
        int radius, int x, Slope top, Slope bottom
    )
    {
        for (; x <= radius; x++)
        {
            var topY = top.X == 1 ? x : ((x * 2 + 1) * top.Y + top.X - 1) / (top.X * 2);
            var bottomY = bottom.Y == 0 ? 0 : ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);

            bool? wasOpaque = null;

            for (var y = topY; y >= bottomY; y--)
            {
                var (tx, ty) = FoVHelpers.TranslateLocalToMap(x, y, origin, octant);

                var inRange = radius < 0 || GetDistance(x, y) <= radius;

                if (inRange)
                    visible.Add((tx, ty));

                var isOpaque = !inRange || map.BlocksLight(tx, ty);

                if (x == radius) continue;

                if (isOpaque)
                {
                    if (wasOpaque == false)
                    {
                        var newBottom = new Slope(y * 2 + 1, x * 2 - 1);

                        if (!inRange || y == bottomY)
                        {
                            bottom = newBottom;
                            break;
                        }

                        ComputeOctant(map, visible, octant, origin, radius, x + 1, top, newBottom);
                    }

                    wasOpaque = true;
                }
                else
                {
                    if (wasOpaque == true)
                        top = new Slope(y * 2 + 1, x * 2 + 1);

                    wasOpaque = false;
                }
            }

            if (wasOpaque != false)
                break;
        }
    }

    private static double GetDistance(int x, int y) => Math.Sqrt(x * x + y * y);
}