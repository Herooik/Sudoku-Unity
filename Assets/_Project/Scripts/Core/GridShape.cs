#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Core
{
	// Defined by box dimensions rather than board size: a 6×6 board could have 2×3 or 3×2 boxes,
	// so the size alone would be ambiguous.
	public sealed class GridShape
	{
		// What the code can handle (16-bit notes, symbols up to 'G'), not which sizes the game offers:
		// that comes from the puzzle packs. One list feeds both the check and the error message.
		private static readonly IReadOnlyList<(int Rows, int Columns)> SupportedBoxShapes =
			Array.AsReadOnly(new[] { (2, 2), (2, 3), (3, 3), (4, 4) });

		private readonly IReadOnlyList<int>[] _peers;

		public GridShape(int boxRows, int boxColumns)
		{
			if (!IsSupported(boxRows, boxColumns))
			{
				var supported = string.Join(", ", SupportedBoxShapes.Select(shape => $"{shape.Rows}×{shape.Columns}"));
				throw new ArgumentException($"Unsupported box shape {boxRows}×{boxColumns}. Supported: {supported}.");
			}

			BoxRows = boxRows;
			BoxColumns = boxColumns;
			Size = BoxRows * BoxColumns;
			CellCount = Size * Size;

			var units = new IReadOnlyList<int>[3 * Size];
			for (var row = 0; row < Size; row++)
			{
				var cells = new int[Size];
				for (var i = 0; i < Size; i++)
				{
					cells[i] = ToIndex(new CellPosition(row, i));
				}

				units[row] = Array.AsReadOnly(cells);
			}

			for (var col = 0; col < Size; col++)
			{
				var cells = new int[Size];
				for (var i = 0; i < Size; i++)
				{
					cells[i] = ToIndex(new CellPosition(i, col));
				}

				units[col + Size] = Array.AsReadOnly(cells);
			}

			var boxes = new IReadOnlyList<int>[Size];
			for (var box = 0; box < Size; box++)
			{
				var cells = new int[Size];

				// Boxes per band is Size / BoxColumns, which always equals BoxRows.
				var band = box / BoxRows;
				var stack = box % BoxRows;
				var corner = new CellPosition(band * BoxRows, stack * BoxColumns);
				var filled = 0;
				for (var rowOffset = 0; rowOffset < BoxRows; rowOffset++)
				{
					for (var columnOffset = 0; columnOffset < BoxColumns; columnOffset++)
					{
						cells[filled++] = ToIndex(
							new CellPosition(corner.Row + rowOffset, corner.Column + columnOffset));
					}
				}

				var boxCells = Array.AsReadOnly(cells);
				boxes[box] = boxCells;
				units[2 * Size + box] = boxCells;
			}

			Units = Array.AsReadOnly(units);
			Boxes = Array.AsReadOnly(boxes);

			var peers = new HashSet<int>[CellCount];
			for (var i = 0; i < peers.Length; i++)
			{
				peers[i] = new HashSet<int>();
			}

			foreach (var unit in units)
			{
				foreach (var cell in unit)
				{
					peers[cell].UnionWith(unit);
				}
			}

			_peers = new IReadOnlyList<int>[CellCount];
			for (var i = 0; i < peers.Length; i++)
			{
				peers[i].Remove(i);

				// HashSet order is arbitrary; sorted peers keep debugging output readable.
				var sorted = peers[i].ToArray();
				Array.Sort(sorted);
				_peers[i] = Array.AsReadOnly(sorted);
			}
		}

		public int BoxRows { get; }

		public int BoxColumns { get; }

		public int Size { get; }

		public int CellCount { get; }

		public IReadOnlyList<IReadOnlyList<int>> Units { get; }

		// Unlike Units, the order here is a contract: boxes are numbered row by row and each box lists its cells
		// row by row, which is the order a grid layout places children in.
		public IReadOnlyList<IReadOnlyList<int>> Boxes { get; }

		public IReadOnlyList<int> GetPeers(int cellIndex)
		{
			if (cellIndex < 0 || cellIndex >= CellCount)
			{
				throw new ArgumentOutOfRangeException(
					nameof(cellIndex), cellIndex, $"Cell index must be in 0..{CellCount - 1}.");
			}

			return _peers[cellIndex];
		}

		public int ToIndex(CellPosition position)
		{
			if (position.Row < 0 || position.Row >= Size || position.Column < 0 || position.Column >= Size)
			{
				throw new ArgumentOutOfRangeException(
					nameof(position), position, $"Position must be within a {Size}×{Size} grid.");
			}

			return position.Row * Size + position.Column;
		}

		public CellPosition ToPosition(int cellIndex)
		{
			if (cellIndex < 0 || cellIndex >= CellCount)
			{
				throw new ArgumentOutOfRangeException(
					nameof(cellIndex), cellIndex, $"Cell index must be in 0..{CellCount - 1}.");
			}

			return new CellPosition(cellIndex / Size, cellIndex % Size);
		}

		private static bool IsSupported(int boxRows, int boxColumns)
		{
			return SupportedBoxShapes.Contains((boxRows, boxColumns));
		}
	}
}
