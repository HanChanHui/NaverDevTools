using System.Collections.Generic;
using UnityEngine;

namespace HornSpirit {
    [CreateAssetMenu(fileName = "PatternData", menuName = "ScriptableObjects/PatternData", order = 1)]
    public class PatternData : ScriptableObject {
        public int[,] basePattern1 = new int[,] {
        { 0, 1},
    };
        public int[,] basePattern2 = new int[,] {
        { 0, 1, 1},
    };
        public int[,] basePattern3 = new int[,] {
        { 1, 0, 1},
    };
        public int[,] basePattern4 = new int[,] {
        { 0, 1},
        { 0, 1},
        { 0, 1},
    };
        public int[,] basePattern5 = new int[,] {
        { 1, 1, 0, 1, 1},
    };
        public int[,] basePattern6 = new int[,] {
        { 1, 0, 1},
        { 1, 0, 1},
        { 1, 0, 1},
    };
        public int[,] basePattern7 = new int[,] {
        { 0, 1, 1, 1, 1},
    };
        public int[,] basePattern8 = new int[,] {
        { 1, 1, 1, 0},
        { 0, 1, 1, 1},
        { 1, 1, 1, 0},
    };
        public int[,] basePattern9 = new int[,] {
        { 0, 1, 1, 0},
        { 1, 1, 1, 1},
        { 0, 1, 1, 1},
        { 1, 1, 1, 1},
        { 0, 1, 1, 0},
    };
        public int[,] basePattern10 = new int[,] {
        { 1, 1, 0},
        { 0, 1, 1},
        { 1, 1, 0},
    };
        public int[,] basePattern11 = new int[,] {
        { 1, 1, 1, 1},
        { 0, 1, 1, 1},
        { 1, 1, 1, 1},
    };
        public int[,] basePattern12 = new int[,] {
        { 1, 1, 0, 0, 0},
        { 0, 1, 1, 1, 1},
        { 1, 1, 0, 0, 0},
    };
        public int[,] basePattern13 = new int[,] {
        { 1, 1, 0, 0},
        { 0, 1, 1, 1},
        { 1, 1, 0, 0},
    };
        public int[,] basePattern14 = new int[,] {
        { 1, 1, 1},
        { 1, 0, 1},
        { 1, 1, 1},
    };
        public int[,] basePattern15 = new int[,] {
        { 1, 1, 1, 1},
        { 1, 0, 1, 1},
        { 1, 1, 1, 1},
    };
        public int[,] basePattern16 = new int[,] {
        { 1, 1},
        { 0, 1},
        { 1, 1},
    };
        public int[,] basePattern17 = new int[,] {
        { 0, 1, 1, 0},
        { 0, 1, 1, 1},
        { 0, 1, 1, 0},
    };
        public int[,] basePattern18 = new int[,] {
        { 0, 0, 1, 0, 0},
        { 0, 1, 1, 1, 0},
        { 1, 1, 0, 1, 1},
        { 0, 1, 1, 1, 0},
        { 0, 0, 1, 0, 0},
    };
        public int[,] basePattern19 = new int[,] {
        { 0, 1, 1, 1},
        { 0, 1, 1, 1},
        { 0, 1, 1, 1},
    };
        public int[,] basePattern20 = new int[,] {
        { 0, 1, 0},
        { 1, 0, 1},
        { 0, 1, 0},
    };

        public List<Vector2Int> GetPattern(int patternIndex) {
            switch (patternIndex) {
                case 1:
                    return ConvertPatternToList(basePattern1);
                case 2:
                    return ConvertPatternToList(basePattern2);
                case 3:
                    return CenterConvertPatternToList(basePattern3);
                case 4:
                    return ConvertPatternToList(basePattern4);
                case 5:
                    return CenterConvertPatternToList(basePattern5);
                case 6:
                    return CenterConvertPatternToList(basePattern6);
                case 7:
                    return ConvertPatternToList(basePattern7);
                case 8:
                    return ConvertPatternToList(basePattern8);
                case 9:
                    return ConvertPatternToList(basePattern9);
                case 10:
                    return ConvertPatternToList(basePattern10);
                case 11:
                    return ConvertPatternToList(basePattern11);
                case 12:
                    return ConvertPatternToList(basePattern12);
                case 13:
                    return ConvertPatternToList(basePattern13);
                case 14:
                    return CenterConvertPatternToList(basePattern14);
                case 15:
                    return CenterConvertPatternToList(basePattern15);
                case 16:
                    return ConvertPatternToList(basePattern16);
                case 17:
                    return ConvertPatternToList(basePattern17);
                case 18:
                    return CenterConvertPatternToList(basePattern18);
                case 19:
                    return ConvertPatternToList(basePattern19);
                case 20:
                    return CenterConvertPatternToList(basePattern20);
                default:
                    Debug.LogWarning("Invalid pattern index! Returning null.");
                    return null;
            }
        }

        public List<Vector2Int> ConvertPatternToList(int[,] patternArray) {
            List<Vector2Int> patternList = new List<Vector2Int>();

            int rows = patternArray.GetLength(0);
            int cols = patternArray.GetLength(1);

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++) {
                    if (patternArray[y, x] == 1) {
                        patternList.Add(new Vector2Int(x, y - (rows / 2))); // 중앙 정렬
                    }
                }
            }

            return patternList;
        }

        public List<Vector2Int> CenterConvertPatternToList(int[,] patternArray) {
            List<Vector2Int> patternList = new List<Vector2Int>();

            int rows = patternArray.GetLength(0);
            int cols = patternArray.GetLength(1);

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++) {
                    if (patternArray[y, x] == 1) {
                        patternList.Add(new Vector2Int(x - (cols / 2), y - (rows / 2))); // 중앙 정렬
                    }
                }
            }

            return patternList;
        }

        public List<Vector2Int> GetDirectionVector(List<Vector2Int> patternArray, Consts.AttackDirection direction = Consts.AttackDirection.Right) {
            List<Vector2Int> pattern = patternArray;

            switch (direction) {
                case Consts.AttackDirection.Right:
                    return pattern;

                case Consts.AttackDirection.Left:
                    return MirrorPattern(pattern); // 좌우 반전으로 Left 방향 변환

                case Consts.AttackDirection.Up:
                    return RotatePatternUp(pattern); // 90도 회전으로 Up 방향 변환

                case Consts.AttackDirection.Down:
                    return RotatePatternDown(pattern); // 90도 회전으로 Down 방향 변환

                default:
                    return pattern;
            }
        }

        public List<Vector2Int> MirrorPattern(List<Vector2Int> pattern) {
            List<Vector2Int> mirroredPattern = new List<Vector2Int>();
            foreach (Vector2Int vector in pattern) {
                mirroredPattern.Add(new Vector2Int(-vector.x, vector.y));
            }
            return mirroredPattern;
        }

        public List<Vector2Int> RotatePatternDown(List<Vector2Int> pattern) {
            List<Vector2Int> rotatedPattern = new List<Vector2Int>();
            foreach (Vector2Int vector in pattern) {
                rotatedPattern.Add(new Vector2Int(vector.y, -vector.x));
            }
            return rotatedPattern;
        }

        public List<Vector2Int> RotatePatternUp(List<Vector2Int> pattern) {
            List<Vector2Int> rotatedPattern = new List<Vector2Int>();
            foreach (Vector2Int vector in pattern) {
                rotatedPattern.Add(new Vector2Int(-vector.y, vector.x));
            }
            return rotatedPattern;
        }
    }
}
