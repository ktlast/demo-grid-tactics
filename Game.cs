using System;
using System.Collections.Generic;
using SplashKitSDK;

public class Game
{
    private readonly GameMap _map;
    private readonly List<Unit> _units;
    private readonly CombatService _combatService;
    private readonly EnemyAI _enemyAI;

    private Unit? _selectedUnit;
    private GameState _state;
    private int _turnNumber;

    private string _message = "Player Turn 1: click a player unit";

    public Game()
    {
        _map = new GameMap(8, 8, 64, 80, 80);
        _units = new List<Unit>();
        _combatService = new CombatService(_map);
        _enemyAI = new EnemyAI(_map);

        _state = GameState.PlayerTurn;
        _turnNumber = 1;

        CreateUnits();
        StartPlayerTurn();
    }

    private void CreateUnits()
    {
        _units.Add(new PlayerUnit("Warrior", "Tank", 1, 1, 24, 3, 6, 1));
        _units.Add(new PlayerUnit("Archer", "Ranged Damage", 0, 1, 14, 3, 5, 3));

        _units.Add(new EnemyUnit("Goblin A", "Goblin", 4, 6, 14, 4, 4, 1));
        _units.Add(new EnemyUnit("Goblin B", "Goblin", 1, 6, 14, 4, 4, 1));
        _units.Add(new EnemyUnit("Orc", "Orc", 7, 7, 30, 2, 6, 1));
        _units.Add(new EnemyUnit("Shaman", "Shaman", 2, 5, 15, 2, 3, 2));
    }

    public void HandleInput()
    {
        SplashKit.ProcessEvents();

        if (_state == GameState.Win || _state == GameState.Lose)
        {
            return;
        }

        if (_state == GameState.EnemyTurn)
        {
            RunEnemyTurn();
            return;
        }

        if (SplashKit.MouseClicked(MouseButton.LeftButton))
        {
            HandleLeftClick();
        }

        if (SplashKit.KeyTyped(KeyCode.SpaceKey))
        {
            EndPlayerTurn();
        }

        if (SplashKit.KeyTyped(KeyCode.EscapeKey))
        {
            _selectedUnit = null;
            _message = "Selection cleared";
        }
    }

    private void HandleLeftClick()
    {
        if (_state != GameState.PlayerTurn)
        {
            return;
        }

        if (!_map.TryGetTileFromMouse(out Tile? tile) || tile == null)
        {
            _message = "Outside grid";
            return;
        }

        Unit? clickedUnit = GetUnitAt(tile.Row, tile.Col);

        if (clickedUnit != null)
        {
            HandleUnitClick(clickedUnit);
            return;
        }

        HandleEmptyTileClick(tile);
    }

    private void HandleUnitClick(Unit clickedUnit)
    {
        if (clickedUnit.Team == Team.Player)
        {
            _selectedUnit = clickedUnit;
            _message = $"Selected {clickedUnit.Name} | HP {clickedUnit.HP}/{clickedUnit.MaxHP}";
            return;
        }

        if (_selectedUnit != null && CanPlayerAttack(_selectedUnit, clickedUnit))
        {
            AttackTarget(_selectedUnit, clickedUnit);
            CheckWinLose();
            return;
        }

        _message = $"Enemy: {clickedUnit.Name} | HP {clickedUnit.HP}/{clickedUnit.MaxHP}";
    }

    private void HandleEmptyTileClick(Tile tile)
    {
        if (_selectedUnit == null)
        {
            _message = $"Empty tile: Row {tile.Row}, Col {tile.Col}, Terrain {tile.Terrain}";
            return;
        }

        if (CanMove(_selectedUnit, tile.Row, tile.Col))
        {
            _selectedUnit.MoveTo(tile.Row, tile.Col);
            _message = $"{_selectedUnit.Name} moved to Row {tile.Row}, Col {tile.Col}";
            return;
        }

        _message = $"Cannot move to Row {tile.Row}, Col {tile.Col}";
    }

    private bool CanMove(Unit unit, int targetRow, int targetCol)
    {
        if (_state != GameState.PlayerTurn)
        {
            return false;
        }

        if (unit.Team != Team.Player)
        {
            return false;
        }

        if (unit.HasMoved)
        {
            return false;
        }

        if (!_map.IsInside(targetRow, targetCol))
        {
            return false;
        }

        if (!_map.IsWalkable(targetRow, targetCol))
        {
            return false;
        }

        if (GetUnitAt(targetRow, targetCol) != null)
        {
            return false;
        }

        int movementCost = CalculateMovementCost(unit, targetRow, targetCol);
        return movementCost <= unit.MoveRange;
    }

    private bool CanPlayerAttack(Unit attacker, Unit target)
    {
        if (_state != GameState.PlayerTurn)
        {
            return false;
        }

        if (attacker.Team != Team.Player)
        {
            return false;
        }

        if (target.Team != Team.Enemy)
        {
            return false;
        }

        if (attacker.HasActed)
        {
            return false;
        }

        return IsTargetInAttackRange(attacker, target);
    }

