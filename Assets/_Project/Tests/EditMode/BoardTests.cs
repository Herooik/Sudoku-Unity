using System;
using System.Collections.Generic;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests.EditMode
{
	public class BoardTests
	{
		// 4×4 with 2×2 boxes:  1 . | . 4        solution  1 2 | 3 4
		//                      . 4 | . .                  3 4 | 1 2
		//                      ----+----                  ----+----
		//                      2 . | . 3                  2 1 | 4 3
		//                      . . | . 1                  4 3 | 2 1
		private const string Givens4X4 = "1..4.4..2..3...1";
		private const string Solution4X4 = "1234341221434321";

		private const string Givens9X9 =
			"53..7....6..195....98....6.8...6...34..8.3..17...2...6.6....28....419..5....8..79";

		private const string Solution9X9 =
			"534678912672195348198342567859761423426853791713924856961537284287419635345286179";

		private const int GivenCell = 0;
		private const int EmptyCell = 1;
		private const int EmptyCellSolution = 2;
		private const int EmptyCellWrongValue = 3;

		[Test]
		public void Constructor_NullPuzzle_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new Board(null));
		}

		[Test]
		public void GetValue_NewBoard_MatchesGivens()
		{
			var board = Create4x4();

			var values = new int[board.Shape.CellCount];
			for (var i = 0; i < values.Length; i++)
			{
				values[i] = board.GetValue(i);
			}

			Assert.That(values, Is.EqualTo(new[] { 1, 0, 0, 4, 0, 4, 0, 0, 2, 0, 0, 3, 0, 0, 0, 1 }));
		}

		[Test]
		public void IsGiven_GivenAndEmptyCells_ReturnsTrueOnlyForGivens()
		{
			var board = Create4x4();

			var givens = new List<int>();
			for (var i = 0; i < board.Shape.CellCount; i++)
			{
				if (board.IsGiven(i))
				{
					givens.Add(i);
				}
			}

			Assert.That(givens, Is.EqualTo(new[] { 0, 3, 5, 8, 11, 15 }));
		}

		[Test]
		public void SetValue_EmptyCell_StoresValue()
		{
			var board = Create4x4();

			board.SetValue(EmptyCell, EmptyCellSolution);

			Assert.That(board.GetValue(EmptyCell), Is.EqualTo(EmptyCellSolution));
		}

		[Test]
		public void SetValue_PlayerCell_OverwritesValue()
		{
			var board = Create4x4();
			board.SetValue(EmptyCell, EmptyCellWrongValue);

			board.SetValue(EmptyCell, EmptyCellSolution);

			Assert.That(board.GetValue(EmptyCell), Is.EqualTo(EmptyCellSolution));
		}

		[Test]
		public void SetValue_Given_Throws()
		{
			var board = Create4x4();

			Assert.Throws<InvalidOperationException>(() => board.SetValue(GivenCell, 2));
			Assert.That(board.GetValue(GivenCell), Is.EqualTo(1));
		}

		[TestCase(0)]
		[TestCase(5)]
		public void SetValue_ValueOutOfRange_Throws(int value)
		{
			var board = Create4x4();

			Assert.Throws<ArgumentOutOfRangeException>(() => board.SetValue(EmptyCell, value));
		}

		[TestCase(-1)]
		[TestCase(16)]
		public void SetValue_IndexOutOfRange_Throws(int cellIndex)
		{
			var board = Create4x4();

			Assert.Throws<ArgumentOutOfRangeException>(() => board.SetValue(cellIndex, 1));
		}

		[Test]
		public void SetValue_NewValue_RaisesCellChangedWithIndex()
		{
			var board = Create4x4();
			var changed = RecordChanges(board);

			board.SetValue(EmptyCell, EmptyCellSolution);

			Assert.That(changed, Is.EqualTo(new[] { EmptyCell }));
		}

		[Test]
		public void SetValue_SameValue_DoesNotRaiseCellChanged()
		{
			var board = Create4x4();
			board.SetValue(EmptyCell, EmptyCellSolution);
			var changed = RecordChanges(board);

			board.SetValue(EmptyCell, EmptyCellSolution);

			Assert.That(changed, Is.Empty);
		}

		[Test]
		public void Clear_PlayerCell_EmptiesCell()
		{
			var board = Create4x4();
			board.SetValue(EmptyCell, EmptyCellSolution);

			board.Clear(EmptyCell);

			Assert.That(board.GetValue(EmptyCell), Is.EqualTo(0));
		}

		[Test]
		public void Clear_EmptyCell_DoesNotRaiseCellChanged()
		{
			var board = Create4x4();
			var changed = RecordChanges(board);

			board.Clear(EmptyCell);

			Assert.That(changed, Is.Empty);
		}

		[Test]
		public void Clear_Given_Throws()
		{
			var board = Create4x4();

			Assert.Throws<InvalidOperationException>(() => board.Clear(GivenCell));
			Assert.That(board.GetValue(GivenCell), Is.EqualTo(1));
		}

		[Test]
		public void HasMistake_WrongDigit_ReturnsTrue()
		{
			var board = Create4x4();

			board.SetValue(EmptyCell, EmptyCellWrongValue);

			Assert.That(board.HasMistake(EmptyCell), Is.True);
		}

		[Test]
		public void HasMistake_CorrectDigit_ReturnsFalse()
		{
			var board = Create4x4();

			board.SetValue(EmptyCell, EmptyCellSolution);

			Assert.That(board.HasMistake(EmptyCell), Is.False);
		}

		[Test]
		public void HasMistake_EmptyCellOrGiven_ReturnsFalse()
		{
			var board = Create4x4();

			Assert.That(board.HasMistake(EmptyCell), Is.False);
			Assert.That(board.HasMistake(GivenCell), Is.False);
		}

		[Test]
		public void IsSolved_NewBoard_ReturnsFalse()
		{
			var board = Create4x4();

			Assert.That(board.IsSolved, Is.False);
		}

		[Test]
		public void IsSolved_AllCellsCorrect_ReturnsTrue()
		{
			var board = Create4x4();

			FillFromSolution(board);

			Assert.That(board.IsSolved, Is.True);
		}

		[Test]
		public void IsSolved_FullButOneWrong_ReturnsFalse()
		{
			var board = Create4x4();
			FillFromSolution(board);

			board.SetValue(EmptyCell, EmptyCellWrongValue);

			Assert.That(board.IsSolved, Is.False);
		}

		[Test]
		public void IsSolved_9x9AllCellsFilledFromSolution_ReturnsTrue()
		{
			var board = new Board(new Puzzle("9x9-test", new GridShape(3, 3), Givens9X9, Solution9X9));

			FillFromSolution(board);

			Assert.That(board.IsSolved, Is.True);
		}

		private static Board Create4x4()
		{
			return new Board(new Puzzle("4x4-test", new GridShape(2, 2), Givens4X4, Solution4X4));
		}

		private static void FillFromSolution(Board board)
		{
			for (var i = 0; i < board.Shape.CellCount; i++)
			{
				if (!board.IsGiven(i))
				{
					board.SetValue(i, board.Puzzle.Solution[i]);
				}
			}
		}

		private static List<int> RecordChanges(Board board)
		{
			var changed = new List<int>();
			board.CellChanged += changed.Add;
			return changed;
		}
	}
}
