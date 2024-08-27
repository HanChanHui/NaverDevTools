
namespace Consts {
   public enum TowerType {
      None,
      Dealer,
      Tanker,
      Healer,
   }

   public enum TowerAttackType {
      None,
      Single,
      Range,
   }

   public enum TowerProperty {
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

   public enum LayerName {
      Grid,
      BackObject,
      Block,
      TwoLayerGrid,
      Tower,
      PlaceGrid
   }

   public enum BlockType {
      None,
      Block,
      BenBlock,
      TargetBlock,
      SpawnerBlock,
      SpawnerTwoLayerBlock,
   }

   public enum EnemyType {
      General,
      Boss,
   }

   public enum AttackDirection {
      None,
      Up,
      Down,
      Left,
      Right
   }

   public enum StageType {
      Grass,

   }

   public enum SpawnState { 
      Spawning, 
      Waiting, 
      Finish 
   }

   public enum SceneType
   {
      TitleScene,
      BattleScene,
   }

   public enum AttackRangeType 
   {
      None,
      Pattern1,
      Pattern2,
      Pattern3,
      Pattern4,
      Pattern5,
      Pattern6,
      Pattern7,
      Pattern8,
      Pattern9,
      Pattern10,
      Pattern11,
      Pattern12,
      Pattern13,
      Pattern14,
      Pattern15,
      Pattern16,
      Pattern17,
      Pattern18,
      Pattern19,
      Pattern20,
   }

   public enum GoogleSheetType
   {
      TowerManager,
      AttackRange,
      Tower,
      TowerValue,
      TowerUpgradeValue,
      Monster,
      Level,
      Stage,
      Equipment,
   }
}

