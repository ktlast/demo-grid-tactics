public class Tile
{
    public int Row { get; }
    public int Col { get; }
    public TerrainType Terrain { get; }

    public Tile(int row, int col, TerrainType terrain)
    {
        Row = row;
        Col = col;
        Terrain = terrain;
    }

    public bool IsWalkable()
    {
        return true;
    }

    public int MovementCost()
    {
        if (Terrain == TerrainType.Mountain)
        {
            return 2;
        }

        return 1;
    }
}