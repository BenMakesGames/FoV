namespace BenMakesGames.FoV;

public interface IFoVMap
{
    int Width { get; }
    int Height { get; }
    bool BlocksLight(int x, int y);
}