using SplashKitSDK;

public class PlayerUnit : Unit
{
    public string Role { get; }

    public PlayerUnit(
        string name,
        string role,
        int row,
        int col,
        int hp,
        int moveRange,
        int attackPower,
        int attackRange
    )
        : base(name, Team.Player, row, col, hp, moveRange, attackPower, attackRange)
    {
        Role = role;
    }

    public override Color GetDisplayColor()
    {
        if (Name == "Warrior")
        {
            return Color.Blue;
        }

        if (Name == "Archer")
        {
            return Color.Orange;
        }

        return Color.Blue;
    }
}