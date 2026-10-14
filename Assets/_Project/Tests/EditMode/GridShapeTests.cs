using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests.EditMode
{
	public class GridShapeTests
	{
		private static IEnumerable<TestCaseData> SupportedShapes()
		{
			yield return new TestCaseData(2, 2);
			yield return new TestCaseData(2, 3);
			yield return new TestCaseData(3, 3);
			yield return new TestCaseData(4, 4);
		}

		[TestCase(2, 2, 4, 16)]
		[TestCase(2, 3, 6, 36)]
		[TestCase(3, 3, 9, 81)]
		[TestCase(4, 4, 16, 256)]
		public void Constructor_SupportedShape_SetsDimensions(
			int boxRows, int boxColumns, int expectedSize, int expectedCellCount)
		{
			var shape = new GridShape(boxRows, boxColumns);

			Assert.That(shape.BoxRows, Is.EqualTo(boxRows));
			Assert.That(shape.BoxColumns, Is.EqualTo(boxColumns));
			Assert.That(shape.Size, Is.EqualTo(expectedSize));  
			Assert.That(shape.CellCount, Is.EqualTo(expectedCellCount));
		}

		[TestCase(0, 3)]
		[TestCase(3, 0)]
		[TestCase(-1, 2)]
		[TestCase(1, 1)]
		[TestCase(3, 2)]
		[TestCase(2, 4)]
		[TestCase(5, 5)]
		public void Constructor_UnsupportedShape_ThrowsArgumentException(int boxRows, int boxColumns)
		{
			Assert.That(() => new GridShape(boxRows, boxColumns), Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void ToIndex_6x6Row3Column4_Returns22()
		{
			var shape = Create6x6();

			Assert.That(shape.ToIndex(new CellPosition(3, 4)), Is.EqualTo(22));
		}

		[Test]
		public void ToPosition_6x6Index22_ReturnsRow3Column4()
		{
			var shape = Create6x6();

			Assert.That(shape.ToPosition(22), Is.EqualTo(new CellPosition(3, 4)));
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void ToPosition_EveryIndex_RoundTripsThroughToIndex(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			for (int index = 0; index < shape.CellCount; index++)
			{
				CellPosition position = shape.ToPosition(index);
				Assert.That(shape.ToIndex(position), Is.EqualTo(index), $"index {index} -> {position}");
			}
		}

		[TestCase(7, 0)]
		[TestCase(-1, 0)]
		[TestCase(0, 6)]
		[TestCase(0, -1)]
		public void ToIndex_PositionOutsideGrid_ThrowsArgumentOutOfRangeException(int row, int column)
		{
			var shape = Create6x6();

			Assert.That(() => shape.ToIndex(new CellPosition(row, column)),
				Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[TestCase(-1)]
		[TestCase(36)]
		public void ToPosition_IndexOutsideGrid_ThrowsArgumentOutOfRangeException(int index)
		{
			var shape = Create6x6();

			Assert.That(() => shape.ToPosition(index), Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void GetPeers_6x6Row3Column4_ReturnsHandComputedTwelveCells()
		{
			var shape = Create6x6();
			int[] expected =
			{
				// row 3
				Index(3, 0, 6), Index(3, 1, 6), Index(3, 2, 6), Index(3, 3, 6), Index(3, 5, 6),
				// column 4
				Index(0, 4, 6), Index(1, 4, 6), Index(2, 4, 6), Index(4, 4, 6), Index(5, 4, 6),
				// box 3, only the cells not already in row 3 or column 4
				Index(2, 3, 6), Index(2, 5, 6),
			};

			Assert.That(shape.GetPeers(Index(3, 4, 6)), Is.EquivalentTo(expected));
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void GetPeers_AnyCell_DoesNotContainItself(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			for (int cell = 0; cell < shape.CellCount; cell++)
			{
				Assert.That(shape.GetPeers(cell), Has.No.Member(cell), $"cell {cell}");
			}
		}

		[TestCase(2, 2, 7)]
		[TestCase(2, 3, 12)]
		[TestCase(3, 3, 20)]
		[TestCase(4, 4, 39)]
		public void GetPeers_AnyCell_HasExpectedCount(int boxRows, int boxColumns, int expectedCount)
		{
			var shape = new GridShape(boxRows, boxColumns);

			for (int cell = 0; cell < shape.CellCount; cell++)
			{
				Assert.That(shape.GetPeers(cell).Count, Is.EqualTo(expectedCount), $"cell {cell}");
			}
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void GetPeers_AnyCell_HasNoDuplicates(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			for (int cell = 0; cell < shape.CellCount; cell++)
			{
				Assert.That(shape.GetPeers(cell), Is.Unique, $"cell {cell}");
			}
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void GetPeers_AnyTwoCells_RelationIsSymmetric(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			for (int cell = 0; cell < shape.CellCount; cell++)
			{
				foreach (int peer in shape.GetPeers(cell))
				{
					Assert.That(shape.GetPeers(peer), Has.Member(cell), $"{cell} is a peer of {peer}");
				}
			}
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void GetPeers_AnyCell_EqualsUnionOfItsUnitsWithoutItself(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			for (int cell = 0; cell < shape.CellCount; cell++)
			{
				IEnumerable<int> expected = shape.Units
					.Where(unit => unit.Contains(cell))
					.SelectMany(unit => unit)
					.Distinct()
					.Where(other => other != cell);

				Assert.That(shape.GetPeers(cell), Is.EquivalentTo(expected), $"cell {cell}");
			}
		}

		[TestCase(-1)]
		[TestCase(36)]
		public void GetPeers_IndexOutsideGrid_ThrowsArgumentOutOfRangeException(int index)
		{
			var shape = Create6x6();

			Assert.That(() => shape.GetPeers(index), Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void GetPeers_ReturnedList_CannotBeModifiedByCaller()
		{
			var shape = Create6x6();

			ReadOnlyAssert.CannotBeModified(shape.GetPeers(0));
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void Units_AnyShape_Has3NUnitsOfNDistinctCells(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			Assert.That(shape.Units.Count, Is.EqualTo(3 * shape.Size));
			foreach (IReadOnlyList<int> unit in shape.Units)
			{
				Assert.That(unit.Count, Is.EqualTo(shape.Size));
				Assert.That(unit, Is.Unique);
			}
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void Units_AnyShape_EveryCellBelongsToExactlyThreeUnits(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);
			var unitsPerCell = new int[shape.CellCount];

			foreach (IReadOnlyList<int> unit in shape.Units)
			{
				foreach (int cell in unit)
				{
					unitsPerCell[cell]++;
				}
			}

			Assert.That(unitsPerCell, Is.All.EqualTo(3));
		}

		[Test]
		public void Units_6x6_ContainsRow3AndColumn4()
		{
			var shape = Create6x6();
			int[] row3 = Enumerable.Range(0, 6).Select(column => Index(3, column, 6)).ToArray();
			int[] column4 = Enumerable.Range(0, 6).Select(row => Index(row, 4, 6)).ToArray();

			Assert.That(shape.Units, Has.Some.EquivalentTo(row3));
			Assert.That(shape.Units, Has.Some.EquivalentTo(column4));
		}

		[Test]
		public void Units_6x6_ContainsBox3()
		{
			var shape = Create6x6();
			int[] box3 =
			{
				Index(2, 3, 6), Index(2, 4, 6), Index(2, 5, 6),
				Index(3, 3, 6), Index(3, 4, 6), Index(3, 5, 6),
			};

			Assert.That(shape.Units, Has.Some.EquivalentTo(box3));
		}

		[Test]
		public void Units_ReturnedLists_CannotBeModifiedByCaller()
		{
			var shape = Create6x6();

			ReadOnlyAssert.CannotBeModified(shape.Units);
			foreach (IReadOnlyList<int> unit in shape.Units)
			{
				ReadOnlyAssert.CannotBeModified(unit);
			}
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void Boxes_AnyShape_HasNBoxesOfNDistinctCells(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			Assert.That(shape.Boxes.Count, Is.EqualTo(shape.Size));
			foreach (IReadOnlyList<int> box in shape.Boxes)
			{
				Assert.That(box.Count, Is.EqualTo(shape.Size));
				Assert.That(box, Is.Unique);
			}
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void Boxes_AnyShape_EveryCellBelongsToExactlyOneBox(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);
			var boxesPerCell = new int[shape.CellCount];

			foreach (IReadOnlyList<int> box in shape.Boxes)
			{
				foreach (int cell in box)
				{
					boxesPerCell[cell]++;
				}
			}

			Assert.That(boxesPerCell, Is.All.EqualTo(1));
		}

		[TestCaseSource(nameof(SupportedShapes))]
		public void Boxes_AnyShape_EachBoxIsAlsoAUnit(int boxRows, int boxColumns)
		{
			var shape = new GridShape(boxRows, boxColumns);

			foreach (IReadOnlyList<int> box in shape.Boxes)
			{
				Assert.That(shape.Units, Has.Some.EquivalentTo(box));
			}
		}

		[Test]
		public void Boxes_6x6_AreNumberedRowByRow()
		{
			var shape = Create6x6();
			int[] topLeftCells =
			{
				Index(0, 0, 6), Index(0, 3, 6),
				Index(2, 0, 6), Index(2, 3, 6),
				Index(4, 0, 6), Index(4, 3, 6),
			};

			Assert.That(shape.Boxes.Select(box => box[0]), Is.EqualTo(topLeftCells));
		}

		[Test]
		public void Boxes_6x6_Box3ListsCellsRowByRow()
		{
			var shape = Create6x6();
			int[] expected =
			{
				Index(2, 3, 6), Index(2, 4, 6), Index(2, 5, 6),
				Index(3, 3, 6), Index(3, 4, 6), Index(3, 5, 6),
			};

			// Exact order, not just the same set: BoardView relies on it for layout.
			Assert.That(shape.Boxes[3], Is.EqualTo(expected));
		}

		[Test]
		public void Boxes_ReturnedLists_CannotBeModifiedByCaller()
		{
			var shape = Create6x6();

			ReadOnlyAssert.CannotBeModified(shape.Boxes);
			foreach (IReadOnlyList<int> box in shape.Boxes)
			{
				ReadOnlyAssert.CannotBeModified(box);
			}
		}

		private static GridShape Create6x6() => new GridShape(2, 3);

		// Deliberately not GridShape.ToIndex, so peer and unit tests do not depend on the code under test.
		private static int Index(int row, int column, int size) => row * size + column;
	}
}
