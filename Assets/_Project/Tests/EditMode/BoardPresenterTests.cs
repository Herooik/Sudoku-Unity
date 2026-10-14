using NUnit.Framework;
using Sudoku.Core;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	public class BoardPresenterTests
	{
		private const string Givens4x4 = "1..4.4..2..3...1";
		private const string Solution4x4 = "1234341221434321";

		[Test]
		public void Show_Puzzle_BuildsViewWithPuzzleShape()
		{
			var view = new FakeBoardView();
			var puzzle = CreatePuzzle();

			new BoardPresenter(view).Show(puzzle);

			Assert.That(view.BuiltShape, Is.SameAs(puzzle.Shape));
		}

		[Test]
		public void Show_Puzzle_SetsTextOfEveryCellFromGivens()
		{
			var view = new FakeBoardView();

			new BoardPresenter(view).Show(CreatePuzzle());

			Assert.That(view.CellTexts.Count, Is.EqualTo(16));
			Assert.That(view.CellTexts[0], Is.EqualTo("1"));
			Assert.That(view.CellTexts[3], Is.EqualTo("4"));
			Assert.That(view.CellTexts[1], Is.EqualTo(""), "empty cells show nothing");
		}

		[Test]
		public void Show_Puzzle_MakesViewVisibleOnlyAfterFillingIt()
		{
			var view = new FakeBoardView();

			new BoardPresenter(view).Show(CreatePuzzle());

			Assert.That(view.IsVisible, Is.True);
			Assert.That(view.Calls[0], Is.EqualTo(nameof(IBoardView.Build)));
			Assert.That(view.Calls[view.Calls.Count - 1], Is.EqualTo("SetVisible(True)"));
		}

		private static Puzzle CreatePuzzle() =>
			new Puzzle("4x4-test", new GridShape(2, 2), Givens4x4, Solution4x4);
	}
}
