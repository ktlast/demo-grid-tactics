using System;
using System.Collections.Generic;

public class EnemyAI
{
    private readonly GameMap _map;

    public EnemyAI(GameMap map)
    {
        _map = map;
    }

    public Unit? ChooseNearestTarget(EnemyUnit enemy, List<Unit> units)
    {
        Unit? bestTarget = null;
        int bestDistance = int.MaxValue;

        foreach (Unit unit in units)
        {
            if (unit.Team != Team.Player || !unit.IsAlive())
            {
                continue;
            }

            int distance = ManhattanDistance(enemy.Row, enemy.Col, unit.Row, unit.Col);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = unit;
            }
        }

        return bestTarget;
    }

    public void MoveToward(EnemyUnit enemy, Unit target, List<Unit> units)
    {
        int remainingMove = enemy.MoveRange;

        while (remainingMove > 0)
        {
            int costUsed = MoveOneStepToward(enemy, target, units, remainingMove);

            if (costUsed == 0)
            {
                return;
            }

            remainingMove -= costUsed;
        }
    }

    private int MoveOneStepToward(EnemyUnit enemy, Unit target, List<Unit> units, int remainingMove)
    {
        int currentDistance = ManhattanDistance(enemy.Row, enemy.Col, target.Row, target.Col);

        int bestRow = enemy.Row;
        int bestCol = enemy.Col;
        int bestDistance = currentDistance;
        int bestCost = 0;

        int[,] directions =
        {
        { -1, 0 },
        { 1, 0 },
        { 0, -1 },
        { 0, 1 }
    };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int nextRow = enemy.Row + directions[i, 0];
            int nextCol = enemy.Col + directions[i, 1];

            if (!_map.IsWalkable(nextRow, nextCol))
            {
                continue;
            }

            if (IsOccupied(nextRow, nextCol, units))
            {
                continue;
            }

            int movementCost = _map.MovementCost(nextRow, nextCol);

            if (movementCost > remainingMove)
            {
                continue;
            }

            int nextDistance = ManhattanDistance(nextRow, nextCol, target.Row, target.Col);

            if (nextDistance < bestDistance)
            {
                bestDistance = nextDistance;
                bestRow = nextRow;
                bestCol = nextCol;
                bestCost = movementCost;
            }
        }

        if (bestRow == enemy.Row && bestCol == enemy.Col)
        {
            return 0;
        }

        enemy.MoveTo(bestRow, bestCol);
        return bestCost;
    }

    private bool IsOccupied(int row, int col, List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit.IsAlive() && unit.IsAt(row, col))
            {
                return true;
            }
        }

        return false;
    }

    private int ManhattanDistance(int rowA, int colA, int rowB, int colB)
    {
        return Math.Abs(rowA - rowB) + Math.Abs(colA - colB);
    }
}