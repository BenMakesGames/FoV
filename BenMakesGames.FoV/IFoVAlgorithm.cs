namespace BenMakesGames.FoV;

public interface IFoVAlgorithm
{
    static abstract HashSet<(int X, int Y)> Compute(IFoVMap map, (int X, int Y) origin, int radius);
}