    private bool CanEnemyAttack(Unit attacker, Unit target)
    {
        if (attacker.Team != Team.Enemy)
        {
            return false;
        }

        if (target.Team != Team.Player)
        {
            return false;
        }

        if (attacker.HasActed)
        {
            return false;
        }

        return IsTargetInAttackRange(attacker, target);
    }

    private bool IsTargetInAttackRange(Unit attacker, Unit target)
    {
        int distance = ManhattanDistance(attacker.Row, attacker.Col, target.Row, target.Col);
        int attackRange = _combatService.GetAttackRange(attacker);

        return distance <= attackRange;
    }

    private void AttackTarget(Unit attacker, Unit target)
    {
        int damage = _combatService.CalculateDamage(attacker, target);

        target.TakeDamage(damage);
        attacker.HasActed = true;

        _message = $"{attacker.Name} attacked {target.Name} for {damage} damage";

        RemoveDeadUnits();
    }

    private void StartPlayerTurn()
    {
        _state = GameState.PlayerTurn;
        _selectedUnit = null;

        foreach (Unit unit in _units)
        {
            if (unit.Team == Team.Player && unit.IsAlive())
            {
                unit.ResetTurn();
            }
        }

        _message = $"Player Turn {_turnNumber}: click a player unit";
    }

    private void EndPlayerTurn()
    {
        if (_state != GameState.PlayerTurn)
        {
            return;
        }

        _selectedUnit = null;
        _state = GameState.EnemyTurn;
        _message = "Enemy Turn";
    }

    private void RunEnemyTurn()
    {
        foreach (Unit unit in new List<Unit>(_units))
        {
            if (unit.Team != Team.Enemy || !unit.IsAlive())
            {
                continue;
            }

            EnemyUnit enemy = (EnemyUnit)unit;
            enemy.ResetTurn();

            Unit? target = _enemyAI.ChooseNearestTarget(enemy, _units);

            if (target == null)
            {
                CheckWinLose();
                return;
            }

            if (CanEnemyAttack(enemy, target))
            {
                AttackTarget(enemy, target);
                CheckWinLose();

                if (_state == GameState.Win || _state == GameState.Lose)
                {
                    return;
                }

                continue;
            }

            _enemyAI.MoveToward(enemy, target, _units);

            if (target.IsAlive() && CanEnemyAttack(enemy, target))
            {
                AttackTarget(enemy, target);
                CheckWinLose();

                if (_state == GameState.Win || _state == GameState.Lose)
                {
                    return;
                }
            }
        }

        _turnNumber++;
        StartPlayerTurn();
    }

    private void CheckWinLose()
    {
        bool anyPlayerAlive = false;
        bool anyEnemyAlive = false;

        foreach (Unit unit in _units)
        {
            if (!unit.IsAlive())
            {
                continue;
            }

            if (unit.Team == Team.Player)
            {
                anyPlayerAlive = true;
            }

            if (unit.Team == Team.Enemy)
            {
                anyEnemyAlive = true;
            }
        }

        if (!anyEnemyAlive)
        {
            _state = GameState.Win;
            _selectedUnit = null;
            _message = "Victory: all enemies defeated";
            return;
        }

        if (!anyPlayerAlive)
        {
            _state = GameState.Lose;
            _selectedUnit = null;
            _message = "Defeat: all player units defeated";
        }
    }

    private void RemoveDeadUnits()
    {
        for (int i = _units.Count - 1; i >= 0; i--)
        {
            if (!_units[i].IsAlive())
            {
                if (_selectedUnit == _units[i])
                {
                    _selectedUnit = null;
                }

                _units.RemoveAt(i);
            }
        }
    }

    private int ManhattanDistance(int rowA, int colA, int rowB, int colB)
    {
        return Math.Abs(rowA - rowB) + Math.Abs(colA - colB);
    }

