

namespace Consts
{
   public enum TowerType
   {
        None,
        Dealer,
        Tanker,
        Healer,
   }

   public enum TowerAttackType
   {
        None,
        Single,
        Range,
   }

   public enum TowerProperty
   {
        Fire,
        Water,
        Grass,
   }

   public enum GridVisualType {
        Empty,
        White,
        Green,
        Red,
        Forbidden,
    }

    public enum LayerName
    {
        Grid,
        BackObject,
        Block,
        TwoLayerGrid,
        Tower,
        PlaceGrid
    }

    public enum BlockType
    {
       Block,
       BenBlock,
       TargetBlock,

    }

    public enum EnemyType
    {
       General,
       Boss,
    }

    public enum AttackDirection 
    {
       Up,
       Down,
       Left,
       Right
   }
}

