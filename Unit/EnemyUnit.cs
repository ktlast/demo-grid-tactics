using SplashKitSDK;

public class EnemyUnit : Unit
{
    public string EnemyType { get; }

    public EnemyUnit(
        string name,
        string enemyType,
        int row,
        int col,
        int hp,
        int moveRange,
        int attackPower,
        int attackRange
    )
        : base(name, Team.Enemy, row, col, hp, moveRange, attackPower, attackRange)
    {
        EnemyType = enemyType;
    }

    public override Color GetDisplayColor()
    {
        if (EnemyType == "Goblin")
        {
            return Color.Red;
        }

        if (EnemyType == "Orc")
        {
            return Color.DarkRed;
        }

        if (EnemyType == "Shaman")
        {
            return Color.Purple;
        }

        return Color.Red;
    }
}