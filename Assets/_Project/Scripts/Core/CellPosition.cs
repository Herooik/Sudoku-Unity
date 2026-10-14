#nullable enable

using System;

namespace Sudoku.Core
{
	public readonly struct CellPosition : IEquatable<CellPosition>
	{
		public CellPosition(int row, int column)
		{
			Row = row;
			Column = column;
		}

		public int Row { get; }
		public int Column { get; }

		public bool Equals(CellPosition other) => Row == other.Row && Column == other.Column;

		public override bool Equals(object? obj) => obj is CellPosition other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(Row, Column);

		public static bool operator ==(CellPosition left, CellPosition right) => left.Equals(right);

		public static bool operator !=(CellPosition left, CellPosition right) => !left.Equals(right);

		public override string ToString() => $"({Row},{Column})";
	}
}
