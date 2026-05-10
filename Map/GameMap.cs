using SplashKitSDK;

public class GameMap
{
    private readonly Tile[,] _tiles;

    public int Rows { get; }
    public int Cols { get; }
    public int TileSize { get; }
    public int OffsetX { get; }
    public int OffsetY { get; }

    public GameMap(int rows, int cols, int tileSize, int offsetX, int offsetY)
    {
        Rows = rows;
        Cols = cols;
        TileSize = tileSize;
        OffsetX = offsetX;
        OffsetY = offsetY;

        _tiles = new Tile[Rows, Cols];
        LoadSampleMap();
    }

    private void LoadSampleMap()
    {
        Random random = new Random(123);

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                TerrainType terrain = GenerateRandomTerrain(random);
                _tiles[row, col] = new Tile(row, col, terrain);
            }
        }
    }
    private TerrainType GenerateRandomTerrain(Random random)
    {
        int roll = random.Next(100);

        if (roll < 65)
        {
            return TerrainType.Plain;
        }

        if (roll < 78)
        {
            return TerrainType.Forest;
        }

        if (roll < 90)
        {
            return TerrainType.Hill;
        }

        return TerrainType.Mountain;
    }

    public bool IsInside(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Cols;
    }

    public Tile GetTile(int row, int col)
    {
        return _tiles[row, col];
    }
    public bool IsWalkable(int row, int col)
    {
        return IsInside(row, col) && GetTile(row, col).IsWalkable();
    }
    public int MovementCost(int row, int col)
    {
        return GetTile(row, col).MovementCost();
    }

    public bool TryGetTileFromMouse(out Tile? tile)
    {
        int mouseX = (int)SplashKit.MouseX();
        int mouseY = (int)SplashKit.MouseY();

        int col = (mouseX - OffsetX) / TileSize;
        int row = (mouseY - OffsetY) / TileSize;

        if (!IsInside(row, col))
        {
            tile = null;
            return false;
        }

        tile = GetTile(row, col);
        return true;
    }
    public void Draw()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                DrawTile(_tiles[row, col]);
            }
        }
    }

    private void DrawTile(Tile tile)
    {
        int x = OffsetX + tile.Col * TileSize;
        int y = OffsetY + tile.Row * TileSize;

        SplashKit.FillRectangle(GetTerrainColor(tile.Terrain), x, y, TileSize, TileSize);
        SplashKit.DrawRectangle(Color.Black, x, y, TileSize, TileSize);
    }

    private Color GetTerrainColor(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Mountain => Color.DarkGray,
            TerrainType.Forest => Color.ForestGreen,
            TerrainType.Hill => Color.SandyBrown,
            _ => Color.LightGray
        };
    }
}