    private Unit? GetUnitAt(int row, int col)
    {
        foreach (Unit unit in _units)
        {
            if (unit.IsAlive() && unit.IsAt(row, col))
            {
                return unit;
            }
        }

        return null;
    }
    private int CalculateMovementCost(Unit unit, int targetRow, int targetCol)
    {
        int[,] cost = new int[_map.Rows, _map.Cols];

        for (int row = 0; row < _map.Rows; row++)
        {
            for (int col = 0; col < _map.Cols; col++)
            {
                cost[row, col] = int.MaxValue;
            }
        }

        Queue<(int Row, int Col)> queue = new Queue<(int Row, int Col)>();

        cost[unit.Row, unit.Col] = 0;
        queue.Enqueue((unit.Row, unit.Col));

        int[,] directions =
        {
        { -1, 0 },
        { 1, 0 },
        { 0, -1 },
        { 0, 1 }
    };

        while (queue.Count > 0)
        {
            (int currentRow, int currentCol) = queue.Dequeue();

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int nextRow = currentRow + directions[i, 0];
                int nextCol = currentCol + directions[i, 1];

                if (!_map.IsInside(nextRow, nextCol))
                {
                    continue;
                }

                if (!_map.IsWalkable(nextRow, nextCol))
                {
                    continue;
                }

                Unit? occupyingUnit = GetUnitAt(nextRow, nextCol);

                if (occupyingUnit != null && !(nextRow == targetRow && nextCol == targetCol))
                {
                    continue;
                }

                int nextCost = cost[currentRow, currentCol] + _map.MovementCost(nextRow, nextCol);

                if (nextCost < cost[nextRow, nextCol])
                {
                    cost[nextRow, nextCol] = nextCost;
                    queue.Enqueue((nextRow, nextCol));
                }
            }
        }

        return cost[targetRow, targetCol];
    }

    public void Draw()
    {
        SplashKit.ClearScreen(Color.White);

        SplashKit.DrawText("Grid Tactics - Phase 6", Color.Black, 20, 20);
        SplashKit.DrawText($"State: {_state} | Turn: {_turnNumber}", Color.Black, 20, 45);
        SplashKit.DrawText(_message, Color.Black, 20, 70);

        DrawSelectedUnitPanel();

        _map.Draw();

        if (_state == GameState.PlayerTurn)
        {
            DrawMovementRange();
            DrawAttackRange();
        }

        DrawUnits();

        if (_state == GameState.Win)
        {
            DrawEndMessage("VICTORY");
        }

        if (_state == GameState.Lose)
        {
            DrawEndMessage("DEFEAT");
        }

        SplashKit.RefreshScreen(60);
    }

    private void DrawMovementRange()
    {
        if (_selectedUnit == null)
        {
            return;
        }

        if (_selectedUnit.Team != Team.Player)
        {
            return;
        }

        if (_selectedUnit.HasMoved)
        {
            return;
        }

        for (int row = 0; row < _map.Rows; row++)
        {
            for (int col = 0; col < _map.Cols; col++)
            {
                if (CanMove(_selectedUnit, row, col))
                {
                    DrawTileOverlay(row, col, Color.RGBAColor(0, 80, 255, 80));
                }
            }
        }
    }

    private void DrawAttackRange()
    {
        if (_selectedUnit == null)
        {
            return;
        }

        if (_selectedUnit.Team != Team.Player)
        {
            return;
        }

        if (_selectedUnit.HasActed)
        {
            return;
        }

        int attackRange = _combatService.GetAttackRange(_selectedUnit);

        for (int row = 0; row < _map.Rows; row++)
        {
            for (int col = 0; col < _map.Cols; col++)
            {
                int distance = ManhattanDistance(_selectedUnit.Row, _selectedUnit.Col, row, col);

                if (distance <= attackRange)
                {
                    DrawTileOverlay(row, col, Color.RGBAColor(255, 210, 0, 80));
                }
            }
        }
    }

    private void DrawTileOverlay(int row, int col, Color color)
    {
        int x = _map.OffsetX + col * _map.TileSize;
        int y = _map.OffsetY + row * _map.TileSize;

        SplashKit.FillRectangle(color, x, y, _map.TileSize, _map.TileSize);
    }

    private void DrawUnits()
    {
        foreach (Unit unit in _units)
        {
            bool isSelected = _selectedUnit == unit;
            unit.Draw(_map.OffsetX, _map.OffsetY, _map.TileSize, isSelected);
        }
    }

    private void DrawSelectedUnitPanel()
    {
        int x = 20;
        int y = 620;

        if (_selectedUnit == null)
        {
            SplashKit.DrawText("Selected Unit: none | Space = End Turn | Esc = Deselect", Color.Black, x, y);
            return;
        }

        string movedText = _selectedUnit.HasMoved ? "Moved" : "Can move";
        string actedText = _selectedUnit.HasActed ? "Acted" : "Can act";

        int effectiveRange = _combatService.GetAttackRange(_selectedUnit);

        SplashKit.DrawText(
            $"Selected Unit: {_selectedUnit.Name} | HP {_selectedUnit.HP}/{_selectedUnit.MaxHP} | Move {_selectedUnit.MoveRange} | Attack {_selectedUnit.AttackPower} | Range {effectiveRange} | {movedText} | {actedText}",
            Color.Black,
            x,
            y
        );
    }

    private void DrawEndMessage(string text)
    {
        SplashKit.FillRectangle(Color.RGBAColor(255, 255, 255, 220), 180, 250, 360, 120);
        SplashKit.DrawRectangle(Color.Black, 180, 250, 360, 120);
        SplashKit.DrawText(text, Color.Black, 310, 295);
    }
}