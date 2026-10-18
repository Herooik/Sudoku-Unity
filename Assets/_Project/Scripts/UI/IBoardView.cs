using System;
using Sudoku.Core;

namespace Sudoku.UI
{
	// Passive: the presenter decides what each cell shows and means; the view only lays out and picks colours.
	public interface IBoardView
	{
		event Action<int> CellClicked;

		void Build(GridShape shape);

		void SetCellContent(int cellIndex, string text, CellTextStyle style);

		void SetCellHighlight(int cellIndex, CellHighlight highlight);

		void SetSolvedVisible(bool visible);

		void SetVisible(bool visible);
	}
}
