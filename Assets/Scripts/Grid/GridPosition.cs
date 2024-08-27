using System;

namespace HornSpirit {
    public struct GridPosition : IEquatable<GridPosition> {
        public int x;
        public int y;
        public int z; // 층 정보를 위한 필드 추가

        public GridPosition(int x, int y, int z = 1) {
            this.x = x;
            this.y = y;
            this.z = z; // 층 정보 초기화
        }

        public override bool Equals(object obj) {
            return obj is GridPosition position &&
                    x == position.x &&
                    y == position.y &&
                    z == position.z;
        }

        public bool Equals(GridPosition other) {
            return this == other;
        }

        public override int GetHashCode() {
            return HashCode.Combine(x, y, z); // z 필드 포함
        }

        public override string ToString() {
            return $"x: {x}; y: {y}; z: {z}"; // z 필드 포함
        }

        public static bool operator ==(GridPosition a, GridPosition b) {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(GridPosition a, GridPosition b) {
            return !(a == b);
        }

        public static GridPosition operator +(GridPosition a, GridPosition b) {
            return new GridPosition(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static GridPosition operator -(GridPosition a, GridPosition b) {
            return new GridPosition(a.x - b.x, a.y - b.y, a.z - b.z);
        }
    }
}
