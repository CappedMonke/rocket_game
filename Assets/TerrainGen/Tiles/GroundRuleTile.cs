using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(menuName = "Tiles/GroundRuleTile")]
public class GroundRuleTile : RuleTile<GroundRuleTile.Neighbor>
{
    public class Neighbor : TilingRuleOutput.Neighbor
    {
        public const int Nothing = 3;
        public const int Anything = 4;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        return neighbor switch
        {
            Neighbor.Nothing => tile == null,
            Neighbor.Anything => tile != null,
            _ => base.RuleMatch(neighbor, tile),
        };
    }
}
