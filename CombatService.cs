public class CombatService
{
    private readonly GameMap _map;

    public CombatService(GameMap map)
    {
        _map = map;
    }

    public int GetAttackRange(Unit attacker)
    {
        int range = attacker.AttackRange;

        TerrainType attackerTerrain = _map.GetTile(attacker.Row, attacker.Col).Terrain;

        if (attackerTerrain == TerrainType.Hill)
        {
            range += 1;
        }

        return range;
    }

    public int CalculateDamage(Unit attacker, Unit defender)
    {
        int damage = attacker.AttackPower;

        TerrainType defenderTerrain = _map.GetTile(defender.Row, defender.Col).Terrain;

        if (defenderTerrain == TerrainType.Forest)
        {
            damage -= 1;
        }

        if (damage < 1)
        {
            damage = 1;
        }

        return damage;
    }
}