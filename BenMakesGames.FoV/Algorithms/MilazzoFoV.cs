namespace BenMakesGames.FoV.Algorithms;

/// <summary>
/// Adapted from http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html#mycode
/// </summary>
public sealed class MilazzoFoV: IFoVAlgorithm
{
    /// <summary>
    /// Computes field of view (FoV) using Adam Milazzo's beveled-corners algorithm.
    ///
    /// Compared to other algorithms, Milazzo's:
    /// * Is slower than shadow casting and diamond walls, but much faster than permissive FoV
    /// * Is designed to reveal tiles in a way that feels natural, especially for maps containing one-tile obstacles
    ///
    /// If your maps will have lots of single-tile obstacles, and speed is not a top-priority for you, this algorithm
    /// is a good pick.
    /// </summary>
    public static HashSet<(int X, int Y)> Compute(IFoVMap map, (int X, int Y) origin, int radius)
    {
        var visible = new HashSet<(int X, int Y)>();
        visible.Add(origin);

        for(var i = 0; i < 8; i++)
            ComputeOctant(map, visible, i, origin, radius, 1, new Slope(1, 1), new Slope(0, 1));

        return visible;
    }

    private static void ComputeOctant(IFoVMap map, HashSet<(int X, int Y)> visible, int octant, (int X, int Y) origin, int radius, int x, Slope top, Slope bottom)
    {
        for(; x <= radius; x++)
        {
            var topY = ComputeTopY(map, octant, origin, x, top);
            var bottomY = ComputeBottomY(map, octant, origin, x, bottom);

            bool? wasOpaque = null;

            for(var y = topY; y >= bottomY; y--)
            {
                if(radius >= 0 && GetDistance(x, y) > radius) continue;

                var isOpaque = BlocksLight(map, x, y, octant, origin);
                var isVisible =
                    isOpaque || (
                        (y != topY || top.Greater(y * 4 - 1, x * 4 + 1)) &&
                        (y != bottomY || bottom.Less(y * 4 + 1, x * 4 - 1))
                    )
                ;

                if(isVisible)
                    SetVisible(visible, x, y, octant, origin);

                if(x == radius) continue;

                if(isOpaque)
                {
                    if(wasOpaque == false)
                    {
                        var nx = x * 2;
                        var ny = y * 2 + 1;

                        if(BlocksLight(map, x, y + 1, octant, origin))
                            nx--;

                        if(top.Greater(ny, nx))
                        {
                            if(y == bottomY)
                            {
                                bottom = new Slope(ny, nx);
                                break;
                            }

                            ComputeOctant(map, visible, octant, origin, radius, x + 1, top, new Slope(ny, nx));
                        }
                        else
                        {
                            if(y == bottomY)
                                return;
                        }
                    }

                    wasOpaque = true;
                }
                else
                {
                    if(wasOpaque == true)
                    {
                        var nx = x * 2;
                        var ny = y * 2 + 1;

                        if(BlocksLight(map, x + 1, y + 1, octant, origin))
                            nx++;

                        if(bottom.GreaterOrEqual(ny, nx))
                            return;

                        top = new Slope(ny, nx);
                    }

                    wasOpaque = false;
                }
            }

            if(wasOpaque != false)
                break;
        }
    }

    private static int ComputeBottomY(IFoVMap map, int octant, (int X, int Y) origin, int x, Slope bottom)
    {
        int bottomY;

        if (bottom.Y == 0)
            bottomY = 0;
        else
        {
            bottomY = ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);

            if (
                bottom.GreaterOrEqual(bottomY * 2 + 1, x * 2) &&
                BlocksLight(map, x, bottomY, octant, origin) &&
                !BlocksLight(map, x, bottomY + 1, octant, origin)
            )
            {
                bottomY++;
            }
        }

        return bottomY;
    }

    private static int ComputeTopY(IFoVMap map, int octant, (int X, int Y) origin, int x, Slope top)
    {
        int topY;

        if (top.X == 1)
            topY = x;
        else
        {
            topY = ((x * 2 - 1) * top.Y + top.X) / (top.X * 2);

            if (BlocksLight(map, x, topY, octant, origin))
            {
                if (top.GreaterOrEqual(topY * 2 + 1, x * 2) && !BlocksLight(map, x, topY + 1, octant, origin))
                    topY++;
            }
            else
            {
                var ax = x * 2;

                if (BlocksLight(map, x + 1, topY + 1, octant, origin))
                    ax++;

                if (top.Greater(topY * 2 + 1, ax))
                    topY++;
            }
        }

        return topY;
    }

    private static bool BlocksLight(IFoVMap map, int x, int y, int octant, (int X, int Y) origin)
    {
        var (nx, ny) = FoVHelpers.TranslateLocalToMap(x, y, origin, octant);

        return map.BlocksLight(nx, ny);
    }

    private static void SetVisible(HashSet<(int X, int Y)> visible, int x, int y, int octant, (int X, int Y) origin)
    {
        var (nx, ny) = FoVHelpers.TranslateLocalToMap(x, y, origin, octant);

        visible.Add((nx, ny));
    }

    private static double GetDistance(int x, int y) => Math.Sqrt(x * x + y * y);
}