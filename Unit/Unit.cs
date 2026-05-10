using SplashKitSDK;

public class Unit
{
    public string Name { get; }
    public Team Team { get; }

    public int Row { get; protected set; }
    public int Col { get; protected set; }

    public int HP { get; protected set; }
    public int MaxHP { get; }

    public int MoveRange { get; }
    public int AttackPower { get; }
    public int AttackRange { get; }

    public bool HasMoved { get; set; }
    public bool HasActed { get; set; }

    public Unit(
        string name,
        Team team,
        int row,
        int col,
        int hp,
        int moveRange,
        int attackPower,
        int attackRange
    )
    {
        Name = name;
        Team = team;
        Row = row;
        Col = col;
        HP = hp;
        MaxHP = hp;
        MoveRange = moveRange;
        AttackPower = attackPower;
        AttackRange = attackRange;
        HasMoved = false;
        HasActed = false;
    }

    public bool IsAlive()
    {
        return HP > 0;
    }

    public bool IsAt(int row, int col)
    {
        return Row == row && Col == col;
    }
    public void MoveTo(int row, int col)
    {
        Row = row;
        Col = col;
        HasMoved = true;
    }
    public void TakeDamage(int amount)
    {
        HP -= amount;

        if (HP < 0)
        {
            HP = 0;
        }
    }
    public void ResetTurn()
    {
        HasMoved = false;
        HasActed = false;
    }

    public virtual Color GetDisplayColor()
    {
        return Team == Team.Player ? Color.Blue : Color.Red;
    }

    public virtual void Draw(int offsetX, int offsetY, int tileSize, bool isSelected)
    {
        int centerX = offsetX + Col * tileSize + tileSize / 2;
        int centerY = offsetY + Row * tileSize + tileSize / 2;
        int radius = tileSize / 3;

        SplashKit.FillCircle(GetDisplayColor(), centerX, centerY, radius);

        if (isSelected)
        {
            SplashKit.DrawCircle(Color.Yellow, centerX, centerY, radius + 5);
            SplashKit.DrawCircle(Color.Yellow, centerX, centerY, radius + 6);
        }

        SplashKit.DrawText(Name.Substring(0, 1), Color.White, centerX - 5, centerY - 18);
        SplashKit.DrawText(HP.ToString(), Color.White, centerX - 8, centerY + 2);
    }
}
