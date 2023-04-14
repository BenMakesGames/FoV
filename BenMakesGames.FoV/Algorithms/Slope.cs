namespace BenMakesGames.FoV.Algorithms;

internal sealed record Slope(int Y, int X)
{
    public bool Greater(int y, int x) => Y * x > X * y;
    public bool GreaterOrEqual(int y, int x) => Y * x >= X * y;
    public bool Less(int y, int x) => Y * x < X * y;
    public bool LessOrEqual(int y, int x) => Y * x <= X * y;
}
