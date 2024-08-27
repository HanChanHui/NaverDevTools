using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class GridObject {
        private GridSystem<GridObject> gridSystem;
        private GridPosition gridPosition;
        private List<Tower> towerList;
        private List<Block> blockList;
        private List<BaseEnemy> enemyList;

        public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition) {
            this.gridSystem = gridSystem;
            this.gridPosition = gridPosition;
            towerList = new List<Tower>();
            blockList = new List<Block>();
            enemyList = new List<BaseEnemy>();
        }

        public override string ToString() {
            string towerString = "";
            foreach (Tower tower in towerList) {
                towerString += tower + "\n";
            }

            return towerString;
        }

        public void AddTower(Tower unit) {
            towerList.Add(unit);
        }
        public void AddBlock(Block block) {
            blockList.Add(block);
        }
        public void AddEnemy(BaseEnemy enemy) {
            enemyList.Add(enemy);
        }

        public void RemoveTower(Tower unit) {
            towerList.Remove(unit);
        }
        public void RemoveBlock(Block block) {
            blockList.Remove(block);
        }
        public void RemoveEnemy(BaseEnemy enemy) {
            enemyList.Remove(enemy);
        }

        public List<Tower> GetTowerList() {
            return towerList;
        }

        public bool HasAnyTower() {
            return towerList.Count > 0;
        }

        public bool HasAnyBlock() {
            return blockList.Count > 0;
        }

        public bool HasAnyEnemy() {
            return enemyList.Count > 0;
        }


        public Tower GetTower() {
            if (HasAnyTower()) {
                return towerList[0];
            } else {
                return null;
            }
        }

        public Block GetBlock() {
            if (HasAnyBlock()) {
                return blockList[0];
            } else {
                return null;
            }
        }

        public BaseEnemy GetEnemy() {
            if (HasAnyEnemy()) {
                return enemyList[0];
            } else {
                return null;
            }
        }
    }
}
