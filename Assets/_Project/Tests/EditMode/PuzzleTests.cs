using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests.EditMode
{
	public class PuzzleTests
	{
		// 4×4 with 2×2 boxes:  1 2 | 3 4
		//                      3 4 | 1 2
		//                      ----+----
		//                      2 1 | 4 3
		//                      4 3 | 2 1
		private const string Solution4X4 = "1234341221434321";
		private const string Givens4X4 = "1..4.4..2..3...1";
		private const string NoGivens4X4 = "................";

		private const string Givens9X9 =
			"53..7....6..195....98....6.8...6...34..8.3..17...2...6.6....28....419..5....8..79";

		private const string Solution9X9 =
			"534678912672195348198342567859761423426853791713924856961537284287419635345286179";

		[Test]
		public void Constructor_ValidPuzzle_KeepsIdAndShape()
		{
			var shape = new GridShape(2, 2);

			var puzzle = new Puzzle("4x4-test", shape, Givens4X4, Solution4X4);

			Assert.That(puzzle.Id, Is.EqualTo("4x4-test"));
			Assert.That(puzzle.Shape, Is.SameAs(shape));
		}

		[Test]
		public void Constructor_ValidPuzzle_ParsesGivensWithZeroForEmpty()
		{
			var puzzle = Create4x4(Givens4X4, Solution4X4);

			Assert.That(puzzle.Givens, Is.EqualTo(new[] { 1, 0, 0, 4, 0, 4, 0, 0, 2, 0, 0, 3, 0, 0, 0, 1 }));
		}

		[Test]
		public void Constructor_ValidPuzzle_ParsesSolution()
		{
			var puzzle = Create4x4(Givens4X4, Solution4X4);

			Assert.That(puzzle.Solution, Is.EqualTo(new[] { 1, 2, 3, 4, 3, 4, 1, 2, 2, 1, 4, 3, 4, 3, 2, 1 }));
		}

		[Test]
		public void Constructor_Classic9x9_HasThirtyGivens()
		{
			var puzzle = new Puzzle("9x9-easy-001", new GridShape(3, 3), Givens9X9, Solution9X9);

			Assert.That(puzzle.Givens.Count(value => value != 0), Is.EqualTo(30));
		}

		[Test]
		public void Constructor_16x16Letters_ParseAsValuesTenToSixteen()
		{
			var solution = BuildPatternSolution(4);
			var noGivens = new string('.', solution.Length);

			var puzzle = new Puzzle("16x16-test", new GridShape(4, 4), noGivens, solution);

			Assert.That(puzzle.Solution[solution.IndexOf('A')], Is.EqualTo(10));
			Assert.That(puzzle.Solution[solution.IndexOf('G')], Is.EqualTo(16));
		}

		[TestCase(null)]
		[TestCase("")]
		public void Constructor_MissingId_ThrowsArgumentException(string id)
		{
			Assert.That(() => new Puzzle(id, new GridShape(2, 2), Givens4X4, Solution4X4),
				Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void Constructor_NullShape_ThrowsArgumentNullException()
		{
			Assert.That(() => new Puzzle("4x4-test", null, Givens4X4, Solution4X4),
				Throws.TypeOf<ArgumentNullException>());
		}

		[TestCase("1..4.4..2..3...", Solution4X4)]
		[TestCase(Givens4X4, "12343412214343211")]
		[TestCase(null, Solution4X4)]
		[TestCase(Givens4X4, null)]
		public void Constructor_WrongLengthOrNull_ThrowsArgumentException(string givens, string solution)
		{
			Assert.That(() => Create4x4(givens, solution), Throws.InstanceOf<ArgumentException>());
		}

		[TestCase("x..4.4..2..3...1")]
		[TestCase("0..4.4..2..3...1")]
		[TestCase("5..4.4..2..3...1")]
		[TestCase("A..4.4..2..3...1")]
		public void Constructor_GivenOutsideShapeValues_ThrowsArgumentException(string givens)
		{
			Assert.That(() => Create4x4(givens, Solution4X4), Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void Constructor_SolutionWithEmptyCell_ThrowsArgumentException()
		{
			Assert.That(() => Create4x4(NoGivens4X4, ".234341221434321"), Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void Constructor_GivenDiffersFromSolution_ThrowsArgumentException()
		{
			Assert.That(() => Create4x4("2...............", Solution4X4), Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void Constructor_SolutionBreaksRules_ThrowsArgumentException()
		{
			// Rows are fine, but column 0 holds 2 twice.
			Assert.That(() => Create4x4(NoGivens4X4, "2134341221434321"), Throws.InstanceOf<ArgumentException>());
		}

		[Test]
		public void Givens_ReturnedList_CannotBeModifiedByCaller()
		{
			ReadOnlyAssert.CannotBeModified(Create4x4(Givens4X4, Solution4X4).Givens);
		}

		[Test]
		public void Solution_ReturnedList_CannotBeModifiedByCaller()
		{
			ReadOnlyAssert.CannotBeModified(Create4x4(Givens4X4, Solution4X4).Solution);
		}

		private static Puzzle Create4x4(string givens, string solution)
		{
			return new Puzzle("4x4-test", new GridShape(2, 2), givens, solution);
		}

		// A known valid layout for square boxes: shifting each row keeps rows, columns and boxes free of repeats.
		private static string BuildPatternSolution(int boxSize)
		{
			var size = boxSize * boxSize;
			var symbols = new List<char>();
			for (var row = 0; row < size; row++)
			{
				for (var column = 0; column < size; column++)
				{
					var value = (boxSize * (row % boxSize) + row / boxSize + column) % size + 1;
					symbols.Add(value <= 9 ? (char)('0' + value) : (char)('A' + value - 10));
				}
			}

			return new string(symbols.ToArray());
		}
	}
}
