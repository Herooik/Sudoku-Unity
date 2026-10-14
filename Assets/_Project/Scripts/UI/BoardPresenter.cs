using Sudoku.Core;

namespace Sudoku.UI
{
	public sealed class BoardPresenter
	{
		private readonly IBoardView _view;

		public BoardPresenter(IBoardView view)
		{
			_view = view;
		}

		public void Show(Puzzle puzzle)
		{
			_view.Build(puzzle.Shape);

			for (var i = 0; i < puzzle.Givens.Count; i++)
			{
				_view.SetCellText(i, CellSymbol.ToSymbol(puzzle.Givens[i]));
			}

			_view.SetVisible(true);
		}
	}
}
