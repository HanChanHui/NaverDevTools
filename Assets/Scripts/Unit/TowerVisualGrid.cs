using Consts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    public class TowerVisualGrid : MonoBehaviour {

        private PatternData patternData;
        [SerializeField] private AttackRangeType attackRangeType;
        [SerializeField] private AttackDirection attackDirectionType = AttackDirection.None;
        private List<GridSystemVisualSingle> gridSystemVisualSingleList;
        private Material material;
        private bool isTwoLayerType = false;

        delegate void RangeFunc();
        RangeFunc rangeFunc;

        public void Init() {
            gridSystemVisualSingleList = new List<GridSystemVisualSingle>();

            material = GridSystemVisual.Instance.GetGridVisualTypeMaterial(GridVisualType.Forbidden);
            patternData = ResourceManager.Instance.GetPatternData;
            SetType(attackRangeType, attackDirectionType);
            rangeFunc();
        }

        public void SetDirection(bool isTwoLayer, AttackDirection attackDirectionType) {
            this.isTwoLayerType = isTwoLayer;
            this.attackDirectionType = attackDirectionType;
        }

        private void SetType(AttackRangeType type, AttackDirection directionType) {
            this.attackRangeType = type;
            this.attackDirectionType = directionType;
            rangeFunc = GetRangeFunc();
        }

        private void RemoveType() {
            rangeFunc -= GetRangeFunc();
        }

        private void SetGridSystemVisualList(Vector3 worldPosition) {
            Transform gridSystemVisualSingleTransform = Instantiate(ResourceManager.Instance.GridSystemVisualSingPrefab, worldPosition, Quaternion.identity);
            gridSystemVisualSingleTransform.transform.parent = transform;
            gridSystemVisualSingleList.Add(gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>());
        }

        private void AttackPatternRange() {
            List<Vector2Int> pattern;
            if (attackDirectionType != AttackDirection.None) {
                pattern = patternData.GetDirectionVector(patternData.GetPattern((int)attackRangeType), attackDirectionType);
            } else {
                pattern = patternData.GetDirectionVector(patternData.GetPattern((int)attackRangeType));
            }

            if (isTwoLayerType) {
                foreach (Vector2Int directionVector in pattern) {
                    Vector3 worldPosition = transform.position + new Vector3(directionVector.x * 2, directionVector.y * 2 - 0.65f, 0);
                    SetGridSystemVisualList(worldPosition);
                }
            } 
            else 
            {
                foreach (Vector2Int directionVector in pattern) {
                    Vector3 worldPosition = transform.position + new Vector3(directionVector.x * 2, directionVector.y * 2, 0);
                    SetGridSystemVisualList(worldPosition);
                }
            }

            foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) {
                gridVisual.Hide();
                gridVisual.GridLayerChange(LayerName.PlaceGrid.ToString());
            }
        }

        public void ShowAttackDirection()
        {
            List<Vector2Int> pattern;
            if (attackDirectionType != AttackDirection.None) {
                pattern = patternData.GetDirectionVector(patternData.GetPattern((int)attackRangeType), attackDirectionType);
            } else {
                pattern = patternData.GetDirectionVector(patternData.GetPattern((int)attackRangeType));
            }

            foreach (Vector2Int directionVector in pattern) 
            {
                    Vector3 worldPosition = transform.position + new Vector3(directionVector.x * 2, directionVector.y * 2, 0);
                    SetGridSystemVisualList(worldPosition);
            }
            
            foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) {
                gridVisual.GridLayerChange(LayerName.PlaceGrid.ToString());
            }
        }

        public void HideAllGridPosition() {
            foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) {
                gridVisual.Hide();
            }
        }

        public void ShowAllGridPosition() {
            foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) {
                gridVisual.Show(material);
            }
        }

        private RangeFunc GetRangeFunc() {
            return AttackPatternRange;
        }

        public void DestroyGridPositionList() {
            foreach (GridSystemVisualSingle gridVisual in gridSystemVisualSingleList) {
                Destroy(gridVisual.gameObject);
            }
        }

        private void OnDisable() {
            StopAllCoroutines();
            if (gridSystemVisualSingleList != null) {
                DestroyGridPositionList();
                RemoveType();
            }
        }

    }
}
