using System.Drawing;

namespace BenMakesGames.FoV.Algorithms;

/// <summary>
/// Adapted from http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html#raycode
/// </summary>
public sealed class RayCastFoV : IFoVAlgorithm
{
    /// <summary>
    /// Computes field of view (FoV) using the ray casting algorithm.
    ///
    /// Compared to other algorithms, ray casting:
    /// * Is on the slower side, but still much faster than the permissive FoV algorithm
    /// * Reveals a mediumish number of tiles compared to other algorithms, but has gaps that may feel "wrong"
    /// * Milazzo's algorithm is about the same speed, but produces more natural fields of view
    /// </summary>
    public static HashSet<(int X, int Y)> Compute(IFoVMap map, (int X, int Y) origin, int radius)
    {
        var visible = new HashSet<(int X, int Y)>();
        visible.Add(origin);

        if (radius == 0) return visible;

        var area = new Rectangle(0, 0, map.Width, map.Height);

        if(radius >= 0)
            area.Intersect(new Rectangle(origin.X - radius, origin.Y - radius, radius * 2 + 1, radius * 2 + 1));

        for(var x = area.Left; x < area.Right; x++)
        {
            TraceLine(map, visible, origin, x, area.Top, radius);
            TraceLine(map, visible, origin, x, area.Bottom - 1, radius);
        }

        for(var y = area.Top + 1; y<area.Bottom - 1; y++)
        {
            TraceLine(map, visible, origin, area.Left, y, radius);
            TraceLine(map, visible, origin, area.Right - 1, y, radius);
        }

        return visible;
    }

    private static void TraceLine(IFoVMap map, HashSet<(int X, int Y)> visible, (int X, int Y) origin, int x2, int y2, int rangeLimit)
    {
        var xDiff = x2 - origin.X;
        var yDiff = y2 - origin.Y;
        var xLen = Math.Abs(xDiff);
        var yLen = Math.Abs(yDiff);
        var xInc = Math.Sign(xDiff);
        var yInc = Math.Sign(yDiff) << 16;
        var index = (origin.Y<<16) + origin.X;

        if(xLen < yLen)
        {
            (xLen, yLen) = (yLen, xLen);
            (xInc, yInc) = (yInc, xInc);
        }

        int errorInc = yLen*2, error = -xLen, errorReset = xLen*2;

        while(--xLen >= 0)
        {
            index += xInc;
            error += errorInc;

            if (error > 0)
            {
                error -= errorReset;
                index += yInc;
            }

            var x = index & 0xFFFF;
            var y = index >> 16;

            if(rangeLimit >= 0 && GetDistance(origin, x, y) > rangeLimit)
                break;

            visible.Add((x, y));

            if(map.BlocksLight(x, y))
                break;
        }
    }

    private static double GetDistance((int X, int Y) origin, int x, int y)
    {
        var dx = origin.X - x;
        var dy = origin.Y - y;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}