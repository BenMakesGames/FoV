# What Is It?

`BenMakesGames.FoV` is a collection of field-of-view algorithms designed for square tile grids. It features the following algorithms:

* Diamond-wall
* Milazzo's Beveled-wall
* Raycasting
* Shadowcasting

Field-of-view/line-of-sight is useful for roguelikes, and other tactical, tile-based games where vision plays an important role.

[![Buy Me a Coffee at ko-fi.com](https://raw.githubusercontent.com/BenMakesGames/AssetsForNuGet/main/buymeacoffee.png)](https://ko-fi.com/A0A12KQ16)

# How to Use

## Install

```powershell
dotnet add package BenMakesGames.FoV 
```

## Create a Map

Your map must implement `IFoVMap`, which requires a `Width` and `Height` property, and a method that returns whether or not a given tile is opaque:

For example:

```c#
public sealed class MyMap: IFoVMap
{
    public int Width { get; }
    public int Height { get; }

    // store your tiles however you want; here's one possibility:
    public MyTile[] Tiles { get; }

    // BlocksLight is required by IFoVMap; here's one possible implementation:
    bool BlocksLight(int x, int y)
    {
        if(x < 0 || x >= Width || y < 0 || y >= Height)
            return true;

        return Tiles[x + y * Width].IsOpaque;
    }

    ...
}
```

Another common implementation is to use a `Dictionary<(int X, int Y), MyTile>` collection to store the map.

## Call One of the FoV Algorithms

All of the algorithms have the same signature:

```c#
HashSet<(int X, int Y)> Compute(IFoVMap map, (int X, int Y) origin, int radius)
```

They take a map, origin point, and sight radius, and returns a set of points that are visible from the origin.

Basic usage:

```c#
var visibleTiles = DiamondWallsFoV.Compute(Map, (Player.X, Player.Y), Player.SightRadius);

for(int y = 0; y < Map.Height; y++)
{
    for(int x = 0; x < Map.Width; x++)
    {
        if(visibleTiles.Contains((x, y)))
        {
            // tile is visible; draw it!
        }
        else
        {
            // don't draw the tile, or draw it as a fog of war tile
        }
    }
}
```

When implementing field-of-view in your game, you should only compute a new field of view when the player moves, or the map changes (such as a door opening or closing).

### Available Algorithms, and Their Features

* `DiamondWallsFoV`
  * Relatively fast algorithm. Compared to other algorithms, reveals more tiles in a given sight radius.
* `MilazzoFoV`
  * Medium speed; designed to have intuitive lines of sight, especially for maps which contain many single-tile walls/pillars.
* `RayCastFoV`
  * Slowest algorithm, with occasionally unintuitive lines of sight. Not generally recommended; included for historical reasons.
* `ShadowCastFoV`
  * The fastest algorithm of the bunch, especially when used in large, open spaces with large sight radii.
