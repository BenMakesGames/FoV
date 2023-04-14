namespace BenMakesGames.FoV.Algorithms;

/// <summary>
/// Adapted from http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html#diamondcode
/// </summary>
public sealed class DiamondWallsFoV: IFoVAlgorithm
{
    /// <summary>
    /// Computes field of view (FoV) using the diamond wall algorithm.
    ///
    /// Compared to other algorithms, diamond wall:
    /// * Is pretty fast (low CPU usage; shadow cast is a little faster)
    /// * Reveals many tiles (permissive reveals more, but is very slow)
    ///
    /// If speed is important, but you want to reveal more tiles than the shadow cast algorithm, diamond walls is a
    /// good pick.
    /// </summary>
    public static HashSet<(int X, int Y)> Compute(IFoVMap map, (int X, int Y) origin, int radius)
    {
        var visible = new HashSet<(int X, int Y)>();
        visible.Add(origin);

        for(var i = 0; i < 8; i++)
            ComputeOctant(map, visible, i, origin, radius, 1, new Slope(1, 1), new Slope(0, 1));

        return visible;
    }

    private static void ComputeOctant(
        IFoVMap map, HashSet<(int X, int Y)> visible, int octant, (int X, int Y) origin,
        int radius, int x, Slope top, Slope bottom
    )
    {
        for(; x <= radius; x++)
        {
            var topY = ComputeTopY(map, top, octant, origin, x);
            var bottomY = bottom.Y == 0 ? 0 : ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);
            bool? wasOpaque = null;

            for(var y = topY; y >= bottomY; y--)
            {
                var (tx, ty) = FoVHelpers.TranslateLocalToMap(x, y, origin, octant);

                var inRange = radius < 0 || GetDistance(x, y) <= radius;

                if(inRange)
                    visible.Add((tx, ty));

                var isOpaque = !inRange || map.BlocksLight(tx, ty);

                if(
                    isOpaque && (
                        (y == topY && top.LessOrEqual(y * 2 - 1, x * 2) && !BlocksLight(map, x, y - 1, octant, origin)) ||
                        (y == bottomY && bottom.GreaterOrEqual(y * 2 + 1, x * 2) && !BlocksLight(map, x, y + 1, octant, origin))
                    )
                )
                {
                    isOpaque = false;
                }

                if(x == radius) continue;

                if(isOpaque)
                {
                    if(wasOpaque == false)
                    {
                        if(!inRange || y == bottomY)
                        {
                            bottom = new Slope(y * 2 + 1, x * 2);
                            break;
                        }

                        ComputeOctant(map, visible, octant, origin, radius, x + 1, top, new Slope(y * 2 + 1, x * 2));
                    }

                    wasOpaque = true;
                }
                else
                {
                    if(wasOpaque == true)
                        top = new Slope(y * 2 + 1, x * 2);

                    wasOpaque = false;
                }
            }

            if(wasOpaque == true)
                break;
        }
    }

    private static int ComputeTopY(IFoVMap map, Slope top, int octant, (int X, int Y) origin, int x)
    {
        if(top.X == 1)
            return x;

        var topY = ((x * 2 - 1) * top.Y + top.X) / (top.X * 2);
        var ay = (topY * 2 + 1) * top.X;

        if(BlocksLight(map, x, topY, octant, origin))
        {
            if(top.GreaterOrEqual(ay, x * 2))
                topY++;
        }
        else
        {
            if(top.Greater(ay, x * 2 + 1))
                topY++;
        }

        return topY;
    }

    private static bool BlocksLight(IFoVMap map, int x, int y, int octant, (int X, int Y) origin)
    {
        var (nx, ny) = FoVHelpers.TranslateLocalToMap(x, y, origin, octant);

        return map.BlocksLight(nx, ny);
    }

    private static double GetDistance(int x, int y) => Math.Sqrt(x * x + y * y);
}