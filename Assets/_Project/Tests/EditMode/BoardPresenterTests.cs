using NUnit.Framework;
using Sudoku.Core;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	public class BoardPresenterTests
	{
		// 4×4 with 2×2 boxes:  1 . | . 4        solution  1 2 | 3 4
		//  (cell indices        . 4 | . .                  3 4 | 1 2
		//   row by row,         ----+----                  ----+----
		//   0..15)              2 . | . 3                  2 1 | 4 3
		//                       . . | . 1                  4 3 | 2 1
		private const string Givens4x4 = "1..4.4..2..3...1";
		private const string Solution4x4 = "1234341221434321";

		private const int GivenOne = 0;
		private const int EmptyCell = 1;
		private const int EmptyCellSolution = 2;
		private const int EmptyCellWrongValue = 3;
		private const int GivenFourInTopRow = 3;
		private const int GivenFourInSecondRow = 5;

		private FakeBoardView _view;
		private FakeNumberPadView _numberPad;
		private BoardPresenter _presenter;

		[SetUp]
		public void SetUp()
		{
			_view = new FakeBoardView();
			_numberPad = new FakeNumberPadView();
			_presenter = new BoardPresenter(_view, _numberPad);
		}

		[Test]
		public void Show_Puzzle_BuildsViewWithPuzzleShape()
		{
			var puzzle = CreatePuzzle();

			_presenter.Show(puzzle);

			Assert.That(_view.BuiltShape, Is.SameAs(puzzle.Shape));
		}

		[Test]
		public void Show_Puzzle_BuildsNumberPadWithGridSize()
		{
			_presenter.Show(CreatePuzzle());

			Assert.That(_numberPad.BuiltSize, Is.EqualTo(4));
		}

		[Test]
		public void Show_Puzzle_SetsTextOfEveryCellFromGivens()
		{
			_presenter.Show(CreatePuzzle());

			Assert.That(_view.CellTexts.Count, Is.EqualTo(16));
			Assert.That(_view.CellTexts[0], Is.EqualTo("1"));
			Assert.That(_view.CellTexts[3], Is.EqualTo("4"));
			Assert.That(_view.CellTexts[1], Is.EqualTo(""), "empty cells show nothing");
		}

		[Test]
		public void Show_Puzzle_StylesGivensAsGivenAndEmptyCellsAsPlayer()
		{
			_presenter.Show(CreatePuzzle());

			Assert.That(_view.CellStyles[GivenOne], Is.EqualTo(CellTextStyle.Given));
			Assert.That(_view.CellStyles[EmptyCell], Is.EqualTo(CellTextStyle.Player));
		}

		[Test]
		public void Show_Puzzle_HighlightsNothingAndHidesSolved()
		{
			_presenter.Show(CreatePuzzle());

			Assert.That(_view.CellsWith(CellHighlight.None).Count, Is.EqualTo(16));
			Assert.That(_view.Calls, Has.Member("SetSolvedVisible(False)"));
		}

		[Test]
		public void Show_Puzzle_MakesViewVisibleOnlyAfterFillingIt()
		{
			_presenter.Show(CreatePuzzle());

			Assert.That(_view.IsVisible, Is.True);
			Assert.That(_view.Calls[0], Is.EqualTo(nameof(IBoardView.Build)));
			Assert.That(_view.Calls[_view.Calls.Count - 1], Is.EqualTo("SetVisible(True)"));
		}

		[Test]
		public void CellClicked_EmptyCell_HighlightsSelectedAndItsPeers()
		{
			_presenter.Show(CreatePuzzle());

			_view.ClickCell(EmptyCell);

			// Cell (0,1): row 0, column 1 and the top-left box.
			Assert.That(_view.CellsWith(CellHighlight.Selected), Is.EqualTo(new[] { EmptyCell }));
			Assert.That(_view.CellsWith(CellHighlight.Peer), Is.EqualTo(new[] { 0, 2, 3, 4, 5, 9, 13 }));
			Assert.That(_view.CellsWith(CellHighlight.SameDigit), Is.Empty);
		}

		[Test]
		public void CellClicked_CellWithDigit_HighlightsSameDigitOutsidePeers()
		{
			_presenter.Show(CreatePuzzle());

			_view.ClickCell(GivenFourInTopRow);

			Assert.That(_view.CellsWith(CellHighlight.Selected), Is.EqualTo(new[] { GivenFourInTopRow }));
			Assert.That(_view.CellsWith(CellHighlight.SameDigit), Is.EqualTo(new[] { GivenFourInSecondRow }));
		}

		[Test]
		public void DigitClicked_CorrectDigit_ShowsItAsPlayer()
		{
			_presenter.Show(CreatePuzzle());
			_view.ClickCell(EmptyCell);

			_numberPad.ClickDigit(EmptyCellSolution);

			Assert.That(_view.CellTexts[EmptyCell], Is.EqualTo("2"));
			Assert.That(_view.CellStyles[EmptyCell], Is.EqualTo(CellTextStyle.Player));
		}

		[Test]
		public void DigitClicked_WrongDigit_ShowsItAsMistake()
		{
			_presenter.Show(CreatePuzzle());
			_view.ClickCell(EmptyCell);

			_numberPad.ClickDigit(EmptyCellWrongValue);

			Assert.That(_view.CellTexts[EmptyCell], Is.EqualTo("3"));
			Assert.That(_view.CellStyles[EmptyCell], Is.EqualTo(CellTextStyle.Mistake));
		}

		[Test]
		public void DigitClicked_NoSelection_ChangesNothing()
		{
			_presenter.Show(CreatePuzzle());
			_view.Calls.Clear();

			_numberPad.ClickDigit(EmptyCellSolution);

			Assert.That(_view.Calls, Is.Empty);
		}

		[Test]
		public void DigitClicked_GivenSelected_ChangesNothing()
		{
			_presenter.Show(CreatePuzzle());
			_view.ClickCell(GivenOne);

			Assert.DoesNotThrow(() => _numberPad.ClickDigit(2));
			Assert.That(_view.CellTexts[GivenOne], Is.EqualTo("1"));
		}

		[Test]
		public void DigitClicked_SelectedCell_HighlightsCellsWithTheNewDigit()
		{
			_presenter.Show(CreatePuzzle());
			_view.ClickCell(EmptyCell);

			_numberPad.ClickDigit(4);

			// Cell 3 is also a peer of cell 1: the same digit wins, because that is the conflict the player needs to see.
			Assert.That(_view.CellsWith(CellHighlight.SameDigit), Is.EqualTo(new[] { GivenFourInTopRow, GivenFourInSecondRow }));
		}

		[Test]
		public void EraseClicked_PlayerCell_ClearsIt()
		{
			_presenter.Show(CreatePuzzle());
			_view.ClickCell(EmptyCell);
			_numberPad.ClickDigit(EmptyCellWrongValue);

			_numberPad.ClickErase();

			Assert.That(_view.CellTexts[EmptyCell], Is.EqualTo(""));
			Assert.That(_view.CellStyles[EmptyCell], Is.EqualTo(CellTextStyle.Player));
		}

		[Test]
		public void DigitClicked_LastMissingDigit_ShowsSolvedAndIgnoresFurtherInput()
		{
			var puzzle = CreatePuzzle();
			_presenter.Show(puzzle);
			for (var i = 0; i < puzzle.Givens.Count; i++)
			{
				if (puzzle.Givens[i] == 0)
				{
					Assert.That(_view.IsSolvedVisible, Is.False, $"solved too early, before cell {i}");
					_view.ClickCell(i);
					_numberPad.ClickDigit(puzzle.Solution[i]);
				}
			}

			Assert.That(_view.IsSolvedVisible, Is.True);

			_view.ClickCell(EmptyCell);
			_numberPad.ClickDigit(EmptyCellWrongValue);
			_numberPad.ClickErase();
			Assert.That(_view.CellTexts[EmptyCell], Is.EqualTo("2"));
		}

		[Test]
		public void Show_SecondPuzzle_ClearsSelection()
		{
			_presenter.Show(CreatePuzzle());
			_view.ClickCell(EmptyCell);

			_presenter.Show(CreatePuzzle());
			_numberPad.ClickDigit(EmptyCellSolution);

			Assert.That(_view.CellsWith(CellHighlight.None).Count, Is.EqualTo(16));
			Assert.That(_view.CellTexts[EmptyCell], Is.EqualTo(""));
		}

		[Test]
		public void Dispose_Always_UnsubscribesFromViews()
		{
			_presenter.Dispose();

			Assert.That(_view.HasSubscribers, Is.False);
			Assert.That(_numberPad.HasSubscribers, Is.False);
		}

		private static Puzzle CreatePuzzle() =>
			new Puzzle("4x4-test", new GridShape(2, 2), Givens4x4, Solution4x4);
	}
}